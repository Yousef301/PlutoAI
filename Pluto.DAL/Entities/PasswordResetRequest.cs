namespace Pluto.DAL.Entities;

public class PasswordResetRequest : BaseEntity
{
    public int UserId { get; set; }
    public Guid Token { get; set; }
    public long? ExpiryDate { get; set; }

    public User User { get; set; }
}