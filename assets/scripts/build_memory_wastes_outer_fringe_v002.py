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


STEM = "memory_wastes_outer_fringe_v002"
OUT_BLEND = ROOT / "assets" / "blender" / f"{STEM}.blend"
OUT_GLB = ROOT / "assets" / "exports" / f"{STEM}.glb"
OUT_GAMEPLAY = ROOT / "assets" / "renders" / "memory_wastes_outer_fringe_gameplay_2560x1440.png"
OUT_TOPDOWN = ROOT / "assets" / "renders" / "memory_wastes_outer_fringe_topdown_2560x1440.png"
OUT_MANIFEST = ROOT / "assets" / "reports" / "memory_wastes_outer_fringe_manifest.json"
OUT_REPORT = ROOT / "assets" / "reports" / "memory_wastes_outer_fringe_report.md"


COLLECTION_NAMES = [
    "MW_OUTER_TERRAIN",
    "MW_WARDEN_CAMP",
    "MW_ECHO_FLATS",
    "MW_ORANYN_RUINS",
    "MW_ROOTSHEAR_RIFT",
    "MW_MEMORY_GHOSTS",
    "MW_PROPS",
    "MW_FOLIAGE",
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


def outer_materials():
    return {
        "MAT_MWOF_Ground_DarkMoss": make_mat("MAT_MWOF_Ground_DarkMoss", (0.095, 0.145, 0.105)),
        "MAT_MWOF_Ground_SickMoss": make_mat("MAT_MWOF_Ground_SickMoss", (0.16, 0.21, 0.13)),
        "MAT_MWOF_Ground_MemoryMud": make_mat("MAT_MWOF_Ground_MemoryMud", (0.18, 0.15, 0.12)),
        "MAT_MWOF_Path_MossyStone": make_mat("MAT_MWOF_Path_MossyStone", (0.42, 0.43, 0.36)),
        "MAT_MWOF_Path_CrackedDark": make_mat("MAT_MWOF_Path_CrackedDark", (0.12, 0.115, 0.105)),
        "MAT_MWOF_Root_Dark": make_mat("MAT_MWOF_Root_Dark", (0.13, 0.065, 0.035)),
        "MAT_MWOF_Root_Strained": make_mat("MAT_MWOF_Root_Strained", (0.34, 0.18, 0.085)),
        "MAT_MWOF_Forest_Silhouette": make_mat("MAT_MWOF_Forest_Silhouette", (0.035, 0.075, 0.055)),
        "MAT_MWOF_Leaves_Muted": make_mat("MAT_MWOF_Leaves_Muted", (0.105, 0.22, 0.115)),
        "MAT_MWOF_Safe_Green": make_mat("MAT_MWOF_Safe_Green", (0.04, 0.26, 0.14)),
        "MAT_MWOF_Warm_Gold": make_mat(
            "MAT_MWOF_Warm_Gold", (0.86, 0.60, 0.24), emission=(0.62, 0.38, 0.10), strength=0.36
        ),
        "MAT_MWOF_Worldroot_Cyan": make_mat(
            "MAT_MWOF_Worldroot_Cyan", (0.03, 0.62, 0.74), emission=(0.01, 0.55, 0.82), strength=1.10, alpha=0.72
        ),
        "MAT_MWOF_Worldroot_Cyan_Dim": make_mat(
            "MAT_MWOF_Worldroot_Cyan_Dim", (0.07, 0.30, 0.34), emission=(0.01, 0.18, 0.24), strength=0.22, alpha=0.48
        ),
        "MAT_MWOF_Violet_Instability": make_mat(
            "MAT_MWOF_Violet_Instability", (0.30, 0.055, 0.40), emission=(0.35, 0.03, 0.55), strength=0.55, alpha=0.50
        ),
        "MAT_MWOF_Ruin_OranynPale": make_mat(
            "MAT_MWOF_Ruin_OranynPale", (0.68, 0.80, 0.72), emission=(0.10, 0.20, 0.16), strength=0.10, alpha=0.86
        ),
        "MAT_MWOF_Ruin_GhostCrack": make_mat(
            "MAT_MWOF_Ruin_GhostCrack", (0.28, 0.70, 0.78), emission=(0.05, 0.45, 0.56), strength=0.50, alpha=0.58
        ),
        "MAT_MWOF_Echo_Blue": make_mat(
            "MAT_MWOF_Echo_Blue", (0.42, 0.70, 0.92), emission=(0.12, 0.42, 0.75), strength=0.48, alpha=0.44
        ),
        "MAT_MWOF_Echo_Pool": make_mat(
            "MAT_MWOF_Echo_Pool", (0.075, 0.30, 0.34), emission=(0.01, 0.12, 0.16), strength=0.14, alpha=0.50
        ),
        "MAT_MWOF_Banner_Verdant": make_mat("MAT_MWOF_Banner_Verdant", (0.025, 0.23, 0.12)),
        "MAT_MWOF_Flower_Biolume": make_mat(
            "MAT_MWOF_Flower_Biolume", (0.12, 0.46, 0.36), emission=(0.025, 0.28, 0.22), strength=0.30
        ),
        "MAT_MWOF_Atmosphere": make_mat(
            "MAT_MWOF_Atmosphere", (0.06, 0.11, 0.13), emission=(0.02, 0.045, 0.055), strength=0.06, alpha=0.18
        ),
        "MAT_MWOF_LowMist_Cyan": make_mat(
            "MAT_MWOF_LowMist_Cyan", (0.04, 0.21, 0.25), emission=(0.01, 0.11, 0.15), strength=0.13, alpha=0.24
        ),
        "MAT_MWOF_Player": make_mat("MAT_MWOF_Player", (0.50, 0.68, 0.55), alpha=0.95),
        "MAT_MWOF_NPC": make_mat("MAT_MWOF_NPC", (0.66, 0.70, 0.53), alpha=0.92),
    }


def export_scene_glb(collections, out_glb):
    bpy.ops.object.select_all(action="DESELECT")
    for collection in collections.values():
        for obj in collection.objects:
            obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(out_glb), export_format="GLB", use_selection=True)


