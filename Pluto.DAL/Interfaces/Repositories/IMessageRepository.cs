using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetSessionMessagesAsync(int sessionId, int take = 5, bool limit = false);
    Task<Message> CreateAsync(Message message);
}