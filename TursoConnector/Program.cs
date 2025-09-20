using Microsoft.Extensions.Logging;
using TursoConnector.Factories;
using TursoConnector.Services;

namespace TursoConnector;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("🚀 Krazy Planet Survivor - Production Database Connector");
        Console.WriteLine("========================================================");
        Console.WriteLine("Initializing enterprise-grade Turso database connection...\n");

        // Use advanced Factory pattern with proper lifecycle management
        using var serviceFactory = new ServiceFactory();

        try
        {
            // Start the application host
            await serviceFactory.StartAsync();
            Console.WriteLine("✅ Application host started successfully");

            // Get application with full dependency injection
            var application = serviceFactory.CreateApplication();
            Console.WriteLine("✅ Application services initialized");

            // Display health status before running
            var healthStatus = await serviceFactory.GetHealthStatusAsync();
            Console.WriteLine($"🏥 Health Check: {healthStatus}\n");

            // Run the main application logic
            await application.RunAsync();

            // Display final health status and metrics
            Console.WriteLine("\n📊 Final System Status:");
            var finalHealth = await serviceFactory.GetHealthStatusAsync();
            Console.WriteLine($"Health: {finalHealth}");

            // Get database metrics if available
            var databaseService = serviceFactory.GetService<DatabaseService>();
            var metrics = databaseService.GetMetrics();
            Console.WriteLine($"Database Metrics:");
            Console.WriteLine($"  - Cached Queries: {metrics.CachedQueriesCount}");
            Console.WriteLine($"  - Available Connections: {metrics.AvailableConnections}");
            Console.WriteLine($"  - Database URL: {metrics.DatabaseUrl.Split('.')[0]}.*****.io");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");

            // Log detailed error information for debugging
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }

            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            try
            {
                // Graceful shutdown
                await serviceFactory.StopAsync();
                Console.WriteLine("✅ Application host stopped gracefully");
            }
            catch (Exception shutdownEx)
            {
                Console.WriteLine($"⚠️  Warning during shutdown: {shutdownEx.Message}");
            }
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("🎮 Krazy Planet Survivor Database Connector");
        Console.WriteLine("   Production-Ready | Resilient | Scalable");
        Console.WriteLine("👋 Press any key to exit...");
        Console.ReadKey();
    }
}
