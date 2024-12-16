namespace Pluto.Application.DTOs.Auth;

public record SignInResponse(
    string AccessToken,
    string RefreshToken,
    bool EmailConfirmed
);