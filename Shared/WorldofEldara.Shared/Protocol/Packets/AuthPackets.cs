using MessagePack;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
///     Authentication packets
/// </summary>
public static class AuthPackets
{
    [MessagePackObject]
    public class LoginRequest : PacketBase
    {
        [Key(0)] public string Username { get; set; } = string.Empty;

        [Key(1)] public string PasswordHash { get; set; } = string.Empty; // Client-side hashed

        [Key(2)] public string ClientVersion { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public class LoginResponse : PacketBase
    {
        [Key(0)] public ResponseCode Result { get; set; }

        [Key(1)] public string Message { get; set; } = string.Empty;

        [Key(2)] public ulong AccountId { get; set; }

        [Key(3)] public string SessionToken { get; set; } = string.Empty;
    }
}