using System.Linq.Expressions;
using Google.Apis.Auth;
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
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user != null) return user;
        user = new User
        {
            FullName = payload.Name,
            Email = payload.Email,
            Password = null
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
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