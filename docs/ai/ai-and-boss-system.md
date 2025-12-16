# Enemy AI & Boss System

## Overview

The AI system for World of Eldara is built on Unreal Engine 5's **Behavior Trees** and **Blackboards**, with custom C++ controllers and tasks. The system supports standard enemy AI (perception, aggro, combat) and complex boss mechanics (phases, telegraphing, positioning).

## Design Principles

1. **Behavior Tree-Driven**: All AI logic defined in visual Behavior Trees
2. **Blackboard Data**: Shared state (target, health %, aggro) stored in Blackboard
3. **Modular Tasks**: Reusable BT tasks for common behaviors (chase, attack, patrol)
4. **Server Authority**: All AI decisions made on server, replicated to clients
5. **Performance-Conscious**: Efficient perception updates, LOD-based behavior complexity

## AI Controller

### EldaraAIController (C++ Base Class)

All NPC/enemy AI use this controller.

**Responsibilities:**
- Initialize Behavior Tree and Blackboard
- Handle perception (sight, hearing)
- Manage aggro/threat
- Execute Behavior Tree logic
- Update Blackboard keys

**Key Functions:**
```cpp
// Called when AI possesses a pawn
virtual void OnPossess(APawn* InPawn) override;

// Set the behavior tree for this AI
void InitializeBehaviorTree(UBehaviorTree* BehaviorTreeAsset);

// Update perception (called on PerceptionComponent update)
void OnTargetPerceptionUpdated(AActor* Actor, FAIStimulus Stimulus);

// Add threat to aggro table
void AddThreat(AActor* ThreatSource, float ThreatAmount);

// Get current highest threat target
AActor* GetHighestThreatTarget();
```

### OnPossess Implementation
```cpp
void AEldaraAIController::OnPossess(APawn* InPawn)
{
    Super::OnPossess(InPawn);

    // TODO: Get BehaviorTree and Blackboard from possessed pawn's data
    // For now, this is a placeholder
    // In production, each enemy would have a reference to its BT asset

    // Example:
    // UBehaviorTree* BT = GetBehaviorTreeFromPawn(InPawn);
    // if (BT) { RunBehaviorTree(BT); }
}
```

## Behavior Tree Structure

### Standard Enemy Behavior Tree (BT_EnemyBasic)

```
Root
├── Selector: Main Logic
│   ├── Sequence: In Combat
│   │   ├── Blackboard Check: IsInCombat == True
│   │   ├── Selector: Combat Actions
│   │   │   ├── Sequence: Use Ability (if available)
│   │   │   │   ├── Task: SelectAbility
│   │   │   │   └── Task: CastAbility
│   │   │   └── Sequence: Basic Attack
│   │   │       ├── Task: MoveToTarget (if out of range)
│   │   │       └── Task: BasicAttack
│   ├── Sequence: Patrol
│   │   ├── Task: GetNextPatrolPoint
│   │   ├── Task: MoveToLocation
│   │   └── Task: Wait (random 2-5s)
│   └── Task: Idle (default fallback)
```

### Boss Behavior Tree (BT_BossAdvanced)

```
Root
├── Selector: Boss Logic
│   ├── Sequence: Phase 3 (< 25% health)
│   │   ├── Blackboard Check: HealthPercent < 0.25
│   │   ├── Task: TriggerPhase3Transition (summon adds, visual change)
│   │   ├── Decorator: Cooldown (1s)
│   │   └── Selector: Phase 3 Abilities
│   │       ├── Task: UltimateAbility (if cooldown ready)
│   │       ├── Task: SummonMinions
│   │       └── Task: EnragedAttack
│   ├── Sequence: Phase 2 (25-60% health)
│   │   ├── Blackboard Check: HealthPercent < 0.60
│   │   ├── Task: TriggerPhase2Transition
│   │   └── Selector: Phase 2 Abilities
│   │       ├── Task: TelegraphedAoE
│   │       ├── Task: TargetSwap (switch to highest threat)
│   │       └── Task: HeavyAttack
│   └── Sequence: Phase 1 (60-100% health)
│       └── Selector: Phase 1 Abilities
│           ├── Task: LungeAttack
│           ├── Task: ChargeAbility
│           └── Task: BasicAttack
```

