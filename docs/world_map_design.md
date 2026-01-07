# World of Eldara - World Map Design Document

## Overview

This document outlines the complete world map structure for World of Eldara, including region layout, greybox/blockout plans, traversal systems, quest gating, and the overall spatial organization of the game world. All content aligns with the canonical lore defined in WORLD_LORE.md.

---

## World Layout & Region Order

### Continental Structure

Eldara is organized into seven major regions, each representing a distinct philosophical conflict, faction territory, and tier of player progression. The world follows a hub-and-spoke model with non-linear progression after the initial zones.

### Region Progression Map

```
STARTER ZONES (Levels 1-15)
├── Worldroot Expanse (Sylvaen/Verdant Circles) - WEST
├── Verdaniel's Continuity Reach (High Elves/Ascendant League) - NORTH
└── Human Territories (United Kingdoms) - CENTRAL

MID-TIER ZONES (Levels 15-35)
├── Vael's Shattered Totem Wilds (Therakai Civil War) - EAST
├── Korrath's Warfront Marches (Contested Territory) - SOUTH-CENTRAL
└── Nereth's Silent Fens (Death-corrupted Marshlands) - SOUTHEAST

HIGH-TIER ZONES (Levels 35-50)
├── Astrae's Lost Observatories (Temporal Anomalies) - NORTHEAST
└── The Null Scars (Gronnak Dominion/Void Territory) - SOUTH

ENDGAME ZONES (Levels 50-60+)
├── Elar'Thalas Depths (Sealed City Core)
├── The Isle of Giants (World-shift Event Zone)
└── The Null Threshold (Progressive Raid Zone)
```

---

## Region Definitions

### 1. Worldroot Expanse
**Faction**: Verdant Circles (Sylvaen)  
**Level Range**: 1-15  
**Tier**: Starter  
**Worldroot Density**: Maximum

**Theme**: Ancient forest with visible Worldroot manifestations. Bioluminescent flora, living trees, and the Green Memory is tangible here.

**Key Locations**:
- Thornveil Enclave (Main Hub)
- Root's Embrace (Spawn Point)
- Whispering Canopy (Vertical Zone)
- Memory Groves (Questline Focus)

**Traversal**: Ground-level paths, root bridges, canopy platforms

---

### 2. Verdaniel's Continuity Reach
**Faction**: Ascendant League (High Elves)  
**Level Range**: 1-15 (Alternative Starter)  
**Tier**: Starter  
**Worldroot Density**: Weak

**Theme**: Temporal Steppes where time flows inconsistently. Frozen moments, phasing ruins, and Edit Node research stations.

**Key Locations**:
- Spire of Echoes (Main Hub)
- Temporal Steppes (Open Plains)
- Frozen Moment Fields (Time-stopped areas)
- Edit Node Research Site Alpha

**Traversal**: Reality tears allow short-distance teleportation; time-frozen platforms

---

### 3. Vael's Shattered Totem Wilds
**Faction**: Totem Clans (Therakai - Wildborn vs Pathbound)  
**Level Range**: 20-35  
**Tier**: Mid  
**Worldroot Density**: Moderate

**Theme**: Untamed wilderness split between instinct-driven chaos (Wildborn territory) and structured hunting grounds (Pathbound territory). Totem Spirit shrines throughout.

**Key Locations**:
- Vharos (Split City - PvP Hub)
- The Untamed Reaches (Wildborn)
- The Carved Valleys (Pathbound)
- Totem Shrine Network

**Traversal**: Beast paths, rope bridges, spirit-flight totems

---

### 4. Korrath's Warfront Marches
**Faction**: Contested (Multiple Factions)  
**Level Range**: 25-35  
**Tier**: Mid  
**Worldroot Density**: Moderate (Damaged)

**Theme**: War-torn battlefields where factions clash over divine fragments and strategic resources. Ruined fortifications and active combat zones.

**Key Locations**:
- Banner's Stand (Neutral Hub)
- The Scarred Battlefields
- Korrath's Broken Altar
- Forward Operating Bases (Faction-specific)

