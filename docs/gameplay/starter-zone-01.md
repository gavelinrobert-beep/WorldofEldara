# Starter Zone 01: Verdant Outskirts

Verdant Outskirts is the first custom-client vertical slice zone. It is a small controlled grove where new characters learn movement, targeting, combat, quests, and the Worldroot premise.

## Gameplay Goals

- move through a readable grove
- meet multiple quest givers
- fight several hostile creature types
- complete talk, interact, kill, and turn-in quests
- return for a reward
- see combat and quest feedback clearly
- build toward the first lower-root mini-dungeon

## First Scene Elements

- Worldroot marker at the center
- tree clusters for landmarks
- runestones for navigation anchors
- small hostile patrol area
- quest giver near the safe spawn
- Seedvault Veyr as the first mini-dungeon threshold
- Mossglass Pools, Green Scar, and Old Thornway as quest objective landmarks

## Quest Arc

- `A Name for a Season`
- `The Listening Bark`
- `Sap That Remembers`
- `The Hare That Died Twice`
- `The Scar That Learns`
- `The Door Under the Roots`
- `Below the Listening Door`
- repeatable support quest: `First Pruning`

The arc starts in Heartbough Glade, teaches dialogue and interact objectives, then pushes the player toward Mossglass Pools, the Green Scar, and Seedvault Veyr.

## First Mini-Dungeon Hook

Seedvault Veyr now has an open-world threshold area, `Seedvault Antechamber`, used as a prototype mini-dungeon space before true instancing exists.

- trash: Ward-Eaten Rootlings
- mini-boss: Sealgnawer Matron
- objective object: Veyr Memory Core
- purpose: test denser combat, boss targeting, quest chaining, and reward pacing inside one contained sub-area

## Client Requirements

- render server player spawn
- render NPC spawns
- render movement updates
- render damage/healing events
- render quest progress
- show a small quest tracker

## Server Requirements

- spawn Warden Elaris
- spawn hostile NPCs
- validate movement
- validate ability use
- register kills
- update quest state