def make_ground_patch(name, collection, loc, rx, ry, mat, rot=0, height=0.18, sides=14):
    return irregular_slab_mesh(
        name,
        collection,
        loc,
        rx,
        ry,
        height,
        mat,
        sides=sides,
        rot_z=math.radians(rot),
        irregularity=0.16,
    )


def make_character(prefix, collection, loc, mats, mat_key, rot_z=0.0, staff=True, kneel=False):
    x, y, z = loc
    body_h = 0.92 if kneel else 1.22
    body = frustum_obj(
        f"{prefix}_BodyCloak",
        collection,
        (x, y, z + body_h * 0.50),
        0.26,
        0.42,
        body_h,
        6,
        mats[mat_key],
        rot=(0, 0, math.radians(rot_z)),
    )
    head_z = z + (1.20 if kneel else 1.58)
    head = bicone_mesh(
        f"{prefix}_Head",
        collection,
        (x, y, head_z),
        0.20,
        0.24,
        mats[mat_key],
        sides=7,
        rot=(math.radians(90), 0, math.radians(rot_z)),
    )
    cube_obj(
        f"{prefix}_ShoulderShape",
        collection,
        (x, y, z + (0.96 if kneel else 1.22)),
        (0.68, 0.18, 0.16),
        mats[mat_key],
        rot=(0, 0, math.radians(rot_z)),
    )
    if staff:
        cube_obj(
            f"{prefix}_StaffOrSpearSilhouette",
            collection,
            (x + 0.42 * math.cos(math.radians(rot_z + 50)), y + 0.42 * math.sin(math.radians(rot_z + 50)), z + 0.88),
            (0.06, 0.06, 1.55),
            mats["MAT_MWOF_Root_Strained"],
            rot=(math.radians(8), math.radians(3), math.radians(rot_z + 8)),
        )
    return [body, head]


def make_tree(prefix, collection, mats, x, y, scale=1.0, dead=False, lean=0.0):
    trunk_mat = mats["MAT_MWOF_Root_Strained"] if not dead else mats["MAT_MWOF_Root_Dark"]
    leaf_mat = mats["MAT_MWOF_Leaves_Muted"] if not dead else mats["MAT_MWOF_Forest_Silhouette"]
    trunk = frustum_obj(
        f"{prefix}_TwistedStrainedTrunk",
        collection,
        (x, y, 1.75 * scale),
        0.32 * scale,
        0.55 * scale,
        3.55 * scale,
        6,
        trunk_mat,
        rot=(math.radians(lean), math.radians(-lean * 0.45), math.radians(7 + x * 1.7)),
    )
    for i, angle in enumerate([20, 96, 172, 246, 308]):
        length = scale * (1.2 + 0.22 * math.sin(i + x))
        cube_obj(
            f"{prefix}_ExposedRootStrain_{i:02d}",
            collection,
            (x + math.cos(math.radians(angle)) * length * 0.42, y + math.sin(math.radians(angle)) * length * 0.42, 0.18),
            (length, 0.16 * scale, 0.14 * scale),
            mats["MAT_MWOF_Root_Dark"],
            rot=(0, math.radians(3), math.radians(angle)),
        )
    if not dead:
        for i, (ox, oy, oz, rx, rz) in enumerate(
            [(-0.25, 0.05, 3.65, 0.96, 5), (0.42, -0.05, 3.92, 0.86, -8), (0.05, 0.38, 4.14, 0.74, 17)]
        ):
            frustum_obj(
                f"{prefix}_AsymmetricMutedCanopyCluster_{i:02d}",
                collection,
                (x + ox * scale, y + oy * scale, oz * scale),
                0.64 * rx * scale,
                0.90 * rx * scale,
                0.52 * scale,
                7,
                leaf_mat,
                rot=(math.radians(90), math.radians(0), math.radians(rz)),
            )
    else:
        for i, angle in enumerate([72, 118, 240]):
            cube_obj(
                f"{prefix}_BareBranchClaw_{i:02d}",
                collection,
                (x + math.cos(math.radians(angle)) * 0.52 * scale, y + math.sin(math.radians(angle)) * 0.52 * scale, 3.15 * scale),
                (1.35 * scale, 0.11 * scale, 0.12 * scale),
                mats["MAT_MWOF_Root_Dark"],
                rot=(math.radians(22), 0, math.radians(angle)),
            )
    for i, z in enumerate([1.4, 2.3]):
        cube_obj(
            f"{prefix}_SubtleCyanMemoryCrack_{i:02d}",
            collection,
            (x + 0.36 * scale, y - 0.06 * scale, z * scale),
            (0.035 * scale, 0.035 * scale, 0.46 * scale),
            mats["MAT_MWOF_Worldroot_Cyan_Dim"],
            rot=(math.radians(5), 0, math.radians(lean + i * 14)),
        )
    return trunk


