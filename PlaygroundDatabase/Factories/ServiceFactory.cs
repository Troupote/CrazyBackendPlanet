using PlaygroundDatabase.Interfaces;
using PlaygroundDatabase.Services;

namespace PlaygroundDatabase.Factories;

/// <summary>
/// Factory to create and configure all application services
/// </summary>
public class ServiceFactory : IDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public ServiceFactory()
    {
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Creates a complete application instance with all configured services
    /// </summary>
    public ApplicationService CreateApplication()
    {
        // Create services in dependency order
        var logService = CreateLogService();
        var configurationService = CreateConfigurationService(logService);
        var databaseService = CreateDatabaseService(configurationService, logService);
        var exchangeService = CreateExchangeService(databaseService, logService);
        var displayService = CreateDisplayService(logService);

        return new ApplicationService(
            databaseService,
            exchangeService,
            logService,
            displayService,
            configurationService);
    }

    /// <summary>
    /// Creates the logging service
    /// </summary>
    public ILogService CreateLogService()
    {
        return new LogService();
    }

    /// <summary>
    /// Creates the configuration service
    /// </summary>
    public ConfigurationService CreateConfigurationService(ILogService logService)
    {
        return new ConfigurationService(logService);
    }

    /// <summary>
    /// Creates the database service
    /// </summary>
    public IDatabaseService CreateDatabaseService(ConfigurationService configurationService, ILogService logService)
    {
        var tursoConfig = configurationService.GetTursoConfiguration();
        return new DatabaseService(_httpClient, tursoConfig, logService);
    }

    /// <summary>
    /// Creates the exchange management service
    /// </summary>
    public IExchangeService CreateExchangeService(IDatabaseService databaseService, ILogService logService)
    {
        return new ExchangeService(databaseService, logService);
    }

    /// <summary>
    /// Creates the display service
    /// </summary>
    public DisplayService CreateDisplayService(ILogService logService)
    {
        return new DisplayService(logService);
    }

    /// <summary>
    /// Releases resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
