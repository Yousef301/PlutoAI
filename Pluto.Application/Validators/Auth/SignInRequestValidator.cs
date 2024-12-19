using FluentValidation;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Extensions;

namespace Pluto.Application.Validators.Auth;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid");

        RuleFor(r => r.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .ValidPassword();
    }
}