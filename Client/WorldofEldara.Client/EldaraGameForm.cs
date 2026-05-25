using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WorldofEldara.Client.Game;
using WorldofEldara.Client.Networking;
using WorldofEldara.Client.Rendering;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Client;

public sealed class EldaraGameForm : Form
{
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly System.Windows.Forms.Timer _frameTimer = new();
    private readonly HashSet<Keys> _keysDown = new();
    private readonly EldaraServerClient _serverClient = new();
    private readonly ClientPreferences _preferences;
    private readonly SoftwareRenderer _renderer;
    private readonly WorldScene _scene = new();
    private double _lastFrameTime;
    private double _fpsTimer;
    private int _framesSinceFpsUpdate;
    private double _fps;

    public EldaraGameForm()
    {
        _preferences = ClientPreferences.Load();
        var preferredSize = _preferences.GetWindowSize();
        _renderer = new SoftwareRenderer(preferredSize.Width, preferredSize.Height);
        _scene.SetZoom(_preferences.CameraZoom);

        Text = "World of Eldara - Custom Client";
        ClientSize = preferredSize;
        MinimumSize = new Size(960, 540);
        DoubleBuffered = true;
        KeyPreview = true;

        _serverClient.StatusChanged += (_, status) => _scene.NetworkStatus = status;
        _serverClient.PacketReceived += (_, packet) =>
        {
            _scene.ObserveServerPacket(packet);
            if (packet is CharacterPackets.CharacterListResponse)
            {
                _scene.SelectCharacterById(_preferences.LastCharacterId);
            }
        };

        _frameTimer.Interval = 1;
        _frameTimer.Tick += (_, _) => TickFrame();
        _frameTimer.Start();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (ClientSize.Width > 0 && ClientSize.Height > 0)
        {
            _renderer.Resize(ClientSize.Width, ClientSize.Height);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        _keysDown.Add(e.KeyCode);

        if (e.KeyCode == Keys.C)
        {
            _ = _serverClient.ConnectAndLoginAsync("127.0.0.1", 7777, "Wanderer");
        }

        if (e.KeyCode == Keys.Up)
        {
            _scene.SelectNextCharacter(-1);
        }

        if (e.KeyCode == Keys.Down)
        {
            _scene.SelectNextCharacter(1);
        }

        if (e.KeyCode == Keys.Enter && !_scene.GetDebugSnapshot().IsInWorld)
        {
            if (!_serverClient.IsConnected)
            {
                _ = _serverClient.ConnectAndLoginAsync("127.0.0.1", 7777, "Wanderer");
            }
            else if (_scene.CreateSessionRequest(_serverClient.AccountId) is { } sessionRequest)
            {
                _ = _serverClient.SendPacketAsync(sessionRequest);
            }
        }

        if (e.KeyCode == Keys.E)
        {
            if (_scene.GetDebugSnapshot().IsInventoryOpen)
            {
                var equip = _scene.CreateEquipSelectedItemRequest();
                if (equip is not null)
                {
                    _ = _serverClient.SendPacketAsync(equip);
                }

                return;
            }

            var interaction = _scene.CreateInteractionRequest();
            if (interaction is not null)
            {
                _ = _serverClient.SendPacketAsync(interaction);
            }
        }

        if (e.KeyCode == Keys.Tab)
        {
            _scene.SelectNextEntityTarget();
            e.SuppressKeyPress = true;
            return;
        }

        if (e.KeyCode == Keys.I)
        {
            _scene.ToggleInventory();
        }

        if (e.KeyCode == Keys.V)
        {
            _scene.CycleCameraMode();
        }

        var abilitySlot = e.KeyCode switch
        {
            Keys.D1 or Keys.NumPad1 => 1,
            Keys.D2 or Keys.NumPad2 => 2,
            Keys.D3 or Keys.NumPad3 => 3,
            Keys.D4 or Keys.NumPad4 => 4,
            Keys.D5 or Keys.NumPad5 => 5,
            _ => 0
        };
        if (abilitySlot > 0)
        {
            var ability = _scene.CreateAbilityRequest(abilitySlot);
            if (ability is not null)
            {
                _ = _serverClient.SendUseAbilityAsync(ability);
            }
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (_scene.SelectInventoryAt(e.Location, ClientSize))
            {
                base.OnMouseDown(e);
                return;
            }

            _scene.SelectAt(e.Location, ClientSize);
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        Cursor = _scene.UpdateHoverAt(e.Location, ClientSize) ? Cursors.Hand : Cursors.Default;
        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        _scene.AdjustZoom(e.Delta);
        base.OnMouseWheel(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        var debug = _scene.GetDebugSnapshot();
        _preferences.WindowWidth = ClientSize.Width;
        _preferences.WindowHeight = ClientSize.Height;
        _preferences.CameraZoom = debug.CameraZoom;
        _preferences.LastCharacterId = _scene.GetSelectedCharacterId() ?? _preferences.LastCharacterId;
        _preferences.Save();
        _serverClient.Dispose();
        _renderer.Dispose();
        base.OnFormClosed(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        _scene.Render(_renderer);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        e.Graphics.DrawImageUnscaled(_renderer.Frame, 0, 0);
        DrawWorldLabels(e.Graphics);
        DrawFloatingCombatText(e.Graphics);
        DrawLowHealthOverlay(e.Graphics);
        DrawDeathOverlay(e.Graphics);
        DrawOverlay(e.Graphics);
        DrawSessionPanel(e.Graphics);
        DrawPlayerStatus(e.Graphics);
        DrawTargetStatus(e.Graphics);
        DrawAbilityBar(e.Graphics);
        DrawMiniMap(e.Graphics);
        DrawQuestTracker(e.Graphics);
        DrawLootFeed(e.Graphics);
        DrawInventoryPanel(e.Graphics);
        DrawStatusRibbon(e.Graphics);
        DrawDialoguePanel(e.Graphics);
    }

    private void TickFrame()
    {
        var now = _clock.Elapsed.TotalSeconds;
        var deltaSeconds = Math.Clamp(now - _lastFrameTime, 0.0, 0.1);
        _lastFrameTime = now;
        UpdateFrameTiming(now);

        var movement = _scene.Update((float)deltaSeconds, _keysDown);
        if (movement is not null)
        {
            _ = _serverClient.SendMovementInputAsync(movement);
        }

        Invalidate();
    }

    private void UpdateFrameTiming(double now)
    {
        _framesSinceFpsUpdate++;
        var elapsed = now - _fpsTimer;
        if (elapsed < 0.5)
        {
            return;
        }

        _fps = _framesSinceFpsUpdate / elapsed;
        _framesSinceFpsUpdate = 0;
        _fpsTimer = now;
    }

    private void DrawOverlay(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        using var titleBrush = new SolidBrush(Color.FromArgb(235, 242, 238, 220));
        using var mutedBrush = new SolidBrush(Color.FromArgb(220, 170, 184, 174));
        using var accentBrush = new SolidBrush(Color.FromArgb(230, 147, 205, 142));
        using var panelBrush = new SolidBrush(Color.FromArgb(160, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(120, 114, 171, 124));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 15, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        using var debugFont = new Font(FontFamily.GenericMonospace, 9, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, 16, 16, 460, 154);
        graphics.DrawRectangle(borderPen, 16, 16, 460, 154);
        graphics.DrawString("World of Eldara - 2.5D", titleFont, titleBrush, 30, 28);
        graphics.DrawString("WASD move  |  C connect  |  click/Tab target  |  E interact  |  V camera", bodyFont, mutedBrush, 32, 58);
        graphics.DrawString(debug.NetworkStatus, bodyFont, mutedBrush, 32, 82);

        var localId = debug.LocalEntityId?.ToString() ?? "-";
        var worldState = debug.IsInWorld ? "World" : "Menu";
        graphics.DrawString(
            $"FPS {_fps,5:0.0}  State {worldState}  Camera {debug.CameraMode}  Entity {localId}",
            debugFont,
            accentBrush,
            32,
            112);

        var targetDistance = debug.SelectedTargetDistance is null ? "-" : debug.SelectedTargetDistance.Value.ToString("0.0");
        var target = debug.SelectedTargetName is null
            ? "Target -"
            : $"Target {debug.SelectedTargetName} [{debug.SelectedTargetKind}]  Dist {targetDistance}";
        graphics.DrawString(target, debugFont, accentBrush, 32, 134);
    }

    private void DrawFloatingCombatText(Graphics graphics)
    {
        var texts = _scene.GetCombatText(ClientSize);
        if (texts.Count == 0)
        {
            return;
        }

        using var font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
        using var shadowBrush = new SolidBrush(Color.FromArgb(190, 0, 0, 0));

        foreach (var text in texts)
        {
            var progress = Math.Clamp(text.Progress, 0f, 1f);
            var yOffset = 22f * progress;
            var alpha = (int)(255 * (1f - progress));
            using var brush = new SolidBrush(Color.FromArgb(alpha, text.Color));
            var size = graphics.MeasureString(text.Text, font);
            var x = text.ScreenPosition.X - size.Width / 2f;
            var y = text.ScreenPosition.Y - 40f - yOffset;
            graphics.DrawString(text.Text, font, shadowBrush, x + 1, y + 1);
            graphics.DrawString(text.Text, font, brush, x, y);
        }
    }

    private void DrawWorldLabels(Graphics graphics)
    {
        var labels = _scene.GetWorldLabels(ClientSize);
        if (labels.Count == 0)
        {
            return;
        }

        using var font = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Bold);
        using var selectedFont = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Bold);
        using var shadowBrush = new SolidBrush(Color.FromArgb(190, 0, 0, 0));

        foreach (var label in labels)
        {
            using var brush = new SolidBrush(label.Color);
            var activeFont = label.Emphasis ? selectedFont : font;
            var size = graphics.MeasureString(label.Text, activeFont);
            var x = label.ScreenPosition.X - size.Width / 2f;
            var y = label.ScreenPosition.Y;
            graphics.DrawString(label.Text, activeFont, shadowBrush, x + 1, y + 1);
            graphics.DrawString(label.Text, activeFont, brush, x, y);
        }
    }

    private void DrawPlayerStatus(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || debug.PlayerResources.MaxHealth <= 0)
        {
            return;
        }

        const int x = 16;
        const int y = 184;
        const int width = 360;
        const int height = 166;

        using var panelBrush = new SolidBrush(Color.FromArgb(174, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(128, 114, 171, 124));
        using var titleBrush = new SolidBrush(Color.FromArgb(238, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(222, 184, 202, 184));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);

        var playerName = string.IsNullOrWhiteSpace(debug.PlayerName) ? "Wanderer" : debug.PlayerName;
        var playerClass = string.IsNullOrWhiteSpace(debug.PlayerClass) ? "Adventurer" : debug.PlayerClass;
        graphics.DrawString($"{playerName}  Lv {debug.PlayerLevel} {playerClass}", titleFont, titleBrush, x + 14, y + 12);
        graphics.DrawString($"{debug.PlayerGold} gold  |  {debug.PlayerExperience} XP", bodyFont, bodyBrush, x + 14, y + 34);

        DrawMeter(graphics, x + 14, y + 58, width - 28, "Health",
            debug.PlayerResources.CurrentHealth, debug.PlayerResources.MaxHealth,
            Color.FromArgb(162, 54, 56), bodyFont, bodyBrush);
        DrawMeter(graphics, x + 14, y + 86, width - 28, "Mana",
            debug.PlayerResources.CurrentMana, debug.PlayerResources.MaxMana,
            Color.FromArgb(54, 104, 174), bodyFont, bodyBrush);
        DrawMeter(graphics, x + 14, y + 114, width - 28, "Stamina",
            debug.PlayerResources.CurrentStamina, debug.PlayerResources.MaxStamina,
            Color.FromArgb(74, 150, 87), bodyFont, bodyBrush);
        DrawMeterLong(graphics, x + 14, y + 142, width - 28, "XP",
            debug.PlayerExperience, debug.PlayerExperienceForNextLevel,
            Color.FromArgb(184, 142, 68), bodyFont, bodyBrush);
    }

    private void DrawTargetStatus(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || string.IsNullOrWhiteSpace(debug.SelectedTargetName))
        {
            return;
        }

        const int x = 16;
        const int y = 362;
        const int width = 360;
        const int height = 96;

        using var panelBrush = new SolidBrush(Color.FromArgb(166, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(122, 174, 141, 87));
        using var titleBrush = new SolidBrush(Color.FromArgb(238, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(222, 184, 202, 184));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 10.5f, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString($"{debug.SelectedTargetName} [{debug.SelectedTargetKind}]", titleFont, titleBrush, x + 14, y + 10);

        var distance = debug.SelectedTargetDistance is null ? "-" : $"{debug.SelectedTargetDistance.Value:0.0}m";
        var status = string.IsNullOrWhiteSpace(debug.SelectedTargetState) ? "Selected" : debug.SelectedTargetState;
        graphics.DrawString($"{status}  Dist {distance}", bodyFont, bodyBrush, x + 14, y + 34);

        if (debug.SelectedTargetCurrentHealth is int current && debug.SelectedTargetMaxHealth is int max)
        {
            DrawMeter(graphics, x + 14, y + 62, width - 28, "Health", current, max,
                Color.FromArgb(174, 65, 58), bodyFont, bodyBrush);
        }
    }

    private void DrawAbilityBar(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld)
        {
            return;
        }

        const int slotCount = 12;
        const int slotSize = 42;
        const int gap = 4;
        const int padding = 10;
        var width = slotCount * slotSize + (slotCount - 1) * gap + padding * 2;
        var height = 62;
        var x = Math.Max(16, (ClientSize.Width - width) / 2);
        var y = Math.Max(16, ClientSize.Height - height - 22);

        using var panelBrush = new SolidBrush(Color.FromArgb(178, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(145, 114, 171, 124));
        using var titleBrush = new SolidBrush(Color.FromArgb(238, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(222, 184, 202, 184));
        using var readyBrush = new SolidBrush(Color.FromArgb(236, 147, 205, 142));
        using var cooldownBrush = new SolidBrush(Color.FromArgb(210, 174, 141, 87));
        using var slotFont = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 6.6f, FontStyle.Bold);
        using var smallFont = new Font(FontFamily.GenericSansSerif, 6.2f, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);

        var startX = x + padding;
        var slotY = y + 10;
        for (var i = 0; i < slotCount; i++)
        {
            var slotX = startX + i * (slotSize + gap);
            var slot = debug.AbilitySlots.FirstOrDefault(ability => ability.Slot == i + 1);
            var hasAbility = slot.AbilityId != 0;
            var slotBackColor = hasAbility
                ? Color.FromArgb(slot.IsReady ? 148 : 184, 18, 24, 27)
                : Color.FromArgb(90, 24, 28, 31);
            var slotBorderColor = hasAbility
                ? slot.IsReady
                    ? Color.FromArgb(150, 147, 205, 142)
                    : Color.FromArgb(135, 174, 141, 87)
                : Color.FromArgb(84, 114, 130, 124);
            using var slotBack = new SolidBrush(slotBackColor);
            using var slotBorder = new Pen(slotBorderColor);
            graphics.FillRectangle(slotBack, slotX, slotY, slotSize, slotSize);
            graphics.DrawRectangle(slotBorder, slotX, slotY, slotSize, slotSize);

            graphics.DrawString((i + 1).ToString(), slotFont, titleBrush, slotX + 4, slotY + 3);
            if (!hasAbility)
            {
                graphics.DrawString("-", bodyFont, bodyBrush, slotX + 18, slotY + 17);
                continue;
            }

            if (slot.CooldownRemaining > 0f && slot.CooldownDuration > 0f)
            {
                var ratio = Math.Clamp(slot.CooldownRemaining / slot.CooldownDuration, 0f, 1f);
                using var overlayBrush = new SolidBrush(Color.FromArgb(116, 7, 8, 12));
                graphics.FillRectangle(overlayBrush, slotX, slotY, slotSize, (int)(slotSize * ratio));
            }

            DrawWrappedText(graphics, slot.Name, bodyFont, titleBrush,
                new RectangleF(slotX + 7, slotY + 16, slotSize - 14, 14));

            var footer = slot.CooldownRemaining > 0f
                ? $"{slot.CooldownRemaining:0.0}s"
                : slot.ManaCost > 0
                    ? $"{slot.ManaCost} mana"
                    : slot.Type;
            DrawWrappedText(graphics, footer, smallFont, slot.IsReady ? readyBrush : cooldownBrush,
                new RectangleF(slotX + 5, slotY + 30, slotSize - 10, 10));
        }
    }

    private void DrawSessionPanel(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (debug.IsInWorld)
        {
            return;
        }

        var width = Math.Min(560, ClientSize.Width - 44);
        var height = debug.IsLoggedIn ? 220 : 118;
        var x = (ClientSize.Width - width) / 2;
        var y = Math.Max(186, (ClientSize.Height - height) / 2);

        using var panelBrush = new SolidBrush(Color.FromArgb(188, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(145, 174, 141, 87));
        using var titleBrush = new SolidBrush(Color.FromArgb(242, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(226, 202, 214, 190));
        using var accentBrush = new SolidBrush(Color.FromArgb(235, 147, 205, 142));
        using var selectedBrush = new SolidBrush(Color.FromArgb(62, 83, 62, 40));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        using var slotFont = new Font(FontFamily.GenericMonospace, 9, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString("Enter Eldara", titleFont, titleBrush, x + 18, y + 16);

        if (!debug.IsLoggedIn)
        {
            graphics.DrawString("Press C or Enter to connect to the local server.", bodyFont, bodyBrush, x + 18, y + 52);
            graphics.DrawString(debug.NetworkStatus, bodyFont, accentBrush, x + 18, y + 78);
            return;
        }

        graphics.DrawString("Up/Down chooses a character. Enter enters the world.", bodyFont, bodyBrush, x + 18, y + 50);
        if (debug.CharacterMenuItems.Count == 0)
        {
            graphics.DrawString("No characters on this account.", bodyFont, bodyBrush, x + 18, y + 86);
            graphics.DrawString("Press Enter to create Warden, a Sylvaen Memory Warden prototype.", bodyFont,
                accentBrush, x + 18, y + 116);
            return;
        }

        var slotY = y + 84;
        for (var i = 0; i < debug.CharacterMenuItems.Count; i++)
        {
            var item = debug.CharacterMenuItems[i];
            var isSelected = i == debug.SelectedCharacterIndex;
            if (isSelected)
            {
                graphics.FillRectangle(selectedBrush, x + 18, slotY - 4, width - 36, 28);
            }

            var prefix = isSelected ? ">" : " ";
            graphics.DrawString($"{prefix} {item.Name,-12} Lv {item.Level,-2} {item.Race} {item.Class}",
                slotFont, isSelected ? accentBrush : bodyBrush, x + 24, slotY);
            slotY += 32;
            if (slotY > y + height - 30)
            {
                break;
            }
        }
    }

    private void DrawQuestTracker(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (debug.QuestTrackerItems.Count == 0)
        {
            return;
        }

        var width = Math.Min(340, Math.Max(260, ClientSize.Width / 4));
        var x = ClientSize.Width - width - 18;
        var y = 190;
        var maxHeight = Math.Max(132, ClientSize.Height - y - 112);
        var height = Math.Min(maxHeight, 54 + debug.QuestTrackerItems.Count * 42);

        using var panelBrush = new SolidBrush(Color.FromArgb(172, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(130, 114, 171, 124));
        using var titleBrush = new SolidBrush(Color.FromArgb(235, 242, 238, 220));
        using var bodyBrush = new SolidBrush(Color.FromArgb(224, 184, 202, 184));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString("Quest Tracker", titleFont, titleBrush, x + 14, y + 12);

        var lineY = y + 40;
        foreach (var item in debug.QuestTrackerItems)
        {
            DrawWrappedText(graphics, item, bodyFont, bodyBrush, new RectangleF(x + 14, lineY, width - 28, 34));
            lineY += 42;
        }
    }

    private void DrawLootFeed(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || debug.LootFeedItems.Count == 0)
        {
            return;
        }

        var height = 38 + debug.LootFeedItems.Count * 28;
        var width = Math.Min(360, Math.Max(280, ClientSize.Width / 3));
        var x = ClientSize.Width - width - 18;
        var y = Math.Max(18, ClientSize.Height - height - 104);

        using var panelBrush = new SolidBrush(Color.FromArgb(145, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(112, 174, 141, 87));
        using var titleBrush = new SolidBrush(Color.FromArgb(238, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(226, 216, 190, 132));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Regular);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString("Loot", titleFont, titleBrush, x + 14, y + 10);

        var lineY = y + 34;
        foreach (var item in debug.LootFeedItems)
        {
            DrawWrappedText(graphics, item, bodyFont, bodyBrush, new RectangleF(x + 14, lineY, width - 28, 22));
            lineY += 28;
        }
    }

    private void DrawInventoryPanel(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || !debug.IsInventoryOpen)
        {
            return;
        }

        var panel = WorldScene.GetInventoryPanelBounds(ClientSize);
        var x = panel.X;
        var y = panel.Y;
        var width = panel.Width;
        var height = panel.Height;

        using var panelBrush = new SolidBrush(Color.FromArgb(232, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(150, 174, 141, 87));
        using var slotBrush = new SolidBrush(Color.FromArgb(188, 18, 24, 27));
        using var emptySlotBrush = new SolidBrush(Color.FromArgb(142, 18, 24, 27));
        using var titleBrush = new SolidBrush(Color.FromArgb(242, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(226, 224, 230, 212));
        using var mutedBrush = new SolidBrush(Color.FromArgb(190, 154, 170, 158));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 13, FontStyle.Bold);
        using var itemFont = new Font(FontFamily.GenericSansSerif, 7.3f, FontStyle.Bold);
        using var smallFont = new Font(FontFamily.GenericSansSerif, 7f, FontStyle.Regular);
        using var detailFont = new Font(FontFamily.GenericSansSerif, 8.6f, FontStyle.Regular);
        using var detailTitleFont = new Font(FontFamily.GenericSansSerif, 9.4f, FontStyle.Bold);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString("Inventory", titleFont, titleBrush, x + 18, y + 14);
        graphics.DrawString($"{debug.PlayerGold} gold  |  {debug.InventoryItems.Sum(item => item.Quantity)} items",
            smallFont, bodyBrush, x + 18, y + 42);

        for (var i = 0; i < WorldScene.InventoryVisibleSlots; i++)
        {
            var slot = WorldScene.GetInventorySlotBounds(panel, i);
            var hasItem = i < debug.InventoryItems.Count;
            var isSelected = i == debug.SelectedInventoryIndex;
            graphics.FillRectangle(hasItem ? slotBrush : emptySlotBrush, slot);
            using var slotPen = new Pen(
                isSelected ? Color.FromArgb(238, 217, 112) : GetRarityColor(hasItem ? debug.InventoryItems[i].Rarity : ItemRarity.Common),
                isSelected ? 2f : 1f);
            graphics.DrawRectangle(slotPen, slot);

            if (!hasItem)
            {
                continue;
            }

            var item = debug.InventoryItems[i];
            DrawItemIcon(graphics, new Rectangle(slot.X + 8, slot.Y + 7, 26, 22), item);
            if (debug.EquippedItems.Values.Any(equipped =>
                    equipped.ItemId.Equals(item.ItemId, StringComparison.OrdinalIgnoreCase)))
            {
                using var equippedBrush = new SolidBrush(Color.FromArgb(232, 147, 205, 142));
                graphics.DrawString("E", smallFont, equippedBrush, slot.Right - 14, slot.Bottom - 18);
            }

            DrawWrappedText(graphics, item.Name, itemFont, bodyBrush,
                new RectangleF(slot.X + 6, slot.Y + 25, WorldScene.InventorySlotSize - 12, 20));
            graphics.DrawString(item.Quantity.ToString(), smallFont, mutedBrush, slot.Right - 18, slot.Y + 5);
        }

        var detailsX = x + 420;
        var detailsWidth = width - 438;
        if (detailsWidth <= 0)
        {
            return;
        }

        InventoryItemStack? selectedItem = debug.SelectedInventoryIndex >= 0 &&
                                           debug.SelectedInventoryIndex < debug.InventoryItems.Count
            ? debug.InventoryItems[debug.SelectedInventoryIndex]
            : null;

        graphics.DrawString("Selected", detailTitleFont, titleBrush, detailsX, y + 72);
        if (selectedItem is { } itemDetails)
        {
            using var rarityBrush = new SolidBrush(GetRarityColor(itemDetails.Rarity));
            DrawItemIcon(graphics, new Rectangle(detailsX, y + 96, 42, 38), itemDetails);
            DrawWrappedText(graphics, itemDetails.Name, detailTitleFont, rarityBrush,
                new RectangleF(detailsX + 52, y + 94, detailsWidth - 52, 24));
            var slotText = itemDetails.EquipSlot is null ? itemDetails.Rarity.ToString() : $"{itemDetails.Rarity} {itemDetails.EquipSlot}";
            DrawWrappedText(graphics, slotText, smallFont, mutedBrush,
                new RectangleF(detailsX + 52, y + 119, detailsWidth - 52, 18));
            DrawWrappedText(graphics, itemDetails.Description, detailFont, bodyBrush,
                new RectangleF(detailsX, y + 142, detailsWidth, 50));
            DrawWrappedText(graphics, BuildStatBonusText(itemDetails.StatBonus), detailFont, bodyBrush,
                new RectangleF(detailsX, y + 196, detailsWidth, 34));
            if (itemDetails.EquipSlot is EquipmentSlot compareSlot)
            {
                var equippedText = debug.EquippedItems.TryGetValue(compareSlot, out var equipped)
                    ? $"Equipped: {equipped.Name} ({BuildStatBonusText(equipped.StatBonus)})"
                    : $"Equipped: Empty {FormatEquipmentSlot(compareSlot)} slot";
                DrawWrappedText(graphics, equippedText, smallFont, mutedBrush,
                    new RectangleF(detailsX, y + 226, detailsWidth, 18));
            }
        }
        else
        {
            DrawWrappedText(graphics, "Empty slot", detailFont, mutedBrush,
                new RectangleF(detailsX, y + 96, detailsWidth, 22));
        }

        graphics.DrawString("Equipment", detailTitleFont, titleBrush, detailsX, y + 242);
        DrawEquipmentPaperDoll(graphics, debug, new Rectangle(detailsX, y + 266, detailsWidth, 128),
            titleBrush, bodyBrush, mutedBrush, smallFont);
    }

    private static void DrawEquipmentPaperDoll(Graphics graphics, WorldDebugSnapshot debug, Rectangle bounds,
        Brush titleBrush, Brush bodyBrush, Brush mutedBrush, Font font)
    {
        var gap = 8;
        var slotWidth = Math.Max(92, (bounds.Width - gap) / 2);
        var slotHeight = 36;
        var x1 = bounds.X;
        var x2 = bounds.X + slotWidth + gap;
        var y = bounds.Y;

        DrawEquipmentSlot(graphics, debug, EquipmentSlot.Head,
            new Rectangle(x1, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
        DrawEquipmentSlot(graphics, debug, EquipmentSlot.Chest,
            new Rectangle(x2, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
        y += slotHeight + gap;
        DrawEquipmentSlot(graphics, debug, EquipmentSlot.MainHand,
            new Rectangle(x1, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
        DrawEquipmentSlot(graphics, debug, EquipmentSlot.OffHand,
            new Rectangle(x2, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
        y += slotHeight + gap;
        DrawEquipmentSlot(graphics, debug, EquipmentSlot.Ring,
            new Rectangle(x1, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
        DrawEquipmentSlot(graphics, debug, EquipmentSlot.Trinket,
            new Rectangle(x2, y, slotWidth, slotHeight), titleBrush, bodyBrush, mutedBrush, font);
    }

    private static void DrawEquipmentSlot(Graphics graphics, WorldDebugSnapshot debug, EquipmentSlot slot,
        Rectangle bounds, Brush titleBrush, Brush bodyBrush, Brush mutedBrush, Font font)
    {
        var hasItem = debug.EquippedItems.TryGetValue(slot, out var item);
        using var backBrush = new SolidBrush(Color.FromArgb(hasItem ? 156 : 86, 18, 24, 27));
        using var borderPen = new Pen(hasItem ? GetRarityColor(item!.Rarity) : Color.FromArgb(82, 114, 171, 124),
            hasItem ? 2f : 1f);
        graphics.FillRectangle(backBrush, bounds);
        graphics.DrawRectangle(borderPen, bounds);

        if (hasItem)
        {
            DrawItemIcon(graphics, new Rectangle(bounds.X + 5, bounds.Y + 5, 24, Math.Max(20, bounds.Height - 10)), item!);
            DrawWrappedText(graphics, item!.Name, font, bodyBrush,
                new RectangleF(bounds.X + 32, bounds.Y + 5, bounds.Width - 36, bounds.Height - 8));
            return;
        }

        DrawWrappedText(graphics, FormatEquipmentSlot(slot), font, mutedBrush,
            new RectangleF(bounds.X + 6, bounds.Y + 7, bounds.Width - 12, bounds.Height - 10));
    }

    private static void DrawItemIcon(Graphics graphics, Rectangle bounds, InventoryItemStack item)
    {
        var rarity = GetRarityColor(item.Rarity);
        using var glowBrush = new SolidBrush(Color.FromArgb(46, rarity.R, rarity.G, rarity.B));
        using var rarityPen = new Pen(rarity, 2f);
        using var darkPen = new Pen(Color.FromArgb(210, 10, 12, 15), 2f);
        using var lightPen = new Pen(Color.FromArgb(225, 232, 230, 196), 2f);
        using var fillBrush = new SolidBrush(Color.FromArgb(165, rarity.R, rarity.G, rarity.B));
        using var fillBrushSoft = new SolidBrush(Color.FromArgb(95, rarity.R, rarity.G, rarity.B));

        graphics.FillEllipse(glowBrush, bounds);
        graphics.DrawEllipse(rarityPen, bounds);

        switch (item.EquipSlot)
        {
            case EquipmentSlot.MainHand:
                graphics.DrawLine(darkPen, bounds.Left + 7, bounds.Bottom - 6, bounds.Right - 7, bounds.Top + 5);
                graphics.DrawLine(lightPen, bounds.Left + 9, bounds.Bottom - 7, bounds.Right - 8, bounds.Top + 6);
                graphics.FillRectangle(fillBrush, bounds.Left + 5, bounds.Bottom - 9, 9, 4);
                break;
            case EquipmentSlot.OffHand:
                var shield = new[]
                {
                    new Point(bounds.Left + bounds.Width / 2, bounds.Top + 4),
                    new Point(bounds.Right - 6, bounds.Top + 11),
                    new Point(bounds.Right - 10, bounds.Bottom - 5),
                    new Point(bounds.Left + bounds.Width / 2, bounds.Bottom - 2),
                    new Point(bounds.Left + 6, bounds.Bottom - 5),
                    new Point(bounds.Left + 5, bounds.Top + 11)
                };
                graphics.FillPolygon(fillBrushSoft, shield);
                graphics.DrawPolygon(rarityPen, shield);
                break;
            case EquipmentSlot.Ring:
                graphics.DrawEllipse(lightPen, bounds.Left + 7, bounds.Top + 5, bounds.Width - 14, bounds.Height - 10);
                graphics.DrawEllipse(rarityPen, bounds.Left + 10, bounds.Top + 8, bounds.Width - 20, bounds.Height - 16);
                break;
            case EquipmentSlot.Trinket:
                graphics.FillEllipse(fillBrush, bounds.Left + 8, bounds.Top + 6, bounds.Width - 16, bounds.Height - 12);
                graphics.DrawLine(lightPen, bounds.Left + bounds.Width / 2, bounds.Top + 5,
                    bounds.Left + bounds.Width / 2, bounds.Bottom - 5);
                break;
            case EquipmentSlot.Head:
                graphics.FillPie(fillBrushSoft, bounds.Left + 6, bounds.Top + 7, bounds.Width - 12, bounds.Height - 8,
                    180, 180);
                graphics.DrawArc(rarityPen, bounds.Left + 6, bounds.Top + 7, bounds.Width - 12, bounds.Height - 8,
                    180, 180);
                break;
            case EquipmentSlot.Chest:
                graphics.FillRectangle(fillBrushSoft, bounds.Left + 8, bounds.Top + 7, bounds.Width - 16,
                    bounds.Height - 12);
                graphics.DrawRectangle(rarityPen, bounds.Left + 8, bounds.Top + 7, bounds.Width - 16,
                    bounds.Height - 12);
                break;
            default:
                graphics.FillPolygon(fillBrush, new[]
                {
                    new Point(bounds.Left + bounds.Width / 2, bounds.Top + 4),
                    new Point(bounds.Right - 6, bounds.Top + bounds.Height / 2),
                    new Point(bounds.Left + bounds.Width / 2, bounds.Bottom - 4),
                    new Point(bounds.Left + 6, bounds.Top + bounds.Height / 2)
                });
                break;
        }
    }

    private void DrawMiniMap(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld)
        {
            return;
        }

        var markers = _scene.GetMapMarkers();
        const int size = 154;
        var x = ClientSize.Width - size - 18;
        var y = 18;
        const float range = 18f;
        var centerX = x + size / 2f;
        var centerY = y + size / 2f;

        using var panelBrush = new SolidBrush(Color.FromArgb(166, 8, 12, 18));
        using var innerBrush = new SolidBrush(Color.FromArgb(120, 17, 36, 34));
        using var borderPen = new Pen(Color.FromArgb(130, 114, 171, 124));
        using var ringPen = new Pen(Color.FromArgb(62, 114, 171, 124));
        using var titleBrush = new SolidBrush(Color.FromArgb(225, 224, 230, 212));
        using var font = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Bold);

        graphics.FillRectangle(panelBrush, x, y, size, size);
        graphics.DrawRectangle(borderPen, x, y, size, size);
        graphics.FillEllipse(innerBrush, x + 16, y + 22, size - 32, size - 32);
        graphics.DrawEllipse(ringPen, x + 16, y + 22, size - 32, size - 32);
        graphics.DrawLine(ringPen, centerX, y + 26, centerX, y + size - 18);
        graphics.DrawLine(ringPen, x + 18, centerY + 4, x + size - 18, centerY + 4);
        graphics.DrawString(debug.ActiveZone, font, titleBrush, x + 12, y + 7);

        foreach (var marker in markers)
        {
            var dx = marker.Position.X - debug.PlayerPosition.X;
            var dy = marker.Position.Y - debug.PlayerPosition.Y;
            var distance = MathF.Sqrt(dx * dx + dy * dy);
            var clamped = distance > range && distance > 0.001f;
            if (clamped)
            {
                dx = dx / distance * range;
                dy = dy / distance * range;
            }

            var px = centerX + dx / range * ((size - 42) / 2f);
            var py = centerY + 4 + dy / range * ((size - 42) / 2f);
            var color = GetMarkerColor(marker.Kind, marker.Selected);
            using var markerBrush = new SolidBrush(color);
            using var markerPen = new Pen(Color.FromArgb(210, 6, 8, 10));
            var radius = marker.Kind == MapMarkerKind.Player ? 5 : marker.Selected ? 5 : 3;
            graphics.FillEllipse(markerBrush, px - radius, py - radius, radius * 2, radius * 2);
            graphics.DrawEllipse(markerPen, px - radius, py - radius, radius * 2, radius * 2);

            if (clamped)
            {
                graphics.DrawLine(markerPen, centerX, centerY + 4, px, py);
            }
        }
    }

    private void DrawDialoguePanel(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (string.IsNullOrWhiteSpace(debug.DialogueTitle) && string.IsNullOrWhiteSpace(debug.DialogueBody))
        {
            return;
        }

        var margin = 22;
        var width = Math.Min(760, ClientSize.Width - margin * 2);
        var height = 138;
        var x = (ClientSize.Width - width) / 2;
        var y = ClientSize.Height - height - 104;

        if (y < 248)
        {
            return;
        }

        using var panelBrush = new SolidBrush(Color.FromArgb(188, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(150, 174, 141, 87));
        using var titleBrush = new SolidBrush(Color.FromArgb(242, 229, 204, 164));
        using var bodyBrush = new SolidBrush(Color.FromArgb(232, 224, 230, 212));
        using var actionBrush = new SolidBrush(Color.FromArgb(238, 147, 205, 142));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 13, FontStyle.Bold);
        using var bodyFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        using var actionFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString(debug.DialogueTitle ?? "Dialogue", titleFont, titleBrush, x + 18, y + 14);
        DrawWrappedText(graphics, debug.DialogueBody ?? string.Empty, bodyFont, bodyBrush,
            new RectangleF(x + 18, y + 44, width - 36, 52));

        if (!string.IsNullOrWhiteSpace(debug.DialogueAction))
        {
            DrawWrappedText(graphics, debug.DialogueAction, actionFont, actionBrush,
                new RectangleF(x + 18, y + 104, width - 36, 24));
        }
    }

    private void DrawStatusRibbon(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld)
        {
            return;
        }

        var text = !string.IsNullOrWhiteSpace(debug.CombatHint)
            ? debug.CombatHint
            : !string.IsNullOrWhiteSpace(debug.QuestHint)
                ? debug.QuestHint
                : debug.NetworkStatus;
        if (debug.AggroCount > 0 && !debug.IsPlayerDefeated)
        {
            text = $"{debug.AggroCount} attacker{(debug.AggroCount == 1 ? "" : "s")} on you. {text}";
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var width = Math.Min(520, ClientSize.Width - 44);
        var height = 34;
        var x = (ClientSize.Width - width) / 2;
        var hasDialogue = !string.IsNullOrWhiteSpace(debug.DialogueTitle) || !string.IsNullOrWhiteSpace(debug.DialogueBody);
        var y = Math.Max(18, ClientSize.Height - (hasDialogue ? 252 : 142));

        using var panelBrush = new SolidBrush(Color.FromArgb(150, 8, 12, 18));
        using var borderPen = new Pen(Color.FromArgb(95, 147, 205, 142));
        using var textBrush = new SolidBrush(Color.FromArgb(232, 224, 230, 212));
        using var font = new Font(FontFamily.GenericSansSerif, 9.5f, FontStyle.Bold);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        DrawWrappedText(graphics, text, font, textBrush, new RectangleF(x + 14, y + 8, width - 28, 18));
    }

    private void DrawLowHealthOverlay(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || debug.PlayerResources.MaxHealth <= 0)
        {
            return;
        }

        var ratio = Math.Clamp(debug.PlayerResources.CurrentHealth / (float)debug.PlayerResources.MaxHealth, 0f, 1f);
        if (ratio > 0.32f)
        {
            return;
        }

        var alpha = (int)((0.32f - ratio) / 0.32f * 95);
        using var brush = new SolidBrush(Color.FromArgb(alpha, 126, 34, 36));
        var thickness = Math.Max(18, ClientSize.Width / 32);
        graphics.FillRectangle(brush, 0, 0, ClientSize.Width, thickness);
        graphics.FillRectangle(brush, 0, ClientSize.Height - thickness, ClientSize.Width, thickness);
        graphics.FillRectangle(brush, 0, 0, thickness, ClientSize.Height);
        graphics.FillRectangle(brush, ClientSize.Width - thickness, 0, thickness, ClientSize.Height);
    }

    private void DrawDeathOverlay(Graphics graphics)
    {
        var debug = _scene.GetDebugSnapshot();
        if (!debug.IsInWorld || !debug.IsPlayerDefeated)
        {
            return;
        }

        using var veilBrush = new SolidBrush(Color.FromArgb(116, 13, 8, 10));
        graphics.FillRectangle(veilBrush, 0, 0, ClientSize.Width, ClientSize.Height);

        var width = Math.Min(420, ClientSize.Width - 48);
        var height = 96;
        var x = (ClientSize.Width - width) / 2;
        var y = Math.Max(64, (ClientSize.Height - height) / 2);

        using var panelBrush = new SolidBrush(Color.FromArgb(214, 9, 12, 16));
        using var borderPen = new Pen(Color.FromArgb(210, 206, 122, 112));
        using var titleBrush = new SolidBrush(Color.FromArgb(255, 214, 168, 128));
        using var textBrush = new SolidBrush(Color.FromArgb(236, 224, 230, 212));
        using var titleFont = new Font(FontFamily.GenericSansSerif, 17f, FontStyle.Bold);
        using var font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);

        graphics.FillRectangle(panelBrush, x, y, width, height);
        graphics.DrawRectangle(borderPen, x, y, width, height);
        graphics.DrawString("Defeated", titleFont, titleBrush, x + 18, y + 16);
        graphics.DrawString("Returning to the grove...", font, textBrush, x + 20, y + 58);
    }

    private static void DrawMeter(Graphics graphics, int x, int y, int width, string label, int current, int max,
        Color fillColor, Font font, Brush textBrush)
    {
        if (max <= 0)
        {
            return;
        }

        var ratio = Math.Clamp(current / (float)max, 0f, 1f);
        var barX = x + 74;
        var barWidth = width - 74;
        using var backBrush = new SolidBrush(Color.FromArgb(170, 20, 23, 27));
        using var fillBrush = new SolidBrush(fillColor);
        using var borderPen = new Pen(Color.FromArgb(110, 184, 202, 184));

        graphics.DrawString(label, font, textBrush, x, y - 2);
        graphics.FillRectangle(backBrush, barX, y, barWidth, 12);
        var filledWidth = (int)(barWidth * ratio);
        if (filledWidth > 0)
        {
            graphics.FillRectangle(fillBrush, barX, y, filledWidth, 12);
        }
        graphics.DrawRectangle(borderPen, barX, y, barWidth, 12);
        graphics.DrawString($"{current}/{max}", font, textBrush, barX + 6, y - 2);
    }

    private static void DrawMeterLong(Graphics graphics, int x, int y, int width, string label, long current, long max,
        Color fillColor, Font font, Brush textBrush)
    {
        if (max <= 0)
        {
            return;
        }

        var ratio = Math.Clamp(current / (float)max, 0f, 1f);
        var barX = x + 74;
        var barWidth = width - 74;
        using var backBrush = new SolidBrush(Color.FromArgb(170, 20, 23, 27));
        using var fillBrush = new SolidBrush(fillColor);
        using var borderPen = new Pen(Color.FromArgb(110, 184, 202, 184));

        graphics.DrawString(label, font, textBrush, x, y - 2);
        graphics.FillRectangle(backBrush, barX, y, barWidth, 12);
        var filledWidth = (int)(barWidth * ratio);
        if (filledWidth > 0)
        {
            graphics.FillRectangle(fillBrush, barX, y, filledWidth, 12);
        }

        graphics.DrawRectangle(borderPen, barX, y, barWidth, 12);
        graphics.DrawString($"{current}/{max}", font, textBrush, barX + 6, y - 2);
    }

    private static Color GetMarkerColor(MapMarkerKind kind, bool selected)
    {
        if (selected)
        {
            return Color.FromArgb(245, 230, 132);
        }

        return kind switch
        {
            MapMarkerKind.Player => Color.FromArgb(170, 236, 174),
            MapMarkerKind.Hostile => Color.FromArgb(224, 74, 62),
            MapMarkerKind.Quest => Color.FromArgb(226, 204, 95),
            MapMarkerKind.Npc => Color.FromArgb(126, 184, 154),
            MapMarkerKind.Landmark => Color.FromArgb(132, 164, 224),
            MapMarkerKind.Object => Color.FromArgb(179, 142, 95),
            _ => Color.FromArgb(210, 210, 210)
        };
    }

    private static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Uncommon => Color.FromArgb(98, 210, 116),
            ItemRarity.Rare => Color.FromArgb(96, 153, 230),
            ItemRarity.Epic => Color.FromArgb(181, 113, 226),
            _ => Color.FromArgb(166, 172, 164)
        };
    }

    private static string BuildStatBonusText(StatModifier bonus)
    {
        if (!bonus.HasAnyBonus)
        {
            return "No stat bonus.";
        }

        var parts = new List<string>();
        if (bonus.MaxHealth != 0) parts.Add($"Health +{bonus.MaxHealth}");
        if (bonus.MaxMana != 0) parts.Add($"Mana +{bonus.MaxMana}");
        if (bonus.MaxStamina != 0) parts.Add($"Stamina +{bonus.MaxStamina}");
        if (bonus.AttackPower != 0) parts.Add($"Attack +{bonus.AttackPower}");
        if (bonus.SpellPower != 0) parts.Add($"Spell +{bonus.SpellPower}");
        if (bonus.Armor != 0) parts.Add($"Armor +{bonus.Armor}");
        return string.Join(", ", parts);
    }

    private static string FormatEquipmentSlot(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.MainHand => "Main",
            EquipmentSlot.OffHand => "Off",
            _ => slot.ToString()
        };
    }

    private static void DrawWrappedText(Graphics graphics, string text, Font font, Brush brush, RectangleF bounds)
    {
        using var format = new StringFormat
        {
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit
        };
        graphics.DrawString(text, font, brush, bounds, format);
    }
}
