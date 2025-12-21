using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Server.World;

/// <summary>
///     Manages entity spawning and respawning
/// </summary>
public class SpawnSystem
{
    private readonly EntityManager _entityManager;
    private readonly List<SpawnPoint> _spawnPoints = new();
    private readonly ZoneManager _zoneManager;
    private readonly object _spawnLock = new();
    private Networking.NetworkServer? _networkServer;

    public SpawnSystem(EntityManager entityManager, ZoneManager zoneManager)
    {
        _entityManager = entityManager;
        _zoneManager = zoneManager;
    }

    public void AttachNetworkServer(Networking.NetworkServer networkServer)
    {
        _networkServer = networkServer;
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
            Position = new Vector3(0, 0, 0),
            NPCTemplateId = 1001,
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_temporal_steppes",
            Position = new Vector3(10, 5, 0),
            NPCTemplateId = 1002,
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_borderkeep",
            Position = new Vector3(-8, -4, 0),
            NPCTemplateId = 1003,
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_untamed_reaches",
            Position = new Vector3(6, -12, 0),
            NPCTemplateId = 1004,
            RespawnTime = 35.0f,
            TimeSinceLastSpawn = 35.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_carved_valleys",
            Position = new Vector3(-12, 3, 0),
            NPCTemplateId = 1005,
            RespawnTime = 35.0f,
            TimeSinceLastSpawn = 35.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_scarred_highlands",
            Position = new Vector3(4, 14, 0),
            NPCTemplateId = 1006,
            RespawnTime = 40.0f,
            TimeSinceLastSpawn = 40.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_blackwake_haven",
            Position = new Vector3(-2, 18, 0),
            NPCTemplateId = 1007,
            RespawnTime = 40.0f,
            TimeSinceLastSpawn = 40.0f
        });
    }

    public void Update(float deltaTime)
    {
        // Check spawn points and respawn if needed
        foreach (var spawnPoint in _spawnPoints)
        {
            spawnPoint.Update(deltaTime);

            if (spawnPoint.SpawnedEntityId.HasValue &&
                _entityManager.GetEntity(spawnPoint.SpawnedEntityId.Value) == null)
            {
                spawnPoint.OnDespawned();
            }

            if (spawnPoint.ShouldSpawn())
            {
                SpawnNPC(spawnPoint);
                spawnPoint.OnSpawned();
            }
        }
    }

    private void SpawnNPC(SpawnPoint spawnPoint)
    {
        lock (_spawnLock)
        {
            var zone = _zoneManager.GetZone(spawnPoint.ZoneId);
            var npc = new NPCEntity
            {
                EntityId = _entityManager.GenerateEntityId(),
                Name = $"NPC_{spawnPoint.NPCTemplateId}",
                ZoneId = spawnPoint.ZoneId,
                Position = spawnPoint.Position,
                SpawnPosition = spawnPoint.Position,
                NPCTemplateId = spawnPoint.NPCTemplateId,
                Level = 1,
                MaxHealth = 120,
                CurrentHealth = 120,
                Faction = zone?.ControllingFaction ?? Faction.Neutral,
                IsHostile = zone?.IsPvPEnabled ?? false,
                LeashDistance = 50.0f
            };

            if (_entityManager.AddEntity(npc))
            {
                spawnPoint.SpawnedEntityId = npc.EntityId;
            }
        }
    }
}

/// <summary>
///     Represents a spawn point for NPCs/monsters
/// </summary>
public class SpawnPoint
{
    public string ZoneId { get; set; } = string.Empty;
    public Vector3 Position { get; set; }
    public int NPCTemplateId { get; set; }
    public float RespawnTime { get; set; } = 30.0f; // seconds
    public float TimeSinceLastSpawn { get; set; }
    public ulong? SpawnedEntityId { get; set; }

    public void Update(float deltaTime)
    {
        if (SpawnedEntityId.HasValue)
            // Already spawned, don't count time
            TimeSinceLastSpawn = 0;
        else
            TimeSinceLastSpawn += deltaTime;
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
