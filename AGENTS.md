# World of Eldara — Codex Instructions

## Project Identity

World of Eldara is a stylized fantasy MMORPG prototype.

The target visual style is:
- stylized low-poly / hand-painted MMORPG
- readable silhouettes
- saturated but controlled color
- chunky shapes
- strong third-person gameplay readability
- magical atmosphere without photorealism

Avoid:
- photorealistic Unreal-style output
- generic medieval fantasy
- over-detailed procedural noise
- thin unreadable props
- unorganized Blender files
- objects named Cube, Plane, Cylinder, etc.

## Blender Asset Rules

When creating Blender scenes or assets:

1. Use metric scale.
2. 1 Blender unit = 1 meter.
3. Use named collections.
4. Use named materials with `MAT_` prefix.
5. Use named objects with category prefixes:
   - ENV_
   - ARCH_
   - PROP_
   - FOL_
   - CHAR_
   - FX_
   - LIGHT_
   - CAM_
6. Every asset should have clean origin placement.
7. Major props should be modular and reusable.
8. Prefer simple stylized geometry over high-poly realism.
9. Use emission materials only for magical crystals, runes, lanterns and memory effects.
10. Always create a render and a quality report.

## Render Rules

For in-game mockup renders:
- Use a third-person camera.
- Use a clear foreground, mid-ground and background.
- Show a navigable player path.
- Use soft heroic fantasy lighting.
- Use readable silhouettes.
- Avoid depth-of-field blur unless explicitly requested.
- Output both 2560x1440 and 1920x1080 renders.

## Thornveil Enclave Visual Rules

Thornveil Enclave is the Sylvaen starting zone.

Core ingredients:
- ancient living forest
- giant Worldroot trees
- treehouses grown into trunks
- root bridges
- mossy stone paths
- cyan/turquoise Worldroot crystals
- green Verdant banners
- warm lanterns
- magical but safe first-zone atmosphere
- subtle Memory Echo markers
- hints of mystery, not horror

Palette:
- deep leaf green
- moss green
- warm bark brown
- mossy stone gray
- golden trim
- cyan/turquoise magic glow
- small purple/blue flower accents

Forbidden:
- generic medieval cottages
- realistic scanned trees
- horror forest as the main mood
- gray/brown muddy palette
- thin bridges or tiny details that cannot be read from gameplay camera

## Quality Bar

A scene is not complete until:
- it reads correctly from the camera within 3 seconds
- the main path is obvious
- the composition has foreground, mid-ground and background
- every major object has a meaningful name
- materials are named and reusable
- exported .blend and .glb files exist
- render outputs exist
- a manifest and quality report exist