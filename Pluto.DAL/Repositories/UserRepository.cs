using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pluto.DAL.DBContext;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.DAL.Repositories;

public class UserRepository : IUserRepository
{
    private PlutoDbContext _context;

    public UserRepository(PlutoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetAsync(int id)
    {
        return await _context.Users
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        var createdUser = await _context.Users
            .AddAsync(user);

        return createdUser.Entity;
    }

    public User Update(User updatedUser)
    {
        _context.Users.Update(updatedUser);

        return updatedUser;
    }

    public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
    {
        return await _context.Users.AnyAsync(predicate);
    }
}