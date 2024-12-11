using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Pluto.DAL.Interfaces;

namespace Pluto.DAL.Interceptors;

public class UpdateAuditableInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext context)
    {
        DateTime now = DateTime.Now;
        var entities = context.ChangeTracker.Entries<IAuditableEntity>().ToList();

        foreach (EntityEntry<IAuditableEntity> entry in entities)
        {
            if (entry.State == EntityState.Added)
            {
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.CreatedAt), now);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetCurrentPropertyValue(entry, nameof(IAuditableEntity.UpdatedAt), now);
            }
        }

        static void SetCurrentPropertyValue(
            EntityEntry entry,
            string propertyName,
            DateTime now) =>
            entry.Property(propertyName).CurrentValue = now;
    }
}