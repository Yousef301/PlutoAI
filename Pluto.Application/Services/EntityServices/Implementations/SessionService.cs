﻿using AutoMapper;
using Pluto.Application.DTOs.Sessions;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public SessionService(
        ISessionRepository sessionRepository,
        IMapper mapper,
        IUserRepository userRepository
    )
    {
        _sessionRepository = sessionRepository;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<GetSessionsResponse>> GetUserSessionsAsync(int userId)
    {
        if (!await _userRepository.ExistsAsync(u => u.Id == userId))
            throw new ArgumentException("User not found");

        var sessions = await _sessionRepository
            .GetUserSessionsAsync(userId, includeMessages: true);

        var _sessions = _mapper.Map<IEnumerable<GetSessionsResponse>>(sessions);

        return _sessions;
    }

    public async Task<CreateSessionResponse> CreateAsync(CreateSessionRequest session)
    {
        var createdSession = _mapper.Map<Session>(session);

        await _sessionRepository.CreateAsync(createdSession);

        return _mapper.Map<CreateSessionResponse>(createdSession);
    }

    public async Task<UpdateSessionTitleResponse> UpdateAsync(UpdateSessionTitleRequest session)
    {
        var sessionToUpdate = await _sessionRepository.GetAsync(session.Id);

        if (sessionToUpdate == null)
            throw new ArgumentException("Session not found");

        if (sessionToUpdate.UserId != session.UserId)
            throw new ArgumentException("Unauthorized");

        sessionToUpdate.Title = session.Title;

        await _sessionRepository.Update(sessionToUpdate);

        return _mapper.Map<UpdateSessionTitleResponse>(sessionToUpdate);
    }
}