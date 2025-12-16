# World of Eldara - Network Protocol Specification

## Overview

The World of Eldara uses a custom binary protocol over TCP with MessagePack serialization for efficient, type-safe communication between client and server.

## Protocol Principles

1. **Server Authority**: Server is the source of truth. Client sends requests, server sends updates.
2. **Binary Protocol**: MessagePack serialization for compact binary messages
3. **Type Safety**: Strongly typed packet classes shared between client and server
4. **Reliable Delivery**: TCP ensures packets arrive in order
5. **Client Prediction**: Client predicts movement locally, server reconciles

## Connection Flow

```
┌────────┐                                    ┌────────┐
│ Client │                                    │ Server │
└────┬───┘                                    └───┬────┘
     │                                            │
     ├──── TCP Connect ──────────────────────────►│
     │                                            │
     ├──── LoginRequest ─────────────────────────►│
     │      (username, password hash)             ├─ Authenticate
     │◄──── LoginResponse ────────────────────────┤
     │      (success, account ID, token)          │
     │                                            │
     ├──── CharacterListRequest ─────────────────►│
     │◄──── CharacterListResponse ────────────────┤
     │      (list of characters)                  │
     │                                            │
     ├──── CreateCharacterRequest (optional) ────►│
     │      (name, race, class, appearance)       ├─ Validate lore
     │◄──── CreateCharacterResponse ──────────────┤
     │                                            │
     ├──── SelectCharacterRequest ───────────────►│
     │      (character ID)                        ├─ Load character
     │◄──── SelectCharacterResponse ──────────────┤  ├─ Spawn entity
     │◄──── EnterWorldPacket ─────────────────────┤  └─ in world
     │      (character data, zone, server time)   │
     │                                            │
     │         [Game Session Active]              │
     │                                            │
     ├──── MovementInput (continuous) ───────────►│
     │◄──── MovementUpdate (broadcast) ───────────┤
     │◄──── EntitySpawn/Despawn ──────────────────┤
     │                                            │
     ├──── UseAbilityRequest ────────────────────►│
     │◄──── AbilityResult ─────────────────────────┤
     │◄──── DamagePacket (broadcast) ─────────────┤
     │                                            │
     ├──── ChatMessage ──────────────────────────►│
     │◄──── ChatMessage (broadcast) ──────────────┤
     │                                            │
     ├──── Disconnect ───────────────────────────►│
     │                                            ├─ Save state
     │                                            └─ Remove entity
     │                                            
```

## Packet Format

### Wire Protocol

Every packet on the wire consists of:

```
┌──────────────┬────────────────────────────────┐
│ Length (4B)  │   MessagePack Data (N bytes)   │
└──────────────┴────────────────────────────────┘
```

**Length**: 32-bit integer (little-endian), number of bytes in MessagePack data
**Data**: MessagePack-serialized packet object

### Packet Structure

All packets inherit from `PacketBase`:

```csharp
public abstract class PacketBase
{
    public long Timestamp { get; set; }
    public uint SequenceNumber { get; set; }
}
```

## Packet Categories

### Authentication (0-9)

#### LoginRequest (0)
**Direction**: Client → Server

```csharp
{
    "Username": string,
    "PasswordHash": string,  // Client-side SHA256 hash
    "ClientVersion": string
}
```

#### LoginResponse (1)
**Direction**: Server → Client

```csharp
{
    "Result": ResponseCode,
    "Message": string,
    "AccountId": ulong,
    "SessionToken": string
}
```

### Character Management (2-9)

#### CharacterListRequest (2)
**Direction**: Client → Server

```csharp
{
    "AccountId": ulong
}
```

#### CharacterListResponse (3)
**Direction**: Server → Client

```csharp
{
    "Result": ResponseCode,
    "Characters": CharacterData[]
}
```

#### CreateCharacterRequest (4)
**Direction**: Client → Server

```csharp
{
    "AccountId": ulong,
    "Name": string,
    "Race": Race,
    "Class": Class,
    "Faction": Faction,
    "TotemSpirit": TotemSpirit?,  // Required if Therakai
    "Appearance": CharacterAppearance
}
```

**Lore Validation**:
- Race-class combination must be valid per `ClassInfo.IsClassAvailableForRace()`
- Faction-race combination must be valid per `FactionInfo.IsRaceAvailableForFaction()`
- Therakai must have TotemSpirit set

#### CreateCharacterResponse (5)
**Direction**: Server → Client

```csharp
{
    "Result": ResponseCode,
    "Message": string,
    "Character": CharacterData?
}
```

