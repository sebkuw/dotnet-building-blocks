using sebkuw.QueryableProcessor.Constants;
using sebkuw.QueryableProcessor.Models;

namespace sebkuw.QueryableProcessor.Extensions;

/// <summary>
/// Provides pagination operations for queryable sources.
/// </summary>
public static class QueryablePaginate
{
    /// <summary>
    /// Applies pagination to an IQueryable collection using Skip and Take operations.
    /// </summary>
    /// <typeparam name="T">The type of elements in the IQueryable collection.</typeparam>
    /// <param name="source">The IQueryable source to paginate.</param>
    /// <param name="pagination">Pagination options specifying page number and size.</param>
    /// <returns>An IQueryable with pagination applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source or pagination is null.</exception>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, PaginationOptions pagination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(pagination);

        if (pagination.PageNumber < 1)
            throw new ArgumentException("Page number must be at least 1.", nameof(pagination));

        if (pagination.PageSize < 1)
            throw new ArgumentException("Page size must be at least 1.", nameof(pagination));

        if (pagination.PageSize > SettingsConstants.MaxPageSize)
            throw new ArgumentException($"Page size must be between 1 and {SettingsConstants.MaxPageSize}.", nameof(pagination));

        return source.Skip((pagination.PageNumber - 1) * pagination.PageSize).Take(pagination.PageSize);
    }
}
