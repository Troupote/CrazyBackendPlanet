using TursoConnector.Enums;

namespace TursoConnector.Interfaces;

/// <summary>
/// Interface for logging service
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Configures the log level
    /// </summary>
    /// <param name="logLevel">The log level to set</param>
    void SetLogLevel(LogLevel logLevel);

    /// <summary>
    /// Logs an information message
    /// </summary>
    /// <param name="message">The message to log</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs a debug message (displayed only if LogLevel.Complete)
    /// </summary>
    /// <param name="message">The debug message to log</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs a work logic message (displayed only if LogLevel.Work or LogLevel.Complete)
    /// </summary>
    /// <param name="message">The work logic message to log</param>
    void LogWork(string message);

    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">The error message to log</param>
    void LogError(string message);
}
