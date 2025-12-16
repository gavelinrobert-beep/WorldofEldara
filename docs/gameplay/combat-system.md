# Combat System

## Overview

The combat system is **server-authoritative**, **ability-based**, and **data-driven**. All combat resolution happens on the server to ensure fairness and prevent cheating. The client provides prediction and visual feedback for responsiveness.

## Design Principles

1. **Server Authority**: All damage calculation, ability validation, and effect application on server
2. **Ability-Based**: Combat centered around abilities with cooldowns, resources, and targeting
3. **Effect System**: Abilities apply effects (damage, healing, buffs, debuffs) to targets
4. **Resource Management**: Each class has a primary resource (mana, rage, energy, etc.)
5. **Crowd Control**: CC abilities with diminishing returns and break-on-damage rules
6. **PvE and PvP**: Separate balance flags and multipliers

## Combat Flow

```
Player Input (Ability Hotkey Press)
    ↓
Client: Validate (cooldown ready, resource available, target in range)
    ↓ (if valid)
Client: Send RPC to Server
    ↓
Server: Validate (cooldown, resource, range, line-of-sight)
    ↓ (if valid)
Server: Deduct resource, trigger cooldown
    ↓
Server: Execute ability logic (apply effects to target(s))
    ↓
Server: Replicate results to clients (damage numbers, effects)
    ↓
Clients: Play visual/audio feedback (particles, animations, hit sounds)
```

## Core Components

### EldaraCombatComponent (Actor Component)

Attached to all combat-capable actors (players, NPCs, bosses).

**Responsibilities:**
- Manage ability cooldowns
- Track resources (health, mana, rage, etc.)
- Handle ability activation requests
- Apply effects (damage, healing, buffs, debuffs)
- Track active effects and durations
- Handle crowd control states
- Emit combat events for UI and logging

**Key Functions:**
```cpp
// RPC: Client requests ability activation
UFUNCTION(Server, Reliable, WithValidation)
void Server_ActivateAbility(UEldaraAbility* Ability, AActor* Target);

// Can this ability be activated right now?
bool CanActivateAbility(UEldaraAbility* Ability, AActor* Target);

// Apply an effect to this actor
void ApplyEffect(UEldaraEffect* Effect, AActor* Instigator);

// Get current resource value (mana, rage, etc.)
float GetResourceValue(EResourceType Type);

// Deduct resource (returns true if sufficient resource available)
bool ConsumeResource(EResourceType Type, float Amount);
```

## Ability System

### UEldaraAbility (Data Asset)

Defines an ability's behavior.

**Properties:**
- `FText AbilityName` (e.g., "Worldroot Slam")
- `FText Description` (what the ability does)
- `UTexture2D* Icon` (UI display)
- `EAbilityTargetType TargetType` (Self, SingleTarget, GroundAoE, ConeAoE, etc.)
- `float Range` (max targeting range in cm)
- `float Cooldown` (seconds)
- `EResourceType ResourceType` (Mana, Rage, Energy, etc.)
- `float ResourceCost` (amount of resource consumed)
- `float CastTime` (seconds, 0 for instant)
- `bool bCanCastWhileMoving` (movement restriction)
- `TArray<UEldaraEffect*> EffectsToApply` (effects triggered by this ability)
- `UAnimMontage* CastAnimation` (animation to play)
- `USoundBase* CastSound` (sound effect)
- `UParticleSystem* CastVisual` (visual effect)

**Target Types:**
```cpp
UENUM()
enum class EAbilityTargetType : uint8
{
    Self,              // Cast on self (e.g., defensive buff)
    SingleTarget,      // Single enemy/ally
    GroundAoE,         // Area at target location
    ConeAoE,           // Cone in front of caster
    CircleAoE,         // Circle around caster
    LineAoE,           // Line from caster to target point
    NoTarget           // No targeting required (e.g., toggle)
};
```

### Ability Validation (Server)

Before activating an ability, server checks:
1. **Cooldown**: Is ability off cooldown?
2. **Resource**: Does caster have enough mana/rage/energy?
3. **Range**: Is target within ability's range?
4. **Line of Sight**: Can caster see target? (raycast check)
5. **Crowd Control**: Is caster stunned/silenced/feared?
6. **Target Valid**: Is target alive, targetable, correct faction?
7. **Movement**: If `bCanCastWhileMoving` is false, is caster stationary?

If any check fails, ability activation is denied and error sent to client.

## Effect System

### UEldaraEffect (Data Asset)

Defines an effect applied by an ability.

