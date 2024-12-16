using Pluto.Application.Services.SharedServices.Interfaces;


namespace Pluto.Application.Services.SharedServices.Implementations;

public class BCryptPasswordService : IPasswordEncryptionService
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