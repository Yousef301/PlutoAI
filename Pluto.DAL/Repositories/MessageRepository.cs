using Microsoft.EntityFrameworkCore;
using Pluto.DAL.DBContext;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.DAL.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly PlutoDbContext _context;

    public MessageRepository(PlutoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetSessionMessagesAsync(int sessionId)
    {
        return await _context.Messages
            .Where(m => m.SessionId == sessionId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Message> CreateAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        return message;
    }
}