using System.Text.Json;

namespace WorldofEldara.Client3D.Game.Assets;

public sealed class ModelAssetCatalog
{
    public IReadOnlyList<ModelAssetDefinition> Models { get; init; } = [];

    public static ModelAssetCatalog Load(string path)
    {
        if (!File.Exists(path))
        {
            return new ModelAssetCatalog();
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        return JsonSerializer.Deserialize<ModelAssetCatalog>(File.ReadAllText(path), options) ??
               new ModelAssetCatalog();
    }
}

public sealed class ModelAssetDefinition
{
    public string Id { get; init; } = "";
    public string Category { get; init; } = "";
    public string RelativePath { get; init; } = "";
    public float Scale { get; init; } = 1f;
    public float CollisionRadius { get; init; }
    public float CollisionHeight { get; init; }
    public int TriangleBudget { get; init; }
    public string Notes { get; init; } = "";
}
