# Eldara World Map Component

An interactive React component displaying the World of Eldara progression map with full lore integration.

## Features

- **Full Region Labels**: All region names displayed without apostrophe truncation
- **Progression Paths**: Visual representation of parallel progression options
- **Level Ranges**: Clear tier/level indicators for each region
- **Traversal Unlocks**: Badge showing required unlock item for each region
- **Lore Hooks**: Short lore descriptions for each region from canonical world documents
- **Interactive Tooltips**: Hover to see detailed information (tier, level, hub, unlock)
- **Focus Selection**: Click regions to focus (de-emphasizes others with opacity/desaturation)

## Regions

1. **Worldroot** (1-10, Starter) - Root bridges unlock
   - Hub: Thornveil Enclave
   - Lore: Planetary nervous system strain and memory wells

2. **Verdaniel** (15-35, Mid-Level) - Seed Sigil unlock
   - Hub: Memory Wastes
   - Lore: Ancient seed vaults with extinct civilization memories

3. **Vael** (25-45, Mid-Level) - Totem Shard/Wind lanes unlock
   - Hub: Vharos Warfront
   - Lore: Fractured totem spirits from shattered god

4. **Korrath** (35-60, High-Level) - Banner Commendation unlock
   - Hub: Scarred Highlands
   - Lore: Warfront with god-eaters and divine blood

5. **Nereth** (35-60, High-Level) - Veil Token unlock
   - Hub: Temporal Maze
   - Lore: Broken god of death creating undeath

6. **Astrae** (50-65, Endgame) - Star Prism unlock
   - Hub: The Isle of Giants
   - Lore: Missing observatories and trapped god of knowledge

7. **Null Scars** (60+, Endgame) - Null Anchor Kit unlock
   - Hub: The Null Threshold
   - Lore: Where reality meets erasure

## Progression Flow

```
Worldroot (1-10)
    ├─→ Verdaniel (15-35) ──┐
    └─→ Vael (25-45) ───────┤
                             ├─→ Korrath (35-60) ──┐
                             └─→ Nereth (35-60) ────┤
                                                     ├─→ Astrae (50-65) ──→ Null Scars (60+)
```

## Usage

### As React Component

```tsx
import EldaraWorldMap from './EldaraWorldMap';

function App() {
  return <EldaraWorldMap />;
}
```

### Standalone Demo

Open `index.html` in a browser to see the interactive map.

## Dependencies

- React 18.2+
- lucide-react 0.263+ (for icons, though currently using emoji for compatibility)

## Technical Details

- **Export**: Default export
- **Type**: React Functional Component
- **Styling**: Inline styles for portability (no external CSS dependencies)
- **SVG-based**: Scalable vector graphics for regions and paths
- **Responsive**: Adapts to container width

## Lore Alignment

All region data, level ranges, traversal unlocks, and lore hooks are consistent with:
- WORLD_LORE.md (canonical lore document)
- Shared/WorldofEldara.Shared/Data/World/Zone.cs (zone definitions)
- Game progression design

## Integration

This component can be integrated into:
- Game website/wiki
- Character progression guides
- Admin/developer tools
- In-game UI (with appropriate styling adjustments)
