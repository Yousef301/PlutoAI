using AutoMapper;
using Pluto.Application.DTOs.Auth;
using Pluto.DAL.Entities;

namespace Pluto.Application.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<SignUpRequest, User>();
        CreateMap<User, SignUpResponse>();
    }
}