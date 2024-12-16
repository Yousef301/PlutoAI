namespace Pluto.DAL.Interfaces.Repositories;

public interface IRepositoryManager
{
    IMessageRepository MessageRepository { get; }
    IPasswordResetRequestRepository PasswordResetRequestRepository { get; }
    ISessionRepository SessionRepository { get; }
    IUserRepository UserRepository { get; }
}