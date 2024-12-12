using AutoMapper;
using Pluto.Application.DTOs.Auth;
using Pluto.Application.Services.Interfaces;
using Pluto.DAL.Entities;
using Pluto.DAL.Interfaces;
using Pluto.DAL.Interfaces.Repositories;

namespace Pluto.Application.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenGeneratorService _tokenGeneratorService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenGeneratorService tokenGeneratorService,
        IMapper mapper
    )
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenGeneratorService = tokenGeneratorService;
        _mapper = mapper;
    }

    public async Task<SignInResponse> SignInAsync(SignInRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email)
                   ?? throw new Exception("Email or password is incorrect");

        if (!_passwordService.ValidatePassword(request.Password, user.Password))
            throw new Exception("Email or password is incorrect");

        return new SignInResponse(_tokenGeneratorService.GenerateToken(user));
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
}