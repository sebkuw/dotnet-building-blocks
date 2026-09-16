using System.Globalization;
using System.Text;

namespace sebkuw.ImportProcessor;

/// <summary>Controls CSV parsing and import execution.</summary>
public sealed class ImportOptions
{
    /// <summary>Gets the maximum number of valid values sent to the writer in one call.</summary>
    public int BatchSize { get; init; } = int.MaxValue;

    /// <summary>Gets the maximum number of rows mapped and validated concurrently.</summary>
    public int MaxDegreeOfParallelism { get; init; } = 1;

    /// <summary>Gets the maximum number of parsed rows awaiting ordered completion.</summary>
    public int BufferCapacity { get; init; } = 256;

    /// <summary>Gets the maximum number of initial processed row reports retained in the returned batch.</summary>
    public int MaxRetainedRows { get; init; } = int.MaxValue;

    /// <summary>Gets the character separating fields.</summary>
    public char Delimiter { get; init; } = ',';

    /// <summary>Gets whether the first record contains column names.</summary>
    public bool HasHeader { get; init; } = true;

    /// <summary>Gets whether writing and idempotency recording are disabled.</summary>
    public bool Preview { get; init; }

    /// <summary>Gets the culture used by value converters.</summary>
    public CultureInfo Culture { get; init; } = CultureInfo.InvariantCulture;

    /// <summary>Gets the encoding used by the stream overload.</summary>
    public Encoding Encoding { get; init; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Culture);
        ArgumentNullException.ThrowIfNull(Encoding);
        if (BatchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(BatchSize), "Batch size must be greater than zero.");

        if (MaxDegreeOfParallelism <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxDegreeOfParallelism), "Maximum parallelism must be greater than zero.");

        if (BufferCapacity < MaxDegreeOfParallelism)
            throw new ArgumentOutOfRangeException(nameof(BufferCapacity), "Buffer capacity must be at least the maximum parallelism.");

        if (MaxRetainedRows < 0)
            throw new ArgumentOutOfRangeException(nameof(MaxRetainedRows), "Maximum retained rows cannot be negative.");

        if (Delimiter is '\r' or '\n' or '"' or '\0')
            throw new ArgumentOutOfRangeException(nameof(Delimiter), "Delimiter cannot be a quote, line break, or null character.");
    }
}
