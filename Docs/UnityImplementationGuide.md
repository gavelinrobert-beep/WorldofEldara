# Unity Client Implementation Guide

Actionable steps to bring up the Unity 2022.3 LTS client and build the first game-playable loop that matches the authoritative server.

## 1) Project Initialization (Unity 2022.3 LTS)
1. Create a **3D (URP)** project named `WorldofEldara` targeting `.NET Standard 2.1` scripting backend and **Enable New Input System**.
2. Add packages:
   - **Input System** (Unity) – already included when enabling.
   - **Cinemachine** – camera rig + target follow.
   - **TextMeshPro** – UI text rendering.
   - **MessagePack C#** (via NuGet for Unity or embedded dll) – matches server serialization.
3. Project layout (aligns with `PROJECT_STRUCTURE.md`):
   ```
   Assets/_Project/
     Scripts/{Core,Networking,Movement,Combat,UI,NPCs,Quest}
     Prefabs/{Characters,NPCs,UI,VFX}
     Scenes/{MainMenu,CharacterCreation,Zone_Prototype}
   ```
4. Assembly Definitions:
   - `WorldofEldara.Shared` (import `Shared/WorldofEldara.Shared` C# files into `Assets/_Project/Shared/` or reference via UPM Git package).
   - `WorldofEldara.Client` depends on `WorldofEldara.Shared`, `Unity.InputSystem`, `Unity.RenderPipelines.Universal`.
5. Networking bootstrap:
   - `NetworkClient` MonoBehaviour handles TCP connect to `ServerConfig` host/port, reads `PacketBase` using MessagePack.
   - `PacketHandler` dispatches to feature managers (Movement, Combat, Chat, Quest).
6. Scenes:
   - `MainMenu`: connect + account placeholder; buttons for character select.
   - `CharacterCreation`: uses shared race/class data; sends `CreateCharacterRequest`.
   - `Zone_Prototype`: loads player prefab, camera, UI (action bar, target frame, quest tracker).

## 2) Client Prediction + Reconciliation (planned)
- **Input sampling** → send movement inputs each tick.
- Predict locally; keep a **circular buffer** of inputs with timestamps.
- On server position update, **rewind + replay** unmatched inputs; snap with smoothing.
- Client-side validation: clamp speed, ground check to avoid obvious desync.

## 3) Server-Authoritative Combat (minimum slice)
**Goal:** request-based combat with server truth; client only displays results.
1. Client sends `AbilityCastRequest` (abilityId, targetId, castStartTime).
2. Server validates (cooldown, resources, range, LOS, state) and replies:
   - `AbilityCastAccepted` (castId, serverStartTick) **or** `AbilityRejected`.
3. Server runs resolution on tick loop:
   - Apply resource costs on accept.
   - At impact tick: apply damage/heal/status via `DamageCalculator`, `ThreatManager`.
4. Server pushes results:
   - `CombatEvent` (source, target, amount, damageType, flags)
   - `StatusApplied/Removed`, `CooldownUpdated`, `InterruptEvent`
5. Client UX:
   - Cast bar driven by `serverStartTick + ability.castTime`.
   - Floaters/VFX from `CombatEvent`.
   - Desync guard: if server rejects, reset local cast and show error toast.

## 4) NPC AI Behavior Trees (server-side)
**Authoritative AI** lives on server; client only animates replicated state.
1. Behavior Tree shape:
   - **Root Selector**: Combat > Alert > Patrol > Idle.
   - **Patrol**: waypoint loop; on sight/hearing player → Alert.
   - **Alert**: face target, validate threat, transition to Combat when in range.
   - **Combat**: priority list (e.g., maintain distance, choose ability by cooldown/threat).
2. Blackboard data: target entityId, current zone node, lastHeardPos, isUnderCC.
3. Tick cadence: align with 20 TPS world tick. BT node budget per tick to avoid spikes.
4. Replication: server sends `NPCStateUpdate` (state enum, anim hints, targetId); client animates locally.
5. Authoring option: simple JSON/ScriptableObject describing BT nodes to keep data-driven.

## 5) Quest System (server-driven)
1. **Data**: quest definitions (id, prerequisites, objectives list, rewards, faction/world flags). Store in shared data for client display; authoritative copy on server.
2. **Flow**:
   - Client requests `AcceptQuest(id)` → server validates (prereqs, faction, world state) → replies `QuestAccepted`.
   - Server tracks objective progress (kill, collect, interact, explore, escort) and pushes `QuestProgress` deltas.
   - Completion: `QuestComplete` response includes rewards + reputation changes; server updates world state hooks.
3. **World State hooks**: triggers for Giant awakenings/Void rifts; quest chains unlock zones.
4. Client UI: quest tracker, quest log, interaction prompts (accept/turn-in). All progress read-only mirrors of server state.

## 6) Recommended Assets for Fast Prototyping
All are free (at time of writing) unless noted; verify licenses before shipping.
- **Environment**: “Low Poly Simple Nature Pack” or “Polygon Nature” (Synty, paid) for quick stylized zones; “Terrain Sample Asset Pack” for URP terrain setup.
- **Characters/Animations**: Unity “Starter Assets – Third Person” (controller + camera); **Mixamo** animations (export as FBX); “Free RPG Hero” pack for a placeholder player model.
- **UI**: “Fantasy UI Free” or “Simple UI Pack” for menus/bars; TextMeshPro for text.
- **VFX**: “Cartoon FX Free” (JMO) or “Stylized VFX Free Sample” for spell placeholders.
- **Tools**: “Behavior Bricks” (open-source BT) or “NodeCanvas” (paid, editor tooling) for BT authoring. “Odin Serializer” is not needed if using MessagePack.

## 7) Milestone Order (practical)
1. Stand up Unity project + connect to server; display networked position + chat.
2. Implement movement prediction + reconciliation against server transforms.
3. Add ability request → server combat resolution → client VFX/UX.
4. Implement NPC patrol + simple combat BT; replicate state.
5. Add quest accept/progress/complete loop with 1–2 sample quests.
6. Improve content (assets, scenes) and expand to chains/world events.
