using System.Net.Sockets;
using MessagePack;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Client.Networking;

public sealed class EldaraServerClient : IDisposable
{
    private readonly CancellationTokenSource _shutdown = new();
    private TcpClient? _client;
    private NetworkStream? _stream;
    private ulong _accountId;
    private uint _movementSequence;
    private uint _actionSequence;

    public event EventHandler<string>? StatusChanged;
    public event EventHandler<PacketBase>? PacketReceived;

    public ulong AccountId => _accountId;
    public bool IsConnected => _client?.Connected == true && _stream is not null;

    public async Task ConnectAndLoginAsync(string host, int port, string username)
    {
        if (_client?.Connected == true)
        {
            PublishStatus("Already connected.");
            return;
        }

        try
        {
            PublishStatus($"Connecting to {host}:{port}...");
            _client = new TcpClient();
            await _client.ConnectAsync(host, port, _shutdown.Token);
            _stream = _client.GetStream();
            PublishStatus("Connected. Sending login...");

            _ = Task.Run(() => ReadLoopAsync(_shutdown.Token));

            await SendPacketAsync(new AuthPackets.LoginRequest
            {
                Username = username,
                PasswordHash = "local-prototype",
                ClientVersion = "custom-client-0.1",
                ProtocolVersion = ProtocolVersions.Current
            });
        }
        catch (Exception ex)
        {
            PublishStatus($"Connection failed: {ex.Message}");
            Disconnect();
        }
    }

    public async Task SendPacketAsync(PacketBase packet)
    {
        if (_stream == null)
        {
            PublishStatus("Cannot send packet: not connected.");
            return;
        }

        var payload = MessagePackSerializer.Serialize<PacketBase>(packet);
        var length = BitConverter.GetBytes(payload.Length);
        await _stream.WriteAsync(length, _shutdown.Token);
        await _stream.WriteAsync(payload, _shutdown.Token);
        await _stream.FlushAsync(_shutdown.Token);
    }

    public Task SendMovementInputAsync(MovementPackets.MovementInputPacket packet)
    {
        packet.InputSequence = ++_movementSequence;
        return SendPacketAsync(packet);
    }

    public Task SendUseAbilityAsync(CombatPackets.UseAbilityRequest packet)
    {
        packet.InputSequence = ++_actionSequence;
        return SendPacketAsync(packet);
    }

    public void Dispose()
    {
        _shutdown.Cancel();
        Disconnect();
        _shutdown.Dispose();
    }

    private async Task ReadLoopAsync(CancellationToken cancellationToken)
    {
        if (_stream == null) return;

        var lengthBuffer = new byte[4];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ReadExactAsync(_stream, lengthBuffer, cancellationToken);
                var length = BitConverter.ToInt32(lengthBuffer);
                if (length <= 0 || length > NetworkConstants.MaxPacketSize)
                {
                    PublishStatus($"Invalid packet size: {length}");
                    Disconnect();
                    return;
                }

                var payload = new byte[length];
                await ReadExactAsync(_stream, payload, cancellationToken);
                var packet = MessagePackSerializer.Deserialize<PacketBase>(payload, cancellationToken: cancellationToken);
                await HandleProtocolAutomationAsync(packet);
                PacketReceived?.Invoke(this, packet);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            PublishStatus($"Disconnected: {ex.Message}");
            Disconnect();
        }
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            if (read == 0) throw new IOException("Remote host closed the connection.");
            offset += read;
        }
    }

    private void Disconnect()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }

    private void PublishStatus(string status)
    {
        StatusChanged?.Invoke(this, status);
    }

    private async Task HandleProtocolAutomationAsync(PacketBase packet)
    {
        switch (packet)
        {
            case AuthPackets.LoginResponse login when login.Result == ResponseCode.Success:
                _accountId = login.AccountId;
                PublishStatus("Logged in. Requesting characters...");
                await SendPacketAsync(new CharacterPackets.CharacterListRequest { AccountId = _accountId });
                break;

            case CharacterPackets.CharacterListResponse list when list.Result == ResponseCode.Success:
                PublishStatus(list.Characters.Count > 0
                    ? "Character list ready. Choose a character."
                    : "No characters yet. Create a prototype character.");
                break;

            case CharacterPackets.CreateCharacterResponse create when create.Result == ResponseCode.Success &&
                                                                 create.Character != null:
                PublishStatus($"Created {create.Character.Name}. Refreshing character list...");
                await SendPacketAsync(new CharacterPackets.CharacterListRequest { AccountId = _accountId });
                break;

            case CharacterPackets.SelectCharacterResponse select when select.Result != ResponseCode.Success:
                PublishStatus($"Select failed: {select.Message}");
                break;
        }
    }
}
