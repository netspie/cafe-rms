using CafeRMS.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CafeRMS.Api.Persistence.Interceptors;

public sealed class SoftDeletableSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertHardDeletesToSoftDeletes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertHardDeletesToSoftDeletes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ConvertHardDeletesToSoftDeletes(DbContext? context)
    {
        if (context is null)
            return;

        var softDeleteEntries = context.ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(x => x.State == EntityState.Deleted)
            .ToArray();

        if (softDeleteEntries.Length == 0)
            return;

        var utcNow = DateTimeOffset.UtcNow;
        var currentUserId = httpContextAccessor.CurrentUserId;

        foreach (var entry in softDeleteEntries)
        {
            entry.State = EntityState.Unchanged;
            entry.Property(x => x.DeletedAt).CurrentValue = utcNow;
            entry.Property(x => x.DeletedBy).CurrentValue = currentUserId;
        }
    }
}
