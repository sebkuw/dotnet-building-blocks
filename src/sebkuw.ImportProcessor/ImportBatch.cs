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

/// <summary>Contains the complete result of a CSV import operation.</summary>
public sealed record ImportBatch<T>(
    Guid Id,
    bool IsPreview,
    IReadOnlyList<ImportRow<T>> Rows,
    IReadOnlyList<ImportIssue> Issues)
{
    /// <summary>Gets the number of rows without error-level issues.</summary>
    public int ValidRowCount => Rows.Count(row => row.IsValid);

    /// <summary>Gets the number of rows containing an error-level issue.</summary>
    public int InvalidRowCount => Rows.Count - ValidRowCount;

    /// <summary>Gets whether the batch or any row contains an error.</summary>
    public bool HasErrors => Issues.Any(issue => issue.Severity == ImportIssueSeverity.Error) || InvalidRowCount > 0;
}
