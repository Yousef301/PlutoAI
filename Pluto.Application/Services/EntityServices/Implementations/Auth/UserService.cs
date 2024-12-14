using AutoMapper;
using Microsoft.Extensions.Configuration;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.EntityServices.Interfaces.Auth;
using Pluto.Application.Services.SharedServices.Interfaces;
using Pluto.DAL.Entities;
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

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenGeneratorService tokenGeneratorService,
        IMapper mapper,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenGeneratorService = tokenGeneratorService;
        _mapper = mapper;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
                   ?? throw new Exception("Email or password is incorrect");

        if (!_passwordService.ValidatePassword(request.Password, user.Password))
            throw new Exception("Email or password is incorrect");

        return new SignInResponse(_tokenGeneratorService.GenerateToken(user), user.EmailConfirmed);
    }

    public async Task<SignUpResponse> SignUpAsync(SignUpRequest request)
    {
        if (await _userRepository.ExistsAsync(u => u.Email == request.Email))
            throw new Exception("User with this email already exists");

        var user = _mapper.Map<User>(request);

        user.Password = _passwordService.HashPassword(request.Password);

        var createdUser = await _userRepository.CreateAsync(user);

        return _mapper.Map<SignUpResponse>(createdUser);
    }

    public async Task SendConfirmationEmail(EmailConfirmationRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
                   ?? throw new Exception("User with this email does not exist");


        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var baseUrl = _configuration["BaseUrl"];

            user.EmailConfirmationToken = Guid.NewGuid();
            user.EmailConfirmationTokenExpiration =
                ((DateTimeOffset)DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds();

            await _userRepository.UpdateAsync(user);

            await _emailService.SendEmailAsync(request.Email, "Confirmation Email",
                new EmailConfirmationBody(baseUrl + "/confirm-email?token=" + user.EmailConfirmationToken));

            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task ConfirmEmail(string token)
    {
        var user = await _userRepository.GetByConfirmationTokenAsync(Guid.Parse(token))
                   ?? throw new Exception("Invalid token");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiration = null;

        await _userRepository.UpdateAsync(user);
    }
}