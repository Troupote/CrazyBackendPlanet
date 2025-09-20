using PlaygroundDatabase.Interfaces;
using PlaygroundDatabase.Models.Business;
using PlaygroundDatabase.Models.Turso;

namespace PlaygroundDatabase.Services;

/// <summary>
/// Service for managing card exchanges
/// </summary>
public class ExchangeService : IExchangeService
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogService _logService;

    public ExchangeService(IDatabaseService databaseService, ILogService logService)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    public async Task<bool> CreateExchangeTableAsync()
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS ExchangeTable (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                requestOpener TEXT NOT NULL,
                requestFollower TEXT NOT NULL,
                openerCard TEXT NOT NULL,
                followerCard TEXT NOT NULL,
                date TEXT NOT NULL
            )";

        var result = await _databaseService.ExecuteSqlAsync(sql);
        var success = result?.Results?.Any() == true;

        if (success)
        {
            _logService.LogInfo("Table 'ExchangeTable' created or already exists.");
        }
        else
        {
            _logService.LogError("Failed to create table 'ExchangeTable'.");
        }

        return success;
    }

    public async Task<bool> InsertExchangeAsync(Exchange exchange)
    {
        var sql = $@"
            INSERT INTO ExchangeTable (requestOpener, requestFollower, openerCard, followerCard, date) 
            VALUES ('{EscapeSqlString(exchange.RequestOpener)}', 
                    '{EscapeSqlString(exchange.RequestFollower)}', 
                    '{EscapeSqlString(exchange.OpenerCard)}', 
                    '{EscapeSqlString(exchange.FollowerCard)}', 
                    '{exchange.Date:yyyy-MM-dd HH:mm:ss}')";

        var result = await _databaseService.ExecuteSqlAsync(sql);
        var success = result?.Results?.Any() == true;

        if (success)
        {
            _logService.LogInfo("Exchange inserted successfully.");
        }
        else
        {
            _logService.LogError("Failed to insert exchange.");
        }

        return success;
    }

    public async Task<List<Exchange>> GetAllExchangesAsync()
    {
        const string sql = "SELECT id, requestOpener, requestFollower, openerCard, followerCard, date FROM ExchangeTable ORDER BY date DESC";

        var result = await _databaseService.ExecuteSqlAsync(sql);
        var exchanges = new List<Exchange>();

        if (result?.Results?.Any() == true &&
            result.Results[0]?.Response?.Result?.Rows?.Any() == true)
        {
            var rows = result.Results[0].Response.Result.Rows;

            foreach (var row in rows)
            {
                try
                {
                    if (row != null)
                    {
                        var exchange = MapRowToExchange(row);
                        if (exchange != null)
                        {
                            exchanges.Add(exchange);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogError($"Error converting row to Exchange: {ex.Message}");
                }
            }

            _logService.LogInfo($"{exchanges.Count} exchange(s) retrieved.");
        }
        else
        {
            _logService.LogInfo("No exchanges found in the database.");
        }

        return exchanges;
    }

    public async Task<Exchange?> GetExchangeByIdAsync(int id)
    {
        var sql = $"SELECT id, requestOpener, requestFollower, openerCard, followerCard, date FROM ExchangeTable WHERE id = {id}";

        var result = await _databaseService.ExecuteSqlAsync(sql);

        if (result?.Results?.Any() == true &&
            result.Results[0]?.Response?.Result?.Rows?.Any() == true)
        {
            var row = result.Results[0].Response.Result.Rows[0];
            if (row != null)
            {
                return MapRowToExchange(row);
            }
        }

        return null;
    }

    /// <summary>
    /// Maps a SQL result row to an Exchange object
    /// </summary>
    private Exchange? MapRowToExchange(TursoValue[] row)
    {
        if (row.Length < 6) return null;

        return new Exchange
        {
            Id = int.TryParse(row[0]?.Value, out var id) ? id : 0,
            RequestOpener = row[1]?.Value ?? string.Empty,
            RequestFollower = row[2]?.Value ?? string.Empty,
            OpenerCard = row[3]?.Value ?? string.Empty,
            FollowerCard = row[4]?.Value ?? string.Empty,
            Date = DateTime.TryParse(row[5]?.Value, out var date) ? date : DateTime.MinValue
        };
    }

    /// <summary>
    /// Escapes strings to prevent SQL injection
    /// </summary>
    private string EscapeSqlString(string input)
    {
        return input.Replace("'", "''");
    }
}
