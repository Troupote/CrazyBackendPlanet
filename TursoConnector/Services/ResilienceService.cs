using TursoConnector.Interfaces;

namespace TursoConnector.Services;

/// <summary>
/// Simplified resilience service with basic retry logic
/// Provides reliable HTTP operations without complex dependencies
/// </summary>
public class ResilienceService
{
    private readonly ILogService _logService;

    public ResilienceService(ILogService logService)
    {
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));
    }

    /// <summary>
    /// Executes an HTTP request with simple retry logic
    /// </summary>
    public async Task<HttpResponseMessage> ExecuteWithResilienceAsync(Func<Task<HttpResponseMessage>> operation)
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logService.LogDebug($"Executing operation - attempt {attempt}/{maxRetries}");

                var response = await operation();

                if (response.IsSuccessStatusCode)
                {
                    _logService.LogDebug($"Operation successful on attempt {attempt}");
                    return response;
                }

                if (attempt == maxRetries)
                {
                    _logService.LogError($"Operation failed after {maxRetries} attempts. Final status: {response.StatusCode}");
                    return response; // Return the last response even if failed
                }

                _logService.LogError($"Attempt {attempt} failed with status {response.StatusCode}, retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
            }
            catch (HttpRequestException ex)
            {
                if (attempt == maxRetries)
                {
                    _logService.LogError($"Operation failed after {maxRetries} attempts with HttpRequestException: {ex.Message}");
                    throw;
                }

                _logService.LogError($"Attempt {attempt} failed with HttpRequestException: {ex.Message}, retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
            }
            catch (TaskCanceledException ex)
            {
                if (attempt == maxRetries)
                {
                    _logService.LogError($"Operation failed after {maxRetries} attempts with timeout: {ex.Message}");
                    throw;
                }

                _logService.LogError($"Attempt {attempt} timed out: {ex.Message}, retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
            }
        }

        // This should never be reached, but just in case
        throw new InvalidOperationException("Retry logic failed unexpectedly");
    }
}
