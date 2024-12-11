using Pluto.Application.DTOs.Auth;

namespace Pluto.Application.Services.Interfaces;

public interface IUserService
{
    public Task<SignInResponse> SignInAsync(SignInRequest request);
    public Task<SignUpResponse> SignUpAsync(SignUpRequest request);
}