using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.EntityServices.Interfaces.Auth;

public interface IUserService
{
    public Task<SignInResponse> SignInAsync(SignInRequest request);
    public Task<SignUpResponse> SignUpAsync(SignUpRequest request);
    public Task SendConfirmationEmail(EmailConfirmationRequest request);
    public Task ConfirmEmail(string token);
}