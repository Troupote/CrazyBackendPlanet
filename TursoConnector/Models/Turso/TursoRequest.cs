using System.Text.Json.Serialization;

namespace TursoConnector.Models.Turso;

/// <summary>
/// Main request for Turso API
/// </summary>
public class TursoRequest
{
    [JsonPropertyName("requests")]
    public TursoRequestItem[] Requests { get; set; } = Array.Empty<TursoRequestItem>();
}

/// <summary>
/// Individual request item
/// </summary>
public class TursoRequestItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("stmt")]
    public TursoStatement Stmt { get; set; } = new();
}

/// <summary>
/// SQL statement for Turso
/// </summary>
public class TursoStatement
{
    [JsonPropertyName("sql")]
    public string Sql { get; set; } = string.Empty;
}