def build_terrain(collections, mats):
    col = collections["MW_OUTER_TERRAIN"]
    make_ground_patch("MW_OUTER_TERRAIN_MainGroundedMemoryWastesForestFloor_80x60", col, (0, 0, -0.05), 41, 31, mats["MAT_MWOF_Ground_DarkMoss"], rot=3, height=0.22, sides=18)
    make_ground_patch("MW_OUTER_TERRAIN_SickMossBank_LeftEchoFlat", col, (-19, -1, 0.08), 15, 12, mats["MAT_MWOF_Ground_SickMoss"], rot=-12)
    make_ground_patch("MW_OUTER_TERRAIN_StrainedRootBank_RightRuins", col, (18, 4, 0.10), 18, 12, mats["MAT_MWOF_Ground_MemoryMud"], rot=9)
    make_ground_patch("MW_OUTER_TERRAIN_BackRidge_DarkerForestEdge", col, (0, 26, 0.18), 39, 7.5, mats["MAT_MWOF_Forest_Silhouette"], rot=0, height=0.32, sides=13)
    for i, (x, y, rx, ry, rot) in enumerate([(-26, 20, 8, 3.2, -10), (-9, 24, 10, 3.6, 4), (14, 25, 11, 3.4, -3), (30, 20, 7, 3, 12)]):
        make_ground_patch(f"MW_OUTER_TERRAIN_BackgroundBrokenForestMound_{i:02d}", col, (x, y, 0.38), rx, ry, mats["MAT_MWOF_Ground_SickMoss"], rot=rot, height=0.34, sides=9)
    for i, (x, y, rx, ry, rot, mat_key) in enumerate(
        [
            (-16.5, -18.5, 8.2, 3.0, -11, "MAT_MWOF_Ground_MemoryMud"),
            (10.5, -14.0, 9.0, 3.4, 8, "MAT_MWOF_Ground_SickMoss"),
            (-8.5, 10.5, 7.4, 2.7, 16, "MAT_MWOF_Ground_MemoryMud"),
            (17.0, 17.0, 8.8, 2.8, -6, "MAT_MWOF_Ground_SickMoss"),
        ]
    ):
        make_ground_patch(
            f"MW_OUTER_TERRAIN_WoundedGroundLowRise_NotFlatPlate_{i:02d}",
            col,
            (x, y, 0.18 + i * 0.015),
            rx,
            ry,
            mats[mat_key],
            rot=rot,
            height=0.16,
            sides=11,
        )

    path_points = [
        (0.0, -27.5, 2.9, 1.15, -4),
        (0.8, -22.5, 3.0, 1.10, 3),
        (2.0, -17.2, 2.75, 1.05, 8),
        (1.0, -11.8, 2.45, 0.95, -7),
        (-1.8, -6.0, 2.40, 0.90, -12),
        (-3.5, 0.2, 2.15, 0.85, 5),
        (-1.0, 6.0, 2.25, 0.82, 12),
        (3.0, 11.5, 2.20, 0.78, -8),
        (6.5, 16.5, 2.10, 0.76, 5),
        (7.4, 21.4, 2.00, 0.72, 0),
    ]
    for i, (x, y, rx, ry, rot) in enumerate(path_points):
        make_ground_patch(
            f"MW_OUTER_TERRAIN_MainPath_DarkerWalkableSoilUnderlay_{i:02d}",
            col,
            (x, y, 0.19 + i * 0.01),
            rx + 0.85,
            ry + 0.36,
            mats["MAT_MWOF_Ground_MemoryMud"],
            rot=rot,
            height=0.075,
            sides=9,
        )
        make_ground_patch(f"MW_OUTER_TERRAIN_MainPath_MossyCrackedStoneSlab_{i:02d}", col, (x, y, 0.26 + i * 0.012), rx, ry, mats["MAT_MWOF_Path_MossyStone"], rot=rot, height=0.12, sides=8)
        if i % 2 == 0:
            cube_obj(f"MW_OUTER_TERRAIN_MainPath_DarkCrackLine_{i:02d}", col, (x + 0.15, y, 0.42), (rx * 0.72, 0.055, 0.035), mats["MAT_MWOF_Path_CrackedDark"], rot=(0, 0, math.radians(rot + 22)))
        for side, sx in [("Left", -1), ("Right", 1)]:
            cube_obj(
                f"MW_OUTER_TERRAIN_MainPath_RootBorder_{side}_{i:02d}",
                col,
                (x + sx * (rx + 0.28), y, 0.38),
                (0.72, 0.13, 0.12),
                mats["MAT_MWOF_Root_Dark"],
                rot=(0, math.radians(2 * sx), math.radians(rot + 12 * sx)),
            )
        if i in [2, 5, 8]:
            bicone_mesh(f"MW_OUTER_TERRAIN_SubtleCyanRouteRune_{i:02d}", col, (x, y + 0.48, 0.52), 0.09, 0.05, mats["MAT_MWOF_Worldroot_Cyan_Dim"], sides=5, rot=(math.radians(90), 0, math.radians(rot)))
    for i, (x, y, length, rot) in enumerate([(-5.2, -16.8, 4.4, 19), (4.6, -7.6, 5.1, -13), (-5.3, 2.2, 4.8, 25), (3.5, 14.4, 5.8, -17)]):
        cube_obj(
            f"MW_OUTER_TERRAIN_ExposedWorldrootRidge_CrossingPathEdge_{i:02d}",
            col,
            (x, y, 0.49),
            (length, 0.18, 0.13),
            mats["MAT_MWOF_Root_Dark"],
            rot=(math.radians(1.5), 0, math.radians(rot)),
        )
    for i, (x, y, sx, sy, rot) in enumerate([(-24, 5, 2.8, 0.8, 9), (24, -4, 3.1, 0.9, -16), (-13, 17, 2.6, 0.75, 22), (18, -19, 2.9, 0.85, -5)]):
        disc_mesh(
            f"MW_OUTER_TERRAIN_ShallowMemoryDepression_EdgeDetail_{i:02d}",
            col,
            (x, y, 0.39),
            sx,
            sy,
            mats["MAT_MWOF_Path_CrackedDark"],
            sides=14,
            rot=(0, 0, math.radians(rot)),
        )


