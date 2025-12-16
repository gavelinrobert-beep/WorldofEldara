# Unreal Engine Client Implementation & Migration Guide

Actionable steps to deliver an Unreal Engine–based client while keeping the existing authoritative .NET server and shared protocol.

## 1) Project Initialization (Unreal Engine 5.x)
1. Create a **C++ project** (Games → Third Person or Blank) with **Starter Content** enabled for quick prototyping.
2. Target **DX12/Vulkan**, enable **Nanite** and **Lumen** as needed, and set **Fixed Frame Rate** to 60 for consistent client prediction tuning.
3. Plugin/Module setup:
   - **Enhanced Input** (replace legacy input).
   - **MassAI** (optional) or **Behavior Trees/Blackboard** for AI visualization.
   - **GameplayAbilitySystem (GAS)** for ability/cooldown/resource handling (recommended).
   - **Replication Graph** if large-player-count optimization is needed later.
4. Project folders (match existing repo structure conceptually):
   ```
   Content/WorldofEldara/
     Blueprints/{Core,UI,Character,Abilities,NPC,Quests}
     C++/{Core,Networking,Movement,Combat,UI,NPC,Quest}
     Data/{DataTables,Curves}
     Maps/{MainMenu,CharacterCreation,Zone_Prototype}
   ```

## 2) Networking: Client with Authoritative .NET Server
- Keep the existing TCP/MessagePack protocol. Implement a lightweight **C++ socket layer** (BSD sockets) or integrate a small networking plugin that allows raw TCP.
- Deserialize **PacketBase** using one of:
  - **MessagePack-CSharp via C++/CLI bridge** (max compatibility with existing serializers; adds build complexity and is Windows-focused).
  - **MessagePack C++** (native and simpler to ship cross-platform; requires explicit schema sync and tests).
- In all cases, ensure schema parity with `Shared/WorldofEldara.Shared`.
- **Recommended**:
  - Choose **MessagePack C++** when you need cross-platform delivery, minimal build friction, and the team is comfortable maintaining schema sync/tests.
  - Choose **MessagePack-CSharp via C++/CLI** only if deployment is Windows-only, the team already uses C# serializers, and minimizing serialization drift outweighs added build complexity.
- Create a `UNetworkClient` UObject:
  - Connect to server host/port.
  - Background thread for socket I/O; game thread enqueues processed packets.
  - Dispatch table mirrors packet types: Auth, Character, Movement, Combat, Chat, Quest.
- Movement flow:
  - Send **input commands** (Forward/Right, Jump, Sprint) with timestamps.
  - Receive authoritative transforms; apply reconciliation (see below).

## 3) Client Prediction & Reconciliation (planned)
- Maintain a **circular buffer of inputs** with world timestamps.
- Predict locally using CharacterMovementComponent (CMC) with custom network smoothing disabled for local owner.
- On server correction:
  1) Snap to server transform.
  2) Rewind & replay buffered inputs after the authoritative timestamp.
  3) Blend to reduce visual pops (short interp window).
- Validate locally: clamp speed, floor checks, avoid client-side physics impulses affecting authority.

## 4) Server-Authoritative Combat (minimum slice via GAS)
1. Use **GAS** for client ability UI + cooldowns; treat server as authority for resolution.
2. Client sends `AbilityCastRequest` (abilityId, targetId, castStartTime).
3. Server validates (cooldown, resources, range, LOS, state) and responds `AbilityCastAccepted` or `AbilityRejected`.
4. Server resolves on tick, pushes:
   - `CombatEvent` (source, target, amount, damageType, flags)
   - `StatusApplied/Removed`, `CooldownUpdated`, `InterruptEvent`
5. Client UX:
   - Cast bar driven by server start tick + cast time.
   - GAS handles local montage/UI; use server events to confirm/adjust.
   - On reject, cancel local cast, reset UI, show error toast.

## 5) NPC AI (server-driven, client visual only)
- Keep AI authoritative on the .NET server with behavior trees/blackboard data there.
- Client only mirrors state:
  - Receive `NPCStateUpdate` (state enum, anim hints, targetId, position).
  - Drive Anim BP states (Idle/Patrol/Alert/Combat/Death) based on hints.
- For quick authoring of visuals, optional Unreal BTs can be used **only** for animation/FX fallbacks when server data is delayed—avoid logic divergence.

## 6) Quest System (server-driven)
1. Data definitions stay in shared layer (ids, prerequisites, objectives, rewards).
2. Flow:
   - `AcceptQuest(id)` → server validates → `QuestAccepted`.
   - Server sends `QuestProgress` deltas (kill, collect, interact, explore, escort).
   - Completion: `QuestComplete` with rewards and reputation; world-state hooks for events.
3. Client UI: Quest Log + Tracker widgets; read-only mirrors of server state.

## 7) Asset & Tooling Recommendations (Unreal)
- **Starter Content + Third Person template** for baseline movement/animations.
- **Advanced Locomotion System (ALS) Replication** (community) if you need a more advanced locomotion base with replicated movement states; verify the current maintainer, license terms, and last-update cadence (e.g., check the official repo/readme) before adoption.
- **Lyra Sample** (Epic) for reference on modular gameplay features, GAS patterns, and replication setups.
- **Free Assets**: “Infinity Blade” packs (environment/props), “Paragon” characters (for prototyping), marketplace free monthly assets.
- **UI**: Use UMG + CommonUI; start with minimal HUD (chat, action bar, target frame, quest tracker).
- **VFX**: Niagara samples from starter content or free marketplace packs.

## 8) Migration Steps from Legacy Client Plan
1. **Retire legacy client references**; README and `PROJECT_STRUCTURE.md` are Unreal-aligned. Create a GitHub issue (or backlog ticket) to replace any remaining non-Unreal details and assign it in the next sprint.
2. **Networking parity**: ensure MessagePack schemas stay identical; add client C++ deserializer tests using `Shared` definitions as reference.
3. **Input rewrite**: map to Enhanced Input actions (Move, Look, Jump, Sprint, Interact, Ability1-n).
4. **Movement**: customize CMC settings (max speed, accel, braking) to match server validation constants.
5. **Abilities**: implement GAS abilities that proxy to server requests; local prediction only for visuals.
6. **Quests/UI**: rebuild UI in UMG; bind to replicated quest/character state data models fed by server packets.
7. **Packaging**: configure platform targets (Win64/Linux) and ensure deterministic FPS for prediction tuning.

## 9) Milestone Order (practical)
1. Stand up Unreal project, socket connection, and packet dispatch.
2. Show authoritative position + chat round-trip.
3. Add client prediction + reconciliation with server transforms.
4. Implement ability request → server combat resolution → client VFX/UX via GAS.
5. Mirror NPC state updates for patrol/combat; drive animations accordingly.
6. Implement quest accept/progress/complete UI flow.
7. Expand content (assets/maps) and optimize replication.
