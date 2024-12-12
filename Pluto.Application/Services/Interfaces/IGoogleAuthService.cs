using System.Security.Claims;
using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.Interfaces;

public interface IGoogleAuthService
{
    string GetGoogleOAuthUrl();
    Task<string?> HandleGoogleCallbackAsync(string code);
}