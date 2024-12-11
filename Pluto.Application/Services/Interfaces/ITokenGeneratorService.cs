using Pluto.DAL.Entities;

namespace Pluto.Application.Services.Interfaces;

public interface ITokenGeneratorService
{
    public string GenerateToken(User user);
}