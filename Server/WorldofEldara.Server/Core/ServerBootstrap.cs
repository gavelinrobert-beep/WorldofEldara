using Microsoft.Extensions.Configuration;
using Serilog;
using WorldofEldara.Server.Networking;
using WorldofEldara.Server.World;

namespace WorldofEldara.Server.Core;

/// <summary>
///     Handles server initialization, startup, and shutdown
/// </summary>
public class ServerBootstrap
{
    private ServerConfig? _config;
    private IConfiguration? _configuration;
    private NetworkServer? _networkServer;
    private WorldSimulation? _worldSimulation;

    public async Task Initialize()
    {
        Log.Information("Initializing server...");

        // Load configuration
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        _config = new ServerConfig();
        _configuration.GetSection("Server").Bind(_config);

        if (_config.Port < 1 || _config.Port > 65535)
        {
            var message = $"Invalid server configuration: Port must be between 1 and 65535. Current value: {_config.Port}.";
            Log.Error(message);
            throw new InvalidOperationException(message);
        }

        if (_config.MaxPlayers <= 0)
        {
            var message = $"Invalid server configuration: MaxPlayers must be greater than 0. Current value: {_config.MaxPlayers}.";
            Log.Error(message);
            throw new InvalidOperationException(message);
        }

        if (_config.TickRate <= 0)
        {
            var message = $"Invalid server configuration: TickRate must be greater than 0. Current value: {_config.TickRate}.";
            Log.Error(message);
            throw new InvalidOperationException(message);
        }

        Log.Information($"Server Name: {_config.Name}");
        Log.Information($"Port: {_config.Port}");
        Log.Information($"Max Players: {_config.MaxPlayers}");
        Log.Information($"Tick Rate: {_config.TickRate} TPS");

        // Initialize world simulation
        Log.Information("Initializing world simulation...");
        _worldSimulation = new WorldSimulation(_config.TickRate);
        await _worldSimulation.Initialize();

        // Initialize network server
        Log.Information("Initializing network server...");
        _networkServer = new NetworkServer(_config.Port, _config.MaxPlayers, _worldSimulation);
        await _networkServer.Initialize();
        _worldSimulation.AttachNetworkServer(_networkServer);

        Log.Information("Server initialization complete.");
    }

    public async Task Start()
    {
        if (_networkServer == null || _worldSimulation == null)
            throw new InvalidOperationException("Server not initialized. Call Initialize() first.");

        Log.Information("Starting server systems...");

        // Start world simulation
        _worldSimulation.Start();

        // Start network server
        await _networkServer.Start();

        Log.Information("All systems online.");
    }

    public async Task Shutdown()
    {
        Log.Information("Shutting down server...");

        // Stop accepting new connections
        if (_networkServer != null)
        {
            await _networkServer.Stop();
            Log.Information("Network server stopped.");
        }

        // Stop world simulation
        if (_worldSimulation != null)
        {
            _worldSimulation.Stop();
            Log.Information("World simulation stopped.");
        }

        // Save any pending data
        // TODO: Persist character data

        Log.Information("Shutdown complete.");
    }
}

/// <summary>
///     Server configuration loaded from appsettings.json
/// </summary>
public class ServerConfig
{
    public string Name { get; set; } = "World of Eldara Server";
    public int Port { get; set; } = 7777;
    public int MaxPlayers { get; set; } = 5000;
    public int TickRate { get; set; } = 20;
    public string Region { get; set; } = "NA-EAST";
}
