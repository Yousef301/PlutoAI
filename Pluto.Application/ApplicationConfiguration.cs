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

        services.AddSingleton<IPasswordService, BCryptPasswordService>();
        services.AddScoped<ITokenGeneratorService, JwtTokenGeneratorService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IModelService, OllamaService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddHttpClient();

        return services;
    }
}