using FluentValidation;
using Pluto.Application.DTOs.Messages;

namespace Pluto.Application.Validators.Messages;

public class GetMessagesRequestValidator : AbstractValidator<GetMessagesRequest>
{
    public GetMessagesRequestValidator()
    {
        Include(new BaseMessageRequestValidator());
    }
}