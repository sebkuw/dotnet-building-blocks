using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetDevs.QueryableProcessor.Models;

/// <summary>
/// Represents a request for filtering, sorting, and paginating data.
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Sorting parameter in format "asc_PropertyName", "desc_PropertyName",
    /// "PropertyName asc", or "PropertyName desc".
    /// </summary>
    [JsonPropertyName("SortParam")]
    public string? SortParam { get; init; }

    /// <summary>
    /// Collection of filter conditions to apply.
    /// </summary>
    [JsonPropertyName("Filters")]
    public IEnumerable<FilterCondition>? Filters { get; init; }

    /// <summary>
    /// Pagination settings including page number and size.
    /// </summary>
    [Required]
    [JsonPropertyName("PaginationOptions")]
    public required PaginationOptions PaginationOptions { get; init; }
}
