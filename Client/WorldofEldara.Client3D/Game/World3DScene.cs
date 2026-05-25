using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using WorldofEldara.Client3D.Game.Rendering;

namespace WorldofEldara.Client3D.Game;

public sealed class World3DScene
{
    private const float MaxTargetRange = 13f;
    private const float MeleeRange = 3.1f;
    private const float InteractionRange = 3.4f;
    private const float AggroRange = 4.6f;
    private const float LeashRange = 12.5f;
    private const float EnemyAttackRange = 2.15f;
    private const int FirstPruningRequiredKills = 3;

    private readonly SceneData _scene = ThornveilSceneData.Create();
    private readonly WorldCamera _camera = new();
    private readonly WorldRenderer _renderer = new();
    private readonly TerrainSystem _terrain = new();
    private readonly WorldState _state = new();
    private readonly WorldObjectRenderer _objectRenderer;
    private readonly PlayerController _playerController;
    private float _pulse;

    public World3DScene()
    {
        _playerController = new PlayerController(_terrain, _state);
        _objectRenderer = new WorldObjectRenderer(_terrain, _state, _camera);
        _state.PlayerPosition = _terrain.AtGround(Vector3.Zero);

        foreach (var actor in _scene.Actors)
        {
            _state.ActorHealth[actor.Name] = 100f;
            _state.ActorRuntimePositions[actor.Name] = actor.Position;
            _state.ActorSpawnPositions[actor.Name] = actor.Position;
            _state.ActorAttackCooldown[actor.Name] = 0.6f;
        }
    }

    public void ResetCamera()
    {
        _camera.Reset(_state.PlayerYaw, _state.PlayerPosition);
    }

    public void AdjustCameraDistance(int wheelDelta)
    {
        _camera.AdjustDistance(wheelDelta);
    }

    public void OrbitCamera(float yawDelta, float heightDelta)
    {
        _camera.Orbit(yawDelta, heightDelta);
    }

    public void TargetNearest(Size viewport, Point? preferredScreenPoint = null)
    {
        _camera.Build();
        var anchor = preferredScreenPoint is { } point
            ? new PointF(point.X, point.Y)
            : new PointF(viewport.Width * 0.5f, viewport.Height * 0.54f);
        var maxScreenDistanceSquared = preferredScreenPoint.HasValue ? 150f * 150f : float.MaxValue;

        string? bestName = null;
        var bestScore = float.MaxValue;
        foreach (var actor in _scene.Actors)
        {
            if (_state.ActorHealth.GetValueOrDefault(actor.Name, 100f) <= 0f)
            {
                continue;
            }

            var position = _objectRenderer.GetActorWorldPosition(actor, _pulse);
            if (Vector3.Distance(position, _state.PlayerPosition) > MaxTargetRange)
            {
                continue;
            }

            if (!_camera.TryProject(viewport, position + new Vector3(0, actor.Height * 0.7f, 0), out var screen, out var depth))
            {
                continue;
            }

            var dx = screen.X - anchor.X;
            var dy = screen.Y - anchor.Y;
            var screenDistance = dx * dx + dy * dy;
            if (screenDistance > maxScreenDistanceSquared)
            {
                continue;
            }

            var score = screenDistance + depth * 1.4f;
            if (score < bestScore)
            {
                bestScore = score;
                bestName = actor.Name;
            }
        }

        _state.TargetName = bestName;
        _state.StatusText = bestName is null ? "No target in range." : $"Targeting {bestName}.";
    }

