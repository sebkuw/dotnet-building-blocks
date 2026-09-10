using System.Globalization;
using System.Text;

namespace sebkuw.ImportProcessor;

/// <summary>Controls CSV parsing and import execution.</summary>
public sealed class ImportOptions
{
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
        if (Delimiter is '\r' or '\n' or '"' or '\0')
        {
            throw new ArgumentOutOfRangeException(nameof(Delimiter), "Delimiter cannot be a quote, line break, or null character.");
        }
    }
}
