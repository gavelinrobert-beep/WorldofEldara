# World of Eldara - Interactive Map Component

This directory contains the interactive React map component for World of Eldara, providing a visual representation of the game world's regions, quest arcs, encounters, and monsters.

## Component Overview

**EldaraWorldMap.tsx** - A comprehensive interactive map component built with React and TypeScript that displays:
- Seven major regions with consistent naming and color coding
- Quest arcs with gating items and level ranges
- Major hubs (cities and settlements)
- Boss encounters with mechanics
- Monster families and elite creatures
- Interactive selection and detailed information panels

## Features

### Interactive Map
- Click on any region to view detailed information
- Visual representation of region connections
- Hub locations marked with blue circles
- Color-coded regions by tier (Starter, Mid, High, Endgame)

### Information Tabs
- **Overview**: Region hubs and key locations
- **Quests**: Quest arcs, gating items, and progression
- **Bosses**: Major encounters with mechanics breakdown
- **Monsters**: Monster families, types, and abilities

### Data Included

#### Regions (Consistent Naming)
1. **Worldroot Expanse** (Levels 1-15)
2. **Verdaniel's Continuity Reach** (Levels 15-20)
3. **Vael's Shattered Totem Wilds** (Levels 20-35)
4. **Korrath's Warfront Marches** (Levels 25-35)
5. **Nereth's Silent Fens** (Levels 30-40)
6. **Astrae's Lost Observatories** (Levels 40-50)
7. **The Null Scars** (Levels 45-60)

#### Gating Items
- Root Sigil
- Seed Sigil
- Totem Shard
- Banner Commendation
- Veil Token
- Star Prism
- Null Anchor Kit

## Dependencies

```json
{
  "react": "^18.0.0",
  "lucide-react": "^0.263.0"
}
```

## Installation

To use this component in a React application:

1. Install dependencies:
```bash
npm install react lucide-react
```

2. Import the component:
```tsx
import EldaraWorldMap from './docs/ui/EldaraWorldMap';
```

3. Use in your application:
```tsx
function App() {
  return <EldaraWorldMap />;
}
```

## Usage in Different Contexts

### Standalone Web Application
Create an `index.html` and mount the component:
```tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import EldaraWorldMap from './EldaraWorldMap';
import './index.css'; // Include Tailwind CSS

const root = ReactDOM.createRoot(document.getElementById('root')!);
root.render(<EldaraWorldMap />);
```

### Integration with Unreal Engine
While this is a React component, you can:
1. Build it as a standalone web view
2. Embed it in Unreal's UMG Web Browser widget
3. Use it as a reference for native UI implementation

### Documentation Site
Include in a documentation site (e.g., Docusaurus, VitePress):
```tsx
import EldaraWorldMap from '@site/docs/ui/EldaraWorldMap';

<EldaraWorldMap />
```

## Styling Requirements

The component uses Tailwind CSS classes. If not using Tailwind, you'll need to:

1. **Add Tailwind CSS**:
```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

2. **Configure tailwind.config.js**:
```js
module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}"],
  theme: { extend: {} },
  plugins: [],
}
```

3. **Add to your CSS**:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

Alternatively, you can convert the Tailwind classes to regular CSS.

## Data Structure

The component uses TypeScript interfaces for type safety:

```typescript
interface Region {
  id: string;
  name: string;
  levelRange: string;
  tier: string;
  faction: string;
  position: { x: number; y: number };
  description: string;
  color: string;
}

interface QuestArc {
  id: string;
  name: string;
  regionId: string;
  questLine: string;
  gatingItem: string;
  levelRange: string;
}

// ... additional interfaces for Hub, Monster, Encounter
```

## Customization

### Adding New Regions
Add to the `regions` array:
```typescript
{
  id: 'new-region',
  name: 'New Region Name',
  levelRange: '60-70',
  tier: 'Endgame',
  faction: 'Faction Name',
  position: { x: 30, y: 50 },
  description: 'Region description',
  color: '#hexcolor'
}
```

### Adding New Encounters
Add to the `encounters` array:
```typescript
{
  id: 'new-boss',
  name: 'Boss Name',
  regionId: 'region-id',
  bossType: 'Raid Boss',
  level: 50,
  mechanics: ['Mechanic 1', 'Mechanic 2']
}
```

### Modifying the Map Layout
Adjust the SVG viewBox or region positions:
```typescript
position: { x: 50, y: 50 } // Values 0-100 represent % of map
```

## Alignment with Documentation

This component visualizes data from:
- **WORLD_LORE.md**: Canonical lore and world structure
- **docs/world_map_design.md**: Region layout and traversal systems
- **docs/encounter_tables.md**: Monster families and boss encounters

All region names, level ranges, and gating items are consistent with these documents.

## Future Enhancements

Potential additions to consider:
- Zoom and pan functionality
- More detailed sub-region maps
- Quest path visualization
- Player location tracking (for game integration)
- Dynamic data loading from API/database
- Animated transitions between regions
- 3D terrain representation
- Real-time world event indicators

## Lore Consistency

All content in this component aligns with the canonical lore defined in **WORLD_LORE.md**:
- Region names and descriptions
- Faction territories
- Worldroot density references
- Divine influence areas
- Progression gating tied to narrative

## License & Credits

Part of the World of Eldara project. All content is consistent with the game's lore and design documents.

## Support

For questions or issues with the component, refer to:
- Main documentation in `/docs`
- WORLD_LORE.md for canonical reference
- PROJECT_STRUCTURE.md for repository organization
