using NetDevs.QueryableProcessor.Enums;

namespace NetDevs.QueryableProcessor.Models;

/// <summary>
/// Represents a filtering condition for queries.
/// </summary>
public class FilterCondition
{
    /// <summary>
    /// The property name or path to filter by.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("PropertyPath")]
    public required string PropertyPath { get; init; }

    /// <summary>
    /// The comparison operation to use.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("Operation")]
    public required FilterOperation Operation { get; init; }

    /// <summary>
    /// The value to compare against.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("Value")]
    public required object Value { get; init; }
}
