using Pluto.Application.DTOs.Auth;
using Pluto.DAL.Entities;

namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface ITokenGeneratorService
{
    public Task<TokenDto> GenerateToken(User user, bool populateExp);
    public Task<TokenDto> RefreshTokenAsync(TokenDto token);
    public Task<TokenClaims> GetTokenClaims(string token);
}