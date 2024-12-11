namespace Pluto.Application.Services.Interfaces;

public interface IPasswordService
{
    string HashPassword(string plainPassword);
    bool ValidatePassword(string plainPassword, string hashedPassword);
}