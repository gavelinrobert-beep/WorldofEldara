using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using MessagePack;
using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Server.World;
using WorldofEldara.Shared.Constants;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;
using WorldofEldara.Shared.Data.Quest;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;
using WorldofEldara.Shared.Data.World;
using ProtocolEntityType = WorldofEldara.Shared.Protocol.Packets.EntityType;

namespace WorldofEldara.Server.Networking;

/// <summary>
///     Represents a single client connection.
///     Handles packet sending/receiving and game state for this client.
/// </summary>
public class ClientConnection
{
    private readonly object _sendLock = new();
    private readonly Queue<byte[]> _sendQueue = new();
    private readonly NetworkServer _server;
    private readonly NetworkStream _stream;

    private readonly TcpClient _tcpClient;
    private readonly WorldSimulation _worldSimulation;

    private bool _isConnected = true;
    private Thread? _receiveThread;

    public ClientConnection(ulong connectionId, TcpClient tcpClient, NetworkServer server,
        WorldSimulation worldSimulation)
    {
        ConnectionId = connectionId;
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
        _server = server;
        _worldSimulation = worldSimulation;
    }

    private static readonly ConcurrentDictionary<ulong, List<CharacterData>> AccountCharacters = new();
    private static ulong _nextCharacterId = 10000;
    private static readonly object CharacterIdLock = new();

    public ulong ConnectionId { get; }
    public ulong? AccountId { get; private set; }
    public ulong? PlayerEntityId { get; private set; }
    public string? CurrentZoneId { get; private set; }

    public void Start()
    {
        _receiveThread = new Thread(ReceiveLoop)
        {
            Name = $"ClientReceive_{ConnectionId}",
            IsBackground = true
        };
        _receiveThread.Start();
    }

    public void Disconnect(string reason)
    {
        if (!_isConnected) return;

        Log.Information($"Disconnecting client [{ConnectionId}]: {reason}");
        _isConnected = false;

        try
        {
            _stream.Close();
            _tcpClient.Close();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error closing connection [{ConnectionId}]");
        }

        _server.OnClientDisconnected(ConnectionId);
    }

    public void SendPacket(byte[] data)
    {
        if (!_isConnected) return;

        lock (_sendLock)
        {
            _sendQueue.Enqueue(data);
        }

        // Send immediately
        ProcessSendQueue();
    }

    private void ProcessSendQueue()
    {
        lock (_sendLock)
        {
            while (_sendQueue.Count > 0 && _isConnected)
            {
                var data = _sendQueue.Dequeue();

                try
                {
                    // Write packet length (4 bytes)
                    var lengthBytes = BitConverter.GetBytes(data.Length);
                    _stream.Write(lengthBytes, 0, 4);

                    // Write packet data
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error sending packet to [{ConnectionId}]");
                    Disconnect("Send error");
                    break;
                }
            }
        }
    }

    private void ReceiveLoop()
    {
        var lengthBuffer = new byte[4];

        while (_isConnected)
            try
            {
                // Read packet length
                var bytesRead = _stream.Read(lengthBuffer, 0, 4);
                if (bytesRead != 4)
                {
                    Disconnect("Invalid packet length");
                    break;
                }

                var packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Validate length
                if (packetLength <= 0 || packetLength > NetworkConstants.MaxPacketSize)
                {
                    Disconnect($"Invalid packet size: {packetLength}");
                    break;
                }

                // Read packet data
                var packetData = new byte[packetLength];
                var totalRead = 0;
                while (totalRead < packetLength)
                {
                    bytesRead = _stream.Read(packetData, totalRead, packetLength - totalRead);
                    if (bytesRead == 0)
                    {
                        Disconnect("Connection closed during packet read");
                        break;
                    }

                    totalRead += bytesRead;
                }

                if (totalRead != packetLength) break;

                // Process packet
                ProcessPacket(packetData);
            }
            catch (IOException)
            {
                Disconnect("IO error");
                break;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in receive loop [{ConnectionId}]");
                Disconnect("Receive error");
                break;
            }
    }

