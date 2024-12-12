using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<IEnumerable<Session>> GetUserSessionsAsync(int userId, bool includeMessages = false);
    Task<Session> CreateAsync(Session session);
}