using MessagePack;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Shared.Protocol;

/// <summary>
///     Base class for all network packets.
///     Server is authoritative - client sends requests, server sends updates.
/// </summary>
[MessagePackObject]
[Union(0, typeof(AuthPackets.LoginRequest))]
[Union(1, typeof(AuthPackets.LoginResponse))]
[Union(2, typeof(CharacterPackets.CharacterListRequest))]
[Union(3, typeof(CharacterPackets.CharacterListResponse))]
[Union(4, typeof(CharacterPackets.CreateCharacterRequest))]
[Union(5, typeof(CharacterPackets.CreateCharacterResponse))]
[Union(6, typeof(CharacterPackets.SelectCharacterRequest))]
[Union(7, typeof(CharacterPackets.SelectCharacterResponse))]
[Union(10, typeof(MovementPackets.MovementInputPacket))]
[Union(11, typeof(MovementPackets.MovementUpdatePacket))]
[Union(12, typeof(MovementPackets.PositionCorrectionPacket))]
[Union(20, typeof(CombatPackets.UseAbilityRequest))]
[Union(21, typeof(CombatPackets.AbilityResultPacket))]
[Union(22, typeof(CombatPackets.DamagePacket))]
[Union(23, typeof(CombatPackets.HealingPacket))]
[Union(24, typeof(CombatPackets.StatusEffectPacket))]
[Union(30, typeof(ChatPackets.ChatMessagePacket))]
[Union(100, typeof(WorldPackets.EnterWorldPacket))]
[Union(101, typeof(WorldPackets.LeaveWorldPacket))]
[Union(102, typeof(WorldPackets.EntitySpawnPacket))]
[Union(103, typeof(WorldPackets.EntityDespawnPacket))]
public abstract class PacketBase
{
    /// <summary>
    ///     Packet timestamp (set by sender)
    /// </summary>
    [IgnoreMember]
    public long Timestamp { get; set; }

    /// <summary>
    ///     Sequence number for packet ordering
    /// </summary>
    [IgnoreMember]
    public uint SequenceNumber { get; set; }
}

/// <summary>
///     Packet types for quick identification
/// </summary>
public enum PacketType : ushort
{
    // Authentication (0-9)
    LoginRequest = 0,
    LoginResponse = 1,

    // Character Management (2-9)
    CharacterListRequest = 2,
    CharacterListResponse = 3,
    CreateCharacterRequest = 4,
    CreateCharacterResponse = 5,
    SelectCharacterRequest = 6,
    SelectCharacterResponse = 7,

    // Movement (10-19)
    MovementInput = 10,
    MovementUpdate = 11,
    PositionCorrection = 12,

    // Combat (20-29)
    UseAbilityRequest = 20,
    AbilityResult = 21,
    Damage = 22,
    Healing = 23,
    StatusEffect = 24,
    ThreatUpdate = 25,

    // Chat (30-39)
    ChatMessage = 30,

    // World (100-199)
    EnterWorld = 100,
    LeaveWorld = 101,
    EntitySpawn = 102,
    EntityDespawn = 103,
    EntityUpdate = 104,

    // Inventory (200-249)
    InventoryUpdate = 200,
    ItemPickup = 201,
    ItemDrop = 202,
    EquipItem = 203,
    UnequipItem = 204,

    // Quest (250-299)
    QuestAccept = 250,
    QuestComplete = 251,
    QuestUpdate = 252,
    QuestAbandon = 253,

    // Social (300-349)
    GroupInvite = 300,
    GroupJoin = 301,
    GroupLeave = 302,
    GuildInvite = 310,
    GuildJoin = 311,

    // Server (1000+)
    ServerHeartbeat = 1000,
    ServerShutdown = 1001,
    ServerError = 1002
}

/// <summary>
///     Response codes for client requests
/// </summary>
public enum ResponseCode : byte
{
    Success = 0,
    Error = 1,
    InvalidRequest = 2,
    NotAuthenticated = 3,
    AlreadyExists = 4,
    NotFound = 5,
    InsufficientPermissions = 6,
    InvalidData = 7,
    ServerError = 8,
    Timeout = 9,

    // Character creation specific
    NameTaken = 10,
    InvalidName = 11,
    InvalidRaceClassCombination = 12,
    MaxCharactersReached = 13,

    // Combat specific
    NotInRange = 20,
    NotEnoughMana = 21,
    OnCooldown = 22,
    Interrupted = 23,
    InvalidTarget = 24,

    // Lore-specific errors
    LoreInconsistency = 100 // Attempted action violates lore (e.g., wrong race-class combo)
}

/// <summary>
///     Network constants
/// </summary>
public static class NetworkConstants
{
    // Connection
    public const int DefaultPort = 7777;
    public const int MaxPacketSize = 8192; // 8KB
    public const int BufferSize = 16384; // 16KB

    // Timeouts
    public const int ConnectionTimeoutMs = 30000; // 30 seconds
    public const int HeartbeatIntervalMs = 5000; // 5 seconds

    // Limits
    public const int MaxPlayersPerServer = 5000;
    public const int MaxCharactersPerAccount = 8;

    // Client prediction
    public const int MaxPredictionMs = 500; // Max time to predict ahead
    public const int InterpolationDelayMs = 100; // Delay for smooth interpolation
}