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

    public TerrainSample Sample(SceneData scene, float x, float z)
    {
        var sample = TerrainSample.Empty;
        foreach (var chunk in scene.TerrainChunks)
        {
            var influence = ChunkInfluence(chunk, x, z);
            if (influence <= 0f)
            {
                continue;
            }

            sample = sample.WithCoverage(MathF.Max(sample.Coverage, influence));
            sample = chunk.Material switch
            {
                TerrainMaterial.Clearing => sample with { Clearing = MathF.Max(sample.Clearing, influence) },
                TerrainMaterial.Path => sample with { Path = MathF.Max(sample.Path, influence) },
                TerrainMaterial.Water => sample with { Water = MathF.Max(sample.Water, influence) },
                TerrainMaterial.Corruption => sample with { Corruption = MathF.Max(sample.Corruption, influence) },
                TerrainMaterial.Shrine => sample with { Shrine = MathF.Max(sample.Shrine, influence) },
                _ => sample with { Forest = MathF.Max(sample.Forest, influence) }
            };
        }

        return sample;
    }

    private static float ChunkInfluence(TerrainChunk chunk, float x, float z)
    {
        var dx = (x - chunk.Center.X) / MathF.Max(0.01f, chunk.RadiusX);
        var dz = (z - chunk.Center.Z) / MathF.Max(0.01f, chunk.RadiusZ);
        var normalized = MathF.Sqrt(dx * dx + dz * dz);
        var softness = Math.Clamp(chunk.EdgeSoftness, 0.05f, 0.9f);
        return Math.Clamp((1f - normalized) / softness, 0f, 1f);
    }
}

public enum TerrainMaterial
{
    Forest,
    Clearing,
    Path,
    Water,
    Corruption,
    Shrine
}

public readonly record struct TerrainChunk(
    string Id,
    Vector3 Center,
    float RadiusX,
    float RadiusZ,
    TerrainMaterial Material,
    float EdgeSoftness = 0.26f);

public readonly record struct TerrainSample(
    float Coverage,
    float Forest,
    float Clearing,
    float Path,
    float Water,
    float Corruption,
    float Shrine)
{
    public static TerrainSample Empty => new(0f, 0f, 0f, 0f, 0f, 0f, 0f);

    public TerrainSample WithCoverage(float coverage) => this with { Coverage = coverage };
}