def build_warden_camp(collections, mats):
    col = collections["MW_WARDEN_CAMP"]
    x, y = (-3.8, -25.2)
    disc_mesh("MW_WARDEN_CAMP_ProtectedContainmentGlow_WarmCyan", col, (x + 2.2, y + 1.2, 0.36), 4.8, 2.8, mats["MAT_MWOF_Worldroot_Cyan_Dim"], sides=24, rot=(0, 0, math.radians(7)))
    tri_prism_obj("MW_WARDEN_CAMP_RootShelter_LeafCanvas", col, (x, y, 0.55), 4.2, 2.7, 1.25, mats["MAT_MWOF_Safe_Green"], rot=(0, 0, math.radians(-8)))
    for i, dx in enumerate([-1.55, -0.35, 0.85]):
        cube_obj(f"MW_WARDEN_CAMP_RootShelter_BentLivingRib_{i:02d}", col, (x + dx, y - 0.12, 1.05), (0.14, 0.20, 1.65), mats["MAT_MWOF_Root_Strained"], rot=(math.radians(7), 0, math.radians(-8 + i * 4)))
    cube_obj("MW_WARDEN_CAMP_VerdantBanner_Pole", col, (x - 3.0, y + 1.9, 1.34), (0.12, 0.12, 2.6), mats["MAT_MWOF_Root_Strained"])
    cube_obj("MW_WARDEN_CAMP_VerdantBanner_Cloth_GreenGold", col, (x - 2.75, y + 1.9, 2.35), (0.08, 0.06, 1.25), mats["MAT_MWOF_Banner_Verdant"], rot=(0, 0, math.radians(-5)))
    cube_obj("MW_WARDEN_CAMP_VerdantBanner_GoldContainmentMark", col, (x - 2.69, y + 1.9, 2.48), (0.04, 0.08, 0.35), mats["MAT_MWOF_Warm_Gold"])
    cube_obj("MW_WARDEN_CAMP_ArchiveTable_RootWood", col, (x + 2.2, y + 0.4, 0.63), (1.55, 0.72, 0.18), mats["MAT_MWOF_Root_Strained"], rot=(0, 0, math.radians(9)))
    cube_obj("MW_WARDEN_CAMP_ArchiveTable_CyanMemoryMap", col, (x + 2.2, y + 0.4, 0.76), (1.14, 0.46, 0.035), mats["MAT_MWOF_Worldroot_Cyan_Dim"], rot=(0, 0, math.radians(9)))
    frustum_obj("MW_WARDEN_CAMP_MemoryBrazier_RootBowl", col, (x + 3.55, y + 1.65, 0.72), 0.40, 0.28, 0.48, 7, mats["MAT_MWOF_Root_Dark"])
    bicone_mesh("MW_WARDEN_CAMP_MemoryBrazier_ContainedCyanFlame", col, (x + 3.55, y + 1.65, 1.08), 0.18, 0.44, mats["MAT_MWOF_Worldroot_Cyan"], sides=7)
    make_character("MW_WARDEN_CAMP_NPC_WardenContainmentLead", col, (x + 1.2, y + 2.3, 0.42), mats, "MAT_MWOF_NPC", rot_z=8)
    make_character("MW_WARDEN_CAMP_NPC_WardenRootScout", col, (x + 4.45, y - 0.95, 0.42), mats, "MAT_MWOF_NPC", rot_z=-18)
    make_character("MW_WARDEN_CAMP_PlayerPlaceholder_EntryScale", col, (0, -30.0, 0.42), mats, "MAT_MWOF_Player", rot_z=0)
    for i, (dx, dy, length, rot) in enumerate([(-3.3, -1.6, 2.4, 18), (-1.0, -2.05, 2.1, -5), (2.1, -1.75, 2.5, -16), (4.8, 0.85, 1.9, 38)]):
        cube_obj(
            f"MW_WARDEN_CAMP_RootFence_FieldContainment_{i:02d}",
            col,
            (x + dx, y + dy, 0.72),
            (length, 0.12, 0.16),
            mats["MAT_MWOF_Root_Dark"],
            rot=(0, math.radians(2), math.radians(rot)),
        )
    for i, (dx, dy) in enumerate([(-3.6, -1.1), (-1.2, -2.4), (2.4, -2.0), (5.0, 0.25)]):
        frustum_obj(
            f"MW_WARDEN_CAMP_MarkerStone_ContainmentPerimeter_{i:02d}",
            col,
            (x + dx, y + dy, 0.60),
            0.24,
            0.34,
            0.46,
            5,
            mats["MAT_MWOF_Path_MossyStone"],
            rot=(0, 0, math.radians(i * 13)),
        )


def build_echo_flats(collections, mats):
    col = collections["MW_ECHO_FLATS"]
    make_ground_patch("MW_ECHO_FLATS_LowOpenMemoryLeakGround", col, (-17, -3, 0.20), 12, 8.5, mats["MAT_MWOF_Ground_SickMoss"], rot=-6, height=0.14, sides=13)
    for i, (x, y, rx, ry, rot) in enumerate([(-22, -5, 4.0, 1.25, -9), (-15, -1.7, 3.4, 1.1, 12), (-10, -5.5, 2.5, 0.8, 4)]):
        disc_mesh(f"MW_ECHO_FLATS_ShallowReflectiveMemoryPool_{i:02d}", col, (x, y, 0.42), rx, ry, mats["MAT_MWOF_Echo_Pool"], sides=28, rot=(0, 0, math.radians(rot)))
    for i, (x, y, rot) in enumerate([(-20.5, -0.8, 12), (-15.2, -6.2, -10), (-11.7, -1.4, 4)]):
        disc_mesh(f"MW_ECHO_FLATS_CircularMemoryStain_OldFootprint_{i:02d}", col, (x, y, 0.45), 1.35, 0.42, mats["MAT_MWOF_Worldroot_Cyan_Dim"], sides=18, rot=(0, 0, math.radians(rot)))
    for i, (x, y, rot) in enumerate([(-23.0, -1.5, -8), (-13.0, -7.3, 11)]):
        cube_obj(f"MW_ECHO_FLATS_BrokenBanner_Pole_{i:02d}", col, (x, y, 0.95), (0.08, 0.08, 1.40), mats["MAT_MWOF_Root_Dark"], rot=(math.radians(12), 0, math.radians(rot)))
        cube_obj(f"MW_ECHO_FLATS_TornMemoryBanner_Cloth_{i:02d}", col, (x + 0.30, y, 1.45), (0.55, 0.055, 0.42), mats["MAT_MWOF_Echo_Blue"], rot=(0, math.radians(6), math.radians(rot)))
    for i, (x, y, rot, kneel) in enumerate([(-19.4, -2.3, 34, True), (-15.8, -4.7, -5, False), (-11.7, -1.2, 72, False)]):
        make_character(
            f"MW_ECHO_FLATS_TranslucentMemorySilhouette_OverlapPastPresent_{i:02d}",
            col,
            (x, y, 0.48),
            mats,
            "MAT_MWOF_Echo_Blue",
            rot_z=rot,
            staff=False,
            kneel=kneel,
        )
    for i, (x, y, rot) in enumerate([(-20.6, -4.0, -18), (-18.2, -3.5, -12), (-15.9, -3.2, -4), (-13.6, -2.9, 6), (-11.6, -2.2, 11)]):
        cube_obj(
            f"MW_ECHO_FLATS_HalfVisibleRepeatedFootprint_{i:02d}",
            col,
            (x, y, 0.56),
            (0.34, 0.12, 0.025),
            mats["MAT_MWOF_Worldroot_Cyan_Dim"],
            rot=(0, 0, math.radians(rot)),
        )
    for i, (x, y, sx, rot) in enumerate([(-18.8, -0.1, 1.4, 9), (-14.1, -6.7, 1.2, -14)]):
        cube_obj(
            f"MW_ECHO_FLATS_HalfVisibleRepeatedPathFragment_{i:02d}",
            col,
            (x, y, 0.50),
            (sx, 0.18, 0.035),
            mats["MAT_MWOF_Ruin_GhostCrack"],
            rot=(0, 0, math.radians(rot)),
        )


