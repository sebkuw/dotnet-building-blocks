using System.Globalization;
using System.Text;

namespace sebkuw.ExportProcessor;

/// <summary>Controls CSV formatting, row scope, and selected columns.</summary>
public sealed class CsvExportOptions
{
    /// <summary>Gets the names of mapped columns to export, in output order. A null value selects every mapped column.</summary>
    public IReadOnlyList<string>? Columns { get; init; }

    /// <summary>Gets whether all matching rows or only the requested page is exported.</summary>
    public CsvExportScope Scope { get; init; } = CsvExportScope.AllMatching;

    /// <summary>Gets the character separating fields.</summary>
    public char Delimiter { get; init; } = ',';

    /// <summary>Gets whether the first record contains mapped column headers.</summary>
    public bool IncludeHeader { get; init; } = true;

    /// <summary>Gets the culture used by default value formatting.</summary>
    public CultureInfo Culture { get; init; } = CultureInfo.InvariantCulture;

    /// <summary>Gets the output encoding. UTF-8 with a byte-order mark is used by default for spreadsheet compatibility.</summary>
    public Encoding Encoding { get; init; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);

    /// <summary>Gets whether formula-like cell values are prefixed with an apostrophe.</summary>
    public bool ProtectFormulaCells { get; init; } = true;

    /// <summary>Gets an optional upper bound on exported rows after request criteria are applied.</summary>
    public int? MaxRows { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Culture);
        ArgumentNullException.ThrowIfNull(Encoding);

        if (Delimiter is '\r' or '\n' or '"' or '\0')
            throw new ArgumentOutOfRangeException(nameof(Delimiter), "Delimiter cannot be a quote, line break, or null character.");

        if (!Enum.IsDefined(Scope))
            throw new ArgumentOutOfRangeException(nameof(Scope), "The export scope is not supported.");

        if (MaxRows is <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxRows), "Maximum rows must be greater than zero.");
    }
}
