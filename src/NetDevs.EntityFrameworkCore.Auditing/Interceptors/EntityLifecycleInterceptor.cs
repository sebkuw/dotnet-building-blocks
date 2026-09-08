using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using NetDevs.Domain.Abstractions.History;
using NetDevs.Domain.Abstractions.Persistence;
using NetDevs.EntityFrameworkCore.Auditing.Exceptions;

namespace NetDevs.EntityFrameworkCore.Auditing.Interceptors;

/// <summary>
/// Validates persistence lifecycle constraints for tracked entities before they are saved.
/// </summary>
public sealed class EntityLifecycleInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ValidateChanges(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateChanges(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ValidateChanges(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is IHardDeleteProtected)
            {
                throw new HardDeleteNotAllowedException(entry.Metadata.ClrType.Name);
            }

            if (entry.State == EntityState.Modified && entry.Entity is IAppendOnlyEntity)
            {
                throw new AppendOnlyEntityModificationException(entry.Metadata.ClrType.Name);
            }
        }
    }
}
