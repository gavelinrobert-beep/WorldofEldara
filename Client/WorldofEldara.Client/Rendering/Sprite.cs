namespace WorldofEldara.Client.Rendering;

public sealed class Sprite
{
    public Sprite(string name, int width, int height, int[] pixels)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (pixels.Length != width * height) throw new ArgumentException("Pixel count must match sprite size.", nameof(pixels));

        Name = name;
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int[] Pixels { get; }
}
