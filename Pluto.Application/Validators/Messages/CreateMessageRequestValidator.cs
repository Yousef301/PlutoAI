using FluentValidation;
using Pluto.Application.DTOs.Messages;

namespace Pluto.Application.Validators.Messages;

public class CreateMessageRequestValidator : AbstractValidator<CreateMessageRequest>
{
    public CreateMessageRequestValidator()
    {
        Include(new BaseMessageRequestValidator());

        RuleFor(r => r.Query)
            .NotEmpty()
            .WithMessage("Query is required");

        RuleFor(r => r.Model)
            .NotEmpty()
            .WithMessage("Model is required");
    }
}