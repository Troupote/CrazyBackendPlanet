using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TursoConnector.Configuration;
using TursoConnector.Interfaces;
using TursoConnector.Services;

namespace TursoConnector.Factories;

/// <summary>
/// Advanced service factory with proper dependency injection container
/// Manages the complete application lifecycle and service configuration
/// </summary>
public class ServiceFactory : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHost _host;
    private bool _disposed = false;

    public ServiceFactory()
    {
        _host = CreateHost();
        _serviceProvider = _host.Services;
    }

    /// <summary>
    /// Creates and configures the application host with all services
    /// </summary>
    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core infrastructure
                services.AddHttpClient<IDatabaseService, DatabaseService>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

                // Logging and configuration
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<ConfigurationService>();
                services.AddSingleton<ResilienceService>();

                // Configuration binding
                services.AddSingleton(serviceProvider =>
                {
                    var configService = serviceProvider.GetRequiredService<ConfigurationService>();
                    return configService.GetTursoConfiguration();
                });

                // Business services with proper lifetimes
                services.AddScoped<IDatabaseService, DatabaseService>();
                services.AddScoped<IExchangeService, ExchangeService>();
                services.AddScoped<DisplayService>();
                services.AddScoped<ApplicationService>();

                // Health checks for production readiness
                services.AddHealthChecks()
                    .AddCheck<DatabaseHealthCheck>("database", timeout: TimeSpan.FromSeconds(10))
                    .AddCheck("memory", () =>
                    {
                        var memoryUsage = GC.GetTotalMemory(false);
                        var memoryInMB = memoryUsage / (1024 * 1024);

                        return memoryInMB > 500
                            ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Memory usage too high: {memoryInMB}MB")
                            : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Memory usage normal: {memoryInMB}MB");
                    });

                // Enhanced logging with structured logging
                services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    // Completely disable Microsoft logging to avoid system logs
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.None);
                });
            })
            .Build();
    }

    /// <summary>
    /// Creates the main application service with all dependencies resolved
    /// </summary>
    public ApplicationService CreateApplication()
    {
        try
        {
            return _serviceProvider.GetRequiredService<ApplicationService>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create application service. Check your configuration and dependencies.", ex);
        }
    }

    /// <summary>
    /// Gets a service by type - useful for testing and advanced scenarios
    /// </summary>
    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Starts the application host
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _host.StartAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the application host gracefully
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _host.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Gets health check results for monitoring
    /// </summary>
    public async Task<string> GetHealthStatusAsync()
    {
        var healthCheckService = _serviceProvider.GetRequiredService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync();

        return $"Status: {result.Status}, Duration: {result.TotalDuration.TotalMilliseconds}ms";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _host?.Dispose();
            _disposed = true;
        }
    }
}
