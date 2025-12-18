using Serilog;
using WorldofEldara.Server.Core;

namespace WorldofEldara.Server;

/// <summary>
///     World of Eldara - Authoritative Game Server
///     This is a PC-based MMORPG server implementing:
///     - Server-authoritative gameplay
///     - 20 TPS world simulation
///     - Client prediction with reconciliation
///     - Lore-grounded systems
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        // Initialize logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("Logs/server.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("===========================================");
            Log.Information("  World of Eldara - Authoritative Server");
            Log.Information("  The Worldroot is bleeding. Memory awaits.");
            Log.Information("===========================================");

            // Initialize and start server
            var bootstrap = new ServerBootstrap();
            await bootstrap.Initialize();
            await bootstrap.Start();

            Log.Information("Server started successfully. Press Ctrl+C to shutdown.");

            // Wait for shutdown signal
            var shutdownEvent = new ManualResetEventSlim(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                shutdownEvent.Set();
            };

            shutdownEvent.Wait();

            Log.Information("Shutdown signal received. Stopping server...");
            await bootstrap.Shutdown();

            Log.Information("Server shutdown complete.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Server crashed with unhandled exception");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}