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
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenGeneratorService(
        IConfiguration configuration,
        IUserRepository userRepository
    )
    {
        _configuration = configuration;
        _userRepository = userRepository;

        _secretKey = _configuration["SecretKey"] ??
                     throw new InvalidConfigurationException("SecretKey is not configured");
        _issuer = _configuration["Issuer"] ??
                  throw new InvalidConfigurationException("Issuer is not configured");
        _audience = _configuration["Audience"] ??
                    throw new InvalidConfigurationException("Audience is not configured");
    }

    public async Task<TokenDto> GenerateToken(User user, bool populateExp)
    {
        var signingCredentials = GetSigningCredentials();
        var claims = GetClaims(user);
        var jwtToken = GenerateTokenOptions(signingCredentials, claims);

        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;

        if (populateExp)
            user.RefreshTokenExpiration = ((DateTimeOffset)DateTime.UtcNow.AddDays(7))
                .ToUnixTimeSeconds();

        await _userRepository.UpdateAsync(user);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        return new TokenDto(accessToken, refreshToken);
    }

    public async Task<TokenDto> RefreshTokenAsync(TokenDto token)
    {
        var principal = GetPrincipalFromExpiredToken(token.AccessToken);
        var user = await _userRepository.GetAsync(Int32.Parse(principal.Claims.First(x => x.Type == "id").Value));

        if (user == null || user.RefreshToken != token.RefreshToken ||
            user.RefreshTokenExpiration < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            throw new BadRequestException("Refresh token is invalid.");

        return await GenerateToken(user, false);
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
        return new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.Now.AddHours(1),
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