# World of Eldara - Server Architecture

## Overview

The World of Eldara server is an authoritative game server built on .NET 8, designed to handle thousands of concurrent players in a persistent MMORPG world grounded in deep lore.

## Core Principles

1. **Server Authority**: All gameplay logic runs on server. Client is display only with prediction.
2. **Fixed Tick Rate**: World simulates at 20 TPS (50ms per tick) for deterministic gameplay.
3. **Lore-First Design**: All systems enforce canonical lore rules (e.g., race-class restrictions).
4. **Thread Safety**: Entity management uses concurrent data structures for multi-threaded access.
5. **Scalability**: Designed to eventually support server clustering and zone instancing.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Program.cs                            │
│                    (Entry Point + Logging)                   │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                     ServerBootstrap                          │
│  • Load Configuration                                        │
│  • Initialize World Simulation                               │
│  • Initialize Network Server                                 │
│  • Coordinate Startup/Shutdown                               │
└───────────┬──────────────────────────┬──────────────────────┘
            │                          │
            ▼                          ▼
┌──────────────────────┐    ┌──────────────────────────────────┐
│  WorldSimulation     │    │      NetworkServer               │
│  (20 TPS Loop)       │    │      (TCP Listener)              │
│                      │    │                                  │
│  • EntityManager     │◄───│  • ClientConnection (per player) │
│  • ZoneManager       │    │  • Packet Routing                │
│  • SpawnSystem       │    │  • Broadcast Messages            │
│  • TimeManager       │    │                                  │
│  • CombatSystem      │    └──────────────────────────────────┘
│  • QuestSystem       │
│  • EventSystem       │
└──────────────────────┘
```

## Component Breakdown

### ServerBootstrap
**Responsibility**: Initialization and lifecycle management

- Loads `appsettings.json` configuration
- Initializes all subsystems in correct order
- Handles graceful shutdown
- Coordinates between network and simulation threads

### WorldSimulation
**Responsibility**: Core game loop and world state

**Tick Rate**: 20 TPS (50ms per tick)
**Thread**: Dedicated simulation thread with fixed timestep

**Per-Tick Operations**:
1. Update world time (day/night cycle, Worldroot strain)
2. Update all entities (players, NPCs, objects)
3. Process combat (damage, healing, status effects)
4. Update AI (NPC behaviors, pathing)
5. Handle spawns/respawns
6. Process world events (Giant stirrings, Void rifts, Memory echoes)

**Key Properties**:
- `CurrentTick`: Monotonically increasing tick counter
- `Entities`: Reference to EntityManager
- `Zones`: Reference to ZoneManager

### EntityManager
**Responsibility**: Manage all game entities

**Thread Safety**: Uses `ConcurrentDictionary` for safe access from network and simulation threads.

**Entity Types**:
- `PlayerEntity`: Player-controlled characters
- `NPCEntity`: Server-controlled NPCs/monsters
- `ObjectEntity`: Interactive world objects (future)

**Key Operations**:
- `AddEntity(entity)`: Spawn entity in world
- `RemoveEntity(entityId)`: Despawn entity
- `GetEntity(entityId)`: Retrieve entity by ID
- `GetEntitiesInZone(zoneId)`: Spatial query
- `GetEntitiesInRange(position, range)`: Radius query
- `UpdateAll(deltaTime)`: Per-frame entity updates

### ZoneManager
**Responsibility**: Manage world zones

**Zones** are defined in `Shared/Data/World/Zone.cs` and loaded on startup.

**Key Properties**:
- Zone metadata (name, level range, type)
- Controlling faction
- Worldroot density (lore-specific)
- PvP rules
- Connected zones

**Future**: Zone instancing for dungeons/raids.

### SpawnSystem
**Responsibility**: Entity spawning and respawning

**Spawn Points**: Define NPC/monster spawn locations
**Respawn Logic**: Timed respawn after entity death
**Leashing**: NPCs return to spawn if pulled too far

### TimeManager
**Responsibility**: World time simulation

**Lore**: Time in Eldara flows according to the Worldroot's perception.
**Time Scale**: 1 real day = 7 Eldara days (3.43 hours per Eldara day)

**Features**:
- Day/night cycle
- Time-of-day queries
- Worldroot strain calculation (increases over time, lore driver)
- Seasonal events (future)

### NetworkServer
**Responsibility**: Client connections and packet distribution

**Protocol**: TCP for reliable delivery
**Serialization**: MessagePack for binary serialization

**Key Operations**:
- Accept new client connections
- Route packets to handlers
- Broadcast messages to zones
- Send unicast messages to specific clients
- Handle disconnections

### ClientConnection
**Responsibility**: Per-player network state and packet handling

**One instance per connected player**

**State**:
- `ConnectionId`: Unique connection identifier
- `AccountId`: Authenticated account (after login)
- `PlayerEntityId`: Entity ID in world (after character selection)
- `CurrentZoneId`: Current zone for spatial messaging

**Packet Handlers**:
- `HandleLogin`: Authenticate player
- `HandleCharacterListRequest`: Send character list
- `HandleCreateCharacter`: Validate and create character (with lore checks)
- `HandleSelectCharacter`: Enter world with character
- `HandleMovementInput`: Process movement with reconciliation
- `HandleUseAbility`: Trigger combat abilities
- `HandleChatMessage`: Process chat messages

## Data Flow Examples

### Player Login and World Entry

```
Client                    NetworkServer              ClientConnection           WorldSimulation
   │                           │                           │                          │
   ├─── LoginRequest ─────────►│                           │                          │
   │                           ├──────────────────────────►│                          │
   │                           │                           ├─ Authenticate            │
   │                           │                           ├─ Set AccountId           │
   │◄─── LoginResponse ────────┼───────────────────────────┤                          │
   │                           │                           │                          │
   ├─── CharacterListReq ─────►│                           │                          │
   │◄─── CharacterListResp ────┼───────────────────────────┤ (Load from DB)          │
   │                           │                           │                          │
   ├─── SelectCharacter ──────►│                           │                          │
   │                           │                           ├─ Create PlayerEntity ───►│
   │                           │                           │                          ├─ AddEntity
   │◄─── SelectResponse ───────┼───────────────────────────┤                          │
   │◄─── EnterWorld ───────────┼───────────────────────────┤                          │
   │                           │                           │                          │