def build_oranyn_ruins(collections, mats):
    col = collections["MW_ORANYN_RUINS"]
    make_ground_patch("MW_ORANYN_RUINS_HalfManifestedGroundShelf", col, (18, 1.5, 0.24), 12.5, 7.2, mats["MAT_MWOF_Ground_MemoryMud"], rot=7, height=0.18, sides=12)
    arch_x, arch_y = 18.5, 3.0
    cube_obj("MW_ORANYN_RUINS_BrokenArch_LeftPaleColumn", col, (arch_x - 2.2, arch_y, 2.55), (0.68, 0.62, 4.4), mats["MAT_MWOF_Ruin_OranynPale"], rot=(math.radians(0), math.radians(0), math.radians(-5)))
    cube_obj("MW_ORANYN_RUINS_BrokenArch_RightPaleColumn", col, (arch_x + 2.2, arch_y, 2.18), (0.64, 0.58, 3.75), mats["MAT_MWOF_Ruin_OranynPale"], rot=(math.radians(0), math.radians(0), math.radians(7)))
    xz_beam_obj("MW_ORANYN_RUINS_BrokenArch_LeftCrownSegment", col, arch_y, (arch_x - 2.15, 4.65), (arch_x - 0.35, 6.0), 0.58, 0.44, mats["MAT_MWOF_Ruin_OranynPale"])
    xz_beam_obj("MW_ORANYN_RUINS_BrokenArch_RightCrownSegment", col, arch_y, (arch_x + 0.45, 5.9), (arch_x + 2.15, 4.45), 0.58, 0.44, mats["MAT_MWOF_Ruin_OranynPale"])
    for i, (dx, dy, h, lean) in enumerate([(-5.4, -1.2, 2.8, -7), (-0.6, -3.2, 2.0, 4), (5.8, 1.6, 2.5, 11)]):
        cube_obj(f"MW_ORANYN_RUINS_PartialColumn_CloseGroundManifest_{i:02d}", col, (arch_x + dx, arch_y + dy, 0.55 + h * 0.5), (0.48, 0.48, h), mats["MAT_MWOF_Ruin_OranynPale"], rot=(math.radians(0), math.radians(lean * 0.25), math.radians(lean)))
        cube_obj(f"MW_ORANYN_RUINS_GhostCyanCrack_OnColumn_{i:02d}", col, (arch_x + dx + 0.24, arch_y + dy - 0.02, 1.1 + h * 0.55), (0.035, 0.035, 0.72), mats["MAT_MWOF_Ruin_GhostCrack"], rot=(math.radians(0), 0, math.radians(lean)))
    for i in range(5):
        cube_obj(f"MW_ORANYN_RUINS_IncompleteStairPiece_CloseToGround_{i:02d}", col, (12.3 + i * 1.15, -2.0 + i * 0.42, 0.55 + i * 0.16), (1.05, 0.58, 0.18), mats["MAT_MWOF_Ruin_OranynPale"], rot=(0, 0, math.radians(8)))
    for i, (dx, dy, z, rot) in enumerate([(-4.6, 2.7, 2.0, 12), (2.8, -2.9, 1.7, -8), (6.3, -0.4, 2.35, 18)]):
        cube_obj(f"MW_ORANYN_RUINS_FloatingWallFragment_LowManifest_{i:02d}", col, (arch_x + dx, arch_y + dy, z), (1.9, 0.28, 1.0), mats["MAT_MWOF_Ruin_OranynPale"], rot=(math.radians(4), math.radians(5), math.radians(rot)))
    for i, (dx, dy, sx, sy, rot) in enumerate([(-3.2, -4.0, 2.4, 0.22, -11), (4.1, 3.3, 2.1, 0.20, 17), (0.9, 4.4, 1.7, 0.18, 6)]):
        cube_obj(
            f"MW_ORANYN_RUINS_GhostCyanSeam_MisalignedWallPiece_{i:02d}",
            col,
            (arch_x + dx, arch_y + dy, 1.18 + i * 0.22),
            (sx, sy, 0.055),
            mats["MAT_MWOF_Ruin_GhostCrack"],
            rot=(math.radians(3), math.radians(1), math.radians(rot)),
        )


