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
    bicone_mesh,
    cube_obj,
    disable_shadow,
    disc_mesh,
    frustum_obj,
    look_at,
    render_to,
    setup_scene_settings,
    tri_prism_obj,
    xz_beam_obj,
)
from build_heartbough_glade_entrance import (  # noqa: E402
    ROOT,
    clear_scene,
    irregular_slab_mesh,
    make_mat,
    object_triangles,
)


STEM = "memory_wastes_rebuild_v004"
OUT_BLEND = ROOT / "assets" / "blender" / f"{STEM}.blend"
OUT_GLB = ROOT / "assets" / "exports" / f"{STEM}.glb"
OUT_GAMEPLAY = ROOT / "assets" / "renders" / f"{STEM}_gameplay_2560x1440.png"
OUT_TOPDOWN = ROOT / "assets" / "renders" / f"{STEM}_topdown_2560x1440.png"
OUT_MANIFEST = ROOT / "assets" / "reports" / f"{STEM}_manifest.json"
OUT_REPORT = ROOT / "assets" / "reports" / f"{STEM}_report.md"


COLLECTION_NAMES = [
    "MW_PLAYER_SAFE_CAMP",
    "MW_WORLDROOT_RIFT",
    "MW_LOST_RUINS",
    "MW_DEAD_GOD_SHRINE",
    "MW_ECHO_BATTLEFIELD",
    "MW_TRAVERSAL",
    "MW_BACKGROUND_FRAGMENTS",
    "MW_ATMOSPHERE",
    "MW_LIGHTING",
    "MW_CAMERA",
]


def ensure_dirs():
    for path in [OUT_BLEND.parent, OUT_GLB.parent, OUT_GAMEPLAY.parent, OUT_REPORT.parent]:
        path.mkdir(parents=True, exist_ok=True)


def create_collections():
    collections = {}
    for name in COLLECTION_NAMES:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
        collections[name] = collection
    return collections


def mw_materials():
    return {
        "MAT_MW_Terrain_MutedMoss": make_mat("MAT_MW_Terrain_MutedMoss", (0.20, 0.26, 0.21)),
        "MAT_MW_Terrain_ColdStone": make_mat("MAT_MW_Terrain_ColdStone", (0.27, 0.30, 0.31)),
        "MAT_MW_Terrain_DarkUnderside": make_mat("MAT_MW_Terrain_DarkUnderside", (0.025, 0.028, 0.035)),
        "MAT_MW_Terrain_BrokenEdge": make_mat("MAT_MW_Terrain_BrokenEdge", (0.08, 0.085, 0.09)),
        "MAT_MW_Root_Dark": make_mat("MAT_MW_Root_Dark", (0.16, 0.075, 0.038)),
        "MAT_MW_Root_Warm": make_mat("MAT_MW_Root_Warm", (0.34, 0.18, 0.08)),
        "MAT_MW_Safe_Green": make_mat("MAT_MW_Safe_Green", (0.055, 0.29, 0.16)),
        "MAT_MW_Safe_Glow": make_mat(
            "MAT_MW_Safe_Glow", (0.08, 0.58, 0.46), emission=(0.02, 0.46, 0.36), strength=0.48, alpha=0.34
        ),
        "MAT_MW_Banner_Verdant": make_mat("MAT_MW_Banner_Verdant", (0.035, 0.28, 0.14)),
        "MAT_MW_Warm_Gold": make_mat(
            "MAT_MW_Warm_Gold", (0.88, 0.62, 0.22), emission=(0.62, 0.38, 0.08), strength=0.24
        ),
        "MAT_MW_Worldroot_Cyan": make_mat(
            "MAT_MW_Worldroot_Cyan", (0.04, 0.70, 0.86), emission=(0.02, 0.66, 0.95), strength=1.55, alpha=0.70
        ),
        "MAT_MW_Worldroot_Cyan_Dim": make_mat(
            "MAT_MW_Worldroot_Cyan_Dim", (0.08, 0.34, 0.40), emission=(0.02, 0.20, 0.25), strength=0.28, alpha=0.42
        ),
        "MAT_MW_Violet_Instability": make_mat(
            "MAT_MW_Violet_Instability", (0.28, 0.06, 0.47), emission=(0.36, 0.04, 0.70), strength=0.95, alpha=0.54
        ),
        "MAT_MW_Ghost_Ruin": make_mat(
            "MAT_MW_Ghost_Ruin", (0.70, 0.84, 0.78), emission=(0.16, 0.30, 0.26), strength=0.28, alpha=0.62
        ),
        "MAT_MW_Ghost_Blue": make_mat(
            "MAT_MW_Ghost_Blue", (0.55, 0.82, 1.0), emission=(0.18, 0.52, 0.92), strength=0.68, alpha=0.48
        ),
        "MAT_MW_Dead_Stone": make_mat("MAT_MW_Dead_Stone", (0.34, 0.33, 0.31)),
        "MAT_MW_Dead_Shadow": make_mat("MAT_MW_Dead_Shadow", (0.11, 0.10, 0.10)),
        "MAT_MW_PaleGold_Divine": make_mat(
            "MAT_MW_PaleGold_Divine", (0.96, 0.72, 0.28), emission=(0.98, 0.58, 0.10), strength=1.18, alpha=0.74
        ),
        "MAT_MW_Battle_Stain": make_mat(
            "MAT_MW_Battle_Stain", (0.12, 0.16, 0.20), emission=(0.03, 0.07, 0.11), strength=0.10, alpha=0.56
        ),
        "MAT_MW_Distant_Island": make_mat(
            "MAT_MW_Distant_Island", (0.09, 0.11, 0.16), emission=(0.025, 0.035, 0.07), strength=0.06, alpha=0.54
        ),
        "MAT_MW_Distant_Ruin": make_mat(
            "MAT_MW_Distant_Ruin", (0.36, 0.44, 0.47), emission=(0.08, 0.13, 0.16), strength=0.12, alpha=0.42
        ),
        "MAT_MW_Atmosphere_BlueGray": make_mat(
            "MAT_MW_Atmosphere_BlueGray", (0.12, 0.16, 0.24), emission=(0.018, 0.026, 0.052), strength=0.03, alpha=0.07
        ),
        "MAT_MW_Player": make_mat("MAT_MW_Player", (0.50, 0.70, 0.56), alpha=0.94),
        "MAT_MW_NPC": make_mat("MAT_MW_NPC", (0.74, 0.72, 0.54), alpha=0.88),
    }


