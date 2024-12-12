using System.Linq.Expressions;
using Google.Apis.Auth;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload);
    User Update(User updatedUser);
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
}