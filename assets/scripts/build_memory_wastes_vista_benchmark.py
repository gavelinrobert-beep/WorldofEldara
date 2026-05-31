import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.append(str(SCRIPT_DIR))

from build_eldara_zone_progression_scenes import (  # noqa: E402
    add_text,
    bicone_mesh,
    cube_obj,
    disable_shadow,
    disc_mesh,
    frustum_obj,
    look_at,
    render_to,
    setup_scene_settings,
    tri_prism_obj,
    vertical_poly_obj,
    xz_beam_obj,
)
from build_heartbough_glade_entrance import (  # noqa: E402
    ROOT,
    clear_scene,
    irregular_slab_mesh,
    make_mat,
    object_triangles,
)


STEM = "memory_wastes_vista_benchmark_v001"
OUT_BLEND = ROOT / "assets" / "blender" / f"{STEM}.blend"
OUT_GLB = ROOT / "assets" / "exports" / f"{STEM}.glb"
OUT_RENDER = ROOT / "assets" / "renders" / "memory_wastes_vista_benchmark_render_2560x1440.png"
OUT_TOPDOWN = ROOT / "assets" / "renders" / "memory_wastes_vista_benchmark_topdown.png"
OUT_REPORT = ROOT / "assets" / "reports" / "memory_wastes_vista_benchmark_report.md"
OUT_MANIFEST = ROOT / "assets" / "reports" / "memory_wastes_vista_benchmark_manifest.json"


COLLECTION_NAMES = [
    "VISTA_Foreground_SafeCamp",
    "VISTA_Midground_LostCivilizationRuins",
    "VISTA_Midground_DeadGodShrine",
    "VISTA_FarMid_WorldrootSheddingRift",
    "VISTA_Background_MemoryArchipelago",
    "VISTA_Traversal_MemoryBridges",
    "VISTA_Characters",
    "VISTA_LightingRender",
]


def ensure_dirs():
    for path in [OUT_BLEND.parent, OUT_GLB.parent, OUT_RENDER.parent, OUT_REPORT.parent]:
        path.mkdir(parents=True, exist_ok=True)


def create_collections():
    collections = {}
    for name in COLLECTION_NAMES:
        col = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(col)
        collections[name] = col
    return collections


def vista_materials():
    return {
        "MAT_Vista_TerrainTop_MutedMoss": make_mat("MAT_Vista_TerrainTop_MutedMoss", (0.22, 0.28, 0.22)),
        "MAT_Vista_TerrainSide_DarkStone": make_mat("MAT_Vista_TerrainSide_DarkStone", (0.08, 0.09, 0.10)),
        "MAT_Vista_TerrainUnderside_Void": make_mat("MAT_Vista_TerrainUnderside_Void", (0.025, 0.030, 0.040)),
        "MAT_Vista_RootDark": make_mat("MAT_Vista_RootDark", (0.18, 0.09, 0.045)),
        "MAT_Vista_RootWarm": make_mat("MAT_Vista_RootWarm", (0.37, 0.20, 0.08)),
        "MAT_Vista_SafeCampGreen": make_mat("MAT_Vista_SafeCampGreen", (0.08, 0.33, 0.18)),
        "MAT_Vista_SafeGlow": make_mat(
            "MAT_Vista_SafeGlow", (0.08, 0.52, 0.44), emission=(0.02, 0.38, 0.33), strength=0.45, alpha=0.34
        ),
        "MAT_Vista_BannerVerdant": make_mat("MAT_Vista_BannerVerdant", (0.04, 0.30, 0.16)),
        "MAT_Vista_CampGold": make_mat(
            "MAT_Vista_CampGold", (0.86, 0.62, 0.20), emission=(0.55, 0.34, 0.08), strength=0.20
        ),
        "MAT_Vista_CyanMemory": make_mat(
            "MAT_Vista_CyanMemory", (0.05, 0.67, 0.82), emission=(0.02, 0.64, 0.90), strength=1.55, alpha=0.72
        ),
        "MAT_Vista_CyanDim": make_mat(
            "MAT_Vista_CyanDim", (0.08, 0.30, 0.36), emission=(0.02, 0.18, 0.22), strength=0.24, alpha=0.40
        ),
        "MAT_Vista_VioletInstability": make_mat(
            "MAT_Vista_VioletInstability", (0.26, 0.06, 0.46), emission=(0.30, 0.03, 0.62), strength=0.92, alpha=0.50
        ),
        "MAT_Vista_GhostStone": make_mat(
            "MAT_Vista_GhostStone", (0.68, 0.84, 0.78), emission=(0.18, 0.34, 0.30), strength=0.30, alpha=0.62
        ),
        "MAT_Vista_GhostBlue": make_mat(
            "MAT_Vista_GhostBlue", (0.55, 0.82, 1.0), emission=(0.18, 0.52, 0.90), strength=0.72, alpha=0.48
        ),
        "MAT_Vista_DeadStone": make_mat("MAT_Vista_DeadStone", (0.34, 0.34, 0.32)),
        "MAT_Vista_PaleGold": make_mat(
            "MAT_Vista_PaleGold", (0.96, 0.72, 0.28), emission=(0.95, 0.58, 0.10), strength=1.15, alpha=0.74
        ),
        "MAT_Vista_BattleStain": make_mat(
            "MAT_Vista_BattleStain", (0.13, 0.18, 0.22), emission=(0.03, 0.08, 0.11), strength=0.12, alpha=0.52
        ),
        "MAT_Vista_DistantIsland": make_mat(
            "MAT_Vista_DistantIsland", (0.10, 0.13, 0.18), emission=(0.03, 0.04, 0.08), strength=0.08, alpha=0.54
        ),
        "MAT_Vista_DistantRuin": make_mat(
            "MAT_Vista_DistantRuin", (0.42, 0.50, 0.52), emission=(0.10, 0.16, 0.18), strength=0.14, alpha=0.42
        ),
        "MAT_Vista_BackdropBlueViolet": make_mat(
            "MAT_Vista_BackdropBlueViolet", (0.055, 0.075, 0.13), emission=(0.025, 0.035, 0.085), strength=0.10, alpha=0.14
        ),
        "MAT_Vista_BackdropMist": make_mat(
            "MAT_Vista_BackdropMist", (0.16, 0.20, 0.29), emission=(0.035, 0.045, 0.08), strength=0.06, alpha=0.20
        ),
        "MAT_Vista_Player": make_mat("MAT_Vista_Player", (0.52, 0.72, 0.58), alpha=0.92),
        "MAT_Vista_Npc": make_mat("MAT_Vista_Npc", (0.76, 0.74, 0.56), alpha=0.86),
    }


