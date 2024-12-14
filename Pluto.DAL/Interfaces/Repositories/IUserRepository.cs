using System.Linq.Expressions;
using Google.Apis.Auth;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByConfirmationTokenAsync(Guid token);
    Task<User> CreateAsync(User user);
    Task<User> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload);
    Task<User> UpdateAsync(User user);
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
}