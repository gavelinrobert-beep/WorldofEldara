using MessagePack;
using WorldofEldara.Shared.Data.Quest;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     Quest and dialogue related packets (server authoritative).
/// </summary>
public static class QuestPackets
{
    [MessagePackObject]
    public class QuestAcceptRequest : PacketBase
    {
        [Key(0)] public int QuestId { get; set; }
    }

    [MessagePackObject]
    public class QuestAcceptResponse : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public string Message { get; set; } = string.Empty;

        [Key(2)] public QuestStateData? State { get; set; }

        [Key(3)] public QuestDefinition? Definition { get; set; }
    }

    [MessagePackObject]
    public class QuestLogSnapshot : PacketBase
    {
        [Key(0)] public IReadOnlyList<QuestDefinition> Definitions { get; set; } = Array.Empty<QuestDefinition>();

        [Key(1)] public IReadOnlyList<QuestStateData> States { get; set; } = Array.Empty<QuestStateData>();
    }

    [MessagePackObject]
    public class QuestProgressUpdate : PacketBase
    {
        [Key(0)] public QuestStateData State { get; set; } = new();

        [Key(1)] public QuestDefinition? Definition { get; set; }
    }

    [MessagePackObject]
    public class QuestDialogueRequest : PacketBase
    {
        [Key(0)] public ulong NpcEntityId { get; set; }

        [Key(1)] public int? NpcTemplateId { get; set; }
    }

    [MessagePackObject]
    public class QuestDialogueResponse : PacketBase
    {
        [Key(0)] public ulong? NpcEntityId { get; set; }

        [Key(1)] public string NpcName { get; set; } = string.Empty;

        [Key(2)] public IReadOnlyList<QuestDialogueOption> Options { get; set; } = Array.Empty<QuestDialogueOption>();

        [Key(3)] public IReadOnlyList<QuestStateData> UpdatedStates { get; set; } = Array.Empty<QuestStateData>();
    }
}
