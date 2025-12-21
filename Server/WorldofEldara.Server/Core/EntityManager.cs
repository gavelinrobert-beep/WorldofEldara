using System.Collections.Concurrent;
using System.Linq;
using Serilog;
using WorldofEldara.Server.World;
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
    private const float CombatTimeoutSeconds = 5.0f;
    private float _healthRegenBuffer;
    private float _timeSinceCombat;

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

    public void MarkCombatEngaged()
    {
        IsInCombat = true;
        _timeSinceCombat = 0f;
        LastCombatTime = DateTime.UtcNow;
    }

    public override void Update(float deltaTime)
    {
        if (IsInCombat)
        {
            _timeSinceCombat += deltaTime;
            if (_timeSinceCombat >= CombatTimeoutSeconds)
            {
                IsInCombat = false;
                _timeSinceCombat = 0f;
            }
        }
        else
        {
            _timeSinceCombat = 0f;
            var regenRate = Math.Max(1f, CharacterData.Stats.MaxHealth * 0.01f);
            _healthRegenBuffer += regenRate * deltaTime;
            var healAmount = (int)_healthRegenBuffer;
            if (healAmount > 0)
            {
                CharacterData.Stats.CurrentHealth =
                    Math.Min(CharacterData.Stats.MaxHealth, CharacterData.Stats.CurrentHealth + healAmount);
                _healthRegenBuffer -= healAmount;
            }
        }
    }
}

/// <summary>
///     NPC entity (includes monsters, vendors, quest givers)
/// </summary>
public class NPCEntity : Entity
{
    private const float CombatResetSeconds = 6.0f;
    private float _attackTimer;
    private float _combatResetTimer;
    private float _patrolPauseTimer;

    public int NPCTemplateId { get; set; }
    public Faction Faction { get; set; }
    public bool IsHostile { get; set; }
    public bool IsQuestGiver { get; set; }
    public bool IsVendor { get; set; }
    public int Level { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentHealth { get; set; }

    public float MovementSpeed { get; set; } = 3.5f;
    public float AggroRange { get; set; } = 12.0f;
    public float AttackRange { get; set; } = 2.5f;
    public float AttackCooldown { get; set; } = 1.5f;
    public List<Vector3> PatrolPath { get; set; } = new();
    public int CurrentPatrolIndex { get; set; }
    public float PatrolPauseDuration { get; set; } = 1.0f;
    public bool ActiveDuringDaytimeOnly { get; set; }
    public bool ActiveDuringNightOnly { get; set; }

    public EntityManager? EntityManager { get; set; }
    public ZoneManager? ZoneManager { get; set; }
    public TimeManager? TimeManager { get; set; }

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
        if (!IsActiveBasedOnTime())
        {
            Velocity = new Vector3(0, 0, 0);
            MovementState = MovementState.Idle;
            if (AIState != NPCAIState.Returning && Distance(Position, SpawnPosition) > 0.25f)
                AIState = NPCAIState.Returning;

            if (AIState == NPCAIState.Returning)
                UpdateReturn(deltaTime);

            return;
        }

        switch (AIState)
        {
            case NPCAIState.Idle:
                Velocity = new Vector3(0, 0, 0);
                MovementState = MovementState.Idle;
                TryAcquireTarget();
                if (PatrolPath.Count > 0) AIState = NPCAIState.Patrolling;
                break;
            case NPCAIState.Patrolling:
                TryAcquireTarget();
                UpdatePatrol(deltaTime);
                break;
            case NPCAIState.Combat:
                UpdateCombat(deltaTime);
                break;
            case NPCAIState.Returning:
                UpdateReturn(deltaTime);
                break;
        }
    }

    private bool IsActiveBasedOnTime()
    {
        if (TimeManager == null) return true;
        if (ActiveDuringDaytimeOnly && !TimeManager.IsDaytime()) return false;
        if (ActiveDuringNightOnly && TimeManager.IsDaytime()) return false;
        return true;
    }

    private void TryAcquireTarget()
    {
        if (!IsHostile || EntityManager == null) return;

        var target = EntityManager.GetEntitiesInRange(ZoneId, Position.X, Position.Y, Position.Z, AggroRange)
            .OfType<PlayerEntity>()
            .FirstOrDefault(player =>
                player.CharacterData.Stats.CurrentHealth > 0 && player.CharacterData.Faction != Faction);

        if (target == null) return;

        TargetEntityId = target.EntityId;
        AIState = NPCAIState.Combat;
        _attackTimer = 0f;
        _combatResetTimer = 0f;
    }

