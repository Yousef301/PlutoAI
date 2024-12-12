﻿using AutoMapper;
using Pluto.Application.DTOs.Sessions;
using Pluto.DAL.Entities;

namespace Pluto.Application.Mappers;

public class SessionProfile : Profile
{
    public SessionProfile()
    {
        CreateMap<CreateSessionRequest, Session>();
        CreateMap<Session, CreateSessionResponse>();
        CreateMap<Session, GetSessionsResponse>()
            .ForMember(
                dest => dest.MessagesCount,
                opt => opt.MapFrom(src => src.Messages.Count)
            );
    }
}