def build_rootshear_rift(collections, mats):
    col = collections["MW_ROOTSHEAR_RIFT"]
    make_ground_patch("LANDMARK_ROOTSHEAR_RIFT_MainCrackedGroundMound", col, (5.5, 18.0, 0.33), 12.5, 8.7, mats["MAT_MWOF_Ground_MemoryMud"], rot=-6, height=0.26, sides=13)
    disc_mesh("LANDMARK_ROOTSHEAR_RIFT_CyanMemoryLeak_GroundFissureGlow", col, (5.5, 18.0, 0.58), 6.8, 1.28, mats["MAT_MWOF_Worldroot_Cyan"], sides=34, rot=(0, 0, math.radians(7)))
    cube_obj("LANDMARK_ROOTSHEAR_RIFT_DarkSplitMouth_InPhysicalTerrain", col, (5.5, 18.0, 0.72), (9.8, 0.48, 0.12), mats["MAT_MWOF_Path_CrackedDark"], rot=(0, math.radians(2), math.radians(7)))
    cube_obj("MW_ROOTSHEAR_RIFT_VioletInstability_LeakEdgeLeft", col, (3.6, 18.2, 0.82), (3.8, 0.12, 0.10), mats["MAT_MWOF_Violet_Instability"], rot=(0, math.radians(2), math.radians(23)))
    for i, (dx, dy, length, rot) in enumerate([(-3.7, -0.6, 4.2, -18), (-1.6, 0.75, 5.5, 18), (1.0, -0.75, 5.0, -8), (3.4, 0.5, 3.8, 26)]):
        cube_obj(f"MW_ROOTSHEAR_RIFT_ExposedRootFiber_UnderStrain_{i:02d}", col, (5.5 + dx, 18.0 + dy, 0.93), (length, 0.16, 0.14), mats["MAT_MWOF_Root_Dark"], rot=(math.radians(2), 0, math.radians(rot)))
    for i, (dx, dy, height, lean, rot) in enumerate([(-2.4, 0.85, 3.4, -12, -18), (1.2, -0.65, 4.1, 9, 12), (3.0, 0.9, 2.9, 15, 28)]):
        frustum_obj(
            f"MW_ROOTSHEAR_RIFT_StrainedRootClaw_RisingFromFissure_{i:02d}",
            col,
            (5.5 + dx, 18.0 + dy, 0.82 + height * 0.48),
            0.12,
            0.28,
            height,
            6,
            mats["MAT_MWOF_Root_Dark"],
            rot=(math.radians(lean), math.radians(lean * 0.3), math.radians(rot)),
        )
    for i, (dx, dy, sx, sy, rot) in enumerate([(-5.0, 1.6, 2.4, 1.1, -16), (-1.6, -2.4, 2.0, 0.9, 7), (2.8, 2.2, 2.8, 1.2, 22), (5.5, -1.0, 2.1, 0.9, -4)]):
        cube_obj(f"MW_ROOTSHEAR_RIFT_BrokenTerrainPlate_AroundFissure_{i:02d}", col, (5.5 + dx, 18.0 + dy, 0.92), (sx, sy, 0.20), mats["MAT_MWOF_Ground_SickMoss"], rot=(math.radians(2 + i), math.radians(i * 1.5), math.radians(rot)))
    for i, (dx, dy, z, mat_key) in enumerate(
        [
            (-2.6, -0.2, 2.2, "MAT_MWOF_Worldroot_Cyan"),
            (-1.0, 0.8, 2.9, "MAT_MWOF_Worldroot_Cyan_Dim"),
            (0.8, 0.4, 3.2, "MAT_MWOF_Violet_Instability"),
            (2.5, -0.6, 2.25, "MAT_MWOF_Worldroot_Cyan_Dim"),
            (-3.5, 1.1, 1.85, "MAT_MWOF_Worldroot_Cyan_Dim"),
            (3.6, 0.9, 2.7, "MAT_MWOF_Worldroot_Cyan"),
            (0.2, -1.35, 2.0, "MAT_MWOF_Violet_Instability"),
        ]
    ):
        bicone_mesh(
            f"MW_ROOTSHEAR_RIFT_FloatingMemoryShard_Controlled_{i:02d}",
            col,
            (5.5 + dx, 18.0 + dy, z),
            0.23,
            0.78,
            mats[mat_key],
            sides=5,
            rot=(math.radians(8), math.radians(0), math.radians(i * 28)),
        )
    for i, (dx, dy, sx, sy, rot) in enumerate([(-1.8, -0.4, 3.8, 0.55, 6), (1.8, 0.5, 3.2, 0.46, -11), (0.0, 1.25, 4.2, 0.42, 18)]):
        disc_mesh(
            f"LANDMARK_ROOTSHEAR_RIFT_SubtleLowMistAroundFissure_{i:02d}",
            col,
            (5.5 + dx, 18.0 + dy, 0.98 + i * 0.05),
            sx,
            sy,
            mats["MAT_MWOF_LowMist_Cyan"],
            sides=20,
            rot=(0, 0, math.radians(rot)),
        )


def build_memory_ghosts(collections, mats):
    col = collections["MW_MEMORY_GHOSTS"]
    make_character("MW_MEMORY_GHOSTS_KneelingEcho_ByPool", col, (-18.8, -2.8, 0.48), mats, "MAT_MWOF_Echo_Blue", rot_z=35, staff=False, kneel=True)
    make_character("MW_MEMORY_GHOSTS_WalkingEcho_OnBrokenPath", col, (-2.2, 4.0, 0.52), mats, "MAT_MWOF_Echo_Blue", rot_z=-4, staff=False)
    make_character("MW_MEMORY_GHOSTS_ReachingEcho_TowardOranynRuin", col, (14.0, 1.1, 0.55), mats, "MAT_MWOF_Echo_Blue", rot_z=74, staff=False)
    make_character("MW_MEMORY_GHOSTS_FadingEcho_NearRootshear", col, (1.8, 15.2, 0.60), mats, "MAT_MWOF_Echo_Blue", rot_z=16, staff=False)


