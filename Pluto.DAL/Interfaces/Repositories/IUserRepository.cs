using System.Linq.Expressions;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    User Update(User updatedUser);
    Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate);
}