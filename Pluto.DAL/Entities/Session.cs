using Pluto.DAL.Enums;

namespace Pluto.DAL.Entities;

public class Session : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = "New Chat";
    public DateTime UpdatedAt = DateTime.Now;

    public User User { get; set; }
    public ICollection<Message> Messages { get; set; }
}