def export_scene_glb(collections, out_glb):
    bpy.ops.object.select_all(action="DESELECT")
    for collection in collections.values():
        for obj in collection.objects:
            obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(out_glb), export_format="GLB", use_selection=True)


def make_island(prefix, collection, mats, loc, rx, ry, drop_height, top_mat_key="MAT_MW_Terrain_MutedMoss", rot=0.0, skin_mat_key=None):
    x, y, z = loc
    irregular_slab_mesh(
        f"{prefix}_IrregularFloatingIslandMass",
        collection,
        (x, y, z),
        rx,
        ry,
        0.60,
        mats[top_mat_key],
        sides=13,
        rot_z=math.radians(rot),
        irregularity=0.22,
    )
    disc_mesh(
        f"{prefix}_PaintedMemorySurface",
        collection,
        (x + rx * 0.03, y - ry * 0.04, z + 0.34),
        rx * 0.70,
        ry * 0.55,
        mats[skin_mat_key or top_mat_key],
        sides=10,
        rot=(0, 0, math.radians(rot + 9)),
    )
    underside = frustum_obj(
        f"{prefix}_TaperedVoidUnderside",
        collection,
        (x, y, z - drop_height * 0.48),
        0.35,
        1.0,
        drop_height,
        9,
        mats["MAT_MW_Terrain_DarkUnderside"],
        rot=(0, 0, math.radians(rot)),
    )
    underside.scale.x = rx * 0.82
    underside.scale.y = ry * 0.82
    disable_shadow(underside)
    for i, (ox, oy, sx, sy, tilt) in enumerate(
        [(-0.46, -0.42, 0.24, 0.10, -10), (0.12, -0.55, 0.20, 0.12, 8), (0.52, -0.33, 0.18, 0.10, 17), (-0.08, 0.48, 0.18, 0.08, -18)]
    ):
        cube_obj(
            f"{prefix}_JaggedLowerStrata_{i:02d}",
            collection,
            (x + ox * rx, y + oy * ry, z - 0.26 - i * 0.05),
            (rx * sx, ry * sy, 0.22),
            mats["MAT_MW_Terrain_BrokenEdge"],
            rot=(math.radians(2 + i), 0, math.radians(rot + tilt)),
        )


def make_character(prefix, collection, mats, loc, mat_key="MAT_MW_NPC", scale=1.0, staff=False):
    x, y, z = loc
    mat = mats[mat_key]
    frustum_obj(f"{prefix}_BodyCloak", collection, (x, y, z + 0.78 * scale), 0.30 * scale, 0.21 * scale, 1.16 * scale, 6, mat)
    frustum_obj(f"{prefix}_Head", collection, (x, y, z + 1.55 * scale), 0.20 * scale, 0.17 * scale, 0.30 * scale, 9, mat)
    cube_obj(f"{prefix}_ShoulderSilhouette", collection, (x, y, z + 1.17 * scale), (0.78 * scale, 0.13 * scale, 0.18 * scale), mat)
    if staff:
        cube_obj(
            f"{prefix}_StaffOrSpearLine",
            collection,
            (x + 0.43 * scale, y - 0.02, z + 0.88 * scale),
            (0.055 * scale, 0.055 * scale, 1.68 * scale),
            mats["MAT_MW_Root_Warm"],
            rot=(math.radians(7), 0, math.radians(-8)),
        )