## Blackboard Keys

### EldaraAIKeys.h (Constants)

Defines all Blackboard key names used by AI.

```cpp
// Key for target actor (player or NPC)
const FName TargetActor = TEXT("TargetActor");

// Key for target location (patrol point, ground AoE location)
const FName TargetLocation = TEXT("TargetLocation");

// Key for current aggro/threat value
const FName AggroValue = TEXT("AggroValue");

// Key for combat state (bool)
const FName IsInCombat = TEXT("IsInCombat");

// Key for health percentage (0.0 - 1.0)
const FName HealthPercent = TEXT("HealthPercent");

// Key for line-of-sight to target (bool)
const FName HasLineOfSight = TEXT("HasLineOfSight");

// Key for corruption state (bool, for corrupted enemies)
const FName IsCorrupted = TEXT("IsCorrupted");
```

**Usage in Behavior Trees:**
- Decorators check Blackboard values (e.g., "Is HealthPercent < 0.5?")
- Tasks read and write Blackboard values (e.g., "Set IsInCombat to True")
- Services update Blackboard periodically (e.g., update HealthPercent every 0.5s)

## AI Archetypes

### 1. Melee Aggro (Tank Enemy)
- **Behavior**: Chase player, melee attack, high health
- **Abilities**: None or simple cleave
- **Threat**: Generates high threat, ideal for tanking in group content
- **Example**: Corrupted Wolf, Root Horror

### 2. Ranged DPS
- **Behavior**: Keep distance from player, ranged attacks
- **Abilities**: Fireball, poison shot
- **Threat**: Low threat, priority target for players
- **Example**: Corrupted Bird, Void Sprite

### 3. Healer/Support
- **Behavior**: Stay back, heal allies, buff allies
- **Abilities**: Heal spell, buff aura
- **Threat**: High priority, players must interrupt/kill quickly
- **Example**: Corruption Shaman

### 4. Caster DPS
- **Behavior**: Cast channeled or long-cast spells, vulnerable while casting
- **Abilities**: Heavy-hitting spell, AoE, DoT
- **Threat**: Medium threat, can be interrupted
- **Example**: Void Warlock

### 5. Stealth/Assassin
- **Behavior**: Stealth, ambush player, high burst damage
- **Abilities**: Stealth, backstab, vanish
- **Threat**: Low threat, disruptive
- **Example**: Shadow Stalker

### 6. Swarm/Weak
- **Behavior**: Weak individually, dangerous in packs
- **Abilities**: None, just melee
- **Threat**: Minimal per-enemy, high if ignored
- **Example**: Corrupted Squirrel, Void Rat

## Perception & Aggro System

### Perception Component (UE5 Built-In)

**Senses:**
- **Sight**: Primary sense for detecting players (cone-based, range-limited)
- **Hearing**: Detect loud player actions (running, casting abilities)
- **Damage**: Automatically aggro when taking damage

**Configuration:**
- **Sight Range**: 1500 cm (standard enemies), 3000 cm (bosses)
- **Sight Angle**: 90 degrees (standard), 360 degrees (bosses)
- **Hearing Range**: 1000 cm
- **Forget Time**: 10 seconds without stimulus → lose aggro

### Aggro/Threat System

**Threat Sources:**
- Damage dealt: 1 threat per 1 damage
- Healing: 0.5 threat per 1 healing (healer pulls aggro)
- Taunts: Instant high threat (tank ability)
- Proximity: Slight threat for being nearby

**Threat Table:**
- AI maintains a threat table (map of Actor → Threat Value)
- Highest threat actor becomes `TargetActor` in Blackboard
- Threat decays over time if target is not in combat

**De-Aggro Conditions:**
- Target dies
- Target leaves combat zone (leash distance)
- AI loses line-of-sight for >10 seconds (stealth/evasion)
- AI health fully restored (evade reset)

### Leashing
If player kites enemy too far from spawn point:
- Enemy stops chasing
- Enemy returns to spawn point
- Enemy health fully restored
- Aggro table cleared

**Leash Distance:** 50 meters from spawn point

