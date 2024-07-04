﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CatalogApi.Services
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateAccessToken(IEnumerable<Claim> claims, IConfiguration config);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration config);
    }
}