def build_props_and_foliage(collections, mats):
    props = collections["MW_PROPS"]
    fol = collections["MW_FOLIAGE"]
    for i, (x, y, rot) in enumerate([(-7, -16, 12), (5.2, -9.2, -7), (-8.6, 5.5, 20), (10.8, 10.2, -14)]):
        cube_obj(f"MW_PROPS_BrokenContainmentStake_Rootbound_{i:02d}", props, (x, y, 0.86), (0.16, 0.16, 1.55), mats["MAT_MWOF_Root_Dark"], rot=(math.radians(7), 0, math.radians(rot)))
        bicone_mesh(f"MW_PROPS_SmallCyanMemoryRune_OnValidRoute_{i:02d}", props, (x + 0.25, y + 0.12, 1.55), 0.08, 0.18, mats["MAT_MWOF_Worldroot_Cyan_Dim"], sides=5)
    for i, (x, y) in enumerate([(-28, 12), (-24, 22), (-9, 27), (12, 29), (27, 20), (35, 7), (-34, -5), (31, -13)]):
        make_tree(f"MW_FOLIAGE_ForestEdge_StrainedWorldrootTree_{i:02d}", fol, mats, x, y, scale=1.35 + (i % 3) * 0.18, dead=i in [1, 4, 6], lean=(-7 + i * 2))
    for i, (x, y, sx, sy) in enumerate([(-30, 5, 4.2, 1.2), (-23, -14, 3.6, 1.0), (22, -10, 4.0, 1.1), (25, 11, 3.5, 0.9), (-5, 20, 4.8, 1.2)]):
        disc_mesh(f"MW_FOLIAGE_Cluster_DarkMossAndBroadLeaves_{i:02d}", fol, (x, y, 0.47), sx, sy, mats["MAT_MWOF_Leaves_Muted"], sides=12, rot=(0, 0, math.radians(i * 17)))
        for j in range(4):
            bicone_mesh(f"MW_FOLIAGE_BiolumeMemoryPlant_{i:02d}_{j:02d}", fol, (x - sx * 0.35 + j * sx * 0.23, y + sy * 0.22 * math.sin(j), 0.72), 0.06, 0.28, mats["MAT_MWOF_Flower_Biolume"], sides=5)
    for i, (x, y, scale, dead, lean) in enumerate(
        [
            (-38, 31, 1.55, True, -10),
            (-28, 33, 1.85, False, 8),
            (-17, 34, 1.70, True, -5),
            (-5, 35, 2.05, False, 4),
            (9, 35, 1.82, True, 10),
            (21, 33, 1.95, False, -7),
            (33, 31, 1.62, True, 12),
        ]
    ):
        make_tree(f"MW_FOLIAGE_BackgroundForestWall_LowPolyTrunkSilhouette_{i:02d}", fol, mats, x, y, scale=scale, dead=dead, lean=lean)
    for i, (x, y, scale, dead, lean) in enumerate(
        [
            (-42, 34, 2.55, True, -12),
            (-31, 35, 2.85, False, 8),
            (-19, 36, 2.70, True, -5),
            (-6, 36, 3.05, False, 4),
            (8, 36, 2.92, True, 10),
            (22, 35, 3.00, False, -7),
            (36, 34, 2.62, True, 12),
        ]
    ):
        make_tree(f"MW_FOLIAGE_DistantForestWall_TallStrainedWorldrootMass_{i:02d}", fol, mats, x, y, scale=scale, dead=dead, lean=lean)


def build_atmosphere(collections, mats):
    col = collections["MW_ATMOSPHERE"]
    for i, (x, y, sx, sy, rot) in enumerate([(-1.2, 16.4, 7.8, 0.9, 4), (6.4, 19.4, 6.2, 0.72, -13), (1.5, 22.0, 5.0, 0.6, 17)]):
        mist = disc_mesh(
            f"MW_ATMOSPHERE_RootshearLowGroundMemoryMist_NoBackdropPanel_{i:02d}",
            col,
            (x, y, 0.74 + i * 0.04),
            sx,
            sy,
            mats["MAT_MWOF_LowMist_Cyan"],
            sides=22,
            rot=(0, 0, math.radians(rot)),
        )
        disable_shadow(mist)


def setup_lighting_and_cameras(collections):
    lighting = collections["MW_LIGHTING"]
    cameras = collections["MW_CAMERA"]
    bpy.ops.object.light_add(type="SUN", location=(-22, -18, 34), rotation=(math.radians(50), 0, math.radians(-35)))
    sun = bpy.context.object
    sun.name = "MW_LIGHTING_MutedOuterFringeSun"
    sun.data.energy = 1.15
    sun.data.angle = math.radians(8.0)
    lighting.objects.link(sun)
    bpy.context.scene.collection.objects.unlink(sun)
    for name, loc, color, energy, radius in [
        ("MW_LIGHTING_RootshearCyanMemoryLeak", (5.5, 18.0, 3.2), (0.12, 0.75, 1.0), 720, 7.0),
        ("MW_LIGHTING_RootshearVioletLeak", (3.2, 18.7, 2.0), (0.55, 0.12, 0.85), 160, 4.2),
        ("MW_LIGHTING_WardenCampProtectedGlow", (-1.2, -23.0, 2.4), (0.32, 0.86, 0.52), 210, 5.0),
        ("MW_LIGHTING_BrazierWarmGold", (-0.25, -23.5, 1.65), (1.0, 0.62, 0.22), 105, 2.4),
        ("MW_LIGHTING_EchoFlatsColdMemoryGlow", (-17.0, -3.5, 1.7), (0.20, 0.55, 0.95), 130, 5.4),
        ("MW_LIGHTING_OranynGhostCrackGlow", (18.0, 3.0, 3.0), (0.42, 0.95, 0.86), 120, 4.5),
    ]:
        bpy.ops.object.light_add(type="POINT", location=loc)
        light = bpy.context.object
        light.name = name
        light.data.color = color
        light.data.energy = energy
        light.data.shadow_soft_size = radius
        lighting.objects.link(light)
        bpy.context.scene.collection.objects.unlink(light)

    bpy.ops.object.camera_add(location=(0, -42.0, 5.7))
    gameplay = bpy.context.object
    gameplay.name = "MW_CAMERA_Gameplay_OuterFringeThirdPerson"
    gameplay.data.lens = 25
    gameplay.data.dof.use_dof = False
    look_at(gameplay, Vector((3.2, 8.0, 2.0)))
    cameras.objects.link(gameplay)
    bpy.context.scene.collection.objects.unlink(gameplay)
    bpy.context.scene.camera = gameplay

    bpy.ops.object.camera_add(location=(0, 0, 82))
    top = bpy.context.object
    top.name = "MW_CAMERA_TopDown_OuterFringeReadability"
    top.data.lens = 34
    look_at(top, Vector((0, 0, 0)))
    cameras.objects.link(top)
    bpy.context.scene.collection.objects.unlink(top)
    return gameplay, top


