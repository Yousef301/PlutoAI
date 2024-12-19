using FluentValidation;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Extensions;

namespace Pluto.Application.Validators.Auth;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(r => r.FullName)
            .ValidString(6, 50, "Full Name");

        RuleFor(r => r.Email)
            .EmailAddress()
            .WithMessage("Email is not valid");

        RuleFor(r => r.Password)
            .ValidPassword();
    }
}