**Traversal**: Military roads, siege tunnels, supply lines

---

### 5. Nereth's Silent Fens
**Faction**: Void Compact (Outcasts)  
**Level Range**: 30-40  
**Tier**: Mid-High  
**Worldroot Density**: Absent (Corrupted)

**Theme**: Death-corrupted marshlands where Nereth's broken divine influence causes undead persistence and reality bleeds. Blackwake Haven operates here.

**Key Locations**:
- Blackwake Haven (Pirate City Hub)
- The Drowned Graves
- Nereth's Shattered Temple
- The Leviathan's Carcass

**Traversal**: Waterways, phase-shifted boats, reality anchors

---

### 6. Astrae's Lost Observatories
**Faction**: Seekers of Astrae (Neutral/Research)  
**Level Range**: 40-50  
**Tier**: High  
**Worldroot Density**: Inverted (Temporal Anomaly)

**Theme**: Ancient astronomical observatories where the missing god Astrae's influence remains. Time loops, future echoes, and knowledge caches.

**Key Locations**:
- The Stellar Vault (Hub)
- Observatory Ruins (Multiple Sites)
- The Temporal Labyrinth (Dungeon)
- Star Prism Collection Points

**Traversal**: Temporal bridges, observatory teleporters, constellation paths

---

### 7. The Null Scars
**Faction**: Dominion Warhost (Gronnak) / Void-Touched  
**Level Range**: 45-60  
**Tier**: High-Endgame  
**Worldroot Density**: Reality Scar (Damaged/Null)

**Theme**: Reality scar atop which Krag'Thuun stands. Gods can die permanently here. Null corruption visible. Dominion magic is raw and unstable.

**Key Locations**:
- Krag'Thuun (Gronnak Capital)
- The Scarred Highlands
- God-Corpse Sites
- Null Breach Points

**Traversal**: Iron roads, Dominion waypoints, reality tears (dangerous)

---

## Greybox Blocking Plan

### Cell System

The world uses a modular cell-based design for efficient construction and streaming:

**Cell Types**:
- **Hub Cells**: 500m x 500m, high detail, social spaces
- **Standard Cells**: 250m x 250m, standard detail
- **Wilderness Cells**: 500m x 500m, lower detail, open exploration
- **Dungeon Cells**: Variable, instanced or semi-instanced

### Region Cell Counts

| Region | Hub Cells | Standard Cells | Wilderness Cells | Dungeon Cells |
|--------|-----------|----------------|------------------|---------------|
| Worldroot Expanse | 2 | 12 | 8 | 3 |
| Verdaniel's Continuity Reach | 1 | 10 | 10 | 2 |
| Vael's Shattered Totem Wilds | 2 | 15 | 15 | 4 |
| Korrath's Warfront Marches | 3 | 12 | 8 | 3 |
| Nereth's Silent Fens | 2 | 10 | 12 | 4 |
| Astrae's Lost Observatories | 1 | 8 | 6 | 5 |
| The Null Scars | 2 | 14 | 10 | 6 |

### Vertical Ranges

**Elevation Strategy**: Each region utilizes vertical space to maximize playable area and create memorable landmarks.

- **Worldroot Expanse**: -20m to +80m (underground roots to canopy)
- **Verdaniel's Continuity Reach**: 0m to +40m (flat plains with floating ruins)
- **Vael's Shattered Totem Wilds**: -10m to +120m (valleys to mountain peaks)
- **Korrath's Warfront Marches**: 0m to +60m (battlefields with siege towers)
- **Nereth's Silent Fens**: -30m to +10m (underwater areas to shoreline)
- **Astrae's Lost Observatories**: +50m to +200m (elevated observatories)
- **The Null Scars**: +20m to +150m (elevated on reality scar)

### Sublevel Strategy

**Underground/Sublevel Zones**: Several regions feature extensive sublevel content:

