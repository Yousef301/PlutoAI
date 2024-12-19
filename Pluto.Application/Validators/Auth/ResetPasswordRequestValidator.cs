using FluentValidation;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Extensions;

namespace Pluto.Application.Validators.Auth;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .ValidPassword();

        RuleFor(r => r.Token)
            .NotEmpty()
            .WithMessage("Token is required");
    }
}