using FluentValidation;
using Pluto.Application.DTOs.Messages;

namespace Pluto.Application.Validators.Messages;

public class BaseMessageRequestValidator : AbstractValidator<BaseMessageRequest>
{
    public BaseMessageRequestValidator()
    {
        RuleFor(r => r.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required")
            .GreaterThan(0)
            .WithMessage("SessionId must be greater than 0");

        RuleFor(r => r.UserId)
            .NotEmpty()
            .WithMessage("UserId is required")
            .GreaterThan(0)
            .WithMessage("SessionId must be greater than 0");
    }
}