    public void UsePrimaryAbility()
    {
        if (_state.PlayerDefeated)
        {
            _state.StatusText = "You are defeated. Respawning soon.";
            return;
        }

        if (_state.PrimaryCooldown > 0f)
        {
            _state.StatusText = $"Basic Strike cooling down: {_state.PrimaryCooldown:0.0}s.";
            return;
        }

        if (_state.TargetName is not { } targetName)
        {
            _state.StatusText = "Select a hostile target first.";
            return;
        }

        if (!TryGetActor(targetName, out var actor))
        {
            _state.TargetName = null;
            _state.StatusText = "Target lost.";
            return;
        }

        if (!actor.Hostile)
        {
            _state.StatusText = $"{targetName} is friendly.";
            return;
        }

        var position = _objectRenderer.GetActorWorldPosition(actor, _pulse);
        var distance = Vector3.Distance(position, _state.PlayerPosition);
        if (distance > MeleeRange)
        {
            _state.StatusText = $"{targetName} is too far away.";
            AddCombatText("Out of range", position + new Vector3(0, actor.Height + 0.4f, 0),
                Color.FromArgb(240, 238, 196, 116), 0.9f);
            return;
        }

        var damage = actor.Name == "Twice-Dead Hare" ? 28f : 22f;
        var health = Math.Max(0f, _state.ActorHealth.GetValueOrDefault(targetName, 100f) - damage);
        _state.ActorHealth[targetName] = health;
        _state.ActorHitFlash[targetName] = 0.22f;
        _state.StrikeFlash = 0.16f;
        _state.StrikeStart = _state.PlayerPosition + new Vector3(0, 0.92f, 0);
        _state.StrikeEnd = position + new Vector3(0, actor.Height * 0.62f, 0);
        _state.PrimaryCooldown = 0.85f;
        _state.StatusText = health <= 0f ? $"{targetName} defeated." : $"Basic Strike hits {targetName} for {damage:0}.";
        AddCombatText($"-{damage:0}", position + new Vector3(0, actor.Height + 0.55f, 0),
            Color.FromArgb(245, 255, 214, 110), 0.8f);

        if (health <= 0f)
        {
            AddCombatText("Defeated", position + new Vector3(0, actor.Height + 0.9f, 0),
                Color.FromArgb(245, 146, 236, 154), 1.2f);
            RegisterDefeat(actor);
            _state.TargetName = null;
        }
    }

    public void Interact()
    {
        if (_state.PlayerDefeated)
        {
            _state.StatusText = "You are defeated. Respawning soon.";
            return;
        }

        if (_state.TargetName is not { } targetName)
        {
            _state.StatusText = "Select a friendly target first.";
            return;
        }

        if (!TryGetActor(targetName, out var actor))
        {
            _state.TargetName = null;
            _state.StatusText = "Target lost.";
            return;
        }

        var position = _objectRenderer.GetActorWorldPosition(actor, _pulse);
        var distance = Vector3.Distance(position, _state.PlayerPosition);
        if (distance > InteractionRange)
        {
            _state.StatusText = $"{targetName} is too far away to interact.";
            AddCombatText("Move closer", position + new Vector3(0, actor.Height + 0.4f, 0),
                Color.FromArgb(240, 214, 232, 174), 0.9f);
            return;
        }

        if (actor.Hostile)
        {
            _state.StatusText = $"{targetName} is hostile. Use Basic Strike on key 1.";
            return;
        }

        if (targetName == "Root Guardian" && _state.FirstPruningKills >= FirstPruningRequiredKills && !_state.FirstPruningTurnedIn)
        {
            _state.FirstPruningTurnedIn = true;
            _state.Experience += 85;
            _state.Gold += 3;
            _state.Level = 2;
            _state.StatusText = "Quest complete: First Pruning. Reward: 85 XP, 3 gold.";
            AddCombatText("Quest complete", position + new Vector3(0, actor.Height + 0.6f, 0),
                Color.FromArgb(245, 178, 238, 164), 1.2f);
            return;
        }

        _state.StatusText = targetName switch
        {
            "Root Guardian" when _state.FirstPruningTurnedIn => "Root Guardian: Thornveil breathes easier for now.",
            "Root Guardian" when _state.FirstPruningKills >= FirstPruningRequiredKills => "Root Guardian: Return your pruning report.",
            "Root Guardian" => "Root Guardian: Prune hostile growth around the glade.",
            "Memory Keeper" => "Memory Keeper: The enclave remembers every footstep.",
            "Elder Thaelir" => "Elder Thaelir: Keep to the lantern paths, Warden.",
            _ => $"{targetName}: Stay close to the glade."
        };
        AddCombatText("Talk", position + new Vector3(0, actor.Height + 0.6f, 0),
            Color.FromArgb(245, 178, 238, 164), 0.85f);
    }

