using System.Linq.Expressions;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IPasswordResetRequestRepository
{
    Task<IEnumerable<PasswordResetRequest>> GetActiveRequestsByEmailAsync(int id);
    Task<PasswordResetRequest?> GetByTokenAsync(Guid token);
    Task<PasswordResetRequest> CreateAsync(PasswordResetRequest passwordResetRequest);
    PasswordResetRequest UpdateAsync(PasswordResetRequest passwordResetRequest);
    Task<bool> ExistsAsync(Expression<Func<PasswordResetRequest, bool>> predicate);
}