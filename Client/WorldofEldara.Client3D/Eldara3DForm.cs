using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WorldofEldara.Client3D.Game;

namespace WorldofEldara.Client3D;

public sealed class Eldara3DForm : Form
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer _frameTimer = new();
    private readonly HashSet<Keys> _keysDown = new();
    private readonly World3DScene _scene = new();
    private double _lastFrameTime;
    private double _fpsTimer;
    private int _framesSinceFpsUpdate;
    private double _fps;
    private bool _isRotatingCamera;
    private Point _lastMousePosition;

    public Eldara3DForm()
    {
        Text = "World of Eldara - 3D Camera Prototype";
        ClientSize = new Size(1600, 900);
        MinimumSize = new Size(960, 540);
        DoubleBuffered = true;
        KeyPreview = true;

        _frameTimer.Interval = 1;
        _frameTimer.Tick += (_, _) => TickFrame();
        _frameTimer.Start();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _keysDown.Add(e.KeyCode);
        if (e.KeyCode == Keys.R)
        {
            _scene.ResetCamera();
        }

        if (e.KeyCode == Keys.Tab)
        {
            _scene.TargetNearest(ClientSize);
        }

        if (e.KeyCode == Keys.E)
        {
            _scene.Interact();
        }

        if (e.KeyCode is Keys.D1 or Keys.NumPad1)
        {
            _scene.UsePrimaryAbility();
        }

        if (e.KeyCode == Keys.Escape)
        {
            Close();
        }

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _keysDown.Remove(e.KeyCode);
        base.OnKeyUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        _scene.AdjustCameraDistance(e.Delta);
        base.OnMouseWheel(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            _isRotatingCamera = true;
            _lastMousePosition = e.Location;
            Capture = true;
        }
        else if (e.Button == MouseButtons.Left)
        {
            _scene.TargetNearest(ClientSize, e.Location);
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isRotatingCamera)
        {
            var deltaX = e.X - _lastMousePosition.X;
            var deltaY = e.Y - _lastMousePosition.Y;
            _scene.OrbitCamera(deltaX * 0.18f, deltaY * 0.015f);
            _lastMousePosition = e.Location;
        }

        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            _isRotatingCamera = false;
            Capture = false;
        }

        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        _scene.Render(e.Graphics, ClientSize, _fps);
    }

    private void TickFrame()
    {
        var now = _clock.Elapsed.TotalSeconds;
        var delta = Math.Clamp(now - _lastFrameTime, 0.001, 0.05);
        _lastFrameTime = now;

        _scene.Update((float)delta, _keysDown);
        _framesSinceFpsUpdate++;
        _fpsTimer += delta;
        if (_fpsTimer >= 0.25)
        {
            _fps = _framesSinceFpsUpdate / _fpsTimer;
            _fpsTimer = 0;
            _framesSinceFpsUpdate = 0;
        }

        Invalidate();
    }
}
