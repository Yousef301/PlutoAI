using Pluto.DAL.Interfaces;

namespace Pluto.DAL.Entities;

public class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}