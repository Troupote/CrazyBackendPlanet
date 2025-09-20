using Microsoft.Extensions.Logging;
using TursoConnector.Factories;
using TursoConnector.Services;

namespace TursoConnector;

class Program
{
    static async Task Main()
    {
        // Use advanced Factory pattern with proper lifecycle management
        using var serviceFactory = new ServiceFactory();

        try
        {
            // Start the application host
            await serviceFactory.StartAsync();

            // Get application with full dependency injection
            var application = serviceFactory.CreateApplication();

            // Run the main application logic
            await application.RunAsync();
        }
        catch (Exception ex)
        {
            // Only show critical errors
            Console.WriteLine($"[CRITICAL ERROR] {ex.Message}");
        }
        finally
        {
            try
            {
                // Graceful shutdown
                await serviceFactory.StopAsync();
            }
            catch (Exception shutdownEx)
            {
                Console.WriteLine($"[SHUTDOWN ERROR] {shutdownEx.Message}");
            }
        }
    }
}
