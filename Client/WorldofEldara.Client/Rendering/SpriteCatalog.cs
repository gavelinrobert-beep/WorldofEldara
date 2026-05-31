using System.Drawing;
using System.Globalization;

namespace WorldofEldara.Client.Rendering;

public sealed class SpriteCatalog
{
    private readonly Dictionary<string, Sprite> _sprites;

    private SpriteCatalog(Dictionary<string, Sprite> sprites)
    {
        _sprites = sprites;
    }

    public static SpriteCatalog LoadDefault()
    {
        var sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        var spriteRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "Sprites");
        if (!Directory.Exists(spriteRoot))
        {
            return new SpriteCatalog(sprites);
        }

        foreach (var path in Directory.EnumerateFiles(spriteRoot, "*.eldsprite", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var sprite = LoadSprite(path);
                sprites[sprite.Name] = sprite;
            }
            catch
            {
                // Asset failures should never take down the prototype client.
            }
        }

        foreach (var path in Directory.EnumerateFiles(spriteRoot, "*.png", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var sprite = LoadPngSprite(path);
                sprites[sprite.Name] = sprite;
            }
            catch
            {
                // Asset failures should never take down the prototype client.
            }
        }

        return new SpriteCatalog(sprites);
    }

    public bool TryGet(string name, out Sprite sprite) => _sprites.TryGetValue(name, out sprite!);

    private static Sprite LoadSprite(string path)
    {
        var lines = File.ReadAllLines(path);
        var palette = new Dictionary<char, int>();
        var pixelLines = new List<string>();
        var width = 0;
        var height = 0;
        var readingPixels = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            if (readingPixels)
            {
                pixelLines.Add(line);
                continue;
            }

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0].ToLowerInvariant())
            {
                case "size" when parts.Length == 3:
                    width = int.Parse(parts[1], CultureInfo.InvariantCulture);
                    height = int.Parse(parts[2], CultureInfo.InvariantCulture);
                    break;
                case "palette" when parts.Length == 3 && parts[1].Length == 1:
                    palette[parts[1][0]] = ParseColor(parts[2]);
                    break;
                case "pixels":
                    readingPixels = true;
                    break;
            }
        }

        if (width <= 0 || height <= 0)
        {
            throw new InvalidDataException($"Sprite {path} is missing a valid size.");
        }

        var pixels = new int[width * height];
        for (var y = 0; y < height; y++)
        {
            var row = y < pixelLines.Count ? pixelLines[y] : string.Empty;
            for (var x = 0; x < width; x++)
            {
                var symbol = x < row.Length ? row[x] : '.';
                pixels[y * width + x] = palette.TryGetValue(symbol, out var color) ? color : 0;
            }
        }

        return new Sprite(Path.GetFileNameWithoutExtension(path), width, height, pixels);
    }

    private static Sprite LoadPngSprite(string path)
    {
        using var bitmap = new Bitmap(path);
        var pixels = new int[bitmap.Width * bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                pixels[y * bitmap.Width + x] = bitmap.GetPixel(x, y).ToArgb();
            }
        }

        return new Sprite(Path.GetFileNameWithoutExtension(path), bitmap.Width, bitmap.Height, pixels);
    }

    private static int ParseColor(string value)
    {
        if (value.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (value.Length == 7 && value[0] == '#')
        {
            var r = byte.Parse(value[1..3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g = byte.Parse(value[3..5], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b = byte.Parse(value[5..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Color.FromArgb(255, r, g, b).ToArgb();
        }

        if (value.Length == 9 && value[0] == '#')
        {
            var a = byte.Parse(value[1..3], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var r = byte.Parse(value[3..5], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g = byte.Parse(value[5..7], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b = byte.Parse(value[7..9], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Color.FromArgb(a, r, g, b).ToArgb();
        }

        throw new FormatException($"Unsupported sprite color value: {value}");
    }
}
