using AutoMapper;
using Pluto.Application.DTOs.Messages;
using Pluto.DAL.Entities;

namespace Pluto.Application.Mappers;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<Message, GetMessagesResponse>()
            .ForMember(dest => dest.MessageDate, opt => opt.MapFrom(src => src.CreatedAt));
        CreateMap<CreateMessageRequest, Message>();
        CreateMap<Message, CreateMessageResponse>();
    }
}