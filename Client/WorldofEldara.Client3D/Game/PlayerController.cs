using System.Numerics;
using System.Windows.Forms;
using WorldofEldara.Client3D.Game.Rendering;

namespace WorldofEldara.Client3D.Game;

public sealed class PlayerController
{
    private readonly TerrainSystem _terrain;
    private readonly WorldState _state;

    public PlayerController(TerrainSystem terrain, WorldState state)
    {
        _terrain = terrain;
        _state = state;
    }

    public void Update(float deltaSeconds, IReadOnlySet<Keys> keysDown, WorldCamera camera)
    {
        if (_state.PlayerDefeated)
        {
            _state.PlayerIsMoving = false;
            return;
        }

        var forward = WorldCamera.ForwardFromYaw(camera.Yaw);
        var right = new Vector3(forward.Z, 0, -forward.X);
        var movement = Vector3.Zero;
        if (keysDown.Contains(Keys.W)) movement += forward;
        if (keysDown.Contains(Keys.S)) movement -= forward;
        if (keysDown.Contains(Keys.D)) movement += right;
        if (keysDown.Contains(Keys.A)) movement -= right;

        _state.PlayerIsMoving = movement.LengthSquared() > 0.0001f;
        if (_state.PlayerIsMoving)
        {
            movement = Vector3.Normalize(movement);
            var speed = keysDown.Contains(Keys.ShiftKey) ? 7.4f : 4.9f;
            _state.PlayerPosition += movement * speed * deltaSeconds;
            _state.PlayerYaw = MathF.Atan2(movement.X, movement.Z) * 180f / MathF.PI;
        }

        _state.PlayerPosition = new Vector3(
            _state.PlayerPosition.X,
            _terrain.HeightAt(_state.PlayerPosition.X, _state.PlayerPosition.Z),
            _state.PlayerPosition.Z);
    }
}
