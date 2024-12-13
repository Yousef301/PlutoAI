namespace Pluto.Application.DTOs.Messages;

public class GetMessagesResponse
{
    public string Query { get; set; }
    public string Response { get; set; }
    public DateTime MessageDate { get; set; }
}