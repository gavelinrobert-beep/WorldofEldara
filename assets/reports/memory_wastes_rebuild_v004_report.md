# Memory Wastes Hard Rebuild v004 Report

## Scope
- Scene: The Memory Wastes
- Build: Hard Rebuild v004
- Output blend: C:\Users\Admin\Documents\New project\WorldofEldara-analysis\assets\blender\memory_wastes_rebuild_v004.blend
- Output GLB: C:\Users\Admin\Documents\New project\WorldofEldara-analysis\assets\exports\memory_wastes_rebuild_v004.glb
- Gameplay render: C:\Users\Admin\Documents\New project\WorldofEldara-analysis\assets\renders\memory_wastes_rebuild_v004_gameplay_2560x1440.png
- Top-down render: C:\Users\Admin\Documents\New project\WorldofEldara-analysis\assets\renders\memory_wastes_rebuild_v004_topdown_2560x1440.png
- Manifest: C:\Users\Admin\Documents\New project\WorldofEldara-analysis\assets\reports\memory_wastes_rebuild_v004_manifest.json

## What Changed
- The old compact arena layout was not preserved.
- The scene was rebuilt around a 120m x 90m visual footprint with separated islands and visible void gaps.
- New role collections were created: MW_PLAYER_SAFE_CAMP, MW_WORLDROOT_RIFT, MW_LOST_RUINS, MW_DEAD_GOD_SHRINE, MW_ECHO_BATTLEFIELD, MW_TRAVERSAL, MW_BACKGROUND_FRAGMENTS, MW_ATMOSPHERE, MW_LIGHTING, MW_CAMERA.
- The Worldroot Rift is now the largest and tallest landmark, with a split island, root nerves, lifted plates, cyan/violet/gold light and a vertical energy column above 15m.
- Lost Ruins, Dead God Shrine and Echo Battlefield each received distinct silhouettes and color logic.
- Background floating fragments were added for scale and atmospheric depth.

## Required Answers
1. Did you rebuild the Memory Wastes layout instead of making minor edits?
   Yes. This v004 script clears the scene and creates a new large archipelago layout rather than editing the old compact v002 platform arrangement.

2. What objects/collections from the old compact layout were deleted or replaced?
   The rebuild starts from a cleared Blender scene, so old v002 compact arena objects, random marker clusters, board-like test disks, older island placements and unassigned placeholder props are not carried into the v004 output. They are replaced by the MW_* role collections listed above.

3. Is the Worldroot Rift the largest and strongest focal point?
   Yes. MW_WORLDROOT_RIFT is the largest island, sits in far-mid center at approximately (0, 25, 5), and has the strongest vertical silhouette and light hierarchy.

4. Which island is the Safe Camp?
   MW_PLAYER_SAFE_CAMP at approximately (0, -35, 0). It is small, low, stable and foreground-facing.

5. Which island is the Lost Civilization Ruins?
   MW_LOST_RUINS at approximately (-35, 10, 8). It uses a large broken arch, floating columns, stair fragments and ghost-white/cyan material.

6. Which island is the Dead God Shrine?
   MW_DEAD_GOD_SHRINE at approximately (35, 5, 9). It uses a circular isolated island, cracked halo, broken altar, missing-name slab and pale gold divine light.

7. Which island is the Echo Battlefield?
   MW_ECHO_BATTLEFIELD at approximately (-25, 35, 3). It is flatter and scarred with embedded spears, broken shields, ghost banners and spectral soldier silhouettes.

8. Are the islands separated by meaningful void space?
   Yes. The major islands are tens of meters apart, with incomplete traversal routes and visible open void between them.

9. Are there foreground, midground and background layers?
   Yes. Safe Camp and player are foreground, Lost Ruins and Dead God Shrine are midground, the Worldroot Rift is far-mid dominant, and distant background fragments extend behind the landmarks.

10. What still reads as placeholder?
   Character silhouettes are still placeholders. Terrain and traversal are visual blockout geometry, not final collision-ready meshes. Materials are controlled stylized color direction rather than final painted textures.

## Color Hierarchy
- Cyan = Worldroot memory.
- Violet = instability / rift danger.
- Pale gold = dead god / divine remnants.
- Ghost white-blue = echoes / lost civilization.
- Muted gray-brown-green = terrain.
- Warm green/gold = Sylvaen safe camp only.

## Technical QA
- Collections: 10
- Objects: 232
- Approximate triangles: 3203
- Default primitive names: 0
- Non-MAT materials: 0
