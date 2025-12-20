using System.Collections.Concurrent;
using Serilog;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Server.Core;

/// <summary>
///     Manages all entities in the game world (players, NPCs, objects).
///     Thread-safe for concurrent access from network and simulation threads.
/// </summary>
public class EntityManager
{
    private readonly ConcurrentDictionary<ulong, Entity> _entities = new();
    private readonly object _idLock = new();
    private ulong _nextEntityId = 1;

    public event Action<Entity>? EntityAdded;
    public event Action<Entity>? EntityRemoved;

    public int GetEntityCount()
    {
        return _entities.Count;
    }

    /// <summary>
    ///     Generate a new unique entity ID
    /// </summary>
    public ulong GenerateEntityId()
    {
        lock (_idLock)
        {
            return _nextEntityId++;
        }
    }

    /// <summary>
    ///     Add an entity to the world
    /// </summary>
    public bool AddEntity(Entity entity)
    {
        if (_entities.TryAdd(entity.EntityId, entity))
        {
            Log.Debug($"Entity {entity.EntityId} ({entity.Name}) added to world");
            EntityAdded?.Invoke(entity);
            return true;
        }

        Log.Warning($"Failed to add entity {entity.EntityId} - already exists");
        return false;
    }

    /// <summary>
    ///     Remove an entity from the world
    /// </summary>
    public bool RemoveEntity(ulong entityId)
    {
        if (_entities.TryRemove(entityId, out var entity))
        {
            Log.Debug($"Entity {entityId} ({entity.Name}) removed from world");
            EntityRemoved?.Invoke(entity);
            return true;
        }

        Log.Warning($"Failed to remove entity {entityId} - not found");
        return false;
    }

    /// <summary>
    ///     Get an entity by ID
    /// </summary>
    public Entity? GetEntity(ulong entityId)
    {
        _entities.TryGetValue(entityId, out var entity);
        return entity;
    }

    /// <summary>
    ///     Get player entity by character ID
    /// </summary>
    public PlayerEntity? GetPlayerByCharacterId(ulong characterId)
    {
        return _entities.Values
            .OfType<PlayerEntity>()
            .FirstOrDefault(p => p.CharacterData.CharacterId == characterId);
    }

    /// <summary>
    ///     Get all entities in a zone
    /// </summary>
    public List<Entity> GetEntitiesInZone(string zoneId)
    {
        return _entities.Values
            .Where(e => e.ZoneId == zoneId)
            .ToList();
    }

    /// <summary>
    ///     Get all entities within range of a position
    /// </summary>
    public List<Entity> GetEntitiesInRange(string zoneId, float x, float y, float z, float range)
    {
        var rangeSq = range * range;

        return _entities.Values
            .Where(e => e.ZoneId == zoneId)
            .Where(e =>
            {
                var dx = e.Position.X - x;
                var dy = e.Position.Y - y;
                var dz = e.Position.Z - z;
                return dx * dx + dy * dy + dz * dz <= rangeSq;
            })
            .ToList();
    }

    /// <summary>
    ///     Update all entities (called from simulation thread)
    /// </summary>
    public void UpdateAll(float deltaTime)
    {
        foreach (var entity in _entities.Values) entity.Update(deltaTime);
    }

    /// <summary>
    ///     Get all player entities
    /// </summary>
    public List<PlayerEntity> GetAllPlayers()
    {
        return _entities.Values.OfType<PlayerEntity>().ToList();
    }

    /// <summary>
    ///     Get all NPC entities
    /// </summary>
    public List<NPCEntity> GetAllNPCs()
    {
        return _entities.Values.OfType<NPCEntity>().ToList();
    }
}

/// <summary>
///     Base entity class for all world objects
/// </summary>
public abstract class Entity
{
    public ulong EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ZoneId { get; set; } = string.Empty;
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float RotationYaw { get; set; }
    public float RotationPitch { get; set; }
    public MovementState MovementState { get; set; }

    public abstract EntityType GetEntityType();
    public abstract void Update(float deltaTime);
}

/// <summary>
///     Player-controlled entity
/// </summary>
public class PlayerEntity : Entity
{
    public ulong AccountId { get; set; }
    public CharacterData CharacterData { get; set; } = new();
    public DateTime LastInputTime { get; set; }
    public uint LastProcessedInputSequence { get; set; }

    // Combat state
    public ulong? TargetEntityId { get; set; }
    public bool IsInCombat { get; set; }
    public DateTime LastCombatTime { get; set; }

    // Network
    public object? ClientConnection { get; set; } // Reference to network connection

    public override EntityType GetEntityType()
    {
        return EntityType.Player;
    }

    public override void Update(float deltaTime)
    {
        // Player movement is handled by input processing
        // This handles decay effects, regeneration, etc.

        // Check combat timeout
        if (IsInCombat && (DateTime.UtcNow - LastCombatTime).TotalSeconds > 5.0f) IsInCombat = false;
        // TODO: Trigger out-of-combat effects (regen, etc.)
        // TODO: Update buffs/debuffs
        // TODO: Mana/health regeneration
    }
}

/// <summary>
///     NPC entity (includes monsters, vendors, quest givers)
/// </summary>
public class NPCEntity : Entity
{
    public int NPCTemplateId { get; set; }
    public Faction Faction { get; set; }
    public bool IsHostile { get; set; }
    public bool IsQuestGiver { get; set; }
    public bool IsVendor { get; set; }
    public int Level { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }

    // AI state
    public NPCAIState AIState { get; set; } = NPCAIState.Idle;
    public ulong? TargetEntityId { get; set; }
    public Vector3 SpawnPosition { get; set; }
    public float LeashDistance { get; set; } = 50.0f;

    public override EntityType GetEntityType()
    {
        return EntityType.NPC;
    }

    public override void Update(float deltaTime)
    {
        // TODO: AI behavior
        // TODO: Patrol logic
        // TODO: Combat logic
        // TODO: Leash check
    }
}

public enum EntityType
{
    Player,
    NPC,
    Monster,
    Object,
    Vehicle,
    Pet
}

public enum NPCAIState
{
    Idle,
    Patrolling,
    Combat,
    Returning,
    Evading
}
