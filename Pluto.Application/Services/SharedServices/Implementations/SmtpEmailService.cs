using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Enums;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly string _mail;
    private readonly string _password;
    private readonly string _baseUrl;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(
        IConfiguration configuration
    )
    {
        _configuration = configuration;

        _mail = _configuration["Email"] ?? throw new ArgumentNullException(nameof(_mail));
        _password = _configuration["EmailPassword"] ?? throw new ArgumentNullException(nameof(_password));
        _baseUrl = _configuration["BaseUrl"] ?? throw new ArgumentNullException(nameof(_baseUrl));
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
                _ => throw new ArgumentOutOfRangeException(nameof(template), template, null)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_mail),
                Subject = subject,
                Body = emailBody,
                IsBodyHtml = true
            };
            message.To.Add(email);

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_mail, _password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
        catch (SmtpException smtpEx)
        {
            throw new Exception("Failed to send email due to an SMTP error.", smtpEx);
        }
        catch (IOException ioEx)
        {
            throw new Exception("Failed to read the email template file.", ioEx);
        }
    }

    private async Task<string> GetEmailConfirmationBody(string email, string link)
    {
        var emailTemplatePath = _configuration["EmailTemplatePath"];

        var emailBody = await File.ReadAllTextAsync(emailTemplatePath);

        return emailBody
            .Replace("{{ACTIVATION_URL}}", link)
            .Replace("{{USER_EMAIL}}", email);
    }

    private async Task<string> GetResetPasswordBody(string link)
    {
        var resetTemplatePath = _configuration["ResetPasswordTemplatePath"];

        var emailBody = await File.ReadAllTextAsync(resetTemplatePath);

        return emailBody
            .Replace("{{RESET_URL}}", link);
    }
}