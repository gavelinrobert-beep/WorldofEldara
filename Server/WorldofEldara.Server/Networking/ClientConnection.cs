using Serilog;
using System.Net.Sockets;
using MessagePack;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;
using WorldofEldara.Server.World;
using WorldofEldara.Server.Core;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Server.Networking;

/// <summary>
/// Represents a single client connection.
/// Handles packet sending/receiving and game state for this client.
/// </summary>
public class ClientConnection
{
    public ulong ConnectionId { get; }
    public ulong? AccountId { get; private set; }
    public ulong? PlayerEntityId { get; private set; }
    public string? CurrentZoneId { get; private set; }

    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;
    private readonly NetworkServer _server;
    private readonly WorldSimulation _worldSimulation;

    private bool _isConnected = true;
    private Thread? _receiveThread;
    private readonly Queue<byte[]> _sendQueue = new();
    private readonly object _sendLock = new();

    public ClientConnection(ulong connectionId, TcpClient tcpClient, NetworkServer server, WorldSimulation worldSimulation)
    {
        ConnectionId = connectionId;
        _tcpClient = tcpClient;
        _stream = tcpClient.GetStream();
        _server = server;
        _worldSimulation = worldSimulation;
    }

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
                    byte[] lengthBytes = BitConverter.GetBytes(data.Length);
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
        byte[] lengthBuffer = new byte[4];

        while (_isConnected)
        {
            try
            {
                // Read packet length
                int bytesRead = _stream.Read(lengthBuffer, 0, 4);
                if (bytesRead != 4)
                {
                    Disconnect("Invalid packet length");
                    break;
                }

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Validate length
                if (packetLength <= 0 || packetLength > NetworkConstants.MaxPacketSize)
                {
                    Disconnect($"Invalid packet size: {packetLength}");
                    break;
                }

                // Read packet data
                byte[] packetData = new byte[packetLength];
                int totalRead = 0;
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

                if (totalRead != packetLength)
                {
                    break;
                }

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
        Log.Information($"Create character request from [{ConnectionId}]: {request.Name} ({request.Race}, {request.Class})");

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
        string starterZone = FactionInfo.GetStarterZone(character.Faction);
        character.Position = new CharacterPosition
        {
            ZoneId = starterZone,
            X = 0,
            Y = 0,
            Z = 0
        };

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

        // Create player entity in world
        var playerEntity = new PlayerEntity
        {
            EntityId = _worldSimulation.Entities.GenerateEntityId(),
            AccountId = AccountId ?? 0,
            CharacterData = character,
            Name = character.Name,
            ZoneId = character.Position.ZoneId,
            Position = new Shared.Protocol.Packets.Vector3(character.Position.X, character.Position.Y, character.Position.Z),
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
    }

    private void HandleUseAbility(CombatPackets.UseAbilityRequest packet)
    {
        if (!PlayerEntityId.HasValue) return;

        Log.Debug($"Player [{ConnectionId}] using ability {packet.AbilityId}");

        // TODO: Implement combat system

        var response = new CombatPackets.AbilityResultPacket
        {
            Result = ResponseCode.NotInRange,
            CasterEntityId = PlayerEntityId.Value,
            AbilityId = packet.AbilityId,
            InputSequence = packet.InputSequence,
            Message = "Combat system not yet implemented"
        };

        SendPacket(MessagePackSerializer.Serialize<PacketBase>(response));
    }

    private void HandleChatMessage(ChatPackets.ChatMessagePacket packet)
    {
        if (!PlayerEntityId.HasValue) return;

        Log.Information($"Chat from [{ConnectionId}] ({packet.Channel}): {packet.Message}");

        // Broadcast to zone
        if (CurrentZoneId != null)
        {
            _server.BroadcastToZone(CurrentZoneId, MessagePackSerializer.Serialize<PacketBase>(packet));
        }
    }
}
