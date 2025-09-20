using Microsoft.Extensions.Configuration;
using TursoConnector.Configuration;
using TursoConnector.Enums;
using TursoConnector.Interfaces;

namespace TursoConnector.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public ConfigurationService(ILogService logService)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));

        // Configuration with enhanced error handling
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    /// <summary>
    /// Retrieves Turso configuration
    /// </summary>
    public TursoConfiguration GetTursoConfiguration()
    {
        var config = new TursoConfiguration
        {
            DatabaseUrl = _configuration["Tibo:DatabaseUrl"] ?? string.Empty,
            AuthToken = _configuration["Tibo:AuthToken"] ?? string.Empty
        };

        if (!config.IsValid)
        {
            _logService.LogError("Incomplete Turso configuration in appsettings.json");
            _logService.LogError($"Current directory: {Directory.GetCurrentDirectory()}");
            _logService.LogError($"appsettings.json file exists: {File.Exists("appsettings.json")}");
        }

        return config;
    }

    /// <summary>
    /// Retrieves the configured log level
    /// </summary>
    public LogLevel GetLogLevel()
    {
        if (Enum.TryParse<LogLevel>(_configuration["Logging:Level"], out var configuredLogLevel))
        {
            return configuredLogLevel;
        }

        return LogLevel.Simple; // Default value
    }

    /// <summary>
    /// Validates that the configuration is correct
    /// </summary>
    public bool ValidateConfiguration()
    {
        var tursoConfig = GetTursoConfiguration();
        return tursoConfig.IsValid;
    }
}
