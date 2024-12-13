using System.Text;
using AutoMapper;
using Pluto.Application.DTOs.Messages;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations;

public class MessageService : IMessageService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IModelService _ollamaService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageService(
        ISessionRepository sessionRepository,
        IMessageRepository messageRepository,
        IMapper mapper,
        IModelService ollamaService,
        IUnitOfWork unitOfWork
    )
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
        _ollamaService = ollamaService;
        _unitOfWork = unitOfWork;
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

    public async Task<CreateMessageResponse> SendMessageAsync(CreateMessageRequest request)
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

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var recentMessages = await _messageRepository
                .GetSessionMessagesAsync(request.SessionId, 10, true);

            var contextPrompt = BuildContextualPrompt(recentMessages, request.Query);

            var response = await _ollamaService.GenerateResponseAsync(contextPrompt);

            var message = _mapper.Map<Message>(request);

            message.Response = response;

            await _messageRepository.CreateAsync(message);

            session.UpdatedAt = DateTime.Now;

            await _sessionRepository.Update(session);

            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<CreateMessageResponse>(message);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private string BuildContextualPrompt(IEnumerable<Message> recentMessages, string newQuery)
    {
        var contextBuilder = new StringBuilder();

        foreach (var msg in recentMessages.OrderBy(m => m.Id))
        {
            contextBuilder.AppendLine($"User: {msg.Query}");

            if (!string.IsNullOrWhiteSpace(msg.Response))
            {
                contextBuilder.AppendLine($"Assistant: {msg.Response}");
            }
        }

        contextBuilder.AppendLine($"User: {newQuery}");

        return contextBuilder.ToString();
    }
}