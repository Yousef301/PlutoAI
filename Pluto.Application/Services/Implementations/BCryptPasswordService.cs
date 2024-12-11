using Pluto.Application.Services.Interfaces;


namespace Pluto.Application.Services.Implementations;

public class BCryptPasswordService : IPasswordService
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool ValidatePassword(string plainPassword,
        string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}