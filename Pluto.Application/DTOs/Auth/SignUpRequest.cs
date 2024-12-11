namespace Pluto.Application.DTOs.Auth;

public record SignUpRequest(
    string FullName,
    string Email,
    string Password
);