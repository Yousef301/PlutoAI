namespace Pluto.Application.DTOs.Auth;

public record SignInRequest(
    string Email,
    string Password
);