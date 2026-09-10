namespace sebkuw.ImportProcessor;

/// <summary>Validates a mapped row without coupling the package to domain rules.</summary>
public interface IImportRowValidator<in T>
{
    /// <summary>Validates one mapped value.</summary>
    ValueTask<IReadOnlyList<ImportIssue>> ValidateAsync(T value, long rowNumber, CancellationToken cancellationToken);
}

/// <summary>Writes valid mapped values using application-owned persistence semantics.</summary>
public interface IImportWriter<in T>
{
    /// <summary>Writes valid values using application-defined semantics.</summary>
    ValueTask WriteAsync(IReadOnlyList<T> values, CancellationToken cancellationToken);
}

/// <summary>Checks and records application-defined idempotency keys.</summary>
public interface IImportIdempotencyStore
{
    /// <summary>Checks whether a key was processed previously.</summary>
    ValueTask<bool> ContainsAsync(string key, CancellationToken cancellationToken);

    /// <summary>Records a key after its value has been written.</summary>
    ValueTask MarkProcessedAsync(string key, CancellationToken cancellationToken);
}