def export_scene_glb(collections, out_glb):
    bpy.ops.object.select_all(action="DESELECT")
    for collection in collections.values():
        for obj in collection.objects:
            obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(out_glb), export_format="GLB", use_selection=True)


def make_floating_island(prefix, collection, mats, loc, rx, ry, height, top_mat_key="MAT_Vista_TerrainTop_MutedMoss", rot=0.0):
    x, y, z = loc
    island = irregular_slab_mesh(
        f"{prefix}_IrregularFloatingIslandTop",
        collection,
        (x, y, z),
        rx,
        ry,
        0.52,
        mats[top_mat_key],
        sides=11,
        rot_z=math.radians(rot),
        irregularity=0.20,
    )
    disc_mesh(
        f"{prefix}_MutedMossSkin_AsymmetricalMemorySurface",
        collection,
        (x + rx * 0.05, y - ry * 0.02, z + 0.30),
        rx * 0.68,
        ry * 0.52,
        mats["MAT_Vista_SafeCampGreen"] if "SafeCamp" in prefix else mats["MAT_Vista_TerrainTop_MutedMoss"],
        sides=9,
        rot=(0, 0, math.radians(rot + 11)),
    )
    underside = frustum_obj(
        f"{prefix}_TaperedUnderside_HangingMemoryMass",
        collection,
        (x, y, z - height * 0.47),
        0.30,
        1.0,
        height,
        9,
        mats["MAT_Vista_TerrainUnderside_Void"],
        rot=(0, 0, math.radians(rot)),
    )
    underside.scale.x = rx * 0.82
    underside.scale.y = ry * 0.82
    disable_shadow(underside)
    for i, (ox, oy, sx, sy) in enumerate([(-0.42, -0.44, 0.24, 0.10), (0.08, -0.54, 0.20, 0.12), (0.52, -0.38, 0.18, 0.09)]):
        cube_obj(
            f"{prefix}_BrokenLowerStrata_DarkMemoryEdge_{i:02d}",
            collection,
            (x + ox * rx, y + oy * ry, z - 0.24 - i * 0.06),
            (rx * sx, ry * sy, 0.22),
            mats["MAT_Vista_TerrainSide_DarkStone"],
            rot=(math.radians(2), 0, math.radians(rot + i * 13)),
        )
    return island


def make_character(prefix, collection, mats, loc, mat_key="MAT_Vista_Npc", scale=1.0, staff=False):
    x, y, z = loc
    mat = mats[mat_key]
    frustum_obj(f"{prefix}_BodyCloak", collection, (x, y, z + 0.78 * scale), 0.30 * scale, 0.22 * scale, 1.18 * scale, 6, mat)
    frustum_obj(f"{prefix}_Head", collection, (x, y, z + 1.55 * scale), 0.20 * scale, 0.18 * scale, 0.30 * scale, 9, mat)
    cube_obj(f"{prefix}_ShoulderSilhouette", collection, (x, y, z + 1.18 * scale), (0.80 * scale, 0.14 * scale, 0.18 * scale), mat)
    if staff:
        cube_obj(
            f"{prefix}_StaffOrSpearScaleLine",
            collection,
            (x + 0.45 * scale, y - 0.03, z + 0.90 * scale),
            (0.055 * scale, 0.055 * scale, 1.70 * scale),
            mats["MAT_Vista_RootWarm"],
            rot=(math.radians(7), 0, math.radians(-8)),
        )


