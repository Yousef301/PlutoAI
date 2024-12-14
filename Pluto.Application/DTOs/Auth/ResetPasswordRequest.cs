namespace Pluto.Application.DTOs.Auth;

public record ResetPasswordRequest(string Password, string Token);