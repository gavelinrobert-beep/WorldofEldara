using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class MeshBuilder
{
    private readonly List<MeshPart> _parts = [];

    public void AddBox(Vector3 center, Vector3 size, MeshMaterial material, float yawDegrees = 0f)
    {
        foreach (var face in PrimitiveMesh.Box(center, size))
        {
            _parts.Add(new MeshPart(Rotate(face.Vertices, center, yawDegrees), face.Shade, material));
        }
    }

    public void AddCylinder(Vector3 center, float radius, float height, int sides, MeshMaterial material)
    {
        foreach (var face in PrimitiveMesh.Cylinder(center, radius, height, sides))
        {
            _parts.Add(new MeshPart(face.Vertices, face.Shade, material));
        }
    }

    public void AddDiamond(Vector3 center, Vector3 size, MeshMaterial material)
    {
        var hx = size.X * 0.5f;
        var hy = size.Y * 0.5f;
        var hz = size.Z * 0.5f;
        var top = center + new Vector3(0, hy, 0);
        var bottom = center - new Vector3(0, hy, 0);
        var north = center + new Vector3(0, 0, -hz);
        var east = center + new Vector3(hx, 0, 0);
        var south = center + new Vector3(0, 0, hz);
        var west = center + new Vector3(-hx, 0, 0);

        _parts.Add(new MeshPart([top, east, north], 1.16f, material));
        _parts.Add(new MeshPart([top, south, east], 1.02f, material));
        _parts.Add(new MeshPart([top, west, south], 0.86f, material));
        _parts.Add(new MeshPart([top, north, west], 0.94f, material));
        _parts.Add(new MeshPart([bottom, north, east], 0.68f, material));
        _parts.Add(new MeshPart([bottom, east, south], 0.72f, material));
        _parts.Add(new MeshPart([bottom, south, west], 0.58f, material));
        _parts.Add(new MeshPart([bottom, west, north], 0.64f, material));
    }

    public void AddGableRoof(Vector3 center, Vector3 size, MeshMaterial material)
    {
        var hx = size.X * 0.5f;
        var hy = size.Y * 0.5f;
        var hz = size.Z * 0.5f;
        var yBase = center.Y - hy;
        var yTop = center.Y + hy;
        var leftFront = new Vector3(center.X - hx, yBase, center.Z - hz);
        var rightFront = new Vector3(center.X + hx, yBase, center.Z - hz);
        var leftBack = new Vector3(center.X - hx, yBase, center.Z + hz);
        var rightBack = new Vector3(center.X + hx, yBase, center.Z + hz);
        var ridgeFront = new Vector3(center.X, yTop, center.Z - hz);
        var ridgeBack = new Vector3(center.X, yTop, center.Z + hz);

        _parts.Add(new MeshPart([leftFront, ridgeFront, ridgeBack, leftBack], 0.94f, material));
        _parts.Add(new MeshPart([ridgeFront, rightFront, rightBack, ridgeBack], 0.76f, material));
        _parts.Add(new MeshPart([leftFront, rightFront, ridgeFront], 1.04f, material));
        _parts.Add(new MeshPart([rightBack, leftBack, ridgeBack], 0.7f, material));
    }

    public void AddWedge(Vector3 center, Vector3 size, MeshMaterial material, float yawDegrees = 0f)
    {
        var hx = size.X * 0.5f;
        var hy = size.Y * 0.5f;
        var hz = size.Z * 0.5f;
        var bottomA = center + new Vector3(-hx, -hy, -hz);
        var bottomB = center + new Vector3(hx, -hy, -hz);
        var bottomC = center + new Vector3(hx, -hy, hz);
        var bottomD = center + new Vector3(-hx, -hy, hz);
        var topLowA = center + new Vector3(-hx, -hy * 0.1f, -hz);
        var topLowB = center + new Vector3(hx, -hy * 0.1f, -hz);
        var topHighC = center + new Vector3(hx, hy, hz);
        var topHighD = center + new Vector3(-hx, hy, hz);

        AddRotatedFace([bottomA, bottomB, bottomC, bottomD], center, yawDegrees, 0.62f, material);
        AddRotatedFace([topLowA, topLowB, topHighC, topHighD], center, yawDegrees, 1.04f, material);
        AddRotatedFace([bottomA, topLowA, topHighD, bottomD], center, yawDegrees, 0.72f, material);
        AddRotatedFace([bottomB, bottomC, topHighC, topLowB], center, yawDegrees, 0.82f, material);
        AddRotatedFace([bottomD, bottomC, topHighC, topHighD], center, yawDegrees, 0.68f, material);
    }

    public IReadOnlyList<MeshPart> ToArray() => _parts.ToArray();

    private static IReadOnlyList<Vector3> Rotate(IReadOnlyList<Vector3> vertices, Vector3 origin, float yawDegrees)
    {
        if (MathF.Abs(yawDegrees) <= 0.001f)
        {
            return vertices;
        }

        var radians = yawDegrees * MathF.PI / 180f;
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);
        var transformed = new Vector3[vertices.Count];
        for (var i = 0; i < vertices.Count; i++)
        {
            var local = vertices[i] - origin;
            transformed[i] = origin + new Vector3(
                local.X * cos + local.Z * sin,
                local.Y,
                -local.X * sin + local.Z * cos);
        }

        return transformed;
    }

    private void AddRotatedFace(IReadOnlyList<Vector3> vertices, Vector3 origin, float yawDegrees, float shade,
        MeshMaterial material)
    {
        _parts.Add(new MeshPart(Rotate(vertices, origin, yawDegrees), shade, material));
    }
}
