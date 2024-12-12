using Pluto.DAL.Enums;

namespace Pluto.DAL.Entities;

public class Session : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = "New Chat";
    public Model Model = Model.Ollama;

    public User User { get; set; }
    public ICollection<Message> Messages { get; set; }
}