def build_safe_camp(collections, mats):
    col = collections["MW_PLAYER_SAFE_CAMP"]
    x, y, z = 0.0, -35.0, 0.0
    make_island("MW_SAFE_CAMP_ForegroundStableLowIsland", col, mats, (x, y, z), 7.0, 5.0, 2.8, rot=4, skin_mat_key="MAT_MW_Safe_Green")
    disc_mesh("MW_SAFE_CAMP_ProtectedWarmCyanGlow_Secondary", col, (x, y, z + 0.46), 3.0, 1.55, mats["MAT_MW_Safe_Glow"], sides=34)
    tri_prism_obj("MW_SAFE_CAMP_RootShelter_GreenCanvas", col, (x - 2.0, y + 1.10, z + 0.70), 3.2, 2.0, 1.10, mats["MAT_MW_Safe_Green"], rot=(0, 0, math.radians(8)))
    for i, dx in enumerate([-2.95, -1.35, 0.20]):
        cube_obj(
            f"MW_SAFE_CAMP_RootShelter_LivingRootRib_{i:02d}",
            col,
            (x + dx, y + 1.05, z + 0.95),
            (0.20, 0.20, 1.72),
            mats["MAT_MW_Root_Dark"],
            rot=(math.radians(5), 0, math.radians(-8 + i * 8)),
        )
    cube_obj("MW_SAFE_CAMP_VerdantBanner_Pole", col, (x + 2.75, y + 1.20, z + 1.25), (0.10, 0.10, 2.25), mats["MAT_MW_Root_Warm"])
    cube_obj("MW_SAFE_CAMP_VerdantBanner_GreenCloth", col, (x + 3.08, y + 1.20, z + 1.82), (0.74, 0.06, 0.88), mats["MAT_MW_Banner_Verdant"], rot=(0, 0, math.radians(2)))
    cube_obj("MW_SAFE_CAMP_VerdantBanner_GoldLeafMotif", col, (x + 3.09, y + 1.16, z + 1.82), (0.15, 0.035, 0.38), mats["MAT_MW_Warm_Gold"], rot=(0, 0, math.radians(28)))
    cube_obj("MW_SAFE_CAMP_ArchiveTable_RootWood", col, (x - 2.30, y - 1.05, z + 0.70), (1.30, 0.74, 0.22), mats["MAT_MW_Root_Warm"])
    bicone_mesh("MW_SAFE_CAMP_ArchiveTable_CyanMemoryMap", col, (x - 2.30, y - 1.05, z + 0.90), 0.26, 0.08, mats["MAT_MW_Safe_Glow"], sides=8)
    frustum_obj("MW_SAFE_CAMP_MemoryBrazier_RootBowl", col, (x + 1.35, y - 1.12, z + 0.72), 0.36, 0.28, 0.34, 7, mats["MAT_MW_Root_Dark"])
    frustum_obj("MW_SAFE_CAMP_MemoryBrazier_ProtectedCyanFlame", col, (x + 1.35, y - 1.12, z + 1.03), 0.16, 0.04, 0.62, 6, mats["MAT_MW_Worldroot_Cyan"])
    make_character("MW_SAFE_CAMP_NPC_MemoryKeeperSylvaen", col, mats, (x - 0.90, y + 0.55, z + 0.48), staff=True)
    make_character("MW_SAFE_CAMP_NPC_RootGuardianSylvaen", col, mats, (x + 1.80, y - 0.05, z + 0.48), staff=True)
    make_character("MW_PLAYER_SylvaenPlaceholder_ThirdPersonScale", col, mats, (x, y - 5.65, z + 0.38), mat_key="MAT_MW_Player", scale=1.07, staff=True)


