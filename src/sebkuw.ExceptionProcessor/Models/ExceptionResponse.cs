using System.Text.Json.Serialization;

namespace sebkuw.ExceptionProcessor.Models;

/// <summary>
/// Contains the client-facing error fields serialized by the exception middleware.
/// </summary>
public class ExceptionResponse
{
    /// <summary>Gets the stable machine-readable error code.</summary>
    [JsonPropertyName("code")]
    public string Code { get; init; }

    /// <summary>Gets the HTTP status code selected by the exception manager.</summary>
    [JsonPropertyName("httpCode")]
    public int HttpCode { get; init; }

    /// <summary>Gets the short client-facing error title.</summary>
    [JsonPropertyName("mainText")]
    public string MainText { get; init; }

    /// <summary>Gets the client-facing description. Custom exception authors must supply safe display text.</summary>
    [JsonPropertyName("description")]
    public string Description { get; init; }

    /// <summary>Gets the UTC timestamp, initialized from <see cref="DateTime.UtcNow"/> during construction.</summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    /// <summary>Gets the optional correlation identifier supplied by the caller.</summary>
    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }

    /// <summary>Initializes the response fields and the current UTC timestamp.</summary>
    /// <param name="code">The stable machine-readable error code.</param>
    /// <param name="httpCode">The HTTP status code.</param>
    /// <param name="mainText">The safe client-facing title.</param>
    /// <param name="description">The safe client-facing description.</param>
    /// <param name="traceId">The optional correlation identifier.</param>
    public ExceptionResponse(
        string code,
        int httpCode,
        string mainText,
        string description,
        string? traceId = null)
    {
        Code = code;
        HttpCode = httpCode;
        MainText = mainText;
        Description = description;
        Timestamp = DateTime.UtcNow;
        TraceId = traceId;
    }
}
