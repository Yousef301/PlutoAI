using FluentValidation;
using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Validators.Auth;

public class SendPasswordResetRequestValidator : AbstractValidator<SendPasswordResetRequest>
{
    public SendPasswordResetRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid");
    }
}