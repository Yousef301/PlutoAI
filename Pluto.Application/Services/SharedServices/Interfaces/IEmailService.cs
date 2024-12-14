using Pluto.Application.DTOs.Auth;
using Pluto.DAL.Enums;

namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, EmailConfirmationBody body, Template template);
}