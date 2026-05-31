# Architecture

World of Eldara uses a custom client and an authoritative server.

```text
Client/WorldofEldara.Client
        |
        | TCP + length-prefixed MessagePack packets
        v
Server/WorldofEldara.Server
        |
        v
Shared/WorldofEldara.Shared
```

## Client

The client owns presentation, input collection, local camera behavior, and rendering. It does not own game truth.

Current client layers:

- `EldaraGameForm`: window, frame loop, keyboard input, overlay
- `WorldScene`: prototype world state and draw calls
- `SoftwareRenderer`: first-party frame buffer renderer
- `EldaraServerClient`: TCP/MessagePack protocol adapter

## Server

The server owns authentication prototype state, character data, entity state, movement validation, combat, quests, chat, NPC spawning, and NPC simulation.

Important server rules:

- client requests are treated as requests, not truth
- movement is reconciled against server state
- ability use is validated for resources, cooldowns, range, target validity, and known abilities
- quest state progresses on server-side triggers

## Shared Contract

`Shared/WorldofEldara.Shared` is the protocol and data contract. Packets must remain compatible across client and server. Any protocol change should update both sides in the same PR.

## Rendering Direction

The first renderer is intentionally software-based. This keeps the frame loop understandable while we prove the game loop. Later we can add a hardware backend without changing the server contract.

Near-term renderer responsibilities:

- draw world-space primitives
- draw sprites/textures
- support camera zoom
- cull off-screen objects
- expose debug overlays for packet/entity state
