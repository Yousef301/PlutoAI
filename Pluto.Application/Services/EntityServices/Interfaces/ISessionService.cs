using Pluto.Application.DTOs.Sessions;
using Pluto.DAL.Entities;

namespace Pluto.Application.Services.EntityServices.Interfaces;

public interface ISessionService
{
    Task<IEnumerable<GetSessionsResponse>> GetUserSessionsAsync(int userId);
    Task<CreateSessionResponse> CreateAsync(CreateSessionRequest session);
}