using MessagePack;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;

namespace WorldofEldara.Shared.Protocol.Packets;

public static class InventoryPackets
{
    [MessagePackObject]
    public class EquipItemRequest : PacketBase
    {
        [Key(0)] public string ItemId { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class InventoryUpdatePacket : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public string Message { get; set; } = string.Empty;

        [Key(2)] public IReadOnlyList<InventoryItemStack> Inventory { get; set; } =
            Array.Empty<InventoryItemStack>();

        [Key(3)] public IReadOnlyDictionary<EquipmentSlot, InventoryItemStack> EquippedItems { get; set; } =
            new Dictionary<EquipmentSlot, InventoryItemStack>();

        [Key(4)] public ResourceSnapshot Resources { get; set; } = ResourceSnapshot.Empty;

        [Key(5)] public long TotalGold { get; set; }

        [Key(6)] public long TotalExperience { get; set; }

        [Key(7)] public int Level { get; set; }

        [Key(8)] public long ExperienceForNextLevel { get; set; }

        [Key(9)] public IReadOnlyList<AbilitySummary> Abilities { get; set; } = Array.Empty<AbilitySummary>();
    }

    [MessagePackObject]
    public class ItemPickupPacket : PacketBase
    {
        [Key(0)] public ulong SourceEntityId { get; set; }

        [Key(1)] public string SourceName { get; set; } = string.Empty;

        [Key(2)] public int Gold { get; set; }

        [Key(3)] public long TotalGold { get; set; }

        [Key(4)] public int Experience { get; set; }

        [Key(5)] public long TotalExperience { get; set; }

        [Key(6)] public IReadOnlyList<string> Items { get; set; } = Array.Empty<string>();

        [Key(7)] public string Message { get; set; } = string.Empty;

        [Key(8)] public IReadOnlyList<InventoryItemStack> Inventory { get; set; } =
            Array.Empty<InventoryItemStack>();

        [Key(9)] public int Level { get; set; }

        [Key(10)] public bool LeveledUp { get; set; }

        [Key(11)] public long ExperienceForNextLevel { get; set; }

        [Key(12)] public ResourceSnapshot Resources { get; set; } = ResourceSnapshot.Empty;

        [Key(13)] public IReadOnlyList<AbilitySummary> Abilities { get; set; } = Array.Empty<AbilitySummary>();
    }
}
