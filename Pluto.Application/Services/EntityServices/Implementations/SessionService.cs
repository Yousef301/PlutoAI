using AutoMapper;
using Pluto.Application.DTOs.Sessions;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations;

public class SessionService : ISessionService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public SessionService(
        IMapper mapper,
        IRepositoryManager repositoryManager
    )
    {
        _mapper = mapper;
        _repositoryManager = repositoryManager;
    }

    public async Task<IEnumerable<GetSessionsResponse>> GetUserSessionsAsync(int userId)
    {
        if (!await _repositoryManager.UserRepository.ExistsAsync(u => u.Id == userId))
            throw new NotFoundException("User", userId);

        var sessions = await _repositoryManager.SessionRepository
            .GetUserSessionsAsync(userId, includeMessages: true);

        return _mapper.Map<IEnumerable<GetSessionsResponse>>(sessions);
    }

    public async Task<CreateSessionResponse> CreateAsync(CreateSessionRequest session)
    {
        var createdSession = _mapper.Map<Session>(session);

        await _repositoryManager.SessionRepository.CreateAsync(createdSession);

        return _mapper.Map<CreateSessionResponse>(createdSession);
    }

    public async Task<UpdateSessionTitleResponse> UpdateAsync(UpdateSessionTitleRequest session)
    {
        var sessionToUpdate = await _repositoryManager.SessionRepository.GetAsync(session.Id);

        if (sessionToUpdate == null)
            throw new NotFoundException("Session", session.Id);

        if (sessionToUpdate.UserId != session.UserId)
            throw new UnauthorizedAccessException("You are not allowed to update this session.");

        sessionToUpdate.Title = session.Title;

        await _repositoryManager.SessionRepository.Update(sessionToUpdate);

        return _mapper.Map<UpdateSessionTitleResponse>(sessionToUpdate);
    }
}