    private void ProcessPacket(byte[] data)
    {
        try
        {
            var packet = MessagePackSerializer.Deserialize<PacketBase>(data);

            // Route packet to appropriate handler
            switch (packet)
            {
                case AuthPackets.LoginRequest loginRequest:
                    HandleLogin(loginRequest);
                    break;

                case CharacterPackets.CharacterListRequest listRequest:
                    HandleCharacterListRequest(listRequest);
                    break;

                case CharacterPackets.CreateCharacterRequest createRequest:
                    HandleCreateCharacter(createRequest);
                    break;

                case CharacterPackets.SelectCharacterRequest selectRequest:
                    HandleSelectCharacter(selectRequest);
                    break;

                case MovementPackets.MovementInputPacket movementInput:
                    HandleMovementInput(movementInput);
                    break;

                case CombatPackets.UseAbilityRequest abilityRequest:
                    HandleUseAbility(abilityRequest);
                    break;

                case ChatPackets.ChatMessagePacket chatMessage:
                    HandleChatMessage(chatMessage);
                    break;

                case QuestPackets.QuestAcceptRequest questAccept:
                    HandleQuestAccept(questAccept);
                    break;

                case QuestPackets.QuestDialogueRequest dialogueRequest:
                    HandleQuestDialogue(dialogueRequest);
                    break;

                default:
                    Log.Warning($"Unhandled packet type: {packet.GetType().Name}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error processing packet from [{ConnectionId}]");
        }
    }

    // ===== Packet Handlers =====

    private void HandleLogin(AuthPackets.LoginRequest request)
    {
        Log.Information($"Login request from [{ConnectionId}]: {request.Username}");

        // TODO: Validate credentials
        // For now, accept any login

        AccountId = 1000 + ConnectionId; // Fake account ID
        AccountCharacters.TryAdd(AccountId.Value, new List<CharacterData>());

        var response = new AuthPackets.LoginResponse
        {
            Result = ResponseCode.Success,
            Message = "Login successful",
            AccountId = AccountId.Value,
            SessionToken = Guid.NewGuid().ToString()
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));
    }

