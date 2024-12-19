using FluentValidation;
using Pluto.Application.DTOs.Sessions;

namespace Pluto.Application.Validators.Sessions;

public class DeleteSessionRequestValidator : AbstractValidator<DeleteSessionRequest>
{
    public DeleteSessionRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required")
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User Id is required")
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}