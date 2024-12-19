using AutoMapper;
using Microsoft.AspNetCore.Http;
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

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IRepositoryManager repositoryManager,
        IServiceManager serviceManager
    )
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _repositoryManager = repositoryManager;
        _serviceManager = serviceManager;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var user = await _repositoryManager.UserRepository.GetByEmailAsync(request.Email);
        if (user == null || !_serviceManager.PasswordEncryptionService
                .ValidatePassword(request.Password, user.Password))
            throw new InvalidCredentialsException("Invalid email or password.");

        var tokens = await _serviceManager.TokenGeneratorService
            .GenerateToken(user, true);

        return new SignInResponse(tokens.AccessToken, tokens.RefreshToken, user.EmailConfirmed);
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        if (await _repositoryManager.UserRepository.ExistsAsync(u => u.Email == request.Email))
            throw new UserAlreadyExistsException("User with this email already exists.");

        var user = _mapper.Map<User>(request);
        user.Password = _serviceManager.PasswordEncryptionService.HashPassword(request.Password);

        var createdUser = await _repositoryManager.UserRepository.CreateAsync(user);

        return _mapper.Map<SignUpResponse>(createdUser);
    }

    public async Task SendConfirmationEmail(EmailConfirmationRequest request)
    {
        try
        {
            var user = await _repositoryManager.UserRepository.GetByEmailAsync(request.Email)
                       ?? throw new NotFoundException("User", "email", request.Email);

            await _unitOfWork.BeginTransactionAsync();

            var baseUrl = _configuration["BaseUrl"];

            user.EmailConfirmationToken = Guid.NewGuid();
            user.EmailConfirmationTokenExpiration =
                ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds();

            await _repositoryManager.UserRepository.UpdateAsync(user);

            var confirmationLink = $"{baseUrl}/auth/confirm-email?token={user.EmailConfirmationToken}";
            await _serviceManager.EmailService
                .SendEmailAsync(request.Email, "Confirmation Email",
                    new EmailConfirmationBody(confirmationLink), Template.EmailConfirmation);

            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }


    public async Task ConfirmEmail(string token)
    {
        var user = await _repositoryManager.UserRepository.GetByConfirmationTokenAsync(Guid.Parse(token))
                   ?? throw new InvalidTokenException("The provided token is invalid.");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiration = null;

        await _repositoryManager.UserRepository.UpdateAsync(user);
    }


    public void SetTokenInsideCookie(TokenDto token, HttpContext httpContext)
    {
        var accessTokenExpiration = _configuration.GetValue<int>("Cookies:AccessTokenExpirationMinutes");
        var refreshTokenExpiration = _configuration.GetValue<int>("Cookies:RefreshTokenExpirationDays");

        httpContext.Response.Cookies.Append("accessToken", token.AccessToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpiration),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

        httpContext.Response.Cookies.Append("refreshToken", token.RefreshToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenExpiration),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
    }

    public void RemoveCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete("accessToken", new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });

        httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });
    }
}