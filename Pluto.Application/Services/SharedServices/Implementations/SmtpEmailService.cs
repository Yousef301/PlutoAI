using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using System.IO;

namespace Pluto.Application.Services.SharedServices.Implementations;

public class SmtpEmailService : IEmailService
{
    private readonly string _mail;
    private readonly string _password;
    private readonly string _baseUrl;
    private readonly string _emailTemplatePath;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(
        IConfiguration configuration
    )
    {
        _configuration = configuration;

        _mail = _configuration["Email"] ?? throw new ArgumentNullException(nameof(_mail));
        _password = _configuration["EmailPassword"] ?? throw new ArgumentNullException(nameof(_password));
        _baseUrl = _configuration["BaseUrl"] ?? throw new ArgumentNullException(nameof(_baseUrl));
        _emailTemplatePath = _configuration["EmailTemplatePath"] ??
                             throw new ArgumentNullException(nameof(_emailTemplatePath));
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        EmailConfirmationBody body
    )
    {
        try
        {
            var emailBody = await File.ReadAllTextAsync(_emailTemplatePath);

            emailBody = emailBody
                .Replace("{{ACTIVATION_URL}}", body.Link)
                .Replace("{{USER_EMAIL}}", email);

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
}