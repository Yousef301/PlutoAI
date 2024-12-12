using Pluto.DAL.Entities;

namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface ITokenGeneratorService
{
    public string GenerateToken(User user);
}