**Response Codes**:
- `Success`: Character created
- `NameTaken`: Name already in use
- `InvalidName`: Name violates rules
- `InvalidRaceClassCombination`: Lore violation
- `MaxCharactersReached`: Account limit hit

#### SelectCharacterRequest (6)
**Direction**: Client → Server

```csharp
{
    "CharacterId": ulong
}
```

#### SelectCharacterResponse (7)
**Direction**: Server → Client

```csharp
{
    "Result": ResponseCode,
    "Message": string,
    "Character": CharacterData?
}
```

### Movement (10-19)

#### MovementInputPacket (10)
**Direction**: Client → Server
**Frequency**: ~20 per second (client tick rate)

```csharp
{
    "InputSequence": uint,        // Client-side sequence number
    "DeltaTime": float,           // Time since last input
    "Input": {
        "Forward": float,         // -1.0 to 1.0
        "Strafe": float,          // -1.0 to 1.0
        "Jump": bool,
        "Sprint": bool,
        "LookYaw": float,         // Camera rotation
        "LookPitch": float
    },
    "PredictedPosition": Vector3, // Client's predicted position
    "PredictedRotationYaw": float
}
```

**Client Prediction**:
1. Client applies input locally immediately
2. Client predicts new position
3. Client sends input to server with predicted position
4. Client stores this input in history buffer

#### MovementUpdatePacket (11)
**Direction**: Server → All Clients in Zone
**Frequency**: 20 per second (server tick rate)

```csharp
{
    "EntityId": ulong,
    "Position": Vector3,
    "Velocity": Vector3,
    "RotationYaw": float,
    "RotationPitch": float,
    "State": MovementState,
    "ServerTimestamp": long
}
```

**Movement States**: Idle, Walking, Running, Jumping, Falling, Swimming, Flying, Mounted, Stunned, Rooted

#### PositionCorrectionPacket (12)
**Direction**: Server → Client
**Frequency**: Only when correction needed

```csharp
{
    "LastProcessedInput": uint,           // Which input sequence this is for
    "AuthoritativePosition": Vector3,     // Server's true position
    "AuthoritativeVelocity": Vector3,
    "AuthoritativeRotationYaw": float,
    "ServerTimestamp": long
}
```

**Server Reconciliation**:
1. Server processes client input
2. Server calculates authoritative position
3. If client prediction error > threshold (e.g., 1 meter):
   - Server sends PositionCorrectionPacket
   - Client rewinds to that input sequence
   - Client replays all inputs from that point forward

### Combat (20-29)

#### UseAbilityRequest (20)
**Direction**: Client → Server

```csharp
{
    "AbilityId": int,
    "TargetEntityId": ulong?,          // For single-target abilities
    "TargetPosition": Vector3?,        // For ground-targeted abilities
    "InputSequence": uint              // For prediction
}
```

#### AbilityResultPacket (21)
**Direction**: Server → Client

```csharp
{
    "Result": ResponseCode,
    "CasterEntityId": ulong,
    "AbilityId": int,
    "InputSequence": uint,
    "Message": string                  // Error message if failed
}
```

**Response Codes**:
- `Success`: Ability cast successfully
- `NotInRange`: Target too far
- `NotEnoughMana`: Insufficient resource
- `OnCooldown`: Ability not ready
- `Interrupted`: Cast was interrupted
- `InvalidTarget`: Can't target that entity

#### DamagePacket (22)
**Direction**: Server → All Clients in Zone

```csharp
{
    "SourceEntityId": ulong,
    "TargetEntityId": ulong,
    "AbilityId": int,
    "DamageType": DamageType,
    "Amount": int,
    "IsCritical": bool,
    "RemainingHealth": int,
    "IsFatal": bool
}
```

**Damage Types** (lore-based):
- Physical, Nature, Radiant, Holy, Necrotic, Arcane
- Fire, Frost, Lightning
- Void, Shadow, Spirit, Temporal

#### HealingPacket (23)
**Direction**: Server → All Clients in Zone

```csharp
{
    "SourceEntityId": ulong,
    "TargetEntityId": ulong,
    "AbilityId": int,
    "Amount": int,
    "IsCritical": bool,
    "RemainingHealth": int
}
```

#### StatusEffectPacket (24)
**Direction**: Server → All Clients in Zone

```csharp
{
    "TargetEntityId": ulong,
    "Effect": StatusEffectData,
    "Applied": bool,               // true = applied, false = removed
    "StackCount": int
}
```

### Chat (30-39)

#### ChatMessagePacket (30)
**Direction**: Bidirectional