def build_worldroot_rift(collections, mats):
    col = collections["MW_WORLDROOT_RIFT"]
    x, y, z = 0.0, 25.0, 5.0
    make_island("MW_WORLDROOT_RIFT_LargestSplitFloatingIsland", col, mats, (x, y, z), 13.0, 10.0, 8.5, rot=3, skin_mat_key="MAT_MW_Worldroot_Cyan_Dim")
    disc_mesh("MW_WORLDROOT_RIFT_CyanMemoryLight_FromCrack", col, (x, y, z + 0.66), 7.5, 2.7, mats["MAT_MW_Worldroot_Cyan"], sides=52, rot=(0, 0, math.radians(4)))
    cube_obj("MW_WORLDROOT_RIFT_BlackSplitMouth_DownIslandCenter", col, (x, y, z + 0.92), (13.0, 0.95, 0.30), mats["MAT_MW_Terrain_DarkUnderside"], rot=(math.radians(3), 0, math.radians(5)))
    cube_obj("MW_WORLDROOT_RIFT_VioletInstability_LeftCrackEdge", col, (x - 1.30, y + 0.05, z + 1.18), (6.4, 0.20, 0.13), mats["MAT_MW_Violet_Instability"], rot=(math.radians(4), 0, math.radians(18)))
    cube_obj("MW_WORLDROOT_RIFT_CyanMemory_RightCrackEdge", col, (x + 1.25, y - 0.05, z + 1.20), (6.3, 0.18, 0.13), mats["MAT_MW_Worldroot_Cyan"], rot=(math.radians(4), 0, math.radians(-12)))
    for i, angle in enumerate([-82, -62, -42, -20, 4, 28, 50, 70, 86]):
        rad = math.radians(angle)
        length = 6.4 + (i % 3) * 1.08
        nx = x + math.sin(rad) * 2.45
        ny = y - 0.35 + math.cos(rad) * 0.86
        cube_obj(
            f"MW_WORLDROOT_RIFT_ExposedRootFiber_NervePull_{i:02d}",
            col,
            (nx, ny, z + 1.55 + i * 0.06),
            (0.20, length, 0.15),
            mats["MAT_MW_Root_Dark"],
            rot=(math.radians(8), math.radians(2), rad),
        )
        if i in [1, 3, 5, 7]:
            cube_obj(
                f"MW_WORLDROOT_RIFT_CyanMemoryPulseInsideRoot_{i:02d}",
                col,
                (nx, ny + 0.18, z + 1.78 + i * 0.06),
                (0.055, length * 0.58, 0.055),
                mats["MAT_MW_Worldroot_Cyan"],
                rot=(math.radians(8), math.radians(2), rad),
            )
    plate_specs = [
        (-6.3, -1.9, 2.9, 1.08, 1.75, -24, "MAT_MW_Terrain_MutedMoss"),
        (-3.4, 2.0, 2.2, 0.92, 2.35, 16, "MAT_MW_Terrain_ColdStone"),
        (3.0, -1.55, 2.6, 0.98, 2.10, 22, "MAT_MW_Terrain_MutedMoss"),
        (6.2, 1.50, 2.4, 1.0, 2.85, -18, "MAT_MW_Violet_Instability"),
        (0.0, 3.15, 1.9, 0.85, 3.35, 4, "MAT_MW_Ghost_Ruin"),
        (-1.3, -3.2, 1.7, 0.75, 2.65, -5, "MAT_MW_Terrain_ColdStone"),
    ]
    for i, (dx, dy, sx, sy, dz, rot, mat_key) in enumerate(plate_specs):
        plate = cube_obj(
            f"MW_WORLDROOT_RIFT_JaggedUpwardTerrainPlate_{i:02d}",
            col,
            (x + dx, y + dy, z + dz),
            (sx, sy, 0.22),
            mats[mat_key],
            rot=(math.radians(9 + i * 2), math.radians((-1) ** i * 10), math.radians(rot)),
        )
        disable_shadow(plate)
    plume_specs = [
        (0.0, 0.0, 15.5, 31.0, "MAT_MW_Worldroot_Cyan", 0),
        (-0.95, 0.35, 13.5, 24.0, "MAT_MW_Violet_Instability", -7),
        (0.95, -0.18, 12.7, 17.0, "MAT_MW_PaleGold_Divine", 8),
    ]
    for i, (dx, dy, dz, height, mat_key, lean) in enumerate(plume_specs):
        plume = frustum_obj(
            f"MW_WORLDROOT_RIFT_LayeredVerticalEnergyColumn_{i:02d}",
            col,
            (x + dx, y + dy, z + dz),
            0.56 - i * 0.10,
            0.035,
            height,
            7,
            mats[mat_key],
            rot=(math.radians(3), math.radians(1), math.radians(lean)),
        )
        disable_shadow(plume)
    for i, angle in enumerate([10, 38, 68, 106, 148, 196, 242, 288, 326]):
        rad = math.radians(angle)
        mat_key = "MAT_MW_Worldroot_Cyan" if i in [0, 3, 8] else ("MAT_MW_PaleGold_Divine" if i in [2, 6] else "MAT_MW_Violet_Instability")
        shard = bicone_mesh(
            f"MW_WORLDROOT_RIFT_FloatingMemoryShard_PulledUpward_{i:02d}",
            col,
            (x + math.cos(rad) * (3.4 + i * 0.14), y + math.sin(rad) * 1.65, z + 7.4 + i * 1.08),
            0.18,
            0.80,
            mats[mat_key],
            sides=5,
        )
        disable_shadow(shard)


