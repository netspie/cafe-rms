using CafeRMS.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CafeRMS.Api.Persistence.Interceptors;

public sealed class AuditableSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditTimestamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditTimestamps(DbContext? context)
    {
        if (context is null)
            return;

        var auditEntries = context.ChangeTracker
            .Entries<IAuditable>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToArray();

        if (auditEntries.Length == 0)
            return;
    
        var utcNow = DateTimeOffset.UtcNow;
        var currentUserId = httpContextAccessor.CurrentUserId;

        foreach (var entry in auditEntries)
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = utcNow;
                entry.Property(x => x.CreatedBy).CurrentValue = currentUserId;
            }
            else
            {
                entry.Property(x => x.UpdatedAt).CurrentValue = utcNow;
                entry.Property(x => x.UpdatedBy).CurrentValue = currentUserId;
            }
    }
}
