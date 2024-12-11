using Microsoft.IdentityModel.Tokens;
using Pluto.Application;

namespace Pluto.API;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services
        , IConfiguration configuration)
    {
        services.AddApplicationInfrastructure(configuration)
            .AddAuthenticationConfigurations(configuration);

        return services;
    }

    private static IServiceCollection AddAuthenticationConfigurations(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Issuer"],
                    ValidAudience = configuration["Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Convert.FromBase64String(configuration["SecretKey"])),
                    ClockSkew = TimeSpan
                        .Zero
                };
            });

        return services;
    }
}