## Corruption Modifiers

For corrupted enemies (lore-relevant):

**Visual Changes:**
- Black/purple ooze particles
- Glowing red eyes
- Distorted animations

**Stat Changes:**
- +20% damage
- +10% movement speed
- -10% health (trade-off)

**Behavior Changes:**
- More aggressive (lower aggro threshold)
- Ignore pain (continue attacking even at low health)
- Corruption aura (damages nearby players over time)

**Blackboard Key:** `IsCorrupted` (bool)

### Corruption Levels (Future)
- **Lightly Corrupted**: +10% damage, minor visual changes
- **Moderately Corrupted**: +20% damage, +10% speed, visual corruption
- **Heavily Corrupted**: +40% damage, new abilities, extreme visual corruption

## Boss Mechanics

### Boss Phases

Bosses transition through phases based on health percentage.

**Phase 1 (100% - 60%):**
- Basic attack rotation
- 1-2 simple mechanics
- Low threat, learning phase for players

**Phase 2 (60% - 25%):**
- Increase attack speed or damage
- Add 1-2 new mechanics
- May summon adds
- Telegraphed AoE attacks

**Phase 3 (25% - 0%):**
- "Enrage" mode
- All abilities available
- Increased damage
- Desperation mechanics (e.g., "If boss not killed in 60s, wipe")

### Phase Transitions
- Visual cue (e.g., boss roars, particles burst)
- Brief invulnerability (1-2s) during transition
- Announce phase change (boss dialogue or UI message)

**Example (Sorrow Stag Boss):**
- **Phase 1**: Basic charge and antler swipe
- **Phase 2 (60%)**: Summons Corrupted Saplings (adds), ground-targeted AoE (root prison)
- **Phase 3 (25%)**: Enraged, movement speed increased, AoE every 10s, Corruption aura damages all players

### Telegraphing

**Why Telegraph?**
Players need time to react to big attacks, especially in a tab-target/action hybrid game.

**Telegraphing Methods:**
1. **Ground Decal**: Red circle/rectangle on ground showing AoE area
2. **Castbar**: Boss has visible castbar for long-cast abilities
3. **Animation Wind-Up**: Boss rears back before charging
4. **Audio Cue**: Distinct sound before attack (roar, chime)
5. **Visual Effect**: Boss glows or changes color before attack

**Timing:**
- 1-2 seconds telegraph for standard attacks
- 3-4 seconds telegraph for one-shot mechanics

**Example:**
- Boss begins casting "Worldroot Crush" (3s cast)
- Red circle appears on ground under 3 random players
- Players have 3 seconds to move out of circles
- If player is in circle when cast finishes, heavy damage + knockdown

### Boss Positioning

**Melee Bosses:**
- Tank stands in front (faces away from group to avoid cleave)
- Melee DPS behind boss (avoid frontal attacks)
- Ranged DPS and healers at medium range

**Ranged Bosses:**
- Spread out to avoid AoE chaining
- Interrupt casts when possible
- Line-of-sight required (hide behind pillars for some mechanics)

**Multi-Target Bosses:**
- Boss + adds, players must prioritize adds or ignore them based on strategy

## Scaling & Difficulty

### Scaling by Player Level
- Enemy stats scale to match player level (health, damage, armor)
- Abilities remain the same (no new abilities per level)
- Ensures consistent difficulty across level ranges

### Scaling by Group Size
- In group zones, enemies have more health per additional player
- Damage does not scale (prevents one-shot issues)

### Difficulty Tiers (Future)
- **Normal**: Standard difficulty, forgiving mechanics
- **Heroic**: Increased damage, faster casts, more adds
- **Mythic**: New mechanics, tighter enrage timers, coordinated strategy required

## Elites & Named Enemies

### Elite Enemies
- Tougher version of standard enemy (2-3x health, +50% damage)
- 1-2 special abilities
- Requires strategy or group (not soloable for most classes)
- Better loot (blue quality gear)

**Example:** Nightfang Alpha (elite wolf in Whispering Canopy)
- Has "Pack Leader" buff (nearby wolves have +20% damage)
- "Howl" ability (fears nearby players for 3s)

