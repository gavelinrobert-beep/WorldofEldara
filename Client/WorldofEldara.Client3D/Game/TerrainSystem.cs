using System.Numerics;

namespace WorldofEldara.Client3D.Game;

public sealed class TerrainSystem
{
    public Vector3 AtGround(Vector3 point) => GroundPoint(point.X, point.Z, point.Y);

    public Vector3 GroundPoint(float x, float z, float yOffset = 0f) =>
        new(x, HeightAt(x, z) + yOffset, z);

    public float HeightAt(float x, float z)
    {
        var broad = MathF.Sin(x * 0.16f + z * 0.09f) * 0.22f;
        var roll = MathF.Cos(z * 0.18f - x * 0.07f) * 0.15f;
        var detail = MathF.Sin((x + z) * 0.42f) * 0.045f;
        return broad + roll + detail;
    }
}
