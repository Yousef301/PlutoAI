using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.EntityServices.Interfaces.Auth;

public interface IPasswordService
{
    public Task SendPasswordResetEmail(SendPasswordResetRequest request);
    public Task ResetPassword(ResetPasswordRequest request);
}