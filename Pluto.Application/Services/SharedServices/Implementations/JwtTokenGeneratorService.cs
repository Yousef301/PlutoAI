using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Exceptions;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class JwtTokenGeneratorService : ITokenGeneratorService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGeneratorService(
        IConfiguration configuration,
        IRepositoryManager repositoryManager
    )
    {
        _configuration = configuration;
        _repositoryManager = repositoryManager;

        _secretKey = _configuration["Jwt:SecretKey"] ??
                     throw new InvalidConfigurationException("SecretKey is not configured");
        _issuer = _configuration["Jwt:Issuer"] ??
                  throw new InvalidConfigurationException("Issuer is not configured");
        _audience = _configuration["Jwt:Audience"] ??
                    throw new InvalidConfigurationException("Audience is not configured");
    }

    public async Task<TokenDto> GenerateToken(User user, bool populateExp)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = GetClaims(user);
        var jwtToken = GenerateTokenOptions(signingCredentials, claims);

        var refreshToken = GenerateRefreshToken();

        if (populateExp)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiration = ((DateTimeOffset)DateTime.UtcNow.AddDays(7))
                .ToUnixTimeSeconds();
        }

        await _repositoryManager.UserRepository.UpdateAsync(user);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return new TokenDto(accessToken, refreshToken);
    }

    public async Task<TokenDto> RefreshTokenAsync(TokenDto token)
    {
        var principal = GetPrincipalFromExpiredToken(token.AccessToken);
        var user = await _repositoryManager.UserRepository
            .GetAsync(Int32.Parse(principal.Claims.First(x => x.Type == "id").Value));

        if (user == null || user.RefreshToken != token.RefreshToken ||
            user.RefreshTokenExpiration < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            throw new BadRequestException("Refresh token is invalid.");

        return await GenerateToken(user, false);
    }

    public Task<TokenClaims> GetTokenClaims(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);

        return Task.FromResult(new TokenClaims(
            principal.Claims.First(x => x.Type == "id").Value,
            principal.Claims.First(x => x.Type.Contains("email", StringComparison.OrdinalIgnoreCase)).Value,
            principal.Claims.First(x => x.Type == "fullname").Value,
            principal.Claims.First(x => x.Type == "exp").Value
        ));
    }


    private SigningCredentials GetSigningCredentials()
    {
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(_secretKey));
        return new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    private List<Claim> GetClaims(User user)
    {
        return new List<Claim>
        {
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("fullname", user.FullName)
        };
    }

    private JwtSecurityToken GenerateTokenOptions(
        SigningCredentials signingCredentials,
        List<Claim> claims
    )
    {
        var tokenExpiration = _configuration["Jwt:TokenExpirationMinutes"]!;

        return new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.Now.AddMinutes(Int32.Parse(tokenExpiration)),
            signingCredentials: signingCredentials
        );
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_secretKey)),
            ValidateLifetime = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}