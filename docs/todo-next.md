# TODO: Custom Client Roadmap

This roadmap replaces the old Unreal/Henky3D plan.

## Phase 1: Client Foundation

- [x] Create `Client/WorldofEldara.Client`
- [x] Add a desktop window and frame loop
- [x] Add a custom software frame buffer renderer
- [x] Draw a first Eldara grove scene with grid, Worldroot marker, trees, runestones, and player marker
- [x] Add keyboard movement
- [x] Add a TCP/MessagePack client adapter
- [x] Move server calls off prototype hotkeys and into automatic prototype session flow
- [x] Add frame timing, FPS, and basic diagnostics

## Phase 2: Server-Backed Gameplay

- [x] Send real `MovementInputPacket` from the client
- [x] Apply `PositionCorrectionPacket` from the server
- [x] Render `PlayerSpawnPacket`
- [x] Render `EntitySpawnPacket` and `EntityDespawnPacket`
- [x] Render `MovementUpdatePacket`
- [x] Render `NPCStateUpdatePacket`

## Phase 3: Character Flow

- [x] Build first custom UI panel for login, character list, creation, and selection
- [x] Send `CreateCharacterRequest`
- [x] Send `SelectCharacterRequest`
- [x] Enter the world after a selected character receives a spawn packet
- [x] Store local client preferences outside game state

## Phase 4: Combat And Quest Loop

- [x] Add target selection
- [x] Send `UseAbilityRequest`
- [x] Render first-pass `CombatEventPacket` damage and healing feedback
- [x] Render health/resource bars from server snapshots
- [x] Render floating combat text for damage and healing
- [x] Render selected target panel and primary ability cooldown feedback
- [x] Send quest dialogue and accept requests
- [x] Send quest turn-in requests and apply rewards server-side
- [x] Render quest log and objective progress as a dedicated panel

## Phase 5: Rendering Upgrade Path

- [x] Add first text-based sprite loading to the software renderer
- [x] Add PNG texture loading to the software renderer
- [x] Add camera zoom and first pass world-space readability
- [ ] Add debug draw layers for entity bounds and packet state
- [ ] Decide whether to keep software rendering for the vertical slice or add a hardware backend

## Phase 6: Vertical Slice

- [x] One race
- [x] One class
- [x] One starter grove
- [x] One quest giver
- [x] One hostile NPC type
- [x] One basic ability
- [x] One complete quest turn-in loop
- [x] One playable server-backed session from login to quest completion

## Immediate Next Step

- Use `Docs/gameplay/thornveil-enclave.md` as the source of truth for starter-zone tone, layout, architecture, and threats.
- Finish the Thornveil 3D zone pass in this order:
  - Heartbough Glade as a warm home hub with treehouses, banners, root paths, and cyan crystal landmarks.
  - Mossglass Pools as the first reflective Memory Echo objective space.
  - The Green Scar as the first contained corruption pocket.
  - First creature silhouettes: Echo Wolf, Mossglass Wraith, Blightroot Ravager, and Grove Stag.
  - First non-hostile life pass: fawns, deer, and small forest creatures near the hub.
- Improve the asset path so Blender/GLB assets become runtime meshes instead of temporary C# proxy meshes.