    private void HandleCharacterListRequest(CharacterPackets.CharacterListRequest request)
    {
        Log.Debug($"Character list request from [{ConnectionId}]");

        // TODO: Load from database
        // For now, return empty list

        if (!AccountId.HasValue)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.CharacterListResponse
            {
                Result = ResponseCode.NotAuthenticated,
                Characters = new List<CharacterData>()
            }));
            return;
        }

        var characters = AccountCharacters.GetOrAdd(AccountId.Value, _ => new List<CharacterData>());
        List<CharacterData> snapshot;
        lock (characters)
        {
            snapshot = characters.ToList();
        }

        var response = new CharacterPackets.CharacterListResponse
        {
            Result = ResponseCode.Success,
            Characters = snapshot
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));
    }

    private void HandleCreateCharacter(CharacterPackets.CreateCharacterRequest request)
    {
        Log.Information(
            $"Create character request from [{ConnectionId}]: {request.Name} ({request.Race}, {request.Class})");

        // Validate lore consistency
        if (!AccountId.HasValue)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.CreateCharacterResponse
            {
                Result = ResponseCode.NotAuthenticated,
                Message = "Login required"
            }));
            return;
        }

        if (!IsNameValid(request.Name))
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.CreateCharacterResponse
            {
                Result = ResponseCode.InvalidName,
                Message = "Name must be 3-16 letters, apostrophes or hyphens"
            }));
            return;
        }

        if (!ClassInfo.IsClassAvailableForRace(request.Class, request.Race))
        {
            var errorResponse = new CharacterPackets.CreateCharacterResponse
            {
                Result = ResponseCode.InvalidRaceClassCombination,
                Message = "Invalid race-class combination (lore violation)"
            };
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(errorResponse));
            return;
        }

        var accountCharacters = AccountCharacters.GetOrAdd(AccountId.Value, _ => new List<CharacterData>());
        CharacterData character;
        lock (accountCharacters)
        {
            if (accountCharacters.Any(c => string.Equals(c.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
            {
                SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.CreateCharacterResponse
                {
                    Result = ResponseCode.NameTaken,
                    Message = "Name already taken on this account"
                }));
                return;
            }

            // Create character
            var characterId = GenerateCharacterId();
            character = new CharacterData
            {
                CharacterId = characterId,
                AccountId = AccountId ?? 0,
                Name = request.Name,
                Race = request.Race,
                Class = request.Class,
                Faction = request.Faction,
                TotemSpirit = request.TotemSpirit,
                Level = 1,
                ExperiencePoints = 0,
                Appearance = request.Appearance,
                CreatedAt = DateTime.UtcNow,
                LastPlayedAt = DateTime.UtcNow
            };

            // Set starting position based on faction
            // Vertical slice: funnel all new characters into the shared starter loop
            var starterZone = ZoneConstants.Zone01;
            var zoneDef = ZoneDefinitions.GetZone(starterZone) ?? ZoneDefinitions.GetZone(ZoneConstants.Borderkeep);
            character.Position = new CharacterPosition
            {
                ZoneId = starterZone,
                X = zoneDef?.SafeSpawnPoint.X ?? 0,
                Y = zoneDef?.SafeSpawnPoint.Y ?? 0,
                Z = zoneDef?.SafeSpawnPoint.Z ?? 0
            };

            ApplyClassArchetype(character);

            accountCharacters.Add(character);
        }

        var response = new CharacterPackets.CreateCharacterResponse
        {
            Result = ResponseCode.Success,
            Message = "Character created successfully",
            Character = character
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));
    }

    private void HandleSelectCharacter(CharacterPackets.SelectCharacterRequest request)
    {
        Log.Information($"Select character request from [{ConnectionId}]: {request.CharacterId}");

        if (!AccountId.HasValue)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.SelectCharacterResponse
            {
                Result = ResponseCode.NotAuthenticated,
                Message = "Login required"
            }));
            return;
        }

        var accountCharacters = AccountCharacters.GetOrAdd(AccountId.Value, _ => new List<CharacterData>());
        CharacterData? character;
        lock (accountCharacters)
        {
            character = accountCharacters.FirstOrDefault(c => c.CharacterId == request.CharacterId);
        }

        if (character == null)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CharacterPackets.SelectCharacterResponse
            {
                Result = ResponseCode.NotFound,
                Message = "Character not found"
            }));
            return;
        }

        var archetype = ApplyClassArchetype(character);
        character.LastPlayedAt = DateTime.UtcNow;

        // Create player entity in world
        var playerEntity = new PlayerEntity
        {
            EntityId = _worldSimulation.Entities.GenerateEntityId(),
            AccountId = AccountId ?? 0,
            CharacterData = character,
            Name = character.Name,
            ZoneId = character.Position.ZoneId,
            Position = new Vector3(character.Position.X, character.Position.Y, character.Position.Z),
            ClientConnection = this,
            ResourceType = archetype.ResourceType
        };
        playerEntity.KnownAbilities.UnionWith(ClassArchetypes.GetAbilitiesForLevel(character.Class, character.Level));
        playerEntity.GlobalCooldownEnd = DateTime.UtcNow;

        _worldSimulation.Entities.AddEntity(playerEntity);
        PlayerEntityId = playerEntity.EntityId;
        CurrentZoneId = character.Position.ZoneId;

        // Send success response
        var selectResponse = new CharacterPackets.SelectCharacterResponse
        {
            Result = ResponseCode.Success,
            Message = "Character loaded",
            Character = character
        };
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(selectResponse));

        // Send enter world packet
        var enterWorld = new WorldPackets.EnterWorldPacket
        {
            Character = character,
            ZoneId = character.Position.ZoneId,
            ServerTime = _worldSimulation.GetServerTimestamp()
        };
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(enterWorld));

        var playerSnapshot = CharacterSnapshot.FromCharacter(character, playerEntity.KnownAbilities.ToList());
        var abilitySummaries = playerEntity.KnownAbilities
            .Select(id => AbilitySummary.From(AbilityBook.GetAbility(id)))
            .ToList();
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(new WorldPackets.PlayerSpawnPacket
        {
            Character = playerSnapshot,
            Resources = ResourceSnapshot.FromStats(character.Stats),
            ZoneId = character.Position.ZoneId,
            ServerTime = _worldSimulation.GetServerTimestamp(),
            Abilities = abilitySummaries
        }));

        // Sync quest log for this character (server authoritative)
        var questStates = _worldSimulation.Quests.GetQuestStates(character.CharacterId);
        var questDefinitions = _worldSimulation.Quests.GetDefinitionsForStates(questStates);
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(new QuestPackets.QuestLogSnapshot
        {
            Definitions = questDefinitions,
            States = questStates
        }));

        // Send existing entities in the same zone to the player for initial sync
        foreach (var entity in _worldSimulation.Entities.GetEntitiesInZone(playerEntity.ZoneId)
                     .Where(e => e.EntityId != playerEntity.EntityId))
        {
            var spawn = new WorldPackets.EntitySpawnPacket
            {
                EntityId = entity.EntityId,
                Type = (ProtocolEntityType)(int)entity.GetEntityType(),
                Name = entity.Name,
                Position = entity.Position,
                RotationYaw = entity.RotationYaw
            };

            if (entity is PlayerEntity otherPlayer) spawn.CharacterData = otherPlayer.CharacterData;
            if (entity is NPCEntity npc)
                spawn.NPCData = new NPCData
                {
                    NPCTemplateId = npc.NPCTemplateId,
                    Name = npc.Name,
                    Level = npc.Level,
                    Faction = npc.Faction,
                    IsHostile = npc.IsHostile,
                    IsQuestGiver = npc.IsQuestGiver,
                    IsVendor = npc.IsVendor,
                    MaxHealth = npc.MaxHealth,
                    CurrentHealth = npc.CurrentHealth
                };

            SendPacket(MessagePackSerializer.Serialize<PacketBase>(spawn));
        }

        Log.Information($"Player [{ConnectionId}] entered world as {character.Name}");
    }

    private void HandleMovementInput(MovementPackets.MovementInputPacket packet)
    {
        if (!PlayerEntityId.HasValue) return;

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null) return;

        var deltaTime = packet.DeltaTime;
        if (deltaTime <= 0) deltaTime = 1f / 20f;
        deltaTime = Math.Min(deltaTime, 0.25f);

        var desiredState = packet.Input;
        var speed = player.CharacterData.Stats.MovementSpeed *
                    (desiredState.Sprint ? GameConstants.SprintMultiplier : 1f);

        var dirX = desiredState.Forward;
        var dirY = desiredState.Strafe;
        var magnitudeSq = dirX * dirX + dirY * dirY;
        var magnitude = 0f;
        if (magnitudeSq > 1e-6f)
        {
            magnitude = (float)Math.Sqrt(magnitudeSq);
            dirX /= magnitude;
            dirY /= magnitude;
        }

        var displacement = new Vector3(
            player.Position.X + dirX * speed * deltaTime,
            player.Position.Y + dirY * speed * deltaTime,
            player.Position.Z);

        // Authoritative position update
        player.LastProcessedInputSequence = packet.InputSequence;
        player.Velocity = new Vector3(dirX * speed, dirY * speed, 0);
        if (!_worldSimulation.Zones.IsPositionInZone(player.ZoneId, displacement.X, displacement.Y, displacement.Z))
        {
            player.Velocity = new Vector3(0, 0, 0);
        }
        else
        {
            player.Position = displacement;
        }
        player.RotationYaw = desiredState.LookYaw;
        player.RotationPitch = desiredState.LookPitch;
        player.MovementState = magnitudeSq > 0.01f &&
                               (Math.Abs(player.Velocity.X) > 0.001f || Math.Abs(player.Velocity.Y) > 0.001f)
            ? MovementState.Running
            : MovementState.Idle;
        player.LastInputTime = DateTime.UtcNow;

        // Reconciliation if client prediction diverges
        var predictionError = Distance(player.Position, packet.PredictedPosition);
        if (predictionError > 1.0f)
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new MovementPackets.PositionCorrectionPacket
            {
                LastProcessedInput = packet.InputSequence,
                AuthoritativePosition = player.Position,
                AuthoritativeVelocity = player.Velocity,
                AuthoritativeRotationYaw = player.RotationYaw,
                ServerTimestamp = _worldSimulation.GetServerTimestamp()
            }));

        var movementUpdate = new MovementPackets.MovementUpdatePacket
        {
            EntityId = player.EntityId,
            Position = player.Position,
            Velocity = player.Velocity,
            RotationYaw = player.RotationYaw,
            RotationPitch = player.RotationPitch,
            State = player.MovementState,
            ServerTimestamp = _worldSimulation.GetServerTimestamp()
        };

        var serialized = MessagePackSerializer.Serialize<PacketBase>(movementUpdate);

        // Send authoritative update back to mover and broadcast to others in zone
        SendPacket(serialized);
        if (CurrentZoneId != null)
            _server.BroadcastToZone(CurrentZoneId, serialized, ConnectionId);
    }

    private void HandleUseAbility(CombatPackets.UseAbilityRequest packet)
    {
        if (!PlayerEntityId.HasValue) return;

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null) return;

        if (!player.IsAlive)
        {
            SendAbilityResult(ResponseCode.InvalidRequest, player, packet.AbilityId, packet.InputSequence,
                "You are dead");
            return;
        }

        Log.Debug($"Player [{ConnectionId}] using ability {packet.AbilityId}");

        Ability ability;
        try
        {
            ability = AbilityBook.GetAbility(packet.AbilityId);
        }
        catch (KeyNotFoundException)
        {
            SendAbilityResult(ResponseCode.InvalidRequest, player, packet.AbilityId, packet.InputSequence,
                "Unknown ability");
            return;
        }

        if (ability.AllowedClasses.Count > 0 && !ability.AllowedClasses.Contains(player.CharacterData.Class))
        {
            SendAbilityResult(ResponseCode.InvalidRequest, player, ability.AbilityId, packet.InputSequence,
                "Class cannot use this ability");
            return;
        }

        if (player.CharacterData.Level < ability.RequiredLevel)
        {
            SendAbilityResult(ResponseCode.InvalidRequest, player, ability.AbilityId, packet.InputSequence,
                "Level too low");
            return;
        }

        if (!player.KnownAbilities.Contains(ability.AbilityId))
        {
            SendAbilityResult(ResponseCode.InvalidRequest, player, ability.AbilityId, packet.InputSequence,
                "Ability not known");
            return;
        }

        var target = ResolveTarget(player, ability, packet);
        if (target == null || target.ZoneId != player.ZoneId)
        {
            SendAbilityResult(ResponseCode.InvalidTarget, player, ability.AbilityId, packet.InputSequence,
                "No valid target");
            return;
        }

        if (!IsTargetAlive(target))
        {
            SendAbilityResult(ResponseCode.InvalidTarget, player, ability.AbilityId, packet.InputSequence,
                "Target already defeated");
            return;
        }

        var distance = Distance(player.Position, target.Position);
        if (distance > ability.Range)
        {
            SendAbilityResult(ResponseCode.NotInRange, player, ability.AbilityId, packet.InputSequence,
                "Target out of range");
            return;
        }

        var now = DateTime.UtcNow;
        if (ability.TriggersGlobalCooldown && player.GlobalCooldownEnd > now)
        {
            var remaining = (player.GlobalCooldownEnd - now).TotalSeconds;
            SendAbilityResult(ResponseCode.OnCooldown, player, ability.AbilityId, packet.InputSequence,
                $"Global cooldown ({remaining:F1}s)");
            return;
        }

        if (player.AbilityCooldowns.TryGetValue(ability.AbilityId, out var readyAt) && readyAt > now)
        {
            var remaining = (readyAt - now).TotalSeconds;
            SendAbilityResult(ResponseCode.OnCooldown, player, ability.AbilityId, packet.InputSequence,
                $"Cooldown ({remaining:F1}s)");
            return;
        }

        if (!HasRequiredResources(player, ability, out var resourceMessage, out var resourceCode))
        {
            SendAbilityResult(resourceCode, player, ability.AbilityId, packet.InputSequence,
                resourceMessage);
            return;
        }

        ExecuteAbility(player, target, ability, now, packet.InputSequence);
    }

    private void HandleChatMessage(ChatPackets.ChatMessagePacket packet)
    {
        if (!PlayerEntityId.HasValue) return;

        Log.Information($"Chat from [{ConnectionId}] ({packet.Channel}): {packet.Message}");

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null || CurrentZoneId == null) return;

        var outbound = new ChatPackets.ChatMessagePacket
        {
            Channel = packet.Channel,
            SenderEntityId = player.EntityId,
            SenderName = player.Name,
            Message = packet.Message,
            TargetEntityId = packet.TargetEntityId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var serialized = MessagePackSerializer.Serialize<PacketBase>(outbound);

        if (packet.Channel == ChatChannel.Whisper && packet.TargetEntityId.HasValue)
        {
            var target = _worldSimulation.Entities.GetEntity(packet.TargetEntityId.Value) as PlayerEntity;
            if (target?.ClientConnection is ClientConnection targetConnection)
            {
                targetConnection.SendPacket(serialized);
                SendPacket(serialized);
            }

            return;
        }

        _server.BroadcastToZone(CurrentZoneId, serialized);
    }

    private void HandleQuestAccept(QuestPackets.QuestAcceptRequest request)
    {
        if (!PlayerEntityId.HasValue) return;

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null) return;

        var result = _worldSimulation.Quests.AcceptQuest(player, request.QuestId);
        var response = new QuestPackets.QuestAcceptResponse
        {
            Result = result.Code,
            Message = result.Message,
            State = result.State,
            Definition = result.Definition
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));

        if (result.Code == ResponseCode.Success && result.State != null)
            SendQuestUpdate(result.State, result.Definition);
    }

    private void HandleQuestDialogue(QuestPackets.QuestDialogueRequest request)
    {
        if (!PlayerEntityId.HasValue) return;

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null) return;

        var npc = _worldSimulation.Entities.GetEntity(request.NpcEntityId) as NPCEntity;
        var dialogue = _worldSimulation.Quests.BuildDialogue(player, npc, request.NpcTemplateId);

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(dialogue));

        foreach (var updated in dialogue.UpdatedStates) SendQuestUpdate(updated);
    }

    private void SendQuestUpdate(QuestStateData state, QuestDefinition? definition = null)
    {
        var update = new QuestPackets.QuestProgressUpdate
        {
            State = state,
            Definition = definition
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(update));
    }

    private static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private Entity? ResolveTarget(PlayerEntity caster, Ability ability, CombatPackets.UseAbilityRequest packet)
    {
        if (ability.TargetType == TargetType.Self) return caster;
        if (!packet.TargetEntityId.HasValue) return null;
        return _worldSimulation.Entities.GetEntity(packet.TargetEntityId.Value);
    }

    private static bool IsTargetAlive(Entity target)
    {
        return target switch
        {
            PlayerEntity player => player.CharacterData.Stats.CurrentHealth > 0,
            NPCEntity npc => npc.CurrentHealth > 0,
            _ => true
        };
    }

    private bool HasRequiredResources(PlayerEntity caster, Ability ability, out string message,
        out ResponseCode responseCode)
    {
        message = string.Empty;
        responseCode = ResponseCode.Success;
        if (ability.ManaCost <= 0) return true;

        var (current, _, label) = GetResourceSnapshot(caster);
        if (current >= ability.ManaCost) return true;

        message = $"Not enough {label}";
        responseCode = label == "mana" ? ResponseCode.NotEnoughMana : ResponseCode.InsufficientResources;
        return false;
    }

    private void ConsumeResources(PlayerEntity caster, Ability ability)
    {
        if (ability.ManaCost <= 0) return;

        var (_, pool, _) = GetResourceSnapshot(caster);
        SpendResource(caster, pool, ability.ManaCost);
    }

    private static void SpendResource(PlayerEntity caster, ResourcePool pool, int cost)
    {
        switch (pool)
        {
            case ResourcePool.Mana:
                caster.CharacterData.Stats.CurrentMana =
                    Math.Max(0, caster.CharacterData.Stats.CurrentMana - cost);
                break;
            case ResourcePool.Stamina:
                caster.CharacterData.Stats.CurrentStamina =
                    Math.Max(0, caster.CharacterData.Stats.CurrentStamina - cost);
                break;
        }
    }

    private (int current, ResourcePool pool, string label) GetResourceSnapshot(PlayerEntity caster)
    {
        return caster.ResourceType switch
        {
            EResourceType.Mana => (caster.CharacterData.Stats.CurrentMana, ResourcePool.Mana, "mana"),
            EResourceType.Stamina => (caster.CharacterData.Stats.CurrentStamina, ResourcePool.Stamina, "stamina"),
            EResourceType.Rage => (caster.CharacterData.Stats.CurrentStamina, ResourcePool.Stamina, "rage"), // Shared physical pool until dedicated resources exist
            EResourceType.Energy => (caster.CharacterData.Stats.CurrentStamina, ResourcePool.Stamina, "energy"),
            EResourceType.Focus => (caster.CharacterData.Stats.CurrentStamina, ResourcePool.Stamina, "focus"),
            EResourceType.Corruption => (caster.CharacterData.Stats.CurrentMana, ResourcePool.Mana,
                "corruption"), // Dominion magic currently uses the mana pool until a bespoke corruption resource exists
            _ => (caster.CharacterData.Stats.CurrentMana, ResourcePool.Mana, "resources")
        };
    }

    private enum ResourcePool
    {
        Mana,
        Stamina
    }

    private void ExecuteAbility(PlayerEntity caster, Entity target, Ability ability, DateTime now,
        uint inputSequence)
    {
        caster.TargetEntityId = target.EntityId;

        ConsumeResources(caster, ability);
        if (ability.Cooldown > 0) caster.AbilityCooldowns[ability.AbilityId] = now.AddSeconds(ability.Cooldown);
        if (ability.TriggersGlobalCooldown)
        {
            var gcd = ability.GlobalCooldown > 0 ? ability.GlobalCooldown : CombatConstants.DefaultGlobalCooldown;
            caster.GlobalCooldownEnd = now.AddSeconds(gcd);
        }

        caster.MarkCombatEngaged();
        if (target is PlayerEntity targetPlayer) targetPlayer.MarkCombatEngaged();

        var isCrit = ability.CanCrit && Random.Shared.NextDouble() < caster.CharacterData.Stats.CriticalChance;
        var value = ability.CalculateValue(caster.CharacterData.Stats, isCrit);

        switch (ability.Type)
        {
            case AbilityType.Healing:
                ApplyHealing(caster, target, value, isCrit, ability.AbilityId);
                break;
            case AbilityType.PhysicalDamage or AbilityType.MeleeDamage or AbilityType.SpellDamage:
                ApplyDamage(caster, target, value, ability.DamageType, isCrit, ability.AbilityId);
                break;
        }

        SendAbilityResult(ResponseCode.Success, caster, ability.AbilityId, inputSequence, "Ability executed");
    }

    private void ApplyDamage(PlayerEntity source, Entity target, int rawAmount, DamageType damageType, bool isCrit,
        int abilityId)
    {
        if (target is NPCEntity npc)
        {
            npc.TargetEntityId ??= source.EntityId;
            npc.AIState = NPCAIState.Combat;
        }

        var finalAmount = ApplyMitigation(target, damageType, rawAmount);

        if (target is PlayerEntity playerTarget)
        {
            playerTarget.CharacterData.Stats.CurrentHealth =
                Math.Max(0, playerTarget.CharacterData.Stats.CurrentHealth - finalAmount);
            BroadcastDamage(source.EntityId, playerTarget.EntityId, finalAmount, damageType, isCrit,
                playerTarget.CharacterData.Stats.CurrentHealth, playerTarget.ZoneId, abilityId);

            if (playerTarget.CharacterData.Stats.CurrentHealth <= 0) HandleEntityDeath(playerTarget, source);
        }
        else if (target is NPCEntity npcTarget)
        {
            npcTarget.CurrentHealth = Math.Max(0, npcTarget.CurrentHealth - finalAmount);
            BroadcastDamage(source.EntityId, npcTarget.EntityId, finalAmount, damageType, isCrit, npcTarget.CurrentHealth,
                npcTarget.ZoneId, abilityId);
            if (npcTarget.CurrentHealth <= 0) HandleEntityDeath(npcTarget, source);
        }
    }

    private void ApplyHealing(PlayerEntity source, Entity target, int amount, bool isCrit, int abilityId)
    {
        if (target is PlayerEntity playerTarget)
        {
            playerTarget.CharacterData.Stats.CurrentHealth =
                Math.Min(playerTarget.CharacterData.Stats.MaxHealth,
                    playerTarget.CharacterData.Stats.CurrentHealth + amount);
            BroadcastHealing(source.EntityId, playerTarget.EntityId, abilityId, amount, isCrit,
                playerTarget.CharacterData.Stats.CurrentHealth, playerTarget.ZoneId);
        }
        else if (target is NPCEntity npcTarget)
        {
            npcTarget.CurrentHealth = Math.Min(npcTarget.MaxHealth, npcTarget.CurrentHealth + amount);
            BroadcastHealing(source.EntityId, npcTarget.EntityId, abilityId, amount, isCrit, npcTarget.CurrentHealth,
                npcTarget.ZoneId);
        }
    }

    private static int ApplyMitigation(Entity target, DamageType damageType, int rawAmount)
    {
        var reduction = 0f;

        if (target is PlayerEntity playerTarget)
        {
            reduction = damageType == DamageType.Physical
                ? CalculateArmorReduction(playerTarget.CharacterData.Stats)
                : playerTarget.CharacterData.Stats.Resistances.TryGetValue(damageType, out var res)
                    ? Math.Min(res, CombatConstants.MaxResistanceReduction)
                    : 0f;
        }

        return (int)Math.Max(0, rawAmount * (1 - reduction));
    }

    private static float CalculateArmorReduction(CharacterStats stats)
    {
        var armor = stats.Armor;
        var denominator = armor + CombatConstants.ArmorMitigationConstant;
        if (denominator <= 0) return 0f;

        var reduction = armor / denominator;
        return Math.Min(CombatConstants.MaxArmorReduction, reduction);
    }

    internal void HandleEntityDeath(Entity target, Entity? killer)
    {
        switch (target)
        {
            case PlayerEntity player:
                OnPlayerDeath(player, killer);
                break;
            case NPCEntity npc:
                OnNpcDeath(npc, killer);
                break;
            default:
                _worldSimulation.Entities.RemoveEntity(target.EntityId);
                break;
        }
    }

    private void OnPlayerDeath(PlayerEntity player, Entity? killer)
    {
        if (!_worldSimulation.Entities.RemoveEntity(player.EntityId)) return;

        const int respawnDelayMs = 3000;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(respawnDelayMs);
                if (!_isConnected) return;

                var zone = ZoneDefinitions.GetZone(player.ZoneId) ?? ZoneDefinitions.GetZone(ZoneConstants.Zone01);
                var spawn = zone?.SafeSpawnPoint ?? new WorldPosition(0, 0, 0);

                player.CharacterData.Stats.CurrentHealth = player.CharacterData.Stats.MaxHealth;
                player.CharacterData.Stats.CurrentMana = player.CharacterData.Stats.MaxMana;
                player.CharacterData.Stats.CurrentStamina = player.CharacterData.Stats.MaxStamina;
                player.CharacterData.Position = new CharacterPosition
                {
                    ZoneId = player.ZoneId,
                    X = spawn.X,
                    Y = spawn.Y,
                    Z = spawn.Z
                };
                player.Position = new Vector3(spawn.X, spawn.Y, spawn.Z);
                player.Velocity = new Vector3(0, 0, 0);
                player.MovementState = MovementState.Idle;
                player.IsInCombat = false;
                player.TargetEntityId = null;
                player.LastInputTime = DateTime.UtcNow;

                _worldSimulation.Entities.AddEntity(player);

                var abilitySummaries = player.KnownAbilities
                    .Select(id => AbilitySummary.From(AbilityBook.GetAbility(id)))
                    .ToList();

                SendPacket(MessagePackSerializer.Serialize<PacketBase>(new WorldPackets.PlayerSpawnPacket
                {
                    Character = CharacterSnapshot.FromCharacter(player.CharacterData, player.KnownAbilities.ToList()),
                    Resources = ResourceSnapshot.FromStats(player.CharacterData.Stats),
                    ZoneId = player.ZoneId,
                    ServerTime = _worldSimulation.GetServerTimestamp(),
                    Abilities = abilitySummaries
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to respawn player {PlayerId}", player.EntityId);
            }
        });
    }

    private void OnNpcDeath(NPCEntity npc, Entity? killer)
    {
        if (!_worldSimulation.Entities.RemoveEntity(npc.EntityId)) return;

        if (killer is PlayerEntity playerKiller)
        {
            var updates = _worldSimulation.Quests.RegisterKill(playerKiller, npc);
            foreach (var update in updates)
            {
                if (playerKiller.ClientConnection is ClientConnection killerConnection)
                    killerConnection.SendQuestUpdate(update.State, update.Definition);
            }
        }
    }

    private void BroadcastDamage(ulong sourceId, ulong targetId, int amount, DamageType damageType, bool isCrit,
        int remainingHealth, string zoneId, int abilityId)
    {
        var metadata = CombatEventMetadata.Create(_worldSimulation.GetServerTimestamp());
        var damage = new CombatPackets.DamagePacket
        {
            SourceEntityId = sourceId,
            TargetEntityId = targetId,
            AbilityId = abilityId,
            DamageType = damageType,
            Amount = amount,
            IsCritical = isCrit,
            RemainingHealth = remainingHealth,
            IsFatal = remainingHealth <= 0,
            Metadata = metadata
        };

        var combatEvent = new CombatPackets.CombatEventPacket
        {
            EventType = CombatEventType.Damage,
            Metadata = metadata,
            Damage = damage
        };

        BroadcastCombatEvent(combatEvent, zoneId);
    }

    private void BroadcastHealing(ulong sourceId, ulong targetId, int abilityId, int amount, bool isCrit,
        int remainingHealth, string zoneId)
    {
        var metadata = CombatEventMetadata.Create(_worldSimulation.GetServerTimestamp());
        var healing = new CombatPackets.HealingPacket
        {
            SourceEntityId = sourceId,
            TargetEntityId = targetId,
            AbilityId = abilityId,
            Amount = amount,
            IsCritical = isCrit,
            RemainingHealth = remainingHealth,
            Metadata = metadata
        };

        var combatEvent = new CombatPackets.CombatEventPacket
        {
            EventType = CombatEventType.Healing,
            Metadata = metadata,
            Healing = healing
        };

        BroadcastCombatEvent(combatEvent, zoneId);
    }

    private void BroadcastCombatEvent(CombatPackets.CombatEventPacket combatEvent, string zoneId)
    {
        _server.BroadcastToZone(zoneId, MessagePackSerializer.Serialize<PacketBase>(combatEvent));
    }

    private void SendAbilityResult(ResponseCode code, PlayerEntity caster, int abilityId, uint inputSequence,
        string message)
    {
        var result = new CombatPackets.AbilityResultPacket
        {
            Result = code,
            CasterEntityId = caster.EntityId,
            AbilityId = abilityId,
            InputSequence = inputSequence,
            Message = message
        };
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(result));
    }

    private static ClassArchetype ApplyClassArchetype(CharacterData character)
    {
        var archetype = ClassArchetypes.Get(character.Class);
        character.Stats = ClassArchetypes.BuildStatsForLevel(character.Class, character.Level);
        return archetype;
    }

    private static ulong GenerateCharacterId()
    {
        lock (CharacterIdLock)
        {
            return _nextCharacterId++;
        }
    }

    private static bool IsNameValid(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Length < GameConstants.MinNameLength || name.Length > GameConstants.MaxNameLength) return false;
        return name.All(c => GameConstants.NameAllowedCharacters.Contains(c));
    }

}