def make_safe_camp(collections, mats):
    col = collections["VISTA_Foreground_SafeCamp"]
    chars = collections["VISTA_Characters"]
    make_floating_island("SAFE_CAMP_ForegroundLowFoothold", col, mats, (0, -20, 0.25), 6.3, 3.8, 2.6, rot=4)
    disc_mesh("SAFE_CAMP_ProtectedWarmCyanGlow_NotMainFocal", col, (0, -20.0, 0.62), 2.7, 1.25, mats["MAT_Vista_SafeGlow"], sides=34)
    tri_prism_obj("SAFE_CAMP_RootShelter_GreenMemoryCanvas", col, (-1.55, -18.95, 0.86), 3.0, 1.8, 1.02, mats["MAT_Vista_SafeCampGreen"], rot=(0, 0, math.radians(8)))
    for i, dx in enumerate([-2.35, -0.75, 0.85]):
        cube_obj(f"SAFE_CAMP_RootShelter_LivingRootRib_{i:02d}", col, (dx, -19.05, 1.05), (0.18, 0.18, 1.65), mats["MAT_Vista_RootDark"], rot=(math.radians(5), 0, math.radians(-8 + i * 8)))
    cube_obj("SAFE_CAMP_VerdantBanner_Pole", col, (2.55, -19.25, 1.28), (0.10, 0.10, 2.20), mats["MAT_Vista_RootWarm"])
    cube_obj("SAFE_CAMP_VerdantBanner_GreenCloth", col, (2.88, -19.25, 1.85), (0.70, 0.06, 0.86), mats["MAT_Vista_BannerVerdant"], rot=(0, 0, math.radians(2)))
    cube_obj("SAFE_CAMP_VerdantBanner_SmallGoldLeafMotif", col, (2.89, -19.30, 1.86), (0.14, 0.035, 0.38), mats["MAT_Vista_CampGold"], rot=(0, 0, math.radians(28)))
    cube_obj("SAFE_CAMP_ArchiveTable_RootWood", col, (-2.25, -20.55, 0.86), (1.25, 0.72, 0.22), mats["MAT_Vista_RootWarm"])
    bicone_mesh("SAFE_CAMP_ArchiveTable_CyanMemoryMap", col, (-2.25, -20.55, 1.05), 0.26, 0.08, mats["MAT_Vista_SafeGlow"], sides=8)
    frustum_obj("SAFE_CAMP_MemoryBrazier_RootBowl", col, (1.20, -20.50, 0.88), 0.36, 0.28, 0.34, 7, mats["MAT_Vista_RootDark"])
    frustum_obj("SAFE_CAMP_MemoryBrazier_ProtectedCyanFlame", col, (1.20, -20.50, 1.18), 0.16, 0.04, 0.62, 6, mats["MAT_Vista_CyanMemory"])
    make_character("SAFE_CAMP_SylvaenMemoryKeeperSilhouette", chars, mats, (-0.85, -19.35, 0.72), staff=True)
    make_character("SAFE_CAMP_SylvaenRootGuardianSilhouette", chars, mats, (1.95, -20.15, 0.72), staff=True)
    make_character("PLAYER_ForegroundSylvaenThirdPersonPlaceholder", chars, mats, (0.0, -25.1, 0.45), mat_key="MAT_Vista_Player", scale=1.06, staff=True)


