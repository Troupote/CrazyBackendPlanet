using TursoConnector.Enums;
using TursoConnector.Interfaces;

namespace TursoConnector.Services;

/// <summary>
/// Logging service for the application
/// </summary>
public class LogService : ILogService
{
    private LogLevel _currentLogLevel = LogLevel.Simple;

    public void SetLogLevel(LogLevel logLevel)
    {
        _currentLogLevel = logLevel;
        // Only show log level configuration in Complete mode
        if (_currentLogLevel == LogLevel.Complete)
        {
            Console.WriteLine($"[CONFIG] Log level configured to: {_currentLogLevel}");
        }
    }

    public void LogInfo(string message)
    {
        // LogInfo is displayed for Simple and Complete levels, but not for Work level
        if (_currentLogLevel != LogLevel.Work)
        {
            Console.WriteLine($"[INFO] {message}");
        }
    }

    public void LogDebug(string message)
    {
        if (_currentLogLevel == LogLevel.Complete)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }
    }

    public void LogWork(string message)
    {
        // LogWork is displayed for Work and Complete levels
        if (_currentLogLevel == LogLevel.Work || _currentLogLevel == LogLevel.Complete)
        {
            Console.WriteLine($"[WORK] {message}");
        }
    }

    public void LogError(string message)
    {
        // Errors are always displayed regardless of log level
        Console.WriteLine($"[ERROR] {message}");
    }
}
