using Microsoft.Extensions.Diagnostics.HealthChecks;
using TursoConnector.Interfaces;

namespace TursoConnector.Services;

/// <summary>
/// Health check for database connectivity
/// Essential for production monitoring and load balancer health checks
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogService _logService;

    public DatabaseHealthCheck(IDatabaseService databaseService, ILogService logService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _databaseService.TestConnectionAsync();

            if (isHealthy)
            {
                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            else
            {
                _logService.LogError("Database health check failed - connection test returned false");
                return HealthCheckResult.Unhealthy("Database connection failed");
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Database health check failed with exception: {ex.Message}");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
