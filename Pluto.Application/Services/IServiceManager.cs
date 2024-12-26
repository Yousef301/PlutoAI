using Pluto.Application.Services.EntityServices.Interfaces;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;

namespace Pluto.Application.Services;

public interface IServiceManager
{
    IEmailService EmailService { get; }
    IModelService ModelService { get; }
    IPasswordService PasswordService { get; }
    ITokenGeneratorService TokenGeneratorService { get; }
    IMessageService MessageService { get; }
    ISessionService SessionService { get; }
    IAuthenticationService AuthenticationService { get; }
    IGoogleOAuthService GoogleOAuthService { get; }
    IPasswordEncryptionService PasswordEncryptionService { get; }
    IMessageEmbeddingService MessageEmbeddingService { get; }
}