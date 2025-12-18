using MessagePack;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     Character management packets
/// </summary>
public static class CharacterPackets
{
    [MessagePackObject]
    public class CharacterListRequest : PacketBase
    {
        [Key(0)] public ulong AccountId { get; set; }
    }

    [MessagePackObject]
    public class CharacterListResponse : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public List<CharacterData> Characters { get; set; } = new();
    }

    [MessagePackObject]
    public class CreateCharacterRequest : PacketBase
    {
        [Key(0)] public ulong AccountId { get; set; }

        [Key(1)] public string Name { get; set; } = string.Empty;

        [Key(2)] public Race Race { get; set; }

        [Key(3)] public Class Class { get; set; }

        [Key(4)] public Faction Faction { get; set; }

        [Key(5)] public TotemSpirit? TotemSpirit { get; set; }

        [Key(6)] public CharacterAppearance Appearance { get; set; } = new();
    }

    [MessagePackObject]
    public class CreateCharacterResponse : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public string Message { get; set; } = string.Empty;

        [Key(2)] public CharacterData? Character { get; set; }
    }

    [MessagePackObject]
    public class SelectCharacterRequest : PacketBase
    {
        [Key(0)] public ulong CharacterId { get; set; }
    }

    [MessagePackObject]
    public class SelectCharacterResponse : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public string Message { get; set; } = string.Empty;

        [Key(2)] public CharacterData? Character { get; set; }
    }
}