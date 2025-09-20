using TursoConnector.Models.Nats;

namespace TursoConnector.Interfaces;

/// <summary>
/// Interface for NATS messaging service
/// </summary>
public interface INatsService
{
    /// <summary>
    /// Starts listening for game messages
    /// </summary>
    Task StartListeningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the NATS service
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Sends a response back to the game
    /// </summary>
    Task SendResponseAsync(string subject, GameResponse response);

    /// <summary>
    /// Checks if NATS connection is healthy
    /// </summary>
    bool IsConnected { get; }
}
