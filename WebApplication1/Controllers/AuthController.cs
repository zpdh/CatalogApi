using CatalogApi.DataTransferObjects;
using CatalogApi.Models;
using CatalogApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CatalogApi.Controllers
{
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ITokenService tokenService, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IConfiguration config,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginDTO loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username!);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginDto.Password!))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName!),
                    new(ClaimTypes.Email, user.Email!),
                    new("id", user.UserName!),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _config);

                var refreshToken = _tokenService.GenerateRefreshToken();

                int.TryParse(_config["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

                user.RefreshToken = refreshToken;

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterDTO registerDto)
        {
            if (registerDto.Username != null)
            {
                var userExistsCheck = await _userManager.FindByNameAsync(registerDto.Username);
                if (userExistsCheck != null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ResponseDTO
                            { Message = "User already exists", Status = "Error" });
                }
            }

            ApplicationUser user = new()
            {
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerDto.Username
            };

            if (registerDto.Password != null)
            {
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return StatusCode(500, new ResponseDTO { Status = "Error", Message = "User creation failed" });
                }
            }

            return Ok(new ResponseDTO { Status = "Success", Message = "Registered Successfully" });
        }

        [HttpPost]
        [Route("refresh-token")]
        [Authorize]
        public async Task<ActionResult> RefreshToken(TokenDTO tokenDto)
        {
            if (tokenDto == null) return BadRequest("Invalid request");

            string accessToken = tokenDto.AccessToken ?? throw new ArgumentNullException(nameof(tokenDto));
            string refreshToken = tokenDto.RefreshToken ?? throw new ArgumentNullException(nameof(tokenDto));

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken, _config);

            if (principal == null) return BadRequest("Invalid access and/or refresh token");

            string? username = principal.Identity!.Name;

            var user = await _userManager.FindByNameAsync(username!);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid access and/or refresh token");

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _config);

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [Route("revoke/{username}")]
        public async Task<ActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null) return BadRequest("Invalid username");

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost]
        [Route("role/create")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult> CreateRole(string roleName)
        {
            var existenceCheck = await _roleManager.RoleExistsAsync(roleName);

            if (existenceCheck)
            {
                return BadRequest($"Role {roleName} already exists");
            }

            var role = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (role.Succeeded)
            {
                _logger.LogInformation($"Role {roleName} created successfully");
                return Created("role/create", $"Successfully created role {roleName}");
            }
            else
            {
                _logger.LogWarning($"Role {roleName} creation failed");
                return StatusCode(500,
                    new ResponseDTO { Status = "Error", Message = $"Role {roleName} creation failed" });
            }
        }

        [HttpPost]
        [Route("role/add/{username}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<ActionResult> AddUserToRole(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);
            var role = await _roleManager.RoleExistsAsync(roleName);

            if (user == null || !role) return BadRequest("Invalid request: User or role not found");

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                _logger.LogWarning($"Failed to add {roleName} to {username}");
                return BadRequest(result.Errors);
            }

            _logger.LogInformation($"Added {roleName} to {username} successfully");
            return Ok(new ResponseDTO
            {
                Status = "Success", Message = $"Successfully added role '{roleName}' to '{username}'"
            });
        }
    }
}