```

### Movement with Client Prediction

```
Client                    NetworkServer              ClientConnection           WorldSimulation
   │                           │                           │                          │
   ├─ Predict Movement        │                           │                          │
   ├─ Update Local Position   │                           │                          │
   ├─── MovementInput ────────►│                           │                          │
   │    (sequence #N)          │                           │                          │
   │                           ├──────────────────────────►│                          │
   │                           │                           ├─ Store Input            │
   │                           │                           ├─ Update Position ───────►│
   │                           │                           │                          ├─ Validate
   │                           │                           │                          ├─ Simulate
   │                           │                           │                          │
   │                           │◄───── Broadcast Movement Update (all clients) ───────┤
   │◄──────────────────────────┤                           │                          │
   │                           │                           │                          │
   │ (If large error)          │                           │                          │
   │◄─ PositionCorrection ─────┼───────────────────────────┤                          │
   │    (authoritative pos)    │                           │                          │
   ├─ Rewind and Replay       │                           │                          │
```

### Combat Flow

```
Client                    NetworkServer              ClientConnection           WorldSimulation
   │                           │                           │                          │
   ├─── UseAbilityRequest ────►│                           │                          │
   │    (target ID)            │                           │                          │
   │                           ├──────────────────────────►│                          │
   │                           │                           ├─ Validate Range ────────►│
   │                           │                           │                          ├─ Check Mana
   │                           │                           │                          ├─ Check Cooldown
   │                           │                           │                          ├─ Calculate Damage
   │                           │                           │                          ├─ Apply Damage
   │                           │                           │                          ├─ Check Threat
   │                           │                           │                          │
   │◄─── AbilityResult ────────┼───────────────────────────┤◄─────────────────────────┤
   │                           │                           │                          │
   │◄─── DamagePacket ─────────┼─────────── Broadcast to Zone ◄──────────────────────┤
   │    (all clients see)      │                           │                          │
```

## Performance Considerations

### Target Metrics
- **Players per Server**: 5,000 concurrent
- **Tick Rate**: 20 TPS (50ms) stable
- **Network Latency**: Handle up to 200ms client latency
- **Memory**: ~100MB per 1,000 players
- **CPU**: Single core at 40% for simulation, scale network threads

### Optimizations
1. **Spatial Partitioning**: Only send updates to players in same zone
2. **Interest Management**: Only send entity updates for visible entities
3. **Delta Compression**: Send only changed data (future)
4. **Priority Queuing**: Combat updates before cosmetic updates
5. **Connection Throttling**: Limit messages per client per second

## Scalability Path

### Phase 1: Single Server (Current)
- One server handles entire world
- 5,000 concurrent players target

### Phase 2: Zone-based Sharding (Future)
- Each zone runs on separate server process
- Players transition between zone servers
- Shared player database

### Phase 3: Mega-Server (Future)
- Dynamic zone instancing
- Load balancing across servers
- Cross-server communication
- 50,000+ concurrent players

## Lore Integration

The server enforces canonical lore at every level:

### Character Creation
- Race-class restrictions validated
- Faction-race restrictions validated
- Totem spirits required for Therakai
- Starter zones determined by faction

### World State
- Worldroot density affects magic power
- Time flows according to lore (Worldroot perception)
- Giant events can change world state permanently
- Memory echoes create temporal anomalies

### Combat
- Magic sources validated (Worldroot, Divine, Arcane, Primal, etc.)
- Damage types based on lore magic systems
- God-fragment powers for Gronnak
- Phase-shift mechanics for Void-Touched

## Configuration

Server behavior controlled by `appsettings.json`:

```json
{
  "Server": {
    "Port": 7777,
    "MaxPlayers": 5000,
    "TickRate": 20
  },
  "World": {
    "WorldrootStrainLevel": 0.5,
    "EnablePvP": true,
    "EnableGiantEvents": false
  }
}
```

## Monitoring and Logging

**Logging Framework**: Serilog
**Log Levels**: Debug, Information, Warning, Error, Fatal

**Key Metrics Logged**:
- Tick performance (every 100 ticks)
- Player connections/disconnections
- Lore validation failures
- Combat events
- World events

## Future Enhancements

1. **Database Persistence**: PostgreSQL for player data
2. **Redis Caching**: Session state and hot data
3. **AI System**: Behavior trees for complex NPC behaviors
4. **Quest System**: Full quest chains with world state
5. **Guild System**: Player organizations
6. **Raid System**: 40-player coordinated content
7. **World Events**: Giant awakenings, Void rifts, Memory collapses
8. **PvP Battlegrounds**: Structured PvP content
9. **Economy System**: Trading, auction house
10. **Crafting System**: Material gathering and item creation

## Testing Strategy

### Unit Tests
- Entity management logic
- Combat calculations
- Lore validation rules
- Time calculations

### Integration Tests
- Client-server communication
- World simulation stability
- Multi-player scenarios

### Load Tests
- 5,000 simulated players
- Tick rate stability under load
- Network bandwidth usage
- Memory leaks

### Lore Compliance Tests
- Invalid race-class combinations rejected
- Worldroot density affects gameplay
- Faction hostility rules enforced

---

**This is a living document. Update as architecture evolves.**
