using System.Text;
using System.Text.Json;
using PlaygroundDatabase.Configuration;
using PlaygroundDatabase.Interfaces;
using PlaygroundDatabase.Models.Turso;

namespace PlaygroundDatabase.Services;

/// <summary>
/// Service for database operations with Turso
/// </summary>
public class DatabaseService : IDatabaseService
{
    private readonly HttpClient _httpClient;
    private readonly TursoConfiguration _configuration;
    private readonly ILogService _logService;

    public DatabaseService(HttpClient httpClient, TursoConfiguration configuration, ILogService logService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));

        // Configure HTTP client
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.AuthToken}");
    }

    public async Task<TursoResponse?> ExecuteSqlAsync(string sql)
    {
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

            var json = JsonSerializer.Serialize(request);
            _logService.LogDebug($"JSON request sent: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                _logService.LogDebug($"JSON response received: {responseJson}");
                _logService.LogInfo("✅ Query executed successfully");
                return JsonSerializer.Deserialize<TursoResponse>(responseJson);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logService.LogError($"HTTP error {response.StatusCode}: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error executing SQL query: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var result = await ExecuteSqlAsync("SELECT 1 as test");
            return result?.Results?.Any() == true;
        }
        catch
        {
            return false;
        }
    }
}
