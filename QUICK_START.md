# Quick Start

World of Eldara now runs as a custom C# desktop client plus an authoritative .NET server.

## Requirements

- .NET 8 SDK
- Windows for the current WinForms client prototype

## Build Everything

```powershell
dotnet build Server\WorldofEldara.Server\WorldofEldara.Server.csproj
dotnet build Client\WorldofEldara.Client\WorldofEldara.Client.csproj
```

## Run The Server

```powershell
dotnet run --project Server\WorldofEldara.Server\WorldofEldara.Server.csproj
```

The server listens on port `7777` by default.

## Run The Client

Open another terminal:

```powershell
dotnet run --project Client\WorldofEldara.Client\WorldofEldara.Client.csproj
```

## Run The 3D Camera Prototype

This is a standalone visual/camera prototype. It does not need the server yet.

```powershell
dotnet run --project Client\WorldofEldara.Client3D\WorldofEldara.Client3D.csproj
```

3D prototype controls:

- `WASD`: move the player through the grove
- `Q/E`: rotate the third-person camera
- mouse wheel: move the camera closer/farther
- `R`: reset camera distance/yaw
- `Esc`: quit

Controls:

- `WASD`: move in the prototype grove
- `C` or `Enter`: connect to `127.0.0.1:7777`
- `Up/Down`: choose a character in the session panel
- `Enter`: create a prototype character if needed, or enter the world with the selected character
- left click: select NPCs, runestones, or the Worldroot
- `E`: talk/interact with the selected NPC; press again to accept or turn in a shown quest
- `1`: use your primary offensive ability on a selected hostile target
- mouse wheel: zoom the camera
- `Esc`: quit

## What Exists Today

- server build is clean
- shared protocol/data project is reused by the client
- custom software renderer draws a first Eldara grove
- client loads `.eldsprite` and `.png` sprite assets for actors and landmarks
- client has a TCP/MessagePack adapter
- client sends real movement input and applies server position corrections
- client renders player/entity spawns, despawns, movement updates, and NPC state updates
- client has first-pass terrain readability, camera zoom, and clickable targets
- client has a visible local login/character selection panel
- starter grove includes decorative props, clickable lore objects, and quest markers
- selected quest NPCs can request dialogue/quest offers from the server
- completed quests can be turned in through a server-authoritative request/response packet
- selected hostile NPCs can receive server-authoritative ability requests
- combat hits show floating damage/healing numbers over the world
- dialogue responses appear in a bottom panel
- active/completed quest progress appears in a right-side tracker
- player health, mana, and stamina appear in a server-backed status panel
- selected targets and primary ability cooldowns have dedicated combat UI
- prototype login, character creation, character selection, and world entry are automated
- server already has character, movement, combat, chat, quest, spawn, and NPC foundations

## Next Useful Test

1. Start the server.
2. Start the client.
3. Press `C` in the client.
4. Confirm the overlay reaches world/session status.
5. Move with `WASD` and confirm the rendered player remains responsive while the server receives movement input.

After that, the next real work is replacing the automatic prototype flow with visible login and character panels, then adding target selection for combat and quest interaction.