    public void Update(float deltaSeconds, IReadOnlySet<Keys> keysDown)
    {
        _pulse += deltaSeconds;
        _state.PrimaryCooldown = Math.Max(0f, _state.PrimaryCooldown - deltaSeconds);
        _state.StrikeFlash = Math.Max(0f, _state.StrikeFlash - deltaSeconds);
        _state.PlayerHitFlash = Math.Max(0f, _state.PlayerHitFlash - deltaSeconds);
        _state.AggroGraceTimer = Math.Max(0f, _state.AggroGraceTimer - deltaSeconds);
        UpdateHitFlashes(deltaSeconds);
        UpdateCombatTexts(deltaSeconds);
        UpdateRespawn(deltaSeconds);

        _playerController.Update(deltaSeconds, keysDown, _camera);
        UpdateEnemies(deltaSeconds);
        if (_state.PlayerIsMoving)
        {
            _camera.SmoothFollowYaw(_state.PlayerYaw, deltaSeconds);
        }

        _camera.SmoothFocus(_state.PlayerPosition, deltaSeconds);
    }

    public void Render(Graphics graphics, Size viewport, double fps)
    {
        _camera.Build();

        var commands = new List<DrawCommand>(256);
        _renderer.AddTerrainAndPaths(commands, viewport, _scene, _state, _terrain, _camera);
        _objectRenderer.AddSceneObjects(commands, viewport, _scene, _pulse);

        _renderer.RenderFrame(graphics, viewport, commands, g => DrawHud(g, viewport, fps));
    }

    private bool TryGetActor(string name, out WorldActor actor)
    {
        foreach (var candidate in _scene.Actors)
        {
            if (candidate.Name == name)
            {
                actor = candidate;
                return true;
            }
        }

        actor = default;
        return false;
    }

    private void AddCombatText(string text, Vector3 position, Color color, float lifetime)
    {
        _state.CombatTexts.Add(new CombatFloatText(text, position, color, 0f, lifetime));
    }

    private void RegisterDefeat(WorldActor actor)
    {
        if (!actor.Hostile || _state.FirstPruningTurnedIn ||
            _state.FirstPruningKills >= FirstPruningRequiredKills)
        {
            return;
        }

        _state.FirstPruningKills++;
        _state.Experience += 12;
        _state.StatusText = _state.FirstPruningKills >= FirstPruningRequiredKills
            ? "First Pruning complete. Return to Root Guardian."
            : $"First Pruning progress: {_state.FirstPruningKills}/{FirstPruningRequiredKills}.";
    }

    private void UpdateEnemies(float deltaSeconds)
    {
        foreach (var actor in _scene.Actors)
        {
            if (!actor.Hostile)
            {
                continue;
            }

            var health = _state.ActorHealth.GetValueOrDefault(actor.Name, 100f);
            if (health <= 0f)
            {
                _state.AggroActors.Remove(actor.Name);
                continue;
            }

            var position = FlatPosition(_state.ActorRuntimePositions.GetValueOrDefault(actor.Name, actor.Position));
            var spawn = FlatPosition(_state.ActorSpawnPositions.GetValueOrDefault(actor.Name, actor.Position));
            var playerFlat = new Vector3(_state.PlayerPosition.X, 0, _state.PlayerPosition.Z);
            var positionFlat = new Vector3(position.X, 0, position.Z);
            var spawnFlat = new Vector3(spawn.X, 0, spawn.Z);
            var playerDistance = Vector3.Distance(positionFlat, playerFlat);
            var leashDistance = Vector3.Distance(positionFlat, spawnFlat);
            var cooldown = Math.Max(0f, _state.ActorAttackCooldown.GetValueOrDefault(actor.Name, 0f) - deltaSeconds);

            if (!_state.PlayerDefeated && _state.AggroGraceTimer <= 0f && playerDistance <= AggroRange)
            {
                _state.AggroActors.Add(actor.Name);
                if (_state.TargetName is null)
                {
                    _state.StatusText = $"{actor.Name} notices you.";
                }
            }

            if (_state.PlayerDefeated || leashDistance > LeashRange)
            {
                _state.AggroActors.Remove(actor.Name);
            }

            if (_state.AggroActors.Contains(actor.Name))
            {
                if (playerDistance > EnemyAttackRange)
                {
                    var direction = FlatDirection(_state.PlayerPosition - position);
                    var speed = actor.Name == "Twice-Dead Hare" ? 3.1f : 2.25f;
                    position += direction * speed * deltaSeconds;
                }
                else if (cooldown <= 0f)
                {
                    var damage = actor.Name == "Twice-Dead Hare" ? 8f : 11f;
                    DamagePlayer(actor, damage);
                    cooldown = 1.35f;
                }
            }
            else if (Vector3.Distance(positionFlat, spawnFlat) > 0.12f)
            {
                var direction = FlatDirection(spawn - position);
                position += direction * 1.45f * deltaSeconds;
            }

            position = FlatPosition(position);
            _state.ActorRuntimePositions[actor.Name] = position;
            _state.ActorAttackCooldown[actor.Name] = cooldown;
        }
    }

