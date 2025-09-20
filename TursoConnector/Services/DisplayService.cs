using TursoConnector.Interfaces;
using TursoConnector.Models.Business;

namespace TursoConnector.Services;

/// <summary>
/// Service for displaying data in the console
/// </summary>
public class DisplayService
{
    private readonly ILogService _logService;

    public DisplayService(ILogService logService)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    /// <summary>
    /// Displays the list of exchanges in a formatted way
    /// </summary>
    public void DisplayExchanges(List<Exchange> exchanges)
    {
        if (!exchanges.Any())
        {
            _logService.LogWork("No exchange data found");
            return;
        }

        _logService.LogWork($"Found {exchanges.Count} exchange(s) in database:");

        foreach (var exchange in exchanges)
        {
            _logService.LogWork($"  → {exchange}");
        }
    }

    /// <summary>
    /// Displays a specific exchange
    /// </summary>
    public void DisplayExchange(Exchange exchange)
    {
        _logService.LogWork($"Exchange details: {exchange}");
    }

    /// <summary>
    /// Displays a welcome message
    /// </summary>
    public void DisplayWelcomeMessage()
    {
        // Removed welcome message for Work mode - only essential logs
    }

    /// <summary>
    /// Displays connection information
    /// </summary>
    public void DisplayConnectionInfo(bool isConnected, string databaseUrl)
    {
        if (isConnected)
        {
            _logService.LogWork($"Database connected: {MaskSensitiveUrl(databaseUrl)}");
        }
        else
        {
            _logService.LogError("Failed to connect to database");
        }
    }

    /// <summary>
    /// Masks sensitive parts of the URL for display
    /// </summary>
    private string MaskSensitiveUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "Not defined";

        // Keep only the beginning and end of the URL
        var parts = url.Split('.');
        if (parts.Length > 2)
        {
            return $"{parts[0]}.*****.{parts[^1]}";
        }
        return url;
    }
}
