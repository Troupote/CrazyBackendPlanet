using System.Text;
using System.Text.Json;
using TursoConnector.Configuration;
using TursoConnector.Interfaces;
using TursoConnector.Models.Turso;

namespace TursoConnector.Services;

/// <summary>
/// Production-ready database service with enhanced error handling and resilience
/// Provides robust connection to Turso database with modern retry patterns
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly HttpClient _httpClient;
    private readonly TursoConfiguration _configuration;
    private readonly ILogService _logService;
    private readonly ResilienceService _resilienceService;

    // Connection management
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly Dictionary<string, object> _queryCache;
    private readonly object _cacheLock = new();

    public DatabaseService(
        HttpClient httpClient,
        TursoConfiguration configuration,
        ILogService logService,
        ResilienceService resilienceService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        _resilienceService = resilienceService ?? throw new ArgumentNullException(nameof(resilienceService));

        // Initialize connection management
        _connectionSemaphore = new SemaphoreSlim(10, 10); // Max 10 concurrent connections
        _queryCache = new Dictionary<string, object>();

        // Configure HTTP client
        ConfigureHttpClient();
    }

    /// <summary>
    /// Configures HTTP client for optimal Turso connectivity
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.AuthToken}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "KrazyPlanetSurvivor/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _logService.LogInfo("HTTP client configured for Turso connectivity with resilience patterns");
    }

    public async Task<TursoResponse?> ExecuteSqlAsync(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query cannot be null or empty", nameof(sql));
        }

        // Connection throttling
        await _connectionSemaphore.WaitAsync();

        try
        {
            _logService.LogInfo($"Executing SQL query: {sql}");

            var apiUrl = _configuration.GetApiUrl();

            var request = new TursoRequest
            {
                Requests = new[]
                {
                    new TursoRequestItem
                    {
                        Type = "execute",
                        Stmt = new TursoStatement { Sql = sql }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            _logService.LogDebug($"JSON request payload: {json}");

            // Use modern resilience service for HTTP calls
            var response = await _resilienceService.ExecuteWithResilienceAsync(async () =>
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(apiUrl, content);
            });

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                _logService.LogDebug($"JSON response received: {responseJson}");
                _logService.LogInfo("✅ Query executed successfully with resilience");

                var result = JsonSerializer.Deserialize<TursoResponse>(responseJson);

                // Cache successful results for read queries
                CacheQueryResult(sql, result);

                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"HTTP error {response.StatusCode}: {errorContent}";

                _logService.LogError(errorMessage);
                return null;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error executing SQL query: {ex.Message}";
            _logService.LogError(errorMessage);
            return null;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            _logService.LogInfo("Testing database connection...");

            var result = await ExecuteSqlAsync("SELECT 1 as test");
            var isHealthy = result?.Results?.Any() == true;

            if (isHealthy)
            {
                _logService.LogInfo("✅ Database connection test successful");
            }
            else
            {
                _logService.LogError("❌ Database connection test failed - no results returned");
            }

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logService.LogError($"Database connection test failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Caches query results for performance optimization
    /// </summary>
    private void CacheQueryResult(string sql, TursoResponse? result)
    {
        if (sql.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && result != null)
        {
            lock (_cacheLock)
            {
                var cacheKey = sql.GetHashCode().ToString();
                _queryCache[cacheKey] = new { Result = result, Timestamp = DateTime.UtcNow };

                // Keep cache size manageable
                if (_queryCache.Count > 100)
                {
                    var oldestKey = _queryCache.Keys.First();
                    _queryCache.Remove(oldestKey);
                }
            }
        }
    }

    /// <summary>
    /// Gets performance metrics for monitoring
    /// </summary>
    public DatabaseMetrics GetMetrics()
    {
        lock (_cacheLock)
        {
            return new DatabaseMetrics
            {
                CachedQueriesCount = _queryCache.Count,
                AvailableConnections = _connectionSemaphore.CurrentCount,
                DatabaseUrl = _configuration.DatabaseUrl
            };
        }
    }
}

/// <summary>
/// Database performance metrics for monitoring
/// </summary>
public class DatabaseMetrics
{
    public int CachedQueriesCount { get; set; }
    public int AvailableConnections { get; set; }
    public string DatabaseUrl { get; set; } = string.Empty;
}
