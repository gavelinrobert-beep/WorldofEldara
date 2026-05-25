# Combat System

Combat is server-authoritative. The client may present prediction and feedback, but the server decides whether an ability succeeds and what effect it has.

## Current Server Foundation

The server already supports:

- known ability validation
- target validation
- range checks
- global cooldown
- per-ability cooldowns
- resource costs
- physical and magical damage events
- healing events
- NPC death and player respawn flow
- quest progress from NPC kills

## First Client Work

The custom client should add:

- target selection
- basic action bar input
- `UseAbilityRequest`
- combat event rendering
- health/resource bars
- floating damage/healing numbers

## First Ability Set

Keep the vertical slice tiny:

- basic melee strike
- one class-flavored Memory Warden attack
- one simple heal or defensive skill

## Protocol

Relevant packets:

- `UseAbilityRequest`
- `AbilityResultPacket`
- `CombatEventPacket`
- `DamagePacket`
- `HealingPacket`
- `StatusEffectPacket`
