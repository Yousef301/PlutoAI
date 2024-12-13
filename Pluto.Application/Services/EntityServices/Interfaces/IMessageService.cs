using Pluto.Application.DTOs.Messages;

namespace Pluto.Application.Services.EntityServices.Interfaces;

public interface IMessageService
{
    Task<IEnumerable<GetMessagesResponse>> GetSessionMessagesAsync(GetMessagesRequest request);
    Task<CreateMessageResponse> SendMessageAsync(CreateMessageRequest request);
}