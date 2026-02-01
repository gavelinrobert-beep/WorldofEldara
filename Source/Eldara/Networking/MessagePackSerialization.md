# MessagePack Serialization for WorldofEldara

## Overview

This document describes the MessagePack serialization implementation for the Unreal Engine client to communicate with the C# server.

## Wire Format

The C# server uses `MessagePack-CSharp` with `[Union]` attributes. The wire format for a packet is:

```
[ UnionKey, [ Field0, Field1, ... ] ]
```

### Example: LoginRequest

LoginRequest has Union Key 0. The serialized format is:

```
[ 0, [ Timestamp, SequenceNumber, Username, PasswordHash, ClientVersion, ProtocolVersion ] ]
```

## MessagePack Format Reference

### Primitive Types

- **Positive fixint**: `0x00` - `0x7f` (values 0-127)
- **Negative fixint**: `0xe0` - `0xff` (values -32 to -1)
- **int8**: `0xd0` + 1 byte
- **int16**: `0xd1` + 2 bytes (big-endian)
- **int32**: `0xd2` + 4 bytes (big-endian)
- **int64**: `0xd3` + 8 bytes (big-endian)

### Strings

- **fixstr**: `0xa0` - `0xbf` (length 0-31 bytes)
- **str8**: `0xd9` + uint8 length + data
- **str16**: `0xda` + uint16 length + data
- **str32**: `0xdb` + uint32 length + data

### Arrays

- **fixarray**: `0x90` - `0x9f` (0-15 elements)
- **array16**: `0xdc` + uint16 count + elements
- **array32**: `0xdd` + uint32 count + elements

### Float

- **float32**: `0xca` + 4 bytes (big-endian IEEE 754)
- **float64**: `0xcb` + 8 bytes (big-endian IEEE 754)

### Boolean

- **false**: `0xc2`
- **true**: `0xc3`

## Implementation

### FPacketSerializer Class

The `FPacketSerializer` class provides:

1. **Templated Serialize method**: Uses compile-time type detection for optimal performance
2. **MessagePack primitive writers**: Static helper methods for writing basic types
3. **Packet-specific serializers**: Custom serialization logic for each packet type

### Current Implementation Status

- ✅ LoginRequest (Union Key 0)
- ⏳ Other packet types (to be implemented as needed)

### Usage Example

```cpp
// Create a LoginRequest packet
FLoginRequest LoginRequest;
LoginRequest.Username = TEXT("PlayerName");
LoginRequest.PasswordHash = TEXT("HashedPassword");
LoginRequest.ClientVersion = TEXT("1.0.0");
LoginRequest.ProtocolVersion = TEXT("1.0");

// Send the packet (automatically serializes using MessagePack)
if (UNetworkSubsystem* NetSubsystem = GetGameInstance()->GetSubsystem<UNetworkSubsystem>())
{
    NetSubsystem->SendPacket(LoginRequest);
}
```

## Adding New Packet Types

To add serialization support for a new packet type:

1. Add the packet struct to `NetworkPackets.h` (if not already present)
2. Verify the Union Key is defined in `NetworkTypes.h` (in `EPacketType` enum)
   - Union Keys are the numeric values in the enum (e.g., `LoginRequest = 0`)
   - These correspond to the C# server's Union attribute keys
3. Implement a `Serialize[PacketName]` private method in `PacketSerializer.cpp`
4. Add a `constexpr if` branch in the templated `Serialize` method in `PacketSerializer.h`

### Example:

```cpp
// In PacketSerializer.h - Add to the template Serialize method:
else if constexpr (TIsSame<T, FCharacterListRequest>::Value)
{
    SerializeCharacterListRequest(static_cast<const FCharacterListRequest&>(Packet), OutBytes);
    return true;
}

// In PacketSerializer.cpp - Implement the serialization:
void FPacketSerializer::SerializeCharacterListRequest(const FCharacterListRequest& Packet, TArray<uint8>& OutBytes)
{
    // Write outer array [UnionKey, FieldArray]
    WriteArrayHeader(OutBytes, 2);
    
    // Write union key (2 for CharacterListRequest)
    WriteInt(OutBytes, 2);
    
    // Write field array
    WriteArrayHeader(OutBytes, 3); // Timestamp, SequenceNumber, AccountId
    WriteInt64(OutBytes, Packet.Timestamp);
    WriteInt(OutBytes, Packet.SequenceNumber);
    WriteInt64(OutBytes, Packet.AccountId);
}
```

## Testing

To test the serialization:

1. Connect to a C# server
2. Send a LoginRequest packet
3. Verify the server receives and can deserialize the packet
4. Check the server logs for any deserialization errors

## Notes

- All integers are serialized in big-endian byte order (network byte order)
- Strings are encoded as UTF-8
- The implementation uses the most compact MessagePack representation for each value type