### Named Enemies
- Unique NPCs with backstory
- Part of quest chains or world events
- Guaranteed rare loot or quest item drop
- Respawn time: 15-30 minutes (contested by players)

**Example:** Vareth the Corruptor (named Void Warlock)
- Quest target: "Slay Vareth and recover the Corrupted Tome"
- Abilities: Void Bolt, Summon Void Spawn, Corruption Nova

## AI Behavior Tree Tasks (Custom C++ Tasks)

### BTTask_SelectAbility
- Evaluate available abilities (cooldown, range, resource cost)
- Choose best ability based on situation (AoE if multiple targets, single-target if one)
- Write chosen ability to Blackboard

### BTTask_CastAbility
- Read ability from Blackboard
- Validate target and range
- Cast ability via CombatComponent
- Handle animation and effects

### BTTask_MoveToTarget
- Move AI to within ability range of `TargetActor`
- Use pathfinding (NavMesh)
- Stop when in range

### BTTask_BasicAttack
- Perform basic melee or ranged attack
- Play attack animation
- Apply damage to target

### BTTask_Patrol
- Get next patrol point from patrol path
- Move to patrol point
- Wait briefly at patrol point

### BTTask_Flee
- Used by low-health enemies or specific behaviors
- Move away from `TargetActor`
- May trigger "evade" reset if flee succeeds

### BTTask_SummonMinions
- Boss ability: spawn adds
- Spawn X minions at specified locations
- Minions inherit boss's aggro table

### BTTask_TelegraphedAoE
- Boss places AoE warning decal
- Wait for telegraph duration
- Execute AoE damage to all players in area

## Performance Optimization

### LOD-Based AI
- **High Priority (close to player)**: Full behavior tree, frequent perception updates
- **Medium Priority**: Simplified behavior, less frequent updates
- **Low Priority (far from player)**: Minimal updates, sleep AI until needed

### Perception Throttling
- Limit perception updates to every 0.5s instead of every frame
- Bosses and elites update more frequently (0.1s)

### Behavior Tree Services
- Services update Blackboard keys periodically (e.g., HealthPercent every 0.5s)
- Avoids constant recalculation

### Pooling
- Reuse AI controllers and behavior tree instances when possible
- Despawn distant enemies, respawn when players approach

## Implementation Files

### C++ Files:
- `Source/Eldara/AI/EldaraAIController.h/.cpp` (AI controller base class)
- `Source/Eldara/AI/EldaraAIKeys.h` (Blackboard key constants)

### Behavior Tree Assets:
- `BT_EnemyBasic` (Standard enemy)
- `BT_EnemyRanged` (Ranged enemy)
- `BT_EnemyHealer` (Support enemy)
- `BT_Boss_SorrowStag` (Sorrow Stag boss)

### Blackboard Assets:
- `BB_EnemyBasic` (Standard enemy Blackboard)
- `BB_Boss` (Boss Blackboard with phase keys)

### Custom Behavior Tree Tasks (C++):
- `BTTask_SelectAbility.cpp`
- `BTTask_CastAbility.cpp`
- `BTTask_MoveToTarget.cpp`
- `BTTask_BasicAttack.cpp`
- `BTTask_TelegraphedAoE.cpp`

## Testing Checklist

- [ ] AI perceives player and aggros
- [ ] AI chases player and attacks
- [ ] AI uses abilities when available
- [ ] AI leashes when player kites too far
- [ ] AI returns to patrol after combat
- [ ] Boss transitions between phases correctly
- [ ] Boss telegraphed attacks give warning
- [ ] Boss summons adds in Phase 2
- [ ] Elite enemies have buffed stats and abilities
- [ ] Named enemies drop quest items

## Future Enhancements

- **Dynamic Difficulty**: AI adapts to player skill (if player is winning easily, increase difficulty slightly)
- **Squad AI**: Enemies coordinate (healer stays back, tank protects healer)
- **Environmental Awareness**: AI uses terrain (hide behind cover, climb ledges)
- **Emotional AI**: Enemies flee at low health, beg for mercy (lore-appropriate)
- **Factional AI**: Enemies of different factions fight each other if they meet
