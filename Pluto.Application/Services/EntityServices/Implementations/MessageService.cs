using System.Diagnostics;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Messages;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;
using Serilog;

namespace Pluto.Application.Services.EntityServices.Implementations;

public class MessageService : IMessageService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IServiceManager _serviceManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public MessageService(
        IMapper mapper,
        IServiceManager serviceManager,
        IUnitOfWork unitOfWork,
        IRepositoryManager repositoryManager,
        IConfiguration configuration
    )
    {
        _mapper = mapper;
        _serviceManager = serviceManager;
        _unitOfWork = unitOfWork;
        _repositoryManager = repositoryManager;
        _configuration = configuration;
    }

    public async Task<IEnumerable<GetMessagesResponse>> GetSessionMessagesAsync(GetMessagesRequest request)
    {
        var session = await _repositoryManager.SessionRepository.GetAsync(request.SessionId);

        if (session == null)
            throw new NotFoundException("Session", request.SessionId);


        if (session.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to send messages to this session.");

        var messages = await _repositoryManager.MessageRepository
            .GetSessionMessagesAsync(request.SessionId);

        return _mapper.Map<IEnumerable<GetMessagesResponse>>(messages);
    }

    public async Task<CreateMessageResponse> SendMessageAsync(CreateMessageRequest request)
    {
        var session = await _repositoryManager.SessionRepository.GetAsync(request.SessionId);

        if (session == null)
            throw new NotFoundException("Session", request.SessionId);

        if (session.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to send messages to this session.");

        var messagesHistoryLimit = _configuration["HistoryLimit"];
        var messages = 5;

        if (messagesHistoryLimit != null)
            messages = Int32.Parse(messagesHistoryLimit);

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var recentMessages = await _repositoryManager.MessageRepository
                .GetSessionMessagesAsync(request.SessionId, messages, true);

            var contextPrompt = BuildContextualPrompt(recentMessages, request.Query);

            var stopwatch = Stopwatch.StartNew();
            Log.Information("Starting to generate response...");

            var response = await _serviceManager.ModelService
                .GenerateResponseAsync(contextPrompt, request.Model);

            stopwatch.Stop();
            Log.Information("GenerateResponseAsync took {ElapsedMilliseconds} seconds", stopwatch.Elapsed.TotalSeconds);

            var message = _mapper.Map<Message>(request);

            message.Response = response;

            await _repositoryManager.MessageRepository.CreateAsync(message);

            session.UpdatedAt = DateTime.Now;

            await _repositoryManager.SessionRepository.Update(session);

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