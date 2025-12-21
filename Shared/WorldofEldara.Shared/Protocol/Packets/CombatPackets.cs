using MessagePack;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;
using WorldofEldara.Shared.Protocol;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     Combat packets - server authoritative combat
/// </summary>
public static class CombatPackets
{
    /// <summary>
    ///     Client requests to use an ability
    /// </summary>
    [MessagePackObject]
    public class UseAbilityRequest : PacketBase
    {
        [Key(0)] public int AbilityId { get; set; }

        [Key(1)] public ulong? TargetEntityId { get; set; }

        [Key(2)] public Vector3? TargetPosition { get; set; } // For ground-targeted abilities

        [Key(3)] public uint InputSequence { get; set; } // For prediction
    }

    /// <summary>
    ///     Server confirms ability usage and results
    /// </summary>
    [MessagePackObject]
    public class AbilityResultPacket : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public ulong CasterEntityId { get; set; }

        [Key(2)] public int AbilityId { get; set; }

        [Key(3)] public uint InputSequence { get; set; }

        [Key(4)] public string Message { get; set; } = string.Empty; // Error message if failed
    }

    /// <summary>
    ///     Server broadcasts damage events
    /// </summary>
    [MessagePackObject]
    public class DamagePacket : PacketBase
    {
        [Key(0)] public ulong SourceEntityId { get; set; }

        [Key(1)] public ulong TargetEntityId { get; set; }

        [Key(2)] public int AbilityId { get; set; }

        [Key(3)] public DamageType DamageType { get; set; }

        [Key(4)] public int Amount { get; set; }

        [Key(5)] public bool IsCritical { get; set; }

        [Key(6)] public int RemainingHealth { get; set; }

        [Key(7)] public bool IsFatal { get; set; }

        [Key(8)] public CombatEventMetadata Metadata { get; set; } = CombatEventMetadata.Create();
    }

    /// <summary>
    ///     Server broadcasts healing events
    /// </summary>
    [MessagePackObject]
    public class HealingPacket : PacketBase
    {
        [Key(0)] public ulong SourceEntityId { get; set; }

        [Key(1)] public ulong TargetEntityId { get; set; }

        [Key(2)] public int AbilityId { get; set; }

        [Key(3)] public int Amount { get; set; }

        [Key(4)] public bool IsCritical { get; set; }

        [Key(5)] public int RemainingHealth { get; set; }

        [Key(6)] public CombatEventMetadata Metadata { get; set; } = CombatEventMetadata.Create();
    }

    /// <summary>
    ///     Server broadcasts status effect applications/removals
    /// </summary>
    [MessagePackObject]
    public class StatusEffectPacket : PacketBase
    {
        [Key(0)] public ulong TargetEntityId { get; set; }

        [Key(1)] public StatusEffectData Effect { get; set; } = new();

        [Key(2)] public bool Applied { get; set; } // true = applied, false = removed

        [Key(3)] public int StackCount { get; set; }

        [Key(4)] public CombatEventMetadata Metadata { get; set; } = CombatEventMetadata.Create();
    }

    /// <summary>
    ///     Server broadcasts threat/aggro updates
    /// </summary>
    [MessagePackObject]
    public class ThreatUpdatePacket : PacketBase
    {
        [Key(0)] public ulong NPCEntityId { get; set; }

        [Key(1)] public ulong CurrentTargetId { get; set; }

        [Key(2)] public Dictionary<ulong, int> ThreatTable { get; set; } = new(); // EntityId -> Threat amount
    }

    [MessagePackObject]
    public class CombatEventPacket : PacketBase
    {
        [Key(0)] public CombatEventType EventType { get; set; }

        [Key(1)] public CombatEventMetadata Metadata { get; set; } = CombatEventMetadata.Create();

        [Key(2)] public DamagePacket? Damage { get; set; }

        [Key(3)] public HealingPacket? Healing { get; set; }

        [Key(4)] public StatusEffectPacket? StatusEffect { get; set; }
    }
}

[MessagePackObject]
public struct CombatEventMetadata
{
    [Key(0)] public Guid EventId { get; set; }

    [Key(1)] public long ServerTime { get; set; }

    [Key(2)] public string ProtocolVersion { get; set; }

    public static CombatEventMetadata Create(long? serverTime = null)
    {
        return new CombatEventMetadata
        {
            EventId = Guid.NewGuid(),
            ServerTime = serverTime ?? DateTime.UtcNow.Ticks,
            ProtocolVersion = ProtocolVersions.Current
        };
    }
}

public enum CombatEventType : byte
{
    Damage = 0,
    Healing = 1,
    StatusEffect = 2
}
