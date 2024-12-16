namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface IPasswordEncryptionService
{
    string HashPassword(string plainPassword);
    bool ValidatePassword(string plainPassword, string hashedPassword);
}