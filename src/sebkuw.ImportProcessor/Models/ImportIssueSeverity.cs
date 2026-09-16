namespace sebkuw.ImportProcessor;

/// <summary>Defines the severity of an import issue.</summary>
public enum ImportIssueSeverity
{
    /// <summary>The row may still be written.</summary>
    Warning,

    /// <summary>The row must not be written.</summary>
    Error,
}
