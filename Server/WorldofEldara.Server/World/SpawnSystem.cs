using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Server.World;

/// <summary>
/// Manages entity spawning and respawning
/// </summary>
public class SpawnSystem
{
    private readonly EntityManager _entityManager;
    private readonly ZoneManager _zoneManager;
    private readonly List<SpawnPoint> _spawnPoints = new();

    public SpawnSystem(EntityManager entityManager, ZoneManager zoneManager)
    {
        _entityManager = entityManager;
        _zoneManager = zoneManager;
    }

    public async Task Initialize()
    {
        Log.Information("Initializing spawn points...");

        // TODO: Load spawn points from configuration
        // For now, create some basic spawn points for each starter zone

        CreateStarterZoneSpawns();

        Log.Information($"Initialized {_spawnPoints.Count} spawn points");
        await Task.CompletedTask;
    }

    private void CreateStarterZoneSpawns()
    {
        // Thornveil Enclave - Sylvaen starter
        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_thornveil_enclave",
            Position = new Shared.Protocol.Packets.Vector3(0, 0, 0),
            NPCTemplateId = 1001,
            RespawnTime = 30.0f
        });

        // TODO: Add more spawn points
    }

    public void Update(float deltaTime)
    {
        // Check spawn points and respawn if needed
        foreach (var spawnPoint in _spawnPoints)
        {
            spawnPoint.Update(deltaTime);

            if (spawnPoint.ShouldSpawn())
            {
                SpawnNPC(spawnPoint);
                spawnPoint.OnSpawned();
            }
        }
    }

    private void SpawnNPC(SpawnPoint spawnPoint)
    {
        var npc = new NPCEntity
        {
            EntityId = _entityManager.GenerateEntityId(),
            Name = $"NPC_{spawnPoint.NPCTemplateId}",
            ZoneId = spawnPoint.ZoneId,
            Position = spawnPoint.Position,
            SpawnPosition = spawnPoint.Position,
            NPCTemplateId = spawnPoint.NPCTemplateId,
            Level = 1,
            MaxHealth = 100,
            CurrentHealth = 100,
            Faction = Faction.Neutral,
            IsHostile = false,
            LeashDistance = 50.0f
        };

        _entityManager.AddEntity(npc);
        spawnPoint.SpawnedEntityId = npc.EntityId;
    }
}

/// <summary>
/// Represents a spawn point for NPCs/monsters
/// </summary>
public class SpawnPoint
{
    public string ZoneId { get; set; } = string.Empty;
    public Shared.Protocol.Packets.Vector3 Position { get; set; }
    public int NPCTemplateId { get; set; }
    public float RespawnTime { get; set; } = 30.0f; // seconds
    public float TimeSinceLastSpawn { get; set; }
    public ulong? SpawnedEntityId { get; set; }

    public void Update(float deltaTime)
    {
        if (SpawnedEntityId.HasValue)
        {
            // Already spawned, don't count time
            TimeSinceLastSpawn = 0;
        }
        else
        {
            TimeSinceLastSpawn += deltaTime;
        }
    }

    public bool ShouldSpawn()
    {
        return !SpawnedEntityId.HasValue && TimeSinceLastSpawn >= RespawnTime;
    }

    public void OnSpawned()
    {
        TimeSinceLastSpawn = 0;
    }

    public void OnDespawned()
    {
        SpawnedEntityId = null;
        TimeSinceLastSpawn = 0;
    }
}
