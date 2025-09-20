using TursoConnector.Services;
using TursoConnector.Interfaces;
using TursoConnector.Models.Business;

namespace TursoConnector.Services;

/// <summary>
/// Main application service that orchestrates NATS messaging and database operations
/// </summary>
public class ApplicationService
{
    private readonly IDatabaseService _databaseService;
    private readonly IExchangeService _exchangeService;
    private readonly ILogService _logService;
    private readonly INatsService _natsService;
    private readonly ConfigurationService _configurationService;

    public ApplicationService(
        IDatabaseService databaseService,
        IExchangeService exchangeService,
        ILogService logService,
        INatsService natsService,
        ConfigurationService configurationService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _natsService = natsService ?? throw new ArgumentNullException(nameof(natsService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Starts the NATS messaging service
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            // Configure log level
            var logLevel = _configurationService.GetLogLevel();
            _logService.SetLogLevel(logLevel);

            // Test database connection
            var tursoConfig = _configurationService.GetTursoConfiguration();
            var isConnected = await _databaseService.TestConnectionAsync();

            if (!isConnected)
            {
                _logService.LogError("Cannot start NATS service without database connection");
                return;
            }

            _logService.LogWork($"Database connected: {tursoConfig.DatabaseUrl}");

            // Check NATS connection
            if (!_natsService.IsConnected)
            {
                _logService.LogError("NATS connection not available");
                return;
            }

            _logService.LogWork("Starting NATS messaging service for game communication...");

            // Start listening for game messages (this will run indefinitely)
            using var cts = new CancellationTokenSource();

            // Handle shutdown gracefully
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                _logService.LogWork("Shutdown requested...");
                cts.Cancel();
            };

            await _natsService.StartListeningAsync(cts.Token);


        }
        catch (OperationCanceledException)
        {
            _logService.LogWork("Service shutdown requested");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error during NATS service execution: {ex.Message}");
        }
        finally
        {
            await _natsService.StopAsync();
            _logService.LogWork("NATS messaging service stopped");
        }
    }

    /// <summary>
    /// Initializes the database (creates tables) - utility method
    /// </summary>
    public async Task<bool> InitializeDatabaseAsync()
    {
        _logService.LogWork("Initializing database...");

        var success = await _exchangeService.CreateExchangeTableAsync();
        if (!success)
        {
            _logService.LogError("Database initialization failed");
            return false;
        }

        _logService.LogWork("Database initialized successfully");
        return true;
    }

    /// <summary>
    /// Retrieves an exchange by its ID (utility method)
    /// </summary>
    public async Task<Exchange?> GetExchangeByIdAsync(int id)
    {
        return await _exchangeService.GetExchangeByIdAsync(id);
    }

    /// <summary>
    /// Adds a new exchange (utility method)
    /// </summary>
    public async Task<bool> AddExchangeAsync(string opener, string follower, string openerCard, string followerCard)
    {
        _logService.LogWork($"Adding new exchange: {opener} <-> {follower}");

        var exchange = new Exchange
        {
            RequestOpener = opener,
            RequestFollower = follower,
            OpenerCard = openerCard,
            FollowerCard = followerCard,
            Date = DateTime.UtcNow
        };

        return await _exchangeService.InsertExchangeAsync(exchange);
    }
}