def make_lost_ruins(collections, mats):
    col = collections["VISTA_Midground_LostCivilizationRuins"]
    make_floating_island("LOST_RUINS_MidLeftElevatedHalfManifestedIsland", col, mats, (-20, 8, 4.1), 8.2, 5.0, 4.2, rot=-10)
    x, y = -20, 8
    cube_obj("LOST_RUINS_GreatBrokenArch_LeftColumn", col, (x - 3.25, y, 7.0), (0.72, 0.72, 5.35), mats["MAT_Vista_GhostStone"], rot=(math.radians(4), 0, math.radians(-5)))
    cube_obj("LOST_RUINS_GreatBrokenArch_RightColumn", col, (x + 3.05, y + 0.12, 6.55), (0.68, 0.72, 4.55), mats["MAT_Vista_GhostStone"], rot=(math.radians(-3), 0, math.radians(7)))
    xz_beam_obj("LOST_RUINS_GreatBrokenArch_LeftCrown", col, y + 0.04, (x - 3.1, 9.60), (x - 0.45, 10.75), 0.62, 0.38, mats["MAT_Vista_GhostStone"])
    xz_beam_obj("LOST_RUINS_GreatBrokenArch_RightCrown", col, y + 0.04, (x + 0.18, 10.55), (x + 2.8, 9.35), 0.62, 0.36, mats["MAT_Vista_GhostStone"])
    for i, (dx, dy, h, rot) in enumerate([(-5.7, -1.1, 3.2, -10), (5.2, 0.8, 2.7, 12), (-1.0, 2.4, 2.2, 5)]):
        cube_obj(f"LOST_RUINS_PartialColumn_NotFullyTouchingGround_{i:02d}", col, (x + dx, y + dy, 5.8 + h * 0.22), (0.55, 0.55, h), mats["MAT_Vista_GhostStone"], rot=(math.radians(4), math.radians(2), math.radians(rot)))
        bicone_mesh(f"LOST_RUINS_GhostCyanCrack_Column_{i:02d}", col, (x + dx + 0.06, y + dy - 0.22, 7.0 + h * 0.14), 0.055, 0.34, mats["MAT_Vista_CyanDim"], sides=5)
    for i, (dx, dy, z, rot) in enumerate([(-2.2, -1.9, 4.95, -8), (-1.1, -1.25, 5.35, -4), (0.15, -0.72, 5.84, 3), (1.45, -0.18, 6.35, 8)]):
        cube_obj(f"LOST_RUINS_IncompleteFloatingStair_{i:02d}", col, (x + dx, y + dy, z), (1.22, 0.55, 0.16), mats["MAT_Vista_GhostStone"], rot=(math.radians(2), math.radians(-3), math.radians(rot)))
    for i, (dx, dy, z, sx, sz, rot) in enumerate([(-5.0, 1.7, 7.7, 1.8, 2.0, -14), (4.7, 1.5, 7.35, 1.6, 1.7, 12), (0.4, 3.0, 8.4, 1.2, 1.5, 3)]):
        cube_obj(f"LOST_RUINS_FloatingWallPiece_OutOfAlignment_{i:02d}", col, (x + dx, y + dy, z), (sx, 0.22, sz), mats["MAT_Vista_GhostStone"], rot=(math.radians(5), math.radians(8 if i else -7), math.radians(rot)))


def make_dead_god_shrine(collections, mats):
    col = collections["VISTA_Midground_DeadGodShrine"]
    make_floating_island("DEAD_GOD_SHRINE_MidRightIsolatedCeremonialIsland", col, mats, (20, 10, 4.4), 7.0, 4.4, 4.0, rot=12)
    x, y = 20, 10
    disc_mesh("DEAD_GOD_SHRINE_PaleGoldSacredGroundStain", col, (x, y, 4.78), 4.6, 2.1, mats["MAT_Vista_PaleGold"], sides=36, rot=(0, 0, math.radians(10)))
    halo = [(-2.0, 7.6), (-1.1, 8.95), (0.0, 9.45), (1.1, 8.90), (2.0, 7.58)]
    for i in range(len(halo) - 1):
        if i == 2:
            continue
        xz_beam_obj(f"DEAD_GOD_SHRINE_CrackedCircularHalo_Segment_{i:02d}", col, y, (x + halo[i][0], halo[i][1]), (x + halo[i + 1][0], halo[i + 1][1]), 0.36, 0.20, mats["MAT_Vista_PaleGold"] if i in [1, 3] else mats["MAT_Vista_DeadStone"])
    cube_obj("DEAD_GOD_SHRINE_BrokenAltar_CoreStone", col, (x, y - 0.45, 5.36), (2.0, 1.0, 0.62), mats["MAT_Vista_DeadStone"], rot=(math.radians(2), 0, math.radians(4)))
    cube_obj("DEAD_GOD_SHRINE_MissingNameRuneSlab", col, (x - 1.15, y - 1.35, 5.95), (0.34, 0.17, 1.72), mats["MAT_Vista_DeadStone"], rot=(math.radians(8), 0, math.radians(-12)))
    frustum_obj("DEAD_GOD_SHRINE_PaleGoldVerticalLight_DivineRemnant", col, (x + 0.16, y - 0.25, 7.55), 0.28, 0.04, 5.1, 7, mats["MAT_Vista_PaleGold"])
    for i, angle in enumerate([20, 88, 158, 218, 288]):
        rad = math.radians(angle)
        cube_obj(f"DEAD_GOD_SHRINE_ShatteredDivineSigil_Fragment_{i:02d}", col, (x + math.cos(rad) * 2.65, y - 0.3 + math.sin(rad) * 1.45, 5.05), (0.78, 0.18, 0.12), mats["MAT_Vista_PaleGold"] if i % 2 else mats["MAT_Vista_DeadStone"], rot=(math.radians(3), 0, rad + math.radians(8)))


