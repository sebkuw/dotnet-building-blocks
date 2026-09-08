using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using sebkuw.QueryableProcessor.Constants;

namespace sebkuw.QueryableProcessor.Models;

/// <summary>
/// Represents pagination parameters for paginated queries.
/// </summary>
public class PaginationOptions
{
    /// <summary>
    /// The page number (1-based index).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    [JsonPropertyName("PageNumber")]
    public int PageNumber { get; init; }


    /// <summary>
    /// The number of items per page.
    /// </summary>
    [Range(1, SettingsConstants.MaxPageSize, ErrorMessage = "Page size must be between 1 and 100.")]
    [JsonPropertyName("PageSize")]
    public int PageSize { get; init; }

    /// <summary>
    /// Initializes a new instance with default values (Page 1, Size 10).
    /// </summary>
    public PaginationOptions() : this(1, SettingsConstants.DefaultPageSize) { }

    /// <summary>
    /// Initializes a new instance of PaginationOptions.
    /// </summary>
    /// <param name="pageNumber">The requested page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public PaginationOptions(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be at least 1.", nameof(pageNumber));
        if (pageSize < 1 || pageSize > SettingsConstants.MaxPageSize)
            throw new ArgumentException($"Page size must be between 1 and {SettingsConstants.MaxPageSize}.", nameof(pageSize));

        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
