using System.Text;
using System.Text.Json;
using NATS.Client;

namespace GameTestClient;

class Program
{
    private static IConnection? _connection;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Game Test Client - NATS Publisher/Subscriber");
        Console.WriteLine("==============================================\n");

        try
        {
            // Connect to NATS
            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection("nats://localhost:4222");
            Console.WriteLine("[OK] Connected to NATS server\n");

            while (true)
            {
                ShowMenu();
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await PublishHealthCheck();
                        break;
                    case "2":
                        await PublishCreateExchange();
                        break;
                    case "3":
                        await PublishQueryExchanges();
                        break;
                    case "4":
                        SubscribeToResponses();
                        break;
                    case "5":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("[ERROR] Invalid choice, please try again\n");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }
        finally
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine("Choose an action:");
        Console.WriteLine("1. [HEALTH] Publish Health Check");
        Console.WriteLine("2. [CREATE] Publish Create Exchange");
        Console.WriteLine("3. [QUERY] Publish Query Exchanges");
        Console.WriteLine("4. [LISTEN] Subscribe to Responses");
        Console.WriteLine("5. [EXIT] Exit");
        Console.Write("\nYour choice: ");
    }

    static async Task PublishHealthCheck()
    {
        var message = new
        {
            messageId = Guid.NewGuid().ToString(),
            messageType = "health.check",
            playerId = "test-player",
            data = new Dictionary<string, object>(),
            timestamp = DateTime.UtcNow
        };

        await PublishMessage("game.health.check", message);
    }

    static async Task PublishCreateExchange()
    {
        Console.Write("Enter opener name: ");
        var opener = Console.ReadLine() ?? "DefaultOpener";

        Console.Write("Enter follower name: ");
        var follower = Console.ReadLine() ?? "DefaultFollower";

        Console.Write("Enter opener card: ");
        var openerCard = Console.ReadLine() ?? "Default Card";

        Console.Write("Enter follower card: ");
        var followerCard = Console.ReadLine() ?? "Default Card";

        var message = new
        {
            messageId = Guid.NewGuid().ToString(),
            messageType = "exchange.create",
            playerId = "test-player",
            data = new Dictionary<string, object>
            {
                ["opener"] = opener,
                ["follower"] = follower,
                ["openerCard"] = openerCard,
                ["followerCard"] = followerCard
            },
            timestamp = DateTime.UtcNow
        };

        await PublishMessage("game.exchange.create", message);
    }

    static async Task PublishQueryExchanges()
    {
        Console.Write("Enter exchange ID (or press Enter for all): ");
        var input = Console.ReadLine();

        var data = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(input) && int.TryParse(input, out var exchangeId))
        {
            data["exchangeId"] = exchangeId;
        }

        var message = new
        {
            messageId = Guid.NewGuid().ToString(),
            messageType = "exchange.query",
            playerId = "test-player",
            data = data,
            timestamp = DateTime.UtcNow
        };

        await PublishMessage("game.exchange.query", message);
    }

    static async Task PublishMessage(string subject, object message)
    {
        if (_connection == null)
        {
            Console.WriteLine("[ERROR] Not connected to NATS");
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
            var data = Encoding.UTF8.GetBytes(json);

            Console.WriteLine($"\n[PUBLISH] Publishing to '{subject}':");
            Console.WriteLine(json);

            var response = _connection.Request(subject, data, 5000);
            var responseJson = Encoding.UTF8.GetString(response.Data);

            Console.WriteLine($"\n[RESPONSE] Response received:");
            var formattedResponse = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(responseJson),
                new JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine(formattedResponse);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error publishing message: {ex.Message}\n");
        }
    }

    static void SubscribeToResponses()
    {
        if (_connection == null)
        {
            Console.WriteLine("[ERROR] Not connected to NATS");
            return;
        }

        Console.WriteLine("[LISTEN] Subscribing to all game responses...");
        Console.WriteLine("Press any key to stop listening\n");

        var subscription = _connection.SubscribeAsync("game.response.*", (sender, args) =>
        {
            var responseJson = Encoding.UTF8.GetString(args.Message.Data);
            var formattedResponse = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(responseJson),
                new JsonSerializerOptions { WriteIndented = true }
            );

            Console.WriteLine($"\n[RESPONSE] Response on '{args.Message.Subject}':");
            Console.WriteLine(formattedResponse);
        });

        Console.ReadKey();
        subscription.Unsubscribe();
        Console.WriteLine("\n[STOP] Stopped listening\n");
    }
}
