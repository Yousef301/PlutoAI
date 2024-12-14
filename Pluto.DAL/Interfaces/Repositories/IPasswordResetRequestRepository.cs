using System.Linq.Expressions;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IPasswordResetRequestRepository
{
    Task<PasswordResetRequest?> GetByTokenAsync(Guid token);
    Task<PasswordResetRequest> CreateAsync(PasswordResetRequest passwordResetRequest);
    Task<PasswordResetRequest> UpdateAsync(PasswordResetRequest passwordResetRequest);
    Task<bool> ExistsAsync(Expression<Func<PasswordResetRequest, bool>> predicate);
}