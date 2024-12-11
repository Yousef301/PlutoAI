using Pluto.DAL.DBContext;
using Pluto.DAL.Interfaces;

namespace Pluto.DAL.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly PlutoDbContext _context;

    public UnitOfWork(PlutoDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_context.Database.CurrentTransaction is null) return;

        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_context.Database.CurrentTransaction is null) return;

        await _context.Database.RollbackTransactionAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}