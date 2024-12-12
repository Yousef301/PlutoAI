using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<IEnumerable<Session>> GetUserSessionsAsync(int userId);
    Task<Session> CreateAsync(Session session);
}