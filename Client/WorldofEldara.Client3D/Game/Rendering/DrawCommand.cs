using System.Drawing;

namespace WorldofEldara.Client3D.Game.Rendering;

public readonly record struct DrawCommand(float Depth, Action<Graphics> Draw);
