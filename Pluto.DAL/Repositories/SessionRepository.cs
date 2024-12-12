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
}