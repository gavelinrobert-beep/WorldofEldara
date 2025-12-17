# World of Eldara - Quick Start Guide

This guide will get you up and running with the World of Eldara server in 5 minutes.

## Prerequisites

You need:
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - [Download here](https://git-scm.com/downloads)
- **Terminal/Command Prompt**

## Step 1: Clone the Repository

```bash
git clone https://github.com/gavelinrobert-beep/WorldofEldara.git
cd WorldofEldara
```

## Step 2: Verify .NET Installation

```bash
dotnet --version
```

You should see `8.0.x` or higher.

## Step 3: Build the Server

```bash
cd Server/WorldofEldara.Server
dotnet restore
dotnet build
```

If successful, you'll see:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Step 4: Run the Server

```bash
dotnet run
```

You should see startup logs:

```
===========================================
  World of Eldara - Authoritative Server
  The Worldroot is bleeding. Memory awaits.
===========================================
[INFO] Server Name: World of Eldara - Server 1
[INFO] Port: 7777
[INFO] Max Players: 5000
[INFO] Tick Rate: 20 TPS
[INFO] Loading zone definitions...
[INFO] Loaded 5 zones
[INFO] Spawn system initialized
[INFO] World time initialized: Day 0, 00:00 (Night)
[INFO] World simulation started
[INFO] Network server listening on port 7777
[INFO] Server started successfully. Press Ctrl+C to shutdown.
```

**Congratulations!** Your server is now running and ready to accept connections on port 7777.

## What's Running?

The server is now:
- ✅ Simulating the world at **20 TPS** (20 ticks per second)
- ✅ Accepting TCP connections on **port 7777**
- ✅ Managing game zones (Thornveil Enclave, Temporal Steppes, etc.)
- ✅ Tracking world time (Eldara day/night cycle)
- ✅ Ready to authenticate players
- ✅ Ready to spawn entities

## Configuration

Want to change settings? Edit `appsettings.json`:

```json
{
  "Server": {
    "Name": "World of Eldara - Server 1",
    "Port": 7777,              // Change port here
    "MaxPlayers": 5000,        // Max concurrent players
    "TickRate": 20             // Server updates per second
  },
  "World": {
    "DefaultZone": "zone_thornveil_enclave",
    "WorldrootStrainLevel": 0.5,
    "EnablePvP": true,
    "EnableGiantEvents": false  // Giant awakening events
  }
}
```

Restart the server after changes.

## Viewing Logs

Logs are written to:
- **Console**: Real-time output
- **File**: `Server/WorldofEldara.Server/Logs/server.log`

Log levels: Debug, Information, Warning, Error, Fatal

## Common Issues

### "dotnet: command not found"
Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### "Port 7777 already in use"
Change the port in `appsettings.json` or stop the process using port 7777:
```bash
# Windows
netstat -ano | findstr :7777
taskkill /PID [process_id] /F

# Linux/Mac
lsof -ti:7777 | xargs kill -9
```

### Build errors
Make sure you're in the correct directory:
```bash
cd Server/WorldofEldara.Server
```

Then try:
```bash
dotnet clean
dotnet restore
dotnet build
```

## Stopping the Server

Press **Ctrl+C** in the terminal. You'll see:

```
Shutdown signal received. Stopping server...
[INFO] Network server stopped.
[INFO] World simulation stopped.
[INFO] Shutdown complete.
```

The server performs graceful shutdown, saving any pending state.

## Next Steps

1. **Read the lore**: [`WORLD_LORE.md`](WORLD_LORE.md) - Understand the world
2. **Explore the code**: Check out the architecture in `Docs/Architecture/`
3. **Track Unreal client bring-up**: Client is in development
4. **Build your own client**: Use the protocol in `Shared/` library

## Testing Without a Client

Since the Unreal client isn't ready yet, you can:

### 1. Monitor Server Logs

Watch the console output:
```
[INFO] Tick 100: 0 entities, 5 zones loaded
[INFO] World time: Day 0, 01:12 (Night)
```

### 2. Use Telnet (Basic Connection Test)

```bash
telnet localhost 7777
```

If it connects, the server is accepting connections!

### 3. Build a Test Client

Use the `Shared/WorldofEldara.Shared` library to build a simple C# test client:

```csharp
using System.Net.Sockets;
using MessagePack;
using WorldofEldara.Shared.Protocol.Packets;

var client = new TcpClient("localhost", 7777);
var stream = client.GetStream();

// Send login packet
var loginRequest = new AuthPackets.LoginRequest
{
    Username = "TestUser",
    PasswordHash = "test", // In production, this would be SHA256
    ClientVersion = "1.0"
};

byte[] data = MessagePackSerializer.Serialize<PacketBase>(loginRequest);
byte[] lengthBytes = BitConverter.GetBytes(data.Length);

stream.Write(lengthBytes, 0, 4);
stream.Write(data, 0, data.Length);

// Read response
byte[] responseLengthBytes = new byte[4];
stream.Read(responseLengthBytes, 0, 4);
int responseLength = BitConverter.ToInt32(responseLengthBytes, 0);

byte[] responseData = new byte[responseLength];
stream.Read(responseData, 0, responseLength);

var response = MessagePackSerializer.Deserialize<PacketBase>(responseData);
Console.WriteLine($"Received: {response.GetType().Name}");
```

## Let a Friend Connect from Outside Your Network

1. Keep the server running (see steps above) and note the port you use in `appsettings.json` (default **7777**).
2. Allow inbound TCP traffic on that port in your OS firewall.
   - Windows: either allow the `dotnet` process when prompted **or** create an inbound port rule. Windows Security → Firewall & network protection → Advanced settings → Inbound Rules → New Rule → Port → TCP `<your-port>` (default 7777) → Allow.
   - Linux: open the same TCP port with your distro firewall. Ubuntu/Debian: `sudo ufw allow 7777/tcp`; other distros: `sudo iptables -A INPUT -p tcp --dport 7777 -j ACCEPT` (replace 7777 if you changed the port; persist with your distro's iptables-save/netfilter-persistent tools).
   - macOS: System Settings (or Security & Privacy) → Network → Firewall → Options → add an allow rule for the app or TCP port you configured.
3. If you're behind a home router, add a **port forwarding** rule for that TCP port → your machine's local IP (find it via `ipconfig` on Windows or `ip addr show` on Linux/macOS).
4. Give your friend your public IP (or DNS name) and port. They should connect to `your-public-ip:port` (for example `203.0.113.45:7777`) instead of `localhost`.
5. Verify from another device using `telnet 203.0.113.45 7777` (replace with your public IP/port) or the sample client above to confirm the server is reachable.
> Opening a port to the internet exposes your machine; if you prefer not to, consider connecting over a VPN/private network (e.g., Tailscale/ZeroTier) instead of public port forwarding.

## Need Help?

- Read the **README.md** for full overview
- Check **Docs/Architecture/** for technical details
- Review **WORLD_LORE.md** for game lore
- Check **PROJECT_STRUCTURE.md** for code organization

## Performance Monitoring

While the server runs, you can monitor:

**Every 5 seconds** (100 ticks at 20 TPS), you'll see:
```
[DEBUG] Tick 100: 0 entities, 5 zones loaded
[DEBUG] Tick 200: 0 entities, 5 zones loaded
```

This confirms the simulation is running smoothly.

## What's Next?

The server foundation is complete. Next up:
1. Unreal client development
2. Combat system implementation
3. NPC AI behaviors
4. Quest runtime integration
5. Database persistence

**Welcome to the World of Eldara. The Worldroot is bleeding. What will you preserve?**