1. **Worldroot Depths** (Under Worldroot Expanse): Root caverns, Memory Vaults
2. **Edit Node Network** (Under Verdaniel's Reach): High Elf research facilities
3. **Krag'Thuun Depths** (Under The Null Scars): God-corpse chambers, reality scar core
4. **The Drowned Layer** (Under Nereth's Silent Fens): Oceanic memory layer access

---

## Traversal System

### Traversal Lanes

Each region features multiple traversal methods to enhance player mobility and world navigation:

#### Ground Lanes
- **Roads**: Fast travel paths between hubs (mount speed +100%)
- **Trails**: Standard paths through wilderness (mount speed +50%)
- **Wilderness**: Off-path exploration (mount speed +0%)

#### Special Traversal
- **Root Bridges** (Worldroot Expanse): Elevated natural pathways
- **Time Rifts** (Verdaniel's Continuity Reach): Short-range teleportation
- **Spirit Flight Totems** (Vael's Shattered Totem Wilds): Aerial transport between totems
- **Military Supply Routes** (Korrath's Warfront Marches): Faction-specific fast travel
- **Phase Boats** (Nereth's Silent Fens): Waterway navigation
- **Stellar Bridges** (Astrae's Lost Observatories): Constellation-based teleportation
- **Dominion Waypoints** (The Null Scars): Corrupted fast travel (risky)

### Fast Travel Hubs

**Hub Network**: Each region has 1-3 major hubs with fast travel points:

1. **Thornveil Enclave** (Worldroot Expanse)
2. **Spire of Echoes** (Verdaniel's Continuity Reach)
3. **Vharos** (Vael's Shattered Totem Wilds) - Split between factions
4. **Banner's Stand** (Korrath's Warfront Marches)
5. **Blackwake Haven** (Nereth's Silent Fens)
6. **The Stellar Vault** (Astrae's Lost Observatories)
7. **Krag'Thuun** (The Null Scars) - Faction-restricted

**Travel Requirements**: 
- Must discover location before fast travel unlocks
- Some hubs require faction reputation or quest completion
- PvP zones have limited/risky fast travel options

---

## Questline Skeleton & Gating

### Main Story Arc

The main questline progresses through all regions, gated by key items representing each area's thematic struggle:

#### Progression Gating Items

1. **Root Sigil** (Worldroot Expanse, Levels 1-15)
   - **Quest**: "The Bleeding Root"
   - **Acquisition**: Seal corrupted Worldroot breach
   - **Grants**: Access to Verdaniel's Continuity Reach

2. **Seed Sigil** (Verdaniel's Continuity Reach, Levels 15-20)
   - **Quest**: "Echoes of the Severance"
   - **Acquisition**: Recover High Elf memory archive
   - **Grants**: Access to Vael's Shattered Totem Wilds

3. **Totem Shard** (Vael's Shattered Totem Wilds, Levels 20-30)
   - **Quest**: "The War of Instinct"
   - **Acquisition**: Unite or decisively influence Therakai conflict
   - **Grants**: Access to Korrath's Warfront Marches

4. **Banner Commendation** (Korrath's Warfront Marches, Levels 25-35)
   - **Quest**: "The God of War's Silence"
   - **Acquisition**: Prove worth in factional warfare
   - **Grants**: Access to Nereth's Silent Fens

5. **Veil Token** (Nereth's Silent Fens, Levels 30-40)
   - **Quest**: "Beyond Death's Veil"
   - **Acquisition**: Survive Void exposure in Blackwake Haven
   - **Grants**: Access to Astrae's Lost Observatories

6. **Star Prism** (Astrae's Lost Observatories, Levels 40-50)
   - **Quest**: "The Missing God"
   - **Acquisition**: Solve temporal puzzles and witness Astrae's capture
   - **Grants**: Access to The Null Scars

7. **Null Anchor Kit** (The Null Scars, Levels 50-60)
   - **Quest**: "Into the Scar"
   - **Acquisition**: Survive reality-scar exposure and obtain anchor
   - **Grants**: Access to Endgame Zones (Elar'Thalas, Isle of Giants, Null Threshold)

### Side Quest Chains

Each region features 3-5 major side quest chains:

- **Reputation Chains**: Build standing with regional factions
- **Exploration Chains**: Discover landmarks and hidden locations
- **Collection Chains**: Gather region-specific materials
- **Lore Chains**: Uncover regional history and cosmic truths
- **Challenge Chains**: Complete difficult encounters or puzzles

---

## Landmark System

### Major Landmarks (Per Region)

Each region includes 5-10 major landmarks visible from a distance and serving as navigation aids:

**Worldroot Expanse**:
- The Great Root (Massive central Worldroot trunk)
- Memory Grove Circle
- Thornveil Canopy
- Elder Tree (Ancient Sylvaen gathering place)

**Verdaniel's Continuity Reach**:
- Spire of Echoes (Twisted crystalline tower)
- The Frozen Moment (Time-stopped battlefield)
- Edit Node Alpha (Shimmering reality tear)
- Temporal Falls (Water frozen mid-fall)

**Vael's Shattered Totem Wilds**:
- Vharos Split City
- The Five Great Totems (Massive spirit manifestations)
- Primal Peak (Highest mountain)
- The Carved Valleys

**Korrath's Warfront Marches**:
- Banner's Stand (Fortress)
- Korrath's Broken Altar (Ruined temple)
- The Scarred Battlefields (Massive crater)
- War Memorial Stones

**Nereth's Silent Fens**:
- Blackwake Haven (Pirate city)
- The Leviathan's Skeleton (Massive remains)
- Nereth's Shattered Temple (Broken ziggurat)
- The Drowned Lighthouse

**Astrae's Lost Observatories**:
- The Stellar Vault (Main observatory)
- The Celestial Compass (Ancient device)
- Star Prism Fields (Crystalline formations)
- Time Loop Tower (Endlessly repeating structure)

**The Null Scars**:
- Krag'Thuun (Iron citadel)
- God-Corpse Crater (Where gods died)
- The Reality Tear (Visible Null breach)
- Witch-King Throne (Exposed throne atop scar)

---

## Zone Interconnection

### Region Borders

Regions connect through natural transition zones:

- **Worldroot Expanse ↔ Verdaniel's Continuity Reach**: Forest thins, reality becomes unstable
- **Worldroot Expanse ↔ Korrath's Warfront Marches**: Corrupted forest edge, war damage
- **Vael's Shattered Totem Wilds ↔ Korrath's Warfront Marches**: Mountain passes, contested territories
- **Korrath's Warfront Marches ↔ The Null Scars**: Ascending to reality scar plateau
- **Nereth's Silent Fens ↔ The Null Scars**: Coastal approach to highlands
- **Astrae's Lost Observatories**: Isolated (requires teleportation via Star Prism)

### Phased Content

Certain areas phase based on story progression:

- **Elar'Thalas**: Sealed until late-game quest completion
- **Isle of Giants**: Appears/shifts location during world events
- **Null Threshold**: Only accessible after Null Anchor Kit acquisition

---

## Design Principles

### Lore-First Design
Every geographic feature has lore justification from WORLD_LORE.md.

### Non-Linear Progression
After initial zones, players choose exploration order based on preference and faction alignment.

### Vertical Gameplay
Utilize elevation and sublevels to maximize playable space and create memorable moments.

### Environmental Storytelling
World design communicates lore without requiring dialogue.

### Faction Territories
Clear visual and mechanical differentiation between faction-controlled regions.

---

## Implementation Priorities

### Phase 1: Starter Zones (MVP)
- Worldroot Expanse (primary)
- Verdaniel's Continuity Reach (alternative)

### Phase 2: Mid-Tier Expansion
- Vael's Shattered Totem Wilds
- Korrath's Warfront Marches

### Phase 3: High-Tier Content
- Nereth's Silent Fens
- Astrae's Lost Observatories

### Phase 4: Endgame
- The Null Scars
- Elar'Thalas Depths
- Isle of Giants
- The Null Threshold

---

## Conclusion

This world map design provides a comprehensive framework for building World of Eldara's open world. Each region serves distinct gameplay, narrative, and thematic purposes while maintaining cohesion through shared lore foundations and interconnected systems.

All subsequent development should reference this document alongside WORLD_LORE.md to ensure consistency and quality.
