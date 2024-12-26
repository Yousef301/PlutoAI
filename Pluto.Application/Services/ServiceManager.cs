using Microsoft.Extensions.DependencyInjection;
using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;

namespace Pluto.Application.Services;

public class ServiceManager : IServiceManager
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEmailService EmailService => _serviceProvider.GetRequiredService<IEmailService>();
    public IModelService ModelService => _serviceProvider.GetRequiredService<IModelService>();
    public IMessageService MessageService => _serviceProvider.GetRequiredService<IMessageService>();
    public ISessionService SessionService => _serviceProvider.GetRequiredService<ISessionService>();
    public IPasswordService PasswordService => _serviceProvider.GetRequiredService<IPasswordService>();
    public IGoogleOAuthService GoogleOAuthService => _serviceProvider.GetRequiredService<IGoogleOAuthService>();

    public IPasswordEncryptionService PasswordEncryptionService =>
        _serviceProvider.GetRequiredService<IPasswordEncryptionService>();

    public IMessageEmbeddingService MessageEmbeddingService =>
        _serviceProvider.GetRequiredService<IMessageEmbeddingService>();

    public ITokenGeneratorService TokenGeneratorService =>
        _serviceProvider.GetRequiredService<ITokenGeneratorService>();

    public IAuthenticationService AuthenticationService =>
        _serviceProvider.GetRequiredService<IAuthenticationService>();
}