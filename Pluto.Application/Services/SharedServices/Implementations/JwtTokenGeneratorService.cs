using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class JwtTokenGeneratorService : ITokenGeneratorService
{
    private readonly IConfiguration _configuration;

    public JwtTokenGeneratorService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["SecretKey"]));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>()
        {
            new("id", user.Id.ToString()),
            new("fullname", user.FullName),
            new("email", user.Email)
        };


        var jwtToken = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            audience: _configuration["Audience"],
            claims: tokenClaims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: signingCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return token;
    }
}