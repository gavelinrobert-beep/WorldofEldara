using System.Drawing;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class WorldCamera
{
    public float Yaw { get; private set; }
    public float Distance { get; private set; } = 6.4f;
    public float Height { get; private set; } = 2.65f;
    public Vector3 Focus { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Forward { get; private set; }
    public Vector3 Right { get; private set; }
    public Vector3 Up { get; private set; }

    public void Reset(float playerYaw, Vector3 focus)
    {
        Yaw = playerYaw;
        Distance = 6.4f;
        Height = 2.65f;
        Focus = focus;
    }

    public void AdjustDistance(int wheelDelta)
    {
        Distance = Math.Clamp(Distance + (wheelDelta > 0 ? -0.45f : 0.45f), 4.8f, 10.0f);
    }

    public void Orbit(float yawDelta, float heightDelta)
    {
        Yaw += yawDelta;
        Height = Math.Clamp(Height + heightDelta, 1.9f, 4.6f);
    }

    public void RotateWithKeyboard(float yawDelta)
    {
        Yaw += yawDelta;
    }

    public void SmoothFocus(Vector3 target, float deltaSeconds)
    {
        var focusT = 1f - MathF.Exp(-deltaSeconds * 11f);
        var targetFocus = target + ForwardFromYaw(Yaw) * 0.55f;
        Focus = Vector3.Lerp(Focus, targetFocus, focusT);
    }

    public void SmoothFollowYaw(float playerYaw, float deltaSeconds)
    {
        var delta = NormalizeAngle(playerYaw - Yaw);
        var yawT = 1f - MathF.Exp(-deltaSeconds * 4.2f);
        Yaw += delta * yawT;
    }

    public void Build()
    {
        var cameraForwardFlat = ForwardFromYaw(Yaw);
        var lookAt = Focus + new Vector3(0, 1.15f, 0);
        Position = Focus - cameraForwardFlat * Distance + new Vector3(0, Height, 0);
        Position = new Vector3(Position.X, Math.Max(1.35f, Position.Y), Position.Z);
        Forward = Vector3.Normalize(lookAt - Position);
        Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Forward));
        Up = Vector3.Normalize(Vector3.Cross(Forward, Right));
    }

    public bool TryProject(Size viewport, Vector3 point, out PointF screen, out float depth)
    {
        var rel = point - Position;
        var x = Vector3.Dot(rel, Right);
        var y = Vector3.Dot(rel, Up);
        var z = Vector3.Dot(rel, Forward);
        depth = z;
        if (z <= 0.08f)
        {
            screen = PointF.Empty;
            return false;
        }

        var focal = viewport.Height / (2f * MathF.Tan(70f * MathF.PI / 360f));
        screen = new PointF(
            viewport.Width * 0.5f + x / z * focal,
            viewport.Height * 0.54f - y / z * focal);
        return screen.X > -320 && screen.X < viewport.Width + 320 &&
               screen.Y > -320 && screen.Y < viewport.Height + 320;
    }

    public PointF Project(Size viewport, Vector3 point)
    {
        TryProject(viewport, point, out var screen, out _);
        return screen;
    }

    public static Vector3 ForwardFromYaw(float yawDegrees)
    {
        var radians = yawDegrees * MathF.PI / 180f;
        return Vector3.Normalize(new Vector3(MathF.Sin(radians), 0, MathF.Cos(radians)));
    }

    private static float NormalizeAngle(float degrees)
    {
        while (degrees > 180f)
        {
            degrees -= 360f;
        }

        while (degrees < -180f)
        {
            degrees += 360f;
        }

        return degrees;
    }
}
