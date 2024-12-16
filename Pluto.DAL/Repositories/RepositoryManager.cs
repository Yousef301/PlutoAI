using Pluto.DAL.DBContext;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.DAL.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly PlutoDbContext _context;

    public RepositoryManager(PlutoDbContext context)
    {
        _context = context;
    }

    public IMessageRepository MessageRepository => new MessageRepository(_context);

    public IPasswordResetRequestRepository PasswordResetRequestRepository =>
        new PasswordResetRequestRepository(_context);

    public ISessionRepository SessionRepository => new SessionRepository(_context);
    public IUserRepository UserRepository => new UserRepository(_context);
}