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

        _mail = _configuration["Email:Email"] ??
                throw new InvalidConfigurationException("Email configuration is missing.");
        _password = _configuration["Email:Password"] ??
                    throw new InvalidConfigurationException("Email password configuration is missing.");
    }

    public async Task SendEmailAsync(
        string email,
        string subject,
        EmailConfirmationBody body,
        Template template
    )
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

    private async Task<string> GetEmailConfirmationBody(string email, string link)
    {
        var emailTemplatePath = GetTemplatePath(Template.EmailConfirmation);

        if (!File.Exists(emailTemplatePath))
            throw new FileNotFoundException("Email template file not found.", emailTemplatePath);

        var emailBody = await File.ReadAllTextAsync(emailTemplatePath);

        return emailBody
            .Replace("{{ACTIVATION_URL}}", link)
            .Replace("{{USER_EMAIL}}", email);
    }

    private async Task<string> GetResetPasswordBody(string link)
    {
        var resetTemplatePath = GetTemplatePath(Template.PasswordReset);

        if (!File.Exists(resetTemplatePath))
            throw new FileNotFoundException("Password reset template file not found.", resetTemplatePath);

        var emailBody = await File.ReadAllTextAsync(resetTemplatePath);

        return emailBody
            .Replace("{{RESET_URL}}", link);
    }

    private string GetTemplatePath(Template template,
        [System.Runtime.CompilerServices.CallerFilePath]
        string sourceFilePath = "")
    {
        var classDirectory = Path.GetDirectoryName(sourceFilePath);

        var templatesDirectory = Path
            .GetFullPath(Path.Combine(classDirectory, @"..", @"..", @"..", "Templates"));

        switch (template)
        {
            case Template.EmailConfirmation:
                var emailTemplateName = _configuration["EmailConfirmationTemplateName"] ??
                                        throw new InvalidConfigurationException(
                                            "Email confirmation template name is missing.");
                templatesDirectory = Path.Combine(templatesDirectory, emailTemplateName);
                break;
            case Template.PasswordReset:
                var passwordTemplateName = _configuration["ResetPasswordTemplateName"] ??
                                           throw new InvalidConfigurationException(
                                               "Password reset template name is missing.");
                templatesDirectory = Path.Combine(templatesDirectory, passwordTemplateName);
                break;
        }

        return templatesDirectory;
    }
}