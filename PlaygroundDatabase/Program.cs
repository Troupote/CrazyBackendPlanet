using PlaygroundDatabase.Factories;

namespace PlaygroundDatabase;

class Program
{
    static async Task Main()
    {
        // Use Factory pattern to create and configure all services
        using var serviceFactory = new ServiceFactory();

        try
        {
            // Create application with all configured services
            var application = serviceFactory.CreateApplication();

            // Start the application
            await application.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");
        }

        // Wait for user input before closing
        Console.WriteLine("\n👋 Press any key to exit...");
        Console.ReadKey();
    }
}
