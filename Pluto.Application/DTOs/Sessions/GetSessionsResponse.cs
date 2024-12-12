using Pluto.DAL.Entities;

namespace Pluto.Application.DTOs.Sessions;

public record GetSessionsResponse(int Id, string Title, int MessagesCount);