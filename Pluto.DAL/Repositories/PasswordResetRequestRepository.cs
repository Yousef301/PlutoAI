using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pluto.DAL.DBContext;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.DAL.Repositories;

public class PasswordResetRequestRepository : IPasswordResetRequestRepository
{
    private readonly PlutoDbContext _context;

    public PasswordResetRequestRepository(PlutoDbContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<PasswordResetRequest>> GetActiveRequestsAsync(int id)
    {
        return await _context.PasswordResetRequests
            .Where(c => c.User.Id == id &&
                        c.ExpiryDate.HasValue &&
                        !c.Used
            )
            .ToListAsync();
    }

    public async Task<PasswordResetRequest?> GetByTokenAsync(Guid token)
    {
        var passwordResetRequest = await _context.PasswordResetRequests
            .SingleOrDefaultAsync(c => c.Token == token);

        if (passwordResetRequest != null &&
            passwordResetRequest.ExpiryDate.HasValue &&
            !passwordResetRequest.Used &&
            DateTimeOffset
                .FromUnixTimeSeconds(passwordResetRequest.ExpiryDate.Value)
                .UtcDateTime > DateTime.UtcNow)
        {
            return passwordResetRequest;
        }

        return null;
    }

    public async Task<PasswordResetRequest> CreateAsync(PasswordResetRequest passwordResetRequest)
    {
        await _context.PasswordResetRequests
            .AddAsync(passwordResetRequest);

        await _context.SaveChangesAsync();

        return passwordResetRequest;
    }

    public PasswordResetRequest UpdateAsync(PasswordResetRequest passwordResetRequest)
    {
        _context.PasswordResetRequests.Update(passwordResetRequest);

        return passwordResetRequest;
    }

    public async Task<bool> ExistsAsync(Expression<Func<PasswordResetRequest, bool>> predicate)
    {
        return await _context.PasswordResetRequests
            .AnyAsync(predicate);
    }
}