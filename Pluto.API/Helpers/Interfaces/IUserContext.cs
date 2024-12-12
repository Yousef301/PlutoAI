namespace Pluto.API.Helpers.Interfaces;

public interface IUserContext
{
    int Id { get; }
    string FullName { get; }
    string Email { get; }
}