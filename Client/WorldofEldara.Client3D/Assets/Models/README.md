# Eldara 3D Model Intake

Drop low-poly game assets into these folders and register them in `model-manifest.json`.

Recommended budgets for the current prototype:

- Small props: 500-3,000 triangles
- Repeatable terrain tiles: 500-3,500 triangles
- Repeatable trees: 2,000-10,000 triangles
- Hero trees or landmarks: 15,000-30,000 triangles
- Creatures: 4,000-12,000 triangles
- Player/NPC placeholders: 5,000-15,000 triangles

Preferred GLB export:

- One asset per file
- Y-up if possible
- Mesh centered at world origin with feet/base on ground
- Normals included
- UVs included
- One or two materials
- 1024px textures for repeated props, 2048px for hero props

Avoid for now:

- Multi-million triangle Meshy source meshes
- Huge single-scene GLBs
- Missing normals/UVs
- Embedded 4K+ textures on repeatable objects

## Current Thornveil Kit

The first Blender-authored terrain kit lives in `Terrain/`:

- `terrain-grass-tile.low.glb`
- `terrain-dirt-path-straight.low.glb`
- `terrain-dirt-path-curve.low.glb`
- `terrain-stone-plaza.low.glb`
- `terrain-moss-edge.low.glb`
- `terrain-water-pool.low.glb`

Editable source: `../Source/ThornveilTerrainKit.blend`.

These are intentionally small, flat-shaded low-poly pieces. The runtime still uses the custom C# renderer today, so these files are the asset source of truth for the next importer/proxy pass rather than the final in-game mesh path.

The first landmark kit lives in `Landmarks/`:

- `thornveil-treehouse.low.glb`
- `root-bridge-stairs.low.glb`
- `cyan-crystal-cluster.low.glb`
- `great-root-arch.low.glb`

Editable source: `../Source/ThornveilLandmarkKit.blend`.

The 3D client also has matching temporary C# proxy meshes for treehouses, root bridges, and crystal clusters so the visual direction is testable before the runtime has full GLB rendering.
