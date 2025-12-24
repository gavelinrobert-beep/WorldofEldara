# World of Eldara - PC-Based Fantasy MMORPG

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Unreal Engine](https://img.shields.io/badge/Unreal_Engine-5.x-black?logo=unrealengine&logoColor=white)](https://www.unrealengine.com/)
[![License](https://img.shields.io/badge/license-Proprietary-red)](LICENSE)

**A serious, lore-driven MMORPG set in the World of Eldara.**

> *"The Worldroot is bleeding. Memory awaits. What will you preserve?"*

## 🌍 What is World of Eldara?

World of Eldara is a **PC-based fantasy MMORPG** where ancient cosmic forces, divine fragments, and mortal ambition collide. Built on deep, immutable lore, every game system—from character classes to combat mechanics—is grounded in the world's narrative structure.

This is **not a browser game, not mobile, not casual**. This is a desktop PC MMORPG in the tradition of World of Warcraft, Guild Wars, and Elder Scrolls Online.

### Core Pillars

- **Lore-First Design**: All systems respect canonical world truth
- **Server-Authoritative**: Secure, fair, competitive gameplay
- **Epic Scale**: Designed for thousands of concurrent players
- **Player Agency**: Choices have permanent consequences
- **Cosmic Stakes**: The world is failing—players decide what comes next

## 📖 The World

Eldara is not a natural world. It is a **containment engine** built by beings called the Architects to resist **the Null**—an entropic force that erases meaning itself.

At the core lies the **Worldroot**: a planetary nervous system, memory archive, and reality stabilizer. But the Worldroot is under strain. Memory is overflowing. Giants are stirring. Gods are fragmenting.

**Players are the unrecorded variable**—entities not fully bound by the Worldroot's deterministic patterns. Their actions create statistical noise that neither gods nor fate can predict.

👉 **Read the full lore**: [`WORLD_LORE.md`](WORLD_LORE.md)

## 🎮 Key Features

### Playable Races (Lore-Grounded)

- **Sylvaen** (Wood Elves): Born from Worldroot consciousness, feel ecological imbalance
- **High Elves**: Exiled reality-hackers who broke death itself
- **Humans**: The unrecorded anomaly—origin unknown, unpredictable
- **Therakai** (Beastkin): Descended from a shattered god's choice-fragments
- **Gronnak** (Orcs): God-eaters living atop a reality scar
- **Void-Touched**: Phase-shifted survivors of Void exposure

### Class System (12 Classes)

Each class represents a philosophical stance on Eldara's crisis:

- **Memory Warden** (Sylvaen Tank): Preserve what remains
- **Temporal Mage** (High Elf DPS): Transcend current limits
- **Unbound Warrior** (Human Hybrid): Adapt and survive
- **Totem Shaman** (Therakai Healer): Balance instinct with wisdom
- **Berserker** (Therakai DPS): Embrace pure fury
- **Witch-King Acolyte** (Gronnak DPS): Dominate through power
- **Void Stalker** (Void-Touched Stealth): Escape the system
- ...and more

👉 **Full class breakdown**: [`WORLD_LORE.md#classes`](WORLD_LORE.md#classes-lore-justified-archetypes)

### Magic Systems

All magic has a **source and consequence**:

- **Worldroot Magic**: Nature, growth, life (draws from ecosystem)
- **Divine Fragments**: Faith-based power (requires belief to stabilize gods)
- **Arcane Manipulation**: Reality editing (bypasses Worldroot restrictions)
- **Primal Instinct**: Totem spirits (surrender control for power)
- **Dominion Magic**: Consumed divinity (corrupts user with Null whispers)
- **Void Attunement**: Entropy itself (strips away meaning)

### World Structure

**Starter Zones** (1-10): Faction-specific, lore-integrated
- Thornveil Enclave (Sylvaen)
- Briarwatch Crossing (Sylvaen secondary hub linked to Thornveil)
- Temporal Steppes (High Elves)
- Borderkeep (Humans)
- The Untamed Reaches (Wildborn Therakai)
- The Scarred Highlands (Gronnak)

**Endgame Zones** (60+):
- **The Root Core**: See the Green Memory, including your absence from it
- **The Null Threshold**: Where Eldara meets oblivion

### Endgame Content

- **World Raids**: 40-player raids (The Root Core, God's Corpse, Temporal Labyrinth)
- **Factional Warfare**: Territory control, siegeable fortresses
- **Giant Awakening**: Server-wide events with permanent consequences
- **The Choice**: Collective player actions determine the world's fate

## 🛠️ Technology Stack

### Server (Authoritative)
- **.NET 8** - C# 12
- **TCP Networking** - Reliable delivery
- **MessagePack** - Binary serialization
- **20 TPS** - Fixed tick rate world simulation
- **Thread-safe architecture** - Concurrent entity management

### Client (Display + Prediction)
- **Unreal Engine 5.x**
- **C++20 + Blueprints**
- **Enhanced Input**
- **UMG/CommonUI + GAS-friendly UI patterns**
- **Niagara VFX**

### Shared Library
- **MessagePack** - Serialization
- **Lore-grounded data structures** - Race, Class, Faction, Combat

## 📂 Repository Structure

```
WorldofEldara/
├── Client/                    # Unreal Engine project (prototype)
├── Server/                    # .NET 8 authoritative server ✓
│   └── WorldofEldara.Server/
│       ├── Core/              # Entity management, bootstrap
│       ├── Networking/        # TCP server, client connections
│       ├── World/             # World simulation, zones, time
│       └── Program.cs         # Entry point
├── Shared/                    # Shared library ✓
│   └── WorldofEldara.Shared/
│       ├── Data/              # Character, combat, world data
│       ├── Protocol/          # Network packets
│       └── Constants/         # Game constants
├── Docs/                      # Documentation ✓
│   ├── Architecture/          # Server & network architecture
│   └── Design/                # Game design documents
├── Tools/                     # Development tools (future)
├── WORLD_LORE.md             # Canonical lore document ✓
└── PROJECT_STRUCTURE.md      # Full project layout ✓
```

**✓** = Implemented  
**Future** = Planned

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - For version control
- **(Optional) Unreal Engine 5.7** - For client development

### Running the Server

1. **Clone the repository**
   ```bash
   git clone https://github.com/gavelinrobert-beep/WorldofEldara.git
   cd WorldofEldara
   ```

2. **Build the server**
   ```bash
   cd Server/WorldofEldara.Server
   dotnet restore
   dotnet build
   ```

### Bringing up the Unreal client prototype

1. Install **Unreal Engine 5.7**.
2. From the repository root, open `Eldara.uproject` (Engine will generate project files and compile the `Eldara` module on first launch).
3. Editor targets (`EldaraEditor.Target.cs`) are present so you can build from the Unreal Editor or via `UnrealEditor` command line.
4. Core gameplay classes live in `Source/Eldara` (GameMode, GameInstance, PlayerController, AI, DataAssets).
5. Default configuration lives in `Config/DefaultEngine.ini` and `Config/DefaultGame.ini` (currently using the engine placeholder map `/Engine/Maps/Entry` with `EldaraGameModeBase`).

3. **Run the server**
   ```bash
   dotnet run
   ```

   You should see:
   ```
   ===========================================
     World of Eldara - Authoritative Server
     The Worldroot is bleeding. Memory awaits.
   ===========================================
   [INFO] Server Name: World of Eldara - Server 1
   [INFO] Port: 7777
   [INFO] Max Players: 5000
   [INFO] Tick Rate: 20 TPS
   [INFO] World simulation started
   [INFO] Network server listening on port 7777
   [INFO] Server started successfully. Press Ctrl+C to shutdown.
   ```

4. **Server is now running!** Ready to accept client connections on port 7777.

### Configuration

Edit `Server/WorldofEldara.Server/appsettings.json`:

```json
{
  "Server": {
    "Name": "World of Eldara - Server 1",
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

### Testing (No Client Yet)

Since the Unreal client is not yet implemented, you can:

1. **Use packet sniffer tools** to see network traffic
2. **Write a test client** using the shared protocol
3. **Read the logs** to verify server operation
4. **Remember**: characters are persisted in-memory only for the running server process (no database yet). Restarting the server clears created characters.
5. **Movement**: the server reconciles movement using class movement speed and will correct client prediction if it diverges.

## 📚 Documentation

- [`WORLD_LORE.md`](WORLD_LORE.md) - **Canonical lore (immutable)**
- [`PROJECT_STRUCTURE.md`](PROJECT_STRUCTURE.md) - Full project layout
- [`Docs/Architecture/ServerArchitecture.md`](Docs/Architecture/ServerArchitecture.md) - Server design
- [`Docs/Architecture/NetworkProtocol.md`](Docs/Architecture/NetworkProtocol.md) - Network protocol spec
- [`Docs/UnrealImplementationGuide.md`](Docs/UnrealImplementationGuide.md) - Unreal client bring-up and feature milestones
- [`docs/gameplay/starter-zone-01.md`](docs/gameplay/starter-zone-01.md) - Whispering Canopy starter zone spec + Unreal build guide

## 🏗️ Current Status

### ✅ Completed

- [x] Canonical lore document with cosmic structure
- [x] Shared library with lore-grounded data structures
- [x] Network protocol with MessagePack serialization
- [x] Authoritative game server (20 TPS)
- [x] Entity management (thread-safe)
- [x] Zone system
- [x] Time management (Eldara day/night cycle)
- [x] TCP networking with packet routing
- [x] Character creation with lore validation
- [x] Character selection and world entry
- [x] Basic movement input handling
- [x] Chat system (zone-based)
- [x] Quest system architecture (data-driven, branching, persistent impact)
- [x] World events & corruption model (zone states, thresholds, mutators)

### 🚧 In Progress

- [ ] Unreal client project initialization
- [ ] Combat system (server authoritative)
- [ ] NPC AI behavior trees

### 📋 Planned

- [ ] Client prediction and reconciliation
- [ ] Complete combat system
- [ ] NPC AI with patrol and combat behaviors
- [ ] Quest runtime integration and live content cadence
- [ ] Inventory and equipment
- [ ] Guild system
- [ ] Raid content
- [ ] World events (Giant awakenings, Void rifts)
- [ ] Database persistence (PostgreSQL)

## 🧭 Quest System (Data-Driven, Branching, Persistent)

- **Data Assets**: `UEldaraQuestData` holds title, description, objectives, rewards, prerequisites, repeatable flag, and main story flag.
- **States**:
```cpp
UENUM(BlueprintType)
enum class EQuestState : uint8 { Inactive, Available, Active, Completed, Failed };
```
- **Objectives**: `UEldaraQuestObjective` base (BlueprintType, abstract) with `ObjectiveText`, optional flag, and `IsComplete` override.
  - Supports kill, talk, collect, explore, survive, choice, escort, defend.
  - Multiple objectives can progress simultaneously.
- **Conditions**: Reusable assets gate availability/branches.
  - Examples: race, faction rep, prior choices, corruption thresholds, time of day, hidden triggers.
- **State Management**: Server keeps `TMap<UEldaraQuestData*, EQuestState>` per player; validates objective updates, applies rewards, persists choices, and replicates quest state.
- **Dialogue Integration**: Nodes offer quests, branch on quest state, and record choices that complete/fail paths or unlock hidden quests.
- **Rewards**: Modular XP, items, currency, faction reputation, abilities, titles, cosmetics, or world-state changes (immediate or delayed/conditional).
- **World Impact**: Quests flip world-state flags that drive NPC presence, zone presentation, vendors, travel routes, and future quest unlocks.
- **Debug/Tooling**: Hooks for force-complete, reset quest, simulate choice, and view world-state flags for QA and live ops.

## 🌑 World Events & Corruption (Persistent, Player-Driven)

- **Corruption Metric**: Each zone tracks `CorruptionLevel` (0–100) with states Pristine, Tainted, Corrupted, Overrun, Lost; visuals/audio always reflect state.
- **Drivers**: Time, player actions, failed events, bosses, and narrative triggers push corruption; adjacent zones influence one another.
- **Events**: Threshold-driven event types—Cleansing, Invasion, Ritual, Defense, Escort, Boss Manifestation—respond to corruption instead of random timers.
- **Outcomes**: Success lowers corruption, restores vendors/travel, and unlocks story arcs; failure escalates enemies, locks quests, and spawns empowered bosses with mutators.
- **Factions**: Verdant Accord, Void cultists, pirates, etc., react uniquely with their own objectives and rewards.
- **Persistence & Safety**: World state persists server-side and is replicated to entering players; starter zones have corruption caps and accelerated recovery to protect onboarding.

## 🎯 Design Philosophy

1. **Lore Governs Mechanics**: If a system contradicts lore, change the system
2. **Server Authority**: Client is display only (no client-side hacks)
3. **Player Choices Matter**: Permanent consequences, not just dialogue
4. **No Pure Good/Evil**: All factions have valid philosophies
5. **The World is Failing**: This is not status quo—it's a crisis

## 🔒 Security & Anti-Cheat

- **Server authoritative** - Client cannot directly modify game state
- **Movement validation** - Speed, teleportation, bounds checking
- **Ability validation** - Range, resources, cooldowns, line-of-sight
- **Lore validation** - Invalid race-class combos rejected
- **Rate limiting** - Packet flood protection

## 📊 Performance Targets

- **5,000 concurrent players** per server
- **20 TPS** stable tick rate
- **<200ms** client latency handling
- **<100MB** per 1,000 players memory usage

## 🤝 Contributing

This is a demonstration project. If you want to contribute:

1. Read [`WORLD_LORE.md`](WORLD_LORE.md) thoroughly
2. Ensure all changes respect canonical lore
3. Follow existing code structure and patterns
4. Write tests for new features
5. Update documentation

## 📜 License

Proprietary - All rights reserved

## 🌟 Acknowledgments

Built as a technical demonstration of:
- Lore-driven MMO design
- Server-authoritative architecture
- .NET 8 + Unreal Engine integration
- MessagePack networking
- PC-focused MMORPG development

---

**"Memory is power. Change is inevitable. Meaning must be chosen repeatedly."**

*Welcome to Eldara. The Worldroot is bleeding. What will you preserve?*
