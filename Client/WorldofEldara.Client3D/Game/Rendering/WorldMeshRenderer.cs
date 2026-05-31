using System.Drawing;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class WorldMeshRenderer(WorldCamera camera)
{
    public void AddMesh(List<DrawCommand> commands, Size viewport, MeshInstance instance)
    {
        foreach (var part in instance.Asset.Parts)
        {
            var vertices = new Vector3[part.Vertices.Count];
            for (var i = 0; i < part.Vertices.Count; i++)
            {
                vertices[i] = Transform(part.Vertices[i], instance);
            }

            var points = ProjectPolygon(viewport, vertices, out var depth);
            if (points is null)
            {
                continue;
            }

            var fill = Shade(part.Material.Fill, part.Shade);
            var outline = part.Material.Outline;
            commands.Add(new DrawCommand(depth, g =>
            {
                if (IsGlowMaterial(part.Material))
                {
                    using var glowPen = new Pen(Color.FromArgb(78, outline.R, outline.G, outline.B), 5f)
                    {
                        LineJoin = System.Drawing.Drawing2D.LineJoin.Round
                    };
                    g.DrawPolygon(glowPen, points);
                }

                using var brush = new SolidBrush(fill);
                using var pen = new Pen(outline, 1.15f);
                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
            }));
        }
    }

    private static bool IsGlowMaterial(MeshMaterial material) =>
        material.Name.Contains("glow", StringComparison.OrdinalIgnoreCase) ||
        material.Name.Contains("spirit", StringComparison.OrdinalIgnoreCase) ||
        material.Name.Contains("echo", StringComparison.OrdinalIgnoreCase);

    private static Vector3 Transform(Vector3 vertex, MeshInstance instance)
    {
        var scaled = new Vector3(
            vertex.X * instance.Scale.X,
            vertex.Y * instance.Scale.Y,
            vertex.Z * instance.Scale.Z);
        var radians = instance.YawDegrees * MathF.PI / 180f;
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);
        var rotated = new Vector3(
            scaled.X * cos + scaled.Z * sin,
            scaled.Y,
            -scaled.X * sin + scaled.Z * cos);
        return instance.Position + rotated;
    }

    private PointF[]? ProjectPolygon(Size viewport, IReadOnlyList<Vector3> vertices, out float depth)
    {
        var points = new PointF[vertices.Count];
        depth = 0f;
        for (var i = 0; i < vertices.Count; i++)
        {
            if (!camera.TryProject(viewport, vertices[i], out points[i], out var pointDepth))
            {
                return null;
            }

            depth += pointDepth;
        }

        depth /= vertices.Count;
        return points;
    }

    private static Color Shade(Color color, float factor)
    {
        return Color.FromArgb(
            color.A,
            Math.Clamp((int)(color.R * factor), 0, 255),
            Math.Clamp((int)(color.G * factor), 0, 255),
            Math.Clamp((int)(color.B * factor), 0, 255));
    }
}