def build_manifest(collections, mats):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    objects = []
    object_counts = {}
    total_tris = 0
    for collection_name, collection in collections.items():
        object_counts[collection_name] = len(collection.objects)
        for obj in collection.objects:
            tris = object_triangles(obj, depsgraph)
            total_tris += tris
            objects.append(
                {
                    "name": obj.name,
                    "collection": collection_name,
                    "type": obj.type,
                    "location": [round(v, 3) for v in obj.location],
                    "triangles": tris,
                    "materials": [slot.material.name for slot in obj.material_slots if slot.material],
                }
            )
    unnamed = [obj.name for obj in bpy.data.objects if obj.name.startswith(("Cube", "Plane", "Cylinder", "Cone", "Sphere"))]
    non_prefixed = sorted({mat.name for mat in bpy.data.materials if not mat.name.startswith("MAT_")})
    return {
        "scene": "Memory Wastes - Outer Fringe",
        "build": "v002",
        "scope": "grounded first playable edge of the Memory Wastes",
        "collections": list(collections.keys()),
        "object_counts": object_counts,
        "object_total": sum(object_counts.values()),
        "material_names": sorted(mats.keys()),
        "approximate_total_triangles": total_tris,
        "major_landmarks": {
            "warden_camp": "foreground containment camp around y=-25",
            "main_path": "curved broken mossy root-stone path from player spawn toward Rootshear Rift",
            "echo_flats": "low memory leak area at left midground",
            "oranyn_ruins": "half-manifested ruin cluster at right midground",
            "rootshear_rift": "main far-mid focal fissure around (5.5, 18, 0)",
            "forest_edge": "dark strained Worldroot forest wall around the scene boundary",
        },
        "outputs": {
            "blend": str(OUT_BLEND),
            "glb": str(OUT_GLB),
            "gameplay_render": str(OUT_GAMEPLAY),
            "topdown_render": str(OUT_TOPDOWN),
            "report": str(OUT_REPORT),
        },
        "qa": {
            "unnamed_primitives": unnamed,
            "non_prefixed_materials": non_prefixed,
            "grounded_ratio_target": "about 80 percent physical terrain / 20 percent floating memory fragments",
        },
        "objects": objects,
    }


def write_report(manifest):
    report = f"""# Memory Wastes - Outer Fringe v002 Report

## Scope
- Scene: Memory Wastes - Outer Fringe
- Build: v002
- Output blend: {OUT_BLEND}
- Output GLB: {OUT_GLB}
- Gameplay render: {OUT_GAMEPLAY}
- Top-down render: {OUT_TOPDOWN}
- Manifest: {OUT_MANIFEST}

## What Changed
- Continued from the grounded Outer Fringe scene instead of returning to floating archipelago/endgame Memory Wastes.
- Strengthened LANDMARK_ROOTSHEAR_RIFT with a wider cracked fissure, exposed root fibers, broken plates, low cyan mist and seven restrained memory shards.
- Improved the main path with darker walkable underlay, mossy stone slabs, root borders, cross-roots, crack lines and only subtle route-runes.
- Clarified Warden Camp as a compact containment foothold with root fencing, marker stones, archive table, brazier and two Warden placeholders.
- Expanded Echo Flats with shallow pools, three translucent silhouettes, memory stains, repeated footprints/path fragments and controlled cyan glow.
- Added more Oranyn ruin readability through arch/column/stair silhouettes, misaligned ghost seams and low floating wall fragments.
- Added low-poly terrain variation, shallow depressions, strained forest edge trees and purpose-placed foliage instead of random neon markers.

## Required Answers
1. Is the scene still grounded rather than floating?
   Yes. It remains an approximately 80m x 60m grounded forest-edge space. Floating details are restrained to memory shards near the rift and half-manifested Oranyn fragments.

2. Is the Rootshear Rift now the main focal point?
   Yes. LANDMARK_ROOTSHEAR_RIFT is the strongest far-midground landmark, with the largest glow, strongest crack shape, exposed roots and upward shard motion.

3. Is the main path readable?
   Yes. The route reads Player start -> Warden Camp -> Echo Flats -> Oranyn Ruins -> Rootshear Rift through contrasting mossy stone slabs, root borders and subtle route runes.

4. Does the Warden Camp read as a safe foothold?
   Yes. It is compact, low and warmer than the rest of the zone, with Verdant banner, shelter, archive table, brazier, containment fence and Warden silhouettes.

5. Does Echo Flats read as a memory-leak area?
   Yes. The shallow reflective pools, ghost-blue silhouettes, repeated footprints, circular stains and cyan water/crack glow create a clear past-overlapping-present read.

6. Do the Oranyn Ruins read as half-manifested ruins?
   Yes. The island uses a broken arch, partial columns, incomplete stair fragments, misaligned wall pieces, ghost-cyan seams and pale white-green material.

7. What still reads as placeholder?
   Player/NPC/ghost silhouettes are still intentional placeholders. Terrain is improved for visual readability but is not final collision-ready terrain. Materials are controlled stylized color direction, not final hand-painted texture assets.

## Technical QA
- Collections: {len(manifest["collections"])}
- Objects: {manifest["object_total"]}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_REPORT.write_text(report, encoding="utf-8")


def build_scene():
    ensure_dirs()
    clear_scene()
    setup_scene_settings((0.035, 0.050, 0.055), exposure=-0.06)
    collections = create_collections()
    mats = outer_materials()
    build_terrain(collections, mats)
    build_warden_camp(collections, mats)
    build_echo_flats(collections, mats)
    build_oranyn_ruins(collections, mats)
    build_rootshear_rift(collections, mats)
    build_memory_ghosts(collections, mats)
    build_props_and_foliage(collections, mats)
    build_atmosphere(collections, mats)
    gameplay_camera, topdown_camera = setup_lighting_and_cameras(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_scene_glb(collections, OUT_GLB)
    bpy.context.scene.camera = gameplay_camera
    render_to(str(OUT_GAMEPLAY), 2560, 1440)
    bpy.context.scene.camera = topdown_camera
    render_to(str(OUT_TOPDOWN), 2560, 1440)
    bpy.context.scene.camera = gameplay_camera
    manifest = build_manifest(collections, mats)
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    write_report(manifest)


if __name__ == "__main__":
    build_scene()
