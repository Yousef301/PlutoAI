using Pluto.DAL.Entities;

namespace Pluto.Application.DTOs.Sessions;

public class GetSessionsResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int MessagesCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}