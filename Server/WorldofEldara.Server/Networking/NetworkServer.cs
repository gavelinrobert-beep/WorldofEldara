using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Serilog;
using WorldofEldara.Server.Core;
using WorldofEldara.Server.World;
using WorldofEldara.Shared.Protocol.Packets;
using MessagePack;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol;
using ProtocolEntityType = WorldofEldara.Shared.Protocol.Packets.EntityType;

namespace WorldofEldara.Server.Networking;

/// <summary>
///     TCP-based network server for client connections.
///     Handles:
///     - Client connections and disconnections
///     - Packet routing
///     - Heartbeat/keepalive
///     - Message serialization
/// </summary>
public class NetworkServer
{
    private readonly object _connectionIdLock = new();
    private readonly ConcurrentDictionary<ulong, ClientConnection> _connections = new();
    private readonly int _maxPlayers;
    private readonly int _port;
    private readonly WorldSimulation _worldSimulation;
    private Thread? _acceptThread;
    private bool _isRunning;

    private TcpListener? _listener;

    private ulong _nextConnectionId = 1;

    public NetworkServer(int port, int maxPlayers, WorldSimulation worldSimulation)
    {
        _port = port;
        _maxPlayers = maxPlayers;
        _worldSimulation = worldSimulation;
    }

    public async Task Initialize()
    {
        Log.Information("Network server initialized");
        _worldSimulation.Entities.EntityAdded += OnEntityAdded;
        _worldSimulation.Entities.EntityRemoved += OnEntityRemoved;
        await Task.CompletedTask;
    }

    public async Task Start()
    {
        if (_isRunning)
        {
            Log.Warning("Network server already running");
            return;
        }

        try
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Log.Information($"Network server listening on port {_port}");

            // Start accept thread
            _acceptThread = new Thread(AcceptLoop)
            {
                Name = "NetworkAccept",
                IsBackground = true
            };
            _acceptThread.Start();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start network server");
            throw;
        }
    }

    public async Task Stop()
    {
        if (!_isRunning) return;

        Log.Information("Stopping network server...");
        _isRunning = false;

        // Stop listener
        _listener?.Stop();

        // Disconnect all clients
        foreach (var connection in _connections.Values) connection.Disconnect("Server shutting down");

        _connections.Clear();

        // Wait for accept thread
        _acceptThread?.Join(TimeSpan.FromSeconds(5));

        Log.Information("Network server stopped");
        await Task.CompletedTask;
    }

    private void AcceptLoop()
    {
        Log.Information("Accept loop started");

        while (_isRunning)
            try
            {
                if (_listener == null) break;

                // Check for pending connections
                if (_listener.Pending())
                {
                    var tcpClient = _listener.AcceptTcpClient();
                    HandleNewConnection(tcpClient);
                }
                else
                {
                    Thread.Sleep(10); // Small delay to prevent CPU spinning
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) // Only log if we're still supposed to be running
                    Log.Error(ex, "Error in accept loop");
            }

        Log.Information("Accept loop ended");
    }

    private void HandleNewConnection(TcpClient tcpClient)
    {
        // Check player limit
        if (_connections.Count >= _maxPlayers)
        {
            Log.Warning($"Connection refused from {tcpClient.Client.RemoteEndPoint} - server full");
            tcpClient.Close();
            return;
        }

        ulong connectionId;
        lock (_connectionIdLock)
        {
            connectionId = _nextConnectionId++;
        }

        var connection = new ClientConnection(connectionId, tcpClient, this, _worldSimulation);
        _connections.TryAdd(connectionId, connection);

        Log.Information($"New connection [{connectionId}] from {tcpClient.Client.RemoteEndPoint}");

        // Start receiving data from this client
        connection.Start();
    }

    /// <summary>
    ///     Called by ClientConnection when it disconnects
    /// </summary>
    internal void OnClientDisconnected(ulong connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            Log.Information($"Connection [{connectionId}] disconnected");

            // Remove player entity if exists
            if (connection.PlayerEntityId.HasValue)
                _worldSimulation.Entities.RemoveEntity(connection.PlayerEntityId.Value);
        }
    }

    /// <summary>
    ///     Broadcast a packet to all connected clients
    /// </summary>
    public void BroadcastPacket(byte[] packetData)
    {
        foreach (var connection in _connections.Values) connection.SendPacket(packetData);
    }

    /// <summary>
    ///     Broadcast to all clients in a zone
    /// </summary>
    public void BroadcastToZone(string zoneId, byte[] packetData, ulong? excludeConnectionId = null)
    {
        foreach (var connection in _connections.Values)
        {
            if (connection.ConnectionId == excludeConnectionId)
                continue;

            if (connection.CurrentZoneId == zoneId) connection.SendPacket(packetData);
        }
    }

    /// <summary>
    ///     Send packet to specific connection
    /// </summary>
    public void SendToConnection(ulong connectionId, byte[] packetData)
    {
        if (_connections.TryGetValue(connectionId, out var connection)) connection.SendPacket(packetData);
    }

    public int GetConnectionCount()
    {
        return _connections.Count;
    }

    private void OnEntityAdded(Entity entity)
    {
        try
        {
            var spawnPacket = new WorldPackets.EntitySpawnPacket
            {
                EntityId = entity.EntityId,
                Type = (ProtocolEntityType)(int)entity.GetEntityType(),
                Name = entity.Name,
                Position = entity.Position,
                RotationYaw = entity.RotationYaw
            };

            if (entity is PlayerEntity player)
            {
                spawnPacket.CharacterData = player.CharacterData;
                spawnPacket.Resources = ResourceSnapshot.FromStats(player.CharacterData.Stats);
                spawnPacket.AbilityIds = player.KnownAbilities.ToList();
            }
            else if (entity is NPCEntity npc)
            {
                spawnPacket.NPCData = new NPCData
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
            }

            BroadcastToZone(entity.ZoneId, MessagePackSerializer.Serialize<PacketBase>(spawnPacket));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to broadcast entity spawn");
        }
    }

    private void OnEntityRemoved(Entity entity)
    {
        try
        {
            var despawn = new WorldPackets.EntityDespawnPacket { EntityId = entity.EntityId };
            BroadcastToZone(entity.ZoneId, MessagePackSerializer.Serialize<PacketBase>(despawn));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to broadcast entity despawn");
        }
    }
}
