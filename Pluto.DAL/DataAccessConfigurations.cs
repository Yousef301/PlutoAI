using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pluto.DAL.DBContext;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;
using Pluto.DAL.Repositories;

namespace Pluto.DAL;

public static class DataAccessConfigurations
{
    public static IServiceCollection AddDataAccessInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext(configuration)
            .AddRepositories();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        return services;
    }

    private static IServiceCollection AddDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PlutoDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PlutoAI"));
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IPasswordResetRequestRepository, PasswordResetRequestRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}