using System.Text;
using System.Text.Json;
using NATS.Client;
using TursoConnector.Interfaces;
using TursoConnector.Models.Business;
using TursoConnector.Models.Nats;

namespace TursoConnector.Services;

/// <summary>
/// NATS messaging service for game communication
/// </summary>
public class NatsService : INatsService, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IExchangeService _exchangeService;
    private readonly ILogService _logService;
    private readonly List<IAsyncSubscription> _subscriptions = new();
    private bool _disposed = false;

    public NatsService(IExchangeService exchangeService, ILogService logService)
    {
        _exchangeService = exchangeService ?? throw new ArgumentNullException(nameof(exchangeService));
        _logService = logService ?? throw new ArgumentNullException(nameof(logService));

        try
        {
            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection("nats://localhost:4222");
            _logService.LogWork("NATS connection established");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to connect to NATS: {ex.Message}");
        }
    }

    public bool IsConnected => _connection?.State == ConnState.CONNECTED;

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (_connection == null)
        {
            _logService.LogError("NATS connection not available");
            return;
        }

        _logService.LogWork("Starting NATS message listeners...");

        // Listen for exchange requests
        var exchangeSubscription = _connection.SubscribeAsync("game.exchange.create", async (sender, args) =>
        {
            await HandleExchangeCreateMessage(args);
        });
        _subscriptions.Add(exchangeSubscription);

        // Listen for exchange queries
        var querySubscription = _connection.SubscribeAsync("game.exchange.query", async (sender, args) =>
        {
            await HandleExchangeQueryMessage(args);
        });
        _subscriptions.Add(querySubscription);

        // Listen for health checks
        var healthSubscription = _connection.SubscribeAsync("game.health.check", async (sender, args) =>
        {
            await HandleHealthCheckMessage(args);
        });
        _subscriptions.Add(healthSubscription);

        _logService.LogWork($"NATS listeners started - subscribed to {_subscriptions.Count} subjects");

        // Keep service running until cancellation
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task HandleExchangeCreateMessage(MsgHandlerEventArgs args)
    {
        var response = new GameResponse { MessageId = Guid.NewGuid().ToString() };

        try
        {
            var messageJson = Encoding.UTF8.GetString(args.Message.Data);
            _logService.LogWork($"Received exchange create request: {messageJson}");

            var gameMessage = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (gameMessage?.Data == null)
            {
                response.StatusCode = GameResponseStatus.BadRequest;
                response.Message = "Invalid message format";
                await SendResponseAsync(args.Message.Reply, response);
                return;
            }

            // Extract exchange data
            var opener = gameMessage.Data.GetValueOrDefault("opener")?.ToString() ?? "";
            var follower = gameMessage.Data.GetValueOrDefault("follower")?.ToString() ?? "";
            var openerCard = gameMessage.Data.GetValueOrDefault("openerCard")?.ToString() ?? "";
            var followerCard = gameMessage.Data.GetValueOrDefault("followerCard")?.ToString() ?? "";

            if (string.IsNullOrEmpty(opener) || string.IsNullOrEmpty(follower))
            {
                response.StatusCode = GameResponseStatus.InvalidData;
                response.Message = "Missing required fields: opener and follower";
                await SendResponseAsync(args.Message.Reply, response);
                return;
            }

            // Create exchange via TursoConnector
            var exchange = new Exchange
            {
                RequestOpener = opener,
                RequestFollower = follower,
                OpenerCard = openerCard,
                FollowerCard = followerCard,
                Date = DateTime.UtcNow
            };

            var success = await _exchangeService.InsertExchangeAsync(exchange);

            if (success)
            {
                response.StatusCode = GameResponseStatus.Success;
                response.Message = "Exchange created successfully";
                response.Data = new Dictionary<string, object>
                {
                    ["exchangeId"] = exchange.Id,
                    ["createdAt"] = exchange.Date
                };
                _logService.LogWork($"Exchange created successfully: {opener} <-> {follower}");
            }
            else
            {
                response.StatusCode = GameResponseStatus.DatabaseError;
                response.Message = "Failed to create exchange in database";
            }
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error handling exchange create: {ex.Message}");
            response.StatusCode = GameResponseStatus.InternalError;
            response.Message = "Internal server error";
        }

        await SendResponseAsync(args.Message.Reply, response);
    }

    private async Task HandleExchangeQueryMessage(MsgHandlerEventArgs args)
    {
        var response = new GameResponse { MessageId = Guid.NewGuid().ToString() };

        try
        {
            var messageJson = Encoding.UTF8.GetString(args.Message.Data);
            _logService.LogWork($"Received exchange query request: {messageJson}");

            var gameMessage = JsonSerializer.Deserialize<GameMessage>(messageJson);
            if (gameMessage?.Data == null)
            {
                response.StatusCode = GameResponseStatus.BadRequest;
                response.Message = "Invalid message format";
                await SendResponseAsync(args.Message.Reply, response);
                return;
            }

            // Get exchange ID if provided, otherwise get all exchanges
            if (gameMessage.Data.TryGetValue("exchangeId", out var exchangeIdObj) &&
                int.TryParse(exchangeIdObj.ToString(), out var exchangeId))
            {
                var exchange = await _exchangeService.GetExchangeByIdAsync(exchangeId);
                if (exchange != null)
                {
                    response.StatusCode = GameResponseStatus.Success;
                    response.Message = "Exchange found";
                    response.Data = new Dictionary<string, object>
                    {
                        ["exchange"] = new
                        {
                            id = exchange.Id,
                            opener = exchange.RequestOpener,
                            follower = exchange.RequestFollower,
                            openerCard = exchange.OpenerCard,
                            followerCard = exchange.FollowerCard,
                            date = exchange.Date
                        }
                    };
                }
                else
                {
                    response.StatusCode = GameResponseStatus.NotFound;
                    response.Message = "Exchange not found";
                }
            }
            else
            {
                // Get all exchanges
                var exchanges = await _exchangeService.GetAllExchangesAsync();
                response.StatusCode = GameResponseStatus.Success;
                response.Message = $"Found {exchanges.Count} exchanges";
                response.Data = new Dictionary<string, object>
                {
                    ["exchanges"] = exchanges.Select(e => new
                    {
                        id = e.Id,
                        opener = e.RequestOpener,
                        follower = e.RequestFollower,
                        openerCard = e.OpenerCard,
                        followerCard = e.FollowerCard,
                        date = e.Date
                    }).ToList()
                };
            }

            _logService.LogWork("Exchange query processed successfully");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Error handling exchange query: {ex.Message}");
            response.StatusCode = GameResponseStatus.InternalError;
            response.Message = "Internal server error";
        }

        await SendResponseAsync(args.Message.Reply, response);
    }

    private async Task HandleHealthCheckMessage(MsgHandlerEventArgs args)
    {
        var response = new GameResponse
        {
            MessageId = Guid.NewGuid().ToString(),
            StatusCode = GameResponseStatus.Success,
            Message = "Service is healthy",
            Data = new Dictionary<string, object>
            {
                ["service"] = "TursoConnector",
                ["status"] = "running",
                ["timestamp"] = DateTime.UtcNow,
                ["natsConnected"] = IsConnected
            }
        };

        _logService.LogWork("Health check request processed");
        await SendResponseAsync(args.Message.Reply, response);
    }

    public async Task SendResponseAsync(string subject, GameResponse response)
    {
        if (_connection == null || string.IsNullOrEmpty(subject))
            return;

        try
        {
            var responseJson = JsonSerializer.Serialize(response);
            var responseData = Encoding.UTF8.GetBytes(responseJson);

            _connection.Publish(subject, responseData);
            await Task.CompletedTask; // For async interface compatibility

            _logService.LogWork($"Response sent to {subject}: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logService.LogError($"Failed to send response: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        _logService.LogWork("Stopping NATS service...");

        foreach (var subscription in _subscriptions)
        {
            subscription?.Unsubscribe();
            subscription?.Dispose();
        }
        _subscriptions.Clear();

        _connection?.Close();
        await Task.CompletedTask;

        _logService.LogWork("NATS service stopped");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StopAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
