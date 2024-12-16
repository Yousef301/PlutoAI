using Microsoft.AspNetCore.Http;
using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.EntityServices.Interfaces.Auth;

public interface IAuthenticationService
{
    public Task<SignInResponse> SignInAsync(SignInRequest request);
    public Task<SignUpResponse> SignUpAsync(SignUpRequest request);
    public Task SendConfirmationEmail(EmailConfirmationRequest request);
    public Task ConfirmEmail(string token);
    public Task SendPasswordResetEmail(SendPasswordResetRequest request);
    public Task ResetPassword(ResetPasswordRequest request);
    public void SetTokenInsideCookie(TokenDto token, HttpContext httpContext);
    public void RemoveCookies(HttpContext httpContext);
}