# Eldara Architecture

## Overview

World of Eldara uses Unreal Engine 5 with a layered authority model that balances server authority, client responsiveness, and Blueprint flexibility. This document outlines the core architectural decisions and project structure.

## Layered Authority Model

### Server Authority
- **Combat resolution**: All damage calculation, ability validation, cooldown enforcement
- **Character creation**: Race/class/faction validation, stat assignment
- **World state**: NPC behavior, quest progression, zone events
- **Player data**: Inventory, equipment, progression persistence

### Client Authority
- **Visual presentation**: Effects, animations, UI state
- **Local prediction**: Movement input prediction for responsiveness
- **Camera control**: Player-controlled camera behavior

### Shared Validation
- **Input constraints**: Client and server validate input bounds
- **Line-of-sight**: Client checks before sending, server confirms
- **Cooldown display**: Client tracks for UI, server enforces

## Project Layout

```
Source/Eldara/
├── Core/                      # Core game framework
│   ├── EldaraGameInstance.*
│   ├── EldaraGameModeBase.*
│   └── EldaraPlayerController.*
├── Data/                      # Data asset definitions
│   ├── EldaraRaceData.h
│   ├── EldaraClassData.h
│   ├── EldaraFactionData.h
│   ├── EldaraQuestData.h
│   ├── EldaraZoneData.h
│   └── EldaraCharacterCreatePayload.h
├── Characters/                # Character and combat
│   ├── EldaraCharacterBase.*
│   ├── EldaraCombatComponent.*
│   ├── EldaraAbility.h
│   └── EldaraEffect.h
├── World/                     # World systems (future)
├── UI/                        # UI systems (future)
└── AI/                        # NPC AI
    ├── EldaraAIController.*
    └── EldaraAIKeys.h
```

## C++ vs Blueprint

### Use C++ For:
- **Core gameplay logic**: Combat, character creation, ability validation
- **Network replication**: RPCs, replicated properties, validation
- **Performance-critical systems**: AI behavior tree tasks, mass entity updates
- **Data structures**: Character stats, ability definitions, payload structs
- **Server authority**: All authoritative logic that must be secure

### Use Blueprint For:
- **Content iteration**: Ability visuals, particle effects, animation blueprints
- **UI implementation**: Menus, HUD elements, quest journals
- **Level design**: Zone layout, NPC placement, quest triggers
- **Designer-friendly tuning**: Damage values, cooldown timers, resource costs
- **Rapid prototyping**: Testing new mechanics before C++ implementation

### Integration Points:
- C++ exposes `BlueprintCallable` functions for designer access
- C++ defines `BlueprintImplementableEvent` for visual/content hooks
- Data Assets (C++ defined, BP instanced) bridge code and content
- C++ provides base classes, Blueprints derive for specific content

## Module Organization

### Core Module
- Entry point classes (GameInstance, GameMode, PlayerController)
- Singleton-pattern systems (quest manager, faction registry)
- Network setup and validation hooks

### Data Module
- Data asset definitions for designers
- Payload structs for network communication
- Validation rules and lore constraints

### Gameplay Module
- Character logic and combat components
- Ability and effect systems
- Player interaction systems

### AI Module
- AI controllers with behavior tree integration
- Perception and aggro systems
- Boss-specific logic and phase handlers

## Compilation Model

- **Modular compilation**: Each module compiles independently
- **Forward declarations**: Minimize header dependencies
- **Interface classes**: Loose coupling between modules
- **Data-driven**: Content changes don't require C++ recompilation

## Network Architecture

- **Replicated properties**: Character stats, health, resources
- **RPCs**: Client → Server for input, Server → Client for feedback
- **Validation**: Server validates all client requests
- **Prediction**: Client predicts movement, server reconciles

## Build Targets

- **Development**: Fast iteration, full debug symbols
- **DebugGame**: Optimized game code, debug editor
- **Shipping**: Full optimization, no debug tools

## Version Control Strategy

- C++ headers and implementation in version control
- Blueprint assets (.uasset) in version control with proper diffing
- Generated files (Intermediate/, Binaries/) in .gitignore
- Large assets managed via Git LFS

## Performance Considerations

- Minimize Blueprint overhead in hot paths (use C++)
- Use data assets for static data (loaded once, referenced many times)
- Profile before optimizing (Unreal Insights, Blueprint profiler)
- Batch operations where possible (mass entity updates)

## Security Model

- All authoritative logic in C++ (harder to modify)
- Server validates all client RPCs
- No client-side combat resolution
- Movement bounds checking and speed validation
- Ability cooldown and resource enforcement server-side

## Testing Strategy

- Unit tests for C++ utility functions
- Functional tests for RPC validation
- Integration tests for character creation flow
- Performance tests for mass entity simulation
- Manual QA for Blueprint-heavy content

## Documentation Standards

- C++ classes documented with Doxygen-style comments
- Blueprint functions use UE's `UFUNCTION` metadata for tooltips
- Architecture decisions captured in this document
- Gameplay systems detailed in docs/gameplay/
- AI behavior documented in docs/ai/

## Next Steps

See [docs/todo-next.md](todo-next.md) for prioritized backlog.
