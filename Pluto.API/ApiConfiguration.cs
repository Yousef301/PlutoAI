using Amazon.CloudWatchLogs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Pluto.API.Helpers.Implementations;
using Pluto.API.Helpers.Interfaces;
using Pluto.Application;
using Pluto.DAL.Exceptions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace Pluto.API;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services
        , IConfiguration configuration)
    {
        services.AddApplicationInfrastructure(configuration)
            .AddAuthenticationConfigurations(configuration)
            .AddHttpContextAccessor()
            .AddSerilogConfigurations(configuration)
            .AddScoped<IUserContext, UserContext>()
            .ConfigureApplicationCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Domain = null;
                    options.Cookie.Path = "/";
                }
            );

        return services;
    }

    private static IServiceCollection AddAuthenticationConfigurations(this IServiceCollection services,
        IConfiguration configuration)
    {
        var clientId = configuration["Google:ClientId"];
        var clientSecret = configuration["Google:ClientSecret"];
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var secretKey = configuration["Jwt:SecretKey"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(issuer) ||
            string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secretKey))
            throw new InvalidConfigurationException("Authentication configurations are missing.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Convert.FromBase64String(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Request.Cookies.TryGetValue("accessToken", out var accessToken);
                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });


        return services;
    }

    private static IServiceCollection AddSerilogConfigurations(this IServiceCollection services,
        IConfiguration configuration)
    {
        var cloudWatchSinkOptions = new CloudWatchSinkOptions
        {
            LogGroupName =
                configuration["LogGroup"] ??
                throw new InvalidConfigurationException("Log group configuration is missing."),
            LogStreamNameProvider = new DefaultLogStreamProvider(),
            TextFormatter = new Serilog.Formatting.Json.JsonFormatter(),
            MinimumLogEventLevel = LogEventLevel.Information,
            BatchSizeLimit = 100,
            QueueSizeLimit = 10000,
            RetryAttempts = 5,
            Period = TimeSpan.FromSeconds(10),
            CreateLogGroup = true,
            LogGroupRetentionPolicy = LogGroupRetentionPolicy.OneMonth,
        };

        var cloudWatchClient = new AmazonCloudWatchLogsClient();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Override("Default", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.AmazonCloudWatch(cloudWatchSinkOptions, cloudWatchClient)
            .CreateLogger();

        services.AddSingleton(Log.Logger);

        return services;
    }
}