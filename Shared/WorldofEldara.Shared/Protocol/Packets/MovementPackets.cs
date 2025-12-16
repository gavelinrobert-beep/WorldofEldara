using MessagePack;

namespace WorldofEldara.Shared.Protocol.Packets;

/// <summary>
/// Movement packets with client prediction and server reconciliation
/// </summary>
public static class MovementPackets
{
    /// <summary>
    /// Client sends input to server
    /// </summary>
    [MessagePackObject]
    public class MovementInputPacket : PacketBase
    {
        [Key(0)]
        public uint InputSequence { get; set; } // Client-side input sequence number

        [Key(1)]
        public float DeltaTime { get; set; }

        [Key(2)]
        public MovementInput Input { get; set; } = new();

        [Key(3)]
        public Vector3 PredictedPosition { get; set; } // Client's predicted position

        [Key(4)]
        public float PredictedRotationYaw { get; set; }
    }

    /// <summary>
    /// Server broadcasts movement updates to all clients
    /// </summary>
    [MessagePackObject]
    public class MovementUpdatePacket : PacketBase
    {
        [Key(0)]
        public ulong EntityId { get; set; }

        [Key(1)]
        public Vector3 Position { get; set; }

        [Key(2)]
        public Vector3 Velocity { get; set; }

        [Key(3)]
        public float RotationYaw { get; set; }

        [Key(4)]
        public float RotationPitch { get; set; }

        [Key(5)]
        public MovementState State { get; set; }

        [Key(6)]
        public long ServerTimestamp { get; set; }
    }

    /// <summary>
    /// Server sends correction when client prediction is too far off
    /// </summary>
    [MessagePackObject]
    public class PositionCorrectionPacket : PacketBase
    {
        [Key(0)]
        public uint LastProcessedInput { get; set; } // Which input sequence this correction is for

        [Key(1)]
        public Vector3 AuthoritativePosition { get; set; }

        [Key(2)]
        public Vector3 AuthoritativeVelocity { get; set; }

        [Key(3)]
        public float AuthoritativeRotationYaw { get; set; }

        [Key(4)]
        public long ServerTimestamp { get; set; }
    }
}

[MessagePackObject]
public struct MovementInput
{
    [Key(0)]
    public float Forward { get; set; } // -1 to 1

    [Key(1)]
    public float Strafe { get; set; } // -1 to 1

    [Key(2)]
    public bool Jump { get; set; }

    [Key(3)]
    public bool Sprint { get; set; }

    [Key(4)]
    public float LookYaw { get; set; } // Camera rotation

    [Key(5)]
    public float LookPitch { get; set; }
}

[MessagePackObject]
public struct Vector3
{
    [Key(0)]
    public float X { get; set; }

    [Key(1)]
    public float Y { get; set; }

    [Key(2)]
    public float Z { get; set; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public enum MovementState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Falling,
    Swimming,
    Flying,
    Mounted,
    Stunned,
    Rooted
}
