namespace sebkuw.ExceptionProcessor.Settings;

/// <summary>
/// Represents NLog configuration settings loaded from appsettings.json.
/// </summary>
public class NLogSettings
{
    /// <summary>
    /// Path to the log file.
    /// </summary>
    public string LogFilePath { get; set; } = "logs/app.log";
}
