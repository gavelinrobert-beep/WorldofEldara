using System.Drawing;
using System.Windows.Forms;
using WorldofEldara.Client.Rendering;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;
using WorldofEldara.Shared.Data.Quest;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;
using NetVector3 = WorldofEldara.Shared.Protocol.Packets.Vector3;

namespace WorldofEldara.Client.Game;

public readonly record struct WorldDebugSnapshot(
    bool IsInWorld,
    bool IsLoggedIn,
    string ActiveZone,
    ulong? LocalEntityId,
    int VisibleEntityCount,
    PointF PlayerPosition,
    float MovementSpeed,
    float CameraZoom,
    string? PlayerName,
    string? PlayerClass,
    int PlayerLevel,
    long PlayerGold,
    long PlayerExperience,
    long PlayerExperienceForNextLevel,
    ResourceSnapshot PlayerResources,
    int AggroCount,
    bool IsPlayerDefeated,
    string? PrimaryAbilityName,
    float PrimaryAbilityCooldownRemaining,
    float PrimaryAbilityCooldownDuration,
    string? SelectedTargetName,
    string? SelectedTargetKind,
    float? SelectedTargetDistance,
    int? SelectedTargetCurrentHealth,
    int? SelectedTargetMaxHealth,
    string? SelectedTargetState,
    string? QuestHint,
    string? CombatHint,
    string? DialogueTitle,
    string? DialogueBody,
    string? DialogueAction,
    IReadOnlyList<CharacterMenuItem> CharacterMenuItems,
    int SelectedCharacterIndex,
    IReadOnlyList<string> QuestTrackerItems,
    IReadOnlyList<string> LootFeedItems,
    IReadOnlyList<InventoryItemStack> InventoryItems,
    IReadOnlyDictionary<EquipmentSlot, InventoryItemStack> EquippedItems,
    int SelectedInventoryIndex,
    bool IsInventoryOpen,
    IReadOnlyList<AbilitySlotSnapshot> AbilitySlots,
    string CameraMode,
    string NetworkStatus);

public readonly record struct CombatTextSnapshot(string Text, PointF ScreenPosition, float Progress, Color Color);

public readonly record struct CharacterMenuItem(ulong CharacterId, string Name, string Race, string Class, int Level);

public readonly record struct AbilitySlotSnapshot(
    int Slot,
    int AbilityId,
    string Name,
    string Type,
    int ManaCost,
    float CooldownRemaining,
    float CooldownDuration,
    bool IsReady);

public readonly record struct WorldLabelSnapshot(string Text, PointF ScreenPosition, Color Color, bool Emphasis);

public readonly record struct MapMarkerSnapshot(string Name, PointF Position, MapMarkerKind Kind, bool Selected);

public enum MapMarkerKind
{
    Player,
    Hostile,
    Quest,
    Npc,
    Landmark,
    Object
}

internal enum QuestMarkerState
{
    None,
    Available,
    Ready,
    Objective,
    Interactable
}

internal enum CameraMode
{
    Adventure,
    Isometric
}

public sealed class WorldScene
{
    private const float BasePixelsPerWorldUnit = 23f;
    private const float MovementSendInterval = 1f / 20f;
    private const float ProjectionXScale = 0.92f;
    private const float ProjectionYScale = 0.52f;
    private const float IsometricProjectionYScale = 0.48f;
    private const float IsometricHorizonRatio = 0.56f;
    private const float HorizonRatio = 0.62f;
    private const float CameraDefaultYaw = -90f;
    private const float CameraForwardLead = 4.0f;
    private const float PerspectiveScalePerUnit = 0.012f;
    private const float HostileClickRadius = 46f;
    private const float FriendlyClickRadius = 56f;
    private const float QuestClickRadius = 64f;
    private const float LandmarkClickRadius = 30f;
    public const int InventorySlotSize = 54;
    public const int InventoryColumns = 6;
    public const int InventoryVisibleSlots = 18;
    private readonly object _sync = new();
    private readonly Dictionary<ulong, SceneEntity> _entities = new();
    private readonly List<ZoneRegion> _zoneRegions = CreateThornveilRegions();
    private readonly List<TerrainCorridor> _terrainCorridors = CreateTerrainCorridors();
    private readonly List<TerrainPatch> _terrainPatches = CreateTerrainPatches();
    private readonly List<ForestEdge> _forestEdges = CreateForestEdges();
    private readonly List<GroundDetail> _groundDetails = CreateGroundDetails();
    private readonly List<WorldDecoration> _decorations = CreateStarterDecorations();
    private readonly List<AmbientMote> _ambientMotes = CreateAmbientMotes();
    private readonly List<RootTree> _trees = CreateStarterTrees();
    private readonly List<Runestone> _runestones = CreateRunestones();
    private readonly List<MemoryEcho> _memoryEchoes = CreateMemoryEchoes();
    private readonly List<AbilitySummary> _abilities = new();
    private readonly Dictionary<int, float> _abilityCooldowns = new();
    private readonly List<FloatingCombatText> _combatTexts = new();
    private readonly List<LootFeedItem> _lootFeed = new();
    private readonly List<InventoryItemStack> _inventory = new();
    private readonly Dictionary<EquipmentSlot, InventoryItemStack> _equippedItems = new();
    private readonly Dictionary<ulong, float> _aggroWarningTimers = new();
    private readonly Dictionary<int, QuestDefinition> _questDefinitions = new();
    private readonly Dictionary<int, QuestStateData> _questStates = new();
    private readonly HashSet<int> _turnedInQuestIds = new();
    private readonly SpriteCatalog _sprites = SpriteCatalog.LoadDefault();
    private PointF _camera;
    private PointF _player = new(2, 2);
    private string? _playerName;
    private string? _playerClass;
    private int _playerLevel;
    private long _playerGold;
    private long _playerExperience;
    private long _playerExperienceForNextLevel;
    private ResourceSnapshot _playerResources = ResourceSnapshot.Empty;
    private float _movementSpeed = 6.2f;
    private float _movementSendTimer;
    private float _pulse;
    private float _zoom = 1.12f;
    private CameraMode _cameraMode = CameraMode.Adventure;
    private float _cameraYaw = CameraDefaultYaw;
    private bool _isPlayerMoving;
    private float _lastMoveYaw = CameraDefaultYaw;
    private float _playerAttackFlash;
    private PointF? _playerAttackTarget;
    private float _screenShake;
    private bool _isInWorld;
    private bool _isLoggedIn;
    private bool _isInventoryOpen;
    private int _selectedInventoryIndex = -1;
    private string _activeZone = "None";
    private readonly List<CharacterMenuItem> _characterMenuItems = new();
    private int _selectedCharacterIndex;
    private ulong? _currentCharacterId;
    private ulong? _localEntityId;
    private ulong? _selectedEntityId;
    private string? _selectedLandmarkName;
    private ulong? _hoverEntityId;
    private string? _hoverLandmarkName;
    private int? _pendingQuestOfferId;
    private int? _pendingQuestTurnInId;
    private ulong? _pendingQuestTurnInNpcEntityId;
    private int? _pendingQuestTurnInNpcTemplateId;
    private string? _questHint;
    private string? _combatHint;
    private string? _dialogueTitle;
    private string? _dialogueBody;
    private string? _dialogueAction;

    public string NetworkStatus { get; set; } = "Offline. Start server, then press C to connect.";

    public WorldDebugSnapshot GetDebugSnapshot()
    {
        lock (_sync)
        {
            return new WorldDebugSnapshot(
                _isInWorld,
                _isLoggedIn,
                BuildZoneLabel(),
                _localEntityId,
                _entities.Count,
                _player,
                _movementSpeed,
                _zoom,
                _playerName,
                _playerClass,
                _playerLevel,
                _playerGold,
                _playerExperience,
                _playerExperienceForNextLevel,
                _playerResources,
                GetAggroCount(),
                IsPlayerDefeated(),
                GetPrimaryAbility()?.Name,
                GetPrimaryAbilityCooldownRemaining(),
                GetPrimaryAbilityCooldownDuration(),
                GetSelectedTargetName(),
                GetSelectedTargetKind(),
                GetSelectedTargetDistance(),
                GetSelectedTargetCurrentHealth(),
                GetSelectedTargetMaxHealth(),
                GetSelectedTargetState(),
                _questHint,
                _combatHint,
                _dialogueTitle,
                _dialogueBody,
                _dialogueAction,
                _characterMenuItems.ToList(),
                _selectedCharacterIndex,
                BuildQuestTrackerItems(),
                BuildLootFeedItems(),
                _inventory.ToList(),
                new Dictionary<EquipmentSlot, InventoryItemStack>(_equippedItems),
                _selectedInventoryIndex,
                _isInventoryOpen,
                BuildAbilitySlots(),
                GetCameraModeLabel(),
                NetworkStatus);
        }
    }

    public void SelectNextCharacter(int direction)
    {
        lock (_sync)
        {
            if (_characterMenuItems.Count == 0)
            {
                return;
            }

            _selectedCharacterIndex = Mod(_selectedCharacterIndex + direction, _characterMenuItems.Count);
            NetworkStatus = $"Selected slot: {_characterMenuItems[_selectedCharacterIndex].Name}";
        }
    }

    public bool SelectCharacterById(ulong? characterId)
    {
        lock (_sync)
        {
            if (characterId is not ulong id)
            {
                return false;
            }

            var index = _characterMenuItems.FindIndex(character => character.CharacterId == id);
            if (index < 0)
            {
                return false;
            }

            _selectedCharacterIndex = index;
            NetworkStatus = $"Selected slot: {_characterMenuItems[_selectedCharacterIndex].Name}";
            return true;
        }
    }

    public ulong? GetSelectedCharacterId()
    {
        lock (_sync)
        {
            if (_characterMenuItems.Count == 0)
            {
                return null;
            }

            return _characterMenuItems[Math.Clamp(_selectedCharacterIndex, 0, _characterMenuItems.Count - 1)]
                .CharacterId;
        }
    }

    public void SetZoom(float zoom)
    {
        lock (_sync)
        {
            _zoom = Math.Clamp(zoom, 0.72f, 1.9f);
        }
    }

    public void ToggleInventory()
    {
        lock (_sync)
        {
            _isInventoryOpen = !_isInventoryOpen;
            if (_isInventoryOpen && _selectedInventoryIndex < 0 && _inventory.Count > 0)
            {
                _selectedInventoryIndex = 0;
            }

            NetworkStatus = _isInventoryOpen ? "Inventory opened." : "Inventory closed.";
        }
    }

    public void CycleCameraMode()
    {
        lock (_sync)
        {
            _cameraMode = _cameraMode == CameraMode.Adventure ? CameraMode.Isometric : CameraMode.Adventure;
            _cameraYaw = CameraDefaultYaw;
            _camera = GetDesiredCameraTarget();
            NetworkStatus = $"Camera: {GetCameraModeLabel()}";
        }
    }

    public static Rectangle GetInventoryPanelBounds(Size viewport)
    {
        var width = Math.Min(820, Math.Max(720, viewport.Width - 44));
        var height = 410;
        var x = (viewport.Width - width) / 2;
        var y = Math.Max(58, (viewport.Height - height) / 2);
        return new Rectangle(x, y, width, height);
    }

    public static Rectangle GetInventorySlotBounds(Rectangle panel, int index)
    {
        var col = index % InventoryColumns;
        var row = index / InventoryColumns;
        return new Rectangle(
            panel.X + 18 + col * (InventorySlotSize + 10),
            panel.Y + 72 + row * (InventorySlotSize + 14),
            InventorySlotSize,
            InventorySlotSize);
    }

    public bool SelectInventoryAt(Point screenPoint, Size viewport)
    {
        lock (_sync)
        {
            if (!_isInventoryOpen)
            {
                return false;
            }

            var panel = GetInventoryPanelBounds(viewport);
            for (var i = 0; i < InventoryVisibleSlots; i++)
            {
                if (!GetInventorySlotBounds(panel, i).Contains(screenPoint))
                {
                    continue;
                }

                if (i >= _inventory.Count)
                {
                    _selectedInventoryIndex = -1;
                    NetworkStatus = "Empty inventory slot.";
                    return true;
                }

                _selectedInventoryIndex = i;
                var item = _inventory[i];
                NetworkStatus = item.EquipSlot is null
                    ? $"Selected {item.Name}."
                    : $"Selected {item.Name} ({item.EquipSlot}).";
                return true;
            }

            return panel.Contains(screenPoint);
        }
    }

    public PacketBase? CreateEquipSelectedItemRequest()
    {
        lock (_sync)
        {
            if (!_isInventoryOpen)
            {
                return null;
            }

            if (_selectedInventoryIndex < 0 || _selectedInventoryIndex >= _inventory.Count)
            {
                NetworkStatus = "No inventory item selected.";
                return null;
            }

            var item = _inventory[_selectedInventoryIndex];
            if (item.EquipSlot is null)
            {
                NetworkStatus = $"{item.Name} cannot be equipped.";
                return null;
            }

            NetworkStatus = $"Equipping {item.Name}...";
            return new InventoryPackets.EquipItemRequest { ItemId = item.ItemId };
        }
    }

    public PacketBase? CreateSessionRequest(ulong accountId)
    {
        lock (_sync)
        {
            if (_isInWorld)
            {
                return null;
            }

            if (!_isLoggedIn || accountId == 0)
            {
                NetworkStatus = "Connect first, then choose a character.";
                return null;
            }

            if (_characterMenuItems.Count == 0)
            {
                NetworkStatus = "Creating Sylvaen Memory Warden...";
                return new CharacterPackets.CreateCharacterRequest
                {
                    AccountId = accountId,
                    Name = "Warden",
                    Race = Race.Sylvaen,
                    Class = Class.MemoryWarden,
                    Faction = Faction.VerdantCircles,
                    Appearance = new CharacterAppearance()
                };
            }

            var selected = _characterMenuItems[Math.Clamp(_selectedCharacterIndex, 0, _characterMenuItems.Count - 1)];
            NetworkStatus = $"Entering world as {selected.Name}...";
            return new CharacterPackets.SelectCharacterRequest { CharacterId = selected.CharacterId };
        }
    }

    public void AdjustZoom(int wheelDelta)
    {
        lock (_sync)
        {
            var factor = wheelDelta > 0 ? 1.1f : 0.9f;
            _zoom = Math.Clamp(_zoom * factor, 0.72f, 1.9f);
        }
    }

    public IReadOnlyList<CombatTextSnapshot> GetCombatText(Size viewport)
    {
        lock (_sync)
        {
            return _combatTexts
                .Select(text => new CombatTextSnapshot(
                    text.Text,
                    WorldToScreen(viewport.Width, viewport.Height, text.Position, 1.0f),
                    text.Age / text.Lifetime,
                    text.Color))
                .ToList();
        }
    }

    public IReadOnlyList<WorldLabelSnapshot> GetWorldLabels(Size viewport)
    {
        lock (_sync)
        {
            if (!_isInWorld)
            {
                return Array.Empty<WorldLabelSnapshot>();
            }

            var labels = new List<WorldLabelSnapshot>();
            foreach (var entity in _entities.Values)
            {
                var markerState = GetQuestMarkerState(entity);
                var distance = Distance(_player, entity.Position);
                var shouldShow = _selectedEntityId == entity.EntityId ||
                                 _hoverEntityId == entity.EntityId ||
                                 IsAggroOnPlayer(entity) ||
                                 markerState == QuestMarkerState.Ready ||
                                 markerState == QuestMarkerState.Objective ||
                                 (markerState == QuestMarkerState.Available && distance <= 13.5f) ||
                                 (markerState == QuestMarkerState.Interactable && distance <= 5.8f) ||
                                 (entity.IsHostile && distance <= 6.2f);
                if (!shouldShow)
                {
                    continue;
                }

                var p = WorldToScreen(viewport.Width, viewport.Height, entity.Position, 0.9f);
                var color = IsAggroOnPlayer(entity)
                    ? Color.FromArgb(255, 138, 92)
                    : entity.IsHostile
                    ? Color.FromArgb(238, 154, 102)
                    : markerState == QuestMarkerState.Ready
                        ? Color.FromArgb(152, 238, 164)
                    : markerState == QuestMarkerState.Available
                        ? Color.FromArgb(240, 224, 158)
                    : markerState == QuestMarkerState.Objective
                        ? Color.FromArgb(136, 198, 240)
                        : Color.FromArgb(218, 226, 212);
                labels.Add(new WorldLabelSnapshot(BuildEntityLabel(entity, markerState),
                    new PointF(p.X, p.Y - 58 * _zoom), color,
                    _selectedEntityId == entity.EntityId ||
                    markerState is QuestMarkerState.Ready or QuestMarkerState.Available));
            }

            if (_selectedLandmarkName is not null && TryGetLandmarkPosition(_selectedLandmarkName, out var landmark))
            {
                var p = WorldToScreen(viewport.Width, viewport.Height, landmark, 0.55f);
                labels.Add(new WorldLabelSnapshot(_selectedLandmarkName, new PointF(p.X, p.Y - 46 * _zoom),
                    Color.FromArgb(226, 206, 146), true));
            }

            return labels;
        }
    }

    private static string BuildEntityLabel(SceneEntity entity, QuestMarkerState markerState)
    {
        return markerState switch
        {
            QuestMarkerState.Ready => $"{entity.Name} [Turn in]",
            QuestMarkerState.Available => $"{entity.Name} [Quest]",
            QuestMarkerState.Objective => $"{entity.Name} [Objective]",
            QuestMarkerState.Interactable => entity.Name,
            _ => entity.Name
        };
    }

    public IReadOnlyList<MapMarkerSnapshot> GetMapMarkers()
    {
        lock (_sync)
        {
            if (!_isInWorld)
            {
                return Array.Empty<MapMarkerSnapshot>();
            }

            var markers = new List<MapMarkerSnapshot>
            {
                new("You", _player, MapMarkerKind.Player, true),
                new("Worldroot Heart", new PointF(0, 0), MapMarkerKind.Landmark,
                    _selectedLandmarkName == "Worldroot Heart")
            };

            markers.AddRange(_runestones.Select(runestone =>
                new MapMarkerSnapshot(runestone.Name, runestone.Position, MapMarkerKind.Landmark,
                    _selectedLandmarkName == runestone.Name)));
            markers.AddRange(_memoryEchoes.Where(ShouldRenderMemoryEcho).Select(echo =>
                new MapMarkerSnapshot(echo.Name, echo.Position, MapMarkerKind.Object,
                    _selectedLandmarkName == echo.Name)));
            markers.AddRange(_zoneRegions.Where(region => region.Selectable).Select(region =>
                new MapMarkerSnapshot(region.Name, region.Position, MapMarkerKind.Landmark,
                    _selectedLandmarkName == region.Name)));
            markers.AddRange(_decorations.Where(ShouldShowDecorationMarker).Select(decoration =>
                new MapMarkerSnapshot(decoration.Name, decoration.Position, MapMarkerKind.Object,
                    _selectedLandmarkName == decoration.Name)));

            markers.AddRange(_entities.Values.Where(ShouldShowMapEntity).Select(entity =>
            {
                var markerState = GetQuestMarkerState(entity);
                var kind = markerState is QuestMarkerState.Ready or QuestMarkerState.Available or QuestMarkerState.Objective
                    ? MapMarkerKind.Quest
                    : entity.IsHostile ? MapMarkerKind.Hostile :
                    entity.IsQuestGiver ? MapMarkerKind.Quest :
                    MapMarkerKind.Npc;
                return new MapMarkerSnapshot(entity.Name, entity.Position, kind, _selectedEntityId == entity.EntityId);
            }));

            return markers;
        }
    }

    public void SelectAt(Point screenPoint, Size viewport)
    {
        lock (_sync)
        {
            var pick = PickTargetAt(screenPoint, viewport);
            _selectedEntityId = pick.EntityId;
            _selectedLandmarkName = pick.LandmarkName;
            _pendingQuestOfferId = null;
            _pendingQuestTurnInId = null;
            _pendingQuestTurnInNpcEntityId = null;
            _pendingQuestTurnInNpcTemplateId = null;

            if (pick.EntityId is not null && _entities.TryGetValue(pick.EntityId.Value, out var selected))
            {
                NetworkStatus = $"Target: {selected.Name}";
            }
            else if (pick.LandmarkName is not null)
            {
                NetworkStatus = $"Target: {pick.LandmarkName}";
            }
            else
            {
                NetworkStatus = _isInWorld ? "No target selected" : NetworkStatus;
            }
        }
    }

    public bool UpdateHoverAt(Point screenPoint, Size viewport)
    {
        lock (_sync)
        {
            var pick = PickTargetAt(screenPoint, viewport);
            _hoverEntityId = pick.EntityId;
            _hoverLandmarkName = pick.LandmarkName;
            return pick.HasTarget;
        }
    }

    public void SelectNextEntityTarget()
    {
        lock (_sync)
        {
            if (!_isInWorld || _entities.Count == 0)
            {
                NetworkStatus = "No entities nearby.";
                return;
            }

            var candidates = _entities.Values
                .Where(entity => Distance(_player, entity.Position) <= 12f)
                .OrderBy(GetTargetCyclePriority)
                .ThenBy(entity => Distance(_player, entity.Position))
                .ThenBy(entity => entity.EntityId)
                .ToList();

            if (candidates.Count == 0)
            {
                NetworkStatus = "No entities nearby.";
                return;
            }

            var currentIndex = _selectedEntityId is ulong selected
                ? candidates.FindIndex(entity => entity.EntityId == selected)
                : -1;
            var next = candidates[(currentIndex + 1) % candidates.Count];
            _selectedEntityId = next.EntityId;
            _selectedLandmarkName = null;
            _pendingQuestOfferId = null;
            _pendingQuestTurnInId = null;
            _pendingQuestTurnInNpcEntityId = null;
            _pendingQuestTurnInNpcTemplateId = null;
            NetworkStatus = $"Target: {next.Name}";
        }
    }

