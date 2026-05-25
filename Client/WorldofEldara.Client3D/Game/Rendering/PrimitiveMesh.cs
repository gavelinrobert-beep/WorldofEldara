using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public readonly record struct MeshFace(IReadOnlyList<Vector3> Vertices, float Shade);

public static class PrimitiveMesh
{
    public static IReadOnlyList<MeshFace> Box(Vector3 center, Vector3 size)
    {
        var hx = size.X * 0.5f;
        var hy = size.Y * 0.5f;
        var hz = size.Z * 0.5f;
        var corners = new[]
        {
            center + new Vector3(-hx, -hy, -hz),
            center + new Vector3(hx, -hy, -hz),
            center + new Vector3(hx, -hy, hz),
            center + new Vector3(-hx, -hy, hz),
            center + new Vector3(-hx, hy, -hz),
            center + new Vector3(hx, hy, -hz),
            center + new Vector3(hx, hy, hz),
            center + new Vector3(-hx, hy, hz)
        };

        return
        [
            new([corners[0], corners[1], corners[5], corners[4]], 0.82f),
            new([corners[1], corners[2], corners[6], corners[5]], 0.68f),
            new([corners[2], corners[3], corners[7], corners[6]], 0.76f),
            new([corners[3], corners[0], corners[4], corners[7]], 0.58f),
            new([corners[4], corners[5], corners[6], corners[7]], 1.12f)
        ];
    }

    public static IReadOnlyList<MeshFace> Cylinder(Vector3 center, float radius, float height, int sides)
    {
        sides = Math.Clamp(sides, 5, 12);
        var bottomY = center.Y - height * 0.5f;
        var topY = center.Y + height * 0.5f;
        var bottom = new Vector3[sides];
        var top = new Vector3[sides];
        for (var i = 0; i < sides; i++)
        {
            var angle = i / (float)sides * MathF.PI * 2f + MathF.PI / sides;
            var x = MathF.Cos(angle) * radius;
            var z = MathF.Sin(angle) * radius;
            bottom[i] = new Vector3(center.X + x, bottomY, center.Z + z);
            top[i] = new Vector3(center.X + x, topY, center.Z + z);
        }

        var faces = new List<MeshFace>(sides + 1);
        for (var i = 0; i < sides; i++)
        {
            var next = (i + 1) % sides;
            var shade = 0.64f + 0.34f * ((i % 3) / 2f);
            faces.Add(new MeshFace([bottom[i], bottom[next], top[next], top[i]], shade));
        }

        faces.Add(new MeshFace(top, 1.12f));
        return faces;
    }
}