def build_lost_ruins(collections, mats):
    col = collections["MW_LOST_RUINS"]
    x, y, z = -35.0, 10.0, 8.0
    make_island("MW_LOST_RUINS_MidLeftHalfManifestedIsland", col, mats, (x, y, z), 12.0, 7.0, 5.6, top_mat_key="MAT_MW_Terrain_ColdStone", rot=-10, skin_mat_key="MAT_MW_Ghost_Ruin")
    cube_obj("MW_LOST_RUINS_GreatBrokenArch_LeftColumn", col, (x - 4.15, y, z + 4.9), (0.80, 0.80, 8.5), mats["MAT_MW_Ghost_Ruin"], rot=(math.radians(4), 0, math.radians(-5)))
    cube_obj("MW_LOST_RUINS_GreatBrokenArch_RightColumn", col, (x + 4.0, y + 0.12, z + 4.45), (0.75, 0.80, 7.6), mats["MAT_MW_Ghost_Ruin"], rot=(math.radians(-3), 0, math.radians(7)))
    xz_beam_obj("MW_LOST_RUINS_GreatBrokenArch_LeftCrown", col, y + 0.04, (x - 4.0, z + 9.2), (x - 0.52, z + 10.6), 0.68, 0.42, mats["MAT_MW_Ghost_Ruin"])
    xz_beam_obj("MW_LOST_RUINS_GreatBrokenArch_RightCrown", col, y + 0.04, (x + 0.25, z + 10.45), (x + 3.75, z + 9.0), 0.68, 0.40, mats["MAT_MW_Ghost_Ruin"])
    for i, (dx, dy, h, rot) in enumerate([(-7.0, -1.4, 4.0, -10), (6.6, 0.9, 3.5, 12), (-1.4, 2.8, 2.9, 5)]):
        cube_obj(
            f"MW_LOST_RUINS_PartialColumn_HoveringManifestation_{i:02d}",
            col,
            (x + dx, y + dy, z + 2.5 + h * 0.48),
            (0.58, 0.58, h),
            mats["MAT_MW_Ghost_Ruin"],
            rot=(math.radians(4), math.radians(2), math.radians(rot)),
        )
        bicone_mesh(f"MW_LOST_RUINS_GhostCyanCrack_Column_{i:02d}", col, (x + dx + 0.08, y + dy - 0.22, z + 3.8 + h * 0.44), 0.055, 0.34, mats["MAT_MW_Worldroot_Cyan_Dim"], sides=5)
    for i, (dx, dy, dz, rot) in enumerate([(-3.4, -2.8, 1.1, -8), (-2.1, -2.0, 1.55, -4), (-0.7, -1.15, 2.05, 3), (0.8, -0.35, 2.56, 8), (2.3, 0.45, 3.10, 12)]):
        cube_obj(
            f"MW_LOST_RUINS_IncompleteFloatingStair_{i:02d}",
            col,
            (x + dx, y + dy, z + dz),
            (1.32, 0.60, 0.16),
            mats["MAT_MW_Ghost_Ruin"],
            rot=(math.radians(2), math.radians(-3), math.radians(rot)),
        )
    for i, (dx, dy, dz, sx, sz, rot) in enumerate([(-7.1, 2.0, 4.8, 2.1, 2.5, -14), (6.7, 1.8, 4.35, 1.8, 2.1, 12), (0.6, 3.5, 5.6, 1.4, 1.8, 3), (-2.4, 4.2, 6.3, 1.2, 1.6, -5)]):
        cube_obj(
            f"MW_LOST_RUINS_FloatingWallPiece_OutOfAlignment_{i:02d}",
            col,
            (x + dx, y + dy, z + dz),
            (sx, 0.22, sz),
            mats["MAT_MW_Ghost_Ruin"],
            rot=(math.radians(5), math.radians(8 if i else -7), math.radians(rot)),
        )


def build_dead_god_shrine(collections, mats):
    col = collections["MW_DEAD_GOD_SHRINE"]
    x, y, z = 35.0, 5.0, 9.0
    make_island("MW_DEAD_GOD_SHRINE_MidRightIsolatedCeremonialIsland", col, mats, (x, y, z), 9.0, 9.0, 5.2, top_mat_key="MAT_MW_Dead_Stone", rot=12, skin_mat_key="MAT_MW_Dead_Shadow")
    disc_mesh("MW_DEAD_GOD_SHRINE_PaleGoldSacredGroundStain", col, (x, y, z + 0.45), 4.8, 2.3, mats["MAT_MW_PaleGold_Divine"], sides=40, rot=(0, 0, math.radians(10)))
    halo = [(-3.0, z + 7.4), (-1.55, z + 10.0), (0.0, z + 11.5), (1.55, z + 10.0), (3.0, z + 7.4)]
    for i in range(len(halo) - 1):
        if i == 2:
            continue
        xz_beam_obj(
            f"MW_DEAD_GOD_SHRINE_CrackedCircularHalo_Segment_{i:02d}",
            col,
            y,
            (x + halo[i][0], halo[i][1]),
            (x + halo[i + 1][0], halo[i + 1][1]),
            0.42,
            0.26,
            mats["MAT_MW_PaleGold_Divine"] if i in [1, 3] else mats["MAT_MW_Dead_Stone"],
        )
    cube_obj("MW_DEAD_GOD_SHRINE_BrokenAltar_CoreStone", col, (x, y - 0.40, z + 0.95), (2.4, 1.2, 0.68), mats["MAT_MW_Dead_Stone"], rot=(math.radians(2), 0, math.radians(4)))
    cube_obj("MW_DEAD_GOD_SHRINE_MissingNameRuneSlab", col, (x - 1.45, y - 1.45, z + 1.65), (0.40, 0.20, 2.10), mats["MAT_MW_Dead_Shadow"], rot=(math.radians(8), 0, math.radians(-12)))
    frustum_obj("MW_DEAD_GOD_SHRINE_PaleGoldVerticalLight_DivineRemnant", col, (x + 0.18, y - 0.20, z + 4.0), 0.30, 0.04, 6.2, 7, mats["MAT_MW_PaleGold_Divine"])
    for i, angle in enumerate([20, 88, 158, 218, 288]):
        rad = math.radians(angle)
        cube_obj(
            f"MW_DEAD_GOD_SHRINE_ShatteredDivineSigil_Fragment_{i:02d}",
            col,
            (x + math.cos(rad) * 3.0, y - 0.3 + math.sin(rad) * 1.65, z + 0.68),
            (0.86, 0.20, 0.12),
            mats["MAT_MW_PaleGold_Divine"] if i % 2 else mats["MAT_MW_Dead_Stone"],
            rot=(math.radians(3), 0, rad + math.radians(8)),
        )


