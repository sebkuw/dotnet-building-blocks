using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetDevs.QueryableProcessor.Models;

/// <summary>
/// Represents a request for filtering, sorting, and paginating data.
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Sorting parameter in format: "asc_PropertyName" or "desc_PropertyName".
    /// </summary>
    public string? SortParam { get; init; }

    /// <summary>
    /// Collection of filter conditions to apply.
    /// </summary>
    public IEnumerable<FilterCondition>? Filters { get; init; }

    /// <summary>
    /// Pagination settings including page number and size.
    /// </summary>
    [Required]
    public required PaginationOptions PaginationOptions { get; init; }
}