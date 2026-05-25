using System.Drawing;
using System.Numerics;

namespace WorldofEldara.Client3D.Game.Rendering;

public readonly record struct MeshMaterial(string Name, Color Fill, Color Outline);

public readonly record struct MeshPart(IReadOnlyList<Vector3> Vertices, float Shade, MeshMaterial Material);

public sealed class MeshAsset(string name, IReadOnlyList<MeshPart> parts)
{
    public string Name { get; } = name;
    public IReadOnlyList<MeshPart> Parts { get; } = parts;
}

public readonly record struct MeshInstance(MeshAsset Asset, Vector3 Position, Vector3 Scale, float YawDegrees);

public static class ThornveilMeshes
{
    public static readonly MeshMaterial Bark = new("bark", Color.FromArgb(108, 68, 38), Color.FromArgb(162, 104, 58));
    public static readonly MeshMaterial BarkDark = new("bark-dark", Color.FromArgb(82, 54, 34), Color.FromArgb(132, 84, 50));
    public static readonly MeshMaterial Leaf = new("leaf", Color.FromArgb(62, 158, 82), Color.FromArgb(132, 220, 126));
    public static readonly MeshMaterial LeafDark = new("leaf-dark", Color.FromArgb(38, 124, 68), Color.FromArgb(104, 190, 112));
    public static readonly MeshMaterial LanternWood = new("lantern-wood", Color.FromArgb(92, 68, 42), Color.FromArgb(212, 232, 166));
    public static readonly MeshMaterial LanternGlow = new("lantern-glow", Color.FromArgb(112, 240, 152), Color.FromArgb(224, 255, 184));
    public static readonly MeshMaterial Runestone = new("runestone", Color.FromArgb(104, 134, 130), Color.FromArgb(178, 232, 212));
    public static readonly MeshMaterial Plaster = new("plaster", Color.FromArgb(162, 148, 128), Color.FromArgb(218, 206, 176));
    public static readonly MeshMaterial RoofMoss = new("roof-moss", Color.FromArgb(44, 88, 62), Color.FromArgb(104, 152, 104));
    public static readonly MeshMaterial WarmTimber = new("warm-timber", Color.FromArgb(126, 72, 40), Color.FromArgb(194, 116, 62));
    public static readonly MeshMaterial OldFence = new("old-fence", Color.FromArgb(96, 62, 36), Color.FromArgb(166, 106, 58));
    public static readonly MeshMaterial CutWood = new("cut-wood", Color.FromArgb(130, 84, 48), Color.FromArgb(202, 146, 82));

    public static readonly MeshAsset HeartwoodTree = BuildHeartwoodTree();
    public static readonly MeshAsset Lantern = BuildLantern();
    public static readonly MeshAsset RunestoneShard = BuildRunestoneShard();
    public static readonly MeshAsset ThornveilCottage = BuildThornveilCottage();
    public static readonly MeshAsset RootFence = BuildRootFence();
    public static readonly MeshAsset TreeStump = BuildTreeStump();

    private static MeshAsset BuildHeartwoodTree()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.82f, 0), 0.13f, 1.64f, 7, Bark);
        parts.AddBox(new Vector3(-0.18f, 0.18f, 0.18f), new Vector3(0.7f, 0.08f, 0.12f), BarkDark, -18f);
        parts.AddBox(new Vector3(0.22f, 0.16f, -0.2f), new Vector3(0.66f, 0.08f, 0.12f), BarkDark, 28f);
        parts.AddDiamond(new Vector3(0, 2.08f, 0), new Vector3(0.86f, 0.92f, 0.86f), Leaf);
        parts.AddDiamond(new Vector3(-0.28f, 1.82f, 0.08f), new Vector3(0.68f, 0.72f, 0.68f), LeafDark);
        parts.AddDiamond(new Vector3(0.28f, 1.86f, -0.05f), new Vector3(0.72f, 0.74f, 0.72f), Leaf);
        parts.AddDiamond(new Vector3(0.04f, 2.46f, 0.02f), new Vector3(0.54f, 0.58f, 0.54f), Leaf);
        return new MeshAsset("heartwood-tree", parts.ToArray());
    }

    private static MeshAsset BuildLantern()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.5f, 0), 0.045f, 1.0f, 6, LanternWood);
        parts.AddBox(new Vector3(0, 1.12f, 0), new Vector3(0.34f, 0.34f, 0.12f), LanternGlow);
        parts.AddBox(new Vector3(0, 0.12f, 0), new Vector3(0.22f, 0.08f, 0.22f), LanternWood);
        return new MeshAsset("lantern", parts.ToArray());
    }

    private static MeshAsset BuildRunestoneShard()
    {
        var parts = new MeshBuilder();
        parts.AddBox(new Vector3(0, 0.72f, 0), new Vector3(0.34f, 1.44f, 0.28f), Runestone, -7f);
        parts.AddBox(new Vector3(0.03f, 1.42f, 0), new Vector3(0.24f, 0.16f, 0.3f), Runestone, 6f);
        return new MeshAsset("runestone-shard", parts.ToArray());
    }

    private static MeshAsset BuildThornveilCottage()
    {
        var parts = new MeshBuilder();
        parts.AddBox(new Vector3(0, 0.62f, 0), new Vector3(2.35f, 1.24f, 1.75f), Plaster);
        parts.AddBox(new Vector3(-1.2f, 0.72f, -0.02f), new Vector3(0.16f, 1.36f, 1.92f), WarmTimber);
        parts.AddBox(new Vector3(1.2f, 0.72f, -0.02f), new Vector3(0.16f, 1.36f, 1.92f), WarmTimber);
        parts.AddBox(new Vector3(0, 0.74f, -0.91f), new Vector3(2.55f, 1.34f, 0.12f), WarmTimber);
        parts.AddBox(new Vector3(0, 1.22f, -0.98f), new Vector3(0.74f, 0.44f, 0.14f), WarmTimber);
        parts.AddBox(new Vector3(0, 0.34f, -1.0f), new Vector3(0.48f, 0.68f, 0.14f), BarkDark);
        parts.AddGableRoof(new Vector3(0, 1.56f, 0), new Vector3(2.88f, 0.78f, 2.08f), RoofMoss);
        parts.AddBox(new Vector3(0.72f, 1.42f, 0.38f), new Vector3(0.34f, 0.5f, 0.34f), Bark);
        return new MeshAsset("thornveil-cottage", parts.ToArray());
    }

    private static MeshAsset BuildRootFence()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(-0.72f, 0.34f, 0), 0.05f, 0.68f, 6, OldFence);
        parts.AddCylinder(new Vector3(0.72f, 0.34f, 0), 0.05f, 0.68f, 6, OldFence);
        parts.AddBox(new Vector3(0, 0.46f, 0), new Vector3(1.62f, 0.1f, 0.08f), OldFence, 4f);
        parts.AddBox(new Vector3(0, 0.25f, 0), new Vector3(1.5f, 0.08f, 0.08f), OldFence, -5f);
        return new MeshAsset("root-fence", parts.ToArray());
    }

    private static MeshAsset BuildTreeStump()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.34f, 0), 0.28f, 0.68f, 8, CutWood);
        parts.AddWedge(new Vector3(0.2f, 0.44f, -0.02f), new Vector3(0.32f, 0.18f, 0.22f), BarkDark, 18f);
        return new MeshAsset("tree-stump", parts.ToArray());
    }
}
