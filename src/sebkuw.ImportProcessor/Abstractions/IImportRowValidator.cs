namespace sebkuw.ImportProcessor;

/// <summary>Validates a mapped row without coupling the package to domain rules.</summary>
public interface IImportRowValidator<in T>
{
    /// <summary>Validates one mapped value.</summary>
    ValueTask<IReadOnlyList<ImportIssue>> ValidateAsync(T value, long rowNumber, CancellationToken cancellationToken);
}
