using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pluto.DAL.Entities;

namespace Pluto.DAL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}