using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using sebkuw.Domain.Abstractions.Auditing;
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;

namespace sebkuw.EntityFrameworkCore.Auditing.Interceptors;

/// <summary>
/// Intercepts SaveChanges operations and automatically fills audit fields.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditSaveChangesInterceptor"/> class.
    /// </summary>
    /// <param name="currentUserProvider">Provides current user information.</param>
    /// <param name="dateTimeProvider">Provides current UTC time.</param>
    public AuditSaveChangesInterceptor(
        ICurrentUserProvider currentUserProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUserProvider = currentUserProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Called before synchronous SaveChanges execution.
    /// </summary>
    /// <param name="eventData">Context event data.</param>
    /// <param name="result">Current interception result.</param>
    /// <returns>The interception result.</returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditing(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Called before asynchronous SaveChanges execution.
    /// </summary>
    /// <param name="eventData">Context event data.</param>
    /// <param name="result">Current interception result.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The interception result.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditing(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Applies audit values to tracked entities.
    /// </summary>
    /// <param name="dbContext">The current database context.</param>
    private void ApplyAuditing(DbContext? dbContext)
    {
        if (dbContext is null)
            return;

        string userName = _currentUserProvider.GetUserName();
        DateTimeOffset utcNow = _dateTimeProvider.UtcNow();

        foreach (EntityEntry entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged)
                continue;

            SetCreatedValues(entry, userName, utcNow);
            SetUpdatedValues(entry, userName, utcNow);
        }
    }

    /// <summary>
    /// Sets creation audit values.
    /// </summary>
    /// <param name="entry">Tracked entity entry.</param>
    /// <param name="userName">Current user name.</param>
    /// <param name="utcNow">Current UTC date and time.</param>
    private static void SetCreatedValues(EntityEntry entry, string userName, DateTimeOffset utcNow)
    {
        if (entry.Entity is not ICreatedEntity createdEntity)
            return;

        switch (entry.State)
        {
            case EntityState.Added:
                createdEntity.CreatedBy = userName;
                createdEntity.CreatedAt = utcNow;
                break;

            case EntityState.Modified:
                entry.Property(nameof(ICreatedEntity.CreatedBy)).IsModified = false;
                entry.Property(nameof(ICreatedEntity.CreatedAt)).IsModified = false;
                break;
        }
    }

    /// <summary>
    /// Sets update audit values.
    /// </summary>
    /// <param name="entry">Tracked entity entry.</param>
    /// <param name="userName">Current user name.</param>
    /// <param name="utcNow">Current UTC date and time.</param>
    private static void SetUpdatedValues(EntityEntry entry, string userName, DateTimeOffset utcNow)
    {
        if (entry.Entity is not IUpdatedEntity updatedEntity)
            return;

        switch (entry.State)
        {
            case EntityState.Added:
                entry.Property(nameof(IUpdatedEntity.UpdatedBy)).IsModified = false;
                entry.Property(nameof(IUpdatedEntity.UpdatedAt)).IsModified = false;
                break;

            case EntityState.Modified:
                updatedEntity.UpdatedBy = userName;
                updatedEntity.UpdatedAt = utcNow;
                entry.Property(nameof(IUpdatedEntity.UpdatedBy)).IsModified = true;
                entry.Property(nameof(IUpdatedEntity.UpdatedAt)).IsModified = true;
                break;
        }
    }
}
