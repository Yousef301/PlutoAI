namespace Pluto.DAL.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Password { get; set; }
    public bool EmailConfirmed { get; set; } = false;
    public Guid? EmailConfirmationToken { get; set; }
    public long? EmailConfirmationTokenExpiration { get; set; }
    public ICollection<Session> Sessions { get; set; }
}