**Properties:**
- `FText EffectName` (e.g., "Burning", "Heal Over Time")
- `EEffectType EffectType` (Damage, Healing, Buff, Debuff, CrowdControl)
- `EDamageType DamageType` (Physical, Magic, Fire, Poison, etc.) - for damage effects
- `float Magnitude` (amount of damage/healing per application)
- `float Duration` (how long effect lasts, 0 for instant)
- `float TickInterval` (for DoT/HoT effects, how often to apply, 0 for instant)
- `int32 MaxStacks` (how many times effect can stack, 1 for non-stacking)
- `bool bRefreshDuration` (does applying again refresh duration or add a stack?)
- `ECrowdControlType CCType` (Stun, Root, Silence, Fear, etc.) - for CC effects
- `UParticleSystem* EffectVisual` (visual effect on target)
- `USoundBase* ApplySound` (sound when effect is applied)

**Effect Types:**
```cpp
UENUM()
enum class EEffectType : uint8
{
    Damage,            // Reduce health
    Healing,           // Restore health
    Buff,              // Increase stats (e.g., +10% damage)
    Debuff,            // Decrease stats (e.g., -20% armor)
    CrowdControl,      // Stun, root, silence, etc.
    ResourceRestore    // Restore mana/rage/energy
};
```

### ExecuteEffect (Blueprint Implementable)

```cpp
// Called when effect is applied or ticks
UFUNCTION(BlueprintImplementableEvent, Category="Combat")
void ExecuteEffect(AActor* Target, AActor* Instigator);
```

This allows designers to implement custom effect logic in Blueprint while keeping core combat in C++.

## Targeting System

### Target Selection (Client-Side)
- Tab targeting: Cycle through visible enemies
- Click targeting: Click on enemy
- Self-cast: Hold modifier key to cast on self
- Ground targeting: Show ground reticle for AoE placement

### Target Validation (Server-Side)
- Is target in range?
- Is target alive?
- Line-of-sight check (raycast)
- Faction check (can't heal enemies, can't damage allies - unless flagged for PvP)

### Area of Effect
- **Ground AoE**: All actors within radius of target point
- **Cone AoE**: All actors within cone angle and range from caster
- **Circle AoE**: All actors within radius of caster

## Resource System

### Resource Types:
```cpp
UENUM()
enum class EResourceType : uint8
{
    Health,            // All characters
    Mana,              // Memory Warden, Temporal Mage, Grove Healer, etc.
    Rage,              // Berserker, Witch-King Acolyte
    Energy,            // Void Stalker, Unbound Warrior
    Focus,             // Totem Shaman, Spirit Walker
    Corruption,        // Corruption-based classes (fills instead of depletes)
};
```

### Resource Regeneration:
- **Mana**: Passive regen, out-of-combat faster regen
- **Rage**: Generated by taking damage and dealing damage
- **Energy**: Passive regen, fast recovery
- **Focus**: Generated by specific abilities
- **Corruption**: Decays over time if not maintained

## Cooldown System

### Global Cooldown (GCD)
- 1.5 second GCD triggered by most abilities
- Some abilities are off-GCD (defensive cooldowns, movement abilities)

### Individual Cooldowns
- Each ability has its own cooldown (5s, 10s, 30s, 60s, etc.)
- Cooldown starts when ability is successfully cast
- Cooldown tracked per-ability, per-character

### Cooldown Reduction
- Stats can reduce cooldowns (e.g., Haste stat)
- Proc effects can reset cooldowns (e.g., "Critical hits reset cooldown")

## Crowd Control System

### CC Types:
```cpp
UENUM()
enum class ECrowdControlType : uint8
{
    Stun,              // Cannot move or cast
    Root,              // Cannot move, but can cast
    Silence,           // Cannot cast, but can move
    Fear,              // Lose control, run away randomly
    Sleep,             // Cannot act, breaks on damage
    Slow,              // Movement speed reduced
    Disarm,            // Cannot use weapon abilities
    Polymorph          // Transform into critter, breaks on damage
};
```

### Diminishing Returns
- Repeated CC of same type on same target has reduced duration
- First CC: 100% duration
- Second CC (within 18s): 50% duration
- Third CC (within 18s): 25% duration
- Fourth+ CC (within 18s): Immune

### Break-on-Damage Rules
- **Sleep/Polymorph**: Any damage breaks
- **Fear**: Damage does not break, duration only
- **Stun/Root/Silence**: Damage does not break

## PvE vs PvP Balancing

