using System.Drawing;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public sealed class WorldObjectRenderer(TerrainSystem terrain, WorldState state, WorldCamera camera)
{
    private const int FirstPruningRequiredKills = 3;
    private readonly WorldMeshRenderer _meshRenderer = new(camera);

    public void AddSceneObjects(List<DrawCommand> commands, Size viewport, SceneData scene, float pulse)
    {
        if (scene.Blockout.Placements.Count > 0)
        {
            AddBlockoutScene(commands, viewport, scene);
        }
        else
        {
            AddProps(commands, viewport, scene);
        }

        AddActors(commands, viewport, scene, pulse);
        AddPlayer(commands, viewport);
        AddStrikeFlash(commands, viewport);
        AddCombatTexts(commands, viewport);
    }

    public Vector3 GetActorWorldPosition(WorldActor actor, float pulse)
    {
        var bob = MathF.Sin(pulse * actor.Speed + actor.Phase) * 0.08f;
        var runtime = state.ActorRuntimePositions.GetValueOrDefault(actor.Name, actor.Position);
        var basePosition = new Vector3(runtime.X, 0, runtime.Z);
        if (!state.AggroActors.Contains(actor.Name) && state.ActorHealth.GetValueOrDefault(actor.Name, 100f) > 0f)
        {
            basePosition += new Vector3(MathF.Sin(pulse * 0.35f + actor.Phase) * actor.Wander, 0, 0);
        }

        return terrain.AtGround(basePosition) + new Vector3(0, bob, 0);
    }

    private void AddProps(List<DrawCommand> commands, Size viewport, SceneData scene)
    {
        foreach (var prop in scene.Props)
        {
            if (Vector3.Distance(prop.Position, state.PlayerPosition) > 34f)
            {
                continue;
            }

            var basePosition = terrain.AtGround(prop.Position);
            switch (prop.Kind)
            {
                case "tree":
                    AddTreeMesh(commands, viewport, prop);
                    break;
                case "water":
                case "scar":
                    AddGroundPatch(commands, viewport, prop);
                    break;
                case "runestone":
                    AddRunestoneMesh(commands, viewport, prop, basePosition);
                    break;
                case "cottage":
                    AddCottageMesh(commands, viewport, prop, basePosition);
                    break;
                case "fence":
                    AddFenceMesh(commands, viewport, prop, basePosition);
                    break;
                case "stump":
                    AddStumpMesh(commands, viewport, prop, basePosition);
                    break;
                case "herb":
                    AddHerbMesh(commands, viewport, prop, basePosition);
                    break;
                case "corruption":
                    AddCorruptionMesh(commands, viewport, prop, basePosition);
                    break;
                case "worldroot":
                    AddWorldrootShrineMesh(commands, viewport, prop, basePosition);
                    break;
                case "banner":
                    AddBannerMesh(commands, viewport, prop, basePosition);
                    break;
                case "lantern":
                    AddLanternMesh(commands, viewport, prop, basePosition);
                    break;
                case "shrub":
                    AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, 0.34f, 0),
                        prop.Width, prop.Height, prop.Fill, prop.Outline, null);
                    break;
                case "mushroom":
                    AddBox(commands, viewport, basePosition + new Vector3(0, 0.2f, 0),
                        new Vector3(prop.Width * 0.2f, prop.Height * 0.55f, prop.Width * 0.18f),
                        Color.FromArgb(214, 218, 184), Color.FromArgb(232, 236, 202), null);
                    AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, prop.Height * 0.55f, 0),
                        prop.Width, prop.Height * 0.42f, prop.Fill, prop.Outline, null);
                    break;
                case "rock":
                    AddRockMesh(commands, viewport, prop, basePosition);
                    break;
                case "rootgate":
                    AddRootArchMesh(commands, viewport, prop, basePosition);
                    break;
                default:
                    AddBillboard(commands, viewport, basePosition, prop.Width, prop.Height, prop.Fill, prop.Outline, prop.Kind);
                    break;
            }
        }
    }

    private void AddBlockoutScene(List<DrawCommand> commands, Size viewport, SceneData scene)
    {
        foreach (var placement in scene.Blockout.Placements)
        {
            if (!ShouldRenderBlockout(placement))
            {
                continue;
            }

            var flat = placement.WorldFlat;
            if (Vector3.Distance(flat, new Vector3(state.PlayerPosition.X, 0, state.PlayerPosition.Z)) > 44f)
            {
                continue;
            }

            var basePosition = terrain.AtGround(flat) + new Vector3(0, placement.HeightOffset, 0);
            AddBlockoutPlacement(commands, viewport, placement, basePosition);
        }
    }

    private void AddBlockoutPlacement(List<DrawCommand> commands, Size viewport, BlockoutPlacement placement,
        Vector3 basePosition)
    {
        var yaw = placement.YawDegrees;
        switch (placement.Kind)
        {
            case "plaza":
            case "clearing":
            case "terrace":
            case "garden":
                AddBlockoutDisc(commands, viewport, basePosition, 1.9f * placement.ScaleAt(0),
                    placement.Kind == "plaza"
                        ? Color.FromArgb(142, 114, 132, 92)
                        : Color.FromArgb(128, 34, 92, 48));
                break;
            case "path":
            case "stairs":
            case "root_bridge":
                AddBlockoutSlab(commands, viewport, basePosition, placement.Kind == "stairs"
                    ? new Vector3(0.9f, 0.08f, 0.28f)
                    : new Vector3(1.6f, 0.05f, 0.42f), yaw,
                    placement.Kind == "root_bridge"
                        ? Color.FromArgb(158, 98, 62, 34)
                        : Color.FromArgb(148, 118, 78, 48));
                break;
            case "rune_ring":
            case "rune_spoke":
                AddBlockoutSlab(commands, viewport, basePosition, new Vector3(0.76f, 0.035f, 0.08f), yaw,
                    Color.FromArgb(170, 96, 244, 210));
                break;
            case "landmark":
            case "landmark_root":
                AddCylinder(commands, viewport, basePosition + new Vector3(0, 0.76f, 0), 0.2f, 1.52f, 7,
                    Color.FromArgb(112, 84, 54, 32), Color.FromArgb(116, 170, 112, 62), null);
                break;
            case "landmark_crystal":
            case "ground_crystal":
            case "crystal_lantern":
                AddBlockoutCrystal(commands, viewport, basePosition, placement.Kind == "landmark_crystal" ? 0.64f : 0.28f);
                break;
            case "root_arch":
            case "bridge_post":
                AddCylinder(commands, viewport, basePosition + new Vector3(0, 0.58f, 0), 0.055f, 1.16f, 6,
                    Color.FromArgb(104, 78, 48, 28), Color.FromArgb(118, 152, 96, 54), null);
                break;
            case "waterfall":
            case "water_pool":
                AddBlockoutSlab(commands, viewport, basePosition, new Vector3(0.72f, 0.035f, 0.3f), yaw,
                    Color.FromArgb(154, 58, 150, 198));
                break;
            case "tree":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.HeartwoodTree, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.82f, 1.05f, 0.82f), yaw));
                break;
            case "banner":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.ThornveilBanner, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.72f, 0.9f, 0.72f), yaw));
                break;
            case "lantern":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.Lantern, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.52f, 0.88f, 0.52f), yaw));
                break;
            case "kiosk":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.ThornveilCottage, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.88f, 0.78f, 0.88f), yaw));
                break;
            case "small_shrine":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.ThornveilCottage, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.58f, 0.58f, 0.58f), yaw));
                break;
            case "imported_azure_tree":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.WorldrootShrine, terrain.AtGround(placement.WorldFlat),
                        new Vector3(1.2f, 1.35f, 1.2f), yaw));
                break;
            case "imported_signpost":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.RunestoneShard, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.54f, 0.92f, 0.54f), yaw));
                break;
            case "imported_glowcap":
                _meshRenderer.AddMesh(commands, viewport,
                    new MeshInstance(ThornveilMeshes.HerbCluster, terrain.AtGround(placement.WorldFlat),
                        new Vector3(0.82f, 0.62f, 0.82f), yaw));
                break;
        }
    }

    private static bool ShouldRenderBlockout(BlockoutPlacement placement)
    {
        if (placement.Kind == "tree")
        {
            return placement.Name.EndsWith("_trunk", StringComparison.Ordinal);
        }

        if (placement.Kind == "banner")
        {
            return placement.Name.EndsWith("_pole", StringComparison.Ordinal);
        }

        if (placement.Kind == "kiosk" || placement.Kind == "small_shrine")
        {
            return placement.Name.EndsWith("_body", StringComparison.Ordinal);
        }

        if (placement.Kind == "root_arch")
        {
            return placement.Name.EndsWith("_left_root", StringComparison.Ordinal) ||
                   placement.Name.EndsWith("_right_root", StringComparison.Ordinal) ||
                   placement.Name.EndsWith("_top_root", StringComparison.Ordinal);
        }

        if (placement.Kind is "tree_canopy" or "tree_root" or "flower" or "banner_sigil" or
            "kiosk_roof" or "small_shrine_roof" or "npc_marker" or "player_marker" or
            "deer_placeholder")
        {
            return false;
        }

        return placement.Kind is "plaza" or "clearing" or "terrace" or "garden" or
            "path" or "stairs" or "root_bridge" or "rune_ring" or "rune_spoke" or
            "landmark" or "landmark_root" or "landmark_crystal" or "bridge_post" or
            "crystal_lantern" or "ground_crystal" or "waterfall" or "water_pool" or
            "lantern" or "imported_azure_tree" or "imported_signpost" or "imported_glowcap";
    }

    private void AddBlockoutDisc(List<DrawCommand> commands, Size viewport, Vector3 center, float radius, Color color)
    {
        const int segments = 16;
        var vertices = new Vector3[segments];
        var yOffset = GroundOffset(center) + 0.025f;
        for (var i = 0; i < segments; i++)
        {
            var angle = i / (float)segments * MathF.PI * 2f;
            vertices[i] = terrain.GroundPoint(center.X + MathF.Cos(angle) * radius,
                center.Z + MathF.Sin(angle) * radius, yOffset);
        }

        var points = ProjectPolygon(viewport, vertices, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.07f, g =>
        {
            using var brush = new SolidBrush(color);
            using var pen = new Pen(Color.FromArgb(54, 180, 236, 188), 1f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }));
    }

    private void AddBlockoutSlab(List<DrawCommand> commands, Size viewport, Vector3 center, Vector3 size,
        float yawDegrees, Color color)
    {
        var yaw = yawDegrees * MathF.PI / 180f;
        var forward = new Vector3(MathF.Cos(yaw), 0, MathF.Sin(yaw));
        var right = new Vector3(-forward.Z, 0, forward.X);
        var halfForward = forward * size.X;
        var halfRight = right * size.Z;
        var yOffset = GroundOffset(center) + MathF.Max(0.025f, size.Y);
        var vertices = new[]
        {
            terrain.GroundPoint(center.X + halfForward.X + halfRight.X, center.Z + halfForward.Z + halfRight.Z, yOffset),
            terrain.GroundPoint(center.X - halfForward.X + halfRight.X, center.Z - halfForward.Z + halfRight.Z, yOffset),
            terrain.GroundPoint(center.X - halfForward.X - halfRight.X, center.Z - halfForward.Z - halfRight.Z, yOffset),
            terrain.GroundPoint(center.X + halfForward.X - halfRight.X, center.Z + halfForward.Z - halfRight.Z, yOffset)
        };

        var points = ProjectPolygon(viewport, vertices, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.08f, g =>
        {
            using var brush = new SolidBrush(color);
            using var pen = new Pen(Color.FromArgb(48, 226, 246, 198), 1f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }));
    }

    private void AddBlockoutCrystal(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float scale)
    {
        AddBox(commands, viewport, basePosition + new Vector3(0, scale * 0.72f, 0),
            new Vector3(scale * 0.44f, scale * 1.18f, scale * 0.44f),
            Color.FromArgb(122, 74, 226, 218), Color.FromArgb(170, 180, 255, 232), null);
    }

    private float GroundOffset(Vector3 worldPosition) => worldPosition.Y - terrain.HeightAt(worldPosition.X, worldPosition.Z);

    private void AddActors(List<DrawCommand> commands, Size viewport, SceneData scene, float pulse)
    {
        state.ActorPositions.Clear();
        foreach (var actor in scene.Actors)
        {
            var position = GetActorWorldPosition(actor, pulse);
            state.ActorPositions[actor.Name] = position;
            var health = state.ActorHealth.GetValueOrDefault(actor.Name, 100f);
            if (state.TargetName == actor.Name)
            {
                AddSelectionRing(commands, viewport, position, actor.Width * 0.74f,
                    actor.Hostile ? Color.FromArgb(228, 236, 88, 70) : Color.FromArgb(228, 238, 214, 92));
            }
            else if (actor.Hostile && state.AggroActors.Contains(actor.Name))
            {
                AddSelectionRing(commands, viewport, position, actor.Width * 0.68f,
                    Color.FromArgb(150, 246, 72, 54));
            }

            var fill = health <= 0f ? Fade(Shade(actor.Fill, 0.46f), 142) : actor.Fill;
            var outline = health <= 0f ? Fade(actor.Outline, 132) : actor.Outline;
            if (state.ActorHitFlash.GetValueOrDefault(actor.Name) > 0f)
            {
                fill = Color.FromArgb(fill.A, Math.Min(255, fill.R + 76), Math.Min(255, fill.G + 64), Math.Min(255, fill.B + 36));
                outline = Color.FromArgb(outline.A, 255, 236, 164);
            }

            var height = health <= 0f ? actor.Height * 0.54f : actor.Height;
            var basePosition = health <= 0f ? position + new Vector3(0.12f, 0, 0.04f) : position;
            var facing = FlatDirection(state.PlayerPosition - position, WorldCamera.ForwardFromYaw(state.PlayerYaw));
            AddCharacter(commands, viewport, basePosition, actor.Width, height, fill, outline, actor.Name, facing,
                actor.Hostile);
            AddInteractionMarker(commands, viewport, actor, position, pulse);
            AddActorHud(commands, viewport, actor, position, health);
        }
    }

    private void AddPlayer(List<DrawCommand> commands, Size viewport)
    {
        var facing = WorldCamera.ForwardFromYaw(state.PlayerYaw);
        var fill = state.PlayerDefeated
            ? Color.FromArgb(144, 86, 118, 98)
            : state.PlayerHitFlash > 0f ? Color.FromArgb(164, 232, 136, 118) : Color.FromArgb(118, 184, 136);
        var outline = state.PlayerDefeated ? Color.FromArgb(150, 164, 190, 164) : Color.FromArgb(210, 226, 242, 202);
        AddCharacter(commands, viewport, state.PlayerPosition, 0.62f, state.PlayerDefeated ? 0.82f : 1.35f, fill,
            outline, "Warden", facing, false);
        var weaponBase = state.PlayerPosition + new Vector3(0.22f, 0.85f, 0);
        var weaponTip = weaponBase + facing * 0.7f + new Vector3(0, 0.18f, 0);
        if (TryProject(viewport, weaponBase, out var a, out var da) &&
            TryProject(viewport, weaponTip, out var b, out var db))
        {
            commands.Add(new DrawCommand((da + db) * 0.5f - 0.1f, g =>
            {
                using var pen = new Pen(Color.FromArgb(230, 218, 202, 142), 3f);
                g.DrawLine(pen, a, b);
            }));
        }
    }

    private void AddTree(List<DrawCommand> commands, Size viewport, WorldProp prop)
    {
        var basePosition = terrain.AtGround(prop.Position);
        var cameraDistance = Vector3.Distance(basePosition, camera.Position);
        if (cameraDistance < 3.4f)
        {
            return;
        }

        var closeToCamera = cameraDistance < 9.5f;
        var trunkFill = closeToCamera ? Fade(Color.FromArgb(94, 61, 36), 116) : Color.FromArgb(94, 61, 36);
        var trunkOutline = closeToCamera ? Fade(Color.FromArgb(135, 92, 54), 132) : Color.FromArgb(135, 92, 54);
        AddCylinder(commands, viewport, basePosition + new Vector3(0, prop.Height * 0.26f, 0),
            prop.Width * 0.14f, prop.Height * 0.52f, 7, trunkFill, trunkOutline, null);

        if (!closeToCamera)
        {
            var crownBase = basePosition + new Vector3(0, prop.Height * 0.56f, 0);
            AddEllipseBillboard(commands, viewport, crownBase, prop.Width * 0.98f, prop.Height * 0.44f, prop.Fill,
                prop.Outline, null);
            AddEllipseBillboard(commands, viewport, crownBase + camera.Right * prop.Width * 0.22f + new Vector3(0, 0.16f, 0),
                prop.Width * 0.72f, prop.Height * 0.34f, Color.FromArgb(72, 174, 91), prop.Outline, null);
            AddEllipseBillboard(commands, viewport, crownBase - camera.Right * prop.Width * 0.24f + new Vector3(0, 0.07f, 0),
                prop.Width * 0.68f, prop.Height * 0.3f, Color.FromArgb(48, 132, 71), prop.Outline, null);
            AddEllipseBillboard(commands, viewport, crownBase + new Vector3(0, prop.Height * 0.15f, 0),
                prop.Width * 0.58f, prop.Height * 0.26f, Color.FromArgb(88, 194, 98), prop.Outline, null);
        }

        var rootA = prop.Position + new Vector3(-prop.Width * 0.34f, 0.03f, prop.Width * 0.24f);
        var rootB = prop.Position + new Vector3(prop.Width * 0.34f, 0.03f, -prop.Width * 0.22f);
        AddGroundStrip(commands, viewport, rootA, prop.Position + new Vector3(0, 0.03f, 0), 0.12f,
            closeToCamera ? Fade(Color.FromArgb(125, 79, 42), 116) : Color.FromArgb(125, 79, 42));
        AddGroundStrip(commands, viewport, rootB, prop.Position + new Vector3(0, 0.03f, 0), 0.12f,
            closeToCamera ? Fade(Color.FromArgb(98, 65, 38), 116) : Color.FromArgb(98, 65, 38));
    }

    private void AddTreeMesh(List<DrawCommand> commands, Size viewport, WorldProp prop)
    {
        var basePosition = terrain.AtGround(prop.Position);
        var cameraDistance = Vector3.Distance(basePosition, camera.Position);
        if (cameraDistance < 3.4f)
        {
            return;
        }

        var scale = new Vector3(prop.Width * 0.82f, prop.Height / 3.25f, prop.Width * 0.82f);
        var yaw = prop.Position.X * 13.7f + prop.Position.Z * 8.3f;
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.HeartwoodTree, basePosition, scale, yaw));
    }

    private void AddLanternMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        var scale = new Vector3(prop.Width, prop.Height / 1.4f, prop.Width);
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.Lantern, basePosition, scale, prop.Position.X * 9.2f));
    }

    private void AddRunestoneMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        var scale = new Vector3(prop.Width, prop.Height / 1.55f, prop.Width);
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.RunestoneShard, basePosition, scale, prop.Position.Z * 11.4f));
    }

    private void AddCottageMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.ThornveilCottage, basePosition, new Vector3(1.22f, 1.12f, 1.22f), -18f));
    }

    private void AddFenceMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        var yaw = prop.Position.X < -5.5f ? 8f : prop.Position.X < -4f ? -4f : -18f;
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.RootFence, basePosition, new Vector3(1.08f, 1.0f, 1.0f), yaw));
    }

    private void AddStumpMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.TreeStump, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.X * 7.1f));
    }

    private void AddRootArchMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.RootArch, basePosition, new Vector3(prop.Width, prop.Height / 1.9f, prop.Width), prop.Position.Z * 6.5f));
    }

    private void AddRockMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.MossyRock, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.X * 11.1f));
    }

    private void AddHerbMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.HerbCluster, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.Z * 14.7f));
    }

    private void AddCorruptionMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.CorruptedSprout, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.X * 10.3f));
    }

    private void AddWorldrootShrineMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.WorldrootShrine, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.Z * 5.5f));
    }

    private void AddBannerMesh(List<DrawCommand> commands, Size viewport, WorldProp prop, Vector3 basePosition)
    {
        _meshRenderer.AddMesh(commands, viewport,
            new MeshInstance(ThornveilMeshes.ThornveilBanner, basePosition, new Vector3(prop.Width, prop.Height, prop.Width), prop.Position.X * 7.2f));
    }

    private void AddSelectionRing(List<DrawCommand> commands, Size viewport, Vector3 center, float radius, Color color)
    {
        const int segments = 20;
        var outer = new Vector3[segments];
        var inner = new Vector3[segments];
        for (var i = 0; i < segments; i++)
        {
            var angle = i / (float)segments * MathF.PI * 2f;
            var x = MathF.Cos(angle);
            var z = MathF.Sin(angle);
            outer[i] = terrain.GroundPoint(center.X + x * radius, center.Z + z * radius, 0.08f);
            inner[i] = terrain.GroundPoint(center.X + x * radius * 0.72f, center.Z + z * radius * 0.72f, 0.085f);
        }

        for (var i = 0; i < segments; i++)
        {
            var next = (i + 1) % segments;
            var points = ProjectPolygon(viewport, [outer[i], outer[next], inner[next], inner[i]], out var depth);
            if (points is null)
            {
                continue;
            }

            commands.Add(new DrawCommand(depth - 0.08f, g =>
            {
                using var brush = new SolidBrush(color);
                g.FillPolygon(brush, points);
            }));
        }
    }

    private void AddGroundPatch(List<DrawCommand> commands, Size viewport, WorldProp prop)
    {
        var halfX = prop.Width * 0.5f;
        var halfZ = Math.Max(0.7f, prop.Width * 0.32f);
        var points = ProjectPolygon(viewport, new[]
        {
            terrain.GroundPoint(prop.Position.X - halfX, prop.Position.Z - halfZ, 0.055f),
            terrain.GroundPoint(prop.Position.X + halfX, prop.Position.Z - halfZ * 0.75f, 0.055f),
            terrain.GroundPoint(prop.Position.X + halfX * 0.86f, prop.Position.Z + halfZ, 0.055f),
            terrain.GroundPoint(prop.Position.X - halfX * 0.92f, prop.Position.Z + halfZ * 0.82f, 0.055f)
        }, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.04f, g =>
        {
            using var brush = new SolidBrush(prop.Fill);
            using var pen = new Pen(prop.Outline, 1.5f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }));
    }

    private void AddGroundStrip(List<DrawCommand> commands, Size viewport, Vector3 start, Vector3 end, float width,
        Color color)
    {
        var direction = new Vector3(end.X - start.X, 0, end.Z - start.Z);
        if (direction.LengthSquared() <= 0.0001f)
        {
            return;
        }

        direction = Vector3.Normalize(direction);
        var normal = new Vector3(-direction.Z, 0, direction.X) * width * 0.5f;
        var points = ProjectPolygon(viewport, new[]
        {
            terrain.GroundPoint(start.X + normal.X, start.Z + normal.Z, start.Y),
            terrain.GroundPoint(end.X + normal.X, end.Z + normal.Z, end.Y),
            terrain.GroundPoint(end.X - normal.X, end.Z - normal.Z, end.Y),
            terrain.GroundPoint(start.X - normal.X, start.Z - normal.Z, start.Y)
        }, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.03f, g =>
        {
            using var brush = new SolidBrush(color);
            g.FillPolygon(brush, points);
        }));
    }

    private void AddCharacter(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width, float height,
        Color fill, Color outline, string label, Vector3 forward, bool hostile)
    {
        forward = FlatDirection(forward, camera.Forward);
        if (label == "Warden")
        {
            AddWarden(commands, viewport, basePosition, width, height, fill, outline, forward, label);
            return;
        }

        if (hostile)
        {
            if (label.Contains("Hare", StringComparison.OrdinalIgnoreCase))
            {
                AddHareEnemy(commands, viewport, basePosition, width, height, fill, outline, forward, label);
            }
            else if (label.Contains("Fern", StringComparison.OrdinalIgnoreCase) ||
                     label.Contains("Sapling", StringComparison.OrdinalIgnoreCase))
            {
                AddPlantEnemy(commands, viewport, basePosition, width, height, fill, outline, forward, label);
            }
            else
            {
                AddHostileHumanoid(commands, viewport, basePosition, width, height, fill, outline, forward, label);
            }

            return;
        }

        AddFriendlyHumanoid(commands, viewport, basePosition, width, height, fill, outline, forward, label);
    }

    private void AddWarden(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width, float height,
        Color fill, Color outline, Vector3 forward, string label)
    {
        var bodyRight = new Vector3(forward.Z, 0, -forward.X);
        var bodyCenter = basePosition + new Vector3(0, height * 0.53f, 0);
        var cloak = Shade(fill, 0.62f);
        AddBox(commands, viewport, basePosition + new Vector3(0, height * 0.3f, 0) - forward * width * 0.08f,
            new Vector3(width * 0.76f, height * 0.58f, width * 0.28f), cloak, outline, null);
        AddBox(commands, viewport, bodyCenter + forward * width * 0.04f,
            new Vector3(width * 0.58f, height * 0.54f, width * 0.36f), fill, outline, null);
        AddBox(commands, viewport, bodyCenter + bodyRight * width * 0.34f + new Vector3(0, height * 0.16f, 0),
            new Vector3(width * 0.28f, height * 0.16f, width * 0.28f), Shade(fill, 1.16f), outline, null);
        AddBox(commands, viewport, bodyCenter - bodyRight * width * 0.34f + new Vector3(0, height * 0.16f, 0),
            new Vector3(width * 0.28f, height * 0.16f, width * 0.28f), Shade(fill, 1.16f), outline, null);

        AddCylinder(commands, viewport, basePosition + bodyRight * width * 0.18f + new Vector3(0, height * 0.17f, 0),
            width * 0.075f, height * 0.34f, 5, Shade(fill, 0.72f), outline, null);
        AddCylinder(commands, viewport, basePosition - bodyRight * width * 0.18f + new Vector3(0, height * 0.17f, 0),
            width * 0.075f, height * 0.34f, 5, Shade(fill, 0.72f), outline, null);
        AddCylinder(commands, viewport, bodyCenter + bodyRight * width * 0.38f + forward * width * 0.03f,
            width * 0.06f, height * 0.42f, 5, Shade(fill, 0.82f), outline, null);
        AddCylinder(commands, viewport, bodyCenter - bodyRight * width * 0.38f + forward * width * 0.03f,
            width * 0.06f, height * 0.42f, 5, Shade(fill, 0.82f), outline, null);

        var chestPoint = bodyCenter + forward * width * 0.23f + new Vector3(0, height * 0.08f, 0);
        AddBillboard(commands, viewport, chestPoint, width * 0.18f, height * 0.2f,
            Color.FromArgb(198, 176, 218, 148), outline, null);
        AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, height * 0.97f, 0),
            width * 0.62f, width * 0.62f, Color.FromArgb(212, 224, 232, 190), outline, label);
    }

    private void AddFriendlyHumanoid(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width,
        float height, Color fill, Color outline, Vector3 forward, string label)
    {
        var bodyRight = new Vector3(forward.Z, 0, -forward.X);
        var bodyCenter = basePosition + new Vector3(0, height * 0.5f, 0);
        AddBox(commands, viewport, basePosition + new Vector3(0, height * 0.32f, 0),
            new Vector3(width * 0.62f, height * 0.62f, width * 0.34f), Shade(fill, 0.78f), outline, null);
        AddBox(commands, viewport, bodyCenter + new Vector3(0, height * 0.14f, 0),
            new Vector3(width * 0.46f, height * 0.46f, width * 0.3f), fill, outline, null);
        AddCylinder(commands, viewport, bodyCenter + bodyRight * width * 0.36f,
            width * 0.055f, height * 0.5f, 5, Shade(fill, 0.86f), outline, null);
        AddCylinder(commands, viewport, bodyCenter - bodyRight * width * 0.36f,
            width * 0.055f, height * 0.5f, 5, Shade(fill, 0.86f), outline, null);
        AddCylinder(commands, viewport, basePosition + bodyRight * width * 0.18f + new Vector3(0, height * 0.15f, 0),
            width * 0.06f, height * 0.3f, 5, Shade(fill, 0.68f), outline, null);
        AddCylinder(commands, viewport, basePosition - bodyRight * width * 0.18f + new Vector3(0, height * 0.15f, 0),
            width * 0.06f, height * 0.3f, 5, Shade(fill, 0.68f), outline, null);
        AddBillboard(commands, viewport, bodyCenter + forward * width * 0.24f + new Vector3(0, height * 0.08f, 0),
            width * 0.14f, height * 0.2f, Color.FromArgb(214, 116, 238, 168), outline, null);
        AddCylinder(commands, viewport, basePosition + bodyRight * width * 0.62f + new Vector3(0, height * 0.45f, 0),
            width * 0.035f, height * 0.88f, 5, Color.FromArgb(176, 116, 82, 44), outline, null);
        AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, height * 0.98f, 0),
            width * 0.56f, width * 0.56f, Color.FromArgb(222, 226, 232, 190), outline, label);
    }

    private void AddHostileHumanoid(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width,
        float height, Color fill, Color outline, Vector3 forward, string label)
    {
        var bodyRight = new Vector3(forward.Z, 0, -forward.X);
        var bodyCenter = basePosition + new Vector3(0, height * 0.5f, 0);
        AddBox(commands, viewport, bodyCenter,
            new Vector3(width * 0.58f, height * 0.56f, width * 0.36f), Shade(fill, 0.88f), outline, null);
        AddBox(commands, viewport, bodyCenter + bodyRight * width * 0.4f + new Vector3(0, height * 0.1f, 0),
            new Vector3(width * 0.22f, height * 0.16f, width * 0.28f), Shade(fill, 1.18f), outline, null);
        AddBox(commands, viewport, bodyCenter - bodyRight * width * 0.4f + new Vector3(0, height * 0.1f, 0),
            new Vector3(width * 0.22f, height * 0.16f, width * 0.28f), Shade(fill, 1.18f), outline, null);
        AddCylinder(commands, viewport, basePosition + bodyRight * width * 0.19f + new Vector3(0, height * 0.17f, 0),
            width * 0.075f, height * 0.34f, 5, Shade(fill, 0.58f), outline, null);
        AddCylinder(commands, viewport, basePosition - bodyRight * width * 0.19f + new Vector3(0, height * 0.17f, 0),
            width * 0.075f, height * 0.34f, 5, Shade(fill, 0.58f), outline, null);
        AddCylinder(commands, viewport, bodyCenter + bodyRight * width * 0.45f + forward * width * 0.18f,
            width * 0.06f, height * 0.46f, 5, Shade(fill, 0.75f), outline, null);
        AddCylinder(commands, viewport, bodyCenter - bodyRight * width * 0.45f + forward * width * 0.18f,
            width * 0.06f, height * 0.46f, 5, Shade(fill, 0.75f), outline, null);
        AddBillboard(commands, viewport, bodyCenter + forward * width * 0.28f,
            width * 0.18f, height * 0.2f, Color.FromArgb(222, 96, 32, 32), outline, null);
        AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, height * 0.95f, 0),
            width * 0.55f, width * 0.55f, Color.FromArgb(218, 226, 196, 162), outline, label);
    }

    private void AddPlantEnemy(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width,
        float height, Color fill, Color outline, Vector3 forward, string label)
    {
        var bodyRight = new Vector3(forward.Z, 0, -forward.X);
        AddCylinder(commands, viewport, basePosition + new Vector3(0, height * 0.38f, 0),
            width * 0.16f, height * 0.76f, 6, Shade(fill, 0.74f), outline, null);
        AddBox(commands, viewport, basePosition + new Vector3(0, height * 0.54f, 0) + forward * width * 0.06f,
            new Vector3(width * 0.5f, height * 0.28f, width * 0.26f), fill, outline, null);
        AddBillboard(commands, viewport, basePosition + bodyRight * width * 0.38f + new Vector3(0, height * 0.76f, 0),
            width * 0.42f, height * 0.5f, Shade(fill, 1.18f), outline, null);
        AddBillboard(commands, viewport, basePosition - bodyRight * width * 0.38f + new Vector3(0, height * 0.72f, 0),
            width * 0.42f, height * 0.5f, Shade(fill, 0.82f), outline, null);
        AddBillboard(commands, viewport, basePosition + forward * width * 0.28f + new Vector3(0, height * 0.66f, 0),
            width * 0.16f, height * 0.22f, Color.FromArgb(226, 238, 86, 64), outline, null);
        AddEllipseBillboard(commands, viewport, basePosition + new Vector3(0, height * 0.98f, 0),
            width * 0.5f, width * 0.42f, Color.FromArgb(224, 238, 196, 150), outline, label);
    }

    private void AddHareEnemy(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width,
        float height, Color fill, Color outline, Vector3 forward, string label)
    {
        var bodyRight = new Vector3(forward.Z, 0, -forward.X);
        var body = basePosition + new Vector3(0, height * 0.32f, 0);
        AddBox(commands, viewport, body,
            new Vector3(width * 0.72f, height * 0.34f, width * 0.46f), Shade(fill, 0.85f), outline, null);
        AddEllipseBillboard(commands, viewport, body + forward * width * 0.42f + new Vector3(0, height * 0.1f, 0),
            width * 0.42f, width * 0.38f, fill, outline, null);
        AddCylinder(commands, viewport, body + bodyRight * width * 0.16f + forward * width * 0.36f + new Vector3(0, height * 0.38f, 0),
            width * 0.035f, height * 0.5f, 5, fill, outline, null);
        AddCylinder(commands, viewport, body - bodyRight * width * 0.16f + forward * width * 0.36f + new Vector3(0, height * 0.38f, 0),
            width * 0.035f, height * 0.5f, 5, fill, outline, null);
        AddCylinder(commands, viewport, basePosition + bodyRight * width * 0.22f + new Vector3(0, height * 0.1f, 0),
            width * 0.055f, height * 0.2f, 5, Shade(fill, 0.68f), outline, null);
        AddCylinder(commands, viewport, basePosition - bodyRight * width * 0.22f + new Vector3(0, height * 0.1f, 0),
            width * 0.055f, height * 0.2f, 5, Shade(fill, 0.68f), outline, null);
        AddBillboard(commands, viewport, body + forward * width * 0.64f,
            width * 0.12f, height * 0.13f, Color.FromArgb(232, 240, 72, 62), outline, label);
    }

    private void AddActorHud(List<DrawCommand> commands, Size viewport, WorldActor actor, Vector3 position, float health)
    {
        var selected = state.TargetName == actor.Name;
        var playerDistance = Vector3.Distance(position, state.PlayerPosition);
        if (health <= 0f || (!selected && playerDistance > 8.5f))
        {
            return;
        }

        var anchor = position + new Vector3(0, actor.Height + 0.35f, 0);
        if (!TryProject(viewport, anchor, out var screen, out var depth))
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.24f, g =>
        {
            var color = actor.Hostile
                ? Color.FromArgb(228, 234, 82, 68)
                : Color.FromArgb(228, 236, 214, 92);
            var width = selected ? 92f : 52f;
            var barY = screen.Y + (selected ? 0f : 3f);
            using var backBrush = new SolidBrush(Color.FromArgb(selected ? 185 : 124, 8, 14, 14));
            using var fillBrush = new SolidBrush(color);
            using var borderPen = new Pen(Color.FromArgb(190, 218, 230, 184), selected ? 1.4f : 1f);
            var fillWidth = MathF.Max(0f, (width - 2f) * Math.Clamp(health / 100f, 0f, 1f));
            g.FillRectangle(backBrush, screen.X - width * 0.5f, barY, width, selected ? 8f : 5f);
            g.FillRectangle(fillBrush, screen.X - width * 0.5f + 1f, barY + 1f, fillWidth, selected ? 6f : 3f);
            g.DrawRectangle(borderPen, screen.X - width * 0.5f, barY, width, selected ? 8f : 5f);

            if (selected)
            {
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(244, 232, 178));
                var textSize = g.MeasureString(actor.Name, font);
                g.DrawString(actor.Name, font, textBrush, screen.X - textSize.Width * 0.5f, screen.Y - 19f);

                var diamond = new[]
                {
                    new PointF(screen.X, screen.Y - 31f),
                    new PointF(screen.X + 7f, screen.Y - 24f),
                    new PointF(screen.X, screen.Y - 17f),
                    new PointF(screen.X - 7f, screen.Y - 24f)
                };
                using var markerBrush = new SolidBrush(Color.FromArgb(232, 255, 232, 118));
                using var markerPen = new Pen(Color.FromArgb(230, 70, 46, 16), 1.2f);
                g.FillPolygon(markerBrush, diamond);
                g.DrawPolygon(markerPen, diamond);
            }
        }));
    }

    private void AddInteractionMarker(List<DrawCommand> commands, Size viewport, WorldActor actor, Vector3 position,
        float pulse)
    {
        if (actor.Hostile || state.ActorHealth.GetValueOrDefault(actor.Name, 100f) <= 0f)
        {
            return;
        }

        var marker = QuestMarkerFor(actor.Name);
        if (marker is null && Vector3.Distance(position, state.PlayerPosition) > 4.2f)
        {
            return;
        }

        var anchor = position + new Vector3(0, actor.Height + 0.72f + MathF.Sin(pulse * 2.4f) * 0.05f, 0);
        if (!TryProject(viewport, anchor, out var screen, out var depth))
        {
            return;
        }

        commands.Add(new DrawCommand(depth - 0.36f, g =>
        {
            if (marker is { } symbol)
            {
                var color = symbol == "?" ? Color.FromArgb(244, 132, 238, 142) : Color.FromArgb(244, 244, 208, 72);
                using var glowBrush = new SolidBrush(Color.FromArgb(90, color.R, color.G, color.B));
                using var coreBrush = new SolidBrush(color);
                using var borderPen = new Pen(Color.FromArgb(228, 72, 48, 18), 1.4f);
                using var font = new Font("Segoe UI", 16f, FontStyle.Bold);
                g.FillEllipse(glowBrush, screen.X - 15f, screen.Y - 15f, 30f, 30f);
                g.FillEllipse(coreBrush, screen.X - 9f, screen.Y - 9f, 18f, 18f);
                g.DrawEllipse(borderPen, screen.X - 9f, screen.Y - 9f, 18f, 18f);
                var size = g.MeasureString(symbol, font);
                using var textBrush = new SolidBrush(Color.FromArgb(245, 42, 28, 12));
                g.DrawString(symbol, font, textBrush, screen.X - size.Width * 0.5f, screen.Y - size.Height * 0.62f);
                return;
            }

            using var talkBrush = new SolidBrush(Color.FromArgb(180, 116, 238, 174));
            using var talkPen = new Pen(Color.FromArgb(190, 216, 250, 190), 1.2f);
            g.FillEllipse(talkBrush, screen.X - 5f, screen.Y - 5f, 10f, 10f);
            g.DrawEllipse(talkPen, screen.X - 5f, screen.Y - 5f, 10f, 10f);
        }));
    }

    private string? QuestMarkerFor(string actorName)
    {
        if (actorName != "Root Guardian" || state.FirstPruningTurnedIn)
        {
            return null;
        }

        return state.FirstPruningKills >= FirstPruningRequiredKills ? "?" : "!";
    }

    private void AddCombatTexts(List<DrawCommand> commands, Size viewport)
    {
        foreach (var text in state.CombatTexts)
        {
            var lift = text.Age / text.Lifetime * 0.52f;
            var alpha = Math.Clamp((int)(255f * (1f - text.Age / text.Lifetime)), 0, 255);
            if (!TryProject(viewport, text.Position + new Vector3(0, lift, 0), out var screen, out var depth))
            {
                continue;
            }

            commands.Add(new DrawCommand(depth - 0.38f, g =>
            {
                using var font = new Font("Segoe UI", 12f, FontStyle.Bold);
                using var brush = new SolidBrush(Color.FromArgb(alpha, text.Color.R, text.Color.G, text.Color.B));
                using var shadow = new SolidBrush(Color.FromArgb(Math.Min(alpha, 160), 0, 0, 0));
                var size = g.MeasureString(text.Text, font);
                g.DrawString(text.Text, font, shadow, screen.X - size.Width * 0.5f + 1f, screen.Y - 1f);
                g.DrawString(text.Text, font, brush, screen.X - size.Width * 0.5f, screen.Y - 2f);
            }));
        }
    }

    private void AddStrikeFlash(List<DrawCommand> commands, Size viewport)
    {
        if (state.StrikeFlash <= 0f)
        {
            return;
        }

        if (!TryProject(viewport, state.StrikeStart, out var start, out var startDepth) ||
            !TryProject(viewport, state.StrikeEnd, out var end, out var endDepth))
        {
            return;
        }

        var alpha = Math.Clamp((int)(255f * state.StrikeFlash / 0.16f), 0, 255);
        commands.Add(new DrawCommand((startDepth + endDepth) * 0.5f - 0.42f, g =>
        {
            using var widePen = new Pen(Color.FromArgb(alpha / 2, 255, 232, 122), 8f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            using var corePen = new Pen(Color.FromArgb(alpha, 255, 250, 184), 2.4f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            g.DrawLine(widePen, start, end);
            g.DrawLine(corePen, start, end);
        }));
    }

    private void AddBox(List<DrawCommand> commands, Size viewport, Vector3 center, Vector3 size, Color fill,
        Color outline, string? label)
    {
        var hy = size.Y * 0.5f;
        foreach (var face in PrimitiveMesh.Box(center, size))
        {
            AddBoxFace(commands, viewport, face.Vertices, Shade(fill, face.Shade), outline);
        }

        if (label is not null &&
            TryProject(viewport, center + new Vector3(0, hy + 0.28f, 0), out var top, out var depth) &&
            ShouldDrawLabel(label, center, top, viewport))
        {
            commands.Add(new DrawCommand(depth - 0.05f, g =>
            {
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(228, 232, 222, 180));
                var textSize = g.MeasureString(label, font);
                g.DrawString(label, font, textBrush, top.X - textSize.Width * 0.5f, top.Y - 18);
            }));
        }
    }

    private void AddCylinder(List<DrawCommand> commands, Size viewport, Vector3 center, float radius, float height,
        int sides, Color fill, Color outline, string? label)
    {
        foreach (var face in PrimitiveMesh.Cylinder(center, radius, height, sides))
        {
            AddBoxFace(commands, viewport, face.Vertices, Shade(fill, face.Shade), outline);
        }

        if (label is not null &&
            TryProject(viewport, center + new Vector3(0, height * 0.62f, 0), out var labelPoint, out var depth) &&
            ShouldDrawLabel(label, center, labelPoint, viewport))
        {
            commands.Add(new DrawCommand(depth - 0.05f, g =>
            {
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(228, 232, 222, 180));
                var textSize = g.MeasureString(label, font);
                g.DrawString(label, font, textBrush, labelPoint.X - textSize.Width * 0.5f, labelPoint.Y - 18);
            }));
        }
    }

    private void AddEllipseBillboard(List<DrawCommand> commands, Size viewport, Vector3 center, float width, float height,
        Color fill, Color outline, string? label)
    {
        if (!TryProject(viewport, center, out var screen, out var depth))
        {
            return;
        }

        if (!TryProject(viewport, center + camera.Right * (width * 0.5f), out var rightScreen, out _) ||
            !TryProject(viewport, center + Vector3.UnitY * (height * 0.5f), out var topScreen, out _))
        {
            return;
        }

        var radiusX = Math.Max(5f, MathF.Abs(rightScreen.X - screen.X));
        var radiusY = Math.Max(4f, MathF.Abs(screen.Y - topScreen.Y));
        commands.Add(new DrawCommand(depth - 0.03f, g =>
        {
            using var shadowBrush = new SolidBrush(Color.FromArgb(74, 0, 0, 0));
            g.FillEllipse(shadowBrush, screen.X - radiusX * 0.72f, screen.Y + radiusY * 0.5f,
                radiusX * 1.44f, radiusY * 0.34f);

            using var brush = new SolidBrush(fill);
            using var pen = new Pen(outline, 2f);
            var bounds = new RectangleF(screen.X - radiusX, screen.Y - radiusY, radiusX * 2f, radiusY * 2f);
            g.FillEllipse(brush, bounds);
            g.DrawEllipse(pen, bounds);

            using var glintPen = new Pen(Color.FromArgb(92, 236, 255, 196), 1f);
            g.DrawArc(glintPen, screen.X - radiusX * 0.52f, screen.Y - radiusY * 0.58f,
                radiusX * 0.8f, radiusY * 0.62f, 190, 125);

            if (label is not null && ShouldDrawLabel(label, center, screen, viewport))
            {
                using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(228, 232, 222, 180));
                var textSize = g.MeasureString(label, font);
                g.DrawString(label, font, textBrush, screen.X - textSize.Width * 0.5f, screen.Y - radiusY - 20);
            }
        }));
    }

    private void AddBoxFace(List<DrawCommand> commands, Size viewport, IReadOnlyList<Vector3> vertices, Color fill,
        Color outline)
    {
        var points = ProjectPolygon(viewport, vertices, out var depth);
        if (points is null)
        {
            return;
        }

        commands.Add(new DrawCommand(depth, g =>
        {
            using var brush = new SolidBrush(fill);
            using var pen = new Pen(outline, 1.2f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }));
    }

    private void AddBillboard(List<DrawCommand> commands, Size viewport, Vector3 basePosition, float width, float height,
        Color fill, Color outline, string? label)
    {
        var half = camera.Right * (width * 0.5f);
        var bottomLeft = basePosition - half;
        var bottomRight = basePosition + half;
        var topRight = bottomRight + Vector3.UnitY * height;
        var topLeft = bottomLeft + Vector3.UnitY * height;
        var points = ProjectPolygon(viewport, new[] { bottomLeft, bottomRight, topRight, topLeft }, out var depth);
        if (points is null)
        {
            return;
        }

        var foot = Project(viewport, basePosition + new Vector3(0, 0.02f, 0));
        commands.Add(new DrawCommand(depth, g =>
        {
            using var shadowBrush = new SolidBrush(Color.FromArgb(105, 0, 0, 0));
            g.FillEllipse(shadowBrush, foot.X - 20, foot.Y - 6, 40, 12);
            using var brush = new SolidBrush(fill);
            using var pen = new Pen(outline, 2f);
            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);

            if (label is not null)
            {
                var top = points.OrderBy(point => point.Y).First();
                if (ShouldDrawLabel(label, basePosition, top, viewport))
                {
                    using var font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    using var textBrush = new SolidBrush(Color.FromArgb(228, 232, 222, 180));
                    var textSize = g.MeasureString(label, font);
                    g.DrawString(label, font, textBrush, top.X - textSize.Width * 0.5f, top.Y - 18);
                }
            }
        }));
    }

    private bool ShouldDrawLabel(string label, Vector3 position, PointF screen, Size viewport)
    {
        if (label == "Warden")
        {
            return true;
        }

        var playerDistance = Vector3.Distance(position, state.PlayerPosition);
        if (playerDistance > 5.8f)
        {
            return false;
        }

        var centerX = viewport.Width * 0.5f;
        return MathF.Abs(screen.X - centerX) < viewport.Width * 0.32f &&
               screen.Y > viewport.Height * 0.12f &&
               screen.Y < viewport.Height * 0.84f;
    }

    private PointF[]? ProjectPolygon(Size viewport, IReadOnlyList<Vector3> vertices, out float depth)
    {
        var points = new PointF[vertices.Count];
        depth = 0f;
        for (var i = 0; i < vertices.Count; i++)
        {
            if (!TryProject(viewport, vertices[i], out points[i], out var pointDepth))
            {
                return null;
            }

            depth += pointDepth;
        }

        depth /= vertices.Count;
        return points;
    }

    private bool TryProject(Size viewport, Vector3 point, out PointF screen, out float depth)
    {
        return camera.TryProject(viewport, point, out screen, out depth);
    }

    private PointF Project(Size viewport, Vector3 point)
    {
        return camera.Project(viewport, point);
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

    private static Vector3 FlatDirection(Vector3 direction, Vector3 fallback)
    {
        var flat = new Vector3(direction.X, 0, direction.Z);
        if (flat.LengthSquared() <= 0.0001f)
        {
            flat = new Vector3(fallback.X, 0, fallback.Z);
        }

        return flat.LengthSquared() <= 0.0001f ? Vector3.UnitZ : Vector3.Normalize(flat);
    }
}
