namespace Pluto.Application.DTOs.Messages;

public class CreateMessageRequest : BaseMessageRequest
{
    public string Query { get; set; }
    public string Model { get; set; }
}