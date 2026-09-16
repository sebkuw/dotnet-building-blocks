using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using sebkuw.QueryableProcessor.Extensions;
using sebkuw.QueryableProcessor.Models;

namespace sebkuw.ExportProcessor;

/// <summary>Exports queryable data to CSV after applying a query request.</summary>
public static class QueryableCsvExportExtensions
{
    /// <summary>
    /// Applies request filters and sorting, optionally applies its pagination, projects rows, and streams CSV output.
    /// The destination stream remains open.
    /// </summary>
    /// <typeparam name="TSource">The queried entity type.</typeparam>
    /// <typeparam name="TExport">The projected export row type.</typeparam>
    /// <param name="source">The EF Core query.</param>
    /// <param name="destination">The writable destination stream.</param>
    /// <param name="request">The query request whose criteria are applied.</param>
    /// <param name="selector">An expression projected by the query provider before rows are streamed.</param>
    /// <param name="map">The allowlisted CSV columns.</param>
    /// <param name="options">Optional export behavior and selected columns.</param>
    /// <param name="cancellationToken">Cancels query enumeration and output writes.</param>
    /// <returns>Metadata describing the completed export.</returns>
    public static async Task<CsvExportResult> ExportCsvAsync<TSource, TExport>(
        this IQueryable<TSource> source,
        Stream destination,
        RequestDto request,
        Expression<Func<TSource, TExport>> selector,
        CsvExportMap<TExport> map,
        CsvExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(map);

        if (!destination.CanWrite)
            throw new ArgumentException("The destination stream must be writable.", nameof(destination));

        options ??= new CsvExportOptions();
        options.Validate();
        IReadOnlyList<CsvExportMap<TExport>.IExportColumn<TExport>> columns = map.Resolve(options.Columns);
        cancellationToken.ThrowIfCancellationRequested();

        IQueryable<TSource> query = source.ApplyRequest(
            request,
            includePagination: options.Scope == CsvExportScope.CurrentPage);
        if (options.MaxRows is int maxRows)
            query = query.Take(maxRows);

        IQueryable<TExport> projectedQuery = query.Select(selector);
        await using var writer = new StreamWriter(
            destination,
            options.Encoding,
            bufferSize: 1024,
            leaveOpen: true);

        long recordCount = 0;
        bool hasOutputRecord = false;
        if (options.IncludeHeader)
        {
            await WriteRecordAsync(
                writer,
                columns.Select(column => column.Header),
                options,
                cancellationToken).ConfigureAwait(false);
            hasOutputRecord = true;
        }

        await foreach (TExport row in projectedQuery
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false))
        {
            if (hasOutputRecord)
                await writer.WriteAsync("\r\n".AsMemory(), cancellationToken).ConfigureAwait(false);

            await WriteRecordAsync(
                writer,
                columns.Select(column => column.Format(row, options.Culture)),
                options,
                cancellationToken).ConfigureAwait(false);
            hasOutputRecord = true;
            recordCount++;
        }

        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        return new CsvExportResult(recordCount, columns.Select(column => column.Name).ToArray());
    }

    private static Task WriteRecordAsync(
        TextWriter writer,
        IEnumerable<string> values,
        CsvExportOptions options,
        CancellationToken cancellationToken)
    {
        string record = string.Join(
            options.Delimiter,
            values.Select(value => Escape(value, options.Delimiter, options.ProtectFormulaCells)));
        return writer.WriteAsync(record.AsMemory(), cancellationToken);
    }

    private static string Escape(string value, char delimiter, bool protectFormulaCells)
    {
        string escaped = protectFormulaCells && value.Length > 0 && value[0] is '=' or '+' or '-' or '@'
            ? $"'{value}"
            : value;

        return escaped.Contains(delimiter) ||
            escaped.Contains('"') ||
            escaped.Contains('\r') ||
            escaped.Contains('\n')
            ? $"\"{escaped.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
            : escaped;
    }
}