def build_echo_battlefield(collections, mats):
    col = collections["MW_ECHO_BATTLEFIELD"]
    x, y, z = -25.0, 35.0, 3.0
    make_island("MW_ECHO_BATTLEFIELD_FarLeftScarredFlatIsland", col, mats, (x, y, z), 11.0, 8.0, 3.6, top_mat_key="MAT_MW_Terrain_ColdStone", rot=-8, skin_mat_key="MAT_MW_Battle_Stain")
    disc_mesh("MW_ECHO_BATTLEFIELD_CircularDarkMemoryStain", col, (x, y, z + 0.50), 5.2, 2.6, mats["MAT_MW_Battle_Stain"], sides=34, rot=(0, 0, math.radians(-10)))
    for i, (dx, dy, lean) in enumerate([(-4.2, -0.8, -18), (-2.0, 0.8, 6), (0.7, -0.3, -5), (3.2, 0.9, 16), (4.8, -1.2, 10)]):
        cube_obj(
            f"MW_ECHO_BATTLEFIELD_EmbeddedSpearMemory_{i:02d}",
            col,
            (x + dx, y + dy, z + 1.25),
            (0.075, 0.075, 1.95),
            mats["MAT_MW_Ghost_Blue"],
            rot=(math.radians(12), math.radians(lean), math.radians(lean)),
        )
    for i, (dx, dy, rot) in enumerate([(-3.2, 1.7, 12), (2.8, -1.5, -14), (0.2, 2.1, 4)]):
        disc_mesh(
            f"MW_ECHO_BATTLEFIELD_BrokenShieldMemory_{i:02d}",
            col,
            (x + dx, y + dy, z + 0.75),
            0.60,
            0.34,
            mats["MAT_MW_Battle_Stain"],
            sides=9,
            rot=(math.radians(72), 0, math.radians(rot)),
        )
    for i, (dx, dy, rot) in enumerate([(-5.0, 1.3, -8), (4.7, 1.0, 10)]):
        cube_obj(f"MW_ECHO_BATTLEFIELD_GhostBanner_Pole_{i:02d}", col, (x + dx, y + dy, z + 1.30), (0.08, 0.08, 2.22), mats["MAT_MW_Root_Dark"], rot=(math.radians(5), 0, math.radians(rot)))
        cube_obj(f"MW_ECHO_BATTLEFIELD_GhostBanner_TornCloth_{i:02d}", col, (x + dx + 0.40, y + dy, z + 1.98), (0.88, 0.06, 0.62), mats["MAT_MW_Ghost_Blue"], rot=(math.radians(4), 0, math.radians(rot)))
    for i, (dx, dy) in enumerate([(-1.15, -0.55), (0.30, 0.60), (1.65, -0.22)]):
        make_character(f"MW_ECHO_BATTLEFIELD_TranslucentSoldierEcho_{i:02d}", col, mats, (x + dx, y + dy, z + 0.45), mat_key="MAT_MW_Ghost_Blue", scale=0.86)


def build_traversal(collections, mats):
    col = collections["MW_TRAVERSAL"]
    # Safe camp to rift: partial stepping path, not a full bridge.
    for i, (x, y, z, rot, mat_key) in enumerate(
        [(-0.8, -26.5, 0.85, -4, "MAT_MW_Terrain_ColdStone"), (-0.2, -17.0, 1.55, 8, "MAT_MW_Ghost_Blue"), (0.7, -7.0, 2.45, -7, "MAT_MW_Worldroot_Cyan_Dim"), (1.4, 4.5, 3.50, 4, "MAT_MW_Worldroot_Cyan_Dim"), (0.5, 15.0, 4.35, -6, "MAT_MW_Terrain_ColdStone")]
    ):
        cube_obj(f"MW_TRAVERSAL_SafeCampToRift_BrokenMemoryStep_{i:02d}", col, (x, y, z), (1.45 - i * 0.10, 0.55, 0.14), mats[mat_key], rot=(math.radians(4), math.radians(-2), math.radians(rot)))
    # Routes to side islands remain readable but incomplete.
    for i, (x, y, z, rot) in enumerate([(-8.0, -10.0, 2.0, -24), (-15.5, -3.5, 3.2, -21), (-23.0, 2.4, 5.0, -18), (-30.0, 7.4, 7.1, -15)]):
        cube_obj(f"MW_TRAVERSAL_ToLostRuins_PaleMemoryBridge_{i:02d}", col, (x, y, z), (1.7, 0.36, 0.10), mats["MAT_MW_Ghost_Blue"], rot=(math.radians(3), math.radians(3), math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(7.5, -8.0, 2.0, 24), (15.2, -2.4, 3.4, 21), (23.0, 2.2, 5.5, 17), (30.8, 4.2, 7.2, 14)]):
        cube_obj(f"MW_TRAVERSAL_ToDeadGodShrine_PaleGoldMemoryBridge_{i:02d}", col, (x, y, z), (1.55, 0.34, 0.10), mats["MAT_MW_PaleGold_Divine"], rot=(math.radians(3), math.radians(-2), math.radians(rot)))
    for i, (x, y, z) in enumerate([(0.0, -20.0, 1.3), (0.5, -7.0, 2.7), (-17.0, -2.0, 3.8), (18.0, 0.0, 4.0), (-9.5, 24.0, 4.3)]):
        shard = bicone_mesh(f"MW_TRAVERSAL_ValidRouteGuideShard_CyanDim_{i:02d}", col, (x, y, z), 0.055, 0.20, mats["MAT_MW_Worldroot_Cyan_Dim"], sides=5)
        disable_shadow(shard)