def make_worldroot_rift(collections, mats):
    col = collections["VISTA_FarMid_WorldrootSheddingRift"]
    make_floating_island("WORLDROOT_RIFT_FarMidDominantSplitIsland", col, mats, (0, 31, 6.0), 13.5, 8.0, 7.8, rot=3)
    x, y = 0, 31
    disc_mesh("WORLDROOT_RIFT_CyanMemoryLight_FromBelow", col, (x, y, 6.56), 7.3, 2.55, mats["MAT_Vista_CyanMemory"], sides=48, rot=(0, 0, math.radians(4)))
    cube_obj("WORLDROOT_RIFT_BlackRootMouth_WideCrack", col, (x, y, 6.80), (12.5, 0.90, 0.26), mats["MAT_Vista_TerrainUnderside_Void"], rot=(math.radians(3), 0, math.radians(5)))
    cube_obj("WORLDROOT_RIFT_VioletInstabilityEdge_Left", col, (x - 1.20, y + 0.06, 7.04), (5.9, 0.18, 0.12), mats["MAT_Vista_VioletInstability"], rot=(math.radians(4), 0, math.radians(18)))
    cube_obj("WORLDROOT_RIFT_CyanMemoryEdge_Right", col, (x + 1.20, y - 0.04, 7.06), (6.0, 0.16, 0.12), mats["MAT_Vista_CyanMemory"], rot=(math.radians(4), 0, math.radians(-12)))
    for i, angle in enumerate([-78, -56, -34, -14, 18, 40, 62, 78]):
        rad = math.radians(angle)
        length = 6.2 + (i % 3) * 1.05
        cube_obj(f"WORLDROOT_RIFT_ExposedRootFiber_Nerve_{i:02d}", col, (x + math.sin(rad) * 2.2, y - 0.35 + math.cos(rad) * 0.72, 7.25 + i * 0.05), (0.18, length, 0.14), mats["MAT_Vista_RootDark"], rot=(math.radians(8), math.radians(2), rad))
        if i in [1, 3, 5, 6]:
            cube_obj(f"WORLDROOT_RIFT_CyanPulseInsideNerve_{i:02d}", col, (x + math.sin(rad) * 2.0, y - 0.15 + math.cos(rad) * 0.72, 7.44 + i * 0.05), (0.050, length * 0.56, 0.05), mats["MAT_Vista_CyanMemory"], rot=(math.radians(8), math.radians(2), rad))
    for i, (dx, dy, sx, sy, z, rot, mat_key) in enumerate([(-5.7, -1.65, 2.6, 1.05, 7.4, -24, "MAT_Vista_TerrainTop_MutedMoss"), (-3.0, 1.75, 2.0, 0.90, 8.00, 16, "MAT_Vista_TerrainSide_DarkStone"), (2.9, -1.38, 2.4, 0.95, 7.80, 22, "MAT_Vista_TerrainTop_MutedMoss"), (5.6, 1.40, 2.3, 0.96, 8.35, -18, "MAT_Vista_VioletInstability"), (0.0, 2.85, 1.80, 0.82, 8.75, 4, "MAT_Vista_GhostStone")]):
        plate = cube_obj(f"WORLDROOT_RIFT_LiftedBrokenTerrainPlate_{i:02d}", col, (x + dx, y + dy, z), (sx, sy, 0.20), mats[mat_key], rot=(math.radians(8 + i * 2), math.radians((-1) ** i * 10), math.radians(rot)))
        disable_shadow(plate)
    for i, (dx, dy, z, h, mat_key, lean) in enumerate([(0.0, 0.0, 17.0, 30.0, "MAT_Vista_CyanMemory", 0), (-0.92, 0.34, 15.0, 23.0, "MAT_Vista_VioletInstability", -7), (0.92, -0.18, 13.9, 16.0, "MAT_Vista_PaleGold", 8)]):
        plume = frustum_obj(f"WORLDROOT_RIFT_DominantVerticalMemoryPlume_{i:02d}_{mat_key.replace('MAT_', '')}", col, (x + dx, y + dy, z), 0.52 - i * 0.10, 0.035, h, 7, mats[mat_key], rot=(math.radians(3), math.radians(1), math.radians(lean)))
        disable_shadow(plume)
    for i, angle in enumerate([12, 42, 74, 116, 158, 208, 254, 302, 338]):
        rad = math.radians(angle)
        mat_key = "MAT_Vista_CyanMemory" if i in [0, 3, 8] else ("MAT_Vista_PaleGold" if i in [2, 6] else "MAT_Vista_VioletInstability")
        shard = bicone_mesh(f"WORLDROOT_RIFT_UpdraftPulledMemoryShard_{i:02d}", col, (x + math.cos(rad) * (3.2 + i * 0.11), y + math.sin(rad) * 1.55, 11.5 + i * 1.02), 0.18, 0.78, mats[mat_key], sides=5)
        disable_shadow(shard)