    private int GetTargetCyclePriority(SceneEntity entity)
    {
        var markerState = GetQuestMarkerState(entity);
        return markerState switch
        {
            QuestMarkerState.Ready => 0,
            QuestMarkerState.Available => 1,
            QuestMarkerState.Objective => 2,
            _ => IsAggroOnPlayer(entity) ? 3 : entity.IsHostile ? 4 : entity.IsQuestGiver ? 5 : 6
        };
    }

    private TargetPick PickTargetAt(Point screenPoint, Size viewport)
    {
        var bestScore = float.MaxValue;
        ulong? bestEntityId = null;
        string? bestLandmark = null;

        foreach (var entity in _entities.Values)
        {
            var foot = WorldToScreen(viewport.Width, viewport.Height, entity.Position);
            var body = WorldToScreen(viewport.Width, viewport.Height, entity.Position, entity.IsHostile ? 0.62f : 0.54f);
            var label = new PointF(body.X, body.Y - 44f * _zoom);
            var distance = Math.Min(Distance(screenPoint, foot),
                Math.Min(Distance(screenPoint, body), Distance(screenPoint, label)));
            var radius = entity.IsQuestGiver ? QuestClickRadius : entity.IsHostile ? HostileClickRadius : FriendlyClickRadius;
            if (distance > radius)
            {
                continue;
            }

            var markerState = GetQuestMarkerState(entity);
            var priority = markerState switch
            {
                QuestMarkerState.Ready => 26f,
                QuestMarkerState.Available => 24f,
                QuestMarkerState.Objective => 22f,
                _ => IsAggroOnPlayer(entity) ? 18f : entity.IsQuestGiver ? 16f : entity.IsHostile ? 10f : 12f
            };
            var score = distance - priority;
            if (score < bestScore)
            {
                bestScore = score;
                bestEntityId = entity.EntityId;
                bestLandmark = null;
            }
        }

        foreach (var runestone in _runestones)
        {
            var p = WorldToScreen(viewport.Width, viewport.Height, runestone.Position);
            var distance = Distance(screenPoint, p);
            var score = distance + 18f;
            if (distance < LandmarkClickRadius && score < bestScore)
            {
                bestScore = score;
                bestEntityId = null;
                bestLandmark = runestone.Name;
            }
        }

        foreach (var echo in _memoryEchoes)
        {
            if (!ShouldRenderMemoryEcho(echo))
            {
                continue;
            }

            var p = WorldToScreen(viewport.Width, viewport.Height, echo.Position, 0.5f);
            var distance = Distance(screenPoint, p);
            var score = distance + 10f;
            if (distance < LandmarkClickRadius + 10f && score < bestScore)
            {
                bestScore = score;
                bestEntityId = null;
                bestLandmark = echo.Name;
            }
        }

        foreach (var region in _zoneRegions.Where(r => r.Selectable))
        {
            var p = WorldToScreen(viewport.Width, viewport.Height, region.Position);
            var distance = Distance(screenPoint, p);
            var score = distance + 34f;
            if (distance < LandmarkClickRadius && score < bestScore)
            {
                bestScore = score;
                bestEntityId = null;
                bestLandmark = region.Name;
            }
        }

        foreach (var decoration in _decorations.Where(d => d.Selectable))
        {
            var p = WorldToScreen(viewport.Width, viewport.Height, decoration.Position);
            var distance = Distance(screenPoint, p);
            var score = distance + 16f;
            if (distance < LandmarkClickRadius + 6f && score < bestScore)
            {
                bestScore = score;
                bestEntityId = null;
                bestLandmark = decoration.Name;
            }
        }

        var worldroot = WorldToScreen(viewport.Width, viewport.Height, new PointF(0, 0));
        var worldrootDistance = Distance(screenPoint, worldroot);
        if (worldrootDistance < LandmarkClickRadius + 22f && worldrootDistance + 28f < bestScore)
        {
            bestEntityId = null;
            bestLandmark = "Worldroot Heart";
        }

        return new TargetPick(bestEntityId, bestLandmark);
    }

    public PacketBase? CreateInteractionRequest()
    {
        lock (_sync)
        {
            if (_pendingQuestTurnInId is int turnInQuestId)
            {
                var npcEntityId = _pendingQuestTurnInNpcEntityId;
                var npcTemplateId = _pendingQuestTurnInNpcTemplateId;
                _pendingQuestTurnInId = null;
                _pendingQuestTurnInNpcEntityId = null;
                _pendingQuestTurnInNpcTemplateId = null;
                _questHint = $"Turning in quest {turnInQuestId}...";
                _dialogueAction = "Completing quest...";
                return new QuestPackets.QuestTurnInRequest
                {
                    QuestId = turnInQuestId,
                    NpcEntityId = npcEntityId,
                    NpcTemplateId = npcTemplateId
                };
            }

            if (_pendingQuestOfferId is int questId)
            {
                _pendingQuestOfferId = null;
                _questHint = $"Accepting quest {questId}...";
                _dialogueAction = "Accepting...";
                return new QuestPackets.QuestAcceptRequest { QuestId = questId };
            }

            if (_selectedEntityId is not ulong entityId || !_entities.TryGetValue(entityId, out var entity))
            {
                if (_selectedLandmarkName is not null &&
                    TryGetServerBackedLandmarkTemplateId(_selectedLandmarkName, out var landmarkTemplateId))
                {
                    _questHint = $"Listening to {_selectedLandmarkName}...";
                    _dialogueTitle = _selectedLandmarkName;
                    _dialogueBody = GetLandmarkDescription(_selectedLandmarkName);
                    _dialogueAction = "Checking Worldroot memory...";
                    NetworkStatus = _questHint;
                    return new QuestPackets.QuestDialogueRequest
                    {
                        NpcEntityId = 0,
                        NpcTemplateId = landmarkTemplateId
                    };
                }

                _questHint = _selectedLandmarkName is null ? "Select a quest NPC first." : $"Inspecting {_selectedLandmarkName}.";
                _dialogueTitle = _selectedLandmarkName;
                _dialogueBody = _selectedLandmarkName is null
                    ? _questHint
                    : GetLandmarkDescription(_selectedLandmarkName);
                _dialogueAction = null;
                NetworkStatus = _questHint;
                return null;
            }

            if (entity.NpcTemplateId is null)
            {
                _questHint = $"{entity.Name} has no dialogue endpoint yet.";
                _dialogueTitle = entity.Name;
                _dialogueBody = _questHint;
                _dialogueAction = null;
                NetworkStatus = _questHint;
                return null;
            }

            _questHint = $"Talking to {entity.Name}...";
            _dialogueTitle = entity.Name;
            _dialogueBody = "Waiting for server dialogue...";
            _dialogueAction = null;
            NetworkStatus = _questHint;
            return new QuestPackets.QuestDialogueRequest
            {
                NpcEntityId = entity.EntityId,
                NpcTemplateId = entity.NpcTemplateId
            };
        }
    }

    public CombatPackets.UseAbilityRequest? CreatePrimaryAbilityRequest()
    {
        return CreateAbilityRequest(1);
    }

    public CombatPackets.UseAbilityRequest? CreateAbilityRequest(int slotNumber)
    {
        lock (_sync)
        {
            var ability = GetAbilityForSlot(slotNumber);
            if (ability is null)
            {
                _combatHint = $"No ability in slot {slotNumber}.";
                NetworkStatus = _combatHint;
                return null;
            }

            if (_abilityCooldowns.TryGetValue(ability.AbilityId, out var cooldownRemaining) && cooldownRemaining > 0f)
            {
                _combatHint = $"{ability.Name} is cooling down: {cooldownRemaining:0.0}s.";
                NetworkStatus = _combatHint;
                return null;
            }

            ulong? targetEntityId = null;
            if (RequiresHostileTarget(ability))
            {
                if (_selectedEntityId is not ulong entityId || !_entities.TryGetValue(entityId, out var entity))
                {
                    _combatHint = "Select a hostile target first.";
                    NetworkStatus = _combatHint;
                    return null;
                }

                if (!entity.IsHostile)
                {
                    _combatHint = $"{entity.Name} is not hostile.";
                    NetworkStatus = _combatHint;
                    return null;
                }

                targetEntityId = entity.EntityId;
                _combatHint = $"Using {ability.Name} on {entity.Name}...";
            }
            else if (ability.TargetType == TargetType.SingleAlly)
            {
                targetEntityId = _localEntityId;
                _combatHint = $"Using {ability.Name} on yourself...";
            }
            else
            {
                _combatHint = $"Using {ability.Name}...";
            }

            NetworkStatus = _combatHint;
            return new CombatPackets.UseAbilityRequest
            {
                AbilityId = ability.AbilityId,
                TargetEntityId = targetEntityId
            };
        }
    }

    public MovementPackets.MovementInputPacket? Update(float deltaSeconds, IReadOnlySet<Keys> keysDown)
    {
        lock (_sync)
        {
            var input = BuildMovementInput(keysDown);
            var hasMovement = MathF.Abs(input.Forward) > 0.001f || MathF.Abs(input.Strafe) > 0.001f;

            if (hasMovement)
            {
                var speed = _movementSpeed * (input.Sprint ? 1.4f : 1f);
                var length = MathF.Sqrt(input.Forward * input.Forward + input.Strafe * input.Strafe);
                _player.X += input.Forward / length * speed * deltaSeconds;
                _player.Y += input.Strafe / length * speed * deltaSeconds;
                _lastMoveYaw = input.LookYaw;
            }

            _isPlayerMoving = hasMovement;
            UpdateCameraFollow(deltaSeconds);
            _pulse += deltaSeconds;
            _screenShake = Math.Max(0f, _screenShake - deltaSeconds * 9f);
            UpdateEntityAnimation(deltaSeconds);
            UpdateCombatText(deltaSeconds);
            UpdateLootFeed(deltaSeconds);
            UpdateAbilityCooldowns(deltaSeconds);

            if (!_isInWorld)
            {
                return null;
            }

            _movementSendTimer += deltaSeconds;
            if (_movementSendTimer < MovementSendInterval)
            {
                return null;
            }

            _movementSendTimer = 0f;
            return new MovementPackets.MovementInputPacket
            {
                DeltaTime = MovementSendInterval,
                Input = input,
                PredictedPosition = ToNetVector(_player),
                PredictedRotationYaw = input.LookYaw
            };
        }
    }

    public void ObserveServerPacket(PacketBase packet)
    {
        lock (_sync)
        {
            switch (packet)
            {
                case AuthPackets.LoginResponse login:
                    _isLoggedIn = login.Result == ResponseCode.Success;
                    NetworkStatus = $"Server login: {login.Result} ({login.ServerProtocolVersion})";
                    break;
                case CharacterPackets.CharacterListResponse list:
                    _characterMenuItems.Clear();
                    foreach (var character in list.Characters)
                    {
                        _characterMenuItems.Add(new CharacterMenuItem(
                            character.CharacterId,
                            character.Name,
                            character.Race.ToString(),
                            character.Class.ToString(),
                            character.Level));
                    }

                    _selectedCharacterIndex = Math.Clamp(_selectedCharacterIndex, 0,
                        Math.Max(0, _characterMenuItems.Count - 1));
                    NetworkStatus = list.Characters.Count == 0
                        ? "No characters found. Press Enter to create Warden."
                        : "Choose a character and press Enter.";
                    break;
                case CharacterPackets.CreateCharacterResponse create:
                    NetworkStatus = create.Result == ResponseCode.Success
                        ? $"Created character: {create.Character?.Name}"
                        : $"Create failed: {create.Message}";
                    break;
                case CharacterPackets.SelectCharacterResponse select:
                    NetworkStatus = select.Result == ResponseCode.Success
                        ? $"Selected character: {select.Character?.Name}"
                        : $"Select failed: {select.Message}";
                    break;
                case WorldPackets.PlayerSpawnPacket spawn:
                    if (_currentCharacterId != spawn.Character.CharacterId)
                    {
                        _turnedInQuestIds.Clear();
                    }

                    _currentCharacterId = spawn.Character.CharacterId;
                    _isInWorld = true;
                    _activeZone = spawn.ZoneId;
                    _player = new PointF(spawn.Character.Position.X, spawn.Character.Position.Y);
                    _cameraYaw = CameraDefaultYaw;
                    _camera = GetDesiredCameraTarget();
                    _playerName = spawn.Character.Name;
                    _playerClass = spawn.Character.Class.ToString();
                    _playerLevel = spawn.Character.Level;
                    _playerGold = spawn.Character.Gold;
                    _playerExperience = spawn.Character.ExperiencePoints;
                    _playerExperienceForNextLevel = Leveling.GetExperienceRequiredForLevel(spawn.Character.Level + 1);
                    _playerResources = spawn.Resources.MaxHealth > 0 ? spawn.Resources : spawn.Character.Resources;
                    _movementSpeed = Math.Max(1f, spawn.Character.Stats.MovementSpeed);
                    _abilities.Clear();
                    _inventory.Clear();
                    _equippedItems.Clear();
                    _abilities.AddRange(spawn.Abilities);
                    _inventory.AddRange(spawn.Character.Inventory);
                    foreach (var equipped in spawn.Character.EquippedItems)
                    {
                        _equippedItems[equipped.Key] = equipped.Value;
                    }

                    ClampSelectedInventory();
                    _combatHint = GetPrimaryAbility() is { } ability
                        ? $"Primary ability: {ability.Name} on key 1."
                        : "No primary ability from server.";
                    NetworkStatus = $"Spawned in {spawn.ZoneId} as {spawn.Character.Name}";
                    break;
                case WorldPackets.EntitySpawnPacket entity:
                    _entities[entity.EntityId] = SceneEntity.FromSpawn(entity);
                    NetworkStatus = $"Entity spawned: {entity.Name}";
                    break;
                case WorldPackets.EntityDespawnPacket despawn:
                    _entities.Remove(despawn.EntityId);
                    if (_selectedEntityId == despawn.EntityId)
                    {
                        _selectedEntityId = null;
                    }

                    NetworkStatus = $"Entity despawned: {despawn.EntityId}";
                    break;
                case MovementPackets.MovementUpdatePacket movement:
                    ApplyMovementUpdate(movement);
                    break;
                case MovementPackets.PositionCorrectionPacket correction:
                    _player = new PointF(correction.AuthoritativePosition.X, correction.AuthoritativePosition.Y);
                    if (_cameraMode == CameraMode.Adventure)
                    {
                        _camera = GetDesiredCameraTarget();
                    }

                    NetworkStatus = "Movement corrected by server";
                    break;
                case WorldPackets.NPCStateUpdatePacket npcState:
                    if (_entities.TryGetValue(npcState.EntityId, out var existing))
                    {
                        var wasTargetingPlayer = IsAggroOnPlayer(existing);
                        existing.State = npcState.State.ToString();
                        existing.TargetEntityId = npcState.TargetEntityId;
                        if (!wasTargetingPlayer && IsAggroOnPlayer(existing))
                        {
                            _aggroWarningTimers[existing.EntityId] = 2.1f;
                            AddCombatText(existing.Position, "AGGRO", Color.FromArgb(244, 112, 74));
                            _combatHint = $"{existing.Name} has noticed you.";
                        }

                        _entities[npcState.EntityId] = existing;
                    }
                    break;
                case QuestPackets.QuestDialogueResponse dialogue:
                    HandleQuestDialogue(dialogue);
                    break;
                case QuestPackets.QuestAcceptResponse accept:
                    if (accept.Definition is not null)
                    {
                        _questDefinitions[accept.Definition.QuestId] = accept.Definition;
                    }

                    if (accept.State is not null)
                    {
                        _turnedInQuestIds.Remove(accept.State.QuestId);
                        _questStates[accept.State.QuestId] = accept.State;
                    }

                    _questHint = accept.Result == ResponseCode.Success && accept.Definition is not null
                        ? $"Quest accepted: {accept.Definition.Title}"
                        : $"Quest accept: {accept.Message}";
                    _dialogueTitle = accept.Definition?.Title ?? "Quest";
                    _dialogueBody = accept.Message;
                    _dialogueAction = accept.Result == ResponseCode.Success ? "Quest tracker updated." : null;
                    NetworkStatus = _questHint;
                    break;
                case QuestPackets.QuestTurnInResponse turnIn:
                    if (turnIn.Definition is not null)
                    {
                        _questDefinitions[turnIn.Definition.QuestId] = turnIn.Definition;
                    }

                    if (turnIn.State is not null)
                    {
                        if (turnIn.Result == ResponseCode.Success)
                        {
                            _turnedInQuestIds.Add(turnIn.State.QuestId);
                            _questStates.Remove(turnIn.State.QuestId);
                        }
                        else
                        {
                            _questStates[turnIn.State.QuestId] = turnIn.State;
                        }
                    }

                    _questHint = turnIn.Result == ResponseCode.Success && turnIn.Definition is not null
                        ? $"Quest completed: {turnIn.Definition.Title}"
                        : $"Quest turn-in: {turnIn.Message}";
                    _dialogueTitle = turnIn.Definition?.Title ?? "Quest";
                    _dialogueBody = turnIn.Result == ResponseCode.Success && turnIn.Definition is not null
                        ? BuildRewardText(turnIn.Definition)
                        : turnIn.Message;
                    _dialogueAction = turnIn.Result == ResponseCode.Success ? "Rewards received." : null;
                    NetworkStatus = _questHint;
                    break;
                case QuestPackets.QuestProgressUpdate progress:
                    var progressDefinition = progress.Definition ??
                                             (_questDefinitions.TryGetValue(progress.State.QuestId, out var knownDefinition)
                                                 ? knownDefinition
                                                 : null);
                    if (progress.Definition is not null)
                    {
                        _questDefinitions[progress.Definition.QuestId] = progress.Definition;
                    }

                    if (!_turnedInQuestIds.Contains(progress.State.QuestId))
                    {
                        _questStates[progress.State.QuestId] = progress.State;
                    }

                    _questHint = progressDefinition is not null
                        ? $"{progressDefinition.Title}: {progress.State.State}"
                        : $"Quest {progress.State.QuestId}: {progress.State.State}";
                    _dialogueTitle = progressDefinition?.Title ?? "Quest Progress";
                    _dialogueBody = BuildQuestProgressText(progress.State, progressDefinition);
                    _dialogueAction = progress.State.State == QuestState.Completed ? "Objective complete." : "Progress updated.";
                    AddCombatText(_player, BuildQuestProgressPulseText(progress.State, progressDefinition),
                        progress.State.State == QuestState.Completed
                            ? Color.FromArgb(226, 210, 238, 150)
                            : Color.FromArgb(142, 214, 132));
                    NetworkStatus = _questHint;
                    break;
                case QuestPackets.QuestLogSnapshot snapshot:
                    _questDefinitions.Clear();
                    foreach (var definition in snapshot.Definitions)
                    {
                        _questDefinitions[definition.QuestId] = definition;
                    }

                    _questStates.Clear();
                    foreach (var state in snapshot.States)
                    {
                        if (_turnedInQuestIds.Contains(state.QuestId))
                        {
                            continue;
                        }

                        _questStates[state.QuestId] = state;
                    }

                    _questHint = snapshot.States.Count == 0
                        ? "Quest log empty."
                        : $"Quest log: {snapshot.States.Count} tracked.";
                    break;
                case CombatPackets.AbilityResultPacket result:
                    if (result.Result == ResponseCode.Success)
                    {
                        var usedAbility = _abilities.FirstOrDefault(a => a.AbilityId == result.AbilityId);
                        if (usedAbility?.AbilityId is > 0)
                        {
                            _abilityCooldowns[result.AbilityId] = Math.Max(0.2f, usedAbility.Cooldown);
                        }
                    }

                    _combatHint = result.Result == ResponseCode.Success
                        ? "Ability executed."
                        : $"Ability failed: {result.Message}";
                    NetworkStatus = _combatHint;
                    break;
                case CombatPackets.CombatEventPacket combat:
                    HandleCombatEvent(combat);
                    break;
                case InventoryPackets.ItemPickupPacket pickup:
                    _playerGold = pickup.TotalGold;
                    _playerExperience = pickup.TotalExperience;
                    if (pickup.Level > 0)
                    {
                        _playerLevel = pickup.Level;
                    }

                    _playerExperienceForNextLevel = pickup.ExperienceForNextLevel > 0
                        ? pickup.ExperienceForNextLevel
                        : Leveling.GetExperienceRequiredForLevel(_playerLevel + 1);
                    if (pickup.Resources.MaxHealth > 0)
                    {
                        _playerResources = pickup.Resources;
                    }

                    if (pickup.Inventory.Count > 0)
                    {
                        _inventory.Clear();
                        _inventory.AddRange(pickup.Inventory);
                        ClampSelectedInventory();
                    }

                    if (pickup.Abilities.Count > 0)
                    {
                        _abilities.Clear();
                        _abilities.AddRange(pickup.Abilities);
                    }

                    _combatHint = string.IsNullOrWhiteSpace(pickup.Message)
                        ? BuildLootMessage(pickup)
                        : pickup.Message;
                    AddLootFeed(_combatHint);
                    AddCombatText(_player, pickup.LeveledUp ? $"LEVEL {_playerLevel}" : $"+{pickup.Gold}g +{pickup.Experience}XP",
                        pickup.LeveledUp ? Color.FromArgb(156, 236, 174) : Color.FromArgb(245, 215, 122));
                    if (pickup.Items.Count > 0)
                    {
                        AddCombatText(_player, pickup.Items[0], Color.FromArgb(180, 214, 236));
                    }
                    NetworkStatus = _combatHint;
                    break;
                case InventoryPackets.InventoryUpdatePacket inventoryUpdate:
                    _playerGold = inventoryUpdate.TotalGold;
                    _playerExperience = inventoryUpdate.TotalExperience;
                    if (inventoryUpdate.Level > 0)
                    {
                        _playerLevel = inventoryUpdate.Level;
                    }

                    _playerExperienceForNextLevel = inventoryUpdate.ExperienceForNextLevel > 0
                        ? inventoryUpdate.ExperienceForNextLevel
                        : Leveling.GetExperienceRequiredForLevel(_playerLevel + 1);
                    if (inventoryUpdate.Resources.MaxHealth > 0)
                    {
                        _playerResources = inventoryUpdate.Resources;
                    }

                    _inventory.Clear();
                    _inventory.AddRange(inventoryUpdate.Inventory);
                    _equippedItems.Clear();
                    foreach (var equipped in inventoryUpdate.EquippedItems)
                    {
                        _equippedItems[equipped.Key] = equipped.Value;
                    }

                    if (inventoryUpdate.Abilities.Count > 0)
                    {
                        _abilities.Clear();
                        _abilities.AddRange(inventoryUpdate.Abilities);
                    }

                    ClampSelectedInventory();
                    _combatHint = string.IsNullOrWhiteSpace(inventoryUpdate.Message)
                        ? $"Inventory update: {inventoryUpdate.Result}"
                        : inventoryUpdate.Message;
                    AddLootFeed(_combatHint);
                    NetworkStatus = _combatHint;
                    break;
                default:
                    NetworkStatus = $"Server packet: {packet.GetType().Name}";
                    break;
            }
        }
    }

