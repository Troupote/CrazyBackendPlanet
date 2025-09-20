namespace TursoConnector.Configuration;

/// <summary>
/// Advanced application configuration with environment-specific settings
/// Supports development, staging, and production environments
/// </summary>
public class ApplicationConfiguration
{
    public DatabaseConfiguration Database { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    public ResilienceConfiguration Resilience { get; set; } = new();
    public CacheConfiguration Cache { get; set; } = new();
    public HealthCheckConfiguration HealthChecks { get; set; } = new();
    public string Environment { get; set; } = "Development";
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Validates the complete application configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Database.ConnectionString))
            throw new InvalidOperationException("Database connection string is required");

        if (Resilience.MaxRetryAttempts < 1 || Resilience.MaxRetryAttempts > 10)
            throw new InvalidOperationException("MaxRetryAttempts must be between 1 and 10");

        if (Cache.MaxCacheSize < 10 || Cache.MaxCacheSize > 10000)
            throw new InvalidOperationException("MaxCacheSize must be between 10 and 10000");
    }
}

/// <summary>
/// Database-specific configuration
/// </summary>
public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public int ConnectionPoolSize { get; set; } = 10;
    public int CommandTimeoutSeconds { get; set; } = 30;
    public bool EnableQueryLogging { get; set; } = false;
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfiguration
{
    public string MinimumLevel { get; set; } = "Information";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = false;
    public string LogFilePath { get; set; } = "logs/application.log";
    public bool EnableStructuredLogging { get; set; } = true;
}

/// <summary>
/// Resilience and retry configuration
/// </summary>
public class ResilienceConfiguration
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 1;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerTimeoutSeconds { get; set; } = 30;
    public bool EnableCircuitBreaker { get; set; } = true;
}

/// <summary>
/// Cache configuration
/// </summary>
public class CacheConfiguration
{
    public bool EnableCaching { get; set; } = true;
    public int MaxCacheSize { get; set; } = 100;
    public int CacheExpirationMinutes { get; set; } = 30;
    public bool CacheReadQueries { get; set; } = true;
}

/// <summary>
/// Health check configuration
/// </summary>
public class HealthCheckConfiguration
{
    public int TimeoutSeconds { get; set; } = 10;
    public int IntervalSeconds { get; set; } = 30;
    public bool EnableDatabaseHealthCheck { get; set; } = true;
    public bool EnableMemoryHealthCheck { get; set; } = true;
    public int MemoryThresholdMB { get; set; } = 500;
}
