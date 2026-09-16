using System.Globalization;

namespace sebkuw.ExportProcessor;

/// <summary>Defines the allowlisted columns and formatting used by a CSV export.</summary>
/// <typeparam name="T">The projected export row type.</typeparam>
public sealed class CsvExportMap<T>
{
    private readonly List<IExportColumn<T>> columns = [];

    /// <summary>Adds an exportable column.</summary>
    /// <typeparam name="TValue">The selected value type.</typeparam>
    /// <param name="name">The stable name used when clients select the column.</param>
    /// <param name="selector">Selects the value from a projected row.</param>
    /// <param name="header">The CSV header. Defaults to <paramref name="name"/>.</param>
    /// <param name="formatter">Optional culture-aware formatter.</param>
    /// <returns>The same map for fluent configuration.</returns>
    public CsvExportMap<T> Map<TValue>(
        string name,
        Func<T, TValue> selector,
        string? header = null,
        Func<TValue, CultureInfo, string?>? formatter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(selector);

        string effectiveHeader = header ?? name;
        ArgumentException.ThrowIfNullOrWhiteSpace(effectiveHeader);

        if (columns.Any(column => string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Column '{name}' is already mapped.", nameof(name));

        columns.Add(new ExportColumn<T, TValue>(name, effectiveHeader, selector, formatter));
        return this;
    }

    internal IReadOnlyList<IExportColumn<T>> Resolve(IReadOnlyList<string>? selectedColumns)
    {
        if (columns.Count == 0)
            throw new InvalidOperationException("At least one export column must be mapped.");

        if (selectedColumns is null)
            return columns;

        if (selectedColumns.Count == 0)
            throw new ArgumentException("At least one column must be selected.", nameof(selectedColumns));

        var selected = new List<IExportColumn<T>>(selectedColumns.Count);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string name in selectedColumns)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (!names.Add(name))
                throw new ArgumentException($"Column '{name}' was selected more than once.", nameof(selectedColumns));

            IExportColumn<T>? column = columns.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
            if (column is null)
                throw new ArgumentException($"Column '{name}' is not mapped for export.", nameof(selectedColumns));

            selected.Add(column);
        }

        return selected;
    }

    internal interface IExportColumn<in TRow>
    {
        string Name { get; }

        string Header { get; }

        string Format(TRow row, CultureInfo culture);
    }

    private sealed record ExportColumn<TRow, TValue>(
        string Name,
        string Header,
        Func<TRow, TValue> Selector,
        Func<TValue, CultureInfo, string?>? Formatter) : IExportColumn<TRow>
    {
        public string Format(TRow row, CultureInfo culture)
        {
            TValue value = Selector(row);
            if (Formatter is not null)
                return Formatter(value, culture) ?? string.Empty;

            return value switch
            {
                null => string.Empty,
                IFormattable formattable => formattable.ToString(format: null, culture) ?? string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
        }
    }
}
