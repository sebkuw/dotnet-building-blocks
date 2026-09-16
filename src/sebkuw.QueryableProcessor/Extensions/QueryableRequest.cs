using sebkuw.QueryableProcessor.Models;

namespace sebkuw.QueryableProcessor.Extensions;

/// <summary>
/// Provides reusable composition of a complete query request.
/// </summary>
public static class QueryableRequest
{
    /// <summary>
    /// Applies the filters, sorting, and optionally pagination from a request without materializing the query.
    /// </summary>
    /// <typeparam name="T">The queried element type.</typeparam>
    /// <param name="source">The query to compose.</param>
    /// <param name="request">The filtering, sorting, and pagination request.</param>
    /// <param name="includePagination">
    /// <see langword="true"/> to apply <see cref="RequestDto.PaginationOptions"/>;
    /// otherwise only filtering and sorting are applied.
    /// </param>
    /// <returns>The composed query.</returns>
    public static IQueryable<T> ApplyRequest<T>(
        this IQueryable<T> source,
        RequestDto request,
        bool includePagination = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PaginationOptions);

        if (request.Filters is not null)
            source = source.ApplyFilters(request.Filters);

        if (!string.IsNullOrWhiteSpace(request.SortParam))
            source = source.SortBy(request.SortParam);

        return includePagination
            ? source.Paginate(request.PaginationOptions)
            : source;
    }
}
