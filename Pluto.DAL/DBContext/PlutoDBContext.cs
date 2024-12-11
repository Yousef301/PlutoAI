using Microsoft.EntityFrameworkCore;
using Pluto.DAL.Entities;
using Pluto.DAL.Interceptors;

namespace Pluto.DAL.DBContext;

public class PlutoDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Message> Messages { get; set; }

    public PlutoDbContext(DbContextOptions<PlutoDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.AddInterceptors(new UpdateAuditableInterceptor());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
}