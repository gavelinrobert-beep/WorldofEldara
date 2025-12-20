using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using MessagePack;
using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Server.World;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;
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

        var response = new CharacterPackets.CharacterListResponse
        {
            Result = ResponseCode.Success,
            Characters = new List<CharacterData>()
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));
    }

    private void HandleCreateCharacter(CharacterPackets.CreateCharacterRequest request)
    {
        Log.Information(
            $"Create character request from [{ConnectionId}]: {request.Name} ({request.Race}, {request.Class})");

        // Validate lore consistency
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

        // Create character
        var character = new CharacterData
        {
            CharacterId = 10000 + ConnectionId, // Fake ID
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
        var starterZone = FactionInfo.GetStarterZone(character.Faction);
        character.Position = new CharacterPosition
        {
            ZoneId = starterZone,
            X = 0,
            Y = 0,
            Z = 0
        };

        ApplyClassArchetype(character);

        // TODO: Save to database

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

        // TODO: Load character from database
        // For now, create fake character

        var character = new CharacterData
        {
            CharacterId = request.CharacterId,
            AccountId = AccountId ?? 0,
            Name = "TestCharacter",
            Race = Race.Human,
            Class = Class.UnboundWarrior,
            Faction = Faction.UnitedKingdoms,
            Level = 1,
            Position = new CharacterPosition
            {
                ZoneId = "zone_borderkeep",
                X = 0,
                Y = 0,
                Z = 0
            }
        };

        ApplyClassArchetype(character);

        // Create player entity in world
        var playerEntity = new PlayerEntity
        {
            EntityId = _worldSimulation.Entities.GenerateEntityId(),
            AccountId = AccountId ?? 0,
            CharacterData = character,
            Name = character.Name,
            ZoneId = character.Position.ZoneId,
            Position = new Vector3(character.Position.X, character.Position.Y, character.Position.Z),
            ClientConnection = this
        };

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

        // TODO: Process movement input with server reconciliation
        // For now, just update position directly (not production-ready)

        player.LastProcessedInputSequence = packet.InputSequence;
        player.Position = packet.PredictedPosition;
        player.RotationYaw = packet.PredictedRotationYaw;
        player.LastInputTime = DateTime.UtcNow;

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

        if (CurrentZoneId != null)
            _server.BroadcastToZone(CurrentZoneId, MessagePackSerializer.Serialize<PacketBase>(movementUpdate),
                ConnectionId);
    }

    private void HandleUseAbility(CombatPackets.UseAbilityRequest packet)
    {
        if (!PlayerEntityId.HasValue) return;

        Log.Debug($"Player [{ConnectionId}] using ability {packet.AbilityId}");

        var player = _worldSimulation.Entities.GetEntity(PlayerEntityId.Value) as PlayerEntity;
        if (player == null) return;

        Ability ability;
        try
        {
            ability = AbilityBook.GetAbility(packet.AbilityId);
        }
        catch (KeyNotFoundException)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CombatPackets.AbilityResultPacket
            {
                Result = ResponseCode.InvalidRequest,
                CasterEntityId = player.EntityId,
                AbilityId = packet.AbilityId,
                InputSequence = packet.InputSequence,
                Message = "Unknown ability"
            }));
            return;
        }

        var target = packet.TargetEntityId.HasValue
            ? _worldSimulation.Entities.GetEntity(packet.TargetEntityId.Value)
            : null;

        if (target == null)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CombatPackets.AbilityResultPacket
            {
                Result = ResponseCode.InvalidTarget,
                CasterEntityId = player.EntityId,
                AbilityId = packet.AbilityId,
                InputSequence = packet.InputSequence,
                Message = "No valid target"
            }));
            return;
        }

        var distance = Distance(player.Position, target.Position);
        if (distance > ability.Range)
        {
            SendPacket(MessagePackSerializer.Serialize<PacketBase>(new CombatPackets.AbilityResultPacket
            {
                Result = ResponseCode.NotInRange,
                CasterEntityId = player.EntityId,
                AbilityId = packet.AbilityId,
                InputSequence = packet.InputSequence,
                Message = "Target out of range"
            }));
            return;
        }

        var isCrit = Random.Shared.NextDouble() < player.CharacterData.Stats.CriticalChance;
        var damage = ability.CalculateValue(player.CharacterData.Stats, isCrit);

        ApplyDamage(target, damage, ability.DamageType, isCrit, ability.AbilityId);

        var result = new CombatPackets.AbilityResultPacket
        {
            Result = ResponseCode.Success,
            CasterEntityId = player.EntityId,
            AbilityId = packet.AbilityId,
            InputSequence = packet.InputSequence,
            Message = "Ability executed"
        };
        SendPacket(MessagePackSerializer.Serialize<PacketBase>(result));
    }

    private void HandleChatMessage(ChatPackets.ChatMessagePacket packet)
    {
        if (!PlayerEntityId.HasValue) return;

        Log.Information($"Chat from [{ConnectionId}] ({packet.Channel}): {packet.Message}");

        // Broadcast to zone
        if (CurrentZoneId != null)
            _server.BroadcastToZone(CurrentZoneId, MessagePackSerializer.Serialize<PacketBase>(packet));
    }

    private static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private void ApplyDamage(Entity target, int amount, DamageType damageType, bool isCrit, int abilityId)
    {
        if (target is PlayerEntity playerTarget)
        {
            playerTarget.CharacterData.Stats.CurrentHealth =
                Math.Max(0, playerTarget.CharacterData.Stats.CurrentHealth - amount);

            BroadcastDamage(playerTarget.EntityId, amount, damageType, isCrit,
                playerTarget.CharacterData.Stats.CurrentHealth, playerTarget.ZoneId, abilityId);

            if (playerTarget.CharacterData.Stats.CurrentHealth <= 0)
            {
                _worldSimulation.Entities.RemoveEntity(playerTarget.EntityId);
            }
        }
        else if (target is NPCEntity npc)
        {
            npc.CurrentHealth = Math.Max(0, npc.CurrentHealth - amount);
            BroadcastDamage(npc.EntityId, amount, damageType, isCrit, npc.CurrentHealth, npc.ZoneId, abilityId);

            if (npc.CurrentHealth <= 0)
            {
                _worldSimulation.Entities.RemoveEntity(npc.EntityId);
            }
        }
    }

    private void BroadcastDamage(ulong targetId, int amount, DamageType damageType, bool isCrit, int remainingHealth,
        string zoneId, int abilityId)
    {
        var packet = new CombatPackets.DamagePacket
        {
            SourceEntityId = PlayerEntityId ?? 0,
            TargetEntityId = targetId,
            AbilityId = abilityId,
            DamageType = damageType,
            Amount = amount,
            IsCritical = isCrit,
            RemainingHealth = remainingHealth,
            IsFatal = remainingHealth <= 0
        };

        _server.BroadcastToZone(zoneId, MessagePackSerializer.Serialize<PacketBase>(packet));
    }

    private static void ApplyClassArchetype(CharacterData character)
    {
        var archetype = ClassArchetypes.Get(ClassArchetypes.MapToArchetype(character.Class));
        character.Stats = CloneStats(archetype.BaseStats);
    }

    private static CharacterStats CloneStats(CharacterStats source)
    {
        return new CharacterStats
        {
            Strength = source.Strength,
            Agility = source.Agility,
            Intellect = source.Intellect,
            Stamina = source.Stamina,
            Willpower = source.Willpower,
            MaxHealth = source.MaxHealth,
            CurrentHealth = source.CurrentHealth,
            MaxMana = source.MaxMana,
            CurrentMana = source.CurrentMana,
            AttackPower = source.AttackPower,
            SpellPower = source.SpellPower,
            CriticalChance = source.CriticalChance,
            CriticalDamage = source.CriticalDamage,
            Armor = source.Armor,
            Resistances = new Dictionary<DamageType, float>(source.Resistances),
            MovementSpeed = source.MovementSpeed
        };
    }
}
