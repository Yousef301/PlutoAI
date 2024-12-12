namespace Pluto.DAL.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    
    public ICollection<Session> Sessions { get; set; }
}