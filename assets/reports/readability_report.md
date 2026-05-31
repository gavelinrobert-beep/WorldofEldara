# Thornveil Readability, Material And Grounding Report

## Scope
- Scene: Heartbough Glade Entrance / Thornveil Enclave
- Camera: preserved
- Player spawn: preserved
- Main path axis: preserved
- Village layout and building/platform placement: preserved
- Scene bounds: preserved

## Pass Summary
- Added clearer material groups for bark, dark roots, leaves, path, moss ground, wood trim, Worldroot cyan, lantern gold and Verdant banners.
- Strengthened the main route with a warm mossy-stone value band and larger readable path steps without blocking the walkable center.
- Added dark contact shadows, moss rings, bridge anchor supports and crystal stone/root nests to ground existing objects.
- Reduced floating/debug-like marker scale so cyan reads as Worldroot, windows, bark cracks, route runes and memory markers.
- Added dark-green background value layers and distant trunk breaks to make the village feel enclosed by forest depth.
- Replaced the cube-body character read with tapered low-poly Sylvaen placeholder silhouettes.

## Required Answers
1. Is the main path readable within 3 seconds?
   Yes. The path now has 13 additional readability pieces: a continuous mossy-stone value band, larger warm-gray stone steps, and existing root borders. The center lane is still clear.

2. Are tree, house, bridge, terrain and path materials visually separated?
   Yes. The pass uses distinct material groups: MAT_BARK_MAIN for trunks, MAT_BARK_DARK_ROOT for roots/bridges, MAT_LEAVES_CANOPY_DEEP/LIGHT for foliage, MAT_PATH_MOSSYSTONE for walkable slabs, MAT_GROUND_MOSS for terrain, MAT_ARCH_WOOD_TRIM for trim/rails, MAT_WORLDROOT_CYAN for meaningful magic, MAT_LANTERN_WARM for lanterns, and MAT_BANNER_VERDANT for banners.

3. Are all major props grounded?
   Mostly yes. Added 25 new readability grounding objects plus the existing GROUNDING_* terrain integration pieces. Trees/treehouses have moss rings/contact shadows, bridges have root support anchors, crystals sit in stone/root nests, and poles already have stone/root bases.

4. Was cyan reduced to meaningful Worldroot uses?
   Yes. Cyan is kept for crystals, windows, bark cracks, route runes and memory markers. Floating marker-like objects scaled down this pass: 12.

5. Does the scene still preserve the original layout?
   Yes. The camera, player position, central village, treehouse positions, platform/bridge network, and main path direction were not changed.

6. What still reads as placeholder?
   Characters remain simple scale placeholders, terrain is still Blender visual terrain rather than final client collision/navmesh, and the material direction is clean stylized color separation rather than authored hand-painted texture maps.

## Technical Checks
- Required material groups missing: none
- Readability objects added: 49
- Main path readability objects: 13
- Grounding readability objects: 25
- Forest depth readability objects: 11
- Default primitive names: 0
- Non-MAT material names: 0
- Total objects: 1539
- Approximate triangles: 34750
