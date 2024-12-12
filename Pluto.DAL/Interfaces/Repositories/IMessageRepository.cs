using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetSessionMessagesAsync(int sessionId);
    Task<Message> CreateMessageAsync(Message message);
}