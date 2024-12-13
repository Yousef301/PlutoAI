using System.Linq.Expressions;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<Session?> GetAsync(int id);
    Task<IEnumerable<Session>> GetUserSessionsAsync(int userId, bool includeMessages = false);
    Task<Session> CreateAsync(Session session);
    Task<Session> Update(Session session);
    Task<bool> ExistsAsync(Expression<Func<Session, bool>> predicate);
}