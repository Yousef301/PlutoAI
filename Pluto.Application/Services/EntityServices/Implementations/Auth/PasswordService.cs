using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.DAL.Entities;
using Pluto.DAL.Enums;
using Pluto.DAL.Exceptions;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations.Auth;

public class PasswordService : IPasswordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IServiceManager _serviceManager;
    private readonly IRepositoryManager _repositoryManager;

    public PasswordService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IServiceManager serviceManager,
        IRepositoryManager repositoryManager
    )
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _serviceManager = serviceManager;
        _repositoryManager = repositoryManager;
    }

    public async Task SendPasswordResetEmail(SendPasswordResetRequest request)
    {
        try
        {
            var user = await _repositoryManager.UserRepository.GetByEmailAsync(request.Email)
                       ?? throw new NotFoundException("User", "email", request.Email);

            await _unitOfWork.BeginTransactionAsync();

            var baseUrl = _configuration["ResetPassword"];

            var passwordResetRequest = new PasswordResetRequest
            {
                Token = Guid.NewGuid(),
                ExpiryDate = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds(),
                UserId = user.Id
            };

            await _repositoryManager.PasswordResetRequestRepository.CreateAsync(passwordResetRequest);

            var resetLink = $"{baseUrl}?token={passwordResetRequest.Token}";
            await _serviceManager.EmailService.SendEmailAsync(request.Email, "Password Reset",
                new EmailConfirmationBody(resetLink), Template.PasswordReset);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (NotFoundException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }


    public async Task ResetPassword(ResetPasswordRequest request)
    {
        try
        {
            var passwordResetRequest = await _repositoryManager.PasswordResetRequestRepository
                                           .GetByTokenAsync(Guid.Parse(request.Token))
                                       ?? throw new InvalidTokenException("Invalid token.");

            var user = await _repositoryManager.UserRepository.GetAsync(passwordResetRequest.UserId)
                       ?? throw new NotFoundException("User", "id", passwordResetRequest.UserId.ToString());

            await _unitOfWork.BeginTransactionAsync();

            user.Password = _serviceManager.PasswordEncryptionService
                .HashPassword(request.Password);

            await _repositoryManager.UserRepository.UpdateAsync(user);

            passwordResetRequest.Used = true;
            await _repositoryManager.PasswordResetRequestRepository.UpdateAsync(passwordResetRequest);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (InvalidTokenException ex)
        {
            throw;
        }
        catch (NotFoundException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}