using PlaygroundDatabase.Enums;
using PlaygroundDatabase.Interfaces;

namespace PlaygroundDatabase.Services;

/// <summary>
/// Logging service for the application
/// </summary>
public class LogService : ILogService
{
    private LogLevel _currentLogLevel = LogLevel.Simple;

    public void SetLogLevel(LogLevel logLevel)
    {
        _currentLogLevel = logLevel;
        Console.WriteLine($"📋 Log level configured to: {_currentLogLevel}");
    }

    public void LogInfo(string message)
    {
        Console.WriteLine($"ℹ️  {message}");
    }

    public void LogDebug(string message)
    {
        if (_currentLogLevel == LogLevel.Complete)
        {
            Console.WriteLine($"🔍 DEBUG: {message}");
        }
    }

    public void LogError(string message)
    {
        Console.WriteLine($"❌ ERROR: {message}");
    }
}
