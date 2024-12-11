namespace Pluto.DAL.Entities;

public class Message : BaseEntity
{
    public int SessionId { get; set; }
    public string Query { get; set; }
    public string Response { get; set; }

    public Session Session { get; set; }
}