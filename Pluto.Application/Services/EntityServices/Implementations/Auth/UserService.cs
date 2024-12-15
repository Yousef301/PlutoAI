using AutoMapper;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Enums;
using Pluto.DAL.Exceptions;
using Pluto.DAL.Exceptions.Base;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.EntityServices.Implementations.Auth;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenGeneratorService _tokenGeneratorService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IPasswordResetRequestRepository _passwordResetRequestRepository;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenGeneratorService tokenGeneratorService,
        IMapper mapper,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IPasswordResetRequestRepository passwordResetRequestRepository
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenGeneratorService = tokenGeneratorService;
        _mapper = mapper;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _passwordResetRequestRepository = passwordResetRequestRepository;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordService.ValidatePassword(request.Password, user.Password))
            throw new InvalidCredentialsException("Invalid email or password.");

        if (!user.EmailConfirmed)
            throw new EmailNotConfirmedException("Email address is not confirmed.");

        return new SignInResponse(_tokenGeneratorService.GenerateToken(user), user.EmailConfirmed);
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        try
        {
            if (await _userRepository.ExistsAsync(u => u.Email == request.Email))
                throw new UserAlreadyExistsException("User with this email already exists.");

            var user = _mapper.Map<User>(request);

            user.Password = _passwordService.HashPassword(request.Password);

            var createdUser = await _userRepository.CreateAsync(user);

            return _mapper.Map<SignUpResponse>(createdUser);
        }
        catch (Exception ex)
        {
            throw new InternalServerErrorException("An error occurred while processing your request.");
        }
    }

    public async Task SendConfirmationEmail(EmailConfirmationRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email)
                       ?? throw new NotFoundException("User", "email", request.Email);

            await _unitOfWork.BeginTransactionAsync();

            var baseUrl = _configuration["BaseUrl"];

            user.EmailConfirmationToken = Guid.NewGuid();
            user.EmailConfirmationTokenExpiration =
                ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds();

            await _userRepository.UpdateAsync(user);

            var confirmationLink = $"{baseUrl}/confirm-email?token={user.EmailConfirmationToken}";
            await _emailService.SendEmailAsync(request.Email, "Confirmation Email",
                new EmailConfirmationBody(confirmationLink), Template.EmailConfirmation);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }


    public async Task ConfirmEmail(string token)
    {
        try
        {
            var user = await _userRepository.GetByConfirmationTokenAsync(Guid.Parse(token))
                       ?? throw new InvalidTokenException("The provided token is invalid.");

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiration = null;

            await _userRepository.UpdateAsync(user);
        }
        catch (Exception ex)
        {
            throw new InternalServerErrorException("An error occurred while processing the email confirmation.");
        }
    }


    public async Task SendPasswordResetEmail(SendPasswordResetRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email)
                       ?? throw new NotFoundException("User", "email", request.Email);

            await _unitOfWork.BeginTransactionAsync();

            var baseUrl = _configuration["ResetPassword"];

            var passwordResetRequest = new PasswordResetRequest
            {
                Token = Guid.NewGuid(),
                ExpiryDate = ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds(),
                UserId = user.Id
            };

            await _passwordResetRequestRepository.CreateAsync(passwordResetRequest);

            var resetLink = $"{baseUrl}?token={passwordResetRequest.Token}";
            await _emailService.SendEmailAsync(request.Email, "Password Reset",
                new EmailConfirmationBody(resetLink), Template.PasswordReset);

            await _unitOfWork.CommitTransactionAsync();
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
            var passwordResetRequest = await _passwordResetRequestRepository
                                           .GetByTokenAsync(Guid.Parse(request.Token))
                                       ?? throw new InvalidTokenException("Invalid token.");

            var user = await _userRepository.GetAsync(passwordResetRequest.UserId)
                       ?? throw new NotFoundException("User", "id", passwordResetRequest.UserId.ToString());

            await _unitOfWork.BeginTransactionAsync();

            user.Password = _passwordService.HashPassword(request.Password);
            await _userRepository.UpdateAsync(user);

            passwordResetRequest.Used = true;
            await _passwordResetRequestRepository.UpdateAsync(passwordResetRequest);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}