### PvE (Player vs Environment)
- Focus on rotation execution and boss mechanics
- Higher damage values for faster encounters
- Crowd control fully effective on normal enemies
- Bosses have CC immunity or greatly reduced duration

### PvP (Player vs Player)
- Damage reduced by 50% across the board
- Healing reduced by 30%
- Crowd control has diminishing returns
- Requires PvP flag to engage

### PvP Flags:
- **Flagged**: Can attack and be attacked by opposing faction
- **Unflagged**: Cannot attack or be attacked (safe mode)
- **Always Flagged Zones**: Contested territories, battlegrounds

## Combat Feedback (Client-Side)

### Visual Feedback:
- Damage numbers floating above target (color-coded by damage type)
- Healing numbers (green) above healed target
- Critical hits (larger, gold numbers)
- Miss/Dodge/Parry text
- Effect icons above target (burning, stunned, etc.)

### Audio Feedback:
- Hit sounds (metal clang, flesh impact, magic whoosh)
- Critical hit sound (distinct chime)
- Miss sound (swoosh)
- Ability cast sounds

### Haptic Feedback:
- Controller vibration on taking damage
- Lighter vibration on dealing damage

## First Implementation Targets

### Phase 1: Basic Melee
- Single-target instant attack ability
- Damage calculation (base + stats)
- Health deduction
- Death state

### Phase 2: Resources & Cooldowns
- Add mana/rage/energy system
- Cooldown tracking
- Resource consumption

### Phase 3: Effects
- Damage-over-time (DoT)
- Heal-over-time (HoT)
- Simple buffs (+damage%, +armor%)

### Phase 4: Crowd Control
- Stun effect
- Root effect
- Diminishing returns

### Phase 5: AoE Abilities
- Ground-targeted AoE
- Cone AoE
- Multi-target hit detection

### Phase 6: PvP
- PvP flagging system
- Damage/healing modifiers for PvP
- Faction-based targeting

## Performance Considerations

### Optimization:
- Cooldown tracking on server only, client displays countdown
- Client-side ability validation before sending RPC (reduce server load)
- Batch effect applications (apply all effects from one ability in single pass)
- Effect pooling (reuse effect objects instead of spawning new)

### Scaling:
- 100+ players in combat: Limit particle effects, use instanced meshes
- Boss with 40 players: Optimize effect replication, batch network updates
- Mass PvP: Reduce visual fidelity, prioritize nearby combat for updates

## Implementation Files

### C++ Files:
- `Source/Eldara/Characters/EldaraCombatComponent.h/.cpp` (Combat component)
- `Source/Eldara/Characters/EldaraAbility.h` (Ability data asset)
- `Source/Eldara/Characters/EldaraEffect.h` (Effect data asset)
- `Source/Eldara/Characters/EldaraCharacterBase.h/.cpp` (Character with health/resources)

### Blueprint Assets:
- `DA_Ability_WorldrootSlam`, `DA_Ability_TemporalBolt`, etc. (Ability definitions)
- `DA_Effect_Burning`, `DA_Effect_HealOverTime`, etc. (Effect definitions)
- `ABP_PlayerCharacter` (Animation blueprint with ability montages)
- `NS_AbilityCast`, `NS_AbilityImpact` (Niagara particle systems)

### UI Widgets:
- `WBP_ActionBar` (Ability hotbar)
- `WBP_TargetFrame` (Target health/resources)
- `WBP_PlayerFrame` (Player health/resources)
- `WBP_DamageNumber` (Floating combat text)

## Testing Checklist

- [ ] Abilities go on cooldown correctly
- [ ] Resources are consumed and regenerate
- [ ] Damage is calculated and applied
- [ ] Healing restores health
- [ ] DoT/HoT effects tick correctly
- [ ] Buffs/debuffs apply and expire
- [ ] Crowd control prevents actions as expected
- [ ] Diminishing returns work on repeated CC
- [ ] AoE abilities hit all targets in area
- [ ] Line-of-sight blocks ability targeting
- [ ] PvP damage reduction is applied
- [ ] Death state is reached at 0 health

## Future Enhancements

- **Combo System**: Abilities chain together for bonus effects
- **Interrupt Mechanics**: Cancel enemy casts
- **Reflect/Absorb**: Effects that redirect or absorb damage
- **Leech**: Damage dealt restores health
- **Dispel/Purge**: Remove buffs or debuffs
- **Immunities**: Temporary immunity to damage types or CC
- **Procs**: Chance-based ability triggers (e.g., "20% chance on hit to...")
