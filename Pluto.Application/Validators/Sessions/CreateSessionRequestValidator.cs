using FluentValidation;
using Pluto.Application.DTOs.Sessions;

namespace Pluto.Application.Validators.Sessions;

public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User Id is required")
            .GreaterThan(0)
            .WithMessage("User Id must be greater than 0");
    }
}