    public void Render(SoftwareRenderer renderer)
    {
        lock (_sync)
        {
            renderer.ClearVerticalGradient(Color.FromArgb(16, 26, 31), Color.FromArgb(5, 10, 14));
            DrawGround(renderer);
            DrawZoneRegions(renderer);
            DrawTerrainCorridors(renderer);
            DrawTerrainPatches(renderer);
            DrawForestEdges(renderer);
            DrawGroundDetails(renderer);
            DrawAmbientMotes(renderer);
            DrawWorldroot(renderer);

            var layers = new List<RenderLayer>();
            foreach (var decoration in _decorations)
            {
                layers.Add(new RenderLayer(GetDepth(decoration.Position), () => DrawDecoration(renderer, decoration)));
            }

            foreach (var tree in _trees)
            {
                layers.Add(new RenderLayer(GetDepth(tree.Position) + 0.05f, () => DrawTree(renderer, tree)));
            }

            foreach (var runestone in _runestones)
            {
                layers.Add(new RenderLayer(GetDepth(runestone.Position) + 0.02f, () => DrawRunestone(renderer, runestone)));
            }

            foreach (var echo in _memoryEchoes)
            {
                if (!ShouldRenderMemoryEcho(echo))
                {
                    continue;
                }

                layers.Add(new RenderLayer(GetDepth(echo.Position) + 0.025f, () => DrawMemoryEcho(renderer, echo)));
            }

            foreach (var entity in _entities.Values)
            {
                layers.Add(new RenderLayer(GetDepth(entity.Position) + 0.03f, () => DrawEntity(renderer, entity)));
            }

            layers.Add(new RenderLayer(GetDepth(_player) + 0.04f, () => DrawPlayer(renderer)));
            foreach (var layer in layers.OrderBy(layer => layer.Depth))
            {
                layer.Draw();
            }

            renderer.Present();
        }
    }

    private void ApplyMovementUpdate(MovementPackets.MovementUpdatePacket movement)
    {
        if (_localEntityId is null && _isInWorld)
        {
            _localEntityId = movement.EntityId;
        }

        if (_localEntityId == movement.EntityId)
        {
            _player = new PointF(movement.Position.X, movement.Position.Y);
            if (_cameraMode == CameraMode.Adventure)
            {
                _camera = GetDesiredCameraTarget();
            }

            return;
        }

        if (_entities.TryGetValue(movement.EntityId, out var entity))
        {
            entity.Position = new PointF(movement.Position.X, movement.Position.Y);
            entity.State = movement.State.ToString();
            _entities[movement.EntityId] = entity;
        }
    }

    private void HandleQuestDialogue(QuestPackets.QuestDialogueResponse dialogue)
    {
        var firstOffer = dialogue.Options.FirstOrDefault(option => option.Type == DialogueOptionType.OfferQuest);
        var firstTurnIn = dialogue.Options.FirstOrDefault(option => option.Type == DialogueOptionType.TurnInQuest);
        _dialogueTitle = dialogue.NpcName;
        _pendingQuestOfferId = null;
        _pendingQuestTurnInId = null;
        _pendingQuestTurnInNpcEntityId = null;
        _pendingQuestTurnInNpcTemplateId = null;

        if (firstOffer?.QuestId is int offerId)
        {
            _pendingQuestOfferId = offerId;
            if (firstOffer.Quest is not null)
            {
                _questDefinitions[firstOffer.Quest.QuestId] = firstOffer.Quest;
            }

            _questHint = $"Quest offer: {firstOffer.Quest?.Title ?? firstOffer.Text}. Press E to accept.";
            _dialogueBody = firstOffer.Quest?.Description ?? firstOffer.Text;
            _dialogueAction = $"Press E to accept: {firstOffer.Quest?.Title ?? firstOffer.Text}";
        }
        else if (firstTurnIn?.QuestId is int turnInId)
        {
            _pendingQuestTurnInId = turnInId;
            _pendingQuestTurnInNpcEntityId = dialogue.NpcEntityId;
            _pendingQuestTurnInNpcTemplateId = firstTurnIn.Quest?.TurnInNpcTemplateId;
            if (firstTurnIn.Quest is not null)
            {
                _questDefinitions[firstTurnIn.Quest.QuestId] = firstTurnIn.Quest;
            }

            _questHint = $"Quest ready: {firstTurnIn.Quest?.Title ?? firstTurnIn.Text}. Press E to complete.";
            _dialogueBody = firstTurnIn.Quest?.CompletionDialogue ?? firstTurnIn.Text;
            _dialogueAction = $"Press E to complete: {firstTurnIn.Quest?.Title ?? firstTurnIn.Text}";
        }
        else if (dialogue.UpdatedStates.Count > 0)
        {
            foreach (var state in dialogue.UpdatedStates)
            {
                if (_turnedInQuestIds.Contains(state.QuestId))
                {
                    continue;
                }

                _questStates[state.QuestId] = state;
            }

            _questHint = $"{dialogue.NpcName}: quest progress updated.";
            _dialogueBody = string.Join(" ", dialogue.UpdatedStates.Select(state =>
                BuildQuestProgressText(state, _questDefinitions.GetValueOrDefault(state.QuestId))));
            _dialogueAction = "Quest tracker updated.";
        }
        else
        {
            _questHint = $"{dialogue.NpcName}: no quest options.";
            _dialogueBody = "No quest options are available from this NPC right now.";
            _dialogueAction = null;
        }

        NetworkStatus = _questHint;
    }

    private void HandleCombatEvent(CombatPackets.CombatEventPacket combat)
    {
        if (combat.Damage is { } damage)
        {
            if (_localEntityId == damage.TargetEntityId && _playerResources.MaxHealth > 0)
            {
                _playerResources = _playerResources with
                {
                    CurrentHealth = Math.Clamp(damage.RemainingHealth, 0, _playerResources.MaxHealth)
                };
                _screenShake = Math.Max(_screenShake, damage.IsCritical ? 1.0f : 0.55f);
                if (damage.IsFatal || _playerResources.CurrentHealth <= 0)
                {
                    AddCombatText(_player, "Defeated", Color.FromArgb(255, 184, 132));
                    _combatHint = "You were defeated. Returning to the grove...";
                }
            }

            if (_entities.TryGetValue(damage.TargetEntityId, out var target))
            {
                AddCombatText(target.Position, damage.IsCritical ? $"CRIT {damage.Amount}" : $"-{damage.Amount}",
                    damage.IsCritical ? Color.FromArgb(255, 230, 132) : Color.FromArgb(238, 92, 82));
                target.CurrentHealth = Math.Max(0, damage.RemainingHealth);
                target.State = damage.IsFatal ? "Defeated" : $"Hit for {damage.Amount}";
                target.HitFlash = damage.IsCritical ? 0.5f : 0.32f;
                target.DeathFade = damage.IsFatal ? 1f : target.DeathFade;
                _entities[damage.TargetEntityId] = target;
                _combatHint = $"{target.Name} took {damage.Amount} damage.";
                if (damage.IsFatal)
                {
                    AddCombatText(target.Position, "Defeated", Color.FromArgb(226, 210, 238, 150));
                    AddLootFeed($"{target.Name} defeated.");
                }
            }
            else
            {
                AddCombatText(_player, $"-{damage.Amount}", Color.FromArgb(238, 92, 82));
                if (_localEntityId != damage.TargetEntityId)
                {
                    _combatHint = $"Damage dealt: {damage.Amount}.";
                }
            }

            if (TryGetCombatPosition(damage.TargetEntityId, out var targetPosition))
            {
                if (_localEntityId == damage.SourceEntityId)
                {
                    _playerAttackFlash = damage.IsCritical ? 0.34f : 0.26f;
                    _playerAttackTarget = targetPosition;
                }
                else if (_entities.TryGetValue(damage.SourceEntityId, out var source))
                {
                    source.AttackFlash = damage.IsCritical ? 0.34f : 0.26f;
                    source.AttackTargetPosition = targetPosition;
                    _entities[damage.SourceEntityId] = source;
                }
            }

            NetworkStatus = _combatHint ?? NetworkStatus;
            return;
        }

        if (combat.Healing is { } healing)
        {
            if (_localEntityId == healing.TargetEntityId && _playerResources.MaxHealth > 0)
            {
                _playerResources = _playerResources with
                {
                    CurrentHealth = Math.Clamp(healing.RemainingHealth, 0, _playerResources.MaxHealth)
                };
            }

            AddCombatText(_player, $"+{healing.Amount}", Color.FromArgb(112, 212, 130));
            _combatHint = $"Healing: {healing.Amount}.";
            NetworkStatus = _combatHint;
        }
    }

    private void AddCombatText(PointF position, string text, Color color)
    {
        _combatTexts.Add(new FloatingCombatText(text, position, color, 0f, 1.1f));
    }

    private void AddLootFeed(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _lootFeed.Insert(0, new LootFeedItem(message, 0f, 4.6f));
        if (_lootFeed.Count > 5)
        {
            _lootFeed.RemoveRange(5, _lootFeed.Count - 5);
        }
    }

    private void UpdateCombatText(float deltaSeconds)
    {
        for (var i = _combatTexts.Count - 1; i >= 0; i--)
        {
            var text = _combatTexts[i] with { Age = _combatTexts[i].Age + deltaSeconds };
            if (text.Age >= text.Lifetime)
            {
                _combatTexts.RemoveAt(i);
            }
            else
            {
                _combatTexts[i] = text;
            }
        }
    }

    private void UpdateLootFeed(float deltaSeconds)
    {
        for (var i = _lootFeed.Count - 1; i >= 0; i--)
        {
            var item = _lootFeed[i] with { Age = _lootFeed[i].Age + deltaSeconds };
            if (item.Age >= item.Lifetime)
            {
                _lootFeed.RemoveAt(i);
            }
            else
            {
                _lootFeed[i] = item;
            }
        }
    }

    private void UpdateAbilityCooldowns(float deltaSeconds)
    {
        foreach (var abilityId in _abilityCooldowns.Keys.ToList())
        {
            var remaining = _abilityCooldowns[abilityId] - deltaSeconds;
            if (remaining <= 0f)
            {
                _abilityCooldowns.Remove(abilityId);
            }
            else
            {
                _abilityCooldowns[abilityId] = remaining;
            }
        }
    }

    private void UpdateEntityAnimation(float deltaSeconds)
    {
        foreach (var entityId in _aggroWarningTimers.Keys.ToList())
        {
            var remaining = _aggroWarningTimers[entityId] - deltaSeconds;
            if (remaining <= 0f || !_entities.TryGetValue(entityId, out var entity) || !IsAggroOnPlayer(entity))
            {
                _aggroWarningTimers.Remove(entityId);
            }
            else
            {
                _aggroWarningTimers[entityId] = remaining;
            }
        }

        foreach (var entityId in _entities.Keys.ToList())
        {
            var entity = _entities[entityId];
            entity.SpawnAge += deltaSeconds;
            entity.HitFlash = Math.Max(0f, entity.HitFlash - deltaSeconds);
            entity.AttackFlash = Math.Max(0f, entity.AttackFlash - deltaSeconds);
            if (entity.AttackFlash <= 0f)
            {
                entity.AttackTargetPosition = null;
            }

            entity.DeathFade = Math.Max(0f, entity.DeathFade - deltaSeconds * 0.35f);
            _entities[entityId] = entity;
        }

        _playerAttackFlash = Math.Max(0f, _playerAttackFlash - deltaSeconds);
        if (_playerAttackFlash <= 0f)
        {
            _playerAttackTarget = null;
        }
    }

    private MovementInput BuildMovementInput(IReadOnlySet<Keys> keysDown)
    {
        var input = new MovementInput
        {
            Forward = 0,
            Strafe = 0,
            Sprint = keysDown.Contains(Keys.ShiftKey) || keysDown.Contains(Keys.LShiftKey) ||
                     keysDown.Contains(Keys.RShiftKey)
        };

        if (keysDown.Contains(Keys.W)) input.Strafe -= 1f;
        if (keysDown.Contains(Keys.S)) input.Strafe += 1f;
        if (keysDown.Contains(Keys.A)) input.Forward -= 1f;
        if (keysDown.Contains(Keys.D)) input.Forward += 1f;

        if (MathF.Abs(input.Forward) > 0.001f || MathF.Abs(input.Strafe) > 0.001f)
        {
            input.LookYaw = MathF.Atan2(input.Strafe, input.Forward) * 180f / MathF.PI;
        }

        return input;
    }

