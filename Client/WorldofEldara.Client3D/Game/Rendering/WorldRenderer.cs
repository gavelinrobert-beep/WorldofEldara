using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class WorldRenderer
{
    public void AddTerrainAndPaths(List<DrawCommand> commands, Size viewport, SceneData scene, WorldState state,
        TerrainSystem terrain, WorldCamera camera)
    {
        AddGround(commands, viewport, state, terrain, camera);
        AddPaths(commands, viewport, scene, terrain, camera);
    }

    public void RenderFrame(Graphics graphics, Size viewport, List<DrawCommand> commands, Action<Graphics> drawHud)
    {
        DrawBackground(graphics, viewport);

        foreach (var command in commands.OrderByDescending(command => command.Depth))
        {
            command.Draw(graphics);
        }

        DrawDepthHaze(graphics, viewport);
        drawHud(graphics);
    }

    private static void AddGround(List<DrawCommand> commands, Size viewport, WorldState state, TerrainSystem terrain,
        WorldCamera camera)
    {
        const int min = -42;
        const int max = 45;
        const int tileSize = 2;
        for (var z = min; z < max; z += tileSize)
        {
            for (var x = min; x < max; x += tileSize)
            {
                var distance = MathF.Sqrt((x - state.PlayerPosition.X) * (x - state.PlayerPosition.X) +
                                          (z - state.PlayerPosition.Z) * (z - state.PlayerPosition.Z));
                if (distance > 42f)
                {
                    continue;
                }

                var points = ProjectPolygon(camera, viewport, new[]
                {
                    terrain.GroundPoint(x, z),
                    terrain.GroundPoint(x + tileSize, z),
                    terrain.GroundPoint(x + tileSize, z + tileSize),
                    terrain.GroundPoint(x, z + tileSize)
                }, out var depth);
                if (points is null)
                {
                    continue;
                }

                var tileHeight = (terrain.HeightAt(x, z) + terrain.HeightAt(x + tileSize, z + tileSize)) * 0.5f;
                var smooth = MathF.Sin(x * 0.09f + z * 0.05f) * 2f;
                var farFade = Math.Clamp(distance / 42f, 0f, 1f);
                var green = 34 + (int)(smooth * 1.8f) + (int)(tileHeight * 14f) - (int)(farFade * 9f);
                var blue = 24 + (int)(tileHeight * 6f) - (int)(farFade * 5f);
                var color = Color.FromArgb(255, 5, Math.Clamp(green, 24, 50), Math.Clamp(blue, 18, 34));
                var edgeAlpha = distance < 24f ? 48 : distance < 34f ? 28 : 0;
                commands.Add(new DrawCommand(depth, g =>
                {
                    using var brush = new SolidBrush(color);
                    g.FillPolygon(brush, points);
                    if (edgeAlpha > 0)
                    {
                        using var pen = new Pen(Color.FromArgb(edgeAlpha, 80, 145, 118), 1f);
                        g.DrawPolygon(pen, points);
                    }
                }));
            }
        }
    }

    private static void AddPaths(List<DrawCommand> commands, Size viewport, SceneData scene, TerrainSystem terrain,
        WorldCamera camera)
    {
        foreach (var path in scene.Paths)
        {
            foreach (var point in path.Points)
            {
                AddGroundOval(commands, viewport, terrain, camera, point, path.Width * 0.5f, path.Width * 0.36f,
                    Shade(path.Color, 1.08f), Color.FromArgb(84, 130, 94, 58));
            }

            for (var i = 0; i + 1 < path.Points.Count; i++)
            {
                AddPathSegment(commands, viewport, terrain, camera, path, path.Points[i], path.Points[i + 1], i);
            }
        }
    }

    private static void AddPathSegment(List<DrawCommand> commands, Size viewport, TerrainSystem terrain,
        WorldCamera camera, PathRibbon path, Vector3 start, Vector3 end, int segmentIndex)
    {
        var flatDelta = new Vector3(end.X - start.X, 0, end.Z - start.Z);
        var length = flatDelta.Length();
        if (length <= 0.001f)
        {
            return;
        }

        var steps = Math.Max(3, (int)MathF.Ceiling(length / 0.75f));
        var direction = Vector3.Normalize(flatDelta);
        var normal = new Vector3(-direction.Z, 0, direction.X);
        for (var step = 0; step < steps; step++)
        {
            var t0 = step / (float)steps;
            var t1 = (step + 1) / (float)steps;
            var a = Vector3.Lerp(start, end, t0);
            var b = Vector3.Lerp(start, end, t1);
            var width0 = path.Width * (0.36f + Hash01(segmentIndex * 97 + step * 17) * 0.06f);
            var width1 = path.Width * (0.36f + Hash01(segmentIndex * 101 + step * 19) * 0.06f);
            var wobble0 = (Hash01(segmentIndex * 37 + step * 13) - 0.5f) * 0.1f;
            var wobble1 = (Hash01(segmentIndex * 41 + step * 11) - 0.5f) * 0.1f;
            var left0 = normal * (width0 + wobble0);
            var right0 = normal * (-width0 + wobble0 * 0.4f);
            var left1 = normal * (width1 + wobble1);
            var right1 = normal * (-width1 + wobble1 * 0.4f);
            var points = ProjectPolygon(camera, viewport, new[]
            {
                terrain.GroundPoint(a.X + left0.X, a.Z + left0.Z, a.Y),
                terrain.GroundPoint(b.X + left1.X, b.Z + left1.Z, b.Y),
                terrain.GroundPoint(b.X + right1.X, b.Z + right1.Z, b.Y),
                terrain.GroundPoint(a.X + right0.X, a.Z + right0.Z, a.Y)
            }, out var depth);
            if (points is null)
            {
                continue;
            }

            var shade = 0.86f + Hash01(segmentIndex * 53 + step * 29) * 0.1f;
            var fill = Fade(Shade(path.Color, shade * 1.18f), 214);
            commands.Add(new DrawCommand(depth - 0.02f, g =>
            {
                using var brush = new SolidBrush(fill);
                g.FillPolygon(brush, points);
                if (step % 3 == 0)
                {
                    using var edgePen = new Pen(Color.FromArgb(48, 158, 116, 72), 1f);
                    g.DrawPolygon(edgePen, points);
                }
            }));
        }
    }

    private static void AddGroundOval(List<DrawCommand> commands, Size viewport, TerrainSystem terrain,
        WorldCamera camera, Vector3 center, float radiusX, float radiusZ, Color fill, Color outline)
    {
        const int segments = 14;
        var vertices = new Vector3[segments];
        for (var i = 0; i < segments; i++)
        {
            var angle = i / (float)segments * MathF.PI * 2f;
            vertices[i] = terrain.GroundPoint(center.X + MathF.Cos(angle) * radiusX,
                center.Z + MathF.Sin(angle) * radiusZ, center.Y + 0.045f);
        }

        var points = ProjectPolygon(camera, viewport, vertices, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.035f, g =>
        {
            using var brush = new SolidBrush(fill);
            using var pen = new Pen(outline, 1f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }));
    }

    private static void DrawBackground(Graphics graphics, Size viewport)
    {
        using var background = new LinearGradientBrush(
            new Rectangle(Point.Empty, viewport),
            Color.FromArgb(9, 22, 23),
            Color.FromArgb(5, 13, 12),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(background, 0, 0, viewport.Width, viewport.Height);
        DrawHorizonFog(graphics, viewport);
    }

    private static void DrawHorizonFog(Graphics graphics, Size viewport)
    {
        var fogRect = new Rectangle(0, (int)(viewport.Height * 0.32f), viewport.Width, (int)(viewport.Height * 0.3f));
        using var fog = new LinearGradientBrush(
            fogRect,
            Color.FromArgb(0, 4, 9, 10),
            Color.FromArgb(70, 8, 24, 20),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(fog, fogRect);
    }

    private static void DrawDepthHaze(Graphics graphics, Size viewport)
    {
        var hazeRect = new Rectangle(0, 0, viewport.Width, (int)(viewport.Height * 0.55f));
        using var haze = new LinearGradientBrush(
            hazeRect,
            Color.FromArgb(38, 3, 9, 10),
            Color.FromArgb(0, 3, 9, 10),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(haze, hazeRect);
    }

    private static PointF[]? ProjectPolygon(WorldCamera camera, Size viewport, IReadOnlyList<Vector3> vertices,
        out float depth)
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

    private static float Hash01(int seed)
    {
        unchecked
        {
            var value = (uint)seed;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            return (value & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }

    private static Color Shade(Color color, float factor)
    {
        return Color.FromArgb(
            color.A,
            Math.Clamp((int)(color.R * factor), 0, 255),
            Math.Clamp((int)(color.G * factor), 0, 255),
            Math.Clamp((int)(color.B * factor), 0, 255));
    }

    private static Color Fade(Color color, int alpha) =>
        Color.FromArgb(Math.Clamp(alpha, 0, 255), color.R, color.G, color.B);
}
