using MessagePack;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;
using WorldofEldara.Shared.Protocol;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     World state and entity management packets
/// </summary>
public static class WorldPackets
{
    [MessagePackObject]
    public class EnterWorldPacket : PacketBase
    {
        [Key(0)] public CharacterData Character { get; set; } = new();

        [Key(1)] public string ZoneId { get; set; } = string.Empty;

        [Key(2)] public long ServerTime { get; set; }

        [Key(3)] public string ProtocolVersion { get; set; } = ProtocolVersions.Current;
    }

    [MessagePackObject]
    public class PlayerSpawnPacket : PacketBase
    {
        [Key(0)] public CharacterSnapshot Character { get; set; } = new();

        [Key(1)] public ResourceSnapshot Resources { get; set; } = ResourceSnapshot.Empty;

        [Key(2)] public string ZoneId { get; set; } = string.Empty;

        [Key(3)] public long ServerTime { get; set; }

        [Key(4)] public IReadOnlyList<AbilitySummary> Abilities { get; set; } = Array.Empty<AbilitySummary>();

        [Key(5)] public string ProtocolVersion { get; set; } = ProtocolVersions.Current;
    }

    [MessagePackObject]
    public class LeaveWorldPacket : PacketBase
    {
        [Key(0)] public ulong EntityId { get; set; }

        [Key(1)] public string Reason { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class EntitySpawnPacket : PacketBase
    {
        [Key(0)] public ulong EntityId { get; set; }

        [Key(1)] public EntityType Type { get; set; }

        [Key(2)] public string Name { get; set; } = string.Empty;

        [Key(3)] public Vector3 Position { get; set; }

        [Key(4)] public float RotationYaw { get; set; }

        [Key(5)] public CharacterData? CharacterData { get; set; } // For player entities

        [Key(6)] public NPCData? NPCData { get; set; } // For NPC entities

        [Key(7)] public ResourceSnapshot? Resources { get; set; }

        [Key(8)] public IReadOnlyList<int>? AbilityIds { get; set; }
    }

    [MessagePackObject]
    public class EntityDespawnPacket : PacketBase
    {
        [Key(0)] public ulong EntityId { get; set; }
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

[MessagePackObject]
public class NPCData
{
    [Key(0)] public int NPCTemplateId { get; set; }

    [Key(1)] public string Name { get; set; } = string.Empty;

    [Key(2)] public int Level { get; set; }

    [Key(3)] public Faction Faction { get; set; }

    [Key(4)] public bool IsHostile { get; set; }

    [Key(5)] public bool IsQuestGiver { get; set; }

    [Key(6)] public bool IsVendor { get; set; }

    [Key(7)] public int MaxHealth { get; set; }

    [Key(8)] public int CurrentHealth { get; set; }

    [Key(9)] public ResourceSnapshot Resources { get; set; } = ResourceSnapshot.Empty;

    [Key(10)] public IReadOnlyList<int> AbilityIds { get; set; } = Array.Empty<int>();
}