def build_background_and_atmosphere(collections, mats):
    bg = collections["MW_BACKGROUND_FRAGMENTS"]
    atm = collections["MW_ATMOSPHERE"]
    for i, (x, y, z, rx, ry, rot) in enumerate(
        [
            (-58, 52, 14, 4.4, 1.0, -8),
            (-44, 66, 22, 5.2, 1.3, 12),
            (-22, 59, 18, 3.9, 0.95, -4),
            (18, 58, 21, 5.8, 1.1, 8),
            (44, 65, 24, 4.5, 0.9, -12),
            (58, 50, 16, 3.8, 0.8, 18),
            (-5, 74, 30, 7.2, 1.5, 4),
            (34, 82, 34, 5.8, 1.2, -6),
            (-50, 84, 35, 5.0, 1.0, 14),
            (2, 91, 42, 7.6, 1.5, 0),
            (-27, 88, 41, 5.3, 1.0, -9),
            (55, 92, 38, 4.2, 0.8, 16),
        ]
    ):
        disc_mesh(f"MW_BACKGROUND_DistantFloatingMemoryIsland_{i:02d}", bg, (x, y, z), rx, ry, mats["MAT_MW_Distant_Island"], sides=8, rot=(math.radians(78), 0, math.radians(rot)))
        for j, dx in enumerate([-0.34, 0.05, 0.38]):
            cube_obj(f"MW_BACKGROUND_DistantRuinOrTowerTrace_{i:02d}_{j:02d}", bg, (x + dx * rx, y - 0.05, z + 1.0 + j * 0.62), (0.18 + j * 0.05, 0.10, 1.1 + j * 0.38), mats["MAT_MW_Distant_Ruin"], rot=(math.radians(4), 0, math.radians(rot + j * 7)))
        if i in [1, 4, 7, 9]:
            xz_beam_obj(f"MW_BACKGROUND_DistantBrokenArchHint_{i:02d}", bg, y - 0.05, (x - rx * 0.38, z + 2.4), (x + rx * 0.34, z + 2.7), 0.10, 0.12, mats["MAT_MW_Distant_Ruin"])
    # Low-contrast wisps add depth without creating a flat backdrop-board read.
    for i, (x, y, z, rx, ry, rot) in enumerate(
        [
            (-44, 62, 13, 7.5, 0.55, -10),
            (-12, 69, 19, 9.0, 0.62, 5),
            (32, 67, 18, 8.0, 0.50, 12),
            (-58, 82, 27, 10.0, 0.70, -4),
            (8, 88, 33, 11.0, 0.72, 7),
            (54, 86, 31, 8.8, 0.60, -14),
        ]
    ):
        wisp = disc_mesh(
            f"MW_ATMOSPHERE_DistantBlueGrayVoidHazeWisp_{i:02d}",
            atm,
            (x, y, z),
            rx,
            ry,
            mats["MAT_MW_Atmosphere_BlueGray"],
            sides=12,
            rot=(math.radians(76), 0, math.radians(rot)),
        )
        disable_shadow(wisp)


