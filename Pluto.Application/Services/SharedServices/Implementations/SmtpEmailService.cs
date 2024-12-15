using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Enums;
using Pluto.DAL.Exceptions;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly string _mail;
    private readonly string _password;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(
        IConfiguration configuration
    )
    {
        _configuration = configuration;

        _mail = _configuration["Email"] ?? throw new InvalidConfigurationException("Email configuration is missing.");
        _password = _configuration["EmailPassword"] ??
                    throw new InvalidConfigurationException("Email password configuration is missing.");
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        EmailConfirmationBody body,
        Template template
    )
    {
        try
        {
            var emailBody = template switch
            {
                Template.EmailConfirmation => await GetEmailConfirmationBody(email, body.Link),
                Template.PasswordReset => await GetResetPasswordBody(body.Link),
                _ => throw new ArgumentOutOfRangeException(nameof(template), template, "Unsupported template type.")
            };

            var message = new MailMessage
            {
                From = new MailAddress(_mail),
                Subject = subject,
                Body = emailBody,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using var client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential(_mail, _password);
            client.EnableSsl = true;

            await client.SendMailAsync(message);
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendingException("Failed to send email to {email} due to an SMTP error.",
                smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendingException("An unexpected error occurred while sending the email.",
                ex);
        }
    }

    private async Task<string> GetEmailConfirmationBody(string email, string link)
    {
        var emailTemplatePath = _configuration["EmailTemplatePath"];

        if (string.IsNullOrEmpty(emailTemplatePath))
            throw new InvalidConfigurationException("Email template path configuration is missing.");

        try
        {
            var emailBody = await File.ReadAllTextAsync(emailTemplatePath);

            return emailBody
                .Replace("{{ACTIVATION_URL}}", link)
                .Replace("{{USER_EMAIL}}", email);
        }
        catch (IOException ioEx)
        {
            throw
                new EmailTemplateException("Error reading the email confirmation template file.",
                    ioEx);
        }
    }

    private async Task<string> GetResetPasswordBody(string link)
    {
        var resetTemplatePath = _configuration["ResetPasswordTemplatePath"];

        if (string.IsNullOrEmpty(resetTemplatePath))
            throw new InvalidConfigurationException("Reset password template path configuration is missing.");

        try
        {
            var emailBody = await File.ReadAllTextAsync(resetTemplatePath);

            return emailBody
                .Replace("{{RESET_URL}}", link);
        }
        catch (IOException ioEx)
        {
            throw new EmailTemplateException("Error reading the password reset template file.",
                ioEx);
        }
    }
}