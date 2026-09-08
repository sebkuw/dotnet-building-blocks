namespace sebkuw.QueryableProcessor.Constants;

/// <summary>
/// Defines the default pagination limits and legacy sorting tokens.
/// </summary>
public static class SettingsConstants
{
    /// <summary>The default number of items in a page.</summary>
    public const int DefaultPageSize = 10;

    /// <summary>The maximum allowed number of items in a page.</summary>
    public const int MaxPageSize = 100;

    /// <summary>The legacy ascending sort prefix.</summary>
    public const string AscendingPrefix = "asc_";

    /// <summary>The legacy descending sort prefix.</summary>
    public const string DescendingPrefix = "desc_";

    /// <summary>The LINQ ascending order method name.</summary>
    public const string OrderByMethod = "OrderBy";

    /// <summary>The LINQ descending order method name.</summary>
    public const string OrderByDescendingMethod = "OrderByDescending";
}
