namespace sebkuw.ImportProcessor;

/// <summary>Writes valid mapped values using application-owned persistence semantics.</summary>
public interface IImportWriter<in T>
{
    /// <summary>Writes valid values using application-defined semantics.</summary>
    ValueTask WriteAsync(IReadOnlyList<T> values, CancellationToken cancellationToken);
}
