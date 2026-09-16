namespace sebkuw.ImportProcessor;

/// <summary>Checks and records application-defined idempotency keys.</summary>
public interface IImportIdempotencyStore
{
    /// <summary>Checks whether a key was processed previously.</summary>
    ValueTask<bool> ContainsAsync(string key, CancellationToken cancellationToken);

    /// <summary>Records a key after its value has been written.</summary>
    ValueTask MarkProcessedAsync(string key, CancellationToken cancellationToken);
}
