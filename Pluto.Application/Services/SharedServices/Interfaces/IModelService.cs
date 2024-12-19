namespace Pluto.Application.Services.SharedServices.Interfaces;

public interface IModelService
{
    Task<string> GenerateResponseAsync(string prompt, string model);
}