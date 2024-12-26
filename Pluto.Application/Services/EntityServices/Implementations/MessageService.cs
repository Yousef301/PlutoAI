using System.Diagnostics;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.MessageEmbeddings;
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
        var session = await ValidateSessionAsync(request.SessionId, request.UserId);
        var messagesLimit = GetMessagesHistoryLimit();
        var recentMessages = await GetRecentMessagesAsync(request.SessionId);
        var filteredMessages = await GetFilteredMessagesAsync(recentMessages, request, messagesLimit);
        var contextPrompt = BuildContextualPrompt(filteredMessages, request.Query);
        var response = await GenerateResponseAsync(contextPrompt, request.Model);
        var message = await SaveMessageAsync(request, response, session);

        return _mapper.Map<CreateMessageResponse>(message);
    }

    private async Task<Session> ValidateSessionAsync(int sessionId, int userId)
    {
        var session = await _repositoryManager.SessionRepository.GetAsync(sessionId);
        if (session == null)
            throw new NotFoundException("Session", sessionId);

        if (session.UserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to send messages to this session.");

        return session;
    }

    private int GetMessagesHistoryLimit()
    {
        var limit = _configuration["HistoryLimit"];
        return string.IsNullOrEmpty(limit) ? 3 : int.Parse(limit);
    }

    private async Task<List<Message>> GetRecentMessagesAsync(int sessionId)
    {
        return (await _repositoryManager.MessageRepository.GetSessionMessagesAsync(sessionId)).ToList();
    }

    private async Task<List<Message>> GetFilteredMessagesAsync(IEnumerable<Message> recentMessages,
        CreateMessageRequest request, int limit)
    {
        var recentMessagesArr = recentMessages as Message[] ?? recentMessages.ToArray();

        var messageBodies = recentMessagesArr.Select(m => new MessageBody(m.Id, m.Query)).ToList();
        var embeddingsRequest = new MessageEmbeddingsRequest(messageBodies,
            request.Query, limit);
        var similarMessages = await _serviceManager.MessageEmbeddingService.GetSimilarMessages(embeddingsRequest);
        var similarMessageIds = similarMessages.Select(m => m.id).ToHashSet() ?? new HashSet<int>();

        return recentMessagesArr.Where(m => similarMessageIds.Contains(m.Id)).ToList();
    }

    private async Task<string> GenerateResponseAsync(string contextPrompt, string model)
    {
        Log.Information("Starting to generate response...");
        var stopwatch = Stopwatch.StartNew();

        var response = await _serviceManager.ModelService.GenerateResponseAsync(contextPrompt, model);

        stopwatch.Stop();
        Log.Information("GenerateResponseAsync took {ElapsedMilliseconds} seconds",
            stopwatch.Elapsed.TotalSeconds);
        return response;
    }

    private async Task<Message> SaveMessageAsync(CreateMessageRequest request, string response, Session session)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var message = _mapper.Map<Message>(request);
            message.Response = response;

            await _repositoryManager.MessageRepository.CreateAsync(message);

            session.UpdatedAt = DateTime.Now;
            await _repositoryManager.SessionRepository.Update(session);

            await _unitOfWork.CommitTransactionAsync();
            return message;
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
            contextBuilder.AppendLine($"Human: {msg.Query}");
            if (!string.IsNullOrWhiteSpace(msg.Response))
            {
                contextBuilder.AppendLine($"AI: {msg.Response}");
            }
        }

        contextBuilder.AppendLine($"Human: {newQuery}");
        return contextBuilder.ToString();
    }
}