```csharp
{
    "Channel": ChatChannel,
    "SenderEntityId": ulong,
    "SenderName": string,
    "Message": string,
    "TargetEntityId": ulong?,      // For whispers
    "Timestamp": long
}
```

**Chat Channels**:
- Say (local area)
- Yell (wider area)
- Whisper (private)
- Party (group)
- Guild
- Faction (lore-specific: Verdant Circles, Ascendant League, etc.)
- System (server messages)
- Combat (combat log)
- Emote

### World (100-199)

#### EnterWorldPacket (100)
**Direction**: Server → Client

```csharp
{
    "Character": CharacterData,
    "ZoneId": string,
    "ServerTime": long
}
```

**Sent after successful character selection. Client loads zone and begins gameplay.**

#### LeaveWorldPacket (101)
**Direction**: Bidirectional

```csharp
{
    "EntityId": ulong,
    "Reason": string
}
```

#### EntitySpawnPacket (102)
**Direction**: Server → Client

```csharp
{
    "EntityId": ulong,
    "Type": EntityType,            // Player, NPC, Monster, Object, etc.
    "Name": string,
    "Position": Vector3,
    "RotationYaw": float,
    "CharacterData": CharacterData?,  // If player
    "NPCData": NPCData?               // If NPC
}
```

**Sent when an entity enters client's awareness range.**

#### EntityDespawnPacket (103)
**Direction**: Server → Client

```csharp
{
    "EntityId": ulong
}
```

**Sent when an entity leaves client's awareness range or is destroyed.**

## Interest Management

To reduce network bandwidth, the server only sends updates to clients for entities they can "see":

**Awareness Range**: 100 meters (configurable)
**Zone-based**: Only entities in same zone
**Priority System**:
1. Player-controlled entities (highest)
2. Combat targets
3. Quest NPCs
4. Nearby NPCs
5. Distant objects (lowest)

## Performance Optimizations

### Delta Compression (Future)
Only send changed properties, not full entity state.

### Priority Queuing
Critical packets (combat, movement corrections) sent before cosmetic updates.

### Batching
Multiple small updates batched into single packet when possible.

### Throttling
Non-critical updates sent at reduced rate (e.g., 5 TPS for distant entities).

## Error Handling

### Network Errors
- **Connection Lost**: Client attempts reconnect with session token
- **Timeout**: Server disconnects client after 30s of no heartbeat
- **Malformed Packet**: Logged, client warned, connection closed if repeated

### Lore Violations
- Invalid race-class combinations rejected at character creation
- Faction-specific areas blocked at runtime
- Magic source mismatches logged as errors

### Response Codes

```csharp
public enum ResponseCode : byte
{
    Success = 0,
    Error = 1,
    InvalidRequest = 2,
    NotAuthenticated = 3,
    AlreadyExists = 4,
    NotFound = 5,
    InsufficientPermissions = 6,
    InvalidData = 7,
    ServerError = 8,
    Timeout = 9,
    
    // Character creation
    NameTaken = 10,
    InvalidName = 11,
    InvalidRaceClassCombination = 12,
    MaxCharactersReached = 13,
    
    // Combat
    NotInRange = 20,
    NotEnoughMana = 21,
    OnCooldown = 22,
    Interrupted = 23,
    InvalidTarget = 24,
    
    // Lore-specific
    LoreInconsistency = 100
}
```

## Security Considerations

### Authentication
- Passwords hashed client-side (SHA256) before transmission
- Server validates against stored hash (bcrypt recommended)
- Session tokens for reconnection

### Validation
- **All** client input validated server-side
- Movement bounds checking
- Ability range checking
- Resource availability (mana, cooldowns)
- Lore rules enforced

### Rate Limiting
- Max packets per second per client
- Ability usage rate limits
- Chat flood protection

### Anti-Cheat
- Server authoritative - client cannot directly modify game state
- Movement validation (speed, teleportation detection)
- Ability validation (cooldowns, resources, range)
- Position history for rollback if needed

## Monitoring and Debugging

### Packet Logging
Enable in `appsettings.json`:

```json
{
  "Logging": {
    "PacketLogging": true,
    "PacketTypes": ["Movement", "Combat"]
  }
}
```

### Network Statistics
Server tracks per-client:
- Bytes sent/received
- Packets sent/received
- Average latency
- Packet loss rate

### Packet Sniffer Tool
`Tools/PacketSniffer` can decode and display packets for debugging.

---

**Protocol Version**: 1.0  
**Last Updated**: 2024

**This protocol is designed for a serious PC MMORPG, not casual/mobile gaming. Treat it accordingly.**
