using System;
using System.Collections.Generic;

namespace NetDevs.QueryableProcessor.Models;

/// <summary>
/// Represents the response for a paginated query, including metadata.
/// </summary>
/// <typeparam name="T">The type of data in the pagination response.</typeparam>
public class PaginationResponse<T>
{
    /// <summary>
    /// The filtered data for the requested page.
    /// </summary>
    public IEnumerable<T> Data { get; set; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// The requested page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Indicates whether there is a next page available.
    /// </summary>
    public bool HasNextPage => PageNumber * PageSize < TotalItems;

    /// <summary>
    /// Indicates whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Initializes a new instance of PaginationResponse with paginated data and metadata.
    /// </summary>
    /// <param name="data">The filtered data.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <param name="pageNumber">The requested page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginationResponse(IEnumerable<T> data, int totalItems, int pageNumber, int pageSize)
    {
        Data = data;
        TotalItems = totalItems;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);
    }
}