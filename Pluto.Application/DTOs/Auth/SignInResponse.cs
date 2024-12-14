namespace Pluto.Application.DTOs.Auth;

public record SignInResponse(
    string Token,
    bool EmailConfirmed
);