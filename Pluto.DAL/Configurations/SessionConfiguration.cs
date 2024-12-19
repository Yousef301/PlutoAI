using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pluto.DAL.Entities;
using Pluto.DAL.Enums;

namespace Pluto.DAL.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}