    private void DamagePlayer(WorldActor actor, float damage)
    {
        _state.PlayerHealth = Math.Max(0f, _state.PlayerHealth - damage);
        _state.PlayerHitFlash = 0.24f;
        _state.StatusText = $"{actor.Name} hits you for {damage:0}.";
        AddCombatText($"-{damage:0}", _state.PlayerPosition + new Vector3(0, 1.55f, 0),
            Color.FromArgb(245, 255, 124, 108), 0.75f);

        if (_state.PlayerHealth > 0f)
        {
            return;
        }

        _state.PlayerDefeated = true;
        _state.RespawnTimer = 3.0f;
        _state.AggroGraceTimer = 3.5f;
        _state.TargetName = null;
        _state.AggroActors.Clear();
        ResetHostilesToSpawn();
        _state.StatusText = "You have fallen. Respawning in 3 seconds.";
        AddCombatText("Defeated", _state.PlayerPosition + new Vector3(0, 1.9f, 0),
            Color.FromArgb(245, 255, 92, 92), 1.4f);
    }

    private void UpdateRespawn(float deltaSeconds)
    {
        if (!_state.PlayerDefeated)
        {
            return;
        }

        _state.RespawnTimer -= deltaSeconds;
        if (_state.RespawnTimer > 0f)
        {
            _state.StatusText = $"You have fallen. Respawning in {_state.RespawnTimer:0.0}s.";
            return;
        }

        _state.PlayerDefeated = false;
        _state.PlayerHealth = _state.PlayerMaxHealth;
        _state.AggroGraceTimer = 3.5f;
        _state.PlayerPosition = _terrain.AtGround(Vector3.Zero);
        ResetHostilesToSpawn();
        _state.StatusText = "You return to the Heartbough.";
    }

    private void ResetHostilesToSpawn()
    {
        foreach (var actor in _scene.Actors)
        {
            if (!actor.Hostile || _state.ActorHealth.GetValueOrDefault(actor.Name, 100f) <= 0f)
            {
                continue;
            }

            var spawn = FlatPosition(_state.ActorSpawnPositions.GetValueOrDefault(actor.Name, actor.Position));
            _state.ActorRuntimePositions[actor.Name] = spawn;
            _state.ActorAttackCooldown[actor.Name] = 1.0f;
        }
    }

    private void UpdateCombatTexts(float deltaSeconds)
    {
        for (var i = _state.CombatTexts.Count - 1; i >= 0; i--)
        {
            var text = _state.CombatTexts[i];
            var aged = text with { Age = text.Age + deltaSeconds };
            if (aged.Age >= aged.Lifetime)
            {
                _state.CombatTexts.RemoveAt(i);
            }
            else
            {
                _state.CombatTexts[i] = aged;
            }
        }
    }

    private void UpdateHitFlashes(float deltaSeconds)
    {
        foreach (var name in _state.ActorHitFlash.Keys.ToArray())
        {
            var time = _state.ActorHitFlash[name] - deltaSeconds;
            if (time <= 0f)
            {
                _state.ActorHitFlash.Remove(name);
            }
            else
            {
                _state.ActorHitFlash[name] = time;
            }
        }
    }

