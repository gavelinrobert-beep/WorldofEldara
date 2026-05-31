# AI And Boss System

AI currently lives on the authoritative server. The client renders AI state; it does not decide target acquisition, attacks, or death.

## Current Server AI

NPC entities can:

- spawn from configured spawn points
- patrol simple waypoint paths
- scan for nearby hostile targets
- chase targets in range
- attack on cooldown
- leash back to spawn
- respawn after death
- broadcast state and combat events to clients

## Next AI Milestone

For the first vertical slice, keep AI simple and readable:

- one passive quest giver
- one hostile creature type
- one patrol route
- one melee attack
- one leash/return behavior
- one death event that advances quest progress

## Boss Direction

Boss logic should still be server-authored. A boss encounter can be modeled as a small state machine:

- `Idle`
- `Pull`
- `PhaseOne`
- `PhaseTwo`
- `Enrage`
- `Defeated`
- `Resetting`

The client should receive clear combat and telegraph packets later, then render warnings, effects, and health bars locally.
