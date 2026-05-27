using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class WorldRenderer
{
    private const float NearClip = 0.12f;
    private const float FovDegrees = 64f;
    private const float ProjectionCenterY = 0.63f;
    private const float GroundTileSize = 1.5f;

    public void AddTerrainAndPaths(List<DrawCommand> commands, Size viewport, SceneData scene, WorldState state,
        TerrainSystem terrain, WorldCamera camera)
    {
        AddGround(commands, viewport, scene, state, terrain, camera);
        if (scene.Blockout.Placements.Count > 0)
        {
            AddBlockoutGroundMaterialPatches(commands, viewport, scene, terrain, camera);
        }
        else
        {
            AddGroundMaterialPatches(commands, viewport, scene, terrain, camera);
        }

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

    private static void AddGround(List<DrawCommand> commands, Size viewport, SceneData scene, WorldState state,
        TerrainSystem terrain, WorldCamera camera)
    {
        const float min = -48f;
        const float max = 51f;
        for (var z = min; z < max; z += GroundTileSize)
        {
            for (var x = min; x < max; x += GroundTileSize)
            {
                var distance = MathF.Sqrt((x - state.PlayerPosition.X) * (x - state.PlayerPosition.X) +
                                          (z - state.PlayerPosition.Z) * (z - state.PlayerPosition.Z));
                if (distance > 48f)
                {
                    continue;
                }

                var sampleX = x + GroundTileSize * 0.5f;
                var sampleZ = z + GroundTileSize * 0.5f;
                var terrainSample = terrain.Sample(scene, sampleX, sampleZ);
                if (terrainSample.Coverage <= 0.03f)
                {
                    continue;
                }

                var points = ProjectClippedPolygon(camera, viewport, new[]
                {
                    terrain.GroundPoint(x, z),
                    terrain.GroundPoint(x + GroundTileSize, z),
                    terrain.GroundPoint(x + GroundTileSize, z + GroundTileSize),
                    terrain.GroundPoint(x, z + GroundTileSize)
                }, out var depth);
                if (points is null)
                {
                    continue;
                }

                var color = TerrainColor(scene, terrain, terrainSample, sampleX, sampleZ, distance);
                commands.Add(new DrawCommand(depth, g =>
                {
                    using var brush = new SolidBrush(color);
                    g.FillPolygon(brush, points);
                }));
            }
        }
    }

    private static void AddGroundMaterialPatches(List<DrawCommand> commands, Size viewport, SceneData scene,
        TerrainSystem terrain, WorldCamera camera)
    {
        foreach (var prop in scene.Props)
        {
            switch (prop.Kind)
            {
                case "tree":
                    AddGroundOval(commands, viewport, terrain, camera, prop.Position, prop.Width * 1.35f,
                        prop.Width * 0.68f, Color.FromArgb(66, 22, 80, 42), Color.FromArgb(18, 82, 142, 78));
                    break;
                case "shrub":
                    AddGroundOval(commands, viewport, terrain, camera, prop.Position, prop.Width * 0.94f,
                        prop.Width * 0.42f, Color.FromArgb(62, 30, 96, 50), Color.FromArgb(16, 92, 162, 88));
                    break;
                case "worldroot":
                    AddGroundOval(commands, viewport, terrain, camera, prop.Position, 3.2f, 2.1f,
                        Color.FromArgb(72, 42, 126, 72), Color.FromArgb(56, 114, 238, 178));
                    break;
                case "cottage":
                    AddGroundOval(commands, viewport, terrain, camera, prop.Position + new Vector3(0.3f, 0, -0.4f),
                        3.3f, 2.2f, Color.FromArgb(72, 74, 54, 34), Color.FromArgb(18, 154, 108, 68));
                    break;
            }
        }
    }

    private static void AddBlockoutGroundMaterialPatches(List<DrawCommand> commands, Size viewport, SceneData scene,
        TerrainSystem terrain, WorldCamera camera)
    {
        foreach (var placement in scene.Blockout.Placements)
        {
            var center = placement.WorldFlat;
            switch (placement.Kind)
            {
                case "plaza":
                    AddGroundOval(commands, viewport, terrain, camera, center, 3.8f * placement.ScaleAt(0),
                        2.7f * placement.ScaleAt(1), Color.FromArgb(96, 50, 112, 70),
                        Color.FromArgb(54, 124, 226, 170));
                    break;
                case "clearing":
                case "terrace":
                    AddGroundOval(commands, viewport, terrain, camera, center, 3.7f * placement.ScaleAt(0),
                        2.3f * placement.ScaleAt(1), Color.FromArgb(78, 26, 90, 48),
                        Color.FromArgb(24, 104, 184, 96));
                    break;
                case "garden":
                    AddGroundOval(commands, viewport, terrain, camera, center, 3.2f * placement.ScaleAt(0),
                        2.2f * placement.ScaleAt(1), Color.FromArgb(84, 74, 32, 94),
                        Color.FromArgb(44, 220, 84, 232));
                    break;
                case "water_pool":
                    AddGroundOval(commands, viewport, terrain, camera, center, 2.35f * placement.ScaleAt(0),
                        1.35f * placement.ScaleAt(1), Color.FromArgb(106, 42, 118, 146),
                        Color.FromArgb(74, 112, 214, 232));
                    break;
                case "path":
                case "stairs":
                    AddGroundOval(commands, viewport, terrain, camera, center, 1.55f * placement.ScaleAt(0),
                        0.72f * placement.ScaleAt(1), Color.FromArgb(72, 102, 70, 42),
                        Color.FromArgb(24, 176, 128, 78), placement.YawDegrees);
                    break;
                case "root_bridge":
                    AddGroundOval(commands, viewport, terrain, camera, center, 1.8f * placement.ScaleAt(0),
                        0.92f * placement.ScaleAt(1), Color.FromArgb(76, 90, 58, 34),
                        Color.FromArgb(28, 176, 128, 78), placement.YawDegrees);
                    break;
                case "tree" when placement.Name.EndsWith("_trunk", StringComparison.Ordinal):
                    AddGroundOval(commands, viewport, terrain, camera, center, 1.25f * placement.ScaleAt(0),
                        0.78f * placement.ScaleAt(0), Color.FromArgb(62, 24, 74, 38),
                        Color.FromArgb(16, 96, 150, 80));
                    break;
                case "kiosk" when placement.Name.EndsWith("_body", StringComparison.Ordinal):
                    AddGroundOval(commands, viewport, terrain, camera, center, 2.2f * placement.ScaleAt(0),
                        1.5f * placement.ScaleAt(1), Color.FromArgb(70, 72, 48, 30),
                        Color.FromArgb(22, 160, 112, 72));
                    break;
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
                AddPathRunes(commands, viewport, terrain, camera, path.Points[i], path.Points[i + 1], i);
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
            var points = ProjectClippedPolygon(camera, viewport, new[]
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
            var nearFade = Math.Clamp((depth - 1.4f) / 5.2f, 0.28f, 1f);
            var fill = Fade(Shade(path.Color, shade * 1.1f), (int)(92 + nearFade * 58f));
            commands.Add(new DrawCommand(depth - 0.02f, g =>
            {
                using var brush = new SolidBrush(fill);
                g.FillPolygon(brush, points);
                if (step % 3 == 0 && depth > 4.2f)
                {
                    using var edgePen = new Pen(Color.FromArgb(24, 178, 128, 78), 1f);
                    g.DrawPolygon(edgePen, points);
                }
            }));
        }
    }

    private static void AddPathRunes(List<DrawCommand> commands, Size viewport, TerrainSystem terrain,
        WorldCamera camera, Vector3 start, Vector3 end, int segmentIndex)
    {
        var flatDelta = new Vector3(end.X - start.X, 0, end.Z - start.Z);
        var length = flatDelta.Length();
        if (length <= 2.5f)
        {
            return;
        }

        var direction = Vector3.Normalize(flatDelta);
        var normal = new Vector3(-direction.Z, 0, direction.X);
        var runeCount = Math.Max(1, (int)(length / 4.6f));
        for (var rune = 0; rune < runeCount; rune++)
        {
            var t = (rune + 0.55f) / (runeCount + 0.2f);
            var side = rune % 2 == 0 ? 1f : -1f;
            var center = Vector3.Lerp(start, end, t) + normal * side * (0.42f + Hash01(segmentIndex * 211 + rune * 31) * 0.32f);
            var size = 0.28f + Hash01(segmentIndex * 173 + rune * 19) * 0.08f;
            var points = ProjectClippedPolygon(camera, viewport, new[]
            {
                terrain.GroundPoint(center.X, center.Z - size, 0.075f),
                terrain.GroundPoint(center.X + size * 0.7f, center.Z, 0.075f),
                terrain.GroundPoint(center.X, center.Z + size, 0.075f),
                terrain.GroundPoint(center.X - size * 0.7f, center.Z, 0.075f)
            }, out var depth);
            if (points is null)
            {
                continue;
            }

            commands.Add(new DrawCommand(depth - 0.06f, g =>
            {
                using var brush = new SolidBrush(Color.FromArgb(84, 94, 232, 194));
                using var pen = new Pen(Color.FromArgb(112, 172, 255, 220), 1f);
                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
            }));
        }
    }

    private static void AddGroundOval(List<DrawCommand> commands, Size viewport, TerrainSystem terrain,
        WorldCamera camera, Vector3 center, float radiusX, float radiusZ, Color fill, Color outline,
        float yawDegrees = 0f)
    {
        const int segments = 14;
        var vertices = new Vector3[segments];
        var yaw = yawDegrees * MathF.PI / 180f;
        var axisX = new Vector3(MathF.Cos(yaw), 0, MathF.Sin(yaw));
        var axisZ = new Vector3(-MathF.Sin(yaw), 0, MathF.Cos(yaw));
        for (var i = 0; i < segments; i++)
        {
            var angle = i / (float)segments * MathF.PI * 2f;
            var offset = axisX * (MathF.Cos(angle) * radiusX) + axisZ * (MathF.Sin(angle) * radiusZ);
            vertices[i] = terrain.GroundPoint(center.X + offset.X, center.Z + offset.Z, center.Y + 0.045f);
        }

        var points = ProjectClippedPolygon(camera, viewport, vertices, out var depth);
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
            Color.FromArgb(12, 29, 30),
            Color.FromArgb(4, 12, 11),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(background, 0, 0, viewport.Width, viewport.Height);
        DrawGroundWash(graphics, viewport);
        DrawHorizonFog(graphics, viewport);
    }

    private static void DrawGroundWash(Graphics graphics, Size viewport)
    {
        var horizon = (int)(viewport.Height * 0.5f);
        var groundRect = new Rectangle(0, horizon, viewport.Width, viewport.Height - horizon);
        using var ground = new LinearGradientBrush(
            groundRect,
            Color.FromArgb(32, 12, 42, 32),
            Color.FromArgb(178, 6, 18, 16),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(ground, groundRect);

        using var pathBrush = new LinearGradientBrush(
            groundRect,
            Color.FromArgb(24, 78, 54, 34),
            Color.FromArgb(54, 56, 40, 26),
            LinearGradientMode.Vertical);
        var path = new[]
        {
            new PointF(viewport.Width * 0.38f, viewport.Height),
            new PointF(viewport.Width * 0.47f, horizon + 28f),
            new PointF(viewport.Width * 0.56f, horizon + 18f),
            new PointF(viewport.Width * 0.71f, viewport.Height)
        };
        graphics.FillPolygon(pathBrush, path);

        using var mossBrush = new SolidBrush(Color.FromArgb(24, 24, 70, 38));
        graphics.FillEllipse(mossBrush, viewport.Width * 0.06f, horizon + 45f, viewport.Width * 0.32f, viewport.Height * 0.32f);
        graphics.FillEllipse(mossBrush, viewport.Width * 0.62f, horizon + 20f, viewport.Width * 0.38f, viewport.Height * 0.38f);
    }

    private static void DrawHorizonFog(Graphics graphics, Size viewport)
    {
        var fogRect = new Rectangle(0, (int)(viewport.Height * 0.32f), viewport.Width, (int)(viewport.Height * 0.3f));
        using var fog = new LinearGradientBrush(
            fogRect,
            Color.FromArgb(0, 4, 9, 10),
            Color.FromArgb(58, 18, 42, 34),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(fog, fogRect);
    }

    private static void DrawDepthHaze(Graphics graphics, Size viewport)
    {
        var hazeRect = new Rectangle(0, 0, viewport.Width, (int)(viewport.Height * 0.55f));
        using var haze = new LinearGradientBrush(
            hazeRect,
            Color.FromArgb(26, 3, 9, 10),
            Color.FromArgb(0, 3, 9, 10),
            LinearGradientMode.Vertical);
        graphics.FillRectangle(haze, hazeRect);
    }

    private static PointF[]? ProjectClippedPolygon(WorldCamera camera, Size viewport, IReadOnlyList<Vector3> vertices,
        out float depth)
    {
        var cameraVertices = new List<CameraVertex>(vertices.Count + 2);
        foreach (var vertex in vertices)
        {
            var relative = vertex - camera.Position;
            cameraVertices.Add(new CameraVertex(
                Vector3.Dot(relative, camera.Right),
                Vector3.Dot(relative, camera.Up),
                Vector3.Dot(relative, camera.Forward)));
        }

        var clipped = ClipNear(cameraVertices);
        if (clipped.Count < 3)
        {
            depth = 0f;
            return null;
        }

        var points = new PointF[clipped.Count];
        depth = 0f;
        var focal = viewport.Height / (2f * MathF.Tan(FovDegrees * MathF.PI / 360f));
        for (var i = 0; i < clipped.Count; i++)
        {
            var vertex = clipped[i];
            points[i] = new PointF(
                viewport.Width * 0.5f + vertex.X / vertex.Z * focal,
                viewport.Height * ProjectionCenterY - vertex.Y / vertex.Z * focal);
            depth += vertex.Z;
        }

        depth /= clipped.Count;
        return points;
    }

    private static List<CameraVertex> ClipNear(IReadOnlyList<CameraVertex> vertices)
    {
        var output = new List<CameraVertex>(vertices.Count + 2);
        for (var i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var previous = vertices[(i + vertices.Count - 1) % vertices.Count];
            var currentInside = current.Z >= NearClip;
            var previousInside = previous.Z >= NearClip;

            if (currentInside && !previousInside)
            {
                output.Add(IntersectNear(previous, current));
            }

            if (currentInside)
            {
                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(IntersectNear(previous, current));
            }
        }

        return output;
    }

    private static CameraVertex IntersectNear(CameraVertex from, CameraVertex to)
    {
        var t = (NearClip - from.Z) / (to.Z - from.Z);
        return new CameraVertex(
            from.X + (to.X - from.X) * t,
            from.Y + (to.Y - from.Y) * t,
            NearClip);
    }

    private readonly record struct CameraVertex(float X, float Y, float Z);

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

    private static float PathInfluence(SceneData scene, float x, float z)
    {
        var nearest = float.MaxValue;
        foreach (var path in scene.Paths)
        {
            for (var i = 0; i + 1 < path.Points.Count; i++)
            {
                nearest = MathF.Min(nearest, DistanceToSegment2D(x, z, path.Points[i], path.Points[i + 1]) - path.Width * 0.38f);
            }
        }

        return Math.Clamp(1f - nearest / 3.4f, 0f, 1f);
    }

    private static float PropInfluence(SceneData scene, float x, float z, string kind, float radius)
    {
        var influence = 0f;
        foreach (var prop in scene.Props)
        {
            if (prop.Kind != kind)
            {
                continue;
            }

            var dx = x - prop.Position.X;
            var dz = z - prop.Position.Z;
            var distance = MathF.Sqrt(dx * dx + dz * dz);
            influence = MathF.Max(influence, Math.Clamp(1f - distance / radius, 0f, 1f));
        }

        return influence;
    }

    private static float DistanceToSegment2D(float x, float z, Vector3 a, Vector3 b)
    {
        var abX = b.X - a.X;
        var abZ = b.Z - a.Z;
        var lengthSquared = abX * abX + abZ * abZ;
        if (lengthSquared <= 0.0001f)
        {
            var pointDx = x - a.X;
            var pointDz = z - a.Z;
            return MathF.Sqrt(pointDx * pointDx + pointDz * pointDz);
        }

        var t = Math.Clamp(((x - a.X) * abX + (z - a.Z) * abZ) / lengthSquared, 0f, 1f);
        var closestX = a.X + abX * t;
        var closestZ = a.Z + abZ * t;
        var dx = x - closestX;
        var dz = z - closestZ;
        return MathF.Sqrt(dx * dx + dz * dz);
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

    private static Color TerrainColor(SceneData scene, TerrainSystem terrain, TerrainSample sample, float x, float z,
        float distance)
    {
        var pathBlend = MathF.Max(sample.Path, PathInfluence(scene, x, z) * 0.72f);
        var waterBlend = MathF.Max(sample.Water, PropInfluence(scene, x, z, "water", 5.6f));
        var scarBlend = MathF.Max(sample.Corruption, PropInfluence(scene, x, z, "scar", 5.2f));
        var shrineBlend = MathF.Max(sample.Shrine, PropInfluence(scene, x, z, "worldroot", 6.8f));
        var canopyBlend = PropInfluence(scene, x, z, "tree", 4.4f);
        var height = terrain.HeightAt(x, z);
        var clearing = MathF.Max(sample.Clearing, MathF.Max(0f, 1f - distance / 22f) * 0.32f);
        var farFade = Math.Clamp(distance / 50f, 0f, 1f);
        var broadMoss = MathF.Sin(x * 0.08f + z * 0.05f) * 0.5f + MathF.Cos(z * 0.09f - x * 0.04f) * 0.5f;
        var fineMoss = MathF.Sin((x + z) * 0.24f) * 0.5f + MathF.Cos((x - z) * 0.2f) * 0.5f;

        var red = 20f + clearing * 6f + pathBlend * 16f + scarBlend * 20f + shrineBlend * 4f -
                  waterBlend * 2f - canopyBlend * 1.2f;
        var green = 50f + height * 5f + broadMoss * 2.1f + fineMoss * 0.8f + clearing * 6f +
                    pathBlend * 5f - scarBlend * 9f + waterBlend * 7f + shrineBlend * 8f - farFade * 2f;
        var blue = 36f + height * 3f + broadMoss * 1.4f + clearing * 2f - pathBlend * 1f +
                   scarBlend * 16f + waterBlend * 25f + shrineBlend * 7f - farFade * 1.5f;
        var alpha = Math.Clamp((int)(235f + sample.Coverage * 20f), 226, 255);

        return Color.FromArgb(
            alpha,
            Math.Clamp((int)red, 8, 64),
            Math.Clamp((int)green, 30, 68),
            Math.Clamp((int)blue, 20, 74));
    }
}
