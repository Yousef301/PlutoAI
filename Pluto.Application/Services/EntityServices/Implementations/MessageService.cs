using AutoMapper;
using Pluto.Application.DTOs.Messages;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations;

public class MessageService : IMessageService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;

    public MessageService(
        ISessionRepository sessionRepository,
        IMessageRepository messageRepository,
        IMapper mapper
    )
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GetMessagesResponse>> GetSessionMessagesAsync(GetMessagesRequest request)
    {
        var session = await _sessionRepository.GetAsync(request.SessionId);

        if (session == null)
        {
            throw new Exception("Session not found");
        }

        if (session.UserId != request.UserId)
        {
            throw new Exception("Unauthorized");
        }

        var messages = await _messageRepository.GetSessionMessagesAsync(request.SessionId);

        return _mapper.Map<IEnumerable<GetMessagesResponse>>(messages);
    }

    public async Task<CreateMessageResponse> CreateMessageAsync(CreateMessageRequest request)
    {
        var session = await _sessionRepository.GetAsync(request.SessionId);

        if (session == null)
        {
            throw new Exception("Session not found");
        }

        if (session.UserId != request.UserId)
        {
            throw new Exception("Unauthorized");
        }

        var message = _mapper.Map<Message>(request);

        message.Response = "Dummy response";

        await _messageRepository.CreateAsync(message);

        return _mapper.Map<CreateMessageResponse>(message);
    }
}