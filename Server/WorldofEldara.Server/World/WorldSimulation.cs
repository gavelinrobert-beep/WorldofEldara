using System.Diagnostics;
using Serilog;
using WorldofEldara.Server.Core;

namespace WorldofEldara.Server.World;

/// <summary>
///     Core world simulation loop running at fixed tick rate (20 TPS by default).
///     This is the authoritative game state manager.
///     Responsibilities:
///     - Update all entities (players, NPCs, objects)
///     - Process combat
///     - Handle AI
///     - Manage spawns and despawns
///     - Update world state (day/night, events, etc.)
///     - Send updates to clients
/// </summary>
public class WorldSimulation
{
    private readonly SpawnSystem _spawnSystem;
    private readonly float _tickDelta;
    private readonly float _tickRate;
    private readonly Stopwatch _tickTimer = new();
    private readonly TimeManager _timeManager;
    private Networking.NetworkServer? _networkServer;
    private bool _isRunning;

    private Thread? _simulationThread;

    public WorldSimulation(int tickRate = 20)
    {
        _tickRate = tickRate;
        _tickDelta = 1.0f / _tickRate;

        Entities = new EntityManager();
        Zones = new ZoneManager();
        _timeManager = new TimeManager();
        _spawnSystem = new SpawnSystem(Entities, Zones, _timeManager);

        Log.Information($"World Simulation initialized at {_tickRate} TPS (Δt = {_tickDelta:F4}s)");
    }

    public long CurrentTick { get; private set; }

    public EntityManager Entities { get; }

    public ZoneManager Zones { get; }

    public async Task Initialize()
    {
        Log.Information("Loading world data...");

        // Load zones
        await Zones.LoadZones();
        Log.Information($"Loaded {Zones.GetLoadedZoneCount()} zones");

        // Initialize spawn points
        await _spawnSystem.Initialize();
        Log.Information("Spawn system initialized");

        // Initialize world time (lore: Eldara's time is tracked by the Worldroot)
        _timeManager.Initialize();
        Log.Information($"World time initialized: {_timeManager.GetCurrentWorldTime()}");

        Log.Information("World initialization complete");
    }

    public void AttachNetworkServer(Networking.NetworkServer networkServer)
    {
        _networkServer = networkServer;
        _spawnSystem.AttachNetworkServer(networkServer);
    }

    public void Start()
    {
        if (_isRunning)
        {
            Log.Warning("World simulation already running");
            return;
        }

        _isRunning = true;
        CurrentTick = 0;

        _simulationThread = new Thread(SimulationLoop)
        {
            Name = "WorldSimulation",
            IsBackground = false,
            Priority = ThreadPriority.AboveNormal
        };

        _simulationThread.Start();
        Log.Information("World simulation started");
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            Log.Warning("World simulation not running");
            return;
        }

        _isRunning = false;
        _simulationThread?.Join(TimeSpan.FromSeconds(5));

        Log.Information($"World simulation stopped at tick {CurrentTick}");
    }

    /// <summary>
    ///     Main simulation loop - runs at fixed tick rate
    /// </summary>
    private void SimulationLoop()
    {
        Log.Information("Entering simulation loop...");
        _tickTimer.Start();

        var accumulator = 0.0;
        var lastTick = _tickTimer.ElapsedMilliseconds;

        while (_isRunning)
        {
            var currentTime = _tickTimer.ElapsedMilliseconds;
            var deltaTime = (currentTime - lastTick) / 1000.0;
            lastTick = currentTime;

            accumulator += deltaTime;

            // Fixed timestep update
            while (accumulator >= _tickDelta)
            {
                Tick(_tickDelta);
                accumulator -= _tickDelta;
                CurrentTick++;
            }

            // Sleep to prevent CPU spinning
            // Calculate how much time we have until next tick
            var timeUntilNextTick = _tickDelta - accumulator;
            if (timeUntilNextTick > 0.001) // Only sleep if > 1ms
                Thread.Sleep((int)(timeUntilNextTick * 1000 * 0.9)); // Sleep for 90% of remaining time
        }

        _tickTimer.Stop();
        Log.Information("Exited simulation loop");
    }

    /// <summary>
    ///     Single simulation tick - this is where the magic happens
    /// </summary>
    private void Tick(float deltaTime)
    {
        try
        {
            // 1. Update world time (Eldara's day/night cycle, Worldroot strain)
            _timeManager.Update(deltaTime);

            // 2. Update all entities
            Entities.UpdateAll(deltaTime);

            // 3. Process combat
            // TODO: Combat system tick

            // 4. Update AI
            // TODO: AI system tick

            // 5. Handle spawns and despawns
            _spawnSystem.Update(deltaTime);

            // 6. Process world events (Giant stirrings, Void rifts, Memory echoes)
            // TODO: World events tick

            // 7. Send updates to clients
            // This happens in NetworkServer based on our entity updates

            // Log every 100 ticks (every 5 seconds at 20 TPS)
            if (CurrentTick % 100 == 0)
                Log.Debug(
                    $"Tick {CurrentTick}: {Entities.GetEntityCount()} entities, {Zones.GetLoadedZoneCount()} zones loaded");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error during simulation tick {CurrentTick}");
        }
    }

    /// <summary>
    ///     Get current server timestamp for synchronization
    /// </summary>
    public long GetServerTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
