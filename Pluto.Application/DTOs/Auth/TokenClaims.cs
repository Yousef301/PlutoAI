namespace Pluto.Application.DTOs.Auth;

public record TokenClaims(string Id, string Email, string FullName, string ExpireDate);