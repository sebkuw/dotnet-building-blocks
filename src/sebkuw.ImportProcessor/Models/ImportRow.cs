namespace sebkuw.ImportProcessor;

/// <summary>Represents one parsed row and its mapping outcome.</summary>
public sealed record ImportRow<T>(
    long RowNumber,
    T? Value,
    IReadOnlyDictionary<string, string> SourceValues,
    IReadOnlyList<ImportIssue> Issues,
    string? IdempotencyKey)
{
    /// <summary>Gets whether the row can be written.</summary>
    public bool IsValid => Value is not null && Issues.All(issue => issue.Severity != ImportIssueSeverity.Error);
}
