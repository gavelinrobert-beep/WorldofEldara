using System.Drawing;
using System.Numerics;

namespace WorldofEldara.Client3D.Game;

public sealed class SceneData
{
    public required BlockoutScene Blockout { get; init; }
    public required IReadOnlyList<TerrainChunk> TerrainChunks { get; init; }
    public required IReadOnlyList<PathRibbon> Paths { get; init; }
    public required IReadOnlyList<WorldProp> Props { get; init; }
    public required IReadOnlyList<WorldActor> Actors { get; init; }
}

public static class ThornveilSceneData
{
    public static SceneData Create() => new()
    {
        Blockout = BlockoutScene.LoadDefault(),
        TerrainChunks =
        [
            new("arrival-clearing", new Vector3(0, 0, -7.2f), 6.2f, 4.2f, TerrainMaterial.Clearing, 0.5f),
            new("hearthbough-hub", new Vector3(0, 0, 0.8f), 7.4f, 5.4f, TerrainMaterial.Shrine, 0.48f),
            new("upper-enclave-terrace", new Vector3(0, 0, 8.6f), 7.2f, 4.7f, TerrainMaterial.Clearing, 0.42f),
            new("west-treehouse-copse", new Vector3(-5.2f, 0, 1.3f), 5.6f, 4.2f, TerrainMaterial.Forest, 0.35f),
            new("east-overlook-copse", new Vector3(5.2f, 0, 3.0f), 5.4f, 4.0f, TerrainMaterial.Forest, 0.35f),
            new("mossglass-pool-pocket", new Vector3(-5.6f, 0, -3.2f), 4.6f, 3.1f, TerrainMaterial.Water, 0.36f),
            new("corruption-pocket", new Vector3(6.4f, 0, -3.6f), 4.8f, 3.4f, TerrainMaterial.Corruption, 0.34f),
            new("main-root-path", new Vector3(0, 0, 1.2f), 3.2f, 12.8f, TerrainMaterial.Path, 0.48f),
            new("west-branch-path", new Vector3(-2.9f, 0, 0.5f), 4.1f, 1.7f, TerrainMaterial.Path, 0.42f),
            new("east-branch-path", new Vector3(3.0f, 0, 1.7f), 4.3f, 1.8f, TerrainMaterial.Path, 0.42f)
        ],
        Paths =
        [
            new([
                new Vector3(0, 0.04f, -8.2f),
                new Vector3(0, 0.04f, -4.0f),
                new Vector3(0, 0.04f, 0.8f),
                new Vector3(0, 0.04f, 5.2f),
                new Vector3(0, 0.04f, 9.7f)
            ], 2.35f, Color.FromArgb(154, 96, 66, 40)),
            new([
                new Vector3(0, 0.045f, 0.4f),
                new Vector3(-2.8f, 0.045f, 0.8f),
                new Vector3(-5.2f, 0.045f, 1.6f)
            ], 1.45f, Color.FromArgb(132, 84, 52, 32)),
            new([
                new Vector3(0.3f, 0.045f, 1.0f),
                new Vector3(2.9f, 0.045f, 1.8f),
                new Vector3(5.8f, 0.045f, -2.7f)
            ], 1.38f, Color.FromArgb(126, 74, 48, 31))
        ],
        Props =
        [
            new("tree", new Vector3(-6, 0, -4), 1.4f, 3.6f, Color.FromArgb(62, 157, 84), Color.FromArgb(136, 220, 130)),
            new("tree", new Vector3(4, 0, -3), 1.7f, 4.2f, Color.FromArgb(55, 145, 78), Color.FromArgb(127, 210, 122)),
            new("tree", new Vector3(8, 0, 4), 1.5f, 3.8f, Color.FromArgb(48, 130, 73), Color.FromArgb(118, 204, 118)),
            new("tree", new Vector3(-12, 0, 5), 1.6f, 3.9f, Color.FromArgb(50, 137, 75), Color.FromArgb(118, 204, 118)),
            new("tree", new Vector3(-15, 0, -3), 1.3f, 3.2f, Color.FromArgb(50, 134, 70), Color.FromArgb(118, 204, 118)),
            new("tree", new Vector3(12, 0, -1), 1.35f, 3.4f, Color.FromArgb(55, 151, 82), Color.FromArgb(127, 210, 122)),
            new("tree", new Vector3(-2, 0, -9), 1.2f, 3.0f, Color.FromArgb(66, 166, 88), Color.FromArgb(142, 220, 132)),
            new("tree", new Vector3(14, 0, 9), 1.6f, 4.0f, Color.FromArgb(44, 126, 70), Color.FromArgb(116, 198, 116)),
            new("cottage", new Vector3(-4.6f, 0, 8.9f), 1.0f, 1.0f, Color.FromArgb(162, 148, 128), Color.FromArgb(218, 206, 176)),
            new("fence", new Vector3(-6.5f, 0, 7.2f), 1.0f, 1.0f, Color.FromArgb(96, 62, 36), Color.FromArgb(166, 106, 58)),
            new("fence", new Vector3(-4.8f, 0, 6.9f), 1.0f, 1.0f, Color.FromArgb(96, 62, 36), Color.FromArgb(166, 106, 58)),
            new("fence", new Vector3(-3.1f, 0, 7.3f), 1.0f, 1.0f, Color.FromArgb(96, 62, 36), Color.FromArgb(166, 106, 58)),
            new("worldroot", new Vector3(0.2f, 0, 3.9f), 1.08f, 1.2f, Color.FromArgb(124, 239, 162), Color.FromArgb(214, 255, 184)),
            new("banner", new Vector3(-3.4f, 0, 2.9f), 0.82f, 1.0f, Color.FromArgb(34, 104, 70), Color.FromArgb(132, 214, 132)),
            new("banner", new Vector3(3.1f, 0, 1.3f), 0.78f, 1.0f, Color.FromArgb(34, 104, 70), Color.FromArgb(132, 214, 132)),
            new("stump", new Vector3(-10.6f, 0, 2.1f), 1.0f, 1.0f, Color.FromArgb(130, 84, 48), Color.FromArgb(202, 146, 82)),
            new("stump", new Vector3(11.6f, 0, -5.1f), 0.82f, 0.82f, Color.FromArgb(130, 84, 48), Color.FromArgb(202, 146, 82)),
            new("lantern", new Vector3(-2, 0, 2.8f), 0.45f, 1.4f, Color.FromArgb(104, 224, 142), Color.FromArgb(208, 254, 173)),
            new("lantern", new Vector3(3.8f, 0, -1.5f), 0.4f, 1.3f, Color.FromArgb(124, 239, 162), Color.FromArgb(214, 255, 184)),
            new("lantern", new Vector3(-7.2f, 0, 1.4f), 0.42f, 1.35f, Color.FromArgb(100, 220, 142), Color.FromArgb(208, 254, 173)),
            new("runestone", new Vector3(2.8f, 0, 3.2f), 0.55f, 1.55f, Color.FromArgb(116, 138, 132), Color.FromArgb(173, 232, 208)),
            new("runestone", new Vector3(-4.8f, 0, -1.2f), 0.45f, 1.25f, Color.FromArgb(106, 132, 128), Color.FromArgb(160, 224, 204)),
            new("rootgate", new Vector3(6.6f, 0, -6.2f), 1.7f, 2.1f, Color.FromArgb(44, 137, 73), Color.FromArgb(146, 210, 112)),
            new("rootgate", new Vector3(-7.3f, 0, -2.3f), 1.35f, 1.8f, Color.FromArgb(44, 137, 73), Color.FromArgb(146, 210, 112)),
            new("water", new Vector3(-9, 0, -6), 2.8f, 0.12f, Color.FromArgb(55, 126, 150), Color.FromArgb(103, 194, 205)),
            new("water", new Vector3(7.4f, 0, 6.4f), 2.1f, 0.12f, Color.FromArgb(43, 112, 140), Color.FromArgb(92, 184, 204)),
            new("scar", new Vector3(10, 0, -5), 2.5f, 0.12f, Color.FromArgb(96, 49, 122), Color.FromArgb(196, 83, 226)),
            new("herb", new Vector3(-1.1f, 0, -1.8f), 0.7f, 0.84f, Color.FromArgb(64, 172, 86), Color.FromArgb(158, 236, 138)),
            new("herb", new Vector3(1.9f, 0, 4.4f), 0.62f, 0.72f, Color.FromArgb(64, 172, 86), Color.FromArgb(158, 236, 138)),
            new("herb", new Vector3(-8.8f, 0, 5.2f), 0.58f, 0.7f, Color.FromArgb(64, 172, 86), Color.FromArgb(158, 236, 138)),
            new("corruption", new Vector3(9.2f, 0, -4.3f), 0.78f, 0.9f, Color.FromArgb(106, 46, 136), Color.FromArgb(230, 92, 242)),
            new("corruption", new Vector3(10.8f, 0, -5.8f), 0.62f, 0.78f, Color.FromArgb(106, 46, 136), Color.FromArgb(230, 92, 242)),
            new("corruption", new Vector3(-9.4f, 0, -7.1f), 0.7f, 0.86f, Color.FromArgb(106, 46, 136), Color.FromArgb(230, 92, 242)),
            new("shrub", new Vector3(-7.8f, 0, -3.2f), 1.3f, 0.7f, Color.FromArgb(36, 112, 63), Color.FromArgb(75, 168, 92)),
            new("shrub", new Vector3(-1.7f, 0, 5.8f), 1.1f, 0.62f, Color.FromArgb(42, 128, 68), Color.FromArgb(84, 178, 96)),
            new("shrub", new Vector3(5.7f, 0, 2.6f), 1.15f, 0.66f, Color.FromArgb(32, 104, 58), Color.FromArgb(72, 158, 88)),
            new("shrub", new Vector3(12.4f, 0, 3.6f), 1.4f, 0.76f, Color.FromArgb(35, 112, 60), Color.FromArgb(82, 170, 92)),
            new("mushroom", new Vector3(-10.4f, 0, -4.4f), 0.52f, 0.72f, Color.FromArgb(226, 105, 116), Color.FromArgb(252, 190, 190)),
            new("mushroom", new Vector3(-6.2f, 0, 6.7f), 0.44f, 0.62f, Color.FromArgb(224, 114, 128), Color.FromArgb(252, 190, 190)),
            new("mushroom", new Vector3(9.1f, 0, 4.9f), 0.5f, 0.68f, Color.FromArgb(214, 98, 128), Color.FromArgb(248, 180, 210)),
            new("rock", new Vector3(-3.2f, 0, 4.4f), 0.72f, 0.56f, Color.FromArgb(83, 92, 82), Color.FromArgb(132, 148, 126)),
            new("rock", new Vector3(5.2f, 0, -7.6f), 0.9f, 0.62f, Color.FromArgb(78, 88, 80), Color.FromArgb(126, 144, 126)),
            new("rock", new Vector3(13.2f, 0, 7.1f), 0.74f, 0.5f, Color.FromArgb(86, 94, 82), Color.FromArgb(132, 148, 126))
        ],
        Actors =
        [
            new("Root Guardian", new Vector3(1.6f, 0, 0.3f), 0.62f, 1.55f, 0.05f, 1.4f,
                Color.FromArgb(122, 177, 127), Color.FromArgb(214, 236, 182), false),
            new("Razor Fern", new Vector3(5.7f, 0, -2.4f), 0.7f, 1.15f, 0.4f, 2.1f,
                Color.FromArgb(170, 78, 66), Color.FromArgb(240, 126, 102), true),
            new("Hollow Sapling", new Vector3(7.2f, 0, -3.9f), 0.7f, 1.25f, 0.32f, 1.8f,
                Color.FromArgb(160, 74, 62), Color.FromArgb(238, 112, 92), true),
            new("Memory Keeper", new Vector3(-1.7f, 0, 1.2f), 0.58f, 1.45f, 0.03f, 1.0f,
                Color.FromArgb(132, 184, 142), Color.FromArgb(230, 236, 184), false),
            new("Elder Thaelir", new Vector3(-3.5f, 0, 1.4f), 0.58f, 1.5f, 0.02f, 1.1f,
                Color.FromArgb(154, 186, 128), Color.FromArgb(238, 226, 166), false),
            new("Twice-Dead Hare", new Vector3(-5.6f, 0, -3.2f), 0.58f, 1.0f, 0.7f, 2.8f,
                Color.FromArgb(210, 226, 216), Color.FromArgb(246, 252, 238), true),
            new("Thornveil Scout", new Vector3(6.6f, 0, -1.6f), 0.62f, 1.3f, 0.42f, 2.2f,
                Color.FromArgb(174, 86, 68), Color.FromArgb(242, 128, 106), true)
        ]
    };
}

public readonly record struct WorldProp(string Kind, Vector3 Position, float Width, float Height, Color Fill,
    Color Outline);

public readonly record struct WorldActor(string Name, Vector3 Position, float Width, float Height, float Wander,
    float Speed, Color Fill, Color Outline, bool Hostile)
{
    public float Phase => Position.X * 0.77f + Position.Z * 0.41f;
}

public readonly record struct PathRibbon(IReadOnlyList<Vector3> Points, float Width, Color Color);
