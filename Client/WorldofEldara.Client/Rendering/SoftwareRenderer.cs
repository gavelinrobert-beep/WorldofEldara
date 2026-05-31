using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WorldofEldara.Client.Rendering;

public sealed class SoftwareRenderer : IDisposable
{
    private int[] _pixels;

    public SoftwareRenderer(int width, int height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        _pixels = new int[Width * Height];
        Frame = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
    }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public Bitmap Frame { get; private set; }

    public void Resize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);
        if (width == Width && height == Height) return;

        Frame.Dispose();
        Width = width;
        Height = height;
        _pixels = new int[Width * Height];
        Frame = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
    }

    public void ClearVerticalGradient(Color top, Color bottom)
    {
        for (var y = 0; y < Height; y++)
        {
            var t = Height <= 1 ? 0f : y / (float)(Height - 1);
            var color = Lerp(top, bottom, t).ToArgb();
            var row = y * Width;
            Array.Fill(_pixels, color, row, Width);
        }
    }

    public void FillRectangle(int x, int y, int width, int height, Color color)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var minX = Math.Clamp(x, 0, Width);
        var minY = Math.Clamp(y, 0, Height);
        var maxX = Math.Clamp(x + width, 0, Width);
        var maxY = Math.Clamp(y + height, 0, Height);
        if (maxX <= minX || maxY <= minY)
        {
            return;
        }

        for (var yy = minY; yy < maxY; yy++)
        {
            FillPixelSpan(yy * Width + minX, maxX - minX, color);
        }
    }

    public void DrawRectangle(int x, int y, int width, int height, Color color)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        DrawLine(x, y, x + width, y, color);
        DrawLine(x + width, y, x + width, y + height, color);
        DrawLine(x + width, y + height, x, y + height, color);
        DrawLine(x, y + height, x, y, color);
    }

    public void FillCircle(int centerX, int centerY, int radius, Color color)
    {
        if (radius <= 0)
        {
            return;
        }

        var radiusSquared = radius * radius;
        var minY = Math.Max(0, centerY - radius);
        var maxY = Math.Min(Height - 1, centerY + radius);

        for (var y = minY; y <= maxY; y++)
        {
            var dy = y - centerY;
            var dx = (int)MathF.Sqrt(radiusSquared - dy * dy);
            var minX = Math.Max(0, centerX - dx);
            var maxX = Math.Min(Width - 1, centerX + dx);
            if (maxX >= minX)
            {
                FillPixelSpan(y * Width + minX, maxX - minX + 1, color);
            }
        }
    }

    public void FillEllipse(int centerX, int centerY, int radiusX, int radiusY, Color color)
    {
        radiusX = Math.Max(1, radiusX);
        radiusY = Math.Max(1, radiusY);
        var minY = Math.Max(0, centerY - radiusY);
        var maxY = Math.Min(Height - 1, centerY + radiusY);
        if (maxY < minY)
        {
            return;
        }

        for (var y = minY; y <= maxY; y++)
        {
            var normalizedY = (y - centerY) / (float)radiusY;
            var span = radiusX * MathF.Sqrt(Math.Max(0, 1f - normalizedY * normalizedY));
            var minX = Math.Max(0, centerX - (int)span);
            var maxX = Math.Min(Width - 1, centerX + (int)span);
            if (maxX >= minX)
            {
                FillPixelSpan(y * Width + minX, maxX - minX + 1, color);
            }
        }
    }

    public void DrawEllipse(int centerX, int centerY, int radiusX, int radiusY, Color color)
    {
        radiusX = Math.Max(1, radiusX);
        radiusY = Math.Max(1, radiusY);
        var steps = Math.Max(28, (radiusX + radiusY) / 2);
        var previousX = centerX + radiusX;
        var previousY = centerY;

        for (var i = 1; i <= steps; i++)
        {
            var angle = i / (float)steps * MathF.Tau;
            var x = centerX + (int)(MathF.Cos(angle) * radiusX);
            var y = centerY + (int)(MathF.Sin(angle) * radiusY);
            DrawLine(previousX, previousY, x, y, color);
            previousX = x;
            previousY = y;
        }
    }

    public void DrawCircle(int centerX, int centerY, int radius, Color color)
    {
        if (radius <= 0)
        {
            return;
        }

        var x = radius;
        var y = 0;
        var error = 0;

        while (x >= y)
        {
            PlotCirclePoints(centerX, centerY, x, y, color);
            y++;
            if (error <= 0)
            {
                error += 2 * y + 1;
            }
            else
            {
                x--;
                error += 2 * (y - x) + 1;
            }
        }
    }

    public void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        var dx = Math.Abs(x1 - x0);
        var sx = x0 < x1 ? 1 : -1;
        var dy = -Math.Abs(y1 - y0);
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;

        while (true)
        {
            SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    public void FillPolygon(IReadOnlyList<PointF> points, Color color)
    {
        if (points.Count < 3)
        {
            return;
        }

        var minY = Math.Clamp((int)MathF.Floor(points.Min(point => point.Y)), 0, Height - 1);
        var maxY = Math.Clamp((int)MathF.Ceiling(points.Max(point => point.Y)), 0, Height - 1);
        if (maxY < minY)
        {
            return;
        }

        var intersections = new List<float>(points.Count);
        for (var y = minY; y <= maxY; y++)
        {
            var scanY = y + 0.5f;
            intersections.Clear();

            for (var i = 0; i < points.Count; i++)
            {
                var a = points[i];
                var b = points[(i + 1) % points.Count];
                if ((a.Y <= scanY && b.Y > scanY) || (b.Y <= scanY && a.Y > scanY))
                {
                    var t = (scanY - a.Y) / (b.Y - a.Y);
                    intersections.Add(a.X + (b.X - a.X) * t);
                }
            }

            intersections.Sort();
            for (var i = 0; i + 1 < intersections.Count; i += 2)
            {
                var minX = Math.Clamp((int)MathF.Ceiling(intersections[i]), 0, Width - 1);
                var maxX = Math.Clamp((int)MathF.Floor(intersections[i + 1]), 0, Width - 1);
                if (maxX >= minX)
                {
                    FillPixelSpan(y * Width + minX, maxX - minX + 1, color);
                }
            }
        }
    }

    public void DrawSprite(Sprite sprite, int anchorX, int anchorY, float scale, SpriteAnchor anchor = SpriteAnchor.BottomCenter)
    {
        var pixelScale = Math.Max(1, (int)MathF.Round(scale));
        var drawWidth = sprite.Width * pixelScale;
        var drawHeight = sprite.Height * pixelScale;
        var startX = anchor switch
        {
            SpriteAnchor.Center => anchorX - drawWidth / 2,
            SpriteAnchor.BottomCenter => anchorX - drawWidth / 2,
            _ => anchorX
        };
        var startY = anchor switch
        {
            SpriteAnchor.Center => anchorY - drawHeight / 2,
            SpriteAnchor.BottomCenter => anchorY - drawHeight,
            _ => anchorY
        };

        for (var sy = 0; sy < sprite.Height; sy++)
        {
            var destY = startY + sy * pixelScale;
            if (destY >= Height)
            {
                break;
            }

            if (destY + pixelScale <= 0)
            {
                continue;
            }

            for (var sx = 0; sx < sprite.Width; sx++)
            {
                var argb = sprite.Pixels[sy * sprite.Width + sx];
                if (((uint)argb & 0xFF000000) == 0)
                {
                    continue;
                }

                var destX = startX + sx * pixelScale;
                if (destX >= Width)
                {
                    break;
                }

                if (destX + pixelScale <= 0)
                {
                    continue;
                }

                FillSpritePixel(destX, destY, pixelScale, argb);
            }
        }
    }

    public void Present()
    {
        var rect = new Rectangle(0, 0, Width, Height);
        var data = Frame.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
        try
        {
            Marshal.Copy(_pixels, 0, data.Scan0, _pixels.Length);
        }
        finally
        {
            Frame.UnlockBits(data);
        }
    }

    public void Dispose()
    {
        Frame.Dispose();
    }

    private void SetPixel(int x, int y, Color color)
    {
        if ((uint)x >= Width || (uint)y >= Height) return;
        var index = y * Width + x;
        var argb = color.ToArgb();
        _pixels[index] = color.A >= 255 ? argb : BlendArgbOver(_pixels[index], argb);
    }

    private void FillPixelSpan(int startIndex, int count, Color color)
    {
        if (count <= 0 || color.A <= 0)
        {
            return;
        }

        var argb = color.ToArgb();
        if (color.A >= 255)
        {
            Array.Fill(_pixels, argb, startIndex, count);
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var index = startIndex + i;
            _pixels[index] = BlendArgbOver(_pixels[index], argb);
        }
    }

    private void FillSpritePixel(int x, int y, int size, int argb)
    {
        var minX = Math.Max(0, x);
        var minY = Math.Max(0, y);
        var maxX = Math.Min(Width, x + size);
        var maxY = Math.Min(Height, y + size);
        if (maxX <= minX || maxY <= minY)
        {
            return;
        }

        for (var yy = minY; yy < maxY; yy++)
        {
            if (((uint)argb & 0xFF000000) == 0xFF000000)
            {
                Array.Fill(_pixels, argb, yy * Width + minX, maxX - minX);
                continue;
            }

            var start = yy * Width + minX;
            for (var xx = 0; xx < maxX - minX; xx++)
            {
                var index = start + xx;
                _pixels[index] = BlendArgbOver(_pixels[index], argb);
            }
        }
    }

    private static int BlendArgbOver(int destination, int source)
    {
        var sourceAlpha = (source >>> 24) & 0xFF;
        if (sourceAlpha <= 0)
        {
            return destination;
        }

        if (sourceAlpha >= 255)
        {
            return source;
        }

        var inverseAlpha = 255 - sourceAlpha;
        var destinationAlpha = (destination >>> 24) & 0xFF;
        var resultAlpha = sourceAlpha + (destinationAlpha * inverseAlpha + 127) / 255;
        var resultRed = (((source >>> 16) & 0xFF) * sourceAlpha + ((destination >>> 16) & 0xFF) * inverseAlpha + 127) / 255;
        var resultGreen = (((source >>> 8) & 0xFF) * sourceAlpha + ((destination >>> 8) & 0xFF) * inverseAlpha + 127) / 255;
        var resultBlue = ((source & 0xFF) * sourceAlpha + (destination & 0xFF) * inverseAlpha + 127) / 255;

        return (resultAlpha << 24) | (resultRed << 16) | (resultGreen << 8) | resultBlue;
    }

    private void PlotCirclePoints(int centerX, int centerY, int x, int y, Color color)
    {
        SetPixel(centerX + x, centerY + y, color);
        SetPixel(centerX + y, centerY + x, color);
        SetPixel(centerX - y, centerY + x, color);
        SetPixel(centerX - x, centerY + y, color);
        SetPixel(centerX - x, centerY - y, color);
        SetPixel(centerX - y, centerY - x, color);
        SetPixel(centerX + y, centerY - x, color);
        SetPixel(centerX + x, centerY - y, color);
    }

    private static Color Lerp(Color a, Color b, float t)
    {
        return Color.FromArgb(
            255,
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }
}

public enum SpriteAnchor
{
    TopLeft,
    Center,
    BottomCenter
}
