using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pluto.Application.Services.Implementations;
using Pluto.Application.Services.Interfaces;
using Pluto.DAL;

namespace Pluto.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDataAccessInfrastructure(configuration);
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddSingleton<ITokenGeneratorService, JwtTokenGeneratorService>();
        services.AddSingleton<IPasswordService, BCryptPasswordService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }
}