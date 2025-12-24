using System;
using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol.Packets;
using System.Linq;
using WorldofEldara.Shared.Data.Quest;
using WorldofEldara.Shared.Constants;

namespace WorldofEldara.Server.World;

/// <summary>
///     Manages entity spawning and respawning
/// </summary>
public class SpawnSystem
{
    private const string TrainingSentinelName = "Greenwatch Sentinel";
    private readonly EntityManager _entityManager;
    private readonly List<SpawnPoint> _spawnPoints = new();
    private readonly ZoneManager _zoneManager;
    private readonly TimeManager _timeManager;
    private readonly Func<long> _serverTimeProvider;
    private readonly object _spawnLock = new();
    private Networking.NetworkServer? _networkServer;
    private static readonly List<Vector3> Zone01SaplingPatrol = new()
    {
        new Vector3(8, 2, 0),
        new Vector3(10, 4, 0),
        new Vector3(12, 2, 0),
        new Vector3(10, 0, 0)
    };

    private static readonly List<Vector3> Zone01RazorFernPatrol = new()
    {
        new Vector3(-4, -1, 0),
        new Vector3(-6, -3, 0),
        new Vector3(-2, -3, 0)
    };

    public SpawnSystem(EntityManager entityManager, ZoneManager zoneManager, TimeManager timeManager,
        Func<long> serverTimeProvider)
    {
        _entityManager = entityManager;
        _zoneManager = zoneManager;
        _timeManager = timeManager;
        _serverTimeProvider = serverTimeProvider;
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
        // Vertical slice Zone_01
        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(2, 2, 0),
            NPCTemplateId = 5001,
            OverrideName = "Warden Elaris",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(8, 2, 0),
            NPCTemplateId = 6001,
            OverrideName = "Hollow Sapling",
            RespawnTime = 12.0f,
            TimeSinceLastSpawn = 12.0f,
            MaxAliveInZone = 4,
            MaxHealth = 110,
            Level = 1,
            MovementSpeed = 2.8f,
            AggroRange = 9.0f,
            AttackRange = 2.0f,
            AttackCooldown = 1.8f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "zone01_hostile",
            PatrolPath = new List<Vector3>(Zone01SaplingPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-4, -1, 0),
            NPCTemplateId = 6002,
            OverrideName = "Razor Fern",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            MaxAliveInZone = 4,
            MaxHealth = 90,
            Level = 2,
            MovementSpeed = 3.8f,
            AggroRange = 13.0f,
            AttackRange = 2.6f,
            AttackCooldown = 1.2f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "zone01_hostile",
            PatrolPauseDuration = 0.6f,
            PatrolPath = new List<Vector3>(Zone01RazorFernPatrol)
        });

        // Thornveil Enclave - Sylvaen starter
        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_thornveil_enclave",
            Position = new Vector3(0, 0, 0),
            NPCTemplateId = 1001,
            OverrideName = "Elder Tharivol",
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.BriarwatchCrossing,
            Position = new Vector3(12, 4, 0),
            NPCTemplateId = 1101,
            OverrideName = "Keeper Aelwyn",
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.BriarwatchCrossing,
            Position = new Vector3(14, 6, 0),
            NPCTemplateId = 1102,
            OverrideName = "Quartermaster Liora",
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_temporal_steppes",
            Position = new Vector3(10, 5, 0),
            NPCTemplateId = 1002,
            OverrideName = "Instructor Lethril",
            RespawnTime = 30.0f,
            TimeSinceLastSpawn = 30.0f
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_borderkeep",
            Position = new Vector3(-8, -4, 0),
            NPCTemplateId = 1003,
            OverrideName = "Scout Maerith",
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

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = "zone_greenwatch_training",
            Position = new Vector3(20, -5, 0),
            NPCTemplateId = 2001,
            RespawnTime = 20.0f,
            TimeSinceLastSpawn = 20.0f,
            MaxAliveInZone = 2,
            DaytimeOnly = true,
            OverrideName = TrainingSentinelName,
            IsHostileOverride = true,
            FactionOverride = Faction.VerdantCircles,
            PatrolPauseDuration = 1.25f,
            MovementSpeed = 3.0f,
            AggroRange = 10.0f,
            AttackRange = 2.2f,
            AttackCooldown = 1.75f,
            LeashDistance = 25.0f,
            PatrolPath = new List<Vector3>
            {
                new Vector3(20, -5, 0),
                new Vector3(24, -5, 0),
                new Vector3(24, -1, 0),
                new Vector3(20, -1, 0)
            }
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

            if (spawnPoint.ShouldSpawn() && IsSpawnAllowed(spawnPoint))
            {
                SpawnNPC(spawnPoint);
                spawnPoint.OnSpawned();
            }
        }
    }

    private bool IsSpawnAllowed(SpawnPoint spawnPoint)
    {
        var zone = _zoneManager.GetZone(spawnPoint.ZoneId);
        if (zone == null) return false;

        if (spawnPoint.DaytimeOnly && !_timeManager.IsDaytime()) return false;
        if (spawnPoint.NightOnly && _timeManager.IsDaytime()) return false;

        if (spawnPoint.MaxAliveInZone > 0)
        {
            var aliveInZone = _entityManager.GetEntitiesInZone(spawnPoint.ZoneId)
                .OfType<NPCEntity>()
                .Count(npc => npc.NPCTemplateId == spawnPoint.NPCTemplateId);
            if (aliveInZone >= spawnPoint.MaxAliveInZone) return false;
        }

        return true;
    }

    private void SpawnNPC(SpawnPoint spawnPoint)
    {
        lock (_spawnLock)
        {
            var zone = _zoneManager.GetZone(spawnPoint.ZoneId);
            var npc = new NPCEntity
            {
                EntityId = _entityManager.GenerateEntityId(),
                Name = spawnPoint.OverrideName ?? $"NPC_{spawnPoint.NPCTemplateId}",
                ZoneId = spawnPoint.ZoneId,
                Position = spawnPoint.Position,
                SpawnPosition = spawnPoint.Position,
                NPCTemplateId = spawnPoint.NPCTemplateId,
                Level = spawnPoint.Level > 0 ? spawnPoint.Level : 1,
                MaxHealth = spawnPoint.MaxHealth > 0 ? spawnPoint.MaxHealth : 120,
                CurrentHealth = spawnPoint.MaxHealth > 0 ? spawnPoint.MaxHealth : 120,
                Faction = spawnPoint.FactionOverride ?? zone?.ControllingFaction ?? Faction.Neutral,
                IsHostile = spawnPoint.IsHostileOverride ?? zone?.IsPvPEnabled ?? false,
                LeashDistance = spawnPoint.LeashDistance > 0 ? spawnPoint.LeashDistance : 50.0f,
                MovementSpeed = spawnPoint.MovementSpeed > 0 ? spawnPoint.MovementSpeed : 3.5f,
                AggroRange = spawnPoint.AggroRange > 0 ? spawnPoint.AggroRange : 12.0f,
                AttackRange = spawnPoint.AttackRange > 0 ? spawnPoint.AttackRange : 2.5f,
                AttackCooldown = spawnPoint.AttackCooldown > 0 ? spawnPoint.AttackCooldown : 1.5f,
                PatrolPauseDuration = spawnPoint.PatrolPauseDuration > 0 ? spawnPoint.PatrolPauseDuration : 1.0f,
                ActiveDuringDaytimeOnly = spawnPoint.DaytimeOnly,
                ActiveDuringNightOnly = spawnPoint.NightOnly,
                Tag = spawnPoint.Tag,
                EntityManager = _entityManager,
                ZoneManager = _zoneManager,
                TimeManager = _timeManager,
                NetworkServer = _networkServer,
                ServerTimeProvider = _serverTimeProvider,
                IsQuestGiver = QuestCatalog.IsQuestNpc(spawnPoint.NPCTemplateId)
            };

            if (spawnPoint.PatrolPath.Any())
            {
                npc.PatrolPath = new List<Vector3>(spawnPoint.PatrolPath);
                npc.AIState = NPCAIState.Patrolling;
            }

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
    public int MaxAliveInZone { get; set; }
    public bool DaytimeOnly { get; set; }
    public bool NightOnly { get; set; }
    public string? OverrideName { get; set; }
    public bool? IsHostileOverride { get; set; }
    public Faction? FactionOverride { get; set; }
    public string? Tag { get; set; }
    public float MovementSpeed { get; set; } = 3.5f;
    public float AggroRange { get; set; } = 12.0f;
    public float AttackRange { get; set; } = 2.5f;
    public float AttackCooldown { get; set; } = 1.5f;
    public float PatrolPauseDuration { get; set; } = 1.0f;
    public float LeashDistance { get; set; } = 50.0f;
    public List<Vector3> PatrolPath { get; set; } = new();
    public int Level { get; set; } = 1;
    public int MaxHealth { get; set; } = 120;

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
