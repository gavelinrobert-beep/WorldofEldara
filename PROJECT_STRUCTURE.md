# World of Eldara - Project Structure

World of Eldara is now organized around a custom C# client, an authoritative .NET server, and shared protocol/data models. Unreal Engine and Henky3D have been removed from the active project.

## Root Structure

```text
WorldofEldara/
├── Client/
│   └── WorldofEldara.Client/
├── Server/
│   └── WorldofEldara.Server/
├── Shared/
│   └── WorldofEldara.Shared/
├── Docs/
├── assets/
├── ui/
├── WORLD_LORE.md
├── PROJECT_STRUCTURE.md
└── README.md
```

## Client

`Client/WorldofEldara.Client` is the first custom client. It is intentionally small:

```text
Client/WorldofEldara.Client/
├── Game/
│   └── WorldScene.cs              # Server-backed scene, camera, local input, draw calls
├── Networking/
│   └── EldaraServerClient.cs      # TCP MessagePack client plus prototype session flow
├── Rendering/
│   └── SoftwareRenderer.cs        # Software frame buffer and primitive drawing
├── EldaraGameForm.cs              # Window, frame loop, overlay, keyboard input
├── Program.cs
└── WorldofEldara.Client.csproj
```

The first renderer is a software renderer on purpose. It gives us total control over the frame loop, camera, draw order, and gameplay feedback before we decide whether to add a hardware backend. The current client can connect to the local server, run an automatic prototype login/create/select flow, enter the world, send movement input, apply server corrections, and render spawned entities.

## Server

`Server/WorldofEldara.Server` remains the authoritative runtime:

```text
Server/WorldofEldara.Server/
├── Core/
│   ├── EntityManager.cs
│   └── ServerBootstrap.cs
├── Networking/
│   ├── ClientConnection.cs
│   └── NetworkServer.cs
├── Quest/
│   └── QuestSystem.cs
├── World/
│   ├── SpawnSystem.cs
│   ├── TimeManager.cs
│   ├── WorldSimulation.cs
│   └── ZoneManager.cs
├── Program.cs
├── appsettings.json
└── WorldofEldara.Server.csproj
```

Responsibilities:

- account/session prototype flow
- character creation and selection
- server-authoritative movement
- combat validation and combat events
- chat routing
- quest state and progression
- NPC spawn, patrol, aggro, attack, death, and respawn behavior

## Shared

`Shared/WorldofEldara.Shared` contains types that must stay consistent between client and server:

```text
Shared/WorldofEldara.Shared/
├── Constants/
├── Data/
│   ├── Character/
│   ├── Combat/
│   ├── Quest/
│   └── World/
└── Protocol/
    ├── PacketBase.cs
    └── Packets/
```

This is the contract. When a packet changes here, both server and custom client should be updated in the same change.

## Documentation

`Docs/` is for architecture and gameplay notes. Unreal-specific implementation notes have been removed; future docs should describe the custom client and server protocol directly.

`WORLD_LORE.md` remains canonical lore.

## Removed Tracks

The following active tracks were removed:

- Unreal Engine project files and content
- Henky3D submodule and CMake client
- shader files tied to the Henky3D prototype
- obsolete Unreal/Henky3D implementation summaries

The new rule is simple: one game client, one authoritative server, one shared protocol.
