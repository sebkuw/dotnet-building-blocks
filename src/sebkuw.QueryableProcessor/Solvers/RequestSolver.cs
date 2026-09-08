using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using sebkuw.QueryableProcessor.Extensions;
using sebkuw.QueryableProcessor.Models;

namespace sebkuw.QueryableProcessor.Solvers;

/// <summary>
/// Applies a complete query request and projects a paginated response.
/// </summary>
public static class RequestSolver
{
    /// <summary>
    /// Processes an IQueryable query by applying filtering, sorting, pagination,
    /// and mapping results using AutoMapper, with support for asynchronous execution.
    /// </summary>
    /// <typeparam name="TSource">The entity type.</typeparam>
    /// <typeparam name="TDestination">The DTO type.</typeparam>
    /// <param name="source">The IQueryable source to modify.</param>
    /// <param name="request">Object containing sortParam, Filters and PaginationProperties</param>
    /// <param name="selector">An expression that projects from TSource to TDestination.</param>
    /// <param name="cancellationToken">Optional cancellation token for async operations.</param>
    /// <returns>
    /// A PaginationResponse containing filtered, sorted, paginated and mapped results.
    /// </returns>
    public static async Task<PaginationResponse<TDestination>> SolveRequest<TSource, TDestination>(
        this IQueryable<TSource> source,
        RequestDto request,
        Expression<Func<TSource, TDestination>> selector,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PaginationOptions);
        ArgumentNullException.ThrowIfNull(selector);

        // Apply filters if provided
        if (request.Filters != null && request.Filters.Any())
            source = source.ApplyFilters(request.Filters);

        // Apply sorting if specified
        if (!string.IsNullOrWhiteSpace(request.SortParam))
            source = source.SortBy(request.SortParam);

        // Get total count before pagination
        int totalItems = await source.CountAsync(cancellationToken);

        // Apply pagination and project results using the provided selector
        List<TDestination> paginatedData = await source
            .Paginate(request.PaginationOptions)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PaginationResponse<TDestination>(paginatedData, totalItems, request.PaginationOptions.PageNumber, request.PaginationOptions.PageSize);
    }
}
