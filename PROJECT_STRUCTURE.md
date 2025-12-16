# World of Eldara - Project Structure

> Client plan: Unreal Engine. The layout below reflects the Unreal project structure alongside the authoritative .NET server.

This document outlines the complete folder structure and architecture for the World of Eldara MMORPG.

## Root Structure

```
WorldofEldara/
├── Client/                 # Unreal Engine project
├── Server/                 # C# dedicated server
├── Shared/                 # Shared code between client and server
├── Docs/                   # Documentation
├── Tools/                  # Development tools and utilities
├── WORLD_LORE.md          # Canonical lore document
├── PROJECT_STRUCTURE.md   # This file
└── README.md              # Project overview
```

## Client/ - Unreal Engine Project

```
Client/
├── Config/                             # DefaultEngine, DefaultInput, etc.
├── Content/
│   └── WorldofEldara/                  # Art, UI, quests, maps
│       ├── Blueprints/                 # Core, UI, Characters, NPC, Quests
│       ├── Data/                       # DataAssets/DataTables/Curves
│       ├── Maps/                       # MainMenu, CharacterCreation, Zone prototypes
│       └── UI/                         # UMG/CommonUI widgets (chat, quest tracker, HUD)
├── Source/
│   └── Eldara/
│       ├── Core/                       # GameInstance, Subsystems, PlayerController
│       ├── Networking/                 # Socket client, packet dispatch
│       ├── Movement/                   # Prediction/reconciliation hooks
│       ├── Combat/                     # GAS abilities, cooldown UX
│       ├── Quest/                      # Quest DataAssets, objective logic stubs
│       ├── World/                      # Zone presentation, corruption visuals
│       ├── UI/                         # Widget controllers
│       └── Plugins/                    # Optional Unreal plugins (e.g., ALS/GAS helpers)
└── Eldara.uproject
```

