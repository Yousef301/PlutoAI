using System.Security.Claims;
using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.EntityServices.Interfaces.Auth;

public interface IGoogleOAuthService
{
    string GetOAuthUrl();
    Task<TokenDto> HandleCallbackAsync(string code);
}