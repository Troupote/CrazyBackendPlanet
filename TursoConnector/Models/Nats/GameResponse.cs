using System.Text.Json.Serialization;

namespace TursoConnector.Models.Nats;

/// <summary>
/// Represents a response message sent back to the game
/// </summary>
public class GameResponse
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("statusCode")]
    public GameResponseStatus StatusCode { get; set; } = GameResponseStatus.Success;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Status codes for game responses
/// </summary>
public enum GameResponseStatus
{
    Success = 200,
    BadRequest = 400,
    NotFound = 404,
    InternalError = 500,
    DatabaseError = 501,
    InvalidData = 422
}
