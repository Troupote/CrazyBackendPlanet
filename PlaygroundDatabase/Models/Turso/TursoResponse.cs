using System.Text.Json.Serialization;

namespace PlaygroundDatabase.Models.Turso;

/// <summary>
/// Turso API response
/// </summary>
public class TursoResponse
{
    [JsonPropertyName("results")]
    public TursoResultWrapper[]? Results { get; set; }
}

/// <summary>
/// Wrapper for Turso results
/// </summary>
public class TursoResultWrapper
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("response")]
    public TursoResponseData? Response { get; set; }
}

/// <summary>
/// Turso response data
/// </summary>
public class TursoResponseData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("result")]
    public TursoResult? Result { get; set; }
}

/// <summary>
/// Result of an SQL query
/// </summary>
public class TursoResult
{
    [JsonPropertyName("cols")]
    public TursoColumn[]? Cols { get; set; }

    [JsonPropertyName("rows")]
    public TursoValue[][]? Rows { get; set; }
}

/// <summary>
/// Table column
/// </summary>
public class TursoColumn
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("decltype")]
    public string? Type { get; set; }
}

/// <summary>
/// Value in a table
/// </summary>
public class TursoValue
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
