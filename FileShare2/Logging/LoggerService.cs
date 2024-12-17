public interface ILoggerService
{
    void Trace(string message);
    void Debug(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string err);
    void Error(string message, string err);
    void Critical(string message);

    IDisposable? Scoped(string scopeName);
}

public class LoggerService : ILoggerService
{
    private readonly ILogger _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void Trace(string message) => _logger.LogTrace(message);
    public void Debug(string message) => _logger.LogDebug(message);
    public void Info(string message) => _logger.LogInformation(message);
    public void Warning(string message) => _logger.LogWarning(message);
    public void Error(string err) => _logger.LogError($"'{err}'");
    public void Error(string message, string err) => _logger.LogError($"{message}:\r\n'{err}'");
    public void Critical(string message) => _logger.LogCritical(message);

    public IDisposable? Scoped(string scopeName)
    {
        return _logger.BeginScope(scopeName);
    }
}