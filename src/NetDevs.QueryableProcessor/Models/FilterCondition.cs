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
    public required string PropertyPath { get; init; }

    /// <summary>
    /// The comparison operation to use.
    /// </summary>
    public required FilterOperation Operation { get; init; }

    /// <summary>
    /// The value to compare against.
    /// </summary>
    public required object Value { get; init; }
}
