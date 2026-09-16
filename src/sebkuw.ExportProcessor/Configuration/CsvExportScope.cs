namespace sebkuw.ExportProcessor;

/// <summary>Defines which rows from a query request are exported.</summary>
public enum CsvExportScope
{
    /// <summary>Exports every row matching the request filters and sorting, ignoring pagination.</summary>
    AllMatching = 0,

    /// <summary>Exports only the page selected by the request pagination options.</summary>
    CurrentPage = 1,
}
