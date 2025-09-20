using PlaygroundDatabase.Interfaces;
using PlaygroundDatabase.Models.Business;
using PlaygroundDatabase.Services;

namespace PlaygroundDatabase.Services;

/// <summary>
/// Main application service that orchestrates all other services
/// </summary>
public class ApplicationService
{
    private readonly IDatabaseService _databaseService;
    private readonly IExchangeService _exchangeService;
    private readonly ILogService _logService;
    private readonly DisplayService _displayService;
    private readonly ConfigurationService _configurationService;

    public ApplicationService(
        IDatabaseService databaseService,
        IExchangeService exchangeService,
        ILogService logService,
        DisplayService displayService,
        ConfigurationService configurationService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Starts the application
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _displayService.DisplayWelcomeMessage();

            // Configure log level
            var logLevel = _configurationService.GetLogLevel();
            _logService.SetLogLevel(logLevel);

            // Test connection
            var tursoConfig = _configurationService.GetTursoConfiguration();
            var isConnected = await _databaseService.TestConnectionAsync();
            _displayService.DisplayConnectionInfo(isConnected, tursoConfig.DatabaseUrl);

            if (!isConnected)
            {
                _logService.LogError("Cannot continue without database connection.");
                return;
            }

            // Initialize database
            await InitializeDatabaseAsync();

            // Insert test data (optional)
            await InsertTestDataAsync();

            // Display data
            await DisplayAllExchangesAsync();
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error during application execution: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the database (creates tables)
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        _logService.LogInfo("🔧 Initializing database...");

        var success = await _exchangeService.CreateExchangeTableAsync();
        if (!success)
        {
            throw new InvalidOperationException("Database initialization failed");
        }
    }

    /// <summary>
    /// Inserts test data
    /// </summary>
    private async Task InsertTestDataAsync()
    {
        _logService.LogInfo("📝 Inserting test data...");

        var testExchange = new Exchange
        {
            RequestOpener = "Crazy caca Niels",
            RequestFollower = "crazy caca tibo",
            OpenerCard = "krazy player1",
            FollowerCard = "krazy player2",
            Date = DateTime.Now
        };

        await _exchangeService.InsertExchangeAsync(testExchange);
    }

    /// <summary>
    /// Displays all exchanges
    /// </summary>
    private async Task DisplayAllExchangesAsync()
    {
        _logService.LogInfo("📊 Retrieving data...");

        var exchanges = await _exchangeService.GetAllExchangesAsync();
        _displayService.DisplayExchanges(exchanges);
    }

    /// <summary>
    /// Retrieves an exchange by its ID (utility method)
    /// </summary>
    public async Task<Exchange?> GetExchangeByIdAsync(int id)
    {
        return await _exchangeService.GetExchangeByIdAsync(id);
    }

    /// <summary>
    /// Adds a new exchange
    /// </summary>
    public async Task<bool> AddExchangeAsync(string opener, string follower, string openerCard, string followerCard)
    {
        var exchange = new Exchange
        {
            RequestOpener = opener,
            RequestFollower = follower,
            OpenerCard = openerCard,
            FollowerCard = followerCard,
            Date = DateTime.Now
        };

        return await _exchangeService.InsertExchangeAsync(exchange);
    }
}
