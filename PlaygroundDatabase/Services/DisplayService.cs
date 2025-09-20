using PlaygroundDatabase.Interfaces;
using PlaygroundDatabase.Models.Business;

namespace PlaygroundDatabase.Services;

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
            _logService.LogInfo("No data found in ExchangeTable.");
            return;
        }

        Console.WriteLine("\n📋 Data in ExchangeTable:");
        Console.WriteLine(new string('=', 80));

        foreach (var exchange in exchanges)
        {
            Console.WriteLine(exchange.ToString());
        }

        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"Total: {exchanges.Count} exchange(s)");
    }

    /// <summary>
    /// Displays a specific exchange
    /// </summary>
    public void DisplayExchange(Exchange exchange)
    {
        Console.WriteLine("\n📄 Exchange details:");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine(exchange.ToString());
        Console.WriteLine(new string('-', 50));
    }

    /// <summary>
    /// Displays a welcome message
    /// </summary>
    public void DisplayWelcomeMessage()
    {
        Console.WriteLine("🚀 Krazy Planet Survivor - Exchange Manager");
        Console.WriteLine(new string('=', 50));
    }

    /// <summary>
    /// Displays connection information
    /// </summary>
    public void DisplayConnectionInfo(bool isConnected, string databaseUrl)
    {
        if (isConnected)
        {
            _logService.LogInfo($"✅ Connection to Turso established successfully!");
            _logService.LogInfo($"🔗 Database: {MaskSensitiveUrl(databaseUrl)}");
        }
        else
        {
            _logService.LogError("❌ Failed to connect to Turso");
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