    private void DrawHud(Graphics graphics, Size viewport, double fps)
    {
        DrawMainPanel(graphics, fps);
        DrawTargetFrame(graphics);
        DrawMinimap(graphics, viewport);
        DrawQuestTracker(graphics, viewport);
        DrawAbilityBar(graphics, viewport);
        DrawCrosshair(graphics, viewport);
    }

    private void DrawMainPanel(Graphics graphics, double fps)
    {
        using var panelBrush = new SolidBrush(Color.FromArgb(176, 4, 12, 14));
        using var borderPen = new Pen(Color.FromArgb(110, 87, 129, 105));
        graphics.FillRectangle(panelBrush, 18, 18, 560, 136);
        graphics.DrawRectangle(borderPen, 18, 18, 560, 136);

        using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
        using var font = new Font("Consolas", 10f);
        using var titleBrush = new SolidBrush(Color.FromArgb(244, 228, 176));
        using var textBrush = new SolidBrush(Color.FromArgb(205, 226, 218));
        using var greenBrush = new SolidBrush(Color.FromArgb(134, 238, 151));
        graphics.DrawString("World of Eldara - 3D Prototype", titleFont, titleBrush, 34, 30);
        graphics.DrawString("WASD move | click/Tab target | E interact | 1 strike | right-drag camera | R reset", font, textBrush, 34, 62);
        graphics.DrawString($"FPS {fps,5:0.0}  Lvl {_state.Level}  XP {_state.Experience}  Gold {_state.Gold}  Target {_state.TargetName ?? "-"}", font,
            greenBrush, 34, 92);
        graphics.DrawString($"{_state.StatusText}  Cooldown {_state.PrimaryCooldown:0.0}s", font, textBrush, 34, 112);
        DrawBar(graphics, 34, 134, 220, 8, _state.PlayerHealth / _state.PlayerMaxHealth,
            _state.PlayerHitFlash > 0f ? Color.FromArgb(230, 255, 96, 86) : Color.FromArgb(210, 184, 58, 58),
            Color.FromArgb(180, 18, 24, 26));
    }

