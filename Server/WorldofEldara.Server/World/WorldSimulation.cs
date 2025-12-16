using Serilog;
using System.Diagnostics;
using WorldofEldara.Server.Core;

namespace WorldofEldara.Server.World;

/// <summary>
/// Core world simulation loop running at fixed tick rate (20 TPS by default).
/// This is the authoritative game state manager.
/// 
/// Responsibilities:
/// - Update all entities (players, NPCs, objects)
/// - Process combat
/// - Handle AI
/// - Manage spawns and despawns
/// - Update world state (day/night, events, etc.)
/// - Send updates to clients
/// </summary>
public class WorldSimulation
{
    private readonly float _tickRate;
    private readonly float _tickDelta;
    private readonly EntityManager _entityManager;
    private readonly ZoneManager _zoneManager;
    private readonly SpawnSystem _spawnSystem;
    private readonly TimeManager _timeManager;

    private Thread? _simulationThread;
    private bool _isRunning;
    private long _currentTick;
    private readonly Stopwatch _tickTimer = new();

    public long CurrentTick => _currentTick;
    public EntityManager Entities => _entityManager;
    public ZoneManager Zones => _zoneManager;

    public WorldSimulation(int tickRate = 20)
    {
        _tickRate = tickRate;
        _tickDelta = 1.0f / _tickRate;

        _entityManager = new EntityManager();
        _zoneManager = new ZoneManager();
        _spawnSystem = new SpawnSystem(_entityManager, _zoneManager);
        _timeManager = new TimeManager();

        Log.Information($"World Simulation initialized at {_tickRate} TPS (Δt = {_tickDelta:F4}s)");
    }

    public async Task Initialize()
    {
        Log.Information("Loading world data...");

        // Load zones
        await _zoneManager.LoadZones();
        Log.Information($"Loaded {_zoneManager.GetLoadedZoneCount()} zones");

        // Initialize spawn points
        await _spawnSystem.Initialize();
        Log.Information("Spawn system initialized");

        // Initialize world time (lore: Eldara's time is tracked by the Worldroot)
        _timeManager.Initialize();
        Log.Information($"World time initialized: {_timeManager.GetCurrentWorldTime()}");

        Log.Information("World initialization complete");
    }

    public void Start()
    {
        if (_isRunning)
        {
            Log.Warning("World simulation already running");
            return;
        }

        _isRunning = true;
        _currentTick = 0;

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

        Log.Information($"World simulation stopped at tick {_currentTick}");
    }

    /// <summary>
    /// Main simulation loop - runs at fixed tick rate
    /// </summary>
    private void SimulationLoop()
    {
        Log.Information("Entering simulation loop...");
        _tickTimer.Start();

        double accumulator = 0.0;
        long lastTick = _tickTimer.ElapsedMilliseconds;

        while (_isRunning)
        {
            long currentTime = _tickTimer.ElapsedMilliseconds;
            double deltaTime = (currentTime - lastTick) / 1000.0;
            lastTick = currentTime;

            accumulator += deltaTime;

            // Fixed timestep update
            while (accumulator >= _tickDelta)
            {
                Tick(_tickDelta);
                accumulator -= _tickDelta;
                _currentTick++;
            }

            // Sleep to prevent CPU spinning
            // Calculate how much time we have until next tick
            double timeUntilNextTick = _tickDelta - accumulator;
            if (timeUntilNextTick > 0.001) // Only sleep if > 1ms
            {
                Thread.Sleep((int)(timeUntilNextTick * 1000 * 0.9)); // Sleep for 90% of remaining time
            }
        }

        _tickTimer.Stop();
        Log.Information("Exited simulation loop");
    }

    /// <summary>
    /// Single simulation tick - this is where the magic happens
    /// </summary>
    private void Tick(float deltaTime)
    {
        try
        {
            // 1. Update world time (Eldara's day/night cycle, Worldroot strain)
            _timeManager.Update(deltaTime);

            // 2. Update all entities
            _entityManager.UpdateAll(deltaTime);

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
            if (_currentTick % 100 == 0)
            {
                Log.Debug($"Tick {_currentTick}: {_entityManager.GetEntityCount()} entities, {_zoneManager.GetLoadedZoneCount()} zones loaded");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error during simulation tick {_currentTick}");
        }
    }

    /// <summary>
    /// Get current server timestamp for synchronization
    /// </summary>
    public long GetServerTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