- **Config/** drives engine, input, and rendering defaults (DX12/Vulkan, Enhanced Input mappings).
- **Source/Eldara** is modular: Core handles bootstrap/state, Networking mirrors MessagePack schema, Movement wires prediction/reconciliation, Combat uses GAS, Quest holds DataAssets/objectives/conditions, World owns corruption/world-state presentation, UI coordinates UMG/CommonUI widgets.
- **Content/WorldofEldara** stores authored assets (Blueprints, Data tables/curves, Maps, UI). Saved/DerivedDataCache are generated and ignored by version control.

## Server/ - Dedicated Game Server

```
Server/
├── WorldofEldara.Server/
│   ├── Core/                          # Core server systems
│   │   ├── ServerBootstrap.cs         # Server startup
│   │   ├── WorldSimulation.cs         # Main game loop
│   │   ├── EntityManager.cs           # Entity management
│   │   └── ServerConfig.cs            # Configuration
│   ├── Networking/                    # Server networking
│   │   ├── NetworkServer.cs
│   │   ├── ClientConnection.cs
│   │   ├── PacketProcessor.cs
│   │   └── NetworkProtocol.cs
│   ├── Character/                     # Character systems
│   │   ├── CharacterData.cs
│   │   ├── CharacterManager.cs
│   │   ├── StatsCalculator.cs
│   │   └── EquipmentSystem.cs
│   ├── Combat/                        # Combat systems
│   │   ├── CombatSystem.cs
│   │   ├── AbilityManager.cs
│   │   ├── DamageCalculator.cs
│   │   ├── ThreatManager.cs
│   │   └── CooldownSystem.cs
│   ├── AI/                            # NPC AI
│   │   ├── BehaviorTree/
│   │   ├── NPCBrain.cs
│   │   ├── CombatAI.cs
│   │   └── PatrolSystem.cs
│   ├── Quest/                         # Quest systems
│   │   ├── QuestSystem.cs
│   │   ├── QuestData.cs
│   │   └── ReputationManager.cs
│   ├── World/                         # World management
│   │   ├── ZoneManager.cs
│   │   ├── SpawnSystem.cs
│   │   ├── WorldState.cs
│   │   └── TimeManager.cs
│   ├── Database/                      # Persistence layer
│   │   ├── DatabaseManager.cs
│   │   ├── PlayerRepository.cs
│   │   └── WorldRepository.cs
│   └── Program.cs                     # Entry point
├── WorldofEldara.Server.csproj
└── appsettings.json                   # Server configuration
```

## Shared/ - Shared Code

```
Shared/
├── WorldofEldara.Shared/
│   ├── Protocol/                      # Network protocol
│   │   ├── Packets/
│   │   │   ├── AuthPackets.cs
│   │   │   ├── CharacterPackets.cs
│   │   │   ├── MovementPackets.cs
│   │   │   ├── CombatPackets.cs
│   │   │   └── ChatPackets.cs
│   │   ├── PacketBase.cs
│   │   └── PacketSerializer.cs
│   ├── Data/                          # Shared data structures
│   │   ├── Character/
│   │   │   ├── CharacterData.cs
│   │   │   ├── Race.cs
│   │   │   ├── Class.cs
│   │   │   └── Stats.cs
│   │   ├── Combat/
│   │   │   ├── Ability.cs
│   │   │   ├── DamageType.cs
│   │   │   └── StatusEffect.cs
│   │   ├── Items/
│   │   │   ├── Item.cs
│   │   │   ├── Equipment.cs
│   │   │   └── Inventory.cs
│   │   ├── Quest/
│   │   │   ├── Quest.cs
│   │   │   └── QuestObjective.cs
│   │   └── World/
│   │       ├── Zone.cs
│   │       ├── Faction.cs
│   │       └── WorldPosition.cs
│   ├── Constants/                     # Game constants
│   │   ├── GameConstants.cs
│   │   ├── NetworkConstants.cs
│   │   └── CombatConstants.cs
│   └── Utils/                         # Shared utilities
│       ├── MathUtils.cs
│       └── Validation.cs
└── WorldofEldara.Shared.csproj
```

## Docs/ - Documentation

```
Docs/
├── Architecture/
│   ├── NetworkProtocol.md             # Network protocol specification
│   ├── CombatSystem.md                # Combat system design
│   ├── CharacterSystem.md             # Character system design
│   └── ServerArchitecture.md          # Server architecture
├── Design/
│   ├── ClassDesign.md                 # Class specifications
│   ├── RaceDesign.md                  # Race specifications
│   ├── ZoneDesign.md                  # Zone layouts and design
│   └── QuestDesign.md                 # Quest design guidelines
└── API/
    ├── NetworkAPI.md                  # Network API documentation
    └── ServerAPI.md                   # Server API documentation
```

## Tools/ - Development Tools

```
Tools/
├── DataEditor/                        # Game data editor
├── MapEditor/                         # World map editor
└── PacketSniffer/                     # Network debugging tool
```

## Technology Stack

### Client
- **Engine**: Unreal Engine 5.x
- **Language**: C++20 + Blueprints
- **Rendering**: Lumen/Nanite (project dependent)
- **Input**: Enhanced Input
- **UI**: UMG/CommonUI

### Server
- **Framework**: .NET 8
- **Language**: C# 12
- **Networking**: Raw TCP/UDP sockets
- **Database**: PostgreSQL (future)
- **Logging**: Serilog

### Shared
- **Serialization**: MessagePack or Protocol Buffers
- **Compression**: LZ4

## Architecture Principles

1. **Authoritative Server**: All game logic runs on server, client is display only
2. **Client Prediction**: Movement and actions predicted locally, confirmed by server
3. **Data-Driven Design**: Game content defined in data files, not code
4. **Modular Systems**: Each system is independent and communicates via events
5. **Lore-First**: All design decisions must align with Eldara lore

## Build Targets

- **Client**: Windows 64-bit (primary), Linux (future), macOS (future)
- **Server**: Linux 64-bit (production), Windows (development)

## Version Control

- **Ignore**: Unreal DerivedDataCache, Intermediate, Saved, Binaries
- **LFS**: Large assets (models, textures, audio)
- **Branches**: feature/, bugfix/, release/

## Next Steps

1. Initialize Unreal project in Client/
2. Create .NET solution in Server/
3. Create shared library project
4. Set up version control properly
5. Implement core networking layer
