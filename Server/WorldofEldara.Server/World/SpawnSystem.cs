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
        new Vector3(9, 1, 0),
        new Vector3(11, 2, 0),
        new Vector3(12, -1, 0),
        new Vector3(9, -2, 0)
    };

    private static readonly List<Vector3> Zone01RazorFernPatrol = new()
    {
        new Vector3(-6, -4, 0),
        new Vector3(-8, -5, 0),
        new Vector3(-5, -7, 0),
        new Vector3(-3, -5, 0)
    };

    private static readonly List<Vector3> Zone01TwiceDeadHarePatrol = new()
    {
        new Vector3(-13, 5, 0),
        new Vector3(-16, 7, 0),
        new Vector3(-13, 9, 0),
        new Vector3(-10, 6, 0)
    };

    private static readonly List<Vector3> Zone01NullrootSproutPatrol = new()
    {
        new Vector3(11, 11, 0),
        new Vector3(13, 13, 0),
        new Vector3(15, 11, 0),
        new Vector3(12, 9, 0)
    };

    private static readonly List<Vector3> Zone01SeedvaultRootlingPatrol = new()
    {
        new Vector3(-7, 16, 0),
        new Vector3(-10, 18, 0),
        new Vector3(-6, 20, 0),
        new Vector3(-3, 17, 0)
    };

    private static readonly List<Vector3> Zone01SealgnawerMatronPatrol = new()
    {
        new Vector3(-9, 22, 0),
        new Vector3(-6, 23, 0),
        new Vector3(-4, 20, 0),
        new Vector3(-8, 19, 0)
    };

    private static readonly List<Vector3> Zone01WhisperrootPatrol = new()
    {
        new Vector3(5, -8, 0),
        new Vector3(8, -10, 0),
        new Vector3(11, -8, 0),
        new Vector3(7, -6, 0)
    };

    private static readonly List<Vector3> Zone01MossglassPatrol = new()
    {
        new Vector3(-18, 7, 0),
        new Vector3(-21, 9, 0),
        new Vector3(-18, 12, 0),
        new Vector3(-15, 8, 0)
    };

    private static readonly List<Vector3> Zone01OldThornwayPatrol = new()
    {
        new Vector3(17, -11, 0),
        new Vector3(20, -13, 0),
        new Vector3(24, -11, 0),
        new Vector3(21, -8, 0)
    };

    private static readonly List<Vector3> Zone01GreenScarPatrol = new()
    {
        new Vector3(13, 12, 0),
        new Vector3(16, 15, 0),
        new Vector3(20, 13, 0),
        new Vector3(15, 10, 0)
    };

    private static readonly List<Vector3> Zone01SeedvaultDeepPatrol = new()
    {
        new Vector3(-9, 24, 0),
        new Vector3(-5, 25, 0),
        new Vector3(-2, 22, 0),
        new Vector3(-7, 21, 0)
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
            Position = new Vector3(1, 1, 0),
            NPCTemplateId = 1001,
            OverrideName = "Elder Thaelir",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-4, -3, 0),
            NPCTemplateId = 1002,
            OverrideName = "Memory Keeper Savaen",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-1, 4, 0),
            NPCTemplateId = 1003,
            OverrideName = "Druid Ylvhara",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(3, 2, 0),
            NPCTemplateId = 5001,
            OverrideName = "Root Guardian Orenhusk",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(5, -5, 0),
            NPCTemplateId = 1004,
            OverrideName = "Initiate Vessa Newleaf",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(17, -11, 0),
            NPCTemplateId = 1005,
            OverrideName = "Lysarien Vaelth",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.Neutral
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(16, 13, 0),
            NPCTemplateId = 1006,
            OverrideName = "Nim Without-Echo",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.Neutral
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-12, -5, 0),
            NPCTemplateId = 1007,
            OverrideName = "Memory Trader Moss-Under-Ferns",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(8, 9, 0),
            NPCTemplateId = 1008,
            OverrideName = "God-Seeker Irielle",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(21, -9, 0),
            NPCTemplateId = 1009,
            OverrideName = "Barksmith Harth Orenroot",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            IsHostileOverride = false,
            FactionOverride = Faction.VerdantCircles
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(9, 1, 0),
            NPCTemplateId = 6001,
            OverrideName = "Hollow Sapling",
            RespawnTime = 12.0f,
            TimeSinceLastSpawn = 12.0f,
            MaxAliveInZone = 4,
            MaxHealth = 110,
            Level = 1,
            MovementSpeed = 2.8f,
            AggroRange = 7.5f,
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
            Position = new Vector3(-6, -4, 0),
            NPCTemplateId = 6002,
            OverrideName = "Razor Fern",
            RespawnTime = 10.0f,
            TimeSinceLastSpawn = 10.0f,
            MaxAliveInZone = 4,
            MaxHealth = 90,
            Level = 2,
            MovementSpeed = 3.8f,
            AggroRange = 8.5f,
            AttackRange = 2.6f,
            AttackCooldown = 1.2f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "zone01_hostile",
            PatrolPauseDuration = 0.6f,
            PatrolPath = new List<Vector3>(Zone01RazorFernPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-13, 5, 0),
            NPCTemplateId = 6003,
            OverrideName = "Twice-Dead Hare",
            RespawnTime = 14.0f,
            TimeSinceLastSpawn = 14.0f,
            MaxAliveInZone = 2,
            MaxHealth = 70,
            Level = 2,
            MovementSpeed = 4.4f,
            AggroRange = 6.5f,
            AttackRange = 1.8f,
            AttackCooldown = 1.4f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "memory_anomaly",
            PatrolPauseDuration = 0.45f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01TwiceDeadHarePatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(11, 11, 0),
            NPCTemplateId = 6004,
            OverrideName = "Nullroot Sprout",
            RespawnTime = 16.0f,
            TimeSinceLastSpawn = 16.0f,
            MaxAliveInZone = 3,
            MaxHealth = 125,
            Level = 3,
            MovementSpeed = 3.2f,
            AggroRange = 7.5f,
            AttackRange = 2.2f,
            AttackCooldown = 1.55f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_corruption",
            PatrolPauseDuration = 0.7f,
            LeashDistance = 16.0f,
            PatrolPath = new List<Vector3>(Zone01NullrootSproutPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-7, 16, 0),
            NPCTemplateId = 6005,
            OverrideName = "Ward-Eaten Rootling",
            RespawnTime = 18.0f,
            TimeSinceLastSpawn = 18.0f,
            MaxAliveInZone = 4,
            MaxHealth = 145,
            Level = 4,
            MovementSpeed = 3.0f,
            AggroRange = 8.0f,
            AttackRange = 2.3f,
            AttackCooldown = 1.45f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_dungeon",
            PatrolPauseDuration = 0.55f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01SeedvaultRootlingPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-9, 22, 0),
            NPCTemplateId = 6006,
            OverrideName = "Sealgnawer Matron",
            RespawnTime = 28.0f,
            TimeSinceLastSpawn = 28.0f,
            MaxAliveInZone = 1,
            MaxHealth = 260,
            Level = 5,
            MovementSpeed = 2.7f,
            AggroRange = 8.0f,
            AttackRange = 2.7f,
            AttackCooldown = 1.7f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_boss",
            PatrolPauseDuration = 0.9f,
            LeashDistance = 20.0f,
            PatrolPath = new List<Vector3>(Zone01SealgnawerMatronPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(5, -8, 0),
            NPCTemplateId = 6007,
            OverrideName = "Rotbound Moth",
            RespawnTime = 12.0f,
            TimeSinceLastSpawn = 12.0f,
            MaxAliveInZone = 4,
            MaxHealth = 115,
            Level = 3,
            MovementSpeed = 4.0f,
            AggroRange = 7.5f,
            AttackRange = 2.2f,
            AttackCooldown = 1.25f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "thornveil_rot",
            PatrolPauseDuration = 0.45f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01WhisperrootPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-18, 7, 0),
            NPCTemplateId = 6008,
            OverrideName = "Echo-Wolf",
            RespawnTime = 14.0f,
            TimeSinceLastSpawn = 14.0f,
            MaxAliveInZone = 3,
            MaxHealth = 150,
            Level = 4,
            MovementSpeed = 4.2f,
            AggroRange = 8.0f,
            AttackRange = 2.3f,
            AttackCooldown = 1.35f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "memory_anomaly",
            PatrolPauseDuration = 0.55f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01MossglassPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-20, 9, 0),
            NPCTemplateId = 6009,
            OverrideName = "Drowned Echo",
            RespawnTime = 14.0f,
            TimeSinceLastSpawn = 14.0f,
            MaxAliveInZone = 3,
            MaxHealth = 140,
            Level = 4,
            MovementSpeed = 3.4f,
            AggroRange = 7.5f,
            AttackRange = 2.2f,
            AttackCooldown = 1.45f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "mossglass_echo",
            PatrolPauseDuration = 0.65f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01MossglassPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-17, 11, 0),
            NPCTemplateId = 6010,
            OverrideName = "Mossglass Wraith",
            RespawnTime = 16.0f,
            TimeSinceLastSpawn = 16.0f,
            MaxAliveInZone = 3,
            MaxHealth = 165,
            Level = 5,
            MovementSpeed = 3.2f,
            AggroRange = 8.0f,
            AttackRange = 2.4f,
            AttackCooldown = 1.55f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "mossglass_echo",
            PatrolPauseDuration = 0.7f,
            LeashDistance = 19.0f,
            PatrolPath = new List<Vector3>(Zone01MossglassPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(20, -12, 0),
            NPCTemplateId = 6011,
            OverrideName = "Aelthar Scout",
            RespawnTime = 18.0f,
            TimeSinceLastSpawn = 18.0f,
            MaxAliveInZone = 4,
            MaxHealth = 180,
            Level = 5,
            MovementSpeed = 3.6f,
            AggroRange = 8.5f,
            AttackRange = 2.4f,
            AttackCooldown = 1.35f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "aelthar_pressure",
            PatrolPauseDuration = 0.55f,
            LeashDistance = 22.0f,
            PatrolPath = new List<Vector3>(Zone01OldThornwayPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(8, -10, 0),
            NPCTemplateId = 6012,
            OverrideName = "Temporal Residue",
            RespawnTime = 16.0f,
            TimeSinceLastSpawn = 16.0f,
            MaxAliveInZone = 3,
            MaxHealth = 155,
            Level = 5,
            MovementSpeed = 3.9f,
            AggroRange = 7.5f,
            AttackRange = 2.1f,
            AttackCooldown = 1.25f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "time_anomaly",
            PatrolPauseDuration = 0.45f,
            LeashDistance = 18.0f,
            PatrolPath = new List<Vector3>(Zone01WhisperrootPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(16, 14, 0),
            NPCTemplateId = 6013,
            OverrideName = "Nameless Growth",
            RespawnTime = 18.0f,
            TimeSinceLastSpawn = 18.0f,
            MaxAliveInZone = 4,
            MaxHealth = 185,
            Level = 6,
            MovementSpeed = 3.0f,
            AggroRange = 8.0f,
            AttackRange = 2.4f,
            AttackCooldown = 1.5f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "nameless_patch",
            PatrolPauseDuration = 0.75f,
            LeashDistance = 20.0f,
            PatrolPath = new List<Vector3>(Zone01GreenScarPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-9, 24, 0),
            NPCTemplateId = 6014,
            OverrideName = "Amsa-Once",
            RespawnTime = 26.0f,
            TimeSinceLastSpawn = 26.0f,
            MaxAliveInZone = 1,
            MaxHealth = 360,
            Level = 8,
            MovementSpeed = 2.8f,
            AggroRange = 8.5f,
            AttackRange = 2.8f,
            AttackCooldown = 1.7f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_boss",
            PatrolPauseDuration = 0.85f,
            LeashDistance = 22.0f,
            PatrolPath = new List<Vector3>(Zone01SeedvaultDeepPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-5, 25, 0),
            NPCTemplateId = 6015,
            OverrideName = "Oranyn Chorus Fragment",
            RespawnTime = 18.0f,
            TimeSinceLastSpawn = 18.0f,
            MaxAliveInZone = 3,
            MaxHealth = 210,
            Level = 8,
            MovementSpeed = 3.2f,
            AggroRange = 8.0f,
            AttackRange = 2.5f,
            AttackCooldown = 1.5f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_chorus",
            PatrolPauseDuration = 0.65f,
            LeashDistance = 20.0f,
            PatrolPath = new List<Vector3>(Zone01SeedvaultDeepPatrol)
        });

        _spawnPoints.Add(new SpawnPoint
        {
            ZoneId = ZoneConstants.Zone01,
            Position = new Vector3(-7, 21, 0),
            NPCTemplateId = 6016,
            OverrideName = "Unchosen Branch",
            RespawnTime = 16.0f,
            TimeSinceLastSpawn = 16.0f,
            MaxAliveInZone = 4,
            MaxHealth = 200,
            Level = 7,
            MovementSpeed = 3.0f,
            AggroRange = 8.0f,
            AttackRange = 2.5f,
            AttackCooldown = 1.55f,
            IsHostileOverride = true,
            FactionOverride = Faction.Neutral,
            Tag = "seedvault_dungeon",
            PatrolPauseDuration = 0.7f,
            LeashDistance = 20.0f,
            PatrolPath = new List<Vector3>(Zone01SeedvaultDeepPatrol)
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
