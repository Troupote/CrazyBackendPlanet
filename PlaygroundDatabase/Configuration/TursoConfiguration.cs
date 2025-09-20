namespace PlaygroundDatabase.Configuration;

/// <summary>
/// Configuration for Turso connection
/// </summary>
public class TursoConfiguration
{
    public string DatabaseUrl { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Validates that the configuration is complete
    /// </summary>
    public bool IsValid => !string.IsNullOrEmpty(DatabaseUrl) && !string.IsNullOrEmpty(AuthToken);

    /// <summary>
    /// Builds the API URL from the database URL
    /// </summary>
    public string GetApiUrl()
    {
        return DatabaseUrl.Replace("libsql://", "https://").TrimEnd('/') + "/v2/pipeline";
    }
}
