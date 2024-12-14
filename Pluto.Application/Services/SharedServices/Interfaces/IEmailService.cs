using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, EmailConfirmationBody body);
}