namespace NetDevs.QueryableProcessor.Enums;

/// <summary>
/// Defines supported filter operations.
/// </summary>
public enum FilterOperation
{
    /// <summary>Equality comparison (==)</summary>
    Equal = 0,
    /// <summary>Inequality comparison (!=)</summary>
    NotEqual = 1,
    /// <summary>Greater than comparison (&gt;)</summary>
    GreaterThan = 2,
    /// <summary>Less than comparison (&lt;)</summary>
    LessThan = 3,
    /// <summary>Greater than or equal comparison (&gt;=)</summary>
    GreaterThanOrEqual = 4,
    /// <summary>Less than or equal comparison (&lt;=)</summary>
    LessThanOrEqual = 5,
    /// <summary>String contains check</summary>
    Contains = 6,
    /// <summary>String starts with check</summary>
    StartsWith = 7,
    /// <summary>String ends with check</summary>
    EndsWith = 8,
    /// <summary>Value is in collection</summary>
    In = 9,
    /// <summary>Value is not in collection</summary>
    NotIn = 10
}