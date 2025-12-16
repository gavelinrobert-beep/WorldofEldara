# World of Eldara - Project Structure

> Note: The client plan has pivoted to Unreal Engine. Unity-specific client details below are legacy placeholders and will be revised to match the Unreal client layout.

This document outlines the complete folder structure and architecture for the World of Eldara MMORPG.

## Root Structure

```
WorldofEldara/
├── Client/                 # Unity client project
├── Server/                 # C# dedicated server
├── Shared/                 # Shared code between client and server
├── Docs/                   # Documentation
├── Tools/                  # Development tools and utilities
├── WORLD_LORE.md          # Canonical lore document
├── PROJECT_STRUCTURE.md   # This file
└── README.md              # Project overview
```

## Client/ - Unity Project

```
Client/
├── Assets/
│   ├── _Project/                      # Main project assets
│   │   ├── Scripts/
│   │   │   ├── Core/                  # Core client systems
│   │   │   │   ├── GameManager.cs
│   │   │   │   ├── SceneManager.cs
│   │   │   │   └── InputManager.cs
│   │   │   ├── Networking/            # Client networking
│   │   │   │   ├── NetworkClient.cs
│   │   │   │   ├── ServerConnection.cs
│   │   │   │   └── PacketHandler.cs
│   │   │   ├── Character/             # Character systems
│   │   │   │   ├── CharacterController.cs
│   │   │   │   ├── CharacterCreation.cs
│   │   │   │   ├── PlayerStats.cs
│   │   │   │   └── EquipmentManager.cs
│   │   │   ├── Movement/              # Movement and camera
│   │   │   │   ├── PlayerMovement.cs
│   │   │   │   ├── CameraController.cs
│   │   │   │   └── ClientPrediction.cs
│   │   │   ├── Combat/                # Client-side combat
│   │   │   │   ├── AbilitySystem.cs
│   │   │   │   ├── CombatUI.cs
│   │   │   │   └── DamageDisplay.cs
│   │   │   ├── UI/                    # User interface
│   │   │   │   ├── ActionBar.cs
│   │   │   │   ├── CharacterFrame.cs
│   │   │   │   ├── TargetFrame.cs
│   │   │   │   ├── ChatWindow.cs
│   │   │   │   ├── QuestTracker.cs
│   │   │   │   └── InventoryUI.cs
│   │   │   ├── NPCs/                  # Client NPC systems
│   │   │   │   ├── NPCController.cs
│   │   │   │   └── NPCInteraction.cs
│   │   │   ├── Quest/                 # Client quest systems
│   │   │   │   ├── QuestManager.cs
│   │   │   │   └── QuestUI.cs
│   │   │   └── VFX/                   # Visual effects
│   │   │       ├── SpellEffects.cs
│   │   │       └── CombatVFX.cs
│   │   ├── Prefabs/                   # Unity prefabs
│   │   │   ├── Characters/
│   │   │   │   ├── Player/
│   │   │   │   └── NPCs/
│   │   │   ├── UI/
│   │   │   └── VFX/
│   │   ├── Scenes/                    # Unity scenes
│   │   │   ├── MainMenu.unity
│   │   │   ├── CharacterCreation.unity
│   │   │   ├── DawnshoreVale.unity    # Covenant starter
│   │   │   ├── WildrootForest.unity   # Dominion starter
│   │   │   ├── ShadowfenMarshes.unity # Compact starter
│   │   │   └── CrossroadsLowlands.unity # Free Realms starter
│   │   ├── Materials/                 # Materials and shaders
│   │   ├── Textures/                  # Texture assets
│   │   ├── Models/                    # 3D models
│   │   ├── Animations/                # Animation clips
│   │   └── Audio/                     # Sound effects and music
│   └── Plugins/                       # Third-party plugins
├── ProjectSettings/                   # Unity project settings
└── Packages/                          # Unity package manager
```

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
- **Engine**: Unity 2022.3 LTS
- **Language**: C# 10
- **Rendering**: URP (Universal Render Pipeline)
- **Input**: New Input System
- **UI**: Unity UI Toolkit

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

- **Ignore**: Unity Library, Temp, Obj, Bin folders
- **LFS**: Large assets (models, textures, audio)
- **Branches**: feature/, bugfix/, release/

## Next Steps

1. Initialize Unity project in Client/
2. Create .NET solution in Server/
3. Create shared library project
4. Set up version control properly
5. Implement core networking layer
