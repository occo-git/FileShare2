public class LoggingConfig
{
    public LogLevel MinimumLogLevel { get; set; }
    public required string LogFilePath { get; set; }
    public long? FileSizeLimitBytes { get; set; }
    public required string FileTemplate { get; set; }
    public required string ConsoleTemplate { get; set; }
    public required string AwsLogGroup { get; set; }
    public required string AwsStreamPrefix { get; set; }
}