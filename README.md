# World of Eldara

**A lore-driven PC fantasy MMORPG built around a custom client and an authoritative .NET server.**

> "The Worldroot is bleeding. Memory awaits. What will you preserve?"

## Direction

World of Eldara is no longer an Unreal Engine or Henky3D project. The current direction is:

- a custom desktop game client built in C#
- a first-party software renderer that we can evolve into a hardware renderer later
- a .NET 8 authoritative server
- shared protocol and game data models used by both client and server
- lore-first systems grounded in `WORLD_LORE.md`

The goal is to make the game pleasant to build, inspect, and iterate on without fighting a large editor or a third-party engine.

## Current Stack

### Client

- `Client/WorldofEldara.Client`
- .NET 8 Windows desktop application
- WinForms host window
- custom software frame buffer renderer
- keyboard movement with server-backed prediction/correction
- TCP/MessagePack client adapter for the existing server protocol
- automatic prototype login, character creation/selection, and world entry
- first-pass terrain readability, mouse target selection, and camera zoom
- sprite asset loading for `.eldsprite` and `.png` files
- server-backed player status panel with health, mana, and stamina bars
- selected-target panel and primary ability cooldown feedback
- floating damage and healing numbers over combat targets
- visible login/character selection panel before entering the world
- starter grove props, clickable lore objects, and quest markers
- selected-target interaction and primary ability requests
- visible dialogue panel and quest tracker

### Server

- `Server/WorldofEldara.Server`
- .NET 8
- TCP networking
- MessagePack packets
- fixed tick world simulation
- server-authoritative movement, combat, quests, chat, NPC spawning, and simple NPC AI

### Shared

- `Shared/WorldofEldara.Shared`
- shared character, combat, quest, world, and protocol models
- packet unions used by both the server and custom client

## Repository Layout

```text
WorldofEldara/
├── Client/
│   └── WorldofEldara.Client/      # Custom C# game client
├── Server/
│   └── WorldofEldara.Server/      # Authoritative MMO server
├── Shared/
│   └── WorldofEldara.Shared/      # Protocol and shared game data
├── Docs/                          # Architecture, design, and roadmap notes
├── assets/                        # Future custom asset workspace
├── ui/                            # Optional world-map/design prototype
├── WORLD_LORE.md                  # Canonical lore
├── PROJECT_STRUCTURE.md           # Technical layout
└── README.md
```

## Run The Server

```powershell
dotnet run --project Server\WorldofEldara.Server\WorldofEldara.Server.csproj
```

The default server listens on `127.0.0.1:7777`.

## Run The Custom Client

In another terminal:

```powershell
dotnet run --project Client\WorldofEldara.Client\WorldofEldara.Client.csproj
```

Controls in the prototype:

- `WASD`: move through the rendered grove
- `C`: connect to the local server, create/select a prototype character, and enter the world
- left click: select NPCs, runestones, or the Worldroot
- `E`: talk/interact with the selected NPC; press again to accept or turn in a shown quest
- `1`: use the primary offensive ability on a selected hostile target
- mouse wheel: zoom the camera
- `Esc`: quit

## Near-Term Milestone

The next milestone is a playable vertical slice:

1. add character create/select UI over the current automatic prototype flow
2. expand the sprite set with authored PNG textures
3. add authored PNG textures for the starter grove
4. polish quest tracker and combat UI into final in-game panels
5. add more server-backed world objects beyond local lore props

## Design Rule

Lore remains the constraint. The engine is ours, but every system should still answer to Eldara's core idea: memory, consequence, and a world trying not to be erased.
