using System.Collections.Generic;
using MessagePack;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Shared.Protocol;

/// <summary>
///     Base class for all network packets.
///     Server is authoritative - client sends requests, server sends updates.
/// </summary>
[MessagePackObject]
[Union((int)PacketType.LoginRequest, typeof(AuthPackets.LoginRequest))]
[Union((int)PacketType.LoginResponse, typeof(AuthPackets.LoginResponse))]
[Union((int)PacketType.CharacterListRequest, typeof(CharacterPackets.CharacterListRequest))]
[Union((int)PacketType.CharacterListResponse, typeof(CharacterPackets.CharacterListResponse))]
[Union((int)PacketType.CreateCharacterRequest, typeof(CharacterPackets.CreateCharacterRequest))]
[Union((int)PacketType.CreateCharacterResponse, typeof(CharacterPackets.CreateCharacterResponse))]
[Union((int)PacketType.SelectCharacterRequest, typeof(CharacterPackets.SelectCharacterRequest))]
[Union((int)PacketType.SelectCharacterResponse, typeof(CharacterPackets.SelectCharacterResponse))]
[Union((int)PacketType.MovementInput, typeof(MovementPackets.MovementInputPacket))]
[Union((int)PacketType.MovementUpdate, typeof(MovementPackets.MovementUpdatePacket))]
[Union((int)PacketType.PositionCorrection, typeof(MovementPackets.PositionCorrectionPacket))]
[Union((int)PacketType.MovementSync, typeof(MovementPackets.MovementSyncPacket))]
[Union((int)PacketType.UseAbilityRequest, typeof(CombatPackets.UseAbilityRequest))]
[Union((int)PacketType.AbilityResult, typeof(CombatPackets.AbilityResultPacket))]
[Union((int)PacketType.Damage, typeof(CombatPackets.DamagePacket))]
[Union((int)PacketType.Healing, typeof(CombatPackets.HealingPacket))]
[Union((int)PacketType.StatusEffect, typeof(CombatPackets.StatusEffectPacket))]
[Union((int)PacketType.ThreatUpdate, typeof(CombatPackets.ThreatUpdatePacket))]
[Union((int)PacketType.CombatEvent, typeof(CombatPackets.CombatEventPacket))]
[Union((int)PacketType.ChatMessage, typeof(ChatPackets.ChatMessagePacket))]
[Union((int)PacketType.EnterWorld, typeof(WorldPackets.EnterWorldPacket))]
[Union((int)PacketType.LeaveWorld, typeof(WorldPackets.LeaveWorldPacket))]
[Union((int)PacketType.EntitySpawn, typeof(WorldPackets.EntitySpawnPacket))]
[Union((int)PacketType.PlayerSpawn, typeof(WorldPackets.PlayerSpawnPacket))]
[Union((int)PacketType.EntityDespawn, typeof(WorldPackets.EntityDespawnPacket))]
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
    MovementSync = 13,

    // Combat (20-29)
    UseAbilityRequest = 20,
    AbilityResult = 21,
    Damage = 22,
    Healing = 23,
    StatusEffect = 24,
    ThreatUpdate = 25,
    CombatEvent = 26,

    // Chat (30-39)
    ChatMessage = 30,

    // World (100-199)
    EnterWorld = 100,
    LeaveWorld = 101,
    EntitySpawn = 102,
    EntityDespawn = 103,
    EntityUpdate = 104,
    PlayerSpawn = 105,

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
///     Protocol versioning helper to keep client/server aligned.
/// </summary>
public static class ProtocolVersions
{
    public const string Current = "1.0.0";

    public static readonly IReadOnlyList<string> Supported = new[] { Current };
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
    InsufficientResources = 25,

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