def make_background_archipelago(collections, mats):
    col = collections["VISTA_Background_MemoryArchipelago"]
    # No full backdrop wall here: the vista should feel like open void, with depth carried by silhouettes.
    island_specs = [
        (-42, 39, 12, 4.2, 1.0, -8),
        (-29, 47, 18, 5.2, 1.3, 12),
        (-16, 43, 15, 3.8, 0.95, -4),
        (14, 45, 17, 5.6, 1.1, 8),
        (30, 50, 20, 4.4, 0.9, -12),
        (43, 41, 13, 3.7, 0.8, 18),
        (-6, 56, 25, 6.8, 1.4, 4),
        (34, 61, 29, 5.5, 1.1, -6),
        (-48, 62, 27, 4.8, 0.9, 14),
        (3, 69, 34, 7.4, 1.5, 0),
        (-24, 67, 35, 5.0, 1.0, -9),
        (52, 70, 31, 4.2, 0.8, 16),
    ]
    for i, (x, y, z, rx, ry, rot) in enumerate(island_specs):
        disc_mesh(f"BACKGROUND_Archipelago_DistantFloatingIsland_{i:02d}", col, (x, y, z), rx, ry, mats["MAT_Vista_DistantIsland"], sides=8, rot=(math.radians(78), 0, math.radians(rot)))
        for j, dx in enumerate([-0.34, 0.05, 0.38]):
            cube_obj(f"BACKGROUND_Archipelago_DistantRuinTrace_{i:02d}_{j:02d}", col, (x + dx * rx, y - 0.05, z + 1.0 + j * 0.62), (0.18 + j * 0.05, 0.10, 1.1 + j * 0.38), mats["MAT_Vista_DistantRuin"], rot=(math.radians(4), 0, math.radians(rot + j * 7)))
        if i in [1, 4, 7]:
            xz_beam_obj(f"BACKGROUND_Archipelago_DistantBrokenArchHint_{i:02d}", col, y - 0.05, (x - rx * 0.38, z + 2.4), (x + rx * 0.34, z + 2.7), 0.10, 0.12, mats["MAT_Vista_DistantRuin"])
    for i, (x, y, z, sx, sz, rot) in enumerate([(-54, 52, 23, 1.4, 2.4, -18), (-36, 66, 39, 1.2, 2.0, 8), (22, 72, 42, 1.8, 3.0, 12), (49, 58, 24, 1.0, 1.8, -10)]):
        cube_obj(
            f"BACKGROUND_FallingMemoryMasonryFragment_{i:02d}",
            col,
            (x, y, z),
            (sx, 0.18, sz),
            mats["MAT_Vista_DistantRuin"],
            rot=(math.radians(5), math.radians(8 if i % 2 else -6), math.radians(rot)),
        )


def make_traversal_and_battle_echo(collections, mats):
    col = collections["VISTA_Traversal_MemoryBridges"]
    echo_col = collections["VISTA_Background_MemoryArchipelago"]
    for i, (x, y, z, rot, mat_key) in enumerate([(-1.2, -11.5, 1.15, -4, "MAT_Vista_TerrainSide_DarkStone"), (-0.2, -3.9, 1.75, 8, "MAT_Vista_GhostBlue"), (1.15, 4.2, 2.65, -7, "MAT_Vista_CyanDim"), (2.2, 12.4, 3.80, 4, "MAT_Vista_CyanDim")]):
        cube_obj(f"TRAVERSAL_SafeCampToRift_BrokenMemorySteppingStone_{i:02d}", col, (x, y, z), (1.30 - i * 0.12, 0.52, 0.14), mats[mat_key], rot=(math.radians(4), math.radians(-2), math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(-6.6, -3.2, 2.0, -20), (-11.0, 1.5, 2.8, -18), (-15.0, 5.0, 3.65, -16)]):
        cube_obj(f"TRAVERSAL_ToLostRuins_PaleMemoryBridgeFragment_{i:02d}", col, (x, y, z), (1.65, 0.36, 0.10), mats["MAT_Vista_GhostBlue"], rot=(math.radians(3), math.radians(3), math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(5.5, 1.0, 2.0, 24), (10.4, 4.7, 2.8, 21), (15.4, 7.7, 3.55, 17)]):
        cube_obj(f"TRAVERSAL_ToDeadGodShrine_PaleGoldMemoryBridgeFragment_{i:02d}", col, (x, y, z), (1.45, 0.32, 0.10), mats["MAT_Vista_PaleGold"], rot=(math.radians(3), math.radians(-2), math.radians(rot)))
    # A sparse echo battlefield silhouette in front-left, kept secondary to the main three landmarks.
    ex, ey, ez = -31, 0, 2.7
    disc_mesh("ECHO_BATTLEFIELD_SecondaryIsland_DarkWarMemoryStain", echo_col, (ex, ey, ez), 4.8, 2.1, mats["MAT_Vista_BattleStain"], sides=28, rot=(0, 0, math.radians(-10)))
    for i, (dx, dy, lean) in enumerate([(-2.4, -0.4, -18), (-0.6, 0.5, 6), (1.2, -0.1, -5), (2.8, 0.6, 16)]):
        cube_obj(f"ECHO_BATTLEFIELD_EmbeddedSpearMemory_{i:02d}", echo_col, (ex + dx, ey + dy, ez + 1.1), (0.07, 0.07, 1.75), mats["MAT_Vista_GhostBlue"], rot=(math.radians(12), math.radians(lean), math.radians(lean)))
    for i, (dx, dy) in enumerate([(-0.8, -0.4), (0.4, 0.4), (1.6, -0.2)]):
        make_character(f"ECHO_BATTLEFIELD_SpectralSoldierEcho_{i:02d}", echo_col, mats, (ex + dx, ey + dy, ez + 0.22), mat_key="MAT_Vista_GhostBlue", scale=0.78)