    private void DrawZoneRegions(SoftwareRenderer renderer)
    {
        foreach (var region in _zoneRegions)
        {
            if (Distance(region.Position, _camera) > 28f / Math.Max(0.75f, _zoom))
            {
                continue;
            }

            var p = WorldToScreen(renderer, region.Position);
            var radiusX = Math.Max(18, (int)(region.RadiusX * GetPixelsPerWorldUnit() * ProjectionXScale));
            var radiusY = Math.Max(8, (int)(region.RadiusY * GetPixelsPerWorldUnit() * ProjectionYScale));
            var isInside = IsInsideRegion(_player, region);
            var alpha = isInside ? 24 : 7;
            renderer.FillEllipse((int)p.X, (int)p.Y, radiusX, radiusY,
                Color.FromArgb(alpha, region.Color.R, region.Color.G, region.Color.B));
            if (isInside || _selectedLandmarkName == region.Name || _hoverLandmarkName == region.Name)
            {
                renderer.DrawEllipse((int)p.X, (int)p.Y, radiusX, radiusY,
                    Color.FromArgb(isInside ? 48 : 36, region.Color.R, region.Color.G, region.Color.B));
            }

            if (region.Selectable && (isInside || _selectedLandmarkName == region.Name))
            {
                var beacon = WorldToScreen(renderer.Width, renderer.Height, region.Position, 0.12f);
                var pulse = 0.55f + MathF.Sin(_pulse * 2.1f + region.Phase) * 0.45f;
                renderer.DrawCircle((int)beacon.X, (int)beacon.Y, Math.Max(4, (int)((5 + pulse * 2) * _zoom)),
                    Color.FromArgb(58, 226, 206, 146));
            }

            if (_hoverLandmarkName == region.Name && _selectedLandmarkName != region.Name)
            {
                renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(18, radiusX / 7), Math.Max(7, radiusY / 4),
                    Color.FromArgb(170, 126, 224, 218));
            }
        }
    }

    private void DrawGround(SoftwareRenderer renderer)
    {
        var cameraX = (int)MathF.Floor(_camera.X);
        var cameraY = (int)MathF.Floor(_camera.Y);
        var range = Math.Max(20, (int)(32 / Math.Max(0.6f, _zoom)));
        for (var line = -range; line <= range; line += 8)
        {
            var xLine = cameraX + line;
            var yLine = cameraY + line;
            var xA = WorldToScreen(renderer, new PointF(xLine, cameraY - range));
            var xB = WorldToScreen(renderer, new PointF(xLine, cameraY + range));
            var yA = WorldToScreen(renderer, new PointF(cameraX - range, yLine));
            var yB = WorldToScreen(renderer, new PointF(cameraX + range, yLine));
            var major = line % 16 == 0;
            var color = major ? Color.FromArgb(28, 13, 40, 36) : Color.FromArgb(16, 10, 30, 29);
            renderer.DrawLine((int)xA.X, (int)xA.Y, (int)xB.X, (int)xB.Y, color);
            renderer.DrawLine((int)yA.X, (int)yA.Y, (int)yB.X, (int)yB.Y, color);
        }

        var origin = WorldToScreen(renderer, new PointF(0, 0));
        renderer.FillEllipse((int)origin.X, (int)origin.Y, (int)(220 * _zoom), (int)(94 * _zoom),
            Color.FromArgb(34, 24, 70, 48));
        renderer.DrawEllipse((int)origin.X, (int)origin.Y, (int)(220 * _zoom), (int)(94 * _zoom),
            Color.FromArgb(34, 96, 70));

        for (var i = 0; i < 14; i++)
        {
            var angle = i / 14f * MathF.Tau + 0.18f;
            var inner = WorldToScreen(renderer,
                new PointF(MathF.Cos(angle) * 3.2f, MathF.Sin(angle) * 2.1f));
            var outer = WorldToScreen(renderer,
                new PointF(MathF.Cos(angle) * 9.4f, MathF.Sin(angle) * 6.1f));
            renderer.DrawLine((int)inner.X, (int)inner.Y, (int)outer.X, (int)outer.Y,
                Color.FromArgb(31, 79, 57));
        }
    }

    private void DrawTerrainPatches(SoftwareRenderer renderer)
    {
        foreach (var patch in _terrainPatches)
        {
            var p = WorldToScreen(renderer, patch.Position);
            var w = Math.Max(4, (int)(patch.Size.Width * GetPixelsPerWorldUnit() * 0.92f));
            var h = Math.Max(3, (int)(patch.Size.Height * GetPixelsPerWorldUnit() * 0.42f));
            var color = GetTerrainColor(patch.Kind, 218);

            renderer.FillEllipse((int)p.X, (int)p.Y, w / 2, h / 2, color);
            DrawTerrainTileStamps(renderer, patch.Kind, patch.Position, patch.Size, patch.Seed,
                patch.Kind == TerrainKind.Path || patch.Kind == TerrainKind.Root ? 3 : 2);
            DrawTerrainTexture(renderer, patch, p, w, h);

            if (patch.Kind == TerrainKind.Water)
            {
                renderer.DrawEllipse((int)p.X, (int)p.Y, w / 2, h / 2, Color.FromArgb(38, 101, 112));
                var shimmer = MathF.Sin(_pulse * 2.4f + patch.Position.X) * 0.5f + 0.5f;
                renderer.DrawLine((int)(p.X - w * 0.28f), (int)(p.Y - h * 0.05f),
                    (int)(p.X + w * 0.24f), (int)(p.Y - h * 0.05f),
                    Color.FromArgb((int)(70 + shimmer * 45), 101, 177, 183));
                renderer.DrawLine((int)(p.X - w * 0.18f), (int)(p.Y + h * 0.12f),
                    (int)(p.X + w * 0.18f), (int)(p.Y + h * 0.12f),
                    Color.FromArgb((int)(44 + shimmer * 35), 78, 137, 150));
            }

            if (patch.Kind == TerrainKind.Path || patch.Kind == TerrainKind.Root || patch.Kind == TerrainKind.VoidScar)
            {
                renderer.DrawEllipse((int)p.X, (int)p.Y, w / 2, h / 2, Color.FromArgb(24, 56, 45));
            }
        }
    }

    private void DrawTerrainCorridors(SoftwareRenderer renderer)
    {
        foreach (var corridor in _terrainCorridors)
        {
            if (corridor.Points.Count < 2 || DistanceToPath(_camera, corridor.Points) > 30f / Math.Max(0.7f, _zoom))
            {
                continue;
            }

            for (var i = 0; i + 1 < corridor.Points.Count; i++)
            {
                var startWorld = corridor.Points[i];
                var endWorld = corridor.Points[i + 1];
                var start = WorldToScreen(renderer, startWorld);
                var end = WorldToScreen(renderer, endWorld);
                var dx = end.X - start.X;
                var dy = end.Y - start.Y;
                var length = MathF.Sqrt(dx * dx + dy * dy);
                if (length < 1f)
                {
                    continue;
                }

                var width = Math.Max(8f, corridor.Width * GetPixelsPerWorldUnit());
                var normalX = -dy / length * width * 0.5f;
                var normalY = dx / length * width * 0.5f;
                var points = new[]
                {
                    new PointF(start.X + normalX, start.Y + normalY),
                    new PointF(end.X + normalX, end.Y + normalY),
                    new PointF(end.X - normalX, end.Y - normalY),
                    new PointF(start.X - normalX, start.Y - normalY)
                };

                renderer.FillPolygon(points, GetTerrainColor(corridor.Kind, 176));
                renderer.DrawLine((int)(start.X + normalX * 0.45f), (int)(start.Y + normalY * 0.45f),
                    (int)(end.X + normalX * 0.45f), (int)(end.Y + normalY * 0.45f),
                    GetTerrainHighlightColor(corridor.Kind, 72));
                renderer.DrawLine((int)(start.X - normalX * 0.42f), (int)(start.Y - normalY * 0.42f),
                    (int)(end.X - normalX * 0.42f), (int)(end.Y - normalY * 0.42f),
                    GetTerrainShadowColor(corridor.Kind, 82));
                DrawTerrainSegmentTiles(renderer, corridor.Kind, startWorld, endWorld, corridor.Width, corridor.Seed + i * 37);
            }

            for (var i = 0; i < corridor.Points.Count; i++)
            {
                var node = WorldToScreen(renderer, corridor.Points[i]);
                var radiusX = Math.Max(8, (int)(corridor.Width * GetPixelsPerWorldUnit() * 0.56f));
                var radiusY = Math.Max(3, (int)(radiusX * 0.36f));
                renderer.FillEllipse((int)node.X, (int)node.Y, radiusX, radiusY, GetTerrainColor(corridor.Kind, 166));
            }
        }
    }

    private void DrawForestEdges(SoftwareRenderer renderer)
    {
        foreach (var edge in _forestEdges)
        {
            if (Distance(edge.Position, _camera) > 34f / Math.Max(0.7f, _zoom))
            {
                continue;
            }

            var p = WorldToScreen(renderer, edge.Position);
            var scale = edge.Scale * _zoom;
            var width = Math.Max(28, (int)(edge.Width * GetPixelsPerWorldUnit() * 0.78f));
            var depth = Math.Max(10, (int)(edge.Depth * GetPixelsPerWorldUnit() * 0.28f));
            var sway = MathF.Sin(_pulse * 0.55f + edge.Phase) * 2f * scale;

            renderer.FillEllipse((int)p.X, (int)(p.Y + 7 * scale), width / 2, depth / 2,
                Color.FromArgb(108, 5, 18, 16));
            renderer.FillEllipse((int)(p.X - width * 0.18f), (int)(p.Y - 3 * scale + sway),
                Math.Max(18, (int)(width * 0.34f)), Math.Max(9, (int)(depth * 0.72f)),
                Color.FromArgb(172, 16, 55, 34));
            renderer.FillEllipse((int)(p.X + width * 0.14f), (int)(p.Y - 10 * scale - sway * 0.4f),
                Math.Max(20, (int)(width * 0.38f)), Math.Max(10, (int)(depth * 0.82f)),
                Color.FromArgb(180, 21, 73, 42));
            renderer.FillEllipse((int)(p.X + width * 0.36f), (int)(p.Y + 2 * scale + sway * 0.35f),
                Math.Max(16, (int)(width * 0.27f)), Math.Max(8, (int)(depth * 0.62f)),
                Color.FromArgb(162, 27, 88, 48));

            var trunkCount = Math.Max(2, (int)MathF.Round(edge.Width * 0.65f));
            for (var i = 0; i < trunkCount; i++)
            {
                var t = trunkCount <= 1 ? 0.5f : i / (float)(trunkCount - 1);
                var jitter = HashRange(edge.Seed * 37 + i * 11, -0.18f, 0.18f);
                var world = new PointF(edge.Position.X + (t - 0.5f) * edge.Width + jitter,
                    edge.Position.Y + HashRange(edge.Seed * 43 + i * 13, -0.34f, 0.28f) * edge.Depth);
                DrawVerticalColumn(renderer, world, 0.72f * edge.Scale, 0.045f * edge.Scale, 0.03f * edge.Scale,
                    Color.FromArgb(92, 62, 39), Color.FromArgb(52, 39, 31));
            }
        }
    }

    private void DrawTerrainTileStamps(SoftwareRenderer renderer, TerrainKind kind, PointF center, SizeF size, int seed,
        int count)
    {
        if (!_sprites.TryGet(GetTerrainSpriteName(kind), out var sprite))
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var x = center.X + HashRange(seed * 29 + i * 17, -size.Width * 0.38f, size.Width * 0.38f);
            var y = center.Y + HashRange(seed * 31 + i * 19, -size.Height * 0.28f, size.Height * 0.28f);
            var p = WorldToScreen(renderer, new PointF(x, y));
            var scale = Math.Clamp(_zoom * HashRange(seed * 41 + i * 23, 0.68f, 0.92f), 0.75f, 1.25f);
            renderer.DrawSprite(sprite, (int)p.X, (int)p.Y, scale, SpriteAnchor.Center);
        }
    }

    private void DrawTerrainSegmentTiles(SoftwareRenderer renderer, TerrainKind kind, PointF start, PointF end,
        float width, int seed)
    {
        if (!_sprites.TryGet(GetTerrainSpriteName(kind), out var sprite))
        {
            return;
        }

        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        if (length <= 0.1f)
        {
            return;
        }

        var steps = Math.Clamp((int)(length * 0.42f), 1, 5);
        for (var i = 0; i < steps; i++)
        {
            var t = (i + 0.5f) / steps;
            var lateral = HashRange(seed * 43 + i * 11, -width * 0.28f, width * 0.28f);
            var normalX = -dy / length;
            var normalY = dx / length;
            var world = new PointF(
                start.X + dx * t + normalX * lateral,
                start.Y + dy * t + normalY * lateral);
            var p = WorldToScreen(renderer, world);
            renderer.DrawSprite(sprite, (int)p.X, (int)p.Y, Math.Clamp(_zoom * 0.76f, 0.75f, 1.18f), SpriteAnchor.Center);
        }
    }

    private static string GetTerrainSpriteName(TerrainKind kind) => kind switch
    {
        TerrainKind.Moss => "tile_moss",
        TerrainKind.Path => "tile_path",
        TerrainKind.Water => "tile_water",
        TerrainKind.Root => "tile_root",
        TerrainKind.VoidScar => "tile_void",
        _ => "tile_moss"
    };

    private static Color GetTerrainColor(TerrainKind kind, int alpha) => kind switch
    {
        TerrainKind.Moss => Color.FromArgb(alpha, 22, 62, 42),
        TerrainKind.Path => Color.FromArgb(alpha, 49, 46, 36),
        TerrainKind.Water => Color.FromArgb(alpha, 18, 55, 66),
        TerrainKind.Root => Color.FromArgb(alpha, 59, 43, 29),
        TerrainKind.VoidScar => Color.FromArgb(alpha, 43, 28, 52),
        _ => Color.FromArgb(alpha, 30, 64, 48)
    };

    private static Color GetTerrainHighlightColor(TerrainKind kind, int alpha) => kind switch
    {
        TerrainKind.Moss => Color.FromArgb(alpha, 64, 118, 72),
        TerrainKind.Path => Color.FromArgb(alpha, 91, 76, 52),
        TerrainKind.Water => Color.FromArgb(alpha, 83, 143, 154),
        TerrainKind.Root => Color.FromArgb(alpha, 104, 72, 42),
        TerrainKind.VoidScar => Color.FromArgb(alpha, 111, 62, 138),
        _ => Color.FromArgb(alpha, 69, 112, 82)
    };

    private static Color GetTerrainShadowColor(TerrainKind kind, int alpha) => kind switch
    {
        TerrainKind.Moss => Color.FromArgb(alpha, 10, 39, 27),
        TerrainKind.Path => Color.FromArgb(alpha, 30, 28, 23),
        TerrainKind.Water => Color.FromArgb(alpha, 8, 35, 46),
        TerrainKind.Root => Color.FromArgb(alpha, 35, 26, 20),
        TerrainKind.VoidScar => Color.FromArgb(alpha, 23, 17, 36),
        _ => Color.FromArgb(alpha, 16, 42, 32)
    };

    private static float DistanceToPath(PointF position, IReadOnlyList<PointF> points)
    {
        var distance = float.MaxValue;
        for (var i = 0; i + 1 < points.Count; i++)
        {
            distance = MathF.Min(distance, DistanceToSegment(position, points[i], points[i + 1]));
        }

        return distance;
    }

    private static float DistanceToSegment(PointF point, PointF start, PointF end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var lengthSquared = dx * dx + dy * dy;
        if (lengthSquared <= 0.0001f)
        {
            return Distance(point, start);
        }

        var t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared;
        t = Math.Clamp(t, 0f, 1f);
        var closest = new PointF(start.X + dx * t, start.Y + dy * t);
        return Distance(point, closest);
    }

    private void DrawTerrainTexture(SoftwareRenderer renderer, TerrainPatch patch, PointF center, int width, int height)
    {
        var marks = patch.Kind == TerrainKind.Water ? 4 : patch.Kind == TerrainKind.Path ? 7 : 9;
        for (var i = 0; i < marks; i++)
        {
            var jitterX = (Hash01(patch.Seed * 31 + i * 7) - 0.5f) * width * 0.72f;
            var jitterY = (Hash01(patch.Seed * 47 + i * 11) - 0.5f) * height * 0.58f;
            var x = center.X + jitterX;
            var y = center.Y + jitterY;
            var scale = 0.65f + Hash01(patch.Seed * 59 + i * 13) * 0.8f;

            switch (patch.Kind)
            {
                case TerrainKind.Moss:
                    renderer.DrawLine((int)x, (int)y, (int)(x + 5 * scale * _zoom), (int)(y - 3 * scale * _zoom),
                        Color.FromArgb(42, 100, 57));
                    renderer.DrawLine((int)x, (int)y, (int)(x - 4 * scale * _zoom), (int)(y - 2 * scale * _zoom),
                        Color.FromArgb(34, 89, 51));
                    break;
                case TerrainKind.Path:
                    renderer.FillEllipse((int)x, (int)y, Math.Max(2, (int)(5 * scale * _zoom)),
                        Math.Max(1, (int)(2 * scale * _zoom)), Color.FromArgb(72, 64, 47));
                    break;
                case TerrainKind.Root:
                    renderer.DrawLine((int)(x - 7 * scale * _zoom), (int)y, (int)(x + 7 * scale * _zoom),
                        (int)(y + 2 * scale * _zoom), Color.FromArgb(91, 61, 36));
                    break;
                case TerrainKind.VoidScar:
                    renderer.DrawLine((int)(x - 6 * scale * _zoom), (int)(y - 3 * scale * _zoom),
                        (int)(x + 6 * scale * _zoom), (int)(y + 3 * scale * _zoom), Color.FromArgb(108, 55, 136));
                    renderer.FillCircle((int)x, (int)y, Math.Max(1, (int)(2 * scale * _zoom)),
                        Color.FromArgb(142, 48, 168));
                    break;
                case TerrainKind.Water:
                    renderer.DrawLine((int)(x - 10 * scale * _zoom), (int)y, (int)(x + 10 * scale * _zoom),
                        (int)y, Color.FromArgb(49, 102, 116));
                    break;
            }
        }
    }

    private void DrawGroundDetails(SoftwareRenderer renderer)
    {
        foreach (var detail in _groundDetails)
        {
            if (Distance(detail.Position, _camera) > 24f / Math.Max(0.75f, _zoom))
            {
                continue;
            }

            var p = WorldToScreen(renderer, detail.Position);
            var scale = detail.Scale * _zoom;
            switch (detail.Kind)
            {
                case GroundDetailKind.Grass:
                    DrawGrassDetail(renderer, detail, p, scale);
                    break;
                case GroundDetailKind.Pebble:
                    renderer.FillEllipse((int)p.X, (int)p.Y, Math.Max(2, (int)(4 * scale)),
                        Math.Max(1, (int)(2 * scale)), detail.Variant % 2 == 0
                            ? Color.FromArgb(65, 74, 68)
                            : Color.FromArgb(74, 69, 58));
                    break;
                case GroundDetailKind.Leaf:
                    var leafX = MathF.Cos(detail.Rotation) * 6f * scale;
                    var leafY = MathF.Sin(detail.Rotation) * 3f * scale;
                    renderer.DrawLine((int)(p.X - leafX), (int)(p.Y - leafY), (int)(p.X + leafX), (int)(p.Y + leafY),
                        detail.Variant % 2 == 0 ? Color.FromArgb(105, 88, 42) : Color.FromArgb(83, 105, 55));
                    break;
                case GroundDetailKind.RootVein:
                    DrawGroundRootVein(renderer, detail);
                    break;
                case GroundDetailKind.GlowBloom:
                    var glow = 0.55f + MathF.Sin(_pulse * 2.8f + detail.Rotation) * 0.45f;
                    renderer.FillCircle((int)p.X, (int)(p.Y - 2 * scale), Math.Max(1, (int)(2 * scale)),
                        Color.FromArgb((int)(90 + glow * 110), 128, 214, 122));
                    break;
            }
        }
    }

    private void DrawGrassDetail(SoftwareRenderer renderer, GroundDetail detail, PointF p, float scale)
    {
        var color = detail.Variant % 3 == 0 ? Color.FromArgb(57, 126, 68) : Color.FromArgb(43, 104, 61);
        for (var i = 0; i < 3; i++)
        {
            var angle = detail.Rotation + (i - 1) * 0.42f;
            var tip = WorldToScreen(renderer.Width, renderer.Height,
                new PointF(detail.Position.X + MathF.Cos(angle) * 0.12f * detail.Scale,
                    detail.Position.Y + MathF.Sin(angle) * 0.08f * detail.Scale),
                (0.12f + i * 0.035f) * detail.Scale);
            renderer.DrawLine((int)p.X, (int)p.Y, (int)tip.X, (int)tip.Y, color);
        }
    }

    private void DrawGroundRootVein(SoftwareRenderer renderer, GroundDetail detail)
    {
        var start = WorldToScreen(renderer, detail.Position);
        var length = 0.42f * detail.Scale;
        var endWorld = new PointF(
            detail.Position.X + MathF.Cos(detail.Rotation) * length,
            detail.Position.Y + MathF.Sin(detail.Rotation) * length);
        var end = WorldToScreen(renderer, endWorld);
        renderer.DrawLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y,
            detail.Variant % 2 == 0 ? Color.FromArgb(66, 69, 35) : Color.FromArgb(79, 55, 31));
    }

    private void DrawAmbientMotes(SoftwareRenderer renderer)
    {
        foreach (var mote in _ambientMotes)
        {
            var drift = new PointF(
                mote.Position.X + MathF.Sin(_pulse * mote.Speed + mote.Phase) * mote.Range,
                mote.Position.Y + MathF.Cos(_pulse * (mote.Speed * 0.7f) + mote.Phase) * mote.Range * 0.45f);
            var p = WorldToScreen(renderer, drift);
            var radius = Math.Max(1, (int)(mote.Size * _zoom));
            var glow = 0.55f + MathF.Sin(_pulse * mote.Speed + mote.Phase) * 0.45f;
            renderer.FillCircle((int)p.X, (int)p.Y, radius,
                Color.FromArgb((int)(120 + glow * 80), 151, 214, 150));
        }
    }

    private void DrawWorldroot(SoftwareRenderer renderer)
    {
        var center = WorldToScreen(renderer, new PointF(0, 0));
        var glow = (16 + MathF.Sin(_pulse * 2f) * 4f) * _zoom;
        renderer.FillEllipse((int)center.X, (int)center.Y, (int)(86 * _zoom + glow), (int)(38 * _zoom + glow * 0.35f),
            Color.FromArgb(26, 96, 67));
        renderer.FillEllipse((int)center.X, (int)center.Y, (int)(32 * _zoom), (int)(16 * _zoom),
            Color.FromArgb(109, 178, 113));

        if (_hoverLandmarkName == "Worldroot Heart" && _selectedLandmarkName != "Worldroot Heart")
        {
            renderer.DrawEllipse((int)center.X, (int)center.Y, (int)(100 * _zoom), (int)(46 * _zoom),
                Color.FromArgb(170, 126, 224, 218));
        }

        if (_selectedLandmarkName == "Worldroot Heart")
        {
            renderer.DrawEllipse((int)center.X, (int)center.Y, (int)(92 * _zoom), (int)(42 * _zoom),
                Color.FromArgb(224, 210, 122));
        }

        for (var i = 0; i < 10; i++)
        {
            var angle = i / 10f * MathF.Tau + MathF.Sin(_pulse * 0.5f) * 0.05f;
            var x = center.X + MathF.Cos(angle) * 120f * _zoom;
            var y = center.Y + MathF.Sin(angle) * 70f * _zoom;
            renderer.DrawLine((int)center.X, (int)center.Y, (int)x, (int)y, Color.FromArgb(54, 116, 78));
        }
    }

    private void DrawTree(SoftwareRenderer renderer, RootTree tree)
    {
        var p = WorldToScreen(renderer, tree.Position);
        var scale = tree.Scale * _zoom;
        var height = 1.65f * tree.Scale;
        var crown = WorldToScreen(renderer.Width, renderer.Height, tree.Position, height);
        var sway = MathF.Sin(_pulse * 1.1f + tree.Position.X * 0.3f) * 2f * scale;

        DrawShadow(renderer, p, 22f * scale, 7f * scale, Color.FromArgb(76, 0, 0, 0));
        renderer.FillEllipse((int)p.X, (int)(p.Y + 5 * scale), Math.Max(13, (int)(18 * scale)),
            Math.Max(4, (int)(6 * scale)), Color.FromArgb(116, 31, 42, 28));
        if (_sprites.TryGet("tree", out var treeSprite))
        {
            renderer.DrawSprite(treeSprite, (int)(p.X + sway * 0.35f), (int)(p.Y + 10 * scale), scale * 2.1f);
            renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X - 18 * scale), (int)(p.Y + 17 * scale),
                Color.FromArgb(80, 66, 40));
            renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X + 20 * scale), (int)(p.Y + 16 * scale),
                Color.FromArgb(80, 66, 40));
            renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X - 7 * scale), (int)(p.Y + 19 * scale),
                Color.FromArgb(54, 45, 34));
            renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X + 8 * scale), (int)(p.Y + 18 * scale),
                Color.FromArgb(54, 45, 34));
            return;
        }

        DrawVerticalColumn(renderer, tree.Position, height, 0.14f * tree.Scale, 0.08f * tree.Scale,
            Color.FromArgb(92, 64, 40), Color.FromArgb(63, 44, 31));
        DrawIsoBox(renderer, new PointF(tree.Position.X, tree.Position.Y + 0.14f * tree.Scale),
            0.22f * tree.Scale, 0.11f * tree.Scale, 0.2f * tree.Scale,
            Color.FromArgb(96, 63, 38), Color.FromArgb(66, 45, 31), Color.FromArgb(79, 52, 34),
            Color.FromArgb(110, 78, 48));

        renderer.FillEllipse((int)(crown.X + sway), (int)crown.Y, Math.Max(16, (int)(28 * scale)),
            Math.Max(9, (int)(17 * scale)), Color.FromArgb(31, 93, 51));
        renderer.FillEllipse((int)(crown.X - 14 * scale + sway), (int)(crown.Y + 11 * scale),
            Math.Max(12, (int)(21 * scale)), Math.Max(8, (int)(14 * scale)), Color.FromArgb(43, 118, 60));
        renderer.FillEllipse((int)(crown.X + 15 * scale + sway), (int)(crown.Y + 13 * scale),
            Math.Max(11, (int)(19 * scale)), Math.Max(7, (int)(13 * scale)), Color.FromArgb(36, 104, 56));
        renderer.FillEllipse((int)(crown.X + 3 * scale + sway), (int)(crown.Y - 7 * scale),
            Math.Max(8, (int)(14 * scale)), Math.Max(5, (int)(8 * scale)), Color.FromArgb(58, 133, 70));
        renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X - 18 * scale), (int)(p.Y + 17 * scale),
            Color.FromArgb(80, 66, 40));
        renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X + 20 * scale), (int)(p.Y + 16 * scale),
            Color.FromArgb(80, 66, 40));
        renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X - 7 * scale), (int)(p.Y + 19 * scale),
            Color.FromArgb(54, 45, 34));
        renderer.DrawLine((int)p.X, (int)(p.Y + 3 * scale), (int)(p.X + 8 * scale), (int)(p.Y + 18 * scale),
            Color.FromArgb(54, 45, 34));
    }

    private void DrawRunestone(SoftwareRenderer renderer, Runestone runestone)
    {
        var p = WorldToScreen(renderer, runestone.Position);
        var scale = _zoom;
        var flicker = 0.65f + MathF.Sin(_pulse * 3f + runestone.Phase) * 0.35f;
        var top = WorldToScreen(renderer.Width, renderer.Height, runestone.Position, 0.82f);
        DrawShadow(renderer, p, 12f * scale, 4f * scale, Color.FromArgb(70, 0, 0, 0));
        DrawIsoBox(renderer, runestone.Position, 0.18f, 0.12f, 0.82f,
            Color.FromArgb(93, 99, 100), Color.FromArgb(50, 55, 58), Color.FromArgb(70, 75, 78),
            Color.FromArgb(128, 136, 138));
        var glowRadius = Math.Max(5, (int)(8 * scale + 3 * flicker));
        renderer.FillCircle((int)top.X, (int)(top.Y + 11 * scale), Math.Max(3, (int)(4 * scale)),
            Color.FromArgb((int)(90 + 90 * flicker), 105, 190, 170));
        renderer.DrawCircle((int)top.X, (int)(top.Y + 11 * scale), glowRadius,
            Color.FromArgb((int)(115 + 90 * flicker), 105, 190, 170));

        if (_hoverLandmarkName == runestone.Name && _selectedLandmarkName != runestone.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, (int)(29 * scale), (int)(11 * scale),
                Color.FromArgb(170, 126, 224, 218));
        }

        if (_selectedLandmarkName == runestone.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, (int)(24 * scale), (int)(9 * scale), Color.FromArgb(224, 210, 122));
        }
    }

    private void DrawMemoryEcho(SoftwareRenderer renderer, MemoryEcho echo)
    {
        var p = WorldToScreen(renderer, echo.Position);
        var scale = _zoom;
        var top = WorldToScreen(renderer.Width, renderer.Height, echo.Position, 0.58f);
        var pulse = 0.5f + MathF.Sin(_pulse * 3.4f + echo.Phase) * 0.5f;
        var echoColor = echo.Kind switch
        {
            MemoryEchoKind.TrueMemory => Color.FromArgb(238, 216, 144),
            MemoryEchoKind.FracturedMemory => Color.FromArgb(181, 117, 232),
            MemoryEchoKind.Unremembering => Color.FromArgb(94, 136, 144),
            _ => Color.FromArgb(202, 220, 184)
        };

        DrawShadow(renderer, p, 14f * scale, 4f * scale, Color.FromArgb(62, 0, 0, 0));
        DrawVerticalColumn(renderer, echo.Position, 0.46f, 0.055f, 0.04f,
            Color.FromArgb(83, 64, 42), Color.FromArgb(48, 38, 31));
        renderer.FillCircle((int)top.X, (int)top.Y, Math.Max(4, (int)(6 * scale)),
            Color.FromArgb(210, echoColor.R, echoColor.G, echoColor.B));
        renderer.DrawCircle((int)top.X, (int)top.Y, Math.Max(8, (int)((11 + pulse * 6) * scale)),
            Color.FromArgb((int)(108 + pulse * 92), echoColor.R, echoColor.G, echoColor.B));

        for (var i = 0; i < 3; i++)
        {
            var angle = echo.Phase + _pulse * 0.45f + i * MathF.Tau / 3f;
            var moteWorld = new PointF(
                echo.Position.X + MathF.Cos(angle) * 0.26f,
                echo.Position.Y + MathF.Sin(angle) * 0.18f);
            var mote = WorldToScreen(renderer.Width, renderer.Height, moteWorld, 0.34f + i * 0.05f);
            renderer.FillCircle((int)mote.X, (int)mote.Y, Math.Max(1, (int)(2 * scale)),
                Color.FromArgb(170, echoColor.R, echoColor.G, echoColor.B));
        }

        if (_hoverLandmarkName == echo.Name && _selectedLandmarkName != echo.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(20, (int)(29 * scale)), Math.Max(7, (int)(11 * scale)),
                Color.FromArgb(170, 126, 224, 218));
        }

        if (_selectedLandmarkName == echo.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(18, (int)(25 * scale)), Math.Max(6, (int)(9 * scale)),
                Color.FromArgb(224, 210, 122));
        }
    }

    private void DrawDecoration(SoftwareRenderer renderer, WorldDecoration decoration)
    {
        var p = WorldToScreen(renderer, decoration.Position);
        var scale = decoration.Scale * _zoom;
        var bob = decoration.SpriteName is "lantern" or "mushroom" or "herb"
            ? MathF.Sin(_pulse * 2f + decoration.Position.X) * 1.5f * _zoom
            : 0f;
        DrawShadow(renderer, p, 9f * scale, 3f * scale, Color.FromArgb(54, 0, 0, 0));

        var drewAssetSprite = false;
        if (_sprites.TryGet(decoration.SpriteName, out var decorationSprite))
        {
            renderer.DrawSprite(decorationSprite, (int)p.X, (int)(p.Y + 8 * scale + bob), scale * 2f);
            drewAssetSprite = true;

            if (decoration.SpriteName == "lantern")
            {
                var lanternTop = WorldToScreen(renderer.Width, renderer.Height, decoration.Position, 0.68f * decoration.Scale);
                renderer.DrawCircle((int)lanternTop.X, (int)(lanternTop.Y + bob), Math.Max(8, (int)(14 * scale)),
                    Color.FromArgb(96, 147, 205, 142));
            }
            else if (decoration.SpriteName is "mushroom" or "herb")
            {
                renderer.FillCircle((int)p.X, (int)(p.Y - 7 * scale + bob), Math.Max(2, (int)(3 * scale)),
                    Color.FromArgb(112, 154, 226, 152));
            }
        }

        if (!drewAssetSprite)
        {
            switch (decoration.SpriteName)
            {
                case "supply_cache":
                    DrawIsoBox(renderer, decoration.Position, 0.34f * decoration.Scale, 0.22f * decoration.Scale,
                        0.22f * decoration.Scale, Color.FromArgb(122, 89, 53), Color.FromArgb(69, 49, 34),
                        Color.FromArgb(92, 63, 39), Color.FromArgb(150, 112, 68));
                    break;
                case "lantern":
                    DrawVerticalColumn(renderer, decoration.Position, 0.74f * decoration.Scale, 0.035f * decoration.Scale,
                        0.025f * decoration.Scale, Color.FromArgb(91, 66, 40), Color.FromArgb(58, 42, 30));
                    var lanternTop = WorldToScreen(renderer.Width, renderer.Height, decoration.Position, 0.68f * decoration.Scale);
                    renderer.FillCircle((int)lanternTop.X, (int)(lanternTop.Y + bob), Math.Max(4, (int)(7 * scale)),
                        Color.FromArgb(176, 136, 214, 132));
                    renderer.DrawCircle((int)lanternTop.X, (int)(lanternTop.Y + bob), Math.Max(7, (int)(12 * scale)),
                        Color.FromArgb(104, 147, 205, 142));
                    break;
                case "mushroom":
                    DrawVerticalColumn(renderer, decoration.Position, 0.22f * decoration.Scale, 0.035f * decoration.Scale,
                        0.025f * decoration.Scale, Color.FromArgb(201, 206, 175), Color.FromArgb(118, 124, 101));
                    var mushroomTop = WorldToScreen(renderer.Width, renderer.Height, decoration.Position, 0.2f * decoration.Scale);
                    renderer.FillEllipse((int)mushroomTop.X, (int)(mushroomTop.Y + bob), Math.Max(6, (int)(12 * scale)),
                        Math.Max(4, (int)(7 * scale)), Color.FromArgb(98, 172, 116));
                    renderer.DrawEllipse((int)mushroomTop.X, (int)(mushroomTop.Y + bob), Math.Max(6, (int)(12 * scale)),
                        Math.Max(4, (int)(7 * scale)), Color.FromArgb(154, 226, 152));
                    break;
                case "root_arch":
                    DrawRootArch(renderer, decoration);
                    break;
                case "herb":
                    DrawHerbCluster(renderer, decoration.Position, decoration.Scale);
                    break;
            }
        }

        if (_hoverLandmarkName == decoration.Name && _selectedLandmarkName != decoration.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(16, (int)(23 * scale)), Math.Max(6, (int)(9 * scale)),
                Color.FromArgb(170, 126, 224, 218));
        }

        if (_selectedLandmarkName == decoration.Name)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(13, (int)(19 * scale)), Math.Max(5, (int)(7 * scale)),
                Color.FromArgb(224, 210, 122));
        }
    }

    private void DrawEntity(SoftwareRenderer renderer, SceneEntity entity)
    {
        var p = WorldToScreen(renderer, entity.Position);
        var bodyTop = WorldToScreen(renderer.Width, renderer.Height, entity.Position, entity.IsHostile ? 0.62f : 0.54f);
        var fill = entity.IsHostile ? Color.FromArgb(145, 64, 56) : Color.FromArgb(92, 134, 109);
        var scale = GetWorldScale(entity.Position) * (entity.IsHostile ? 1.02f : 0.96f);
        var markerState = GetQuestMarkerState(entity);
        var ring = markerState == QuestMarkerState.None
            ? Color.FromArgb(62, 74, 76)
            : GetQuestMarkerColor(markerState);
        var bodyRadius = Math.Max(5, (int)((entity.IsHostile ? 11 : 9) * scale));
        var spriteName = entity.NpcTemplateId switch
        {
            6001 => "npc_hollow_sapling",
            6002 => "npc_razor_fern",
            6003 => "npc_twice_dead_hare",
            6004 => "npc_nullroot_sprout",
            6005 => "npc_ward_eaten_rootling",
            6006 => "npc_sealgnawer_matron",
            _ => entity.IsHostile ? "npc_hostile" : entity.IsQuestGiver ? "npc_quest" : "npc_neutral"
        };
        var isDefeated = entity.CurrentHealth <= 0 && entity.MaxHealth > 0;
        var bob = isDefeated ? 0f : MathF.Sin(_pulse * 3.2f + entity.EntityId) * 1.6f * scale;
        DrawShadow(renderer, p, entity.IsHostile ? 13f * scale : 10f * scale, 4f * scale,
            isDefeated ? Color.FromArgb(34, 0, 0, 0) : Color.FromArgb(76, 0, 0, 0));
        renderer.FillEllipse((int)p.X, (int)(p.Y + 2 * scale), Math.Max(7, (int)((entity.IsHostile ? 12 : 9) * scale)),
            Math.Max(2, (int)(3 * scale)), entity.IsHostile
                ? Color.FromArgb(92, 72, 36, 30)
                : Color.FromArgb(82, 36, 58, 44));
        if (markerState != QuestMarkerState.None ||
            _selectedEntityId == entity.EntityId ||
            _hoverEntityId == entity.EntityId ||
            IsAggroOnPlayer(entity))
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(8, (int)(11 * scale)), Math.Max(3, (int)(4 * scale)), ring);
        }

        if ((markerState is QuestMarkerState.Objective or QuestMarkerState.Ready) &&
            (_selectedEntityId == entity.EntityId || _hoverEntityId == entity.EntityId || Distance(_player, entity.Position) <= 7.5f))
        {
            var pulseRing = 0.5f + MathF.Sin(_pulse * 5f + entity.EntityId) * 0.5f;
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(14, (int)((18 + pulseRing * 4f) * scale)),
                Math.Max(5, (int)((7 + pulseRing * 2f) * scale)), GetQuestMarkerColor(markerState));
        }

        if (_sprites.TryGet(spriteName, out var sprite))
        {
            renderer.DrawSprite(sprite, (int)p.X, (int)(p.Y + 14 * scale + bob), scale * 2.15f);
        }
        else
        {
            DrawVerticalColumn(renderer, entity.Position, entity.IsHostile ? 0.42f : 0.34f,
                0.09f, 0.08f, fill, Color.FromArgb(54, 72, 61));
            renderer.FillCircle((int)bodyTop.X, (int)(bodyTop.Y + bob), bodyRadius, fill);
        }

        if (entity.HitFlash > 0f)
        {
            var flashRadius = Math.Max(9, (int)((18 + entity.HitFlash * 18f) * scale));
            renderer.DrawCircle((int)bodyTop.X, (int)bodyTop.Y, flashRadius, Color.FromArgb(255, 230, 132));
            renderer.DrawCircle((int)bodyTop.X, (int)bodyTop.Y, Math.Max(4, flashRadius - 4), Color.FromArgb(238, 92, 82));
        }

        if (entity.State.Equals("Combat", StringComparison.OrdinalIgnoreCase))
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(12, (int)(18 * scale)), Math.Max(5, (int)(7 * scale)),
                Color.FromArgb(207, 74, 60));
            var pulse = 0.5f + MathF.Sin(_pulse * 8f + entity.EntityId) * 0.5f;
            renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y - 2 * scale),
                Math.Max(7, (int)((10 + pulse * 3f) * scale)), Color.FromArgb(118, 218, 84, 66));
        }

        if (IsAggroOnPlayer(entity))
        {
            DrawAggroIndicator(renderer, entity, p, bodyTop);
        }

        if (entity.AttackFlash > 0f && entity.AttackTargetPosition is { } attackTarget)
        {
            DrawAttackTrail(renderer, entity.Position, attackTarget, entity.AttackFlash, entity.IsHostile);
        }

        if (_hoverEntityId == entity.EntityId && _selectedEntityId != entity.EntityId)
        {
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(21, (int)(32 * _zoom)), Math.Max(8, (int)(13 * _zoom)),
                Color.FromArgb(180, 126, 224, 218));
            renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y - 2 * _zoom), Math.Max(10, (int)(16 * _zoom)),
                Color.FromArgb(150, 126, 224, 218));
        }

        if (ShouldDrawEntityHealthBar(entity))
        {
            DrawHealthBar(renderer, bodyTop, entity);
        }
        DrawQuestMarker(renderer, bodyTop, entity);

        if (_selectedEntityId == entity.EntityId)
        {
            var selectedColor = markerState == QuestMarkerState.None
                ? Color.FromArgb(224, 210, 122)
                : GetQuestMarkerColor(markerState);
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(18, (int)(27 * _zoom)), Math.Max(7, (int)(10 * _zoom)),
                selectedColor);
            renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(21, (int)(31 * _zoom)), Math.Max(8, (int)(12 * _zoom)),
                Color.FromArgb(112, 171, 124));

            if (markerState is QuestMarkerState.Available or QuestMarkerState.Ready or QuestMarkerState.Objective)
            {
                renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y - 50 * _zoom), Math.Max(9, (int)(14 * _zoom)),
                    selectedColor);
            }
        }
    }

    private void DrawAggroIndicator(SoftwareRenderer renderer, SceneEntity entity, PointF foot, PointF bodyTop)
    {
        var playerBody = WorldToScreen(renderer.Width, renderer.Height, _player, 0.58f);
        var pulse = 0.5f + MathF.Sin(_pulse * 9f + entity.EntityId) * 0.5f;
        var lineColor = Color.FromArgb((int)(72 + pulse * 56), 238, 68, 62);
        renderer.DrawLine((int)bodyTop.X, (int)bodyTop.Y, (int)playerBody.X, (int)playerBody.Y, lineColor);

        renderer.DrawEllipse((int)foot.X, (int)foot.Y, Math.Max(16, (int)((23 + pulse * 5f) * _zoom)),
            Math.Max(6, (int)((9 + pulse * 3f) * _zoom)), Color.FromArgb(232, 86, 64));

        if (_aggroWarningTimers.TryGetValue(entity.EntityId, out var warning))
        {
            var progress = Math.Clamp(warning / 2.1f, 0f, 1f);
            var radius = Math.Max(12, (int)((18 + (1f - progress) * 18f) * _zoom));
            renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y - 4 * _zoom), radius, Color.FromArgb(255, 154, 80));
            renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y - 4 * _zoom), Math.Max(7, radius - 6),
                Color.FromArgb(238, 68, 62));
        }
    }

    private void DrawQuestMarker(SoftwareRenderer renderer, PointF p, SceneEntity entity)
    {
        var markerState = GetQuestMarkerState(entity);
        if (markerState == QuestMarkerState.None || !_sprites.TryGet("quest_marker", out var sprite))
        {
            return;
        }

        var distance = Distance(_player, entity.Position);
        if (markerState == QuestMarkerState.Available &&
            _selectedEntityId != entity.EntityId &&
            _hoverEntityId != entity.EntityId &&
            distance > 14.0f)
        {
            return;
        }

        var bob = MathF.Sin(_pulse * 3.5f) * 3f * _zoom;
        var markerColor = GetQuestMarkerColor(markerState);
        var markerX = (int)p.X;
        var markerY = (int)(p.Y - 36 * _zoom + bob);
        var radius = Math.Max(6, (int)(8 * _zoom));
        renderer.DrawCircle(markerX, markerY, radius, markerColor);
        renderer.DrawSprite(sprite, markerX, (int)(p.Y - 31 * _zoom + bob), _zoom * 1.45f);

        if (markerState == QuestMarkerState.Ready)
        {
            renderer.DrawCircle(markerX, markerY, radius + Math.Max(2, (int)(3 * _zoom)), Color.FromArgb(118, 226, 142));
            renderer.FillCircle(markerX + Math.Max(4, (int)(6 * _zoom)), markerY - Math.Max(3, (int)(5 * _zoom)),
                Math.Max(2, (int)(2 * _zoom)), Color.FromArgb(118, 226, 142));
        }
        else if (markerState == QuestMarkerState.Objective)
        {
            renderer.DrawLine(markerX - Math.Max(5, (int)(7 * _zoom)), markerY + radius,
                markerX + Math.Max(5, (int)(7 * _zoom)), markerY + radius, markerColor);
        }
    }

    private bool ShouldDrawEntityHealthBar(SceneEntity entity)
    {
        if (entity.MaxHealth <= 0)
        {
            return false;
        }

        if (_selectedEntityId == entity.EntityId || _hoverEntityId == entity.EntityId || IsAggroOnPlayer(entity))
        {
            return true;
        }

        if (entity.HitFlash > 0f || entity.AttackFlash > 0f)
        {
            return true;
        }

        return entity.CurrentHealth < entity.MaxHealth &&
               Distance(_player, entity.Position) <= 7.0f;
    }

    private void DrawHealthBar(SoftwareRenderer renderer, PointF p, SceneEntity entity)
    {
        if (entity.MaxHealth <= 0)
        {
            return;
        }

        var width = Math.Max(18, (int)(34 * _zoom));
        var y = (int)(p.Y - 28 * _zoom);
        var x = (int)p.X - width / 2;
        var ratio = Math.Clamp(entity.CurrentHealth / (float)entity.MaxHealth, 0f, 1f);
        renderer.FillRectangle(x, y, width, 4, Color.FromArgb(34, 22, 24));
        var filledWidth = (int)(width * ratio);
        if (filledWidth > 0)
        {
            renderer.FillRectangle(x, y, filledWidth, 4,
                entity.IsHostile ? Color.FromArgb(174, 65, 58) : Color.FromArgb(79, 151, 96));
        }
    }

    private void DrawPlayer(SoftwareRenderer renderer)
    {
        var p = WorldToScreen(renderer, _player);
        var bodyTop = WorldToScreen(renderer.Width, renderer.Height, _player, 0.68f);
        var scale = GetWorldScale(_player) * 1.12f;
        DrawShadow(renderer, p, 12f * scale, 4f * scale, Color.FromArgb(86, 0, 0, 0));
        renderer.DrawEllipse((int)p.X, (int)p.Y, Math.Max(13, (int)(18 * scale)), Math.Max(4, (int)(7 * scale)),
            Color.FromArgb(84, 138, 108));
        if (_sprites.TryGet("player", out var sprite))
        {
            var stepBob = _isPlayerMoving ? MathF.Sin(_pulse * 11f) * 2.2f * scale : MathF.Sin(_pulse * 2.2f) * 0.8f * scale;
            renderer.DrawSprite(sprite, (int)p.X, (int)(p.Y + 16 * scale + stepBob), scale * 2.18f);
            DrawPlayerFacing(renderer, bodyTop, scale);
            DrawPlayerEquipment(renderer, bodyTop, p, scale);
            if (_playerAttackFlash > 0f && _playerAttackTarget is { } attackTarget)
            {
                DrawAttackTrail(renderer, _player, attackTarget, _playerAttackFlash, false);
            }

            DrawPlayerWorldHealthBar(renderer, bodyTop);
            return;
        }

        DrawVerticalColumn(renderer, _player, 0.46f, 0.1f, 0.08f, Color.FromArgb(87, 139, 92),
            Color.FromArgb(45, 82, 66));
        renderer.FillCircle((int)bodyTop.X, (int)(bodyTop.Y + 8 * scale), Math.Max(6, (int)(10 * scale)),
            Color.FromArgb(224, 234, 203));
        renderer.DrawCircle((int)bodyTop.X, (int)(bodyTop.Y + 8 * scale), Math.Max(7, (int)(11 * scale)),
            Color.FromArgb(238, 246, 218));
        DrawPlayerFacing(renderer, bodyTop, scale);
        DrawPlayerEquipment(renderer, bodyTop, p, scale);
        if (_playerAttackFlash > 0f && _playerAttackTarget is { } fallbackAttackTarget)
        {
            DrawAttackTrail(renderer, _player, fallbackAttackTarget, _playerAttackFlash, false);
        }

        DrawPlayerWorldHealthBar(renderer, bodyTop);
    }

    private void DrawPlayerEquipment(SoftwareRenderer renderer, PointF bodyTop, PointF feet, float scale)
    {
        if (_equippedItems.TryGetValue(EquipmentSlot.MainHand, out var mainHand))
        {
            var color = GetWorldItemColor(mainHand.Rarity);
            var x1 = (int)(bodyTop.X + 9 * scale);
            var y1 = (int)(bodyTop.Y + 14 * scale);
            var x2 = (int)(bodyTop.X + 23 * scale);
            var y2 = (int)(bodyTop.Y - 7 * scale);
            renderer.DrawLine(x1 + 1, y1 + 1, x2 + 1, y2 + 1, Color.FromArgb(120, 10, 12, 12));
            renderer.DrawLine(x1, y1, x2, y2, color);
            renderer.DrawLine(x2 - Math.Max(2, (int)(3 * scale)), y2 + Math.Max(3, (int)(5 * scale)),
                x2 + Math.Max(2, (int)(3 * scale)), y2 - Math.Max(3, (int)(5 * scale)), color);
        }

        if (_equippedItems.TryGetValue(EquipmentSlot.OffHand, out var offHand))
        {
            var color = GetWorldItemColor(offHand.Rarity);
            var shieldX = (int)(bodyTop.X - 16 * scale);
            var shieldY = (int)(bodyTop.Y + 11 * scale);
            renderer.FillEllipse(shieldX, shieldY, Math.Max(4, (int)(8 * scale)), Math.Max(6, (int)(11 * scale)),
                Color.FromArgb(118, color.R, color.G, color.B));
            renderer.DrawEllipse(shieldX, shieldY, Math.Max(4, (int)(8 * scale)), Math.Max(6, (int)(11 * scale)), color);
        }

        if (_equippedItems.ContainsKey(EquipmentSlot.Trinket) || _equippedItems.ContainsKey(EquipmentSlot.Ring))
        {
            var shimmer = 0.55f + MathF.Sin(_pulse * 3.4f) * 0.45f;
            renderer.DrawEllipse((int)feet.X, (int)feet.Y, Math.Max(15, (int)((20 + shimmer * 4) * scale)),
                Math.Max(5, (int)((7 + shimmer * 2) * scale)), Color.FromArgb(96, 160, 214, 142));
        }
    }

    private void DrawAttackTrail(SoftwareRenderer renderer, PointF source, PointF target, float flash, bool hostile)
    {
        var progress = 1f - Math.Clamp(flash / 0.34f, 0f, 1f);
        var start = WorldToScreen(renderer.Width, renderer.Height, source, 0.48f);
        var end = WorldToScreen(renderer.Width, renderer.Height, target, 0.52f);
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1f)
        {
            return;
        }

        var nx = -dy / len;
        var ny = dx / len;
        var lead = Math.Clamp(progress, 0.18f, 1f);
        var trail = Math.Max(0f, lead - 0.34f);
        var x1 = start.X + dx * trail;
        var y1 = start.Y + dy * trail;
        var x2 = start.X + dx * lead;
        var y2 = start.Y + dy * lead;
        var color = hostile ? Color.FromArgb(238, 86, 71) : Color.FromArgb(230, 210, 238, 150);
        var glow = hostile ? Color.FromArgb(120, 184, 54, 48) : Color.FromArgb(110, 132, 220, 152);

        renderer.DrawLine((int)(x1 + nx * 3 * _zoom), (int)(y1 + ny * 3 * _zoom),
            (int)(x2 + nx * 9 * _zoom), (int)(y2 + ny * 9 * _zoom), glow);
        renderer.DrawLine((int)(x1 - nx * 2 * _zoom), (int)(y1 - ny * 2 * _zoom),
            (int)x2, (int)y2, color);
        renderer.DrawCircle((int)x2, (int)y2, Math.Max(4, (int)(7 * _zoom)), color);
    }

    private void DrawPlayerFacing(SoftwareRenderer renderer, PointF p, float scale)
    {
        var radians = _lastMoveYaw * MathF.PI / 180f;
        var facingWorld = new PointF(
            _player.X + MathF.Cos(radians) * 0.65f,
            _player.Y + MathF.Sin(radians) * 0.65f);
        var tip = WorldToScreen(renderer.Width, renderer.Height, facingWorld, 0.28f);
        renderer.DrawLine((int)p.X, (int)(p.Y - 8 * scale), (int)tip.X, (int)tip.Y, Color.FromArgb(156, 208, 154));
    }

    private void DrawPlayerWorldHealthBar(SoftwareRenderer renderer, PointF p)
    {
        if (_playerResources.MaxHealth <= 0)
        {
            return;
        }

        var width = Math.Max(26, (int)(42 * _zoom));
        var height = Math.Max(4, (int)(5 * _zoom));
        var x = (int)p.X - width / 2;
        var y = (int)(p.Y - 42 * _zoom);
        var ratio = Math.Clamp(_playerResources.CurrentHealth / (float)_playerResources.MaxHealth, 0f, 1f);
        renderer.FillRectangle(x, y, width, height, Color.FromArgb(35, 20, 22));
        renderer.FillRectangle(x, y, Math.Max(1, (int)(width * ratio)), height, Color.FromArgb(92, 184, 106));
        renderer.DrawRectangle(x, y, width, height, Color.FromArgb(175, 218, 226, 192));
    }

    private void DrawIsoBox(SoftwareRenderer renderer, PointF center, float halfX, float halfY, float height,
        Color topColor, Color leftColor, Color rightColor, Color outlineColor)
    {
        var a = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X - halfX, center.Y - halfY));
        var b = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X + halfX, center.Y - halfY));
        var c = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X + halfX, center.Y + halfY));
        var d = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X - halfX, center.Y + halfY));
        var at = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X - halfX, center.Y - halfY), height);
        var bt = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X + halfX, center.Y - halfY), height);
        var ct = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X + halfX, center.Y + halfY), height);
        var dt = WorldToScreen(renderer.Width, renderer.Height, new PointF(center.X - halfX, center.Y + halfY), height);

        renderer.FillPolygon(new[] { b, c, ct, bt }, rightColor);
        renderer.FillPolygon(new[] { d, a, at, dt }, leftColor);
        renderer.FillPolygon(new[] { c, d, dt, ct }, Color.FromArgb(
            Math.Clamp((rightColor.R + leftColor.R) / 2, 0, 255),
            Math.Clamp((rightColor.G + leftColor.G) / 2, 0, 255),
            Math.Clamp((rightColor.B + leftColor.B) / 2, 0, 255)));
        renderer.FillPolygon(new[] { at, bt, ct, dt }, topColor);
        DrawPolygonOutline(renderer, new[] { at, bt, ct, dt }, outlineColor);
        renderer.DrawLine((int)b.X, (int)b.Y, (int)bt.X, (int)bt.Y, outlineColor);
        renderer.DrawLine((int)c.X, (int)c.Y, (int)ct.X, (int)ct.Y, outlineColor);
        renderer.DrawLine((int)d.X, (int)d.Y, (int)dt.X, (int)dt.Y, outlineColor);
    }

    private void DrawVerticalColumn(SoftwareRenderer renderer, PointF position, float height, float baseRadius,
        float topRadius, Color frontColor, Color sideColor)
    {
        var baseLeft = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X - baseRadius, position.Y));
        var baseRight = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X + baseRadius, position.Y));
        var topLeft = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X - topRadius, position.Y), height);
        var topRight = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X + topRadius, position.Y), height);
        var baseBack = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X, position.Y - baseRadius));
        var topBack = WorldToScreen(renderer.Width, renderer.Height, new PointF(position.X, position.Y - topRadius), height);

        renderer.FillPolygon(new[] { baseBack, baseRight, topRight, topBack }, sideColor);
        renderer.FillPolygon(new[] { baseLeft, baseRight, topRight, topLeft }, frontColor);
        renderer.DrawLine((int)baseLeft.X, (int)baseLeft.Y, (int)topLeft.X, (int)topLeft.Y, Color.FromArgb(88, 54, 33));
        renderer.DrawLine((int)baseRight.X, (int)baseRight.Y, (int)topRight.X, (int)topRight.Y, Color.FromArgb(66, 42, 29));
    }

    private void DrawRootArch(SoftwareRenderer renderer, WorldDecoration decoration)
    {
        var spread = 0.38f * decoration.Scale;
        var left = new PointF(decoration.Position.X - spread, decoration.Position.Y);
        var right = new PointF(decoration.Position.X + spread, decoration.Position.Y);
        var height = 1.05f * decoration.Scale;
        DrawVerticalColumn(renderer, left, height, 0.07f * decoration.Scale, 0.06f * decoration.Scale,
            Color.FromArgb(91, 63, 39), Color.FromArgb(59, 42, 30));
        DrawVerticalColumn(renderer, right, height, 0.07f * decoration.Scale, 0.06f * decoration.Scale,
            Color.FromArgb(91, 63, 39), Color.FromArgb(59, 42, 30));

        var archCenter = WorldToScreen(renderer.Width, renderer.Height, decoration.Position, height);
        renderer.FillEllipse((int)archCenter.X, (int)(archCenter.Y + 8 * _zoom), Math.Max(14, (int)(26 * decoration.Scale * _zoom)),
            Math.Max(5, (int)(9 * decoration.Scale * _zoom)), Color.FromArgb(82, 58, 36));
        renderer.DrawEllipse((int)archCenter.X, (int)(archCenter.Y + 8 * _zoom), Math.Max(14, (int)(26 * decoration.Scale * _zoom)),
            Math.Max(5, (int)(9 * decoration.Scale * _zoom)), Color.FromArgb(119, 84, 52));
    }

    private void DrawHerbCluster(SoftwareRenderer renderer, PointF position, float scale)
    {
        var basePoint = WorldToScreen(renderer.Width, renderer.Height, position);
        var bladeColor = Color.FromArgb(92, 164, 93);
        for (var i = 0; i < 5; i++)
        {
            var phase = (i - 2) * 0.08f;
            var tipWorld = new PointF(position.X + phase, position.Y + MathF.Abs(phase) * 0.4f);
            var tip = WorldToScreen(renderer.Width, renderer.Height, tipWorld, (0.18f + i % 2 * 0.08f) * scale);
            renderer.DrawLine((int)basePoint.X, (int)basePoint.Y, (int)tip.X, (int)tip.Y, bladeColor);
        }

        renderer.FillCircle((int)basePoint.X, (int)(basePoint.Y - 2 * _zoom), Math.Max(2, (int)(3 * scale * _zoom)),
            Color.FromArgb(120, 204, 116));
    }

    private static void DrawPolygonOutline(SoftwareRenderer renderer, IReadOnlyList<PointF> points, Color color)
    {
        for (var i = 0; i < points.Count; i++)
        {
            var a = points[i];
            var b = points[(i + 1) % points.Count];
            renderer.DrawLine((int)a.X, (int)a.Y, (int)b.X, (int)b.Y, color);
        }
    }

    private void DrawShadow(SoftwareRenderer renderer, PointF p, float radiusX, float radiusY, Color color)
    {
        renderer.FillEllipse((int)p.X, (int)(p.Y + 3 * _zoom), Math.Max(2, (int)radiusX), Math.Max(1, (int)radiusY), color);
    }

    private Color GetQuestMarkerColor(QuestMarkerState state)
    {
        return state switch
        {
            QuestMarkerState.Ready => Color.FromArgb(118, 226, 142),
            QuestMarkerState.Objective => Color.FromArgb(118, 185, 238),
            QuestMarkerState.Interactable => Color.FromArgb(188, 205, 186),
            _ => Color.FromArgb(224, 210, 122)
        };
    }

    private QuestMarkerState GetQuestMarkerState(SceneEntity entity)
    {
        if (entity.NpcTemplateId is not int npcTemplateId)
        {
            return QuestMarkerState.None;
        }

        if (_questStates.Values.Any(state =>
                state.State == QuestState.Completed &&
                _questDefinitions.TryGetValue(state.QuestId, out var definition) &&
                definition.TurnInNpcTemplateId == npcTemplateId))
        {
            return QuestMarkerState.Ready;
        }

        if (QuestCatalog.GetByGiver(npcTemplateId).Any(CanShowQuestOffer))
        {
            return QuestMarkerState.Available;
        }

        if (_questStates.Values.Any(state =>
                _questDefinitions.TryGetValue(state.QuestId, out var definition) &&
                IsActiveObjectiveForNpc(state, definition, npcTemplateId)))
        {
            return QuestMarkerState.Objective;
        }

        return entity.IsQuestGiver && Distance(_player, entity.Position) <= 6.5f
            ? QuestMarkerState.Interactable
            : QuestMarkerState.None;
    }

    private bool CanShowQuestOffer(QuestDefinition definition)
    {
        if (_playerLevel > 0 && _playerLevel < definition.MinimumLevel)
        {
            return false;
        }

        if (_turnedInQuestIds.Contains(definition.QuestId))
        {
            return false;
        }

        if (_questStates.TryGetValue(definition.QuestId, out var state))
        {
            if (state.State == QuestState.Active)
            {
                return false;
            }

            if (state.State == QuestState.Completed && !_turnedInQuestIds.Contains(definition.QuestId))
            {
                return false;
            }

            if (state.State == QuestState.Completed && !definition.IsRepeatable)
            {
                return false;
            }
        }

        return definition.Prerequisites.All(id => _turnedInQuestIds.Contains(id));
    }

    private bool IsActiveObjectiveForNpc(QuestStateData state, QuestDefinition definition, int npcTemplateId)
    {
        if (state.State != QuestState.Active)
        {
            return false;
        }

        return definition.Objectives.Any(objective =>
        {
            var matchesTarget = objective.TargetNpcTemplateId == npcTemplateId ||
                                definition.QuestId == QuestCatalog.VerdantOutskirtsCullId &&
                                objective.ObjectiveId == 1 &&
                                npcTemplateId is 6001 or 6002;
            if (!matchesTarget)
            {
                return false;
            }

            var progress = state.Objectives.FirstOrDefault(p => p.ObjectiveId == objective.ObjectiveId);
            return progress is not null && !progress.Completed;
        });
    }

    private bool IsActiveObjectiveForTemplate(int templateId)
    {
        return _questStates.Values.Any(state =>
            _questDefinitions.TryGetValue(state.QuestId, out var definition) &&
            IsActiveObjectiveForNpc(state, definition, templateId));
    }

    private bool ShouldRenderMemoryEcho(MemoryEcho echo)
    {
        if (_selectedLandmarkName == echo.Name || _hoverLandmarkName == echo.Name)
        {
            return true;
        }

        if (TryGetServerBackedLandmarkTemplateId(echo.Name, out var templateId) &&
            IsActiveObjectiveForTemplate(templateId))
        {
            return true;
        }

        if (IsLateStoryMemoryEcho(echo.Name))
        {
            return false;
        }

        return Distance(_player, echo.Position) <= 8.5f;
    }

    private bool ShouldShowDecorationMarker(WorldDecoration decoration)
    {
        if (!decoration.Selectable)
        {
            return false;
        }

        return _selectedLandmarkName == decoration.Name ||
               _hoverLandmarkName == decoration.Name ||
               Distance(_player, decoration.Position) <= 10.0f;
    }

    private bool ShouldShowMapEntity(SceneEntity entity)
    {
        if (_selectedEntityId == entity.EntityId || _hoverEntityId == entity.EntityId)
        {
            return true;
        }

        var markerState = GetQuestMarkerState(entity);
        if (markerState is QuestMarkerState.Ready or QuestMarkerState.Available or QuestMarkerState.Objective)
        {
            return true;
        }

        var distance = Distance(_player, entity.Position);
        if (IsAggroOnPlayer(entity))
        {
            return true;
        }

        if (entity.IsHostile)
        {
            return distance <= 12.0f;
        }

        return entity.IsQuestGiver && distance <= 14.0f;
    }

    private static bool IsLateStoryMemoryEcho(string name)
    {
        return name is "Circle Debate Root" or
            "Anchor the Memory" or
            "Prune the Memory" or
            "Aelthar Copy Lens" or
            "Ceremony Root" or
            "Road Beyond Thornveil" or
            "Bleeding Root Bloom" or
            "Aelthar Breach Lens" or
            "Nameless Patch Names";
    }

    private PointF WorldToScreen(SoftwareRenderer renderer, PointF world)
    {
        return WorldToScreen(renderer.Width, renderer.Height, world);
    }

    private PointF WorldToScreen(int width, int height, PointF world)
    {
        return WorldToScreen(width, height, world, 0f);
    }

    private PointF WorldToScreen(int width, int height, PointF world, float elevation)
    {
        var shakeX = _screenShake <= 0f ? 0f : MathF.Sin(_pulse * 57f) * _screenShake * 5f;
        var shakeY = _screenShake <= 0f ? 0f : MathF.Cos(_pulse * 41f) * _screenShake * 4f;
        var ppu = GetPixelsPerWorldUnit();

        if (_cameraMode == CameraMode.Isometric)
        {
            var dx = world.X - _camera.X;
            var dy = world.Y - _camera.Y;
            return new PointF(
                width * 0.5f + shakeX + (dx - dy) * ppu * ProjectionXScale,
                height * IsometricHorizonRatio + shakeY + (dx + dy) * ppu * IsometricProjectionYScale -
                elevation * ppu);
        }

        var cameraSpace = GetCameraSpace(world);
        var perspective = GetPerspectiveScale(cameraSpace.Forward);
        return new PointF(
            width * 0.5f + shakeX + cameraSpace.Right * ppu * ProjectionXScale * perspective,
            height * HorizonRatio + shakeY - cameraSpace.Forward * ppu * ProjectionYScale -
            elevation * ppu * perspective);
    }

    private float GetPixelsPerWorldUnit() => BasePixelsPerWorldUnit * _zoom;

    private void UpdateCameraFollow(float deltaSeconds)
    {
        var desiredCamera = GetDesiredCameraTarget();
        if (_cameraMode == CameraMode.Adventure)
        {
            _camera = desiredCamera;
            return;
        }

        var followT = 1f - MathF.Pow(0.001f, deltaSeconds);
        _camera.X = Lerp(_camera.X, desiredCamera.X, followT);
        _camera.Y = Lerp(_camera.Y, desiredCamera.Y, followT);
    }

    private PointF GetDesiredCameraTarget()
    {
        if (_cameraMode == CameraMode.Isometric)
        {
            return _player;
        }

        var radians = _cameraYaw * MathF.PI / 180f;
        return new PointF(
            _player.X + MathF.Cos(radians) * CameraForwardLead,
            _player.Y + MathF.Sin(radians) * CameraForwardLead);
    }

    private (float Right, float Forward) GetCameraSpace(PointF world)
    {
        var radians = _cameraYaw * MathF.PI / 180f;
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        var dx = world.X - _camera.X;
        var dy = world.Y - _camera.Y;
        return (-dx * sin + dy * cos, dx * cos + dy * sin);
    }

    private float GetWorldScale(PointF world)
    {
        if (_cameraMode == CameraMode.Isometric)
        {
            return _zoom;
        }

        return _zoom * GetPerspectiveScale(GetCameraSpace(world).Forward);
    }

    private static float GetPerspectiveScale(float forward)
    {
        return Math.Clamp(1f - forward * PerspectiveScalePerUnit, 0.72f, 1.28f);
    }

    private float GetDepth(PointF world)
    {
        if (_cameraMode == CameraMode.Isometric)
        {
            return world.X + world.Y;
        }

        var cameraSpace = GetCameraSpace(world);
        return -cameraSpace.Forward + MathF.Abs(cameraSpace.Right) * 0.001f;
    }

    private string GetCameraModeLabel()
    {
        return _cameraMode == CameraMode.Adventure ? "Adventure FOV" : "Classic iso";
    }

    private static NetVector3 ToNetVector(PointF point) => new(point.X, point.Y, 0);

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    private static int Mod(int value, int modulus) => (value % modulus + modulus) % modulus;

    private static bool TryGetServerBackedLandmarkTemplateId(string landmarkName, out int templateId)
    {
        templateId = landmarkName switch
        {
            "Mossglass Testimony" => 7001,
            "Thornway Cutmark" => 7002,
            "Green Scar Nullroot" => 7003,
            "Seedvault Gate" => 7004,
            "Veyr Wardseal" => 7005,
            "Veyr Memory Core" => 7006,
            "Echo Footsteps" => 7007,
            "Oranyn Reflection" => 7008,
            "Drowned Name Fragment" => 7009,
            "Old Thornway Lens" => 7011,
            "Nameless Patch" => 7012,
            "Growth Key" => 7013,
            "Memory Key" => 7014,
            "Decay Key" => 7015,
            "Root Descent" => 7016,
            "Hall of Unchosen Branches" => 7017,
            "Oranyn Chorus Anchor" => 7018,
            "Circle Debate Root" => 7019,
            "Anchor the Memory" => 7020,
            "Prune the Memory" => 7021,
            "Aelthar Copy Lens" => 7022,
            "Ceremony Root" => 7023,
            "Road Beyond Thornveil" => 7024,
            "Tomorrow Fire Site" => 7025,
            "Wolf Memory Den" => 7026,
            "Memory Trader Cache" => 7027,
            "Weakening Prayer Stone" => 7028,
            "Living Bark Forge" => 7029,
            "Bleeding Root Bloom" => 7030,
            "Aelthar Breach Lens" => 7031,
            "Nameless Patch Names" => 7032,
            _ => 0
        };

        return templateId != 0;
    }

    private string BuildZoneLabel()
    {
        if (!_isInWorld)
        {
            return _activeZone;
        }

        return $"Thornveil: {GetCurrentSubzone()}";
    }

    private string GetCurrentSubzone()
    {
        var best = _zoneRegions
            .Where(region => IsInsideRegion(_player, region))
            .OrderBy(region => GetRegionDistanceFactor(_player, region))
            .FirstOrDefault();

        return string.IsNullOrEmpty(best.Name) ? "Thornveil Wilds" : best.Name;
    }

    private static bool IsInsideRegion(PointF point, ZoneRegion region)
    {
        var dx = (point.X - region.Position.X) / Math.Max(0.1f, region.RadiusX);
        var dy = (point.Y - region.Position.Y) / Math.Max(0.1f, region.RadiusY);
        return dx * dx + dy * dy <= 1f;
    }

    private static float GetRegionDistanceFactor(PointF point, ZoneRegion region)
    {
        var dx = (point.X - region.Position.X) / Math.Max(0.1f, region.RadiusX);
        var dy = (point.Y - region.Position.Y) / Math.Max(0.1f, region.RadiusY);
        return dx * dx + dy * dy;
    }

    private string? GetSelectedTargetName()
    {
        if (_selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity))
        {
            return entity.Name;
        }

        return _selectedLandmarkName;
    }

    private int GetAggroCount()
    {
        return _entities.Values.Count(IsAggroOnPlayer);
    }

    private bool IsPlayerDefeated()
    {
        return _playerResources.MaxHealth > 0 && _playerResources.CurrentHealth <= 0;
    }

    private bool IsAggroOnPlayer(SceneEntity entity)
    {
        return _localEntityId is ulong localEntityId &&
               entity.TargetEntityId == localEntityId &&
               entity.CurrentHealth > 0;
    }

    private string? GetSelectedTargetKind()
    {
        if (_selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity))
        {
            var markerState = GetQuestMarkerState(entity);
            if (markerState == QuestMarkerState.Ready) return "Quest Ready";
            if (markerState == QuestMarkerState.Available) return "Quest Available";
            if (markerState == QuestMarkerState.Objective) return "Quest Objective";
            if (entity.IsHostile) return "Hostile";
            if (markerState == QuestMarkerState.Interactable) return "Interactable";
            if (entity.IsQuestGiver) return "Quest";
            return "Entity";
        }

        return _selectedLandmarkName is null ? null : "Landmark";
    }

    private float? GetSelectedTargetDistance()
    {
        PointF? target = null;
        if (_selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity))
        {
            target = entity.Position;
        }
        else if (_selectedLandmarkName is not null && TryGetLandmarkPosition(_selectedLandmarkName, out var landmark))
        {
            target = landmark;
        }

        return target is null ? null : Distance(_player, target.Value);
    }

    private bool TryGetLandmarkPosition(string landmarkName, out PointF position)
    {
        if (landmarkName == "Worldroot Heart")
        {
            position = new PointF(0, 0);
            return true;
        }

        var runestone = _runestones.FirstOrDefault(r => r.Name == landmarkName);
        if (!string.IsNullOrEmpty(runestone.Name))
        {
            position = runestone.Position;
            return true;
        }

        var echo = _memoryEchoes.FirstOrDefault(e => e.Name == landmarkName);
        if (!string.IsNullOrEmpty(echo.Name))
        {
            position = echo.Position;
            return true;
        }

        var region = _zoneRegions.FirstOrDefault(r => r.Name == landmarkName);
        if (!string.IsNullOrEmpty(region.Name))
        {
            position = region.Position;
            return true;
        }

        var decoration = _decorations.FirstOrDefault(d => d.Name == landmarkName);
        if (!string.IsNullOrEmpty(decoration.Name))
        {
            position = decoration.Position;
            return true;
        }

        position = default;
        return false;
    }

    private AbilitySummary? GetPrimaryAbility()
    {
        return _abilities.FirstOrDefault(ability =>
            IsOffensive(ability) && RequiresHostileTarget(ability));
    }

    private AbilitySummary? GetAbilityForSlot(int slotNumber)
    {
        if (slotNumber <= 0 || slotNumber > _abilities.Count)
        {
            return null;
        }

        return _abilities[slotNumber - 1];
    }

    private float GetPrimaryAbilityCooldownRemaining()
    {
        var ability = GetPrimaryAbility();
        return ability is null ? 0f : Math.Max(0f, _abilityCooldowns.GetValueOrDefault(ability.AbilityId));
    }

    private float GetPrimaryAbilityCooldownDuration()
    {
        return GetPrimaryAbility()?.Cooldown ?? 0f;
    }

    private IReadOnlyList<AbilitySlotSnapshot> BuildAbilitySlots()
    {
        return _abilities
            .Take(5)
            .Select((ability, index) => new AbilitySlotSnapshot(
                index + 1,
                ability.AbilityId,
                ability.Name,
                GetAbilityShortType(ability),
                ability.ManaCost,
                Math.Max(0f, _abilityCooldowns.GetValueOrDefault(ability.AbilityId)),
                ability.Cooldown,
                !_abilityCooldowns.TryGetValue(ability.AbilityId, out var cooldown) || cooldown <= 0f))
            .ToList();
    }

    private static bool IsOffensive(AbilitySummary ability)
    {
        return ability.Type is AbilityType.MeleeDamage or AbilityType.PhysicalDamage or AbilityType.SpellDamage;
    }

    private static bool RequiresHostileTarget(AbilitySummary ability)
    {
        return ability.TargetType is TargetType.SingleEnemy ||
               (ability.TargetType == TargetType.SingleTarget && IsOffensive(ability));
    }

    private static string GetAbilityShortType(AbilitySummary ability)
    {
        return ability.Type switch
        {
            AbilityType.Healing => "Heal",
            AbilityType.MeleeDamage => "Melee",
            AbilityType.PhysicalDamage => "Ranged",
            AbilityType.SpellDamage => ability.MagicSource.ToString(),
            AbilityType.Buff => "Buff",
            AbilityType.Debuff => "Debuff",
            _ => ability.Type.ToString()
        };
    }

    private int? GetSelectedTargetCurrentHealth()
    {
        return _selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity) &&
               entity.MaxHealth > 0
            ? entity.CurrentHealth
            : null;
    }

    private int? GetSelectedTargetMaxHealth()
    {
        return _selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity) &&
               entity.MaxHealth > 0
            ? entity.MaxHealth
            : null;
    }

    private string? GetSelectedTargetState()
    {
        return _selectedEntityId is not null && _entities.TryGetValue(_selectedEntityId.Value, out var entity)
            ? entity.State
            : null;
    }

    private bool TryGetCombatPosition(ulong entityId, out PointF position)
    {
        if (_localEntityId == entityId)
        {
            position = _player;
            return true;
        }

        if (_entities.TryGetValue(entityId, out var entity))
        {
            position = entity.Position;
            return true;
        }

        position = default;
        return false;
    }

    private IReadOnlyList<string> BuildQuestTrackerItems()
    {
        if (_questStates.Count == 0)
        {
            return Array.Empty<string>();
        }

        return _questStates.Values
            .Where(state => !_turnedInQuestIds.Contains(state.QuestId))
            .OrderBy(state => state.State == QuestState.Completed ? 1 : 0)
            .ThenBy(state => state.QuestId)
            .Take(4)
            .Select(state =>
            {
                _questDefinitions.TryGetValue(state.QuestId, out var definition);
                return BuildQuestProgressText(state, definition);
            })
            .ToList();
    }

    private IReadOnlyList<string> BuildLootFeedItems()
    {
        return _lootFeed
            .OrderBy(item => item.Age)
            .Select(item => item.Message)
            .ToList();
    }

    private void ClampSelectedInventory()
    {
        if (_inventory.Count == 0)
        {
            _selectedInventoryIndex = -1;
            return;
        }

        if (_selectedInventoryIndex < 0)
        {
            _selectedInventoryIndex = 0;
            return;
        }

        _selectedInventoryIndex = Math.Clamp(_selectedInventoryIndex, 0, _inventory.Count - 1);
    }

    private static string BuildLootMessage(InventoryPackets.ItemPickupPacket pickup)
    {
        var parts = new List<string>();
        if (pickup.Gold > 0)
        {
            parts.Add($"+{pickup.Gold} gold");
        }

        if (pickup.Experience > 0)
        {
            parts.Add($"+{pickup.Experience} XP");
        }

        parts.AddRange(pickup.Items);

        return parts.Count == 0
            ? $"Looted {pickup.SourceName}."
            : $"{string.Join(", ", parts)} from {pickup.SourceName}.";
    }

    private static string BuildRewardText(QuestDefinition definition)
    {
        var rewards = new List<string>();
        if (definition.Rewards.Experience > 0)
        {
            rewards.Add($"{definition.Rewards.Experience} XP");
        }

        if (definition.Rewards.Gold > 0)
        {
            rewards.Add($"{definition.Rewards.Gold} gold");
        }

        if (definition.Rewards.ReputationFaction is not null && definition.Rewards.ReputationAmount > 0)
        {
            rewards.Add($"{definition.Rewards.ReputationAmount} {definition.Rewards.ReputationFaction} reputation");
        }

        return rewards.Count == 0
            ? "Quest completed."
            : $"Quest completed. Reward: {string.Join(", ", rewards)}.";
    }

    private static string BuildQuestProgressText(QuestStateData state, QuestDefinition? definition)
    {
        var title = definition?.Title ?? $"Quest {state.QuestId}";
        var stateLabel = state.State == QuestState.Completed ? "READY TO TURN IN" : state.State.ToString();
        if (state.Objectives.Count == 0)
        {
            return $"{title}: {stateLabel}";
        }

        var objectiveText = string.Join(", ", state.Objectives.Select(objective =>
        {
            var description = definition?.Objectives.FirstOrDefault(def => def.ObjectiveId == objective.ObjectiveId)
                ?.Description;
            var label = string.IsNullOrWhiteSpace(description) ? $"Objective {objective.ObjectiveId}" : description;
            return $"{label} {objective.Current}/{objective.Target}";
        }));

        return $"{title}: {stateLabel} - {objectiveText}";
    }

    private static string BuildQuestProgressPulseText(QuestStateData state, QuestDefinition? definition)
    {
        var title = definition?.Title ?? $"Quest {state.QuestId}";
        if (state.State == QuestState.Completed)
        {
            return $"{title} complete";
        }

        var firstOpen = state.Objectives.FirstOrDefault(objective => !objective.Completed);
        if (firstOpen is null)
        {
            return $"{title} updated";
        }

        return $"{title} {firstOpen.Current}/{firstOpen.Target}";
    }

    private static Color GetWorldItemColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Uncommon => Color.FromArgb(126, 224, 132),
            ItemRarity.Rare => Color.FromArgb(122, 170, 238),
            ItemRarity.Epic => Color.FromArgb(196, 128, 236),
            _ => Color.FromArgb(210, 202, 174)
        };
    }

    private static string GetLandmarkDescription(string landmarkName)
    {
        return landmarkName switch
        {
            "Worldroot Heart" =>
                "Heartbough Glade is the protected root-heart of Thornveil Enclave. The Worldroot here is not only alive; it remembers.",
            "Heartbough Glade" =>
                "The Sylvaen starting hollow. Elder Thaelir's circle gathers here while the Worldroot bleeds faint green-black sap below the moss.",
            "The Listener's Ring" =>
                "A training ring of low memory-roots where initiates learn Root Sense: listening before cutting, remembering before judging.",
            "Whisperroot Paths" =>
                "Narrow paths where roots whisper underfoot. Good ground for tracking repeated movement and teaching first combat rhythm.",
            "Mossglass Pools" =>
                "Still pools that reflect what the Worldroot remembers, not always what stands above them.",
            "Old Thornway" =>
                "An old border path toward Aelthar pressure. The bark here carries clean cuts that do not feel Sylvaen.",
            "The Green Scar" =>
                "A wounded place where meaning thins. The scar is small, but it stains root-memory violet-black at the edges.",
            "Seedvault Veyr" =>
                "A sealed lower root-chamber. Its threshold is becoming Thornveil's first mini-dungeon hook and Act I capstone space.",
            "Seedvault Antechamber" =>
                "The first breath below the Seedvault Gate: cramped roots, broken wards, and hungry growth that learned the taste of sealwork.",
            "Thornwatch Threshold" =>
                "A narrow watchline at the edge of the grove. The path points toward wider Thornveil routes, but the roots hold you here for now.",
            "Nullroot Verge" =>
                "The outer lip of the Green Scar. The moss thins, the air tastes metallic, and the Worldroot grows careful.",
            "True Memory Root" =>
                "A stable echo-root. It repeats a true memory: footsteps, sap, breath, and fear all in the right order.",
            "Fractured Memory Root" =>
                "This echo stutters. Something has copied the shape of a memory without understanding why it happened.",
            "Unremembering Root" =>
                "A cold patch in the archive. The Worldroot knows something was here, but the name has been scraped away.",
            "Rootbound Sigil" =>
                "The sigil is half-carved, half-grown. Its pattern locks Heartbough Glade against the first hints of the Bleeding Root.",
            "Threshold Warden Stone" =>
                "A border stone etched with old patrol marks. It points toward the next route out of Thornveil, once the grove is stable.",
            "Scar Listening Stone" =>
                "A small stone grown into the scar-line. It hums only when something nearby repeats a memory incorrectly.",
            "Seedvault Lockstone" =>
                "A lockstone grown into the antechamber threshold. Its marks are Orenhusk's work, but something has chewed at the inner edge.",
            "Mossglass Testimony" =>
                "The pool throws up a pale-gold recollection of Oranyn's death. It may be true, forged, or both in sequence.",
            "Thornway Cutmark" =>
                "A clean Aelthar-style cut in old bark. It looks recent, but the memory around it has been folded back on itself.",
            "Green Scar Nullroot" =>
                "A root that refuses to answer Root Sense. It is not dead. It is waiting without a name.",
            "Seedvault Gate" =>
                "A living gate grown from old rootwood. The seal listens, but something beneath it is listening back.",
            "Veyr Wardseal" =>
                "A wardseal sunk into the root floor. Its glyphs answer in sequence: grove, memory, warning.",
            "Veyr Memory Core" =>
                "A pale core of old root-memory. It repeats the same warning in three voices, then goes silent.",
            "Echo Footsteps" =>
                "A repeated trail crossing the Whisperroot Paths. It walks the same panic into the moss until small predators start believing it.",
            "Oranyn Reflection" =>
                "The pool shows Oranyn's death as a pale reflection. The image is too smooth, like a testimony rehearsed too many times.",
            "Drowned Name Fragment" =>
                "A name sunk under the Mossglass Pools. It rises only when the water is still and the grove is brave enough to listen.",
            "Old Thornway Lens" =>
                "A silver-edged lens grown into damaged bark. It catches Aelthar pressure as clean lines of impossible light.",
            "Nameless Patch" =>
                "A quiet blank at the Green Scar. The Worldroot knows something is here, but refuses to say what.",
            "Growth Key" =>
                "A living key bound to new shoots. It opens only when the grove accepts that growth can be dangerous.",
            "Memory Key" =>
                "A key of resin and ambered names. It hums with everything Thornveil refuses to forget.",
            "Decay Key" =>
                "A dark key wrapped in soft rot. It does not feel evil; it feels honest about endings.",
            "Root Descent" =>
                "The first step into Seedvault Veyr's lower chamber. The safe grove ends at the lip of this root-stair.",
            "Hall of Unchosen Branches" =>
                "A Seedvault chamber lined with paths the grove never took. Some of those paths still want to be chosen.",
            "Oranyn Chorus Anchor" =>
                "A chorus of preserved Oranyn memory. It asks to live with too much clarity to be dismissed as noise.",
            "Circle Debate Root" =>
                "A root grown for disagreement. Thaelir, Ylvhara, Savaen, and Lysarien all leave different pressure-marks here.",
            "Anchor the Memory" =>
                "One possible Thornveil ending: preserve the Oranyn memory inside the Worldroot and accept the risk of what it becomes.",
            "Prune the Memory" =>
                "One possible Thornveil ending: cut the Oranyn memory away before it can rewrite the grove.",
            "Aelthar Copy Lens" =>
                "One possible Thornveil ending: allow a controlled Aelthar copy and gamble that knowledge shared is safer than knowledge buried.",
            "Ceremony Root" =>
                "The root that records Thornveil's choice. Later this should become a true branching state.",
            "Road Beyond Thornveil" =>
                "A marker for the next zone. The path waits for architecture, roads, and the first real step toward 3D expansion.",
            "Tomorrow Fire Site" =>
                "Cold ash from a campfire that has not happened yet. The memory smells of smoke and fresh rain.",
            "Wolf Memory Den" =>
                "A shallow den where one wolf-memory answers to three names and no single death.",
            "Memory Trader Cache" =>
                "A leaf-wrapped cache hidden beneath ferns. For now it is a quest object; later it can anchor a real vendor.",
            "Weakening Prayer Stone" =>
                "A shrine-stone to a small, fading god. It answers softly, but still answers.",
            "Living Bark Forge" =>
                "A barksmith's living forge. Aelthar iron has left silver hooks in the grain.",
            "Bleeding Root Bloom" =>
                "A public-event bloom leaking corrupted sap. Seal it before rot-feeders gather.",
            "Aelthar Breach Lens" =>
                "A thin silver breach at Old Thornway. It looks more like a question than a doorway.",
            "Nameless Patch Names" =>
                "Loose names pulled from the blank patch. Some are whole; some are only the shape of being remembered.",
            "Mooncap Cluster" =>
                "Soft mooncaps drink in the Worldroot glow. Later these could become gathering nodes or alchemy ingredients.",
            "Scout Cache" =>
                "A small Verdant Circles field cache with training supplies, wrapped in waxed leafcloth against the damp.",
            "Seedvault Cache" =>
                "A sealed seedvault box. Its wax sigil is intact, but the memory around it has started to flicker.",
            "Seedvault Gate Marker" =>
                "Fresh Orenhusk marks show where the lower root-chamber has started to strain against its seal.",
            "Antechamber Wardlight" =>
                "A low wardlight marking the point where the safe grove ends and the Seedvault threshold begins.",
            "Thornwatch Marker" =>
                "A low marker for grove patrols. Fresh scratches show where someone tested the boundary recently.",
            "Nullroot Warning" =>
                "A warning stone grown at the scar edge. Its glyph means listen twice before you touch.",
            "Mossglass Marker" =>
                "A small lantern marking the safest edge of the pools.",
            "Scarbound Lantern" =>
                "A lantern wrapped in green-black thread. Its flame leans away from the scar.",
            "Root Arch" =>
                "The root arch bends without breaking. It feels like a natural gate toward Old Thornway and deeper Sylvaen paths.",
            "Glowleaf Lantern" =>
                "A lantern grown rather than built, tuned to keep lost initiates close to the safe grove paths.",
            _ => $"{landmarkName} is quiet."
        };
    }

    private static float Distance(Point a, PointF b) => Distance(new PointF(a.X, a.Y), b);

    private static float Distance(PointF a, PointF b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float Hash01(int seed)
    {
        var value = MathF.Sin(seed * 12.9898f) * 43758.5453f;
        return value - MathF.Floor(value);
    }

    private static float HashRange(int seed, float min, float max) => min + (max - min) * Hash01(seed);

    private static List<ZoneRegion> CreateThornveilRegions() =>
    [
        new("Heartbough Glade", new PointF(0, 0), 5.4f, 3.4f, Color.FromArgb(55, 138, 82), true, 0.1f),
        new("The Listener's Ring", new PointF(-4.2f, -3.8f), 3.4f, 2.3f, Color.FromArgb(80, 156, 110), true, 1.0f),
        new("Whisperroot Paths", new PointF(7.6f, -7.4f), 5.0f, 2.8f, Color.FromArgb(95, 118, 70), true, 2.1f),
        new("Mossglass Pools", new PointF(-18.0f, 8.4f), 5.8f, 3.3f, Color.FromArgb(50, 130, 150), true, 3.2f),
        new("Old Thornway", new PointF(19.4f, -10.4f), 5.4f, 2.4f, Color.FromArgb(134, 93, 55), true, 4.0f),
        new("The Green Scar", new PointF(16.2f, 13.0f), 4.8f, 3.2f, Color.FromArgb(117, 66, 143), true, 5.2f),
        new("Seedvault Veyr", new PointF(-6.6f, 15.6f), 4.0f, 2.6f, Color.FromArgb(80, 112, 74), true, 6.4f),
        new("Seedvault Antechamber", new PointF(-7.2f, 22.4f), 4.8f, 2.9f, Color.FromArgb(87, 65, 112), true, 6.9f),
        new("Thornwatch Threshold", new PointF(23.0f, -12.0f), 3.6f, 1.9f, Color.FromArgb(142, 104, 66), true, 7.1f),
        new("Nullroot Verge", new PointF(18.6f, 15.4f), 3.4f, 2.2f, Color.FromArgb(128, 74, 150), true, 7.8f)
    ];

    private static List<TerrainCorridor> CreateTerrainCorridors() =>
    [
        new(
            [
                new PointF(-18.8f, 8.8f),
                new PointF(-13.4f, 6.4f),
                new PointF(-7.8f, 3.8f),
                new PointF(-2.8f, 1.2f),
                new PointF(0.0f, 0.0f),
                new PointF(5.8f, -2.2f),
                new PointF(10.8f, -5.6f),
                new PointF(16.0f, -8.6f),
                new PointF(23.0f, -12.0f)
            ],
            1.25f,
            TerrainKind.Path,
            501),
        new(
            [
                new PointF(-4.2f, -3.8f),
                new PointF(-1.8f, -2.0f),
                new PointF(0.0f, 0.0f),
                new PointF(2.6f, 3.8f),
                new PointF(4.2f, 7.4f),
                new PointF(7.2f, 9.2f),
                new PointF(12.5f, 11.2f),
                new PointF(17.2f, 14.4f)
            ],
            1.05f,
            TerrainKind.Root,
            502),
        new(
            [
                new PointF(-7.2f, 22.4f),
                new PointF(-6.8f, 17.2f),
                new PointF(-5.4f, 12.8f),
                new PointF(-2.0f, 7.4f),
                new PointF(0.0f, 0.0f)
            ],
            1.05f,
            TerrainKind.Root,
            503),
        new(
            [
                new PointF(-18.0f, 8.4f),
                new PointF(-15.8f, 9.2f),
                new PointF(-12.8f, 8.4f)
            ],
            1.65f,
            TerrainKind.Water,
            504),
        new(
            [
                new PointF(14.4f, 12.8f),
                new PointF(16.2f, 13.0f),
                new PointF(19.0f, 15.4f)
            ],
            1.85f,
            TerrainKind.VoidScar,
            505)
    ];

    private static List<ForestEdge> CreateForestEdges() =>
    [
        new(new PointF(-12.8f, -8.9f), 5.0f, 2.8f, 1.1f, 601),
        new(new PointF(-6.2f, -10.8f), 6.0f, 2.6f, 1.0f, 602),
        new(new PointF(3.8f, -10.6f), 6.4f, 2.8f, 1.08f, 603),
        new(new PointF(11.6f, -8.8f), 5.4f, 2.5f, 0.98f, 604),
        new(new PointF(18.6f, -6.2f), 4.8f, 2.3f, 0.94f, 605),
        new(new PointF(-18.8f, -1.0f), 5.8f, 2.6f, 1.04f, 606),
        new(new PointF(-19.4f, 5.6f), 4.8f, 2.4f, 0.9f, 607),
        new(new PointF(-13.8f, 13.0f), 6.4f, 2.9f, 1.1f, 608),
        new(new PointF(-5.2f, 18.8f), 6.0f, 2.7f, 1.0f, 609),
        new(new PointF(5.4f, 16.0f), 5.2f, 2.4f, 0.96f, 610),
        new(new PointF(13.4f, 18.2f), 5.8f, 3.0f, 1.06f, 611),
        new(new PointF(21.4f, 11.4f), 5.2f, 2.8f, 0.96f, 612),
        new(new PointF(23.2f, 2.4f), 5.8f, 2.7f, 1.02f, 613),
        new(new PointF(20.6f, -13.8f), 4.8f, 2.4f, 0.9f, 614),
        new(new PointF(-23.0f, 8.6f), 4.4f, 2.4f, 0.86f, 615)
    ];

    private static List<TerrainPatch> CreateTerrainPatches() =>
    [
        new(new PointF(0, 0), new SizeF(10f, 2.2f), TerrainKind.Root, 11),
        new(new PointF(-5.5f, 1.5f), new SizeF(8f, 2.0f), TerrainKind.Path, 12),
        new(new PointF(5.8f, -1.1f), new SizeF(7.5f, 2.0f), TerrainKind.Path, 13),
        new(new PointF(-1.5f, -5.9f), new SizeF(5.6f, 1.55f), TerrainKind.Path, 14),
        new(new PointF(4.8f, 7.4f), new SizeF(6.8f, 1.45f), TerrainKind.Path, 15),
        new(new PointF(-8.5f, -6.2f), new SizeF(4.8f, 3.2f), TerrainKind.Moss, 21),
        new(new PointF(-13.8f, -2.2f), new SizeF(5.2f, 2.6f), TerrainKind.Moss, 22),
        new(new PointF(7.8f, 5.5f), new SizeF(5.8f, 3.4f), TerrainKind.Moss, 23),
        new(new PointF(13.8f, 2.9f), new SizeF(4.8f, 2.7f), TerrainKind.Moss, 24),
        new(new PointF(11.5f, -5.2f), new SizeF(4.4f, 2.6f), TerrainKind.Water, 31),
        new(new PointF(-11.8f, 5.8f), new SizeF(5.4f, 2.8f), TerrainKind.Water, 32),
        new(new PointF(1.7f, 8.5f), new SizeF(6.2f, 2.0f), TerrainKind.Path, 16),
        new(new PointF(-4.2f, 9.1f), new SizeF(5.6f, 2.2f), TerrainKind.Root, 17),
        new(new PointF(9.2f, -8.8f), new SizeF(5.8f, 1.9f), TerrainKind.Root, 18),
        new(new PointF(-1.2f, 3.2f), new SizeF(3.8f, 1.1f), TerrainKind.Root, 19),
        new(new PointF(7.7f, 9.6f), new SizeF(3.6f, 2.1f), TerrainKind.Moss, 25),
        new(new PointF(-15.2f, 7.6f), new SizeF(3.4f, 1.8f), TerrainKind.Path, 20),
        new(new PointF(-4.2f, -3.8f), new SizeF(4.4f, 1.7f), TerrainKind.Root, 26),
        new(new PointF(5.0f, -4.9f), new SizeF(5.2f, 1.6f), TerrainKind.Path, 27),
        new(new PointF(8.0f, 8.0f), new SizeF(4.3f, 2.5f), TerrainKind.VoidScar, 33),
        new(new PointF(3.2f, 3.8f), new SizeF(5.8f, 1.25f), TerrainKind.Root, 34),
        new(new PointF(-7.2f, 5.1f), new SizeF(5.9f, 1.35f), TerrainKind.Path, 35),
        new(new PointF(11.8f, 9.4f), new SizeF(4.4f, 1.5f), TerrainKind.VoidScar, 36),
        new(new PointF(14.0f, -9.2f), new SizeF(4.8f, 1.5f), TerrainKind.Path, 37),
        new(new PointF(-4.8f, 10.2f), new SizeF(4.8f, 1.35f), TerrainKind.Moss, 38),
        new(new PointF(-5.6f, 13.8f), new SizeF(6.8f, 2.2f), TerrainKind.Root, 39),
        new(new PointF(-6.6f, 16.0f), new SizeF(4.6f, 2.4f), TerrainKind.VoidScar, 40),
        new(new PointF(-3.4f, 12.6f), new SizeF(4.2f, 1.35f), TerrainKind.Path, 41),
        new(new PointF(7.6f, -7.4f), new SizeF(7.6f, 1.7f), TerrainKind.Path, 42),
        new(new PointF(11.5f, -8.3f), new SizeF(5.0f, 1.4f), TerrainKind.Moss, 43),
        new(new PointF(-18.0f, 8.4f), new SizeF(7.2f, 3.3f), TerrainKind.Water, 44),
        new(new PointF(-17.2f, 11.2f), new SizeF(5.4f, 1.6f), TerrainKind.Path, 45),
        new(new PointF(19.4f, -10.4f), new SizeF(8.6f, 1.8f), TerrainKind.Root, 46),
        new(new PointF(22.6f, -12.2f), new SizeF(5.2f, 1.5f), TerrainKind.Path, 47),
        new(new PointF(16.2f, 13.0f), new SizeF(6.4f, 3.1f), TerrainKind.VoidScar, 48),
        new(new PointF(18.8f, 15.4f), new SizeF(4.4f, 2.0f), TerrainKind.Moss, 49),
        new(new PointF(-6.6f, 15.6f), new SizeF(6.4f, 2.2f), TerrainKind.Root, 50),
        new(new PointF(-7.2f, 22.4f), new SizeF(7.2f, 2.8f), TerrainKind.VoidScar, 51),
        new(new PointF(-4.8f, 24.2f), new SizeF(4.8f, 1.5f), TerrainKind.Root, 52)
    ];

    private static List<WorldDecoration> CreateStarterDecorations() =>
    [
        new("Mooncap Cluster", "mushroom", new PointF(-3.5f, 5.6f), 0.8f, true),
        new("Mooncap Cluster", "mushroom", new PointF(3.2f, 6.8f), 0.7f, false),
        new("Mooncap Cluster", "mushroom", new PointF(-11.4f, -1.3f), 0.65f, false),
        new("Mooncap Cluster", "mushroom", new PointF(10.5f, 4.8f), 0.75f, false),
        new("Mooncap Cluster", "mushroom", new PointF(-6.9f, -7.7f), 0.55f, false),
        new("Glowleaf Lantern", "lantern", new PointF(-6.5f, -1.2f), 0.85f, true),
        new("Glowleaf Lantern", "lantern", new PointF(6.7f, 1.3f), 0.85f, false),
        new("Glowleaf Lantern", "lantern", new PointF(-0.6f, 7.3f), 0.78f, false),
        new("Glowleaf Lantern", "lantern", new PointF(12.8f, -6.7f), 0.72f, false),
        new("Scout Cache", "supply_cache", new PointF(2.7f, -5.2f), 0.9f, true),
        new("Scout Cache", "supply_cache", new PointF(-8.3f, 7.2f), 0.8f, false),
        new("Root Arch", "root_arch", new PointF(10.8f, -7.2f), 1.15f, true),
        new("Root Arch", "root_arch", new PointF(-13.7f, 2.6f), 0.88f, false),
        new("Thornwatch Marker", "runestone", new PointF(13.9f, -9.4f), 0.75f, true),
        new("Nullroot Warning", "runestone", new PointF(10.3f, 10.1f), 0.72f, true),
        new("Mossglass Marker", "lantern", new PointF(-10.3f, 4.1f), 0.62f, false),
        new("Scarbound Lantern", "lantern", new PointF(7.2f, 9.7f), 0.64f, false),
        new("Seedvault Cache", "supply_cache", new PointF(-5.4f, 10.0f), 0.75f, true),
        new("Seedvault Gate Marker", "runestone", new PointF(-3.9f, 8.5f), 0.84f, true),
        new("Glowleaf Lantern", "lantern", new PointF(-2.5f, 9.0f), 0.68f, false),
        new("Glowleaf Lantern", "lantern", new PointF(-6.1f, 8.6f), 0.68f, false),
        new("Antechamber Wardlight", "lantern", new PointF(-4.1f, 12.6f), 0.7f, true),
        new("Antechamber Wardlight", "lantern", new PointF(-7.5f, 14.5f), 0.64f, false),
        new("Root Arch", "root_arch", new PointF(-5.1f, 12.5f), 0.95f, true),
        new("Worldroot Fern", "herb", new PointF(-1.9f, 3.4f), 0.8f, false),
        new("Worldroot Fern", "herb", new PointF(8.9f, -4.2f), 0.75f, false),
        new("Worldroot Fern", "herb", new PointF(-12.6f, 4.8f), 0.7f, false),
        new("Worldroot Fern", "herb", new PointF(5.9f, 8.8f), 0.62f, false),
        new("Worldroot Fern", "herb", new PointF(-4.8f, -8.5f), 0.58f, false)
    ];

    private static List<AmbientMote> CreateAmbientMotes() =>
    [
        new(new PointF(-4.4f, -1.9f), 0.08f, 1.1f, 0.9f, 1.1f),
        new(new PointF(2.2f, -2.8f), 0.11f, 1.7f, 1.1f, 2.7f),
        new(new PointF(6.8f, 2.6f), 0.09f, 0.9f, 0.8f, 4.2f),
        new(new PointF(-8.2f, 3.7f), 0.12f, 1.4f, 0.9f, 5.4f),
        new(new PointF(11.6f, -4.9f), 0.1f, 1.2f, 0.75f, 6.1f),
        new(new PointF(-12.4f, 6.1f), 0.08f, 1.6f, 0.8f, 0.6f),
        new(new PointF(0.4f, 6.8f), 0.1f, 1.3f, 1.2f, 3.5f),
        new(new PointF(-1.1f, 0.8f), 0.13f, 0.8f, 0.7f, 2.1f),
        new(new PointF(8.6f, -8.1f), 0.09f, 1.5f, 0.85f, 5.0f),
        new(new PointF(-14.2f, -2.1f), 0.1f, 1.0f, 0.75f, 1.8f),
        new(new PointF(9.6f, 9.4f), 0.12f, 1.2f, 0.95f, 2.3f),
        new(new PointF(14.0f, -8.8f), 0.09f, 1.1f, 0.85f, 4.6f),
        new(new PointF(-5.1f, 9.8f), 0.1f, 1.0f, 0.9f, 5.9f)
    ];

    private static List<RootTree> CreateStarterTrees() =>
    [
        new(new PointF(-10, -6), 1.25f),
        new(new PointF(-7, 4), 0.9f),
        new(new PointF(-2, -9), 1.1f),
        new(new PointF(5, -7), 1.35f),
        new(new PointF(9, 3), 1.0f),
        new(new PointF(1, 7), 0.85f),
        new(new PointF(-12, 9), 1.15f),
        new(new PointF(12, -2), 0.95f),
        new(new PointF(-15, -1), 0.8f),
        new(new PointF(15, 4), 1.05f),
        new(new PointF(7, 10), 0.9f),
        new(new PointF(-5, -11), 0.95f),
        new(new PointF(11, 11), 0.85f),
        new(new PointF(14, -10), 0.75f),
        new(new PointF(-6, 11), 0.9f),
        new(new PointF(-15, 6), 0.8f)
    ];

    private static List<Runestone> CreateRunestones() =>
    [
        new("True Memory Root", new PointF(-4, -3), 0.1f),
        new("Fractured Memory Root", new PointF(5, -3), 1.7f),
        new("Unremembering Root", new PointF(4, 5), 3.1f),
        new("Rootbound Sigil", new PointF(-6, 4), 4.4f),
        new("Threshold Warden Stone", new PointF(13.9f, -9.4f), 5.3f),
        new("Scar Listening Stone", new PointF(9.7f, 9.5f), 6.2f),
        new("Seedvault Lockstone", new PointF(-5.0f, 12.3f), 7.0f)
    ];

    private static List<MemoryEcho> CreateMemoryEchoes() =>
    [
        new("Mossglass Testimony", new PointF(-17.4f, 8.0f), MemoryEchoKind.TrueMemory, 0.6f),
        new("Thornway Cutmark", new PointF(20.8f, -10.5f), MemoryEchoKind.FracturedMemory, 2.4f),
        new("Green Scar Nullroot", new PointF(16.6f, 13.1f), MemoryEchoKind.Unremembering, 4.1f),
        new("Seedvault Gate", new PointF(-6.1f, 15.1f), MemoryEchoKind.FracturedMemory, 5.2f),
        new("Veyr Wardseal", new PointF(-7.4f, 17.0f), MemoryEchoKind.TrueMemory, 6.0f),
        new("Veyr Memory Core", new PointF(-8.0f, 22.8f), MemoryEchoKind.Unremembering, 7.1f),
        new("Echo Footsteps", new PointF(7.0f, -7.4f), MemoryEchoKind.FracturedMemory, 0.9f),
        new("Oranyn Reflection", new PointF(-19.2f, 8.8f), MemoryEchoKind.TrueMemory, 1.5f),
        new("Drowned Name Fragment", new PointF(-16.6f, 10.8f), MemoryEchoKind.Unremembering, 2.1f),
        new("Old Thornway Lens", new PointF(21.0f, -11.2f), MemoryEchoKind.FracturedMemory, 2.8f),
        new("Nameless Patch", new PointF(17.2f, 14.0f), MemoryEchoKind.Unremembering, 3.4f),
        new("Growth Key", new PointF(-4.4f, 15.2f), MemoryEchoKind.TrueMemory, 3.9f),
        new("Memory Key", new PointF(-6.8f, 17.3f), MemoryEchoKind.TrueMemory, 4.5f),
        new("Decay Key", new PointF(-8.8f, 15.5f), MemoryEchoKind.FracturedMemory, 5.1f),
        new("Root Descent", new PointF(-7.0f, 19.5f), MemoryEchoKind.FracturedMemory, 5.7f),
        new("Hall of Unchosen Branches", new PointF(-8.6f, 24.3f), MemoryEchoKind.Unremembering, 6.3f),
        new("Oranyn Chorus Anchor", new PointF(-4.8f, 24.8f), MemoryEchoKind.TrueMemory, 6.9f),
        new("Circle Debate Root", new PointF(0.8f, 1.1f), MemoryEchoKind.TrueMemory, 7.5f),
        new("Anchor the Memory", new PointF(-1.4f, 1.7f), MemoryEchoKind.TrueMemory, 8.1f),
        new("Prune the Memory", new PointF(1.5f, 1.7f), MemoryEchoKind.FracturedMemory, 8.7f),
        new("Aelthar Copy Lens", new PointF(0.0f, 2.4f), MemoryEchoKind.Unremembering, 9.3f),
        new("Ceremony Root", new PointF(0.0f, 0.0f), MemoryEchoKind.TrueMemory, 9.9f),
        new("Road Beyond Thornveil", new PointF(24.0f, -13.2f), MemoryEchoKind.FracturedMemory, 10.5f),
        new("Tomorrow Fire Site", new PointF(8.4f, -10.4f), MemoryEchoKind.FracturedMemory, 11.1f),
        new("Wolf Memory Den", new PointF(-20.6f, 7.2f), MemoryEchoKind.Unremembering, 11.7f),
        new("Memory Trader Cache", new PointF(-12.8f, -5.8f), MemoryEchoKind.TrueMemory, 12.3f),
        new("Weakening Prayer Stone", new PointF(8.4f, 9.6f), MemoryEchoKind.TrueMemory, 12.9f),
        new("Living Bark Forge", new PointF(21.8f, -9.2f), MemoryEchoKind.FracturedMemory, 13.5f),
        new("Bleeding Root Bloom", new PointF(7.8f, -5.9f), MemoryEchoKind.Unremembering, 14.1f),
        new("Aelthar Breach Lens", new PointF(23.4f, -11.4f), MemoryEchoKind.FracturedMemory, 14.7f),
        new("Nameless Patch Names", new PointF(18.7f, 15.5f), MemoryEchoKind.TrueMemory, 15.3f)
    ];

    private enum TerrainKind
    {
        Moss,
        Path,
        Water,
        Root,
        VoidScar
    }

    private readonly record struct ZoneRegion(string Name, PointF Position, float RadiusX, float RadiusY, Color Color,
        bool Selectable, float Phase);

    private readonly record struct TerrainCorridor(IReadOnlyList<PointF> Points, float Width, TerrainKind Kind,
        int Seed);

    private readonly record struct ForestEdge(PointF Position, float Width, float Depth, float Scale, int Seed)
    {
        public float Phase => Seed * 0.137f;
    }

    private readonly record struct TerrainPatch(PointF Position, SizeF Size, TerrainKind Kind, int Seed);

    private static List<GroundDetail> CreateGroundDetails()
    {
        var details = new List<GroundDetail>();
        for (var i = 0; i < 110; i++)
        {
            var x = HashRange(i * 17 + 3, -17.5f, 17.5f);
            var y = HashRange(i * 29 + 5, -11.5f, 11.5f);
            var distanceFromHeart = MathF.Sqrt(x * x + y * y);
            if (distanceFromHeart < 1.35f)
            {
                x += 2.2f;
            }

            var kind = (i % 11) switch
            {
                0 or 4 or 7 => GroundDetailKind.Grass,
                1 or 8 => GroundDetailKind.Pebble,
                2 or 9 => GroundDetailKind.Leaf,
                3 or 6 => GroundDetailKind.RootVein,
                _ => GroundDetailKind.GlowBloom
            };
            var scale = HashRange(i * 37 + 9, 0.55f, 1.25f);
            var rotation = HashRange(i * 41 + 13, 0f, MathF.Tau);
            details.Add(new GroundDetail(new PointF(x, y), kind, scale, rotation, i));
        }

        details.AddRange(new[]
        {
            new GroundDetail(new PointF(-0.8f, -0.5f), GroundDetailKind.RootVein, 1.7f, 0.35f, 200),
            new GroundDetail(new PointF(0.9f, 0.2f), GroundDetailKind.RootVein, 1.4f, 2.72f, 201),
            new GroundDetail(new PointF(1.8f, -1.0f), GroundDetailKind.GlowBloom, 1.3f, 1.2f, 202),
            new GroundDetail(new PointF(-2.2f, 0.9f), GroundDetailKind.GlowBloom, 1.1f, 4.4f, 203)
        });

        return details;
    }

    private readonly record struct WorldDecoration(string Name, string SpriteName, PointF Position, float Scale,
        bool Selectable);

    private readonly record struct AmbientMote(PointF Position, float Size, float Range, float Speed, float Phase);

    private readonly record struct RootTree(PointF Position, float Scale);

    private readonly record struct Runestone(string Name, PointF Position, float Phase);

    private readonly record struct MemoryEcho(string Name, PointF Position, MemoryEchoKind Kind, float Phase);

    private readonly record struct FloatingCombatText(string Text, PointF Position, Color Color, float Age, float Lifetime);

    private readonly record struct LootFeedItem(string Message, float Age, float Lifetime);

    private readonly record struct RenderLayer(float Depth, Action Draw);

    private readonly record struct TargetPick(ulong? EntityId, string? LandmarkName)
    {
        public bool HasTarget => EntityId is not null || LandmarkName is not null;
    }

    private readonly record struct GroundDetail(PointF Position, GroundDetailKind Kind, float Scale, float Rotation,
        int Variant);

    private enum GroundDetailKind
    {
        Grass,
        Pebble,
        Leaf,
        RootVein,
        GlowBloom
    }

    private enum MemoryEchoKind
    {
        TrueMemory,
        FracturedMemory,
        Unremembering
    }

    private struct SceneEntity
    {
        public ulong EntityId { get; init; }
        public string Name { get; init; }
        public PointF Position { get; set; }
        public int? NpcTemplateId { get; init; }
        public bool IsHostile { get; init; }
        public bool IsQuestGiver { get; init; }
        public int MaxHealth { get; init; }
        public int CurrentHealth { get; set; }
        public ulong? TargetEntityId { get; set; }
        public string State { get; set; }
        public float SpawnAge { get; set; }
        public float HitFlash { get; set; }
        public float AttackFlash { get; set; }
        public PointF? AttackTargetPosition { get; set; }
        public float DeathFade { get; set; }

        public static SceneEntity FromSpawn(WorldPackets.EntitySpawnPacket packet)
        {
            return new SceneEntity
            {
                EntityId = packet.EntityId,
                Name = packet.Name,
                Position = new PointF(packet.Position.X, packet.Position.Y),
                NpcTemplateId = packet.NPCData?.NPCTemplateId,
                IsHostile = packet.NPCData?.IsHostile ?? false,
                IsQuestGiver = packet.NPCData?.IsQuestGiver ?? false,
                MaxHealth = Math.Max(0, packet.NPCData?.MaxHealth ?? packet.Resources?.MaxHealth ?? 0),
                CurrentHealth = Math.Max(0, packet.NPCData?.CurrentHealth ?? packet.Resources?.CurrentHealth ?? 0),
                State = "Spawned"
            };
        }
    }
}
