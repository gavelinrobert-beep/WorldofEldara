using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorldofEldara.Client3D.Game;

public sealed class BlockoutScene
{
    public string Scene { get; init; } = "";
    public IReadOnlyList<BlockoutPlacement> Placements { get; init; } = [];

    public static BlockoutScene Empty { get; } = new();

    public static BlockoutScene LoadDefault()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Scenes",
            "thornveil-enclave-blockout.placements.json");
        if (!File.Exists(path))
        {
            return Empty;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<BlockoutScene>(File.ReadAllText(path), options) ?? Empty;
    }
}

public sealed class BlockoutPlacement
{
    public string Name { get; init; } = "";
    public string Kind { get; init; } = "";
    public float[] Location { get; init; } = [0f, 0f, 0f];
    public float[] Rotation { get; init; } = [0f, 0f, 0f];
    public float[] Scale { get; init; } = [1f, 1f, 1f];
    public string Note { get; init; } = "";

    [JsonIgnore]
    public Vector3 WorldFlat => new(ValueAt(Location, 0), 0f, ValueAt(Location, 1));

    [JsonIgnore]
    public float HeightOffset => ValueAt(Location, 2);

    [JsonIgnore]
    public float YawDegrees => ValueAt(Rotation, 2);

    public float ScaleAt(int index, float fallback = 1f) => ValueAt(Scale, index, fallback);

    private static float ValueAt(IReadOnlyList<float> values, int index, float fallback = 0f) =>
        index >= 0 && index < values.Count ? values[index] : fallback;
}
