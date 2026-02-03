# Debug Logging Test Guide

## Overview
This guide explains how to test the debug logging changes added to the network receive path in `EldaraNetworkSubsystem.cpp`.

## Purpose
The C# server is successfully sending responses, but the Unreal client was not showing any logs for received data. These new logs will help identify exactly where in the receive chain the issue occurs.

## Changes Made
Added extensive debug logging to:
- `CheckForData()` - Main receive loop function (18 new log statements)
- `ProcessReceivedData()` - Packet processing function (2 new log statements)

## Testing Steps

### 1. Compile the Unreal Project
1. Open `Eldara.uproject` in Unreal Engine 5
2. Wait for compilation to complete (may take 5-10 minutes on first compile)
3. Verify no compilation errors appear

### 2. Start the C# Server
```bash
cd Server/WorldofEldara.Server
dotnet run
```

Wait until you see:
```
[INFO] Server started successfully. Press Ctrl+C to shutdown.
```

### 3. Play in Editor
1. In Unreal Editor, click "Play" button or press Alt+P
2. Open the Output Log window (Window → Developer Tools → Output Log)
3. Set log filter to "LogTemp" to see network logs
4. Perform in-game actions that trigger network communication

### 4. What to Look For

#### Success Case - Data is Being Received
If data is being received properly, you should see logs like:
```
LogTemp: EldaraNetworkSubsystem: HasPendingData returned TRUE - 72 bytes pending
LogTemp: EldaraNetworkSubsystem: Successfully read 72 bytes from socket
LogTemp: EldaraNetworkSubsystem: Appending 72 bytes to ReceiveBuffer (current size: 0)
LogTemp: EldaraNetworkSubsystem: Read length prefix - expecting 68 byte packet
LogTemp: EldaraNetworkSubsystem: Complete packet received (68 bytes), processing...
LogTemp: EldaraNetworkSubsystem: Calling ProcessReceivedData with 68 bytes
LogTemp: EldaraNetworkSubsystem: ProcessReceivedData called with 68 bytes
LogTemp: EldaraNetworkSubsystem: Received packet type 1, routing to deserializer...
LogTemp: PacketDeserializer: Deserialized LoginResponse - Result: 0, Message: Login successful
```

#### Diagnostic Scenarios

**Scenario 1: No "HasPendingData returned TRUE" logs**
- Issue: Socket is not detecting incoming data
- Possible causes:
  - Server not actually sending data
  - Network connectivity issue
  - Socket not in non-blocking mode
  - Connection not established

**Scenario 2: "HasPendingData" but no "Successfully read" log**
- Issue: Socket.Recv() is failing
- Look for: "Recv() returned false" warning log
- Possible causes:
  - Socket read error
  - Connection dropped
  - Buffer allocation issue

**Scenario 3: "Successfully read" but "BytesRead = 0"**
- Issue: Recv succeeded but returned no data
- Look for: "Recv succeeded but BytesRead = 0" warning log
- Possible causes:
  - Graceful disconnect from server
  - TCP zero-length packet

**Scenario 4: Data read but "Waiting for length prefix"**
- Issue: Not enough data to read 4-byte length prefix
- This is normal if packet arrives fragmented
- Should resolve when more data arrives

**Scenario 5: "Invalid packet size" error**
- Issue: Length prefix parsing returned invalid size
- Possible causes:
  - Endianness mismatch
  - Data corruption
  - Protocol mismatch between client and server

**Scenario 6: "Complete packet received" but fails in deserializer**
- Issue: Packet parsing logic problem
- Check PacketDeserializer logs for specific error

### 5. Enable Verbose Logging (Optional)

To see even more detailed logs, enable VeryVerbose logging:

In Unreal Editor:
1. Open Project Settings
2. Search for "Log Verbosity"
3. Set LogTemp verbosity to "VeryVerbose"

This will show:
- "CheckForData - checking for data..." every poll (60 times per second)
- "No pending data on socket" every second when idle
- Buffer state during packet processing
- "Waiting for length prefix" when assembling fragmented packets
- "Waiting for complete packet" when packet is split across multiple reads

⚠️ **Warning:** VeryVerbose logging creates a LOT of output and may impact performance. Only use for detailed debugging.

### 6. Log Levels Reference

| Level | When Used | Example |
|-------|-----------|---------|
| Log | Important events | "HasPendingData returned TRUE - 72 bytes pending" |
| Warning | Unexpected but recoverable | "Recv succeeded but BytesRead = 0" |
| Error | Fatal errors | "Invalid packet size: -1" |
| VeryVerbose | Routine operations | "CheckForData - checking for data..." |

## Expected Results

### If Everything Works
You should see a complete flow of logs from socket read → buffer append → length prefix read → packet extraction → deserialization.

### If Something Fails
The logs will stop at the exact point where the issue occurs, making it easy to identify:
- Network layer issues (socket operations)
- Buffer management issues (packet assembly)
- Protocol issues (packet parsing)
- Deserialization issues (MessagePack decoding)

## Troubleshooting

### No Logs at All
- Verify you're looking at the Output Log window in Unreal Editor
- Check that LogTemp filter is enabled
- Ensure you're connected to the server (check connection logs)
- Try sending a packet from client to trigger activity

### Too Many Logs
- Reduce verbosity from VeryVerbose to Log level
- Focus on specific log messages using search/filter
- Use "Clear" button in Output Log to start fresh

### Server Shows Sending But Client Shows Nothing
- This is the original issue - the new logs will help diagnose it
- Check which logs appear to identify where the receive pipeline breaks

## Next Steps After Testing

1. Capture the relevant log output
2. Report which logs appear and which don't
3. Based on results, we can pinpoint the exact issue:
   - Socket layer problem
   - Buffer management issue
   - Protocol mismatch
   - Deserialization problem

## Additional Notes

- The logging is designed to be minimal impact on performance
- VeryVerbose logs are throttled where appropriate (e.g., "No pending data" only once per second)
- All logs include "EldaraNetworkSubsystem:" prefix for easy filtering
- Byte counts are included in all relevant logs for data tracking