def setup_lighting_and_cameras(collections):
    lighting = collections["MW_LIGHTING"]
    cameras = collections["MW_CAMERA"]
    bpy.ops.object.light_add(type="SUN", location=(-22, -28, 34), rotation=(math.radians(52), 0, math.radians(-34)))
    sun = bpy.context.object
    sun.name = "MW_LIGHTING_SoftMemorySun_NotCheerfulDaylight"
    sun.data.energy = 1.28
    sun.data.angle = math.radians(9.0)
    lighting.objects.link(sun)
    bpy.context.collection.objects.unlink(sun)

    bpy.ops.object.light_add(type="AREA", location=(0, -4, 28), rotation=(math.radians(58), 0, 0))
    fill = bpy.context.object
    fill.name = "MW_LIGHTING_BlueGrayMemoryFogFill"
    fill.data.energy = 520
    fill.data.size = 50
    fill.data.color = (0.34, 0.46, 0.78)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    lighting.objects.link(fill)
    bpy.context.collection.objects.unlink(fill)

    light_specs = [
        ("SafeCampProtectedGlow", (0, -35, 2.1), (0.17, 0.88, 0.68), 95, 5.0),
        ("LostRuinsGhostGlow", (-35, 10, 15.0), (0.48, 0.84, 0.96), 125, 8.0),
        ("DeadGodPaleGold", (35, 5, 17.0), (1.0, 0.64, 0.16), 180, 7.0),
        ("RiftCyanCore", (0, 25, 19.0), (0.04, 0.78, 1.0), 480, 12.0),
        ("RiftVioletEdge", (-1.2, 25, 15.5), (0.55, 0.10, 0.95), 260, 9.5),
    ]
    for i, (name, loc, color, power, size) in enumerate(light_specs):
        bpy.ops.object.light_add(type="POINT", location=loc)
        light = bpy.context.object
        light.name = f"MW_LIGHTING_Accent_{i:02d}_{name}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = size
        if hasattr(light.data, "use_shadow"):
            light.data.use_shadow = False
        lighting.objects.link(light)
        bpy.context.collection.objects.unlink(light)

    bpy.ops.object.camera_add(location=(0, -50.0, 7.0))
    gameplay = bpy.context.object
    gameplay.name = "MW_CAMERA_GameplayVista_ThirdPersonBehindPlayer"
    look_at(gameplay, Vector((0, 25.0, 15.0)))
    gameplay.data.lens = 21
    gameplay.data.dof.use_dof = False
    cameras.objects.link(gameplay)
    bpy.context.collection.objects.unlink(gameplay)
    bpy.context.scene.camera = gameplay

    bpy.ops.object.camera_add(location=(0, 18, 105))
    top = bpy.context.object
    top.name = "MW_CAMERA_TopDown_Readability_120x90Footprint"
    look_at(top, Vector((0, 15, 0)))
    top.data.type = "ORTHO"
    top.data.ortho_scale = 110
    top.data.dof.use_dof = False
    cameras.objects.link(top)
    bpy.context.collection.objects.unlink(top)
    return gameplay, top


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
        "scene": "The Memory Wastes Hard Rebuild v004",
        "file_stem": STEM,
        "purpose": "hard rebuilt visual/playable blockout direction for a large fragmented memory-collapse archipelago",
        "collections": list(collections.keys()),
        "object_counts": counts,
        "objects": objects,
        "material_names": material_names,
        "approximate_total_triangles": sum(item["approximate_triangles"] for item in objects),
        "render_paths": [str(OUT_GAMEPLAY), str(OUT_TOPDOWN)],
        "export_paths": {"blend": str(OUT_BLEND), "glb": str(OUT_GLB)},
        "layout": {
            "safe_camp": [0, -35, 0],
            "worldroot_rift": [0, 25, 5],
            "lost_ruins": [-35, 10, 8],
            "dead_god_shrine": [35, 5, 9],
            "echo_battlefield": [-25, 35, 3],
            "visual_area_meters": [120, 90],
        },
        "qa": {
            "non_prefixed_materials": [name for name in material_names if not name.startswith("MAT_")],
            "unnamed_primitives": [obj["name"] for obj in objects if obj["name"].startswith(("Cube", "Plane", "Cylinder"))],
        },
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    return manifest


def write_report(manifest):
    report = f"""# Memory Wastes Hard Rebuild v004 Report

## Scope
- Scene: The Memory Wastes
- Build: Hard Rebuild v004
- Output blend: {OUT_BLEND}
- Output GLB: {OUT_GLB}
- Gameplay render: {OUT_GAMEPLAY}
- Top-down render: {OUT_TOPDOWN}
- Manifest: {OUT_MANIFEST}

## What Changed
- The old compact arena layout was not preserved.
- The scene was rebuilt around a 120m x 90m visual footprint with separated islands and visible void gaps.
- New role collections were created: {", ".join(COLLECTION_NAMES)}.
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
- Collections: {len(manifest["collections"])}
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
    mats = mw_materials()
    setup_scene_settings((0.045, 0.055, 0.085), exposure=-0.12)
    build_safe_camp(collections, mats)
    build_worldroot_rift(collections, mats)
    build_lost_ruins(collections, mats)
    build_dead_god_shrine(collections, mats)
    build_echo_battlefield(collections, mats)
    build_traversal(collections, mats)
    build_background_and_atmosphere(collections, mats)
    gameplay_camera, topdown_camera = setup_lighting_and_cameras(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_scene_glb(collections, OUT_GLB)
    bpy.context.scene.camera = gameplay_camera
    render_to(OUT_GAMEPLAY, 2560, 1440)
    bpy.context.scene.camera = topdown_camera
    render_to(OUT_TOPDOWN, 2560, 1440)
    bpy.context.scene.camera = gameplay_camera
    manifest = build_manifest(collections)
    write_report(manifest)
    return manifest


if __name__ == "__main__":
    build_scene()
