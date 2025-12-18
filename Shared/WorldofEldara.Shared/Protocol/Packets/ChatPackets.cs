using MessagePack;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     Chat and communication packets
/// </summary>
public static class ChatPackets
{
    [MessagePackObject]
    public class ChatMessagePacket : PacketBase
    {
        [Key(0)] public ChatChannel Channel { get; set; }

        [Key(1)] public ulong SenderEntityId { get; set; }

        [Key(2)] public string SenderName { get; set; } = string.Empty;

        [Key(3)] public string Message { get; set; } = string.Empty;

        [Key(4)] public ulong? TargetEntityId { get; set; } // For whispers

        [Key(5)] public new long Timestamp { get; set; } // Shadows base Timestamp for serialization
    }
}

public enum ChatChannel
{
    Say, // Local area
    Yell, // Wider area
    Whisper, // Private message
    Party, // Group chat
    Guild, // Guild chat
    Faction, // Faction-wide (lore-specific channels)
    System, // Server messages
    Combat, // Combat log
    Emote // Character emotes
}