using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pluto.Application.Services.EntityServices.Implementations;
using Pluto.Application.Services.EntityServices.Implementations.Auth;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Implementations;
using Pluto.Application.Services.SharedServices.Interfaces;
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
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        return services;
    }
}