def setup_lighting_and_cameras(collections):
    col = collections["VISTA_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-16, -20, 28), rotation=(math.radians(50), 0, math.radians(-32)))
    sun = bpy.context.object
    sun.name = "VISTA_Lighting_SoftBrokenMemorySun"
    sun.data.energy = 1.65
    sun.data.angle = math.radians(8.0)
    col.objects.link(sun)
    bpy.context.collection.objects.unlink(sun)

    bpy.ops.object.light_add(type="AREA", location=(0, -5, 22), rotation=(math.radians(58), 0, 0))
    fill = bpy.context.object
    fill.name = "VISTA_Lighting_BlueVioletAtmosphereFill"
    fill.data.energy = 560
    fill.data.size = 42
    fill.data.color = (0.38, 0.52, 0.86)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    col.objects.link(fill)
    bpy.context.collection.objects.unlink(fill)

    for i, (name, loc, color, power, size) in enumerate(
        [
            ("SafeCamp", (0, -20, 2.2), (0.18, 0.92, 0.72), 95, 5.0),
            ("LostRuins", (-20, 8, 8.4), (0.50, 0.86, 1.0), 120, 7.5),
            ("DeadGod", (20, 10, 8.4), (1.0, 0.64, 0.16), 150, 7.0),
            ("RiftCyan", (0, 29, 14.5), (0.04, 0.78, 1.0), 360, 10.5),
            ("RiftViolet", (-0.8, 29, 12.0), (0.55, 0.10, 0.95), 220, 9.0),
        ]
    ):
        bpy.ops.object.light_add(type="POINT", location=loc)
        light = bpy.context.object
        light.name = f"VISTA_Lighting_MemoryAccent_{i:02d}_{name}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = size
        if hasattr(light.data, "use_shadow"):
            light.data.use_shadow = False
        col.objects.link(light)
        bpy.context.collection.objects.unlink(light)

    bpy.ops.object.camera_add(location=(0, -38.0, 6.2))
    camera = bpy.context.object
    camera.name = "VISTA_Camera_ThirdPersonMemoryArchipelago"
    look_at(camera, Vector((0, 28.0, 12.2)))
    camera.data.lens = 22
    camera.data.dof.use_dof = False
    col.objects.link(camera)
    bpy.context.collection.objects.unlink(camera)
    bpy.context.scene.camera = camera

    bpy.ops.object.camera_add(location=(0, 18, 78))
    top = bpy.context.object
    top.name = "VISTA_Camera_TopdownCompositionRead"
    look_at(top, Vector((0, 18, 0)))
    top.data.type = "ORTHO"
    top.data.ortho_scale = 96
    top.data.dof.use_dof = False
    col.objects.link(top)
    bpy.context.collection.objects.unlink(top)
    return camera, top


def build_manifest(collections):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    objects = []
    counts = {}
    for collection_name, collection in collections.items():
        counts[collection_name] = len(collection.objects)
        for obj in collection.objects:
            mats = [slot.material.name for slot in obj.material_slots if slot.material]
            objects.append(
                {
                    "name": obj.name,
                    "collection": collection_name,
                    "type": obj.type,
                    "materials": mats,
                    "approximate_triangles": object_triangles(obj, depsgraph),
                }
            )
    material_names = sorted({mat.name for mat in bpy.data.materials})
    manifest = {
        "scene": "The Memory Wastes Vista Benchmark",
        "file_stem": STEM,
        "purpose": "separate art-direction vista benchmark, not gameplay/collision terrain",
        "collections": list(collections.keys()),
        "object_counts": counts,
        "objects": objects,
        "material_names": material_names,
        "approximate_total_triangles": sum(item["approximate_triangles"] for item in objects),
        "render_paths": [str(OUT_RENDER), str(OUT_TOPDOWN)],
        "export_paths": {"blend": str(OUT_BLEND), "glb": str(OUT_GLB)},
        "qa": {
            "non_prefixed_materials": [name for name in material_names if not name.startswith("MAT_")],
            "unnamed_primitives": [obj["name"] for obj in objects if obj["name"].startswith(("Cube", "Plane", "Cylinder"))],
        },
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    return manifest


def write_report(manifest):
    report = f"""# Memory Wastes Vista Benchmark Report

## Scope
- Scene: The Memory Wastes
- Output blend: {OUT_BLEND}
- Output GLB: {OUT_GLB}
- Vista render: {OUT_RENDER}
- Top-down render: {OUT_TOPDOWN}
- Purpose: separate art-direction composition for mood, silhouette, scale and landmark hierarchy.
- Not final gameplay terrain.
- Not collision-ready.

## Composition
- Foreground: small Sylvaen Safe Camp island with player silhouette, root shelter, Verdant banner, archive table, memory brazier and two NPC silhouettes.
- Midground left: Lost Civilization Ruins island with large broken arch, columns, incomplete stairs and floating wall fragments.
- Midground right: Dead God Shrine island with cracked halo, altar, missing-name slab and pale gold vertical light.
- Far-mid center: dominant Worldroot Shedding Rift with split island, root nerves, cyan/violet/gold energy and lifted terrain plates.
- Background: floating memory archipelago with distant ruined arches, towers and island silhouettes fading into blue-violet haze.

## Required Answers
1. Does the scene read as a vast fragmented memory archipelago?
   Yes. The vista is built as a wide layered composition: foreground safe foothold, separated midground role islands, a far-mid rift and many distant floating island silhouettes.

2. Is the Worldroot Rift the strongest focal point?
   Yes. It has the tallest vertical plumes, largest energy contrast, exposed root fibers, a wide split island and the strongest cyan/violet motion.

3. Is the Safe Camp clearly foreground and secondary?
   Yes. It is lowest, closest to the camera and calmer in color/scale. Its glow is warm/cyan but deliberately smaller than the rift.

4. Is the Lost Civilization Ruins island readable?
   Yes. It has a large broken arch, partial columns, incomplete floating stairs, out-of-alignment wall fragments and ghost-white/cyan stone.

5. Is the Dead God Shrine island readable?
   Yes. It has a clean sacred silhouette: cracked circular halo, broken altar, missing-name rune slab, shattered sigil pieces and pale gold vertical light.

6. Is there enough negative space and atmospheric depth?
   Yes. The main islands are separated by open void, traversal is partial rather than continuous, and the background archipelago sits in blue-violet haze.

7. What should be extracted as prefabs for the playable scene?
   Recommended prefabs: WORLDROOT_RIFT split-island fissure pieces, exposed root-fiber nerves, pulled memory shard set, LOST_RUINS broken arch kit, LOST_RUINS floating stair/wall fragments, DEAD_GOD_SHRINE cracked halo/altar/sigil set, SAFE_CAMP root shelter/banner/brazier set, and TRAVERSAL memory bridge fragments.

## Acceptance Notes
- The scene intentionally favors mood, silhouette and depth over collision accuracy.
- Cyan is reserved for Worldroot memory, ghost-white/blue for echoes and lost civilization, violet for rift danger, pale gold for dead god remnants, and warm green/gold for safe camp.
- No random cyan diamond scatter was used.

## Technical QA
- Objects: {sum(manifest["object_counts"].values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_REPORT.write_text(report, encoding="utf-8")


def build_scene():
    ensure_dirs()
    clear_scene()
    collections = create_collections()
    mats = vista_materials()
    setup_scene_settings((0.055, 0.070, 0.11), exposure=-0.08)
    make_safe_camp(collections, mats)
    make_lost_ruins(collections, mats)
    make_dead_god_shrine(collections, mats)
    make_worldroot_rift(collections, mats)
    make_background_archipelago(collections, mats)
    make_traversal_and_battle_echo(collections, mats)
    main_camera, top_camera = setup_lighting_and_cameras(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_scene_glb(collections, OUT_GLB)
    bpy.context.scene.camera = main_camera
    render_to(OUT_RENDER, 2560, 1440)
    bpy.context.scene.camera = top_camera
    render_to(OUT_TOPDOWN, 2560, 1440)
    bpy.context.scene.camera = main_camera
    manifest = build_manifest(collections)
    write_report(manifest)
    return manifest


if __name__ == "__main__":
    build_scene()
