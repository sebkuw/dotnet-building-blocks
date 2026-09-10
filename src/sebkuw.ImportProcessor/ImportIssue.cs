namespace sebkuw.ImportProcessor;

/// <summary>Describes a problem found while importing a CSV document.</summary>
public sealed record ImportIssue(
    string Code,
    string Message,
    ImportIssueSeverity Severity = ImportIssueSeverity.Error,
    string? Column = null);

/// <summary>Defines the severity of an import issue.</summary>
public enum ImportIssueSeverity
{
    /// <summary>The row may still be written.</summary>
    Warning,
    /// <summary>The row must not be written.</summary>
    Error,
}
