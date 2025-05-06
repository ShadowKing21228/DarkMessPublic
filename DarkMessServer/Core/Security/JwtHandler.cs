using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DarkMessServer.API.Models;
using DarkMessServer.Core.Config;
using DarkMessServer.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;

namespace DarkMessServer.Core.Security;

public static class JwtHandler
{
    public static readonly TokenValidationParameters JwtParameters = new()
    {
       ValidateIssuer = JwtConfig.Issuer,
       ValidateAudience = JwtConfig.Audience,
       ValidateLifetime = JwtConfig.Lifetime,
       ClockSkew = TimeSpan.Zero,
       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfig.SecretKey))
    };
    
    public static string GenerateJwtToken(UserLoginModel user)
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, UserRepository.GetId(user).Result)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtConfig.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
        var token = new JwtSecurityToken(
            issuer: "DarkMessServer",
            audience: "user",
            claims: claims,
            expires: DateTime.Now.AddHours(24), 
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}