    private static void DrawBar(Graphics graphics, int x, int y, int width, int height, float ratio, Color fill,
        Color back)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        using var backBrush = new SolidBrush(back);
        using var fillBrush = new SolidBrush(fill);
        using var borderPen = new Pen(Color.FromArgb(138, 190, 212, 172), 1f);
        graphics.FillRectangle(backBrush, x, y, width, height);
        graphics.FillRectangle(fillBrush, x + 1, y + 1, (width - 2) * ratio, height - 2);
        graphics.DrawRectangle(borderPen, x, y, width, height);
    }

    private void DrawTargetFrame(Graphics graphics)
    {
        if (_state.TargetName is not { } targetName || !TryGetActor(targetName, out var actor))
        {
            return;
        }

        var health = Math.Clamp(_state.ActorHealth.GetValueOrDefault(targetName, 100f), 0f, 100f);
        using var panelBrush = new SolidBrush(Color.FromArgb(178, 4, 12, 14));
        using var borderPen = new Pen(actor.Hostile ? Color.FromArgb(178, 218, 82, 68) : Color.FromArgb(178, 222, 208, 112), 1.4f);
        using var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        using var font = new Font("Consolas", 9f);
        using var titleBrush = new SolidBrush(Color.FromArgb(244, 228, 176));
        using var textBrush = new SolidBrush(Color.FromArgb(205, 226, 218));
        using var healthBrush = new SolidBrush(actor.Hostile ? Color.FromArgb(204, 192, 58, 52) : Color.FromArgb(204, 92, 172, 102));
        using var backBrush = new SolidBrush(Color.FromArgb(190, 20, 26, 28));

        var x = 26;
        var y = 168;
        graphics.FillRectangle(panelBrush, x, y, 260, 74);
        graphics.DrawRectangle(borderPen, x, y, 260, 74);
        graphics.DrawString(targetName, titleFont, titleBrush, x + 14, y + 9);
        graphics.DrawString(actor.Hostile ? "Hostile" : "Friendly", font, textBrush, x + 14, y + 31);
        graphics.FillRectangle(backBrush, x + 14, y + 52, 232, 9);
        graphics.FillRectangle(healthBrush, x + 14, y + 52, 232f * health / 100f, 9);
        graphics.DrawRectangle(borderPen, x + 14, y + 52, 232, 9);
    }

    private void DrawAbilityBar(Graphics graphics, Size viewport)
    {
        const int slots = 8;
        const int slotSize = 42;
        const int gap = 6;
        var totalWidth = slots * slotSize + (slots - 1) * gap;
        var startX = (viewport.Width - totalWidth) / 2;
        var y = viewport.Height - 70;

        using var panelBrush = new SolidBrush(Color.FromArgb(160, 4, 10, 12));
        using var borderPen = new Pen(Color.FromArgb(124, 104, 138, 108), 1.2f);
        graphics.FillRectangle(panelBrush, startX - 12, y - 12, totalWidth + 24, slotSize + 24);
        graphics.DrawRectangle(borderPen, startX - 12, y - 12, totalWidth + 24, slotSize + 24);

        for (var i = 0; i < slots; i++)
        {
            var x = startX + i * (slotSize + gap);
            var active = i == 0;
            using var slotBrush = new SolidBrush(active ? Color.FromArgb(210, 54, 72, 52) : Color.FromArgb(180, 12, 18, 20));
            graphics.FillRectangle(slotBrush, x, y, slotSize, slotSize);
            graphics.DrawRectangle(borderPen, x, y, slotSize, slotSize);
            using var font = new Font("Consolas", 9f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(232, 230, 218, 170));
            graphics.DrawString((i + 1).ToString(), font, textBrush, x + 5, y + 4);

            if (active)
            {
                using var abilityBrush = new SolidBrush(Color.FromArgb(232, 226, 196, 92));
                using var abilityPen = new Pen(Color.FromArgb(230, 255, 238, 160), 1.4f);
                graphics.FillEllipse(abilityBrush, x + 13, y + 14, 17, 17);
                graphics.DrawEllipse(abilityPen, x + 13, y + 14, 17, 17);

                if (_state.PrimaryCooldown > 0f)
                {
                    var height = Math.Clamp(_state.PrimaryCooldown / 0.85f, 0f, 1f) * slotSize;
                    using var cooldownBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                    graphics.FillRectangle(cooldownBrush, x, y, slotSize, height);
                }
            }
        }
    }

    private void DrawMinimap(Graphics graphics, Size viewport)
    {
        const int size = 156;
        var x = viewport.Width - size - 26;
        var y = 22;
        var center = new PointF(x + size * 0.5f, y + size * 0.5f);
        var radius = size * 0.43f;

        using var panelBrush = new SolidBrush(Color.FromArgb(176, 4, 12, 14));
        using var mapBrush = new SolidBrush(Color.FromArgb(202, 8, 26, 24));
        using var borderPen = new Pen(Color.FromArgb(142, 116, 154, 118), 1.3f);
        using var gridPen = new Pen(Color.FromArgb(44, 128, 172, 132), 1f);
        graphics.FillRectangle(panelBrush, x - 10, y - 10, size + 20, size + 20);
        graphics.DrawRectangle(borderPen, x - 10, y - 10, size + 20, size + 20);
        graphics.FillEllipse(mapBrush, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        graphics.DrawEllipse(borderPen, center.X - radius, center.Y - radius, radius * 2f, radius * 2f);
        graphics.DrawLine(gridPen, center.X - radius, center.Y, center.X + radius, center.Y);
        graphics.DrawLine(gridPen, center.X, center.Y - radius, center.X, center.Y + radius);

        foreach (var prop in _scene.Props)
        {
            if (prop.Kind is not ("tree" or "water" or "rootgate"))
            {
                continue;
            }

            var point = MapPoint(center, radius, prop.Position);
            using var brush = new SolidBrush(prop.Kind == "water"
                ? Color.FromArgb(210, 84, 172, 202)
                : Color.FromArgb(210, 82, 178, 104));
            graphics.FillEllipse(brush, point.X - 2f, point.Y - 2f, 4f, 4f);
        }

        foreach (var actor in _scene.Actors)
        {
            if (_state.ActorHealth.GetValueOrDefault(actor.Name, 100f) <= 0f)
            {
                continue;
            }

            var actorPosition = _state.ActorRuntimePositions.GetValueOrDefault(actor.Name, actor.Position);
            var point = MapPoint(center, radius, actorPosition);
            using var brush = new SolidBrush(actor.Hostile
                ? Color.FromArgb(232, 222, 76, 64)
                : Color.FromArgb(232, 226, 204, 92));
            graphics.FillEllipse(brush, point.X - 3f, point.Y - 3f, 6f, 6f);
        }

        using var playerBrush = new SolidBrush(Color.FromArgb(245, 184, 226, 188));
        var player = MapPoint(center, radius, _state.PlayerPosition);
        graphics.FillEllipse(playerBrush, player.X - 4f, player.Y - 4f, 8f, 8f);
    }

    private void DrawQuestTracker(Graphics graphics, Size viewport)
    {
        const int width = 320;
        var x = viewport.Width - width - 26;
        var y = 210;
        using var panelBrush = new SolidBrush(Color.FromArgb(166, 4, 12, 14));
        using var borderPen = new Pen(Color.FromArgb(110, 87, 129, 105));
        using var titleFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        using var font = new Font("Segoe UI", 9f);
        using var titleBrush = new SolidBrush(Color.FromArgb(244, 228, 176));
        using var textBrush = new SolidBrush(Color.FromArgb(210, 226, 218));
        using var doneBrush = new SolidBrush(Color.FromArgb(158, 232, 164));
        graphics.FillRectangle(panelBrush, x, y, width, 108);
        graphics.DrawRectangle(borderPen, x, y, width, 108);
        graphics.DrawString("Quest Tracker", titleFont, titleBrush, x + 14, y + 12);

        if (_state.FirstPruningTurnedIn)
        {
            graphics.DrawString("First Pruning: Completed", font, doneBrush, x + 14, y + 42);
            graphics.DrawString("The Root Guardian has accepted your report.", font, textBrush, x + 14, y + 65);
            return;
        }

        var progress = Math.Min(_state.FirstPruningKills, FirstPruningRequiredKills);
        graphics.DrawString("First Pruning", font, titleBrush, x + 14, y + 42);
        graphics.DrawString($"Defeat hostile growth near the glade: {progress}/{FirstPruningRequiredKills}", font,
            progress >= FirstPruningRequiredKills ? doneBrush : textBrush, x + 14, y + 64);
        graphics.DrawString(progress >= FirstPruningRequiredKills ? "Return to Root Guardian." : "Targets: Razor Fern, Sapling, Scout, Hare.", font,
            textBrush, x + 14, y + 84);
    }

    private static PointF MapPoint(PointF center, float radius, Vector3 worldPosition)
    {
        const float mapScale = 17f;
        var x = Math.Clamp(worldPosition.X / mapScale, -1f, 1f);
        var z = Math.Clamp(worldPosition.Z / mapScale, -1f, 1f);
        var length = MathF.Sqrt(x * x + z * z);
        if (length > 1f)
        {
            x /= length;
            z /= length;
        }

        return new PointF(center.X + x * radius, center.Y + z * radius);
    }

    private static Vector3 FlatDirection(Vector3 direction)
    {
        var flat = new Vector3(direction.X, 0, direction.Z);
        return flat.LengthSquared() <= 0.0001f ? Vector3.Zero : Vector3.Normalize(flat);
    }

    private static Vector3 FlatPosition(Vector3 position) => new(position.X, 0, position.Z);

    private static void DrawCrosshair(Graphics graphics, Size viewport)
    {
        var crosshairX = viewport.Width / 2;
        var crosshairY = (int)(viewport.Height * 0.54f);
        using var crosshairPen = new Pen(Color.FromArgb(130, 218, 238, 194), 1.5f);
        graphics.DrawLine(crosshairPen, crosshairX - 8, crosshairY, crosshairX - 2, crosshairY);
        graphics.DrawLine(crosshairPen, crosshairX + 2, crosshairY, crosshairX + 8, crosshairY);
        graphics.DrawLine(crosshairPen, crosshairX, crosshairY - 8, crosshairX, crosshairY - 2);
        graphics.DrawLine(crosshairPen, crosshairX, crosshairY + 2, crosshairX, crosshairY + 8);

    }
}
