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
    public static readonly MeshMaterial MossStone = new("moss-stone", Color.FromArgb(76, 88, 78), Color.FromArgb(146, 166, 132));
    public static readonly MeshMaterial Herb = new("herb", Color.FromArgb(64, 172, 86), Color.FromArgb(158, 236, 138));
    public static readonly MeshMaterial SpiritBlue = new("spirit-blue", Color.FromArgb(58, 136, 164), Color.FromArgb(142, 226, 238));
    public static readonly MeshMaterial Corruption = new("corruption", Color.FromArgb(106, 46, 136), Color.FromArgb(230, 92, 242));
    public static readonly MeshMaterial SigilGold = new("sigil-gold", Color.FromArgb(174, 132, 58), Color.FromArgb(248, 218, 132));
    public static readonly MeshMaterial BannerGreen = new("banner-green", Color.FromArgb(34, 104, 70), Color.FromArgb(132, 214, 132));

    public static readonly MeshAsset HeartwoodTree = BuildHeartwoodTree();
    public static readonly MeshAsset Lantern = BuildLantern();
    public static readonly MeshAsset RunestoneShard = BuildRunestoneShard();
    public static readonly MeshAsset ThornveilCottage = BuildThornveilCottage();
    public static readonly MeshAsset RootFence = BuildRootFence();
    public static readonly MeshAsset TreeStump = BuildTreeStump();
    public static readonly MeshAsset RootArch = BuildRootArch();
    public static readonly MeshAsset MossyRock = BuildMossyRock();
    public static readonly MeshAsset HerbCluster = BuildHerbCluster();
    public static readonly MeshAsset CorruptedSprout = BuildCorruptedSprout();
    public static readonly MeshAsset WorldrootShrine = BuildWorldrootShrine();
    public static readonly MeshAsset ThornveilBanner = BuildThornveilBanner();
    public static readonly MeshAsset ThornveilTreehouse = BuildThornveilTreehouse();
    public static readonly MeshAsset RootBridgeStairs = BuildRootBridgeStairs();
    public static readonly MeshAsset CyanCrystalCluster = BuildCyanCrystalCluster();

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

    private static MeshAsset BuildRootArch()
    {
        var parts = new MeshBuilder();
        parts.AddBox(new Vector3(-0.58f, 0.72f, 0), new Vector3(0.18f, 1.44f, 0.18f), BarkDark, -10f);
        parts.AddBox(new Vector3(0.58f, 0.72f, 0), new Vector3(0.18f, 1.44f, 0.18f), BarkDark, 10f);
        parts.AddBox(new Vector3(0, 1.42f, 0), new Vector3(1.34f, 0.18f, 0.22f), Bark, 0f);
        parts.AddDiamond(new Vector3(0, 1.72f, 0), new Vector3(0.36f, 0.36f, 0.36f), LanternGlow);
        parts.AddWedge(new Vector3(-0.32f, 0.18f, 0.18f), new Vector3(0.5f, 0.12f, 0.24f), Bark, -18f);
        parts.AddWedge(new Vector3(0.34f, 0.18f, -0.16f), new Vector3(0.46f, 0.12f, 0.22f), Bark, 20f);
        return new MeshAsset("root-arch", parts.ToArray());
    }

    private static MeshAsset BuildMossyRock()
    {
        var parts = new MeshBuilder();
        parts.AddWedge(new Vector3(-0.08f, 0.25f, 0), new Vector3(0.94f, 0.5f, 0.66f), MossStone, -8f);
        parts.AddBox(new Vector3(0.18f, 0.48f, -0.08f), new Vector3(0.54f, 0.22f, 0.5f), MossStone, 14f);
        parts.AddWedge(new Vector3(-0.12f, 0.58f, 0.08f), new Vector3(0.42f, 0.16f, 0.36f), LeafDark, 18f);
        return new MeshAsset("mossy-rock", parts.ToArray());
    }

    private static MeshAsset BuildHerbCluster()
    {
        var parts = new MeshBuilder();
        parts.AddDiamond(new Vector3(-0.12f, 0.22f, 0), new Vector3(0.26f, 0.34f, 0.2f), Herb);
        parts.AddDiamond(new Vector3(0.12f, 0.28f, 0.02f), new Vector3(0.3f, 0.42f, 0.22f), Herb);
        parts.AddDiamond(new Vector3(0, 0.34f, -0.12f), new Vector3(0.22f, 0.42f, 0.18f), LanternGlow);
        return new MeshAsset("herb-cluster", parts.ToArray());
    }

    private static MeshAsset BuildCorruptedSprout()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.36f, 0), 0.06f, 0.72f, 5, Corruption);
        parts.AddDiamond(new Vector3(0, 0.88f, 0), new Vector3(0.36f, 0.42f, 0.36f), Corruption);
        parts.AddDiamond(new Vector3(0.22f, 0.5f, 0.08f), new Vector3(0.28f, 0.28f, 0.24f), SpiritBlue);
        return new MeshAsset("corrupted-sprout", parts.ToArray());
    }

    private static MeshAsset BuildWorldrootShrine()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.15f, 0), 0.72f, 0.3f, 10, MossStone);
        parts.AddCylinder(new Vector3(0, 0.78f, 0), 0.18f, 1.28f, 7, BarkDark);
        parts.AddBox(new Vector3(-0.42f, 0.72f, 0.08f), new Vector3(0.12f, 1.18f, 0.14f), Bark, -22f);
        parts.AddBox(new Vector3(0.42f, 0.72f, -0.08f), new Vector3(0.12f, 1.18f, 0.14f), Bark, 22f);
        parts.AddDiamond(new Vector3(0, 1.65f, 0), new Vector3(0.78f, 0.9f, 0.78f), LanternGlow);
        parts.AddDiamond(new Vector3(0, 1.66f, 0), new Vector3(0.42f, 0.5f, 0.42f), SpiritBlue);
        return new MeshAsset("worldroot-shrine", parts.ToArray());
    }

    private static MeshAsset BuildThornveilBanner()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.82f, 0), 0.04f, 1.64f, 6, LanternWood);
        parts.AddBox(new Vector3(0.32f, 1.26f, 0), new Vector3(0.58f, 0.76f, 0.06f), BannerGreen);
        parts.AddBox(new Vector3(0.32f, 1.36f, -0.04f), new Vector3(0.28f, 0.08f, 0.08f), SigilGold);
        parts.AddDiamond(new Vector3(0.32f, 1.18f, -0.05f), new Vector3(0.22f, 0.22f, 0.08f), SigilGold);
        return new MeshAsset("thornveil-banner", parts.ToArray());
    }

    private static MeshAsset BuildThornveilTreehouse()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 1.35f, 0), 0.36f, 2.7f, 9, Bark);
        parts.AddBox(new Vector3(-0.72f, 0.54f, 0.22f), new Vector3(1.42f, 0.12f, 0.16f), BarkDark, -24f);
        parts.AddBox(new Vector3(0.74f, 0.48f, -0.24f), new Vector3(1.34f, 0.12f, 0.16f), BarkDark, 28f);
        parts.AddBox(new Vector3(0, 1.84f, 0), new Vector3(2.05f, 0.16f, 1.58f), WarmTimber, 5f);
        parts.AddBox(new Vector3(0, 2.34f, 0), new Vector3(1.36f, 0.88f, 0.98f), Plaster, 5f);
        parts.AddBox(new Vector3(0, 2.45f, -0.54f), new Vector3(0.34f, 0.42f, 0.08f), SpiritBlue, 5f);
        parts.AddBox(new Vector3(0.7f, 2.42f, 0), new Vector3(0.08f, 0.36f, 0.34f), SpiritBlue, 5f);
        parts.AddGableRoof(new Vector3(0, 2.92f, 0), new Vector3(1.82f, 0.62f, 1.34f), RoofMoss);
        parts.AddDiamond(new Vector3(0, 3.42f, -0.46f), new Vector3(0.24f, 0.38f, 0.16f), LanternGlow);
        parts.AddBox(new Vector3(0, 2.08f, -0.86f), new Vector3(1.74f, 0.08f, 0.07f), BarkDark);
        parts.AddCylinder(new Vector3(-0.78f, 2.12f, -0.86f), 0.035f, 0.52f, 6, BarkDark);
        parts.AddCylinder(new Vector3(0.78f, 2.12f, -0.86f), 0.035f, 0.52f, 6, BarkDark);
        parts.AddDiamond(new Vector3(-0.62f, 3.22f, 0.22f), new Vector3(1.0f, 0.58f, 0.86f), LeafDark);
        parts.AddDiamond(new Vector3(0.52f, 3.42f, -0.04f), new Vector3(1.04f, 0.64f, 0.92f), Leaf);
        parts.AddDiamond(new Vector3(0.02f, 3.18f, 0.68f), new Vector3(1.12f, 0.52f, 0.78f), LeafDark);
        return new MeshAsset("thornveil-treehouse", parts.ToArray());
    }

    private static MeshAsset BuildRootBridgeStairs()
    {
        var parts = new MeshBuilder();
        for (var i = 0; i < 7; i++)
        {
            var z = -1.8f + i * 0.62f;
            parts.AddBox(new Vector3(0, 0.28f + i * 0.025f, z), new Vector3(1.56f, 0.1f, 0.26f),
                i % 2 == 0 ? WarmTimber : Bark, (i - 3) * 1.6f);
        }

        parts.AddBox(new Vector3(-0.88f, 0.46f, 0), new Vector3(0.12f, 0.16f, 4.24f), BarkDark);
        parts.AddBox(new Vector3(0.88f, 0.46f, 0), new Vector3(0.12f, 0.16f, 4.24f), BarkDark);
        parts.AddBox(new Vector3(-1.04f, 1.1f, 0), new Vector3(0.08f, 0.08f, 3.1f), Bark);
        parts.AddBox(new Vector3(1.04f, 1.1f, 0), new Vector3(0.08f, 0.08f, 3.1f), Bark);
        for (var i = 0; i < 5; i++)
        {
            var z = -1.35f + i * 0.68f;
            parts.AddCylinder(new Vector3(-1.04f, 0.78f, z), 0.04f, 0.72f, 6, Bark);
            parts.AddCylinder(new Vector3(1.04f, 0.78f, z), 0.04f, 0.72f, 6, Bark);
        }

        for (var i = 0; i < 4; i++)
        {
            parts.AddBox(new Vector3(0, 0.16f + i * 0.1f, 2.08f + i * 0.32f),
                new Vector3(1.44f, 0.1f, 0.26f), WarmTimber);
        }

        return new MeshAsset("root-bridge-stairs", parts.ToArray());
    }

    private static MeshAsset BuildCyanCrystalCluster()
    {
        var parts = new MeshBuilder();
        parts.AddCylinder(new Vector3(0, 0.12f, 0), 0.58f, 0.24f, 8, MossStone);
        parts.AddDiamond(new Vector3(0.08f, 0.88f, 0.02f), new Vector3(0.62f, 1.55f, 0.5f), LanternGlow);
        parts.AddDiamond(new Vector3(-0.34f, 0.58f, 0.08f), new Vector3(0.34f, 0.9f, 0.3f), SpiritBlue);
        parts.AddDiamond(new Vector3(0.38f, 0.52f, -0.12f), new Vector3(0.3f, 0.78f, 0.28f), LanternGlow);
        parts.AddDiamond(new Vector3(0, 0.4f, -0.38f), new Vector3(0.22f, 0.56f, 0.2f), SpiritBlue);
        parts.AddBox(new Vector3(-0.44f, 0.18f, -0.28f), new Vector3(0.28f, 0.06f, 0.08f), LeafDark, -18f);
        parts.AddBox(new Vector3(0.42f, 0.18f, 0.28f), new Vector3(0.28f, 0.06f, 0.08f), LeafDark, 24f);
        return new MeshAsset("cyan-crystal-cluster", parts.ToArray());
    }
}