    private void UpdatePatrol(float deltaTime)
    {
        if (PatrolPath.Count == 0)
        {
            AIState = NPCAIState.Idle;
            return;
        }

        var waypoint = PatrolPath[CurrentPatrolIndex];
        var distance = Distance(Position, waypoint);

        if (distance <= 0.35f)
        {
            Velocity = new Vector3(0, 0, 0);
            MovementState = MovementState.Idle;
            _patrolPauseTimer += deltaTime;
            if (_patrolPauseTimer >= PatrolPauseDuration)
            {
                CurrentPatrolIndex = (CurrentPatrolIndex + 1) % PatrolPath.Count;
                _patrolPauseTimer = 0f;
            }

            return;
        }

        MoveTowards(waypoint, deltaTime, MovementSpeed * 0.6f);
        MovementState = MovementState.Walking;
    }

    private void UpdateCombat(float deltaTime)
    {
        _attackTimer += deltaTime;
        _combatResetTimer += deltaTime;

        var target = TargetEntityId.HasValue ? EntityManager?.GetEntity(TargetEntityId.Value) as PlayerEntity : null;
        if (target == null)
        {
            ResetToReturn();
            return;
        }

        if (Distance(Position, SpawnPosition) > LeashDistance || _combatResetTimer >= CombatResetSeconds)
        {
            ResetToReturn();
            return;
        }

        var distanceToTarget = Distance(Position, target.Position);
        if (distanceToTarget > AggroRange * 1.5f || target.CharacterData.Stats.CurrentHealth <= 0)
        {
            ResetToReturn();
            return;
        }

        if (distanceToTarget > AttackRange)
        {
            MoveTowards(target.Position, deltaTime, MovementSpeed);
            MovementState = MovementState.Running;
            return;
        }

        Velocity = new Vector3(0, 0, 0);
        MovementState = MovementState.Idle;

        if (_attackTimer >= AttackCooldown)
        {
            _attackTimer = 0f;
            var damage = Math.Max(5, Level * 3);
            target.CharacterData.Stats.CurrentHealth =
                Math.Max(0, target.CharacterData.Stats.CurrentHealth - damage);
            target.MarkCombatEngaged();
            _combatResetTimer = 0f;

            if (target.CharacterData.Stats.CurrentHealth <= 0)
            {
                EntityManager?.RemoveEntity(target.EntityId);
                ResetToReturn();
            }
        }
    }

    private void UpdateReturn(float deltaTime)
    {
        MoveTowards(SpawnPosition, deltaTime, MovementSpeed);
        MovementState = MovementState.Walking;

        if (Distance(Position, SpawnPosition) <= 0.25f)
        {
            Position = SpawnPosition;
            Velocity = new Vector3(0, 0, 0);
            TargetEntityId = null;
            CurrentHealth = MaxHealth;
            _attackTimer = 0f;
            _combatResetTimer = 0f;
            AIState = PatrolPath.Count > 0 ? NPCAIState.Patrolling : NPCAIState.Idle;
            MovementState = MovementState.Idle;
        }
    }

    private void ResetToReturn()
    {
        TargetEntityId = null;
        _combatResetTimer = 0f;
        _attackTimer = 0f;
        AIState = NPCAIState.Returning;
    }

    private void MoveTowards(Vector3 targetPosition, float deltaTime, float speed)
    {
        var dirX = targetPosition.X - Position.X;
        var dirY = targetPosition.Y - Position.Y;
        var dirZ = targetPosition.Z - Position.Z;
        var magnitudeSq = dirX * dirX + dirY * dirY + dirZ * dirZ;

        if (magnitudeSq < 1e-6f)
        {
            Velocity = new Vector3(0, 0, 0);
            return;
        }

        var magnitude = (float)Math.Sqrt(magnitudeSq);
        dirX /= magnitude;
        dirY /= magnitude;
        dirZ /= magnitude;

        Velocity = new Vector3(dirX * speed, dirY * speed, dirZ * speed);
        Position = new Vector3(
            Position.X + Velocity.X * deltaTime,
            Position.Y + Velocity.Y * deltaTime,
            Position.Z + Velocity.Z * deltaTime);
    }

    private static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
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
