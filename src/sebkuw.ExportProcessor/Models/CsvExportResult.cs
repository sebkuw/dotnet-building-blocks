namespace sebkuw.ExportProcessor;

/// <summary>Describes a completed CSV export.</summary>
/// <param name="RecordCount">The number of exported data records.</param>
/// <param name="Columns">The mapped column names in output order.</param>
public sealed record CsvExportResult(long RecordCount, IReadOnlyList<string> Columns);
