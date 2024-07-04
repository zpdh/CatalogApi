using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CatalogApi.Services
{
    public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration config)
        {
            var key = config.GetSection("JWT").GetValue<string>("SecretKey") ?? throw new InvalidOperationException                                                                                         ("Invalid secret key.");
            var privateKey = Encoding.UTF8.GetBytes(key);

            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(privateKey),
                                                                SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")),
                Audience = config.GetSection("JWT").GetValue<string>("ValidAudience"),
                Issuer = config.GetSection("JWT").GetValue<string>("ValidIssuer"),
                SigningCredentials = signingCredentials
            };

            var token = new JwtSecurityTokenHandler().CreateJwtSecurityToken(tokenDescriptor);
            return token;
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[128];

            using var RNG = RandomNumberGenerator.Create();

            RNG.GetBytes(randomBytes);

            var refreshToken = Convert.ToBase64String(randomBytes);
            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config)
        {
            var key = config["JWT:SecretKey"] ?? throw new InvalidOperationException("Invalid secret key.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals
                                                                          (SecurityAlgorithms.HmacSha256,
                                                                          StringComparison.InvariantCultureIgnoreCase)) 
            throw new SecurityTokenException("Invalid token.");

            return principal;
        }
    }
}
