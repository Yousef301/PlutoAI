using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pluto.DAL.DBContext;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.DAL.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly PlutoDbContext _context;

    public SessionRepository(PlutoDbContext context)
    {
        _context = context;
    }

    public async Task<Session?> GetAsync(int id)
    {
        return await _context.Sessions
            .SingleOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Session>> GetUserSessionsAsync(
        int userId,
        bool includeMessages = false
    )
    {
        var query = _context.Sessions
            .Where(s => s.UserId == userId);

        if (includeMessages)
            query = query.Include(s => s.Messages);

        return await query.ToListAsync();
    }

    public async Task<Session> CreateAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<Session> Update(Session session)
    {
        _context.Sessions.Update(session);

        await _context.SaveChangesAsync();

        return session;
    }

    public async Task DeleteAsync(Session session)
    {
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<Session, bool>> predicate)
    {
        return await _context.Sessions.AnyAsync(predicate);
    }
}