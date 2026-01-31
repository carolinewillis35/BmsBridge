namespace BmsBridge.Configuration;

public record SerilogSettings
{
    public List<string> Using { get; init; } = new();
    public LogEventLevel MinimumLevel { get; init; } = LogEventLevel.Debug;
    public List<SerilogSink> WriteTo { get; init; } = new();
    public List<string> Enrich { get; init; } = new();
    public Dictionary<string, string> Properties { get; init; } = new();
}

public record SerilogSink
{
    public string Name { get; init; } = string.Empty;
    public SerilogSinkArgs? Args { get; init; }
}

public record SerilogSinkArgs
{
    public string Path { get; init; } = "logs/app.log";
    public RollingInterval RollingInterval { get; init; } = RollingInterval.Day;
    public int FileSizeLimitBytes { get; init; } = 1_000_000;
    public int RetainedFileCountLimit { get; init; } = 7;
    public string Formatter { get; init; } =
        "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact";
    public bool RollOnFileSizeLimit { get; init; } = false;
}

public enum SerilogSinkName
{
    Console,
    File
}

public enum LogEventLevel
{
    Verbose,
    Debug,
    Information,
    Warning,
    Error,
    Fatal
}

public enum RollingInterval
{
    Infinite,
    Year,
    Month,
    Day,
    Hour,
    Minute
}
