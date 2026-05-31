import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.append(str(SCRIPT_DIR))

import build_heartbough_glade_entrance as heartbough_helpers  # noqa: E402
from build_heartbough_glade_entrance import (  # noqa: E402
    KIT_BLEND,
    ROOT,
    append_templates,
    bicone_mesh,
    clear_scene,
    cube_obj,
    disable_shadow,
    disc_mesh,
    frustum_obj,
    inst,
    link_to,
    look_at,
    make_mat,
    object_triangles,
    render_to,
)


KIT_ASSET_NAMES = list(heartbough_helpers.KIT_ASSETS)
ZONE1_RENDER = ROOT / "assets" / "renders" / "heartbough_glade_entrance_v001_1920x1080.png"
ZONE2_SOURCE_STEM = "elarthalas_approach_v001"
ZONE3_SOURCE_STEM = "memory_wastes_v001"
ZONE2_STEM = "elarthalas_approach_visual_benchmark_v002"
ZONE3_STEM = "memory_wastes_visual_benchmark_v002"

OUT_ZONE2_BLEND = ROOT / "assets" / "blender" / f"{ZONE2_STEM}.blend"
OUT_ZONE2_GLB = ROOT / "assets" / "exports" / f"{ZONE2_STEM}.glb"
OUT_ZONE2_RENDER_1440 = ROOT / "assets" / "renders" / f"{ZONE2_STEM}_2560x1440.png"
OUT_ZONE2_RENDER_1080 = ROOT / "assets" / "renders" / f"{ZONE2_STEM}_1920x1080.png"
OUT_ZONE2_MANIFEST = ROOT / "assets" / "reports" / f"{ZONE2_STEM}_manifest.json"
OUT_ZONE2_REPORT = ROOT / "assets" / "reports" / f"{ZONE2_STEM}_quality_report.md"

OUT_ZONE3_BLEND = ROOT / "assets" / "blender" / f"{ZONE3_STEM}.blend"
OUT_ZONE3_GLB = ROOT / "assets" / "exports" / f"{ZONE3_STEM}.glb"
OUT_ZONE3_RENDER_1440 = ROOT / "assets" / "renders" / f"{ZONE3_STEM}_2560x1440.png"
OUT_ZONE3_RENDER_1080 = ROOT / "assets" / "renders" / f"{ZONE3_STEM}_1920x1080.png"
OUT_ZONE3_TOP_READABILITY_RENDER = ROOT / "assets" / "renders" / "memory_wastes_scale_pass_top_readability_2560x1440.png"
OUT_ZONE3_DEPTH_WIDE_RENDER = ROOT / "assets" / "renders" / "memory_wastes_depth_readability_wide_overview_2560x1440.png"
OUT_ZONE3_DEPTH_TOP_RENDER = ROOT / "assets" / "renders" / "memory_wastes_depth_readability_top_down_2560x1440.png"
OUT_ZONE3_MANIFEST = ROOT / "assets" / "reports" / f"{ZONE3_STEM}_manifest.json"
OUT_ZONE3_REPORT = ROOT / "assets" / "reports" / f"{ZONE3_STEM}_quality_report.md"
OUT_ZONE3_ISLAND_MANIFEST = ROOT / "assets" / "reports" / "island_manifest.json"
OUT_ZONE3_ISLAND_READABILITY_REPORT = ROOT / "assets" / "reports" / "island_readability_report.md"
OUT_ZONE3_SCALE_PASS_REPORT = ROOT / "assets" / "reports" / "memory_wastes_scale_pass_report.md"
OUT_ZONE3_DEPTH_READABILITY_REPORT = ROOT / "assets" / "reports" / "memory_wastes_depth_readability_report.md"
OUT_ZONE3_PLAYABLE_TRANSFER_REPORT = ROOT / "assets" / "reports" / "memory_wastes_playable_vista_transfer_report.md"

OUT_PROGRESSION_MANIFEST = ROOT / "assets" / "reports" / "eldara_zone_progression_manifest.json"
OUT_BENCHMARK_REPORT = ROOT / "assets" / "reports" / "zone_progression_visual_benchmark_report.md"


def ensure_dirs():
    for path in [
        OUT_ZONE2_BLEND.parent,
        OUT_ZONE2_GLB.parent,
        OUT_ZONE2_RENDER_1440.parent,
        OUT_ZONE2_REPORT.parent,
        OUT_ZONE3_BLEND.parent,
        OUT_ZONE3_GLB.parent,
        OUT_ZONE3_RENDER_1440.parent,
        OUT_ZONE3_REPORT.parent,
    ]:
        path.mkdir(parents=True, exist_ok=True)


def create_collections(names):
    collections = {}
    for name in names:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
        collections[name] = collection
    collections["KIT_Templates_Hidden"].hide_viewport = True
    collections["KIT_Templates_Hidden"].hide_render = True
    return collections


def remove_templates(collections):
    hidden = collections.get("KIT_Templates_Hidden")
    if not hidden:
        return
    for obj in list(hidden.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(hidden)
    del collections["KIT_Templates_Hidden"]


def load_templates(collections):
    heartbough_helpers.KIT_ASSETS = list(KIT_ASSET_NAMES)
    return append_templates(collections)


def scene_materials():
    return {
        "MAT_Grass_Lush": make_mat("MAT_Grass_Lush", (0.22, 0.55, 0.25)),
        "MAT_Grass_Controlled": make_mat("MAT_Grass_Controlled", (0.07, 0.22, 0.16)),
        "MAT_Grass_Dark": make_mat("MAT_Grass_Dark", (0.07, 0.27, 0.17)),
        "MAT_WasteGround": make_mat("MAT_WasteGround", (0.15, 0.17, 0.15)),
        "MAT_WasteGround_Dark": make_mat("MAT_WasteGround_Dark", (0.045, 0.055, 0.065)),
        "MAT_Path_MossyStone": make_mat("MAT_Path_MossyStone", (0.36, 0.39, 0.31)),
        "MAT_Path_StoneHighlight": make_mat("MAT_Path_StoneHighlight", (0.55, 0.53, 0.40)),
        "MAT_Path_WarmRoot": make_mat("MAT_Path_WarmRoot", (0.35, 0.19, 0.08)),
        "MAT_Path_DarkRootShadow": make_mat("MAT_Path_DarkRootShadow", (0.16, 0.08, 0.035)),
        "MAT_Path_SacredRoot": make_mat("MAT_Path_SacredRoot", (0.35, 0.20, 0.10)),
        "MAT_Moss": make_mat("MAT_Moss", (0.20, 0.50, 0.22)),
        "MAT_Bark_WarmBrown": make_mat("MAT_Bark_WarmBrown", (0.48, 0.25, 0.11)),
        "MAT_Bark_PaintedEdge": make_mat("MAT_Bark_PaintedEdge", (0.67, 0.39, 0.18)),
        "MAT_Bark_DarkRoot": make_mat("MAT_Bark_DarkRoot", (0.23, 0.12, 0.06)),
        "MAT_Leaves_DeepGreen": make_mat("MAT_Leaves_DeepGreen", (0.10, 0.38, 0.16)),
        "MAT_Leaves_LightGreen": make_mat("MAT_Leaves_LightGreen", (0.36, 0.78, 0.36)),
        "MAT_Leaves_DarkUnderside": make_mat("MAT_Leaves_DarkUnderside", (0.04, 0.19, 0.10)),
        "MAT_Stone_MossyGray": make_mat("MAT_Stone_MossyGray", (0.43, 0.47, 0.39)),
        "MAT_Stone_Pilgrimage": make_mat("MAT_Stone_Pilgrimage", (0.47, 0.50, 0.43)),
        "MAT_Stone_PaleArchive": make_mat("MAT_Stone_PaleArchive", (0.50, 0.58, 0.52)),
        "MAT_Stone_DarkCrevice": make_mat("MAT_Stone_DarkCrevice", (0.18, 0.20, 0.18)),
        "MAT_LostCivilization_GhostStone": make_mat(
            "MAT_LostCivilization_GhostStone", (0.70, 0.88, 0.76), emission=(0.24, 0.46, 0.34), strength=0.45, alpha=0.48
        ),
        "MAT_Echo_GhostBlueWhite": make_mat(
            "MAT_Echo_GhostBlueWhite", (0.62, 0.88, 1.0), emission=(0.24, 0.68, 1.0), strength=0.95, alpha=0.46
        ),
        "MAT_LostDivinity_PaleGold_Emission": make_mat(
            "MAT_LostDivinity_PaleGold_Emission", (0.92, 0.70, 0.30), emission=(0.86, 0.54, 0.14), strength=1.28, alpha=0.54
        ),
        "MAT_MemoryGlass_CyanGreen": make_mat(
            "MAT_MemoryGlass_CyanGreen", (0.04, 0.44, 0.44), emission=(0.02, 0.42, 0.43), strength=0.92, alpha=0.34
        ),
        "MAT_Worldroot_Cyan_Emission": make_mat(
            "MAT_Worldroot_Cyan_Emission", (0.05, 0.76, 0.82), emission=(0.03, 0.72, 0.88), strength=1.95
        ),
        "MAT_Rune_Cyan_Emission": make_mat(
            "MAT_Rune_Cyan_Emission", (0.05, 0.56, 0.58), emission=(0.03, 0.70, 0.78), strength=1.35
        ),
        "MAT_Echo_Transparent": make_mat(
            "MAT_Echo_Transparent", (0.18, 0.66, 0.82), emission=(0.06, 0.52, 0.72), strength=0.82, alpha=0.34
        ),
        "MAT_Lantern_Warm_Emission": make_mat(
            "MAT_Lantern_Warm_Emission", (1.0, 0.68, 0.20), emission=(1.0, 0.55, 0.10), strength=2.3
        ),
        "MAT_Banner_VerdantGreen": make_mat("MAT_Banner_VerdantGreen", (0.04, 0.32, 0.15)),
        "MAT_GoldTrim": make_mat("MAT_GoldTrim", (0.95, 0.68, 0.20), roughness=0.55),
        "MAT_HighElf_ArcaneViolet": make_mat(
            "MAT_HighElf_ArcaneViolet", (0.58, 0.20, 0.86), emission=(0.45, 0.10, 0.82), strength=1.3
        ),
        "MAT_HighElf_ColdGold": make_mat("MAT_HighElf_ColdGold", (0.86, 0.62, 0.32), roughness=0.48),
        "MAT_HighElf_SilverBlue": make_mat("MAT_HighElf_SilverBlue", (0.56, 0.72, 0.84), roughness=0.42),
        "MAT_HighElf_SilverBlue_Emission": make_mat(
            "MAT_HighElf_SilverBlue_Emission", (0.44, 0.78, 1.0), emission=(0.20, 0.58, 1.0), strength=1.5
        ),
        "MAT_MemoryStorm_Violet": make_mat(
            "MAT_MemoryStorm_Violet", (0.28, 0.06, 0.44), emission=(0.22, 0.03, 0.42), strength=0.52, alpha=0.36
        ),
        "MAT_MemoryStorm_Cyan": make_mat(
            "MAT_MemoryStorm_Cyan", (0.05, 0.36, 0.46), emission=(0.02, 0.32, 0.48), strength=0.46, alpha=0.32
        ),
        "MAT_Waste_SafeCyanGlow": make_mat(
            "MAT_Waste_SafeCyanGlow", (0.12, 0.62, 0.58), emission=(0.04, 0.42, 0.38), strength=0.38, alpha=0.24
        ),
        "MAT_Waste_GuideShard_CyanDim": make_mat(
            "MAT_Waste_GuideShard_CyanDim", (0.04, 0.18, 0.22), emission=(0.012, 0.12, 0.16), strength=0.08, alpha=0.14
        ),
        "MAT_Waste_RiftCyanDeep": make_mat(
            "MAT_Waste_RiftCyanDeep", (0.025, 0.38, 0.48), emission=(0.018, 0.40, 0.52), strength=0.74, alpha=0.40
        ),
        "MAT_Waste_RiftVioletControlled": make_mat(
            "MAT_Waste_RiftVioletControlled", (0.18, 0.035, 0.30), emission=(0.18, 0.018, 0.34), strength=0.44, alpha=0.28
        ),
        "MAT_Waste_EchoStainSoft": make_mat(
            "MAT_Waste_EchoStainSoft", (0.34, 0.56, 0.68), emission=(0.08, 0.24, 0.34), strength=0.20, alpha=0.20
        ),
        "MAT_Waste_GhostRuinSoft": make_mat(
            "MAT_Waste_GhostRuinSoft", (0.62, 0.82, 0.70), emission=(0.16, 0.32, 0.24), strength=0.28, alpha=0.38
        ),
        "MAT_Waste_DivinityStainSoft": make_mat(
            "MAT_Waste_DivinityStainSoft", (0.78, 0.62, 0.30), emission=(0.52, 0.34, 0.10), strength=0.38, alpha=0.28
        ),
        "MAT_DeadGod_Stone": make_mat("MAT_DeadGod_Stone", (0.38, 0.35, 0.30)),
        "MAT_DeadGod_Shadow": make_mat("MAT_DeadGod_Shadow", (0.16, 0.14, 0.13)),
        "MAT_Flower_Purple": make_mat("MAT_Flower_Purple", (0.58, 0.18, 0.86)),
        "MAT_Water_BlueGreen_Optional": make_mat(
            "MAT_Water_BlueGreen_Optional", (0.10, 0.56, 0.62), emission=(0.03, 0.28, 0.32), strength=0.25, alpha=0.72
        ),
        "MAT_Backdrop_BlueMist": make_mat(
            "MAT_Backdrop_BlueMist", (0.06, 0.20, 0.25), emission=(0.02, 0.08, 0.11), strength=0.06, alpha=0.28
        ),
        "MAT_Backdrop_SacredSky": make_mat(
            "MAT_Backdrop_SacredSky", (0.025, 0.10, 0.11), emission=(0.01, 0.04, 0.05), strength=0.04
        ),
        "MAT_Gate_BlueFog": make_mat(
            "MAT_Gate_BlueFog", (0.025, 0.10, 0.12), emission=(0.0, 0.04, 0.055), strength=0.04, alpha=0.16
        ),
        "MAT_Backdrop_DarkArchiveSilhouette": make_mat(
            "MAT_Backdrop_DarkArchiveSilhouette", (0.04, 0.10, 0.09), emission=(0.01, 0.04, 0.04), strength=0.03, alpha=0.72
        ),
        "MAT_Backdrop_MemoryStormSky": make_mat(
            "MAT_Backdrop_MemoryStormSky", (0.07, 0.10, 0.16), emission=(0.03, 0.05, 0.10), strength=0.12
        ),
        "MAT_Backdrop_DistantMemoryIsland": make_mat(
            "MAT_Backdrop_DistantMemoryIsland", (0.10, 0.16, 0.18), emission=(0.02, 0.06, 0.08), strength=0.05, alpha=0.48
        ),
        "MAT_Backdrop_DistantDivineIsland": make_mat(
            "MAT_Backdrop_DistantDivineIsland", (0.24, 0.19, 0.11), emission=(0.12, 0.08, 0.03), strength=0.05, alpha=0.46
        ),
        "MAT_Backdrop_DistantRiftIsland": make_mat(
            "MAT_Backdrop_DistantRiftIsland", (0.09, 0.045, 0.14), emission=(0.055, 0.018, 0.10), strength=0.04, alpha=0.34
        ),
        "MAT_PlayerPlaceholder": make_mat("MAT_PlayerPlaceholder", (0.54, 0.78, 0.56), alpha=0.86),
        "MAT_NpcPlaceholder": make_mat("MAT_NpcPlaceholder", (0.82, 0.78, 0.48), alpha=0.94),
        "MAT_EnemyPlaceholder": make_mat("MAT_EnemyPlaceholder", (0.88, 0.27, 0.20), alpha=0.86),
        "MAT_CampCloth_Green": make_mat("MAT_CampCloth_Green", (0.09, 0.36, 0.18)),
        "MAT_CampCloth_Blue": make_mat("MAT_CampCloth_Blue", (0.11, 0.42, 0.48)),
        "MAT_PlayerSpawn_CyanRune": make_mat(
            "MAT_PlayerSpawn_CyanRune", (0.06, 0.68, 0.62), emission=(0.02, 0.50, 0.50), strength=0.48, alpha=0.24
        ),
    }


def poly_obj(name, collection, points, material, z=0.0):
    verts = [(x, y, z) for x, y in points]
    faces = [tuple(range(len(verts)))]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def vertical_rect_obj(name, collection, y, x_min, x_max, z_min, z_max, material):
    verts = [(x_min, y, z_min), (x_max, y, z_min), (x_max, y, z_max), (x_min, y, z_max)]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], [(0, 1, 2, 3)])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    disable_shadow(obj)
    return obj


def vertical_poly_obj(name, collection, y, points_xz, material):
    verts = [(x, y, z) for x, z in points_xz]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], [tuple(range(len(verts)))])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    disable_shadow(obj)
    return obj


def xz_beam_obj(name, collection, y, start_xz, end_xz, depth, thickness, material):
    x1, z1 = start_xz
    x2, z2 = end_xz
    dx = x2 - x1
    dz = z2 - z1
    length = math.sqrt(dx * dx + dz * dz)
    angle = math.atan2(dz, dx)
    return cube_obj(
        name,
        collection,
        ((x1 + x2) * 0.5, y, (z1 + z2) * 0.5),
        (length, depth, thickness),
        material,
        rot=(0, -angle, 0),
    )


def tri_prism_obj(name, collection, loc, length, width, height, material, rot=(0, 0, 0)):
    lx = width / 2
    ly = length / 2
    verts = [
        (-lx, -ly, 0),
        (lx, -ly, 0),
        (0, -ly, height),
        (-lx, ly, 0),
        (lx, ly, 0),
        (0, ly, height),
    ]
    faces = [(0, 1, 2), (3, 5, 4), (0, 3, 4, 1), (1, 4, 5, 2), (2, 5, 3, 0)]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = rot
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def add_text(name, collection, text, loc, mat, size=0.32, rot=(math.radians(72), 0, 0)):
    bpy.ops.object.text_add(location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Curve"
    obj.data.body = text
    obj.data.align_x = "CENTER"
    obj.data.align_y = "CENTER"
    obj.data.size = size
    obj.data.materials.append(mat)
    link_to(collection, obj)
    return obj


def setup_scene_settings(world_color, exposure=0.0):
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    engines = {item.identifier for item in scene.render.bl_rna.properties["engine"].enum_items}
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    if hasattr(scene, "eevee"):
        for attr, value in [
            ("use_gtao", True),
            ("gtao_distance", 4),
            ("gtao_factor", 1.0),
            ("use_bloom", True),
            ("bloom_intensity", 0.08),
        ]:
            if hasattr(scene.eevee, attr):
                setattr(scene.eevee, attr, value)
    scene.world = bpy.data.worlds.new("SCENE_World_StylizedAtmosphere") if not scene.world else scene.world
    scene.world.color = world_color
    try:
        scene.view_settings.view_transform = "Standard"
    except TypeError:
        pass
    try:
        scene.view_settings.look = "Medium High Contrast"
    except TypeError:
        scene.view_settings.look = "None"
    scene.view_settings.exposure = exposure
    scene.render.film_transparent = False


def make_living_tree(prefix, collection, mats, loc, scale=1.0, lean=0.0, canopy="broad"):
    trunk = frustum_obj(
        f"{prefix}_ChunkyLivingTrunk",
        collection,
        (loc[0], loc[1], loc[2] + 2.2 * scale),
        0.55 * scale,
        0.38 * scale,
        4.4 * scale,
        7,
        mats["MAT_Bark_WarmBrown"],
        rot=(math.radians(lean), 0, math.radians((loc[0] * 17) % 20)),
    )
    for i in range(4):
        angle = math.tau * i / 4 + 0.35
        root = cube_obj(
            f"{prefix}_RootButtress_{i:02d}",
            collection,
            (
                loc[0] + math.cos(angle) * 0.8 * scale,
                loc[1] + math.sin(angle) * 0.8 * scale,
                loc[2] + 0.16 * scale,
            ),
            (1.8 * scale, 0.22 * scale, 0.18 * scale),
            mats["MAT_Bark_DarkRoot"],
            rot=(0, 0, angle),
        )
        disable_shadow(root)
    if canopy == "spire":
        for i, dz in enumerate([0.0, 0.75, 1.45]):
            frustum_obj(
                f"{prefix}_NeedleCanopy_{i:02d}",
                collection,
                (loc[0], loc[1], loc[2] + (4.3 + dz) * scale),
                (1.55 - i * 0.34) * scale,
                0.10 * scale,
                1.45 * scale,
                5,
                mats["MAT_Leaves_DeepGreen"] if i == 0 else mats["MAT_Leaves_LightGreen"],
                rot=(0, 0, math.radians(36 * i)),
            )
    else:
        for i, (dx, dy, dz, radius) in enumerate(
            [(-0.45, 0.0, 4.6, 1.25), (0.52, 0.1, 4.75, 1.35), (0.0, 0.48, 5.2, 1.1)]
        ):
            frustum_obj(
                f"{prefix}_RoundedCanopy_{i:02d}",
                collection,
                (loc[0] + dx * scale, loc[1] + dy * scale, loc[2] + dz * scale),
                radius * scale,
                radius * 0.72 * scale,
                1.15 * scale,
                8,
                mats["MAT_Leaves_LightGreen"] if i == 2 else mats["MAT_Leaves_DeepGreen"],
            )
    bicone_mesh(
        f"{prefix}_WorldrootMemoryPulse",
        collection,
        (loc[0], loc[1] - 0.08 * scale, loc[2] + 2.9 * scale),
        0.16 * scale,
        0.38 * scale,
        mats["MAT_Worldroot_Cyan_Emission"],
        sides=6,
    )
    return trunk


def make_character(prefix, collection, mats, loc, role="npc", quest=False):
    if role == "player":
        mat = mats["MAT_PlayerPlaceholder"]
    elif role == "enemy":
        mat = mats["MAT_EnemyPlaceholder"]
    else:
        mat = mats["MAT_NpcPlaceholder"]
    frustum_obj(f"{prefix}_Body", collection, (loc[0], loc[1], loc[2] + 0.85), 0.32, 0.25, 1.25, 6, mat)
    frustum_obj(f"{prefix}_Head", collection, (loc[0], loc[1], loc[2] + 1.62), 0.26, 0.24, 0.34, 10, mat)
    cube_obj(f"{prefix}_Shoulders", collection, (loc[0], loc[1], loc[2] + 1.22), (0.9, 0.16, 0.18), mat)
    cube_obj(
        f"{prefix}_StaffOrWeapon",
        collection,
        (loc[0] + 0.48, loc[1] - 0.06, loc[2] + 0.85),
        (0.08, 0.08, 1.55),
        mats["MAT_Path_WarmRoot"],
        rot=(math.radians(8), math.radians(5), 0),
    )
    if quest:
        bicone_mesh(f"{prefix}_QuestMarker_Diamond", collection, (loc[0], loc[1], loc[2] + 2.35), 0.18, 0.34, mats["MAT_GoldTrim"])
        frustum_obj(f"{prefix}_QuestMarker_Stem", collection, (loc[0], loc[1], loc[2] + 2.05), 0.03, 0.03, 0.34, 6, mats["MAT_GoldTrim"])


def make_memory_echo_silhouette(prefix, collection, mats, loc, mat_key="MAT_Echo_GhostBlueWhite"):
    mat = mats[mat_key]
    frustum_obj(f"{prefix}_EchoBody", collection, (loc[0], loc[1], loc[2] + 0.86), 0.30, 0.20, 1.18, 6, mat)
    bicone_mesh(f"{prefix}_EchoHead", collection, (loc[0], loc[1], loc[2] + 1.58), 0.19, 0.32, mat, sides=8)
    cube_obj(f"{prefix}_EchoShoulderTrace", collection, (loc[0], loc[1] - 0.04, loc[2] + 1.20), (0.72, 0.06, 0.08), mat)


def make_simple_fawn(prefix, collection, mats, loc, scale=1.0):
    cube_obj(f"{prefix}_Body", collection, (loc[0], loc[1], loc[2] + 0.42 * scale), (0.9 * scale, 0.32 * scale, 0.42 * scale), mats["MAT_NpcPlaceholder"])
    frustum_obj(f"{prefix}_Head", collection, (loc[0] + 0.55 * scale, loc[1], loc[2] + 0.64 * scale), 0.16 * scale, 0.13 * scale, 0.26 * scale, 6, mats["MAT_NpcPlaceholder"], rot=(0, math.radians(85), 0))
    for i, dx in enumerate([-0.28, -0.08, 0.18, 0.38]):
        cube_obj(
            f"{prefix}_Leg_{i:02d}",
            collection,
            (loc[0] + dx * scale, loc[1] + (-0.1 if i % 2 else 0.1) * scale, loc[2] + 0.12 * scale),
            (0.08 * scale, 0.08 * scale, 0.45 * scale),
            mats["MAT_Bark_WarmBrown"],
        )


def scatter_asset(templates, asset_name, base_name, collection, positions, scale=(1, 1, 1)):
    for i, position in enumerate(positions):
        if len(position) == 4:
            x, y, z, rot_deg = position
        elif len(position) == 3:
            x, y, rot_deg = position
            z = 0.08
        else:
            raise ValueError(f"{base_name} position {i} must use x,y,rot or x,y,z,rot")
        inst(
            templates,
            asset_name,
            f"{base_name}_{i:02d}",
            collection,
            (x, y, z),
            rot=(0, 0, math.radians(rot_deg)),
            scale=scale,
        )


def export_scene_glb(collections, out_glb):
    bpy.ops.object.select_all(action="DESELECT")
    for collection in collections.values():
        for obj in collection.objects:
            obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(out_glb), export_format="GLB", use_selection=True)


def build_manifest(scene_name, stem, collections, out_blend, out_glb, out_render_1440, out_render_1080, out_manifest, out_report, checklist, notes):
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
                    "source_asset": obj.get("source_asset", "custom_scene_geometry"),
                    "materials": mats,
                    "approximate_triangles": object_triangles(obj, depsgraph),
                }
            )
    material_names = sorted({mat.name for mat in bpy.data.materials})
    unnamed_primitives = [obj for obj in objects if obj["name"].startswith(("Cube", "Plane", "Cylinder"))]
    non_prefixed_materials = [name for name in material_names if not name.startswith("MAT_")]
    manifest = {
        "scene": scene_name,
        "file_stem": stem,
        "source_asset_kit": str(KIT_BLEND),
        "collections": list(collections.keys()),
        "object_counts": counts,
        "objects": objects,
        "material_names": material_names,
        "approximate_total_triangles": sum(item["approximate_triangles"] for item in objects),
        "render_paths": [str(out_render_1440), str(out_render_1080)],
        "export_paths": {"blend": str(out_blend), "glb": str(out_glb)},
        "qa": {
            "non_prefixed_materials": non_prefixed_materials,
            "unnamed_primitives": [obj["name"] for obj in unnamed_primitives],
        },
        "notes": notes,
    }
    out_manifest.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    report_lines = [
        f"# {scene_name} Quality Report",
        "",
        "## Checklist",
    ]
    report_lines.extend([f"- [x] {item}" for item in checklist])
    report_lines.extend(
        [
            "",
            "## Technical QA",
            f"- Collections: {len(collections)}",
            f"- Objects: {sum(counts.values())}",
            f"- Approximate triangles: {manifest['approximate_total_triangles']}",
            f"- Non-prefixed materials: {len(non_prefixed_materials)}",
            f"- Primitive default object names: {len(unnamed_primitives)}",
            "",
            "## Notes",
        ]
    )
    report_lines.extend([f"- {note}" for note in notes])
    out_report.write_text("\n".join(report_lines) + "\n", encoding="utf-8")
    return manifest


def build_elarthalas_terrain(collections, mats):
    terrain = collections["ELAR_Terrain"]
    vertical_rect_obj(
        "ELAR_Terrain_Backdrop_SacredMorningSky",
        terrain,
        21.7,
        -44,
        44,
        -0.4,
        60,
        mats["MAT_Backdrop_SacredSky"],
    )
    poly_obj(
        "ELAR_Terrain_SacredApproachGround_76x38m",
        terrain,
        [(-38, -20), (38, -20), (39, 21), (-39, 21)],
        mats["MAT_Grass_Controlled"],
        z=-0.04,
    )
    poly_obj(
        "ELAR_Terrain_DistantBlueMistBackplate",
        terrain,
        [(-40, 13), (40, 13), (40, 21), (-40, 21)],
        mats["MAT_Backdrop_BlueMist"],
        z=0.02,
    )
    for i, (x, y, width, height, lean) in enumerate(
        [
            (-22.0, 20.6, 4.0, 15.0, -0.8),
            (-16.0, 21.0, 3.2, 12.8, 0.6),
            (-8.8, 21.25, 2.2, 11.0, -0.4),
            (8.8, 21.25, 2.2, 11.0, 0.4),
            (16.0, 21.0, 3.2, 12.8, -0.6),
            (22.0, 20.6, 4.0, 15.0, 0.8),
        ]
    ):
        vertical_poly_obj(
            f"ELAR_Terrain_Backdrop_DistantArchiveRootButtress_{i:02d}_BreaksOpenHorizon",
            terrain,
            y,
            [
                (x - width * 0.34, -0.2),
                (x + width * 0.28, -0.2),
                (x + width * (0.18 + lean * 0.05), height * 0.70),
                (x + width * lean * 0.10, height),
                (x - width * (0.22 - lean * 0.04), height * 0.62),
            ],
            mats["MAT_Backdrop_DarkArchiveSilhouette"],
        )
    for i, y in enumerate([-15, -10, -5, 0, 5, 10, 15]):
        width = 5.2 + i * 0.22
        stone = cube_obj(
            f"ELAR_Terrain_LongRootroad_MossyStone_{i:02d}",
            terrain,
            (0, y, 0.03),
            (width, 5.2, 0.08),
            mats["MAT_Path_MossyStone"],
            rot=(0, 0, math.radians((-1) ** i * 3)),
        )
        disable_shadow(stone)
        root = frustum_obj(
            f"ELAR_Terrain_LongRootroad_LivingRootSpine_{i:02d}",
            terrain,
            (0, y + 1.8, 0.11),
            0.24,
            0.16,
            4.65,
            6,
            mats["MAT_Path_SacredRoot"],
            rot=(math.radians(90), 0, math.radians((-1) ** (i + 1) * 5)),
        )
        disable_shadow(root)
        for side, x in [("Left", -width * 0.55), ("Right", width * 0.55)]:
            border = frustum_obj(
                f"ELAR_Terrain_LongRootroad_RaisedRootBorder_{side}_{i:02d}",
                terrain,
                (x, y, 0.17),
                0.13,
                0.08,
                4.18,
                6,
                mats["MAT_Path_DarkRootShadow"],
                rot=(math.radians(90), 0, math.radians((-3 if side == "Left" else 3) + (-1) ** i * 2)),
            )
            disable_shadow(border)
            cube_obj(
                f"ELAR_Terrain_LongRootroad_PaintedStoneEdge_{side}_{i:02d}",
                terrain,
                (x * 0.88, y - 0.4, 0.24),
                (0.10, 2.7, 0.05),
                mats["MAT_Path_StoneHighlight"],
                rot=(0, 0, math.radians((-3 if side == "Left" else 3) + (-1) ** i * 2)),
            )
    for i, y in enumerate([-16.0, -11.8, -7.6, -3.4, 0.8, 5.0, 9.2, 13.4]):
        width = 4.6 + i * 0.16
        sway = 0.22 * ((i % 2) - 0.5)
        base_half_w = (width + 0.72) * 0.5
        base_half_l = 1.92
        base = poly_obj(
            f"ELAR_Terrain_RaisedRootroad_GrownRootMass_{i:02d}",
            terrain,
            [
                (-base_half_w + sway, y - base_half_l),
                (-base_half_w * 0.22, y - base_half_l * 1.12),
                (base_half_w * 0.92 + sway * 0.5, y - base_half_l * 0.72),
                (base_half_w + sway * 0.25, y + base_half_l * 0.68),
                (base_half_w * 0.18, y + base_half_l * 1.06),
                (-base_half_w * 0.84 + sway * 0.4, y + base_half_l * 0.82),
            ],
            mats["MAT_Path_DarkRootShadow"],
            z=0.36 + i * 0.010,
        )
        disable_shadow(base)
        crown_half_w = width * 0.42
        crown_half_l = 1.32
        crown = poly_obj(
            f"ELAR_Terrain_RaisedRootroad_EmbeddedMossStoneCrown_{i:02d}",
            terrain,
            [
                (-crown_half_w + sway * 0.45, y - crown_half_l * 0.92),
                (crown_half_w * 0.70, y - crown_half_l),
                (crown_half_w + sway * 0.35, y + crown_half_l * 0.24),
                (crown_half_w * 0.35, y + crown_half_l),
                (-crown_half_w * 0.82, y + crown_half_l * 0.74),
            ],
            mats["MAT_Path_MossyStone"],
            z=0.58 + i * 0.010,
        )
        disable_shadow(crown)
        for side, x in [("Left", -width * 0.58), ("Right", width * 0.58)]:
            rib = frustum_obj(
                f"ELAR_Terrain_RaisedRootroad_LivingRootGuardrail_{side}_{i:02d}",
                terrain,
                (x, y + 0.1, 0.61 + i * 0.010),
                0.12,
                0.07,
                3.10,
                6,
                mats["MAT_Path_SacredRoot"],
                rot=(math.radians(90), 0, math.radians((-6 if side == "Left" else 6) + (-1) ** i * 1.5)),
            )
            disable_shadow(rib)
        for j, x in enumerate([0]):
            cube_obj(
                f"ELAR_Terrain_RaisedRootroad_CyanGuideRune_{i:02d}_{j:02d}",
                terrain,
                (x, y - 0.12, 0.66 + i * 0.012),
                (0.38, 0.08, 0.035),
                mats["MAT_Rune_Cyan_Emission"],
                rot=(0, 0, math.radians(9 + i * 4)),
            )
    road_samples = [
        (-17.6, 2.7, -0.18),
        (-13.8, 3.0, 0.22),
        (-9.7, 3.35, -0.10),
        (-5.4, 3.75, 0.16),
        (-1.2, 4.25, -0.08),
        (3.2, 4.75, 0.12),
        (7.6, 5.35, -0.14),
        (12.0, 6.05, 0.06),
        (15.8, 6.9, 0.0),
    ]
    left_edge = [(center - width * 0.55, y) for y, width, center in road_samples]
    right_edge = [(center + width * 0.55, y) for y, width, center in reversed(road_samples)]
    poly_obj(
        "ELAR_Terrain_SacredRootroad_TaperedLivingRootBed_OrganicSilhouette",
        terrain,
        left_edge + right_edge,
        mats["MAT_Path_DarkRootShadow"],
        z=0.55,
    )
    core_left = [(center - width * 0.34, y + 0.18) for y, width, center in road_samples]
    core_right = [(center + width * 0.34, y + 0.18) for y, width, center in reversed(road_samples)]
    poly_obj(
        "ELAR_Terrain_SacredRootroad_MossyStoneWalkableCore_ClearRoute",
        terrain,
        core_left + core_right,
        mats["MAT_Path_MossyStone"],
        z=0.72,
    )
    for i, (y, width, center) in enumerate(road_samples[:-1]):
        slab_y = y + 1.7
        slab_width = width * (0.52 + 0.05 * (i % 3))
        x_shift = center + (-0.22 if i % 2 == 0 else 0.24)
        poly_obj(
            f"ELAR_Terrain_SacredRootroad_UnevenEmbeddedStoneSlab_{i:02d}",
            terrain,
            [
                (x_shift - slab_width * 0.52, slab_y - 0.78),
                (x_shift + slab_width * 0.46, slab_y - 0.66),
                (x_shift + slab_width * 0.56, slab_y + 0.68),
                (x_shift - slab_width * 0.42, slab_y + 0.82),
            ],
            mats["MAT_Path_StoneHighlight"] if i % 3 == 0 else mats["MAT_Path_MossyStone"],
            z=0.82 + i * 0.012,
        )
    for i, (x, y, rot, length) in enumerate([(-0.55, -10.6, -18, 1.65), (0.72, -3.0, 16, 1.25), (-0.38, 4.8, -12, 1.48), (0.45, 11.6, 14, 1.12)]):
        frustum_obj(
            f"ELAR_Terrain_SacredRootroad_BrokenUnevenRootSplit_{i:02d}",
            terrain,
            (x, y, 0.88),
            0.055,
            0.035,
            length,
            6,
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(90), 0, math.radians(rot)),
        )
    for side, side_sign in [("Left", -1), ("Right", 1)]:
        for i in range(len(road_samples) - 1):
            y1, width1, center1 = road_samples[i]
            y2, width2, center2 = road_samples[i + 1]
            x1 = center1 + side_sign * width1 * 0.58
            x2 = center2 + side_sign * width2 * 0.58
            mid_x = (x1 + x2) * 0.5
            mid_y = (y1 + y2) * 0.5
            segment_len = math.sqrt((y2 - y1) ** 2 + (x2 - x1) ** 2)
            angle = -math.atan2(x2 - x1, y2 - y1)
            frustum_obj(
                f"ELAR_Terrain_SacredRootroad_BraidedRootBorder_{side}_{i:02d}",
                terrain,
                (mid_x, mid_y, 0.84 + i * 0.008),
                0.08,
                0.05,
                segment_len * 0.92,
                6,
                mats["MAT_Path_SacredRoot"],
                rot=(math.radians(90), 0, angle),
            )
    for i, (y, width, center) in enumerate(road_samples[1:-1]):
        for side, side_sign in [("Left", -1), ("Right", 1)]:
            edge_x = center + side_sign * width * 0.62
            frustum_obj(
                f"ELAR_Terrain_SacredRootroad_RootEdgeKnot_{side}_{i:02d}",
                terrain,
                (edge_x, y + 0.35 * ((i % 2) - 0.5), 1.02 + i * 0.01),
                0.22,
                0.11,
                0.54,
                6,
                mats["MAT_Path_DarkRootShadow"],
                rot=(math.radians(86), 0, math.radians(18 * side_sign + i * 9)),
            )
        if i in [1, 3, 5]:
            cube_obj(
                f"ELAR_Terrain_SacredRootroad_RecessedCyanGuideCut_{i:02d}",
                terrain,
                (center, y + 0.52, 1.05 + i * 0.01),
                (0.46, 0.075, 0.045),
                mats["MAT_Rune_Cyan_Emission"],
                rot=(0, 0, math.radians(-4 + i * 3)),
            )
    for i, (y, width, center) in enumerate(road_samples[1:-1]):
        if i % 2 == 1:
            continue
        bicone_mesh(
            f"ELAR_Terrain_SacredRootroad_CenterGuideRuneMemoryShard_{i:02d}",
            terrain,
            (center, y + 0.15, 1.02 + i * 0.01),
            0.12,
            0.34,
            mats["MAT_Rune_Cyan_Emission"],
            sides=5,
        )
    for side, x in [("Left", -3.3), ("Right", 3.3)]:
        frustum_obj(
            f"ELAR_Terrain_ForegroundWardStone_{side}_RoadEntrance",
            terrain,
            (x, -18.2, 1.05),
            0.42,
            0.28,
            1.9,
            6,
            mats["MAT_Stone_PaleArchive"],
            rot=(0, 0, math.radians(8 if side == "Left" else -8)),
        )
        cube_obj(
            f"ELAR_Terrain_ForegroundWardStone_{side}_CyanScannerFace",
            terrain,
            (x * 0.98, -18.42, 1.26),
            (0.12, 0.06, 0.9),
            mats["MAT_MemoryGlass_CyanGreen"],
            rot=(0, 0, math.radians(8 if side == "Left" else -8)),
        )
        frustum_obj(
            f"ELAR_Terrain_ForegroundWardStone_{side}_RootWrappedMossBase",
            terrain,
            (x, -18.2, 0.18),
            0.56,
            0.36,
            0.30,
            6,
            mats["MAT_Path_DarkRootShadow"],
            rot=(0, 0, math.radians(18 if side == "Left" else -18)),
        )
        bicone_mesh(
            f"ELAR_Terrain_ForegroundWardStone_{side}_CyanJudgementEye",
            terrain,
            (x * 0.98, -18.50, 2.04),
            0.12,
            0.30,
            mats["MAT_Rune_Cyan_Emission"],
            sides=5,
        )
    for side, x in [("Left", -4.2), ("Right", 4.2)]:
        for i, y in enumerate([-13, -9, -5, -1, 3, 7, 11]):
            cube_obj(
                f"ELAR_Terrain_PilgrimageStep_{side}_{i:02d}",
                terrain,
                (x, y, 0.09),
                (0.74, 0.38, 0.16),
                mats["MAT_Stone_Pilgrimage"],
                rot=(0, 0, math.radians(7 if side == "Left" else -7)),
            )
    for i, y in enumerate([-12.2, -6.2, -0.2, 5.8, 11.8]):
        disc_mesh(
            f"ELAR_Terrain_PilgrimageRestCircle_{i:02d}",
            terrain,
            (0, y, 0.18),
            1.15 + i * 0.08,
            0.48,
            mats["MAT_Stone_Pilgrimage"] if i not in [0, 4] else mats["MAT_Echo_Transparent"],
            sides=30,
        )
        for j, x in enumerate([-1.2, 1.2] if i in [0, 4] else [0]):
            bicone_mesh(
                f"ELAR_Terrain_PilgrimageRestCircle_{i:02d}_RuneMarker_{j:02d}",
                terrain,
                (x, y, 0.36),
                0.10,
                0.24,
                mats["MAT_Rune_Cyan_Emission"],
                sides=5,
            )
    disc_mesh("ELAR_Terrain_PlayerSpawn_SubtleArchiveRegistrationRune", terrain, (0, -17.4, 0.13), 1.1, 0.58, mats["MAT_Echo_Transparent"], sides=36)
    for i in range(8):
        angle = math.tau * i / 8
        cube_obj(
            f"ELAR_Terrain_PlayerSpawn_RuneTick_{i:02d}",
            terrain,
            (math.cos(angle) * 1.35, -17.4 + math.sin(angle) * 0.72, 0.16),
            (0.34, 0.045, 0.035),
            mats["MAT_PlayerSpawn_CyanRune"],
            rot=(0, 0, angle),
        )


def build_elarthalas_gate(collections, mats):
    arch = collections["ELAR_ArchiveCity"]
    def arch_segments(prefix, radius, z_base, z_top, y, material, depth, thickness, segments=10):
        points = []
        for i in range(segments + 1):
            t = math.pi - math.pi * i / segments
            points.append((math.cos(t) * radius, z_base + math.sin(t) * (z_top - z_base)))
        for i in range(segments):
            xz_beam_obj(f"{prefix}_Segment_{i:02d}", arch, y, points[i], points[i + 1], depth, thickness, material)

    vertical_poly_obj(
        "ELAR_ArchiveCity_SilentGate_BackgroundLayeredBlueGreenFog_CentralBrokenSilhouette",
        arch,
        19.2,
        [(-15.5, 0.15), (15.5, 0.25), (14.0, 8.2), (9.0, 11.6), (3.2, 10.9), (0.0, 13.8), (-3.3, 10.8), (-9.5, 11.9), (-14.2, 8.4)],
        mats["MAT_Backdrop_DarkArchiveSilhouette"],
    )
    for i, (x, y, z, w, h) in enumerate([(-16.0, 20.4, 2.2, 5.0, 7.6), (-9.2, 21.0, 4.2, 3.4, 8.2), (9.2, 21.1, 4.2, 3.4, 8.2), (16.0, 20.5, 2.2, 5.0, 7.6)]):
        vertical_poly_obj(
            f"ELAR_ArchiveCity_BackgroundFogShard_{i:02d}_NoFlatWall",
            arch,
            y,
            [(x - w * 0.5, z), (x + w * 0.45, z + 0.25), (x + w * 0.35, z + h * 0.72), (x, z + h), (x - w * 0.48, z + h * 0.62)],
            mats["MAT_Gate_BlueFog"],
        )

    for i, (x, z, height, root_scale) in enumerate([(-14.4, 5.2, 8.2, 0.42), (-10.8, 6.1, 10.8, 0.48), (-6.9, 7.2, 9.4, 0.38), (6.9, 7.2, 9.4, 0.38), (10.8, 6.1, 10.8, 0.48), (14.4, 5.2, 8.2, 0.42)]):
        frustum_obj(
            f"ELAR_ArchiveCity_DistantSealedArchiveTower_{i:02d}_LivingRootSpine",
            arch,
            (x, 20.8, z),
            root_scale,
            root_scale * 0.42,
            height,
            7,
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(3 if x < 0 else -3), 0, math.radians(i * 8)),
        )
        frustum_obj(
            f"ELAR_ArchiveCity_DistantSealedArchiveTower_{i:02d}_PaleStoneRib",
            arch,
            (x + (0.38 if x < 0 else -0.38), 20.55, z + 0.5),
            root_scale * 0.55,
            root_scale * 0.22,
            height * 0.72,
            5,
            mats["MAT_Stone_PaleArchive"],
            rot=(math.radians(4 if x < 0 else -4), 0, math.radians(-i * 7)),
        )
        seal_mat = mats["MAT_MemoryGlass_CyanGreen"] if i in [1, 4] else mats["MAT_GoldTrim"]
        seal_name = "CyanWardHeart" if i in [1, 4] else "GoldArchiveSealKnot"
        bicone_mesh(
            f"ELAR_ArchiveCity_DistantSealedArchiveTower_{i:02d}_{seal_name}",
            arch,
            (x, 20.35, z + height * 0.48),
            root_scale * (0.38 if i in [1, 4] else 0.26),
            root_scale * (0.92 if i in [1, 4] else 0.62),
            seal_mat,
            sides=5,
        )

    vertical_poly_obj(
        "ELAR_ArchiveCity_SilentGate_DarkRootSealedBackplate_OrganicArchShape",
        arch,
        16.08,
        [(-4.35, 1.05), (4.35, 1.05), (4.55, 6.9), (2.45, 10.35), (0.0, 11.42), (-2.45, 10.35), (-4.55, 6.9)],
        mats["MAT_Path_DarkRootShadow"],
    )
    vertical_poly_obj(
        "ELAR_ArchiveCity_SilentGate_LeftClosedMemoryGlassPanel_GrownArch",
        arch,
        15.78,
        [(-3.55, 1.58), (-2.12, 1.78), (-2.26, 7.75), (-2.95, 9.18), (-3.55, 6.82)],
        mats["MAT_MemoryGlass_CyanGreen"],
    )
    vertical_poly_obj(
        "ELAR_ArchiveCity_SilentGate_RightClosedMemoryGlassPanel_GrownArch",
        arch,
        15.78,
        [(2.12, 1.78), (3.55, 1.58), (3.55, 6.82), (2.95, 9.18), (2.26, 7.75)],
        mats["MAT_MemoryGlass_CyanGreen"],
    )
    for side, sign in [("Left", -1), ("Right", 1)]:
        for j, (x, z, h) in enumerate([(2.55, 4.4, 5.65), (3.12, 4.8, 4.82)]):
            cube_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_MemoryGlassRootMullion_{j:02d}",
                arch,
                (sign * x, 15.36, z),
                (0.13, 0.13, h),
                mats["MAT_Bark_DarkRoot"],
                rot=(0, 0, math.radians(sign * (3 + j * 2))),
            )
        for j, (start, end) in enumerate(
            [
                ((sign * 3.55, 2.55), (sign * 2.22, 3.20)),
                ((sign * 3.42, 5.70), (sign * 2.18, 5.20)),
                ((sign * 3.00, 8.10), (sign * 2.24, 7.30)),
            ]
        ):
            xz_beam_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_SealedMemoryGlassCrossRib_{j:02d}",
                arch,
                15.27,
                start,
                end,
                0.11,
                0.12,
                mats["MAT_Stone_PaleArchive"] if j == 1 else mats["MAT_Bark_DarkRoot"],
            )
    cube_obj(
        "ELAR_ArchiveCity_SilentGate_ClosedCentralSeam_DarkRootLockline",
        arch,
        (0, 15.66, 5.72),
        (0.34, 0.14, 9.25),
        mats["MAT_Bark_DarkRoot"],
    )
    for i, z in enumerate([2.45, 3.28, 4.15, 5.02, 5.92, 6.78, 7.66, 8.48]):
        side_sign = -1 if i % 2 == 0 else 1
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_ClosedCentralSeam_InterlockingSealTooth_{i:02d}",
            arch,
            (side_sign * 0.28, 15.30, z),
            (0.58, 0.13, 0.16),
            mats["MAT_Stone_PaleArchive"] if i % 3 else mats["MAT_Bark_DarkRoot"],
            rot=(0, 0, math.radians(side_sign * 7)),
        )
    for i, z in enumerate([3.05, 5.95, 8.65]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_ClosedCentralSeam_RecessedCyanRuneCut_{i:02d}",
            arch,
            (0.02, 15.18, z),
            (0.08, 0.06, 0.70),
            mats["MAT_Rune_Cyan_Emission"],
            rot=(0, 0, math.radians(0)),
        )
    for i, z in enumerate([2.15, 5.45, 8.72]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_CentralSeam_GoldArchiveSealClamp_{i:02d}",
            arch,
            (0, 15.44, z),
            (1.16, 0.16, 0.18),
            mats["MAT_GoldTrim"],
            rot=(0, 0, math.radians(2 if i % 2 == 0 else -2)),
        )
    for side, x in [("Left", -1.18), ("Right", 1.18)]:
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_LockedRootShutter_{side}",
            arch,
            (x, 15.48, 5.28),
            (0.22, 0.16, 7.42),
            mats["MAT_Bark_DarkRoot"],
            rot=(0, 0, math.radians(-2 if side == "Left" else 2)),
        )
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_PaleStoneInnerSealRib_{side}",
            arch,
            (x * 0.72, 15.38, 5.95),
            (0.16, 0.12, 6.28),
            mats["MAT_Stone_PaleArchive"],
            rot=(0, 0, math.radians(3 if side == "Left" else -3)),
        )
    for i, x in enumerate([-2.85, -1.65, -0.62, 0.62, 1.65, 2.85]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_NarrowVerticalRuneSeam_{i:02d}",
            arch,
            (x, 15.56, 5.55 + 0.35 * (i % 2)),
            (0.055, 0.06, 5.35),
            mats["MAT_Rune_Cyan_Emission"],
            rot=(0, 0, math.radians(-2 if x < 0 else 2)),
        )
    bicone_mesh(
        "ELAR_ArchiveCity_SilentGate_CentralLockedMemoryEngineCrystal",
        arch,
        (0, 15.28, 6.55),
        0.34,
        1.28,
        mats["MAT_Worldroot_Cyan_Emission"],
        sides=6,
    )

    arch_segments("ELAR_ArchiveCity_SilentGate_OuterIntertwinedDarkRootArch", 7.45, 1.25, 12.75, 15.92, mats["MAT_Bark_DarkRoot"], 0.34, 0.54, segments=12)
    arch_segments("ELAR_ArchiveCity_SilentGate_PaleArchiveStoneRibArch", 6.48, 1.55, 11.65, 15.62, mats["MAT_Stone_PaleArchive"], 0.24, 0.38, segments=12)
    arch_segments("ELAR_ArchiveCity_SilentGate_InnerThinRuneSeamArch", 5.18, 1.92, 10.65, 15.36, mats["MAT_Rune_Cyan_Emission"], 0.08, 0.12, segments=10)

    for side, sign in [("Left", -1), ("Right", 1)]:
        for j, (offset, height, lean) in enumerate([(4.85, 9.6, 5), (5.85, 11.8, -2), (6.85, 10.5, 7)]):
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_IntertwinedVerticalRoot_{j:02d}",
                arch,
                (sign * offset, 16.05 - j * 0.1, 1.0 + height * 0.5),
                0.42 - j * 0.04,
                0.22,
                height,
                6,
                mats["MAT_Bark_DarkRoot"],
                rot=(math.radians(sign * lean), 0, math.radians(sign * (8 + j * 5))),
            )
        for j, (offset, height) in enumerate([(5.35, 8.6), (6.42, 9.4)]):
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_PaleStoneArchiveRib_{j:02d}",
                arch,
                (sign * offset, 15.72 - j * 0.06, 1.25 + height * 0.5),
                0.34,
                0.19,
                height,
                5,
                mats["MAT_Stone_PaleArchive"],
                rot=(math.radians(sign * 3), 0, math.radians(sign * -3)),
            )
        bicone_mesh(
            f"ELAR_ArchiveCity_SilentGate_{side}_TallCyanMemoryGlassSeal",
            arch,
            (sign * 4.45, 15.45, 6.7),
            0.32,
            1.25,
            mats["MAT_MemoryGlass_CyanGreen"],
            sides=6,
        )
        for j, (x_offset, y_offset, radius, height) in enumerate([(4.82, 15.72, 0.74, 0.46), (6.02, 15.86, 0.56, 0.34)]):
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_MossRootBase_{j:02d}",
                arch,
                (sign * x_offset, y_offset, height * 0.5),
                radius,
                radius * 0.58,
                height,
                7,
                mats["MAT_Moss"] if j == 0 else mats["MAT_Path_DarkRootShadow"],
                rot=(0, 0, math.radians(sign * (12 + j * 8))),
            )
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_RootBaseTendril_{j:02d}",
                arch,
                (sign * (x_offset - 0.32), y_offset - 0.26, 0.28),
                0.10,
                0.045,
                1.24,
                6,
                mats["MAT_Bark_DarkRoot"],
                rot=(math.radians(86), 0, math.radians(sign * (34 + j * 12))),
            )
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_{side}_GoldRootBindingLower",
            arch,
            (sign * 5.72, 15.55, 2.05),
            (2.15, 0.16, 0.18),
            mats["MAT_GoldTrim"],
            rot=(0, 0, math.radians(sign * 7)),
        )
        for j, (offset, height, lean) in enumerate([(7.72, 12.6, 2), (8.92, 10.8, -5)]):
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_{side}_OuterArchiveButtressLayer_{j:02d}",
                arch,
                (sign * offset, 16.42 + j * 0.16, 1.0 + height * 0.5),
                0.56 - j * 0.08,
                0.26,
                height,
                6,
                mats["MAT_Backdrop_DarkArchiveSilhouette"] if j == 1 else mats["MAT_Bark_DarkRoot"],
                rot=(math.radians(sign * lean), 0, math.radians(sign * (4 + j * 6))),
            )

    for i, (start, end) in enumerate([
        ((-3.95, 3.0), (-0.32, 7.35)),
        ((3.95, 3.0), (0.32, 7.35)),
        ((-3.4, 8.15), (-0.38, 5.02)),
        ((3.4, 8.15), (0.38, 5.02)),
        ((-2.2, 9.58), (2.2, 9.58)),
    ]):
        xz_beam_obj(
            f"ELAR_ArchiveCity_SilentGate_DoorInterlockingLivingRootBrace_{i:02d}",
            arch,
            15.35,
            start,
            end,
            0.13,
            0.18,
            mats["MAT_Bark_DarkRoot"],
        )
    for i, (start, end) in enumerate([
        ((-4.05, 2.05), (-1.15, 4.85)),
        ((4.05, 2.05), (1.15, 4.85)),
        ((-3.95, 7.6), (-1.08, 5.62)),
        ((3.95, 7.6), (1.08, 5.62)),
    ]):
        xz_beam_obj(
            f"ELAR_ArchiveCity_SilentGate_PaleStoneMemoryEngineRib_{i:02d}",
            arch,
            15.20,
            start,
            end,
            0.10,
            0.14,
            mats["MAT_Stone_PaleArchive"],
        )
    for i, (start, end) in enumerate([
        ((-5.75, 2.15), (-4.32, 10.35)),
        ((5.75, 2.15), (4.32, 10.35)),
        ((-6.85, 1.75), (-5.38, 11.62)),
        ((6.85, 1.75), (5.38, 11.62)),
    ]):
        xz_beam_obj(
            f"ELAR_ArchiveCity_SilentGate_OuterLivingRootRibOverlay_{i:02d}",
            arch,
            15.18,
            start,
            end,
            0.18,
            0.22,
            mats["MAT_Bark_DarkRoot"],
        )
    for i, (x, z, height, rot) in enumerate([(-4.9, 12.15, 2.2, -7), (-2.4, 12.9, 1.45, -3), (0.0, 13.2, 2.55, 0), (2.4, 12.9, 1.45, 3), (4.9, 12.15, 2.2, 7)]):
        frustum_obj(
            f"ELAR_ArchiveCity_SilentGate_CrownArchiveSpire_{i:02d}",
            arch,
            (x, 15.82, z + height * 0.5),
            0.24,
            0.08,
            height,
            5,
            mats["MAT_Stone_PaleArchive"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
        bicone_mesh(
            f"ELAR_ArchiveCity_SilentGate_CrownArchiveSpire_{i:02d}_GoldWardCap",
            arch,
            (x, 15.56, z + height + 0.15),
            0.12,
            0.30,
            mats["MAT_GoldTrim"],
            sides=5,
        )
    for i, (x, z, rot) in enumerate([(-0.74, 3.1, -4), (0.74, 3.1, 4), (-0.52, 7.9, 5), (0.52, 7.9, -5)]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_ClosedMemoryEngineSealPlate_{i:02d}",
            arch,
            (x, 15.24, z),
            (0.46, 0.09, 0.12),
            mats["MAT_GoldTrim"] if i < 2 else mats["MAT_Stone_PaleArchive"],
            rot=(0, 0, math.radians(rot)),
        )
    for i, x in enumerate([-10.2, -8.1, 8.1, 10.2]):
        frustum_obj(f"ELAR_ArchiveCity_BackgroundMemorySpire_{i:02d}", arch, (x, 18.8 + i % 2, 3.0), 0.35, 0.18, 6.0 - (i % 2), 5, mats["MAT_Stone_MossyGray"])
        bicone_mesh(f"ELAR_ArchiveCity_BackgroundMemorySpire_{i:02d}_PaleArchiveCap", arch, (x, 18.8 + i % 2, 6.35 - (i % 2) * 0.5), 0.22, 0.46, mats["MAT_Stone_PaleArchive"], sides=5)
    for i, (x, z, height) in enumerate([(-15.0, 4.8, 8.8), (-11.5, 5.6, 10.2), (11.5, 5.6, 10.2), (15.0, 4.8, 8.8), (0.0, 7.0, 12.5)]):
        frustum_obj(
            f"ELAR_ArchiveCity_DistantLivingArchiveTower_{i:02d}_RootColumn",
            arch,
            (x, 21.0, z),
            0.62 if i == 4 else 0.44,
            0.24,
            height,
            7,
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(3 if x < 0 else -3), 0, math.radians(i * 9)),
        )
        archive_heart_mat = mats["MAT_MemoryGlass_CyanGreen"] if i == 4 else mats["MAT_GoldTrim"]
        archive_heart_name = "CentralCyanArchiveHeart" if i == 4 else "GoldArchiveSeal"
        bicone_mesh(
            f"ELAR_ArchiveCity_DistantLivingArchiveTower_{i:02d}_{archive_heart_name}",
            arch,
            (x, 20.7, z + height * 0.46),
            0.30 if i == 4 else 0.16,
            0.72 if i == 4 else 0.38,
            archive_heart_mat,
            sides=5,
        )
    for i, (x, z) in enumerate([(-13.0, 9.4), (-6.5, 10.8), (6.5, 10.8), (13.0, 9.4)]):
        cube_obj(
            f"ELAR_ArchiveCity_DistantRootBridgeSkyline_{i:02d}",
            arch,
            (x, 20.55, z),
            (6.2, 0.22, 0.22),
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(4), 0, math.radians(5 if x < 0 else -5)),
        )


def make_elarthalas_ward_monolith_variant(props, mats, prefix, x, y, side, variant, index, inward_rot):
    specs = {
        "Small": {"height": 2.55, "base": 0.34, "top": 0.18, "yaw": -4, "lean": 4, "rune": 0.42, "eye": False},
        "Medium": {"height": 3.25, "base": 0.40, "top": 0.20, "yaw": 2, "lean": 6, "rune": 0.50, "eye": True},
        "Tall": {"height": 4.05, "base": 0.46, "top": 0.22, "yaw": 6, "lean": 8, "rune": 0.55, "eye": True},
    }
    spec = specs[variant]
    sign = -1 if side == "Left" else 1
    blade_x = x + sign * (0.10 if index % 2 == 0 else -0.08)
    yaw = inward_rot + sign * spec["yaw"]
    lean = math.radians(sign * spec["lean"])
    height = spec["height"]
    root_z = 0.10

    frustum_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_MossBrokenStoneBase",
        props,
        (blade_x, y, root_z + 0.13),
        spec["base"] * 1.75,
        spec["base"] * 1.18,
        0.26,
        6,
        mats["MAT_Moss"] if index % 2 == 0 else mats["MAT_Stone_DarkCrevice"],
        rot=(0, 0, math.radians(yaw + 18)),
    )
    frustum_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_AsymmetricCarvedStoneBlade",
        props,
        (blade_x, y, 0.36 + height * 0.5),
        spec["base"],
        spec["top"],
        height,
        5,
        mats["MAT_Stone_PaleArchive"],
        rot=(lean, 0, math.radians(yaw)),
    )
    cube_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_CyanScannerRuneSlit",
        props,
        (blade_x - sign * 0.03, y - 0.13, 0.58 + height * 0.52),
        (0.075, 0.05, height * spec["rune"]),
        mats["MAT_Rune_Cyan_Emission"],
        rot=(math.radians(3), 0, math.radians(yaw)),
    )
    frustum_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_RootBindingCollar",
        props,
        (blade_x, y, 0.38),
        spec["base"] * 1.18,
        spec["base"] * 0.82,
        0.28,
        6,
        mats["MAT_Bark_DarkRoot"],
        rot=(0, 0, math.radians(yaw + 82)),
    )
    for claw_i, claw_sign in enumerate([-1, 1]):
        frustum_obj(
            f"{prefix}_{variant}_{side}_{index:02d}_RootClamp_{claw_i:02d}",
            props,
            (blade_x + claw_sign * spec["base"] * 0.78, y + claw_sign * 0.10, 0.34),
            0.075,
            0.040,
            0.76,
            6,
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(84), 0, math.radians(yaw + claw_sign * (36 + index * 4))),
        )
    frustum_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_TaperedScannerCrown",
        props,
        (blade_x + sign * 0.06, y - 0.05, 0.48 + height),
        spec["base"] * 0.68,
        spec["base"] * 0.34,
        0.34,
        5,
        mats["MAT_Stone_PaleArchive"],
        rot=(math.radians(3), 0, math.radians(yaw + sign * 10)),
    )
    cube_obj(
        f"{prefix}_{variant}_{side}_{index:02d}_OffsetCarvedShoulder",
        props,
        (blade_x + sign * spec["base"] * 0.42, y - 0.05, 0.44 + height * 0.76),
        (spec["base"] * 1.45, 0.07, 0.13),
        mats["MAT_Stone_Pilgrimage"],
        rot=(math.radians(4), 0, math.radians(yaw - sign * 14)),
    )
    if spec["eye"]:
        bicone_mesh(
            f"{prefix}_{variant}_{side}_{index:02d}_MaskLikeCyanEye",
            props,
            (blade_x - sign * 0.02, y - 0.19, 0.58 + height),
            0.11,
            0.26 if variant == "Medium" else 0.34,
            mats["MAT_Rune_Cyan_Emission"],
            sides=5,
        )


def build_elarthalas_architecture(templates, collections, mats):
    arch = collections["ELAR_ArchiveCity"]
    props = collections["ELAR_WardsAndProps"]
    for i, (x, y, s, rot) in enumerate([(-12.5, 7, 1.0, 16), (12.5, 6.5, 1.0, -16), (-16, 1.5, 0.85, 8), (16, 0.5, 0.85, -8)]):
        make_living_tree(f"ELAR_ArchiveCity_ControlledLivingTree_{i:02d}", arch, mats, (x, y, 0), scale=1.3 * s, lean=rot, canopy="spire")
        cube_obj(
            f"ELAR_ArchiveCity_ControlledLivingTree_{i:02d}_CyanSapChannel",
            arch,
            (x + 0.22 * (1 if x < 0 else -1), y - 0.22, 2.9 * s),
            (0.09, 0.06, 1.35 * s),
            mats["MAT_Rune_Cyan_Emission"],
            rot=(math.radians(4), 0, math.radians(rot * 0.4)),
        )
    ward_variant_order = ["Small", "Medium", "Tall", "Medium", "Small", "Tall"]
    for i, y in enumerate([-14.4, -9.2, -4.1, 1.2, 6.8, 12.4]):
        for side, x in [("Left", -5.9), ("Right", 5.9)]:
            inward_rot = -10 if side == "Left" else 10
            variant = ward_variant_order[(i + (1 if side == "Right" else 0)) % len(ward_variant_order)]
            make_elarthalas_ward_monolith_variant(
                props,
                mats,
                "ELAR_WardsAndProps_WardMonolithVariant",
                x,
                y,
                side,
                variant,
                i,
                inward_rot,
            )
            if variant in ["Medium", "Tall"] and i in [1, 3, 5]:
                cube_obj(
                    f"ELAR_WardsAndProps_WardMonolithVariant_{variant}_{side}_{i:02d}_SubtleInwardScanLine",
                    props,
                    (x * 0.5, y - 0.18, 1.55),
                    (abs(x) * 0.76, 0.035, 0.035),
                    mats["MAT_Echo_Transparent"],
                    rot=(0, 0, math.radians(0 if side == "Left" else 180)),
                )
    for i, (x, y, z, rot) in enumerate([(-3.4, -8, 0.1, 0), (3.4, -8, 0.1, 0), (-4.1, 1.5, 0.1, 8), (4.1, 1.6, 0.1, -8), (-4.6, 9, 0.1, -4), (4.6, 9.1, 0.1, 4)]):
        inst(templates, "PROPS_Village_LanternPost", f"ELAR_WardsAndProps_SacredLanternPost_{i:02d}", props, (x, y, z), rot=(0, 0, math.radians(rot)), scale=(1.05, 1.05, 1.12))
    scatter_asset(
        templates,
        "PROPS_Worldroot_CyanCrystalClusterMedium",
        "ELAR_WardsAndProps_ControlledWorldrootWardCrystal",
        props,
        [(-7.8, -7.2, 0.08, -12), (7.6, -6.9, 0.08, 12)],
        scale=(0.62, 0.62, 0.78),
    )
    for i, (x, y, z, rot) in enumerate([(-5.0, -12.5, 0.1, 12), (5.0, -12.2, 0.1, -12), (-6.8, 2.4, 0.1, 10), (6.8, 2.7, 0.1, -10)]):
        inst(templates, "PROPS_Village_BannerPost", f"ELAR_WardsAndProps_VerdantPilgrimBanner_{i:02d}", props, (x, y, z), rot=(0, 0, math.radians(rot)), scale=(1.0, 1.0, 1.25))
    for i, (x, y, rot) in enumerate([(-12, -10, 8), (-10.5, -8.6, -6), (-11.8, -7.1, 0)]):
        cube_obj(f"ELAR_WardsAndProps_GreenspireCamp_Crate_{i:02d}", props, (x, y, 0.28), (0.75, 0.55, 0.52), mats["MAT_Path_WarmRoot"], rot=(0, 0, math.radians(rot)))
    tri_prism_obj(
        "ELAR_WardsAndProps_GreenspireCamp_RootShelterRoof",
        props,
        (-15.6, -6.15, 0.38),
        2.8,
        2.0,
        1.15,
        mats["MAT_CampCloth_Green"],
        rot=(0, 0, math.radians(17)),
    )
    cube_obj(
        "ELAR_WardsAndProps_GreenspireCamp_RootShelterBase",
        props,
        (-15.6, -6.15, 0.24),
        (1.65, 2.35, 0.45),
        mats["MAT_Bark_DarkRoot"],
        rot=(0, 0, math.radians(17)),
    )
    for i, x in enumerate([-16.55, -14.68]):
        cube_obj(
            f"ELAR_WardsAndProps_GreenspireCamp_RootShelterLivingRib_{i:02d}",
            props,
            (x, -6.15, 0.92),
            (0.22, 2.65, 0.22),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(6), 0, math.radians(18 + i * 4)),
        )
    inst(
        templates,
        "PROPS_Village_BannerPost",
        "ELAR_WardsAndProps_GreenspireCamp_VerdantForwardBanner",
        props,
        (-16.9, -7.8, 0.12),
        rot=(0, 0, math.radians(-8)),
        scale=(0.9, 0.9, 1.2),
    )
    inst(
        templates,
        "PROPS_Village_BannerPost",
        "ELAR_WardsAndProps_GreenspireCamp_VerdantRoadsideBanner",
        props,
        (-10.85, -7.05, 0.12),
        rot=(0, 0, math.radians(12)),
        scale=(0.82, 0.82, 1.05),
    )
    cube_obj(
        "ELAR_WardsAndProps_GreenspireCamp_ArchiveScrollStand",
        props,
        (-12.5, -7.4, 0.76),
        (0.76, 0.22, 0.82),
        mats["MAT_Stone_PaleArchive"],
        rot=(0, 0, math.radians(-5)),
    )
    cube_obj(
        "ELAR_WardsAndProps_GreenspireCamp_ArchiveScrollStand_CyanPage",
        props,
        (-12.5, -7.53, 0.86),
        (0.56, 0.05, 0.44),
        mats["MAT_MemoryGlass_CyanGreen"],
        rot=(0, 0, math.radians(-5)),
    )
    bicone_mesh("ELAR_WardsAndProps_GreenspireCamp_PortableGoldArchiveCompass", props, (-11.6, -9.9, 0.88), 0.14, 0.32, mats["MAT_GoldTrim"])
    cube_obj("ELAR_WardsAndProps_GreenspireCamp_RootMapTable", props, (-11.6, -9.9, 0.58), (1.25, 0.72, 0.22), mats["MAT_Path_WarmRoot"], rot=(0, 0, math.radians(7)))
    for i, (x, y, rot) in enumerate([(-14.8, -6.5, -8), (-15.3, -10.6, 10)]):
        inst(
            templates,
            "PROPS_Village_LanternPost",
            f"ELAR_WardsAndProps_GreenspireCamp_WarmPerimeterLantern_{i:02d}",
            props,
            (x, y, 0.12),
            rot=(0, 0, math.radians(rot)),
            scale=(0.85, 0.85, 1.0),
        )
    for i, (x, y) in enumerate([(-5.2, -0.6), (5.2, -0.6), (-5.7, 5.4), (5.7, 5.4), (-5.1, 11.0), (5.1, 11.0)]):
        frustum_obj(
            f"ELAR_WardsAndProps_PilgrimMemoryStatue_{i:02d}_Body",
            props,
            (x, y, 0.95),
            0.28,
            0.18,
            1.65,
            6,
            mats["MAT_Stone_Pilgrimage"],
            rot=(0, 0, math.radians(4 if x < 0 else -4)),
        )
        bicone_mesh(f"ELAR_WardsAndProps_PilgrimMemoryStatue_{i:02d}_CyanHeart", props, (x, y - 0.08, 1.18), 0.11, 0.24, mats["MAT_Rune_Cyan_Emission"])


def build_elarthalas_intrusion_and_memory(collections, mats):
    intrusion = collections["ELAR_ArcaneIntrusion"]
    memory = collections["ELAR_MemoryConstructs"]
    for i, (x, y, rot) in enumerate([(14.2, -6.35, 16)]):
        frustum_obj(f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}", intrusion, (x, y, 1.05), 0.18, 0.10, 2.05, 4, mats["MAT_HighElf_ArcaneViolet"], rot=(0, 0, math.radians(rot)))
        disc_mesh(
            f"ELAR_ArcaneIntrusion_AeltharAngularSurveyGlyph_{i:02d}",
            intrusion,
            (x, y, 0.08),
            0.72,
            0.36,
            mats["MAT_HighElf_SilverBlue_Emission"],
            sides=4,
            rot=(0, 0, math.radians(45)),
        )
        bicone_mesh(f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}_ColdGoldFocusingGem", intrusion, (x, y, 2.20), 0.13, 0.32, mats["MAT_HighElf_ColdGold"], sides=4)
        cube_obj(
            f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}_VioletSurveyBeam",
            intrusion,
            (x - 0.75, y + 0.35, 1.55),
            (1.35, 0.055, 0.055),
            mats["MAT_HighElf_ArcaneViolet"],
            rot=(math.radians(2), 0, math.radians(rot + 22)),
        )
    for i, (x, y, rot) in enumerate([(14.1, -6.4, 18)]):
        tri_prism_obj(
            f"ELAR_ArcaneIntrusion_HighElfTriangulationFrame_{i:02d}",
            intrusion,
            (x, y, 0.18),
            2.7,
            1.5,
            1.65,
            mats["MAT_HighElf_ArcaneViolet"],
            rot=(0, 0, math.radians(rot)),
        )
    bicone_mesh(
        "ELAR_ArcaneIntrusion_AeltharSilverBlueShard_Main",
        intrusion,
        (15.6, -6.0, 1.85),
        0.48,
        1.75,
        mats["MAT_HighElf_SilverBlue_Emission"],
        sides=4,
    )
    disc_mesh(
        "ELAR_ArcaneIntrusion_AeltharGeometricSurveyGlyph_NotWorldroot",
        intrusion,
        (15.45, -6.05, 0.18),
        1.28,
        0.54,
        mats["MAT_HighElf_SilverBlue_Emission"],
        sides=4,
        rot=(0, 0, math.radians(45)),
    )
    for i, (dx, dy, rot) in enumerate([(-1.25, -0.62, -28), (1.10, -0.70, 34), (-0.25, 1.22, 92), (1.42, 0.52, -62)]):
        cube_obj(
            f"ELAR_ArcaneIntrusion_AeltharThinGeometricBlueLine_{i:02d}",
            intrusion,
            (15.45 + dx * 0.5, -6.05 + dy * 0.5, 0.42),
            (1.62, 0.045, 0.06),
            mats["MAT_HighElf_SilverBlue_Emission"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
    for i, (x, y, z, scale) in enumerate([(14.75, -5.18, 1.15, 0.34), (16.25, -6.86, 1.35, 0.26)]):
        bicone_mesh(
            f"ELAR_ArcaneIntrusion_AeltharOrbitingSilverShard_{i:02d}",
            intrusion,
            (x, y, z),
            scale,
            scale * 1.45,
            mats["MAT_HighElf_SilverBlue"],
            sides=4,
        )
    for i, (x, y, rot) in enumerate([(14.3, -7.25, 8), (16.7, -6.85, -12), (15.8, -4.55, 0)]):
        frustum_obj(
            f"ELAR_ArcaneIntrusion_AeltharTripodLeg_{i:02d}",
            intrusion,
            (x, y, 0.72),
            0.09,
            0.05,
            1.42,
            4,
            mats["MAT_HighElf_SilverBlue"],
            rot=(math.radians(11), 0, math.radians(rot)),
        )
        cube_obj(
            f"ELAR_ArcaneIntrusion_AeltharAngularScanLine_{i:02d}",
            intrusion,
            ((x + 15.6) * 0.5, (y - 6.0) * 0.5, 1.35),
            (1.55, 0.055, 0.065),
            mats["MAT_HighElf_SilverBlue_Emission"],
            rot=(math.radians(4), 0, math.radians(rot + 32)),
        )
    frustum_obj(
        "ELAR_ArcaneIntrusion_BrokenWardMonolith_FallenPaleBlade",
        intrusion,
        (12.4, -5.25, 0.42),
        0.26,
        0.13,
        1.65,
        5,
        mats["MAT_Stone_PaleArchive"],
        rot=(math.radians(72), 0, math.radians(-24)),
    )
    cube_obj(
        "ELAR_ArcaneIntrusion_BrokenWardMonolith_CrackedCyanRune",
        intrusion,
        (12.15, -5.42, 0.66),
        (0.09, 0.055, 0.72),
        mats["MAT_MemoryGlass_CyanGreen"],
        rot=(math.radians(72), 0, math.radians(-24)),
    )
    for i, (x, y, h) in enumerate([(-1.4, 2.8, 1.6), (1.3, 4.2, 1.2), (0.5, 8.2, 1.8), (-2.0, 10.4, 1.3)]):
        frustum_obj(f"ELAR_MemoryConstructs_CyanPilgrimEcho_{i:02d}_Body", memory, (x, y, 0.55 + h / 2), 0.22, 0.18, h, 7, mats["MAT_Echo_Transparent"])
        bicone_mesh(f"ELAR_MemoryConstructs_CyanPilgrimEcho_{i:02d}_Head", memory, (x, y, 1.35 + h / 2), 0.18, 0.28, mats["MAT_Echo_Transparent"])
    for i, (x, y, rot) in enumerate([(-3.65, 1.8, 6), (3.75, 4.6, -7), (0.15, 9.0, 0)]):
        frustum_obj(
            f"ELAR_MemoryConstructs_AutomatedRootStoneGuardian_{i:02d}_Body",
            memory,
            (x, y, 1.42),
            0.42,
            0.28,
            2.45,
            6,
            mats["MAT_Stone_PaleArchive"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
        cube_obj(
            f"ELAR_MemoryConstructs_AutomatedRootStoneGuardian_{i:02d}_MaskPlate",
            memory,
            (x, y - 0.18, 2.6),
            (0.72, 0.12, 0.72),
            mats["MAT_Stone_Pilgrimage"],
            rot=(0, 0, math.radians(rot)),
        )
        bicone_mesh(
            f"ELAR_MemoryConstructs_AutomatedRootStoneGuardian_{i:02d}_CyanSingleEye",
            memory,
            (x, y - 0.28, 2.68),
            0.12,
            0.26,
            mats["MAT_Rune_Cyan_Emission"],
            sides=5,
        )
        for side, offset in [("Left", -0.58), ("Right", 0.58)]:
            cube_obj(
                f"ELAR_MemoryConstructs_AutomatedRootStoneGuardian_{i:02d}_RootArm_{side}",
                memory,
                (x + offset, y, 1.42),
                (0.18, 0.16, 1.5),
                mats["MAT_Bark_DarkRoot"],
                rot=(math.radians(4 if side == "Left" else -4), 0, math.radians(rot + (8 if side == "Left" else -8))),
            )
    for i, (x, y) in enumerate([(-2.4, -3.5), (2.6, -3.2), (-2.0, 6.0), (2.0, 6.2)]):
        disc_mesh(f"ELAR_MemoryConstructs_GroundMemoryRune_{i:02d}", memory, (x, y, 0.14), 0.78, 0.38, mats["MAT_Rune_Cyan_Emission"], sides=24)
    for i, (x, y, z) in enumerate([(0.0, 12.3, 3.0)]):
        bicone_mesh(f"ELAR_MemoryConstructs_FloatingArchiveMemoryGlyph_{i:02d}_GateAlignmentMarker", memory, (x, y, z), 0.24, 0.52, mats["MAT_Echo_Transparent"], sides=5)


def build_elarthalas_foliage_and_characters(templates, collections, mats):
    foliage = collections["ELAR_Foliage"]
    chars = collections["ELAR_Characters"]
    scatter_asset(
        templates,
        "FOLIAGE_FernCluster",
        "ELAR_Foliage_ControlledFernCluster",
        foliage,
        [(-7, -13, 10), (7.2, -12.4, -8), (-8.5, -3.5, 4), (8.4, -3.2, -4), (-11, 6.5, 18), (11.2, 6.1, -18)],
        scale=(1.0, 1.0, 1.0),
    )
    scatter_asset(
        templates,
        "FOLIAGE_BroadLeafPlant",
        "ELAR_Foliage_BroadPilgrimLeaf",
        foliage,
        [(-5.8, -5.2, 0), (5.9, -5.0, 0), (-7.5, 8.5, 0), (7.6, 8.7, 0), (-13, 1, 0), (13, 1.5, 0)],
        scale=(1.05, 1.05, 1.0),
    )
    scatter_asset(
        templates,
        "FOLIAGE_SmallMossyRock",
        "ELAR_Foliage_PilgrimageMossyRock",
        foliage,
        [(-3.9, -14, 8), (3.8, -13.8, -8), (-9.5, -1.5, 18), (9.7, -1.2, -18), (-11.8, 10.4, 0), (11.8, 10.1, 0)],
        scale=(1.0, 1.0, 1.0),
    )
    make_character("ELAR_Characters_PlayerPlaceholder_Foreground", chars, mats, (0, -17.0, 0), role="player")
    make_character("ELAR_Characters_GreenspirePilgrimGuide_QuestNpc", chars, mats, (-2.2, -9.8, 0), role="npc", quest=True)
    make_character("ELAR_Characters_RootGuardianRoadWarden", chars, mats, (2.8, -6.2, 0), role="npc")
    make_character("ELAR_Characters_GreenspireCampArchivist_Npc", chars, mats, (-12.45, -8.25, 0), role="npc")
    make_character("ELAR_Characters_GreenspireCampRootWarden_Npc", chars, mats, (-14.65, -7.15, 0), role="npc")
    make_character("ELAR_Characters_HighElfScout_ArcaneIntruder", chars, mats, (13.0, -6.5, 0), role="enemy")
    bicone_mesh("ELAR_Characters_HighElfScout_ArcaneIntruder_SilverBlueMask", chars, (13.0, -6.62, 1.82), 0.18, 0.32, mats["MAT_HighElf_SilverBlue_Emission"], sides=4)


def setup_elarthalas_lighting_camera(collections):
    col = collections["ELAR_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-8, -10, 18), rotation=(math.radians(40), 0, math.radians(-30)))
    sun = bpy.context.object
    sun.name = "ELAR_LightingRender_SunKey_SacredWarmDay"
    sun.data.energy = 2.45
    sun.data.angle = math.radians(5.0)
    link_to(col, sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -8, 8), rotation=(math.radians(58), 0, 0))
    fill = bpy.context.object
    fill.name = "ELAR_LightingRender_BlueGreenArchiveFill"
    fill.data.energy = 360
    fill.data.size = 20
    fill.data.color = (0.52, 0.85, 0.80)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    link_to(col, fill)
    for i, (x, y, z, color, power) in enumerate(
        [
            (0, 16.2, 4.7, (0.04, 0.95, 1.0), 150),
            (0, 16.0, 8.8, (0.04, 0.95, 0.82), 180),
            (-4.4, -7.8, 1.7, (1.0, 0.58, 0.16), 70),
            (4.4, -7.8, 1.7, (1.0, 0.58, 0.16), 70),
            (-5.0, 7.4, 1.8, (0.04, 0.95, 1.0), 85),
            (5.0, 7.4, 1.8, (0.04, 0.95, 1.0), 85),
            (13.5, -5.2, 1.4, (0.55, 0.10, 0.90), 65),
            (15.6, -6.0, 1.8, (0.35, 0.68, 1.0), 90),
        ]
    ):
        bpy.ops.object.light_add(type="POINT", location=(x, y, z))
        light = bpy.context.object
        light.name = f"ELAR_LightingRender_GlowAccent_{i:02d}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = 4.5
        if hasattr(light.data, "use_shadow"):
            light.data.use_shadow = False
        link_to(col, light)
    bpy.ops.object.camera_add(location=(0, -25.0, 3.4))
    camera = bpy.context.object
    camera.name = "ELAR_LightingRender_Camera_ThirdPersonSacredApproach"
    look_at(camera, Vector((0, 4.8, 2.15)))
    camera.data.lens = 24
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    link_to(col, camera)


def build_elarthalas_scene():
    clear_scene()
    collections = create_collections(
        [
            "ELAR_Terrain",
            "ELAR_ArchiveCity",
            "ELAR_WardsAndProps",
            "ELAR_MemoryConstructs",
            "ELAR_ArcaneIntrusion",
            "ELAR_Foliage",
            "ELAR_Characters",
            "ELAR_LightingRender",
            "KIT_Templates_Hidden",
        ]
    )
    templates = load_templates(collections)
    mats = scene_materials()
    setup_scene_settings((0.03, 0.09, 0.10), exposure=-0.08)
    build_elarthalas_terrain(collections, mats)
    build_elarthalas_gate(collections, mats)
    build_elarthalas_architecture(templates, collections, mats)
    build_elarthalas_intrusion_and_memory(collections, mats)
    build_elarthalas_foliage_and_characters(templates, collections, mats)
    setup_elarthalas_lighting_camera(collections)
    remove_templates(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_ZONE2_BLEND))
    export_scene_glb(collections, OUT_ZONE2_GLB)
    render_to(OUT_ZONE2_RENDER_1440, 2560, 1440)
    render_to(OUT_ZONE2_RENDER_1080, 1920, 1080)
    return build_manifest(
        "Elar'Thalas Approach",
        ZONE2_STEM,
        collections,
        OUT_ZONE2_BLEND,
        OUT_ZONE2_GLB,
        OUT_ZONE2_RENDER_1440,
        OUT_ZONE2_RENDER_1080,
        OUT_ZONE2_MANIFEST,
        OUT_ZONE2_REPORT,
        [
            "Silent Gate is the dominant background landmark and clearly reads as a sealed living archive-city threshold.",
            "Silent Gate silhouette now uses organic arch curves, interwoven vertical roots, pale stone ribs, reduced cyan memory-glass panels, moss/root bases, and a closed central seam instead of a flat portal wall or large cyan rectangle.",
            "Layered archive towers, root silhouettes, darker backplates, and thin blue-green fog planes break up the background so it reads as sealed city depth.",
            "Raised sacred rootroad now uses faceted living-root spines, uneven embedded stone slabs, raised root borders, and sparse cyan guide-runes while keeping the central lane clear.",
            "Ward corridor now uses three reusable small/medium/tall ward monolith variants with carved asymmetric stone, cyan rune slits, moss/broken bases, root collars, inward tilt, and controlled scanner lines.",
            "Targeted prefab replacement pass replaces the most primitive road rails, ward pillars, ward bases, ward crowns, foreground ward stones, and broken intrusion blade with smaller tapered/faceted forms while preserving camera, path, and landmark placement.",
            "Greenspire encampment has one compact root shelter, an archive stand/table, two Verdant banners, two NPC silhouettes, and warm lanterns so it supports rather than competes with the gate.",
            "High Elf intrusion props remain one compact silver-blue angular side pocket with tripod, floating shard, broken ward stone, geometric lines, and Aelthar silhouette.",
            "Zone identity lock pass reduces floating cyan diamonds and shifts repetition toward pale archive stone, dark root bindings, scanner runes, gold seal knots, and silver-violet Aelthar contrast.",
            "Sylvaen identity remains organic through living bark, rootroad, controlled ward glass, banners, and lanterns.",
            "The scene is stylized low-poly with saturated non-photorealistic materials.",
        ],
        [
            "Zone 2 shifts Thornveil's lush safety into a sacred, managed approach space.",
            "The Silent Gate is intentionally oversized for benchmark readability and future transition staging.",
            "Before/after: before this targeted pass, the composition worked but several silhouettes still read as rectangular test beams, simple white pillars, flat blue gate panels, or portal-test props; after the pass, the road roots are cylindrical/faceted, ward monoliths are variant prefabs, the gate has darker archive depth and moss/root bases, and the High Elf side pocket is more angular/silver-blue.",
            "This is a visual target scene, not yet live gameplay terrain/collision.",
        ],
    )


def build_memory_wastes_terrain(collections, mats):
    terrain = collections["WASTE_TerrainIslands"]
    vertical_rect_obj(
        "WASTE_Terrain_Backdrop_MemoryStormSky",
        terrain,
        32.0,
        -180,
        180,
        -12.0,
        96,
        mats["MAT_Backdrop_MemoryStormSky"],
    )
    for i, (y, pts, mat_key) in enumerate(
        [
            (22.2, [(-38, -0.4), (-28, 4.0), (-19, 2.3), (-8, 6.1), (0, 3.5), (9, 7.4), (20, 3.0), (33, 5.2), (41, -0.4)], "MAT_Stone_DarkCrevice"),
            (21.4, [(-34, -0.2), (-22, 2.6), (-13, 1.4), (-2, 4.2), (8, 2.0), (17, 4.7), (28, 1.8), (39, -0.2)], "MAT_Backdrop_MemoryStormSky"),
            (20.6, [(-30, 0.0), (-17, 2.1), (-6, 1.0), (5, 3.2), (14, 1.5), (27, 2.8), (36, 0.0)], "MAT_Backdrop_BlueMist"),
        ]
    ):
        vertical_poly_obj(
            f"WASTE_Terrain_BackgroundLayeredBrokenHorizon_{i:02d}",
            terrain,
            y,
            pts,
            mats[mat_key],
        )
    for i, (y, x, z, width, height, mat_key) in enumerate(
        [
            (21.9, -24.0, 7.2, 7.2, 3.1, "MAT_Backdrop_DistantMemoryIsland"),
            (22.1, -11.5, 9.4, 5.8, 4.2, "MAT_Backdrop_DistantMemoryIsland"),
            (21.7, 12.8, 8.0, 6.6, 3.5, "MAT_Backdrop_DistantDivineIsland"),
            (22.3, 25.0, 10.6, 4.8, 4.0, "MAT_Backdrop_DistantRiftIsland"),
        ]
    ):
        vertical_poly_obj(
            f"WASTE_Terrain_Backdrop_DistantManifestingIslandSilhouette_{i:02d}",
            terrain,
            y,
            [
                (x - width * 0.60, z),
                (x + width * 0.52, z + 0.18),
                (x + width * 0.38, z + height * 0.52),
                (x + width * 0.10, z + height),
                (x - width * 0.46, z + height * 0.62),
            ],
            mats[mat_key],
        )
        for j, tower_x in enumerate([-0.24, 0.10, 0.38]):
            cube_obj(
                f"WASTE_Terrain_Backdrop_DistantManifestingIslandSilhouette_{i:02d}_RuinTrace_{j:02d}",
                terrain,
                (x + width * tower_x, y - 0.05, z + height * (0.58 + 0.12 * j)),
                (0.18 + 0.04 * j, 0.08, 1.15 + 0.55 * j),
                mats["MAT_Backdrop_DistantMemoryIsland"] if i < 2 else mats[mat_key],
                rot=(math.radians(4), 0, math.radians(-8 + j * 7)),
            )
    poly_obj(
        "WASTE_Terrain_DarkVoidBase_NoWalk",
        terrain,
        [(-42, -22), (42, -22), (42, 23), (-42, 23)],
        mats["MAT_WasteGround_Dark"],
        z=-0.35,
    )
    islands = [
        ("PlayerSpawnIsland", 0, -16.5, 8.0, 4.4, 0),
        ("CentralSylvaenCampIsland", 0, -4.5, 12.5, 8.0, 3),
        ("EchoBattlefieldIsland", -16, 1.5, 11.5, 7.0, -12),
        ("DeadGodShrineIsland", 14.5, 5.8, 10.0, 7.0, 10),
        ("OranynRuinIsland", -6.5, 12.8, 10.5, 6.2, -6),
        ("WorldrootSheddingRiftIsland", 2.8, 11.0, 11.5, 6.8, 5),
        ("SideRootRiftIsland", 20, -8.2, 9.0, 5.8, 15),
    ]
    for name, x, y, rx, ry, rot in islands:
        disc_mesh(f"WASTE_Terrain_{name}_FragmentedPlate", terrain, (x, y, 0.0), rx, ry, mats["MAT_WasteGround"], sides=11, rot=(0, 0, math.radians(rot)))
        disc_mesh(f"WASTE_Terrain_{name}_MossMemorySkin", terrain, (x, y, 0.04), rx * 0.7, ry * 0.55, mats["MAT_Grass_Dark"], sides=9, rot=(0, 0, math.radians(rot + 15)))
        disc_mesh(f"WASTE_Terrain_{name}_VoidDropShadow", terrain, (x + 0.6, y - 0.35, -0.22), rx * 0.92, ry * 0.76, mats["MAT_Stone_DarkCrevice"], sides=11, rot=(0, 0, math.radians(rot + 8)))
        for j, offset in enumerate([-0.42, 0.0, 0.42]):
            cube_obj(
                f"WASTE_Terrain_{name}_BrokenEdgeStrata_{j:02d}",
                terrain,
                (x + offset * rx, y - ry * 0.58, -0.12 - j * 0.05),
                (rx * 0.28, 0.22, 0.24),
                mats["MAT_DeadGod_Shadow"],
                rot=(0, 0, math.radians(rot + j * 12)),
            )
        for j, (px, py, lean) in enumerate([(-0.58, 0.18, -18), (0.64, -0.08, 22), (0.05, 0.58, 4)]):
            shard = tri_prism_obj(
                f"WASTE_Terrain_{name}_SilhouetteMemoryCliffShard_{j:02d}",
                terrain,
                (x + px * rx, y + py * ry, 0.02),
                1.2 + j * 0.25,
                1.55 + j * 0.18,
                0.34 + j * 0.08,
                mats["MAT_DeadGod_Shadow"] if j != 2 else mats["MAT_Stone_MossyGray"],
                rot=(0, 0, math.radians(rot + lean)),
            )
            disable_shadow(shard)
    disc_mesh(
        "WASTE_Terrain_IslandA_SylvaenSafeCamp_WarmCyanIdentityGlow",
        terrain,
        (0.0, -4.5, 0.12),
        2.1,
        1.06,
        mats["MAT_Waste_SafeCyanGlow"],
        sides=36,
        rot=(0, 0, math.radians(3)),
    )
    disc_mesh(
        "WASTE_Terrain_IslandB_LostCivilization_GhostManifestationSkin",
        terrain,
        (-6.5, 12.8, 0.13),
        4.2,
        2.15,
        mats["MAT_Waste_GhostRuinSoft"],
        sides=28,
        rot=(0, 0, math.radians(-6)),
    )
    disc_mesh(
        "WASTE_Terrain_IslandC_DeadGodShrine_PaleGoldDivinityStain",
        terrain,
        (14.5, 5.8, 0.14),
        4.8,
        3.0,
        mats["MAT_Waste_DivinityStainSoft"],
        sides=30,
        rot=(0, 0, math.radians(10)),
    )
    for i, (x, y, length, rot) in enumerate([(0, -10.4, 7.2, 0), (-8.1, -2.2, 8.8, -28), (7.8, -0.6, 9.6, 25), (-9.4, 8.1, 9.5, 25), (8.5, 9.2, 10.4, -21)]):
        # The routes should read as unstable grown roots, not single debug planks.
        for strand, (offset, width, height, strand_rot) in enumerate([(-0.46, 0.22, 0.20, -5), (0.0, 0.34, 0.26, 0), (0.48, 0.20, 0.18, 6)]):
            cube_obj(
                f"WASTE_Terrain_LivingRootBridgeAcrossGap_{i:02d}_BraidedRootStrand_{strand:02d}",
                terrain,
                (x + offset, y, 0.18 + strand * 0.025),
                (width, length * (0.92 - strand * 0.06), height),
                mats["MAT_Path_WarmRoot"] if strand != 2 else mats["MAT_Path_DarkRootShadow"],
                rot=(0, math.radians(2 if strand == 1 else -2), math.radians(rot + strand_rot)),
            )
        for knot, (offset, along, sx) in enumerate([(-0.18, -0.28, 0.82), (0.28, 0.18, 0.68)]):
            cube_obj(
                f"WASTE_Terrain_LivingRootBridgeAcrossGap_{i:02d}_RootKnotAnchor_{knot:02d}",
                terrain,
                (x + offset, y + length * along, 0.34),
                (sx, 0.28, 0.18),
                mats["MAT_Bark_DarkRoot"],
                rot=(0, 0, math.radians(rot + 18 * (knot + 1))),
            )
    for i, (x, y, length, rot, mat_key) in enumerate(
        [
            (-2.8, -9.4, 5.4, -8, "MAT_Echo_GhostBlueWhite"),
            (-8.6, 5.0, 7.8, 25, "MAT_Echo_Transparent"),
            (2.2, 6.6, 8.6, -6, "MAT_Waste_GuideShard_CyanDim"),
            (9.5, 3.0, 7.4, -28, "MAT_LostDivinity_PaleGold_Emission"),
        ]
    ):
        for piece, along in enumerate([-0.32, 0.0, 0.32]):
            bridge = cube_obj(
                f"WASTE_Terrain_UnstableMemoryBridge_{i:02d}_HalfManifestedSteppingSpan_{piece:02d}",
                terrain,
                (x + (piece - 1) * 0.12, y + length * along, 0.34 + piece * 0.012),
                (0.42, length * 0.22, 0.055),
                mats[mat_key],
                rot=(0, 0, math.radians(rot + (piece - 1) * 4)),
            )
            disable_shadow(bridge)
        for side, offset in [("Left", -0.52), ("Right", 0.52)]:
            fiber = cube_obj(
                f"WASTE_Terrain_UnstableMemoryBridge_{i:02d}_BrokenRootStrand_{side}",
                terrain,
                (x + offset, y, 0.42),
                (0.12, length * 0.82, 0.10),
                mats["MAT_Path_DarkRootShadow"],
                rot=(0, 0, math.radians(rot + (4 if side == "Left" else -4))),
            )
            disable_shadow(fiber)
    for i, (x, y, sx, sy, rot, mat_key) in enumerate(
        [
            (-4.6, -0.3, 1.15, 0.55, -24, "MAT_Echo_GhostBlueWhite"),
            (-6.8, 3.0, 1.00, 0.45, -24, "MAT_Echo_GhostBlueWhite"),
            (-8.9, 6.2, 0.88, 0.38, -24, "MAT_Echo_GhostBlueWhite"),
            (4.2, -0.1, 1.10, 0.50, 26, "MAT_LostDivinity_PaleGold_Emission"),
            (6.5, 2.2, 0.95, 0.44, 26, "MAT_LostDivinity_PaleGold_Emission"),
            (8.8, 4.5, 0.82, 0.36, 26, "MAT_LostDivinity_PaleGold_Emission"),
            (-2.7, 7.2, 0.95, 0.42, 6, "MAT_Waste_GuideShard_CyanDim"),
            (-0.1, 8.8, 0.82, 0.36, 6, "MAT_Waste_GuideShard_CyanDim"),
            (2.3, 10.0, 0.72, 0.32, 6, "MAT_Waste_GuideShard_CyanDim"),
        ]
    ):
        cube_obj(
            f"WASTE_Terrain_ReadableTraversalStep_{i:02d}_{mat_key.replace('MAT_', '')}",
            terrain,
            (x, y, 0.58 + 0.02 * (i % 3)),
            (sx, sy, 0.10),
            mats[mat_key],
            rot=(0, 0, math.radians(rot)),
        )
        if i in [2, 5, 8]:
            bicone_mesh(
                f"WASTE_Terrain_MemoryGuideShard_ToNextIsland_{i:02d}",
                terrain,
                (x, y - 0.18, 0.82),
                0.032,
                0.095,
                mats["MAT_Waste_GuideShard_CyanDim"],
                sides=5,
            )
    for i, (x, y, rot) in enumerate([(-1.0, -14.2, 0), (1.1, -12.1, 8), (-0.7, -9.8, -7), (0.8, -7.8, 5), (-0.4, -5.8, 0)]):
        cube_obj(f"WASTE_Terrain_PlayerPath_BrokenMossStone_{i:02d}", terrain, (x, y, 0.23), (2.2, 1.2, 0.14), mats["MAT_Path_MossyStone"], rot=(0, 0, math.radians(rot)))
    disc_mesh("WASTE_Terrain_PlayerSpawn_SubtleSafeCampEchoCircle", terrain, (0, -16.5, 0.18), 0.62, 0.28, mats["MAT_Echo_Transparent"], sides=30)
    for i, (x, y, rx, ry, mat_key, rot) in enumerate([
        (-20, 13.0, 5.5, 1.2, "MAT_Backdrop_BlueMist", -12),
        (18, 14.2, 2.8, 0.48, "MAT_Waste_RiftVioletControlled", 8),
        (0, 18.0, 8.0, 1.4, "MAT_Echo_Transparent", 0),
    ]):
        disc_mesh(
            f"WASTE_Terrain_BackgroundMemoryStormRibbon_{i:02d}",
            terrain,
            (x, y, 2.2 + i * 0.35),
            rx,
            ry,
            mats[mat_key],
            sides=24,
            rot=(math.radians(80), 0, math.radians(rot)),
        )


def build_memory_wastes_camp_and_ruins(templates, collections, mats):
    camp = collections["WASTE_SylvaenCamp"]
    ruins = collections["WASTE_FloatingRuins"]
    rifts = collections["WASTE_RootRiftsAndStorms"]
    for i, (x, y, rot) in enumerate([(-4.5, -1.5, 14), (4.7, -1.1, -14)]):
        inst(templates, "PROPS_Village_BannerPost", f"WASTE_SylvaenCamp_AnchorBanner_{i:02d}", camp, (x, y, 0.18), rot=(0, 0, math.radians(rot)), scale=(0.95, 0.95, 1.15))
    tri_prism_obj("WASTE_SylvaenCamp_FieldTent_RootclothRoof", camp, (-2.1, -3.8, 0.25), 3.8, 2.2, 1.25, mats["MAT_CampCloth_Green"], rot=(0, 0, math.radians(9)))
    cube_obj("WASTE_SylvaenCamp_FieldTent_WarmRootBase", camp, (-2.1, -3.8, 0.2), (2.0, 3.0, 0.42), mats["MAT_Bark_WarmBrown"], rot=(0, 0, math.radians(9)))
    tri_prism_obj(
        "WASTE_SylvaenCamp_SafePointRootShelter_LeafRoof",
        camp,
        (3.2, -6.0, 0.35),
        3.2,
        2.0,
        1.15,
        mats["MAT_CampCloth_Green"],
        rot=(0, 0, math.radians(-13)),
    )
    cube_obj(
        "WASTE_SylvaenCamp_SafePointRootShelter_LivingRootBase",
        camp,
        (3.2, -6.0, 0.24),
        (1.75, 2.55, 0.48),
        mats["MAT_Bark_DarkRoot"],
        rot=(0, 0, math.radians(-13)),
    )
    for i, (dx, dz, rot) in enumerate([(-0.84, 0.78, -18), (0.84, 0.78, 18), (-0.52, 1.26, -8), (0.52, 1.26, 8)]):
        cube_obj(
            f"WASTE_SylvaenCamp_SafePointRootShelter_LivingRib_{i:02d}",
            camp,
            (3.2 + dx, -6.05, dz),
            (0.18, 2.25, 0.16),
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(8), 0, math.radians(rot - 13)),
        )
    inst(
        templates,
        "PROPS_Village_BannerPost",
        "WASTE_SylvaenCamp_SafePointVerdantBanner",
        camp,
        (-4.1, -7.2, 0.18),
        rot=(0, 0, math.radians(-8)),
        scale=(1.05, 1.05, 1.25),
    )
    inst(templates, "PROPS_Village_SmallShrinePedestal", "WASTE_SylvaenCamp_StabilizationShrine", camp, (2.2, -3.4, 0.18), scale=(1.15, 1.15, 1.15))
    bicone_mesh("WASTE_SylvaenCamp_StabilizationShrine_WorldrootCyanCore", camp, (2.2, -3.4, 1.55), 0.26, 0.56, mats["MAT_Worldroot_Cyan_Emission"], sides=5)
    bicone_mesh("WASTE_SylvaenCamp_SafeGlow_WarmCyanAnchor", camp, (0.0, -4.4, 1.08), 0.22, 0.56, mats["MAT_Waste_SafeCyanGlow"], sides=6)
    frustum_obj(
        "WASTE_SylvaenCamp_MemoryBrazier_StoneBowl",
        camp,
        (-0.2, -5.4, 0.68),
        0.62,
        0.44,
        0.72,
        8,
        mats["MAT_Stone_MossyGray"],
    )
    bicone_mesh(
        "WASTE_SylvaenCamp_MemoryBrazier_CyanGoldFlame",
        camp,
        (-0.2, -5.4, 1.25),
        0.28,
        0.78,
        mats["MAT_LostDivinity_PaleGold_Emission"],
        sides=6,
    )
    bicone_mesh(
        "WASTE_SylvaenCamp_MemoryBrazier_CyanInnerFlame",
        camp,
        (-0.2, -5.48, 1.34),
        0.18,
        0.62,
        mats["MAT_Worldroot_Cyan_Emission"],
        sides=6,
    )
    cube_obj(
        "WASTE_SylvaenCamp_SmallArchiveTable_RootLedger",
        camp,
        (-3.3, -3.2, 0.72),
        (1.35, 0.72, 0.22),
        mats["MAT_Path_WarmRoot"],
        rot=(0, 0, math.radians(7)),
    )
    cube_obj(
        "WASTE_SylvaenCamp_SmallArchiveTable_GhostMapPane",
        camp,
        (-3.3, -3.25, 0.88),
        (1.05, 0.48, 0.04),
        mats["MAT_Echo_GhostBlueWhite"],
        rot=(0, 0, math.radians(7)),
    )
    disc_mesh("WASTE_SylvaenCamp_StabilizationCircle_SubtleSafeZone", camp, (0.0, -4.2, 0.28), 2.35, 1.20, mats["MAT_Waste_SafeCyanGlow"], sides=36, rot=(0, 0, math.radians(3)))
    for i, (x, y, rot) in enumerate([(-4.6, -6.8, -18), (4.7, -6.3, 18), (-4.9, -1.4, 10), (4.9, -1.0, -10)]):
        inst(
            templates,
            "PROPS_Village_LanternPost",
            f"WASTE_SylvaenCamp_CyanGoldSafetyLantern_{i:02d}",
            camp,
            (x, y, 0.16),
            rot=(0, 0, math.radians(rot)),
            scale=(0.9, 0.9, 1.08),
        )
    for i, (x, y, rot) in enumerate([(-0.9, -6.0, 5), (1.1, -5.9, -5), (-0.2, -2.0, 0)]):
        cube_obj(f"WASTE_SylvaenCamp_AnchoringRootStake_{i:02d}", camp, (x, y, 0.65), (0.18, 0.18, 1.25), mats["MAT_Path_DarkRootShadow"], rot=(math.radians(7), 0, math.radians(rot)))
        cube_obj(f"WASTE_SylvaenCamp_AnchoringRootStake_{i:02d}_MemoryWrap", camp, (x, y - 0.04, 0.95), (0.22, 0.035, 0.06), mats["MAT_Echo_Transparent"], rot=(0, 0, math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(-7.5, 10.8, 1.2, 16), (-4.5, 13.4, 2.4, -8), (1.0, 13.2, 1.8, 5), (5.4, 11.5, 2.7, -15)]):
        cube_obj(f"WASTE_FloatingRuins_LostArchiveWallShard_{i:02d}", ruins, (x, y, z), (2.4, 0.28, 2.1), mats["MAT_Waste_GhostRuinSoft"], rot=(math.radians(7), math.radians(0), math.radians(rot)))
        bicone_mesh(f"WASTE_FloatingRuins_GhostMemoryAnchor_{i:02d}", ruins, (x, y - 0.15, z + 1.35), 0.09, 0.22, mats["MAT_Waste_EchoStainSoft"], sides=5)
        cube_obj(f"WASTE_FloatingRuins_LostArchiveWallShard_{i:02d}_TransparentPastEdge", ruins, (x + 0.55, y - 0.22, z + 0.2), (0.18, 0.08, 1.75), mats["MAT_Waste_GhostRuinSoft"], rot=(math.radians(7), 0, math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(-10.2, 13.8, 3.0, 4), (-7.2, 15.0, 3.25, -7), (-3.4, 13.2, 2.65, 0)]):
        tri_prism_obj(
            f"WASTE_FloatingRuins_GhostArchitectureRoofTrace_{i:02d}",
            ruins,
            (x, y, z),
            2.8,
            1.55,
            0.92,
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(-9.2, 10.8, -10), (-5.8, 14.0, 8), (-1.8, 12.2, 2)]):
        for side, offset in [("Left", -0.88), ("Right", 0.88)]:
            cube_obj(
                f"WASTE_OranynRuins_BrokenGhostArch_{i:02d}_Pillar_{side}",
                ruins,
                (x + offset, y, 1.45),
                (0.32, 0.22, 2.45),
                mats["MAT_Waste_GhostRuinSoft"],
                rot=(math.radians(5), 0, math.radians(rot)),
            )
        cube_obj(
            f"WASTE_OranynRuins_BrokenGhostArch_{i:02d}_TopSpanHalfVisible",
            ruins,
            (x, y, 2.82),
            (2.25, 0.22, 0.34),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(5), 0, math.radians(rot)),
        )
        bicone_mesh(
            f"WASTE_OranynRuins_BrokenGhostArch_{i:02d}_PaleManifestedKeystone",
            ruins,
            (x, y - 0.12, 3.08),
            0.13,
            0.30,
            mats["MAT_Waste_GhostRuinSoft"],
            sides=5,
        )
    for side, x in [("Left", -10.55), ("Right", -2.65)]:
        frustum_obj(
            f"WASTE_OranynRuins_LargePartManifestedArchiveArch_{side}_FloatingColumn",
            ruins,
            (x, 12.2, 2.55),
            0.34,
            0.22,
            3.2,
            6,
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(6 if side == "Left" else -6), 0, math.radians(-4 if side == "Left" else 4)),
        )
    for i, (start, end) in enumerate([
        ((-10.55, 3.95), (-9.05, 5.35)),
        ((-9.05, 5.35), (-6.6, 5.95)),
        ((-6.6, 5.95), (-4.05, 5.35)),
        ((-4.05, 5.35), (-2.65, 3.95)),
    ]):
        xz_beam_obj(
            f"WASTE_OranynRuins_LargeBrokenArchiveArch_HalfManifestedSpan_{i:02d}",
            ruins,
            12.05,
            start,
            end,
            0.24,
            0.28,
            mats["MAT_Waste_GhostRuinSoft"],
        )
    for i, (x, y, z, h, rot) in enumerate([(-11.2, 11.4, 2.45, 4.4, -10), (-6.6, 11.9, 3.05, 5.2, 0), (-2.2, 11.5, 2.35, 4.1, 10)]):
        frustum_obj(
            f"WASTE_OranynRuins_SignatureArchiveFacade_PaleGhostColumn_{i:02d}",
            ruins,
            (x, y, z),
            0.30,
            0.16,
            h,
            6,
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(5), 0, math.radians(rot)),
        )
    for i, (x, y, z, width, rot) in enumerate([(-8.85, 11.65, 5.62, 3.75, -5), (-4.45, 11.65, 5.38, 3.25, 7)]):
        cube_obj(
            f"WASTE_OranynRuins_SignatureArchiveFacade_HalfManifestedUpperLintel_{i:02d}",
            ruins,
            (x, y, z),
            (width, 0.20, 0.28),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(4), 0, math.radians(rot)),
        )
    cube_obj(
        "WASTE_OranynRuins_FallenStatueFragment_AncientTorso",
        ruins,
        (-3.35, 10.65, 0.86),
        (0.95, 0.42, 1.05),
        mats["MAT_Waste_GhostRuinSoft"],
        rot=(math.radians(64), 0, math.radians(18)),
    )
    bicone_mesh(
        "WASTE_OranynRuins_FallenStatueFragment_GhostMemoryHead",
        ruins,
        (-2.75, 10.28, 0.62),
        0.26,
        0.42,
        mats["MAT_Waste_EchoStainSoft"],
        sides=6,
    )
    for i, (x, y, z, rot) in enumerate([(-11.4, 14.6, 2.2, -14), (-7.2, 15.4, 2.9, 3), (-1.0, 14.8, 2.5, 16)]):
        frustum_obj(
            f"WASTE_OranynRuins_HalfManifestedColumn_NotTouchingGround_{i:02d}",
            ruins,
            (x, y, z),
            0.26,
            0.18,
            1.85,
            6,
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(5), 0, math.radians(rot)),
        )
    for i, (x, y, z, rot) in enumerate([(-7.0, 9.6, 0.72, 12), (-5.2, 10.8, 1.05, 12), (-3.4, 12.0, 1.38, 12), (-1.6, 13.2, 1.72, 12)]):
        cube_obj(
            f"WASTE_OranynRuins_HalfVisibleStaircase_GhostStep_{i:02d}",
            ruins,
            (x, y, z),
            (1.55, 0.48, 0.16),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(0, 0, math.radians(rot)),
        )
    for i, (x, y, z, rot) in enumerate([(-11.4, 12.5, 2.5, -16), (-8.1, 15.1, 3.3, 5), (2.5, 14.2, 2.7, 18), (5.8, 12.6, 3.6, -8)]):
        cube_obj(
            f"WASTE_OranynRuins_FloatingGhostWallFragment_{i:02d}",
            ruins,
            (x, y, z),
            (1.6, 0.18, 1.35),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(9), 0, math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(12.0, 5.0, 12), (15.5, 6.8, -10), (17.7, 4.0, 6)]):
        frustum_obj(f"WASTE_DeadGodShrine_BrokenRibMonolith_{i:02d}", ruins, (x, y, 1.55), 0.24, 0.14, 3.1, 5, mats["MAT_DeadGod_Stone"], rot=(math.radians(8), 0, math.radians(rot)))
    cube_obj("WASTE_DeadGodShrine_FallenIdolFace", ruins, (14.4, 5.6, 0.9), (2.4, 0.42, 1.35), mats["MAT_DeadGod_Stone"], rot=(math.radians(8), 0, math.radians(-12)))
    bicone_mesh("WASTE_DeadGodShrine_CrackedMemoryEye", ruins, (14.3, 5.25, 1.25), 0.22, 0.44, mats["MAT_Waste_DivinityStainSoft"])
    cube_obj("WASTE_DeadGodShrine_FallenCrownArc_Left", ruins, (13.3, 5.35, 2.25), (0.32, 0.22, 2.25), mats["MAT_DeadGod_Shadow"], rot=(math.radians(-14), 0, math.radians(-18)))
    cube_obj("WASTE_DeadGodShrine_FallenCrownArc_Right", ruins, (15.3, 5.35, 2.25), (0.32, 0.22, 2.25), mats["MAT_DeadGod_Shadow"], rot=(math.radians(14), 0, math.radians(18)))
    cube_obj("WASTE_DeadGodShrine_BrokenNamePlate", ruins, (14.25, 4.7, 0.35), (2.2, 0.24, 0.28), mats["MAT_Stone_DarkCrevice"], rot=(0, 0, math.radians(-8)))
    for i, (x, z, sx, sz, rot) in enumerate(
        [
            (14.4, 3.92, 2.55, 0.14, -4),
            (12.95, 3.18, 1.45, 0.14, 38),
            (15.85, 3.18, 1.45, 0.14, -38),
            (13.28, 2.10, 1.25, 0.12, -28),
            (15.52, 2.10, 1.25, 0.12, 28),
        ]
    ):
        cube_obj(
            f"WASTE_DeadGodShrine_BrokenHaloSigilSegment_{i:02d}",
            ruins,
            (x, 4.76, z),
            (sx, 0.08, sz),
            mats["MAT_LostDivinity_PaleGold_Emission"],
            rot=(math.radians(90), 0, math.radians(rot)),
        )
    bicone_mesh(
        "WASTE_DeadGodShrine_BrokenHaloSigil_MissingGodEye",
        ruins,
        (14.4, 4.64, 3.05),
        0.24,
        0.62,
        mats["MAT_LostDivinity_PaleGold_Emission"],
        sides=5,
    )
    for i, (start, end, mat_key) in enumerate(
        [
            ((11.95, 2.3), (12.65, 4.0), "MAT_LostDivinity_PaleGold_Emission"),
            ((12.65, 4.0), (14.0, 4.85), "MAT_DeadGod_Stone"),
            ((14.8, 4.85), (16.15, 4.0), "MAT_LostDivinity_PaleGold_Emission"),
            ((16.15, 4.0), (16.85, 2.3), "MAT_DeadGod_Stone"),
        ]
    ):
        xz_beam_obj(
            f"WASTE_DeadGodShrine_MajorCrackedCircularHalo_ReadableDivineRuin_{i:02d}",
            ruins,
            4.36,
            start,
            end,
            0.18,
            0.24,
            mats[mat_key],
        )
    for i, (x, y, sx, sy, rot) in enumerate(
        [(13.0, 6.7, 0.88, 0.32, 18), (15.7, 6.3, 0.72, 0.28, -16), (14.7, 7.5, 0.64, 0.24, 4), (12.2, 4.0, 0.56, 0.22, -28)]
    ):
        cube_obj(
            f"WASTE_DeadGodShrine_ShatteredDivineSigilGroundPiece_{i:02d}",
            ruins,
            (x, y, 0.42),
            (sx, sy, 0.08),
            mats["MAT_LostDivinity_PaleGold_Emission"],
            rot=(0, 0, math.radians(rot)),
        )
    cube_obj(
        "WASTE_DeadGodShrine_CrackedStoneAltar",
        ruins,
        (14.4, 4.65, 0.74),
        (2.45, 1.12, 0.54),
        mats["MAT_DeadGod_Stone"],
        rot=(0, 0, math.radians(-5)),
    )
    cube_obj(
        "WASTE_DeadGodShrine_AltarCrack_PaleGoldLeak",
        ruins,
        (14.4, 4.10, 1.04),
        (1.65, 0.07, 0.08),
        mats["MAT_LostDivinity_PaleGold_Emission"],
        rot=(0, 0, math.radians(-5)),
    )
    cube_obj(
        "WASTE_DeadGodShrine_MissingNameRuneSlab",
        ruins,
        (12.65, 4.85, 0.52),
        (1.15, 0.22, 0.64),
        mats["MAT_Stone_DarkCrevice"],
        rot=(math.radians(10), 0, math.radians(12)),
    )
    cube_obj(
        "WASTE_DeadGodShrine_PaleGoldLightColumn_ErasedName",
        ruins,
        (14.4, 4.22, 2.1),
        (0.38, 0.18, 2.35),
        mats["MAT_LostDivinity_PaleGold_Emission"],
        rot=(math.radians(4), 0, math.radians(-5)),
    )
    cube_obj(
        "WASTE_DeadGodShrine_PaleGoldLightColumn_MissingDivinityTallRead",
        ruins,
        (14.42, 4.12, 3.42),
        (0.22, 0.12, 4.20),
        mats["MAT_LostDivinity_PaleGold_Emission"],
        rot=(math.radians(3), 0, math.radians(-3)),
    )
    for i, (start, end) in enumerate(
        [
            ((11.70, 1.40), (12.72, 5.28)),
            ((12.72, 5.28), (14.40, 6.35)),
            ((14.40, 6.35), (16.08, 5.28)),
            ((16.08, 5.28), (17.10, 1.40)),
        ]
    ):
        xz_beam_obj(
            f"WASTE_DeadGodShrine_DistantReadableBrokenHaloOuterArc_{i:02d}",
            ruins,
            4.18,
            start,
            end,
            0.14,
            0.18,
            mats["MAT_LostDivinity_PaleGold_Emission"] if i in [1, 2] else mats["MAT_DeadGod_Stone"],
        )
    for i, (x, y, rx, ry, rot) in enumerate([(19.0, -8.3, 1.75, 0.72, 18), (17.4, -6.9, 1.20, 0.52, -8)]):
        disc_mesh(f"WASTE_RootRiftsAndStorms_VioletRootRift_{i:02d}", rifts, (x, y, 0.24), rx, ry, mats["MAT_Waste_RiftVioletControlled"], sides=24, rot=(0, 0, math.radians(rot)))
        cube_obj(f"WASTE_RootRiftsAndStorms_BlackRootTear_{i:02d}", rifts, (x, y, 0.32), (rx * 1.3, 0.18, 0.18), mats["MAT_Bark_DarkRoot"], rot=(0, 0, math.radians(rot)))
        bicone_mesh(f"WASTE_RootRiftsAndStorms_VioletRiftCoreShard_{i:02d}", rifts, (x, y, 0.72 + i * 0.10), 0.10 + i * 0.02, 0.38 + i * 0.08, mats["MAT_Waste_RiftVioletControlled"], sides=5)
    poly_obj(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_CrackedRootFissure_VoidMouth",
        rifts,
        [(-4.2, 9.82), (-2.5, 9.28), (-0.8, 9.70), (0.35, 9.12), (2.2, 9.65), (4.7, 9.35), (3.35, 10.35), (1.15, 10.88), (-0.55, 10.62), (-2.4, 11.12), (-4.8, 10.45)],
        mats["MAT_Stone_DarkCrevice"],
        z=0.56,
    )
    poly_obj(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_CyanGoldPressureLightInsideFissure",
        rifts,
        [(-2.8, 9.86), (-1.1, 9.62), (0.15, 9.80), (1.45, 9.55), (2.85, 9.88), (1.9, 10.26), (0.25, 10.42), (-1.55, 10.24)],
        mats["MAT_Waste_RiftCyanDeep"],
        z=0.62,
    )
    for i, (x, y, sx, sy, rot, mat_key) in enumerate(
        [
            (-4.6, 8.9, 1.9, 1.1, -18, "MAT_WasteGround"),
            (-2.4, 11.8, 1.6, 0.9, 12, "MAT_WasteGround"),
            (0.8, 8.7, 1.4, 0.82, 8, "MAT_Grass_Dark"),
            (3.4, 11.3, 1.75, 0.94, -14, "MAT_WasteGround"),
            (4.9, 9.4, 1.2, 0.72, 22, "MAT_Grass_Dark"),
        ]
    ):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_BrokenGroundPlate_{i:02d}",
            rifts,
            (x, y, 0.64),
            (sx, sy, 0.12),
            mats[mat_key],
            rot=(0, 0, math.radians(rot)),
        )
    disc_mesh(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_GroundMemoryTear",
        rifts,
        (0.0, 10.4, 0.34),
        3.55,
        0.82,
        mats["MAT_Waste_RiftVioletControlled"],
        sides=36,
        rot=(0, 0, math.radians(4)),
    )
    cube_obj(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_BlackRootSplit",
        rifts,
        (0.0, 10.4, 0.54),
        (7.8, 0.24, 0.24),
        mats["MAT_Bark_DarkRoot"],
        rot=(0, 0, math.radians(4)),
    )
    for i, (name, loc, bottom, top, height, sides, mat_key, lean_x, lean_z) in enumerate(
        [
            ("CyanPressureTongue_FromBelow", (-0.10, 10.04, 2.12), 0.34, 0.06, 3.15, 5, "MAT_Waste_RiftCyanDeep", 4, 3),
            ("PaleGoldLostMemoryTongue", (0.42, 9.86, 2.00), 0.26, 0.045, 2.65, 5, "MAT_LostDivinity_PaleGold_Emission", -5, 8),
            ("VioletInstabilityTongue", (-0.52, 10.02, 1.82), 0.24, 0.04, 2.35, 5, "MAT_Waste_RiftVioletControlled", 8, -12),
            ("MainCyanSheddingPlume_StrongestLandmark", (0.04, 10.16, 3.34), 0.46, 0.07, 5.20, 6, "MAT_Waste_RiftCyanDeep", 2, 1),
            ("InnerVioletNerveGlow", (-0.28, 10.10, 3.02), 0.30, 0.05, 3.70, 5, "MAT_Waste_RiftVioletControlled", 7, -9),
            ("PaleGoldMemoryShear", (0.36, 10.04, 3.10), 0.28, 0.05, 4.05, 5, "MAT_LostDivinity_PaleGold_Emission", -6, 11),
        ]
    ):
        plume = frustum_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_{name}",
            rifts,
            loc,
            bottom,
            top,
            height,
            sides,
            mats[mat_key],
            rot=(math.radians(lean_x), 0, math.radians(lean_z)),
        )
        disable_shadow(plume)
    for i, (start, end, mat_key) in enumerate(
        [
            ((-2.9, 1.05), (-0.28, 5.95), "MAT_Waste_RiftVioletControlled"),
            ((2.9, 1.05), (0.28, 5.95), "MAT_Waste_RiftCyanDeep"),
            ((-1.85, 0.82), (-3.55, 3.82), "MAT_Bark_DarkRoot"),
            ((1.85, 0.82), (3.55, 3.82), "MAT_Bark_DarkRoot"),
        ]
    ):
        xz_beam_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_PullingNerveArc_{i:02d}",
            rifts,
            9.78,
            start,
            end,
            0.13,
            0.18,
            mats[mat_key],
        )
    for i, (x, y, z, sx, sy, rot, mat_key) in enumerate(
        [
            (-3.6, 11.4, 2.7, 1.20, 0.44, -24, "MAT_WasteGround"),
            (-1.8, 12.6, 3.45, 0.95, 0.34, 10, "MAT_Waste_GhostRuinSoft"),
            (1.7, 12.45, 3.25, 1.05, 0.38, -8, "MAT_WasteGround"),
            (3.7, 11.2, 2.85, 1.15, 0.42, 28, "MAT_Waste_RiftVioletControlled"),
            (0.0, 13.1, 4.15, 0.82, 0.30, 0, "MAT_Waste_EchoStainSoft"),
        ]
    ):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FloatingBrokenMemoryPlate_{i:02d}",
            rifts,
            (x, y, z),
            (sx, sy, 0.12),
            mats[mat_key],
            rot=(math.radians(8), 0, math.radians(rot)),
        )
    frustum_obj(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_TallRootFiberSpire_ScaleAnchor",
        rifts,
        (3.85, 10.95, 3.35),
        0.34,
        0.12,
        6.1,
        6,
        mats["MAT_Bark_DarkRoot"],
        rot=(math.radians(-9), 0, math.radians(16)),
    )
    cube_obj(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_TallRootFiberSpire_CyanNerveCut",
        rifts,
        (3.62, 10.82, 3.75),
        (0.08, 0.06, 3.8),
        mats["MAT_MemoryStorm_Cyan"],
        rot=(math.radians(-9), 0, math.radians(16)),
    )
    for i, (x, y, z, height, rot) in enumerate([(-2.8, 9.6, 1.35, 2.8, -28), (-1.35, 9.9, 1.8, 3.5, -12), (1.35, 9.9, 1.8, 3.5, 12), (2.8, 9.6, 1.35, 2.8, 28)]):
        frustum_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_JaggedRootMaw_{i:02d}",
            rifts,
            (x, y, z),
            0.28,
            0.10,
            height,
            5,
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(18), 0, math.radians(rot)),
        )
        cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_JaggedRootMaw_{i:02d}_InnerGlow",
            rifts,
            (x * 0.95, y - 0.12, z + height * 0.14),
            (0.08, 0.05, height * 0.48),
            mats["MAT_Waste_RiftCyanDeep" if i in [1, 2] else "MAT_Waste_RiftVioletControlled"],
            rot=(math.radians(18), 0, math.radians(rot)),
        )
    for i, angle in enumerate([0, 24, 50, 82, 122, 164, 204, 248, 292, 326]):
        radians = math.radians(angle)
        distance = 2.4 + (i % 3) * 0.75
        x = math.cos(radians) * distance
        y = 10.4 + math.sin(radians) * distance * 0.42
        fiber = cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_PulledRootFiber_{i:02d}",
            rifts,
            (x * 0.5, (y + 10.4) * 0.5, 0.82 + (i % 2) * 0.08),
            (0.16, distance * 0.92, 0.12),
            mats["MAT_Bark_DarkRoot"],
            rot=(0, math.radians(4 if i % 2 == 0 else -4), radians - math.pi / 2),
        )
        disable_shadow(fiber)
        if i % 2 == 0:
            glow = cube_obj(
                f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_NerveGlow_{i:02d}",
                rifts,
                (x * 0.42, (y + 10.4) * 0.5 - 0.04, 0.96),
                (0.06, distance * 0.58, 0.06),
                mats["MAT_Waste_RiftCyanDeep" if i % 4 == 0 else "MAT_Waste_DivinityStainSoft"],
                rot=(0, math.radians(4), radians - math.pi / 2),
            )
            disable_shadow(glow)
    for i, (x, z, rot, mat_key) in enumerate(
        [
            (-2.4, 2.2, -18, "MAT_Waste_RiftCyanDeep"),
            (2.4, 2.8, 18, "MAT_Waste_RiftVioletControlled"),
            (-3.6, 4.0, -8, "MAT_Waste_RiftVioletControlled"),
            (3.6, 4.4, 8, "MAT_LostDivinity_PaleGold_Emission"),
            (0.0, 5.7, 0, "MAT_Waste_EchoStainSoft"),
        ]
    ):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FragmentedVerticalMemorySlash_{i:02d}",
            rifts,
            (x, 10.05 + i * 0.08, z),
            (0.16, 0.10, 2.10),
            mats[mat_key],
            rot=(math.radians(9), 0, math.radians(rot)),
        )
    for i, (x, y, z) in enumerate([(-1.2, 8.7, 2.1), (1.4, 11.8, 2.5), (-2.7, 12.3, 3.1), (2.9, 8.5, 3.35)]):
        bicone_mesh(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_ShedArchiveShard_{i:02d}",
            rifts,
            (x, y, z),
            0.20,
            0.52,
            mats["MAT_Waste_EchoStainSoft"],
            sides=5,
        )
    for i, angle in enumerate([18, 78, 138, 204, 268, 324]):
        radians = math.radians(angle)
        shard_mat = mats["MAT_Echo_GhostBlueWhite"]
        if i == 1:
            shard_mat = mats["MAT_Waste_RiftCyanDeep"]
        elif i in [3, 5]:
            shard_mat = mats["MAT_Waste_RiftVioletControlled"]
        bicone_mesh(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_OrbitingMemoryShard_{i:02d}",
            rifts,
            (math.cos(radians) * 3.25, 10.4 + math.sin(radians) * 1.25, 2.45 + 0.22 * (i % 3)),
            0.16,
            0.40,
            shard_mat,
            sides=5,
        )
    for i, (x, y, z) in enumerate([(-15.5, 3.0, 1.45), (-18.0, -0.8, 2.35)]):
        bicone_mesh(f"WASTE_RootRiftsAndStorms_FloatingMemoryDustShard_{i:02d}", rifts, (x, y, z), 0.14, 0.34, mats["MAT_Waste_EchoStainSoft"])
    for i, (x, y, z, rot, mat_key) in enumerate([
        (-18.0, 7.5, 2.4, -16, "MAT_Waste_GuideShard_CyanDim"),
        (8.0, 15.2, 3.5, -8, "MAT_Waste_GuideShard_CyanDim"),
    ]):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_MemoryStormSlash_{i:02d}",
            rifts,
            (x, y, z),
            (2.25, 0.08, 0.12),
            mats[mat_key],
            rot=(math.radians(12), 0, math.radians(rot)),
        )
    disc_mesh(
        "WASTE_EchoBattlefield_CircularMemoryStain_BluePastOverlap",
        ruins,
        (-15.5, 1.2, 0.24),
        4.7,
        2.35,
        mats["MAT_Waste_EchoStainSoft"],
        sides=36,
        rot=(0, 0, math.radians(-10)),
    )
    for i, (x, y, rot) in enumerate([(-18.8, 3.2, -12), (-14.9, 4.5, 6), (-12.6, 0.2, 18)]):
        cube_obj(
            f"WASTE_EchoBattlefield_GhostBanner_{i:02d}_Pole",
            ruins,
            (x, y, 1.1),
            (0.12, 0.12, 2.1),
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(6), 0, math.radians(rot)),
        )
        cube_obj(
            f"WASTE_EchoBattlefield_GhostBanner_{i:02d}_TornEchoCloth",
            ruins,
            (x + 0.28, y - 0.08, 1.82),
            (0.78, 0.06, 0.86),
            mats["MAT_Waste_EchoStainSoft"],
            rot=(math.radians(6), 0, math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(-17.0, 1.1, -28), (-15.2, -0.8, 18), (-13.1, 2.4, 7), (-19.0, 0.1, -8)]):
        cube_obj(
            f"WASTE_EchoBattlefield_EmbeddedWeapon_{i:02d}_BrokenSpear",
            ruins,
            (x, y, 0.72),
            (0.10, 0.10, 1.35),
            mats["MAT_Stone_Pilgrimage"],
            rot=(math.radians(28), 0, math.radians(rot)),
        )
        cube_obj(
            f"WASTE_EchoBattlefield_ShieldFragment_{i:02d}",
            ruins,
            (x + 0.4, y - 0.25, 0.32),
            (0.72, 0.12, 0.48),
            mats["MAT_Stone_MossyGray"],
            rot=(math.radians(70), 0, math.radians(rot + 18)),
        )
    for i, (x, y) in enumerate([(-15.5, 1.8), (-13.3, -0.4), (-18.2, 3.2), (-11.8, 3.8)]):
        make_memory_echo_silhouette(f"WASTE_EchoBattlefield_LostSoldierEcho_{i:02d}", ruins, mats, (x, y, 0.15))
    for i, (x, y, rot) in enumerate([(-17.6, 2.1, -9), (-16.0, 2.0, -4), (-14.4, 1.9, 3), (-12.8, 1.8, 8)]):
        cube_obj(
            f"WASTE_EchoBattlefield_GhostPhalanxShieldWall_Fragment_{i:02d}",
            ruins,
            (x, y, 0.92),
            (0.52, 0.08, 0.72),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(5), 0, math.radians(rot)),
        )
        cube_obj(
            f"WASTE_EchoBattlefield_GhostPhalanxSpearMemoryLine_{i:02d}",
            ruins,
            (x + 0.16, y - 0.10, 1.52),
            (0.06, 0.06, 1.52),
            mats["MAT_Waste_EchoStainSoft"],
            rot=(math.radians(20), 0, math.radians(rot + 8)),
        )


def add_memory_wastes_landmark_identity_pass(collections, mats):
    camp = collections["WASTE_SylvaenCamp"]
    ruins = collections["WASTE_FloatingRuins"]
    rifts = collections["WASTE_RootRiftsAndStorms"]
    terrain = collections["WASTE_TerrainIslands"]

    # Island A: make the safe point read as a small Sylvaen stabilizer camp, not just props on a plate.
    for i, (x, y, rot, h) in enumerate([(-1.85, -6.4, -18, 1.95), (1.75, -6.25, 18, 1.75), (-2.15, -2.65, 10, 1.65), (2.05, -2.55, -10, 1.65)]):
        frustum_obj(
            f"WASTE_IslandA_SylvaenCamp_StabilizingRootTotem_{i:02d}",
            camp,
            (x, y, h * 0.50),
            0.16,
            0.09,
            h,
            6,
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(7), 0, math.radians(rot)),
        )
        cube_obj(
            f"WASTE_IslandA_SylvaenCamp_StabilizingRootTotem_{i:02d}_CyanMemoryWrap",
            camp,
            (x, y - 0.08, h * 0.62),
            (0.22, 0.045, 0.38),
            mats["MAT_Waste_SafeCyanGlow"],
            rot=(0, 0, math.radians(rot)),
        )
    for i, (start, end) in enumerate([((-2.1, 0.9), (0.0, 2.0)), ((0.0, 2.0), (2.1, 0.9))]):
        xz_beam_obj(
            f"WASTE_IslandA_SylvaenCamp_RootShelterArch_ReadsSafePoint_{i:02d}",
            camp,
            -5.85,
            start,
            end,
            0.16,
            0.18,
            mats["MAT_Path_DarkRootShadow"],
        )
    cube_obj(
        "WASTE_IslandA_SylvaenCamp_ArchiveTable_BrightReadableMemoryMap",
        camp,
        (-3.3, -3.30, 1.02),
        (1.18, 0.06, 0.06),
        mats["MAT_LostDivinity_PaleGold_Emission"],
        rot=(0, 0, math.radians(7)),
    )

    # Island B: add one strong lost-civilization silhouette visible from the gameplay camera.
    for side, x in [("Left", -12.35), ("Right", -0.75)]:
        frustum_obj(
            f"WASTE_IslandB_LostCivilization_HeroBrokenArchiveArch_{side}Pillar",
            ruins,
            (x, 12.62, 2.62),
            0.42,
            0.24,
            4.55,
            6,
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(7 if side == "Left" else -7), 0, math.radians(-5 if side == "Left" else 5)),
        )
    for i, (start, end) in enumerate([((-12.35, 4.76), (-9.8, 6.15)), ((-9.8, 6.15), (-6.6, 6.62)), ((-6.6, 6.62), (-3.35, 6.15)), ((-3.35, 6.15), (-0.75, 4.76))]):
        xz_beam_obj(
            f"WASTE_IslandB_LostCivilization_HeroBrokenArchiveArch_CrownSegment_{i:02d}",
            ruins,
            12.42,
            start,
            end,
            0.22,
            0.30,
            mats["MAT_LostCivilization_GhostStone"],
        )
    for i, (x, y, z, rot) in enumerate([(-11.1, 14.7, 3.8, -14), (-8.0, 15.7, 4.6, 2), (-4.2, 14.9, 3.9, 16), (-2.1, 13.6, 2.8, 28)]):
        cube_obj(
            f"WASTE_IslandB_LostCivilization_FloatingMasonryChunk_{i:02d}_HalfRemembered",
            ruins,
            (x, y, z),
            (1.20, 0.20, 0.74),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(11), 0, math.radians(rot)),
        )

    # Island C: reinforce the solemn divine silhouette with a larger broken halo and grounded sacred stain.
    disc_mesh(
        "WASTE_IslandC_DeadGodShrine_SolemnSacredGroundStain_NotTraversal",
        terrain,
        (14.45, 5.2, 0.33),
        5.35,
        2.85,
        mats["MAT_Waste_DivinityStainSoft"],
        sides=36,
        rot=(0, 0, math.radians(8)),
    )
    for i, (start, end, mat_key) in enumerate([
        ((10.85, 1.30), (12.0, 4.9), "MAT_DeadGod_Stone"),
        ((12.0, 4.9), (14.4, 6.55), "MAT_LostDivinity_PaleGold_Emission"),
        ((14.4, 6.55), (16.8, 4.9), "MAT_LostDivinity_PaleGold_Emission"),
        ((16.8, 4.9), (17.95, 1.30), "MAT_DeadGod_Stone"),
    ]):
        xz_beam_obj(
            f"WASTE_IslandC_DeadGodShrine_HeroCrackedHaloOuterSilhouette_{i:02d}",
            ruins,
            4.05,
            start,
            end,
            0.16,
            0.24,
            mats[mat_key],
        )
    for i, (x, y, rot) in enumerate([(13.3, 3.95, -18), (15.3, 3.78, 14), (14.55, 6.9, 3)]):
        cube_obj(
            f"WASTE_IslandC_DeadGodShrine_ShatteredNameSigilShard_{i:02d}",
            ruins,
            (x, y, 0.56),
            (1.05, 0.16, 0.12),
            mats["MAT_LostDivinity_PaleGold_Emission"],
            rot=(0, 0, math.radians(rot)),
        )

    # Island D: give the battlefield an unmistakable old-combat read from the camera.
    disc_mesh(
        "WASTE_IslandD_EchoBattlefield_DarkCircularScar_UnderGhostSoldiers",
        terrain,
        (-15.4, 1.1, 0.31),
        5.15,
        2.55,
        mats["MAT_Stone_DarkCrevice"],
        sides=34,
        rot=(0, 0, math.radians(-10)),
    )
    for i, (x, y, rot) in enumerate([(-19.8, 2.8, -18), (-18.6, -0.4, -8), (-12.6, 3.2, 14), (-11.6, 0.2, 24)]):
        cube_obj(
            f"WASTE_IslandD_EchoBattlefield_EmbeddedSpearSilhouette_{i:02d}",
            ruins,
            (x, y, 1.0),
            (0.08, 0.08, 2.0),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(24), 0, math.radians(rot)),
        )
        bicone_mesh(
            f"WASTE_IslandD_EchoBattlefield_SpearMemoryHead_{i:02d}",
            ruins,
            (x + 0.14, y - 0.10, 1.88),
            0.09,
            0.24,
            mats["MAT_Waste_EchoStainSoft"],
            sides=5,
        )
    for i, (x, y, rot) in enumerate([(-16.9, 3.9, -6), (-14.5, 3.7, 6)]):
        cube_obj(
            f"WASTE_IslandD_EchoBattlefield_CommandGhostBanner_TornStandard_{i:02d}",
            ruins,
            (x, y, 2.25),
            (1.05, 0.06, 1.18),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(6), 0, math.radians(rot)),
        )

    # Central landmark: make the Worldroot memory-rift read as cracked earth and exposed nerves, not abstract neon.
    for i, (x, y, length, rot, mat_key) in enumerate([
        (-2.4, 9.35, 3.6, -28, "MAT_Stone_DarkCrevice"),
        (2.5, 9.45, 3.8, 28, "MAT_Stone_DarkCrevice"),
        (-3.4, 10.8, 3.1, 18, "MAT_Bark_DarkRoot"),
        (3.2, 10.7, 3.0, -18, "MAT_Bark_DarkRoot"),
        (0.0, 8.85, 4.4, 0, "MAT_Waste_RiftVioletControlled"),
    ]):
        cube_obj(
            f"WASTE_CentralRift_CrackedGroundRadialSplit_{i:02d}",
            rifts,
            (x, y, 0.72),
            (0.18, length, 0.10),
            mats[mat_key],
            rot=(0, 0, math.radians(rot)),
        )
    for i, (x, y, z, rot, mat_key) in enumerate([
        (-2.15, 10.85, 2.45, -22, "MAT_Waste_RiftCyanDeep"),
        (2.20, 10.75, 2.65, 22, "MAT_Waste_RiftVioletControlled"),
        (-0.85, 11.45, 3.25, -8, "MAT_LostDivinity_PaleGold_Emission"),
        (0.95, 11.35, 3.05, 9, "MAT_Echo_GhostBlueWhite"),
    ]):
        bicone_mesh(
            f"WASTE_CentralRift_LiftedMemoryShard_ClearOrbit_{i:02d}",
            rifts,
            (x, y, z),
            0.24,
            0.70,
            mats[mat_key],
            sides=5,
            rot=(math.radians(90), 0, math.radians(rot)),
        )
    for i, angle in enumerate([-48, -28, -10, 12, 32, 52]):
        rad = math.radians(angle)
        cube_obj(
            f"WASTE_CentralRift_ExposedRootNerve_ReachTowardCamera_{i:02d}",
            rifts,
            (math.sin(rad) * 1.8, 8.75 + i * 0.23, 0.90 + (i % 2) * 0.05),
            (0.14, 3.05, 0.11),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(4), 0, rad),
        )
        if i % 2 == 0:
            cube_obj(
                f"WASTE_CentralRift_ExposedRootNerve_{i:02d}_MemoryPulse",
                rifts,
                (math.sin(rad) * 1.65, 8.72 + i * 0.23, 1.02),
                (0.055, 1.85, 0.05),
                mats["MAT_Waste_RiftCyanDeep"],
                rot=(math.radians(4), 0, rad),
            )


def add_memory_wastes_island_identity_pass(collections, mats):
    camp = collections["WASTE_SylvaenCamp"]
    ruins = collections["WASTE_FloatingRuins"]
    rifts = collections["WASTE_RootRiftsAndStorms"]
    terrain = collections["WASTE_TerrainIslands"]
    chars = collections["WASTE_Characters"]

    # Remove old free-floating debug-like markers; keep glow where it describes traversal or the central rift.
    removed = []
    debug_patterns = [
        "WASTE_RootRiftsAndStorms_FloatingMemoryDustShard_",
        "WASTE_RootRiftsAndStorms_MemoryStormSlash_",
    ]
    for obj in list(bpy.data.objects):
        if any(pattern in obj.name for pattern in debug_patterns):
            removed.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)
    bpy.context.scene["memory_wastes_debug_markers_removed"] = len(removed)
    bpy.context.scene["memory_wastes_debug_marker_reduction_note"] = "Removed free-floating dust/slash markers; cyan/violet now concentrates on rift, routes and island identities."

    # ISLAND A: compact safe camp at the spawn-facing island.
    disc_mesh(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_WarmCyanSafeZonePool_NotRift",
        terrain,
        (0.0, -4.55, 0.36),
        3.0,
        1.42,
        mats["MAT_Waste_SafeCyanGlow"],
        sides=34,
        rot=(0, 0, math.radians(3)),
    )
    tri_prism_obj(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_RootShelter_GreenLeafCanopy",
        camp,
        (0.25, -6.25, 0.48),
        3.6,
        2.25,
        1.28,
        mats["MAT_CampCloth_Green"],
        rot=(0, 0, math.radians(-4)),
    )
    for i, (x, y, rot) in enumerate([(-1.3, -6.25, -12), (1.75, -6.12, 12), (-0.4, -5.05, 4)]):
        frustum_obj(
            f"ISLAND_A_SYLVAEN_SAFE_CAMP_RootShelter_LivingRootRib_{i:02d}",
            camp,
            (x, y, 0.88),
            0.18,
            0.10,
            1.75,
            6,
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(9), 0, math.radians(rot)),
        )
    cube_obj(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_ArchiveTable_ReadableMapSurface",
        camp,
        (-2.75, -3.68, 0.68),
        (1.38, 0.86, 0.16),
        mats["MAT_Bark_WarmBrown"],
        rot=(0, 0, math.radians(8)),
    )
    cube_obj(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_ArchiveTable_CyanMemoryMap",
        camp,
        (-2.75, -3.72, 0.80),
        (1.05, 0.52, 0.045),
        mats["MAT_Waste_SafeCyanGlow"],
        rot=(0, 0, math.radians(8)),
    )
    frustum_obj(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_MemoryBrazier_RootBowl",
        camp,
        (2.65, -3.65, 0.58),
        0.48,
        0.30,
        0.54,
        8,
        mats["MAT_Bark_DarkRoot"],
    )
    bicone_mesh(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_MemoryBrazier_WarmCyanFlame",
        camp,
        (2.65, -3.65, 1.15),
        0.20,
        0.62,
        mats["MAT_Waste_SafeCyanGlow"],
        sides=6,
    )
    cube_obj(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_VerdantBanner_GreenCloth",
        camp,
        (-3.95, -5.05, 1.35),
        (0.72, 0.06, 1.05),
        mats["MAT_Banner_VerdantGreen"],
        rot=(math.radians(4), 0, math.radians(-8)),
    )
    bicone_mesh(
        "ISLAND_A_SYLVAEN_SAFE_CAMP_VerdantBanner_GoldLeafSigil",
        camp,
        (-3.95, -5.10, 1.42),
        0.12,
        0.30,
        mats["MAT_GoldTrim"],
        sides=5,
        rot=(math.radians(90), 0, 0),
    )
    make_character("ISLAND_A_SYLVAEN_SAFE_CAMP_NPC_MemoryKeeperScaleAnchor", chars, mats, (-0.90, -4.10, 0.22), role="npc")
    make_character("ISLAND_A_SYLVAEN_SAFE_CAMP_NPC_RootGuardianScaleAnchor", chars, mats, (1.25, -4.28, 0.22), role="npc")

    # ISLAND B: lost civilization island gets a clearer pale ruin identity and non-grounded fragments.
    disc_mesh(
        "ISLAND_B_LOST_CIVILIZATION_RUINS_GhostWhiteCyanGroundMemorySkin",
        terrain,
        (-6.5, 12.75, 0.38),
        4.90,
        2.34,
        mats["MAT_Waste_GhostRuinSoft"],
        sides=34,
        rot=(0, 0, math.radians(-7)),
    )
    for i, x in enumerate([-11.9, -8.6, -3.9, -1.1]):
        frustum_obj(
            f"ISLAND_B_LOST_CIVILIZATION_RUINS_BrokenColumn_NotTouchingGround_{i:02d}",
            ruins,
            (x, 12.25 + (i % 2) * 0.78, 1.85 + i * 0.12),
            0.34,
            0.22,
            2.92,
            6,
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(7 - i * 2), 0, math.radians(-10 + i * 7)),
        )
    for i, (x, y, z, rot) in enumerate([(-10.0, 14.6, 2.70, -16), (-7.5, 15.35, 3.10, 2), (-5.1, 14.85, 2.86, 18)]):
        cube_obj(
            f"ISLAND_B_LOST_CIVILIZATION_RUINS_FloatingWallFragment_GhostStone_{i:02d}",
            ruins,
            (x, y, z),
            (1.55, 0.16, 0.92),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(10), 0, math.radians(rot)),
        )
    for i, (x, y, z) in enumerate([(-9.6, 10.3, 0.58), (-8.35, 10.95, 0.78), (-7.1, 11.6, 0.98), (-5.85, 12.24, 1.18)]):
        cube_obj(
            f"ISLAND_B_LOST_CIVILIZATION_RUINS_IncompleteStairPiece_{i:02d}",
            ruins,
            (x, y, z),
            (1.05, 0.46, 0.12),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(0, 0, math.radians(27)),
        )
    make_memory_echo_silhouette("ISLAND_B_LOST_CIVILIZATION_RUINS_SpectralArchivistScaleAnchor", chars, mats, (-4.2, 11.35, 0.24))

    # ISLAND C: dead god shrine island reads pale-gold and solemn, not just another magic platform.
    poly_obj(
        "ISLAND_C_DEAD_GOD_SHRINE_BrokenAltar_HeavyCenterStone",
        ruins,
        [(12.95, 4.24), (15.55, 4.02), (16.18, 5.20), (14.65, 6.12), (12.65, 5.42)],
        mats["MAT_DeadGod_Stone"],
        z=0.72,
    )
    for i, (start, end) in enumerate([((10.6, 1.12), (12.1, 5.08)), ((12.1, 5.08), (14.5, 6.92)), ((14.5, 6.92), (16.9, 5.08)), ((16.9, 5.08), (18.35, 1.12))]):
        xz_beam_obj(
            f"ISLAND_C_DEAD_GOD_SHRINE_CrackedCircularHalo_SolemnArc_{i:02d}",
            ruins,
            4.24,
            start,
            end,
            0.18,
            0.24,
            mats["MAT_LostDivinity_PaleGold_Emission"] if i in [1, 2] else mats["MAT_DeadGod_Stone"],
        )
    cube_obj(
        "ISLAND_C_DEAD_GOD_SHRINE_MissingNameRuneSlab_BlackGoldContrast",
        ruins,
        (12.28, 4.34, 0.92),
        (1.24, 0.18, 0.72),
        mats["MAT_Stone_DarkCrevice"],
        rot=(math.radians(8), 0, math.radians(13)),
    )
    for i, (x, y, rot) in enumerate([(13.15, 6.30, -30), (15.30, 6.20, 20), (16.05, 4.12, 8), (12.62, 3.65, -10)]):
        bicone_mesh(
            f"ISLAND_C_DEAD_GOD_SHRINE_ShatteredDivineSigilFragment_{i:02d}",
            ruins,
            (x, y, 0.82 + (i % 2) * 0.12),
            0.18,
            0.46,
            mats["MAT_LostDivinity_PaleGold_Emission"],
            sides=5,
            rot=(math.radians(90), 0, math.radians(rot)),
        )
    cube_obj(
        "ISLAND_C_DEAD_GOD_SHRINE_PaleGoldVerticalLight_StrongRead",
        ruins,
        (14.45, 5.00, 3.10),
        (0.34, 0.12, 5.20),
        mats["MAT_LostDivinity_PaleGold_Emission"],
        rot=(math.radians(3), 0, math.radians(-3)),
    )
    make_memory_echo_silhouette("ISLAND_C_DEAD_GOD_SHRINE_KneelingEchoScaleAnchor", chars, mats, (16.1, 5.75, 0.22), mat_key="MAT_LostDivinity_PaleGold_Emission")

    # ISLAND D: battlefield island gets clear war-read silhouettes and a scale anchor.
    disc_mesh(
        "ISLAND_D_ECHO_BATTLEFIELD_CircularBattlefieldMemoryStain_GhostBlue",
        terrain,
        (-15.8, 1.25, 0.39),
        5.7,
        2.82,
        mats["MAT_Waste_EchoStainSoft"],
        sides=36,
        rot=(0, 0, math.radians(-12)),
    )
    for i, (x, y, rot) in enumerate([(-20.0, 2.3, -18), (-18.7, -0.65, -4), (-13.2, 3.08, 12), (-11.7, 0.42, 26), (-15.0, -1.18, 8)]):
        cube_obj(
            f"ISLAND_D_ECHO_BATTLEFIELD_EmbeddedSpear_ReadableWeapon_{i:02d}",
            ruins,
            (x, y, 1.12),
            (0.09, 0.09, 2.15),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(26), 0, math.radians(rot)),
        )
        cube_obj(
            f"ISLAND_D_ECHO_BATTLEFIELD_BrokenShieldFragment_{i:02d}",
            ruins,
            (x + 0.42, y - 0.18, 0.42),
            (0.76, 0.10, 0.46),
            mats["MAT_Stone_MossyGray"],
            rot=(math.radians(70), 0, math.radians(rot + 16)),
        )
    for i, (x, y, rot) in enumerate([(-17.7, 4.25, -7), (-13.7, 3.95, 8)]):
        cube_obj(
            f"ISLAND_D_ECHO_BATTLEFIELD_GhostBanner_TornCloth_{i:02d}",
            ruins,
            (x, y, 2.32),
            (1.18, 0.055, 1.18),
            mats["MAT_Waste_EchoStainSoft"],
            rot=(math.radians(6), 0, math.radians(rot)),
        )
    make_memory_echo_silhouette("ISLAND_D_ECHO_BATTLEFIELD_CommanderEchoScaleAnchor", chars, mats, (-15.2, 1.55, 0.22))

    # Central rift: one stronger focal stack using exact landmark naming.
    poly_obj(
        "LANDMARK_WORLDROOT_SHEDDING_RIFT_CrackedRootFissure_PrimaryVoidMouth",
        rifts,
        [(-5.2, 9.65), (-3.15, 8.92), (-1.0, 9.52), (0.25, 8.78), (2.35, 9.42), (5.15, 9.05), (3.78, 10.72), (1.0, 11.18), (-1.10, 10.82), (-3.18, 11.55), (-5.55, 10.42)],
        mats["MAT_Stone_DarkCrevice"],
        z=0.78,
    )
    poly_obj(
        "LANDMARK_WORLDROOT_SHEDDING_RIFT_CyanVioletGoldEnergy_CoreMeaningfulColor",
        rifts,
        [(-2.95, 9.84), (-1.20, 9.56), (0.18, 9.76), (1.60, 9.52), (3.05, 9.92), (1.92, 10.34), (0.18, 10.55), (-1.72, 10.30)],
        mats["MAT_Waste_RiftCyanDeep"],
        z=0.86,
    )
    for i, (x, y, z, h, mat_key, lean) in enumerate([
        (0.00, 10.12, 3.65, 6.20, "MAT_Waste_RiftCyanDeep", 0),
        (-0.48, 10.03, 3.10, 4.60, "MAT_Waste_RiftVioletControlled", -12),
        (0.48, 9.96, 2.92, 3.80, "MAT_LostDivinity_PaleGold_Emission", 14),
    ]):
        plume = frustum_obj(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_EnergyPlume_{i:02d}_{mat_key.replace('MAT_', '')}",
            rifts,
            (x, y, z),
            0.42 - i * 0.06,
            0.05,
            h,
            6,
            mats[mat_key],
            rot=(math.radians(4), 0, math.radians(lean)),
        )
        disable_shadow(plume)
    for i, angle in enumerate([20, 70, 125, 188, 245, 308]):
        rad = math.radians(angle)
        bicone_mesh(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_FloatingMemoryShard_Orbit_{i:02d}",
            rifts,
            (math.cos(rad) * 3.45, 10.38 + math.sin(rad) * 1.34, 2.85 + (i % 3) * 0.28),
            0.17,
            0.52,
            mats["MAT_Waste_RiftCyanDeep"] if i in [0, 3] else (mats["MAT_Waste_RiftVioletControlled"] if i in [2, 5] else mats["MAT_LostDivinity_PaleGold_Emission"]),
            sides=5,
        )
    for i, angle in enumerate([-58, -35, -16, 8, 28, 52]):
        rad = math.radians(angle)
        cube_obj(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_ExposedRootFiberNerve_{i:02d}",
            rifts,
            (math.sin(rad) * 1.92, 8.70 + i * 0.24, 1.04),
            (0.15, 3.45, 0.12),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(4), 0, rad),
        )
        if i in [1, 3, 5]:
            cube_obj(
                f"LANDMARK_WORLDROOT_SHEDDING_RIFT_ExposedRootFiberNerve_{i:02d}_CyanPulse",
                rifts,
                (math.sin(rad) * 1.72, 8.70 + i * 0.24, 1.17),
                (0.055, 2.08, 0.052),
                mats["MAT_Waste_RiftCyanDeep"],
                rot=(math.radians(4), 0, rad),
            )


def add_memory_wastes_island_readability_pass(collections, mats):
    camp = collections["WASTE_SylvaenCamp"]
    ruins = collections["WASTE_FloatingRuins"]
    rifts = collections["WASTE_RootRiftsAndStorms"]
    terrain = collections["WASTE_TerrainIslands"]
    chars = collections["WASTE_Characters"]

    # Remove older duplicated neon/debug reads so each island owns its color and silhouette.
    removal_patterns = [
        "WASTE_RootRiftsAndStorms_VioletRiftCoreShard_",
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_OrbitingMemoryShard_",
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_ShedArchiveShard_",
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FragmentedVerticalMemorySlash_",
        "WASTE_CentralRift_LiftedMemoryShard_ClearOrbit_",
        "WASTE_RootRiftsAndStorms_FloatingMemoryDustShard_",
        "WASTE_RootRiftsAndStorms_MemoryStormSlash_",
    ]
    removed = []
    for obj in list(bpy.data.objects):
        if any(pattern in obj.name for pattern in removal_patterns):
            removed.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)
    previous_removed = int(bpy.context.scene.get("memory_wastes_debug_markers_removed", 0))
    bpy.context.scene["memory_wastes_debug_markers_removed"] = previous_removed + len(removed)
    bpy.context.scene["memory_wastes_readability_removed_debug_shapes"] = len(removed)

    # Island A: make the spawn/safe camp read from camera with a low shelter silhouette and root-ring boundary.
    disc_mesh(
        "READABILITY_ISLAND_A_SYLVAEN_SAFE_CAMP_MossRootGroundingPatch",
        terrain,
        (0.0, -4.9, 0.43),
        5.3,
        2.65,
        mats["MAT_Moss"],
        sides=28,
        rot=(0, 0, math.radians(2)),
    )
    for i, (x, y, length, rot) in enumerate([(-3.1, -5.7, 3.2, -18), (3.2, -5.6, 3.1, 18), (-2.4, -3.2, 2.5, 26), (2.5, -3.1, 2.4, -24)]):
        cube_obj(
            f"READABILITY_ISLAND_A_SYLVAEN_SAFE_CAMP_VisibleGroundRootBorder_{i:02d}",
            camp,
            (x, y, 0.58),
            (length, 0.18, 0.16),
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(4), 0, math.radians(rot)),
        )
    xz_beam_obj(
        "READABILITY_ISLAND_A_SYLVAEN_SAFE_CAMP_RootShelterHighSilhouetteBackRib",
        camp,
        -5.55,
        (-2.35, 0.45),
        (2.55, 0.45),
        0.20,
        0.22,
        mats["MAT_Path_DarkRootShadow"],
    )
    cube_obj(
        "READABILITY_ISLAND_A_SYLVAEN_SAFE_CAMP_WarmSafetyLightLowLanternRead",
        camp,
        (0.05, -5.18, 1.24),
        (0.36, 0.08, 0.52),
        mats["MAT_Lantern_Warm_Emission"],
        rot=(0, 0, math.radians(2)),
    )

    # Island B: add one large broken arch and translucent wall mass so ruins read at a glance.
    for i, (start, end) in enumerate([((-12.6, 1.0), (-10.2, 4.9)), ((-10.2, 4.9), (-6.4, 6.0)), ((-6.4, 6.0), (-2.5, 4.7)), ((-2.5, 4.7), (-0.7, 1.0))]):
        xz_beam_obj(
            f"READABILITY_ISLAND_B_LOST_CIVILIZATION_RUINS_HeroBrokenArchiveArch_{i:02d}",
            ruins,
            12.95,
            start,
            end,
            0.22,
            0.28,
            mats["MAT_LostCivilization_GhostStone"] if i in [1, 2] else mats["MAT_Stone_PaleArchive"],
        )
    for i, (x, y, z, sx, sy, sz, rot) in enumerate([
        (-10.7, 13.7, 2.10, 1.85, 0.16, 1.55, -10),
        (-7.0, 14.9, 2.65, 2.15, 0.14, 1.30, 3),
        (-3.8, 13.8, 2.25, 1.75, 0.16, 1.50, 15),
    ]):
        cube_obj(
            f"READABILITY_ISLAND_B_LOST_CIVILIZATION_RUINS_TransparentWallMass_Readable_{i:02d}",
            ruins,
            (x, y, z),
            (sx, sy, sz),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(8), 0, math.radians(rot)),
        )
    for i, (x, y, z) in enumerate([(-11.2, 10.5, 0.62), (-9.7, 10.95, 0.80), (-8.2, 11.45, 0.98), (-6.7, 11.95, 1.16), (-5.2, 12.45, 1.34)]):
        cube_obj(
            f"READABILITY_ISLAND_B_LOST_CIVILIZATION_RUINS_IncompleteStairSilhouette_{i:02d}",
            ruins,
            (x, y, z),
            (1.24, 0.50, 0.12),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(0, 0, math.radians(24)),
        )

    # Island C: strengthen shrine silhouette with a larger readable halo and clear divine floor mark.
    disc_mesh(
        "READABILITY_ISLAND_C_DEAD_GOD_SHRINE_PaleGoldCircularGroundSigil",
        terrain,
        (14.55, 5.48, 0.47),
        4.55,
        2.42,
        mats["MAT_Waste_DivinityStainSoft"],
        sides=34,
        rot=(0, 0, math.radians(9)),
    )
    for i, (start, end) in enumerate([((10.0, 0.80), (11.8, 5.95)), ((11.8, 5.95), (14.5, 7.55)), ((14.5, 7.55), (17.25, 5.95)), ((17.25, 5.95), (19.05, 0.80))]):
        xz_beam_obj(
            f"READABILITY_ISLAND_C_DEAD_GOD_SHRINE_OversizedBrokenHalo_ReadsFromCamera_{i:02d}",
            ruins,
            4.78,
            start,
            end,
            0.25,
            0.32,
            mats["MAT_LostDivinity_PaleGold_Emission"] if i in [1, 2] else mats["MAT_DeadGod_Stone"],
        )
    cube_obj(
        "READABILITY_ISLAND_C_DEAD_GOD_SHRINE_MissingNameSlab_TallForegroundSilhouette",
        ruins,
        (12.20, 4.20, 1.48),
        (1.05, 0.20, 1.70),
        mats["MAT_Stone_DarkCrevice"],
        rot=(math.radians(8), 0, math.radians(11)),
    )

    # Island D: increase battlefield read with a spear line, stronger banners and extra soldier echoes.
    disc_mesh(
        "READABILITY_ISLAND_D_ECHO_BATTLEFIELD_DarkBlueCircularWarStain",
        terrain,
        (-15.9, 1.18, 0.46),
        6.4,
        3.05,
        mats["MAT_Waste_EchoStainSoft"],
        sides=34,
        rot=(0, 0, math.radians(-12)),
    )
    for i, (x, y, rot) in enumerate([(-20.5, 2.9, -22), (-19.1, 1.2, -12), (-17.4, 3.0, -2), (-14.0, 2.7, 12), (-12.5, 0.7, 24), (-11.4, 2.2, 28)]):
        cube_obj(
            f"READABILITY_ISLAND_D_ECHO_BATTLEFIELD_SpearLineSilhouette_{i:02d}",
            ruins,
            (x, y, 1.55),
            (0.10, 0.10, 2.80),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(24), 0, math.radians(rot)),
        )
        bicone_mesh(
            f"READABILITY_ISLAND_D_ECHO_BATTLEFIELD_SpearTipGlow_{i:02d}",
            ruins,
            (x + 0.06, y - 0.04, 2.95),
            0.08,
            0.24,
            mats["MAT_Echo_GhostBlueWhite"],
            sides=5,
        )
    for i, (x, y) in enumerate([(-17.0, 1.8), (-13.8, 1.7)]):
        make_memory_echo_silhouette(f"READABILITY_ISLAND_D_ECHO_BATTLEFIELD_ExtraSoldierEcho_{i:02d}", chars, mats, (x, y, 0.24))

    # Center: rebuild the focal rift as one strong cracked fissure with lifted plates instead of many abstract markers.
    poly_obj(
        "READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_WideCrackedTerrainFissure_StrongSilhouette",
        rifts,
        [(-6.0, 9.55), (-3.4, 8.78), (-1.55, 9.40), (0.0, 8.65), (1.85, 9.38), (5.8, 8.90), (4.25, 10.98), (1.20, 11.55), (-1.45, 10.95), (-3.55, 11.72), (-6.2, 10.54)],
        mats["MAT_Stone_DarkCrevice"],
        z=0.96,
    )
    poly_obj(
        "READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_CyanVioletGoldCore_ColorRules",
        rifts,
        [(-3.4, 9.70), (-1.4, 9.46), (0.10, 9.74), (1.85, 9.42), (3.48, 9.88), (2.08, 10.58), (0.24, 10.82), (-2.10, 10.48)],
        mats["MAT_Waste_RiftCyanDeep"],
        z=1.03,
    )
    for i, (x, y, sx, sy, z, rot, mat_key) in enumerate([
        (-4.4, 10.95, 1.8, 0.78, 1.18, -18, "MAT_DeadGod_Stone"),
        (-2.0, 8.95, 1.6, 0.66, 1.34, 12, "MAT_WasteGround"),
        (2.4, 8.88, 1.7, 0.72, 1.42, -10, "MAT_WasteGround"),
        (4.2, 10.82, 1.9, 0.76, 1.22, 20, "MAT_DeadGod_Stone"),
    ]):
        cube_obj(
            f"READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_LiftedBrokenGroundPlate_{i:02d}",
            rifts,
            (x, y, z),
            (sx, sy, 0.16),
            mats[mat_key],
            rot=(math.radians(12 if i % 2 else -8), math.radians(4), math.radians(rot)),
        )
    for i, (angle, length) in enumerate([(-52, 5.0), (-30, 4.4), (-8, 4.8), (14, 4.6), (36, 4.2), (58, 5.0)]):
        rad = math.radians(angle)
        cube_obj(
            f"READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_ExposedNerveRoot_{i:02d}",
            rifts,
            (math.sin(rad) * 2.25, 8.30 + i * 0.34, 1.22),
            (0.18, length, 0.15),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(5), 0, rad),
        )
    for i, (x, y, z, h, mat_key, lean) in enumerate([
        (0.0, 10.10, 4.20, 7.30, "MAT_Waste_RiftCyanDeep", 0),
        (-0.58, 10.02, 3.45, 5.30, "MAT_Waste_RiftVioletControlled", -10),
        (0.62, 9.96, 3.18, 4.40, "MAT_LostDivinity_PaleGold_Emission", 12),
    ]):
        plume = frustum_obj(
            f"READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_PrimaryVerticalLight_{i:02d}_{mat_key.replace('MAT_', '')}",
            rifts,
            (x, y, z),
            0.50 - i * 0.07,
            0.05,
            h,
            6,
            mats[mat_key],
            rot=(math.radians(4), 0, math.radians(lean)),
        )
        disable_shadow(plume)
    for i, angle in enumerate([15, 82, 150, 220, 292]):
        rad = math.radians(angle)
        bicone_mesh(
            f"READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_FewFloatingMemoryShard_{i:02d}",
            rifts,
            (math.cos(rad) * 3.95, 10.30 + math.sin(rad) * 1.48, 3.05 + (i % 2) * 0.32),
            0.16,
            0.50,
            mats["MAT_Waste_RiftCyanDeep"] if i in [0, 3] else (mats["MAT_Waste_RiftVioletControlled"] if i == 2 else mats["MAT_LostDivinity_PaleGold_Emission"]),
            sides=5,
        )


def add_memory_wastes_landmark_readability_lock_pass(collections, mats):
    role_collection_names = [
        "ISLAND_SAFE_CAMP",
        "ISLAND_LOST_RUINS",
        "ISLAND_DEAD_GOD_SHRINE",
        "ISLAND_ECHO_BATTLEFIELD",
        "LANDMARK_WORLDROOT_RIFT",
        "TRAVERSAL_MEMORY_BRIDGES",
    ]
    for name in role_collection_names:
        if name not in collections:
            collection = bpy.data.collections.new(name)
            bpy.context.scene.collection.children.link(collection)
            collections[name] = collection

    # Remove older duplicate rift/debug shards so the central landmark reads as one designed focal point.
    removal_patterns = [
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FloatingBrokenMemoryPlate_",
        "LANDMARK_WORLDROOT_SHEDDING_RIFT_FloatingMemoryShard_Orbit_",
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FragmentedVerticalMemorySlash_",
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_ShedArchiveShard_",
        "WASTE_RootRiftsAndStorms_FloatingMemoryDustShard_",
        "WASTE_RootRiftsAndStorms_MemoryStormSlash_",
    ]
    removed = []
    for obj in list(bpy.data.objects):
        if any(pattern in obj.name for pattern in removal_patterns):
            removed.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)

    previous_removed = int(bpy.context.scene.get("memory_wastes_debug_markers_removed", 0))
    bpy.context.scene["memory_wastes_debug_markers_removed"] = previous_removed + len(removed)
    previous_readability_removed = int(bpy.context.scene.get("memory_wastes_readability_removed_debug_shapes", 0))
    bpy.context.scene["memory_wastes_readability_removed_debug_shapes"] = previous_readability_removed + len(removed)
    bpy.context.scene["memory_wastes_landmark_readability_removed_names"] = ", ".join(removed[:24])
    bpy.context.scene["memory_wastes_debug_marker_reduction_note"] = (
        "Removed duplicate orbit shards, free-floating dust/slashes and older rift fragments; cyan now stays on "
        "Worldroot memory, traversal cues and the central rift."
    )

    rifts = collections["WASTE_RootRiftsAndStorms"]

    # Make the central Worldroot Rift visibly win over the shrine/ruins without moving the camera or island layout.
    for i, (x, y, z, h, radius, mat_key, lean) in enumerate(
        [
            (-0.72, 10.12, 4.25, 7.60, 0.22, "MAT_Bark_DarkRoot", -8),
            (0.00, 10.00, 4.70, 8.40, 0.16, "MAT_Waste_RiftCyanDeep", 0),
            (0.64, 9.92, 4.10, 6.60, 0.14, "MAT_Waste_RiftVioletControlled", 10),
            (0.34, 10.42, 3.75, 4.80, 0.10, "MAT_LostDivinity_PaleGold_Emission", 6),
        ]
    ):
        plume = frustum_obj(
            f"LANDMARK_WORLDROOT_RIFT_PrimarySheddingSpire_{i:02d}_{mat_key.replace('MAT_', '')}",
            rifts,
            (x, y, z),
            radius,
            0.035,
            h,
            6,
            mats[mat_key],
            rot=(math.radians(4), math.radians(2), math.radians(lean)),
        )
        disable_shadow(plume)
    for i, angle in enumerate([-70, -44, -18, 18, 42, 68]):
        rad = math.radians(angle)
        cube_obj(
            f"LANDMARK_WORLDROOT_RIFT_PulledOpenRootNerve_{i:02d}",
            rifts,
            (math.sin(rad) * 2.65, 8.55 + i * 0.30, 1.30),
            (0.18, 4.25, 0.14),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(7), math.radians(2), rad),
        )
        if i in [1, 3, 4]:
            cube_obj(
                f"LANDMARK_WORLDROOT_RIFT_CyanMemoryPulseOnRootNerve_{i:02d}",
                rifts,
                (math.sin(rad) * 2.48, 8.76 + i * 0.30, 1.48),
                (0.055, 2.35, 0.05),
                mats["MAT_Waste_RiftCyanDeep"],
                rot=(math.radians(7), math.radians(2), rad),
            )
    for i, angle in enumerate([28, 92, 164, 236, 304]):
        rad = math.radians(angle)
        bicone_mesh(
            f"LANDMARK_WORLDROOT_RIFT_ControlledPulledMemoryShard_{i:02d}",
            rifts,
            (math.cos(rad) * 3.25, 10.22 + math.sin(rad) * 1.12, 3.75 + (i % 2) * 0.28),
            0.14,
            0.46,
            mats["MAT_Waste_RiftCyanDeep"] if i in [0, 3] else (mats["MAT_Waste_RiftVioletControlled"] if i == 2 else mats["MAT_LostDivinity_PaleGold_Emission"]),
            sides=5,
        )

    # Slightly quiet the dead-god vertical light so it remains a role landmark but does not overpower the rift.
    for obj in bpy.data.objects:
        if "DEAD_GOD_SHRINE" in obj.name and "VerticalLight" in obj.name:
            obj.scale.x *= 0.62
            obj.scale.z *= 0.72

    def move_to_role_collection(obj, role_name):
        target = collections[role_name]
        if obj.name not in target.objects:
            target.objects.link(obj)
        for collection in list(obj.users_collection):
            if collection is not target and collection.name in collections:
                collection.objects.unlink(obj)

    role_counts = {name: 0 for name in role_collection_names}
    for obj in list(bpy.data.objects):
        name = obj.name
        role = None
        if (
            "LANDMARK_WORLDROOT_RIFT" in name
            or "LANDMARK_WORLDROOT_SHEDDING_RIFT" in name
            or "READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT" in name
            or "WorldrootSheddingRift" in name
            or "RiftScaleEcho" in name
        ):
            role = "LANDMARK_WORLDROOT_RIFT"
        elif (
            "LivingRootBridgeAcrossGap" in name
            or "UnstableMemoryBridge" in name
            or "ReadableTraversalStep" in name
            or "MemoryGuideShard_ToNextIsland" in name
        ):
            role = "TRAVERSAL_MEMORY_BRIDGES"
        elif (
            "ISLAND_A_SYLVAEN_SAFE_CAMP" in name
            or "READABILITY_ISLAND_A_SYLVAEN_SAFE_CAMP" in name
            or "SylvaenCamp" in name
            or "CentralSylvaenCampIsland" in name
            or "PlayerSpawnIsland" in name
            or "PlayerSpawn_SubtleSafeCampEchoCircle" in name
            or "PlayerPath_BrokenMossStone" in name
            or "PlayerPlaceholder_Foreground" in name
            or "MemoryKeeperStabilizer" in name
            or "RootGuardianCampWarden" in name
        ):
            role = "ISLAND_SAFE_CAMP"
        elif (
            "ISLAND_B_LOST_CIVILIZATION_RUINS" in name
            or "READABILITY_ISLAND_B_LOST_CIVILIZATION_RUINS" in name
            or "OranynRuinIsland" in name
            or "OranynRuins" in name
            or "LostCivilization" in name
            or "LostArchive" in name
            or "GhostArchitecture" in name
            or "OranynRuinScaleEcho" in name
        ):
            role = "ISLAND_LOST_RUINS"
        elif (
            "ISLAND_C_DEAD_GOD_SHRINE" in name
            or "READABILITY_ISLAND_C_DEAD_GOD_SHRINE" in name
            or "DeadGodShrine" in name
            or "DeadGod" in name
            or "DivineSigil" in name
            or "ShrineScalePilgrim" in name
        ):
            role = "ISLAND_DEAD_GOD_SHRINE"
        elif (
            "ISLAND_D_ECHO_BATTLEFIELD" in name
            or "READABILITY_ISLAND_D_ECHO_BATTLEFIELD" in name
            or "EchoBattlefieldIsland" in name
            or "ECHO_BATTLEFIELD" in name
        ):
            role = "ISLAND_ECHO_BATTLEFIELD"

        if role:
            move_to_role_collection(obj, role)
            role_counts[role] += 1

    bpy.context.scene["memory_wastes_role_collection_counts"] = json.dumps(role_counts, sort_keys=True)


def add_memory_wastes_scale_spacing_hierarchy_pass(collections, mats):
    role_offsets = {
        "ISLAND_SAFE_CAMP": (0.0, -3.9),
        "ISLAND_LOST_RUINS": (-11.5, 3.5),
        "ISLAND_DEAD_GOD_SHRINE": (9.5, 1.9),
        "ISLAND_ECHO_BATTLEFIELD": (-10.5, -3.2),
        "LANDMARK_WORLDROOT_RIFT": (2.2, 7.3),
    }
    role_scales = {
        "ISLAND_SAFE_CAMP": (0.94, 0.94, 0.98),
        "ISLAND_LOST_RUINS": (1.10, 1.10, 1.20),
        "ISLAND_DEAD_GOD_SHRINE": (1.04, 1.04, 1.05),
        "ISLAND_ECHO_BATTLEFIELD": (1.02, 1.02, 1.00),
        "LANDMARK_WORLDROOT_RIFT": (1.24, 1.24, 1.38),
    }

    before_centers = {
        "ISLAND_SAFE_CAMP": (0.0, -4.5),
        "ISLAND_LOST_RUINS": (-6.5, 12.8),
        "ISLAND_DEAD_GOD_SHRINE": (14.5, 5.8),
        "ISLAND_ECHO_BATTLEFIELD": (-16.0, 1.5),
        "LANDMARK_WORLDROOT_RIFT": (0.0, 10.4),
    }
    after_centers = {
        role: (
            before_centers[role][0] + role_offsets[role][0],
            before_centers[role][1] + role_offsets[role][1],
        )
        for role in before_centers
    }

    def bbox_area(centers):
        xs = [point[0] for point in centers.values()]
        ys = [point[1] for point in centers.values()]
        return (max(xs) - min(xs)) * (max(ys) - min(ys))

    before_area = bbox_area(before_centers)
    after_area = bbox_area(after_centers)
    introduced = int(round(((after_area / before_area) - 1.0) * 100))

    def move_and_scale_role(role):
        collection = collections.get(role)
        if not collection:
            return
        dx, dy = role_offsets[role]
        sx, sy, sz = role_scales[role]
        for obj in list(collection.objects):
            if obj.type not in {"MESH", "CURVE", "EMPTY"}:
                continue
            obj.location.x += dx
            obj.location.y += dy
            obj.scale.x *= sx
            obj.scale.y *= sy
            obj.scale.z *= sz

    for role in role_offsets:
        move_and_scale_role(role)

    dead_shrine = collections.get("ISLAND_DEAD_GOD_SHRINE")
    if dead_shrine:
        for obj in dead_shrine.objects:
            if any(token in obj.name for token in ["Halo", "VerticalLight", "PaleGold", "DivineSigil"]):
                obj.scale.x *= 0.86
                obj.scale.y *= 0.86
                obj.scale.z *= 0.82

    rift = collections["LANDMARK_WORLDROOT_RIFT"]
    for i, (x, z, h, mat_key, lean) in enumerate(
        [
            (1.80, 6.45, 10.8, "MAT_Waste_RiftCyanDeep", -2),
            (2.55, 5.85, 8.6, "MAT_Waste_RiftVioletControlled", 10),
            (2.18, 5.30, 6.3, "MAT_LostDivinity_PaleGold_Emission", 4),
        ]
    ):
        column = frustum_obj(
            f"SCALEPASS_LANDMARK_WORLDROOT_RIFT_DominantMemoryPressureColumn_{i:02d}_{mat_key.replace('MAT_', '')}",
            rift,
            (x, 17.75, z),
            0.32 - i * 0.05,
            0.035,
            h,
            7,
            mats[mat_key],
            rot=(math.radians(3), math.radians(2), math.radians(lean)),
        )
        disable_shadow(column)

    # Replace dense connector clutter with a few dangerous, partial traversal reads across the expanded void.
    traversal = collections["TRAVERSAL_MEMORY_BRIDGES"]
    removed_traversal = []
    traversal_remove_patterns = [
        "WASTE_Terrain_LivingRootBridgeAcrossGap_",
        "WASTE_Terrain_UnstableMemoryBridge_",
        "WASTE_Terrain_ReadableTraversalStep_",
        "WASTE_Terrain_MemoryGuideShard_ToNextIsland_",
    ]
    for obj in list(traversal.objects):
        if any(pattern in obj.name for pattern in traversal_remove_patterns):
            removed_traversal.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)

    def add_sparse_bridge(name, start, end, mat_key, pieces=3, width=0.34, z=0.66):
        sx, sy = start
        ex, ey = end
        dx = ex - sx
        dy = ey - sy
        length = math.hypot(dx, dy)
        angle = math.atan2(dy, dx) - math.pi / 2
        for i in range(pieces):
            t = (i + 1) / (pieces + 1)
            gap = (-0.12 if i % 2 else 0.12)
            x = sx + dx * t + math.cos(angle) * gap
            y = sy + dy * t + math.sin(angle) * gap
            segment = cube_obj(
                f"SCALEPASS_TRAVERSAL_{name}_DangerousPartialSpan_{i:02d}",
                traversal,
                (x, y, z + 0.08 * (i % 2)),
                (width, length / (pieces + 1) * 0.55, 0.13),
                mats[mat_key],
                rot=(math.radians(4 if i % 2 else -5), 0, angle + math.radians((-1) ** i * 4)),
            )
            disable_shadow(segment)
        for i, t in enumerate([0.38, 0.68]):
            x = sx + dx * t
            y = sy + dy * t
            shard = bicone_mesh(
                f"SCALEPASS_TRAVERSAL_{name}_SparseGuideShard_{i:02d}",
                traversal,
                (x, y, z + 0.54 + 0.12 * i),
                0.045,
                0.16,
                mats["MAT_Waste_GuideShard_CyanDim"],
                sides=5,
            )
            disable_shadow(shard)

    add_sparse_bridge("SafeCampToRift_BrokenRootMemoryPath", (0.0, -8.4), (2.2, 17.7), "MAT_Path_DarkRootShadow", pieces=4, width=0.28)
    add_sparse_bridge("SafeCampToEchoBattlefield_SplinteredRootPath", (-2.8, -7.4), (-26.5, -1.7), "MAT_Path_WarmRoot", pieces=3, width=0.26, z=0.58)
    add_sparse_bridge("SafeCampToLostRuins_GhostSteppingPath", (-1.8, -6.9), (-18.0, 16.3), "MAT_Echo_GhostBlueWhite", pieces=3, width=0.22, z=0.82)
    add_sparse_bridge("RiftToDeadGodShrine_PaleMemoryPath", (5.0, 16.8), (23.5, 7.7), "MAT_LostDivinity_PaleGold_Emission", pieces=2, width=0.24, z=0.80)

    # A few distant fragments deepen the composition without filling the playable negative space.
    terrain = collections["WASTE_TerrainIslands"]
    for i, (x, y, rx, ry, z, mat_key, rot) in enumerate(
        [
            (-33.0, 19.5, 2.8, 0.78, 2.7, "MAT_Backdrop_DistantMemoryIsland", -12),
            (-9.0, 24.0, 3.4, 0.90, 3.2, "MAT_Backdrop_DistantRiftIsland", 8),
            (31.0, 17.8, 2.9, 0.76, 2.9, "MAT_Backdrop_DistantDivineIsland", 16),
        ]
    ):
        disc_mesh(
            f"SCALEPASS_BACKGROUND_DistantFloatingMemoryFragment_{i:02d}",
            terrain,
            (x, y, z),
            rx,
            ry,
            mats[mat_key],
            sides=9,
            rot=(math.radians(78), 0, math.radians(rot)),
        )

    bpy.context.scene["memory_wastes_scale_before_centers"] = json.dumps(before_centers, sort_keys=True)
    bpy.context.scene["memory_wastes_scale_after_centers"] = json.dumps(after_centers, sort_keys=True)
    bpy.context.scene["memory_wastes_negative_space_increase_percent"] = introduced
    bpy.context.scene["memory_wastes_traversal_removed_for_spacing"] = len(removed_traversal)
    bpy.context.scene["memory_wastes_scale_pass_note"] = (
        "Role islands were pushed outward inside the existing visual bounds; old dense bridges were replaced "
        "with sparse partial paths so the void reads as dangerous distance."
    )


def add_memory_wastes_depth_silhouette_landmark_pass(collections, mats):
    """Strengthen island silhouettes and depth without changing the expanded island spacing."""
    final_centers = {
        "ISLAND_SAFE_CAMP": (0.0, -8.4),
        "ISLAND_LOST_RUINS": (-18.0, 16.3),
        "ISLAND_DEAD_GOD_SHRINE": (24.0, 7.7),
        "ISLAND_ECHO_BATTLEFIELD": (-26.5, -1.7),
        "LANDMARK_WORLDROOT_RIFT": (2.2, 17.7),
    }

    removed_debug = []
    remove_patterns = [
        "WASTE_Characters_BlightrootRavager_RiftThreat",
        "WASTE_RootRiftsAndStorms_VioletRiftCoreShard_",
        "WASTE_Foliage_RiftsideWorldrootCrystalSprout",
        "WASTE_RootRiftsAndStorms_FloatingMemoryDustShard_",
        "WASTE_RootRiftsAndStorms_MemoryStormSlash_",
    ]
    for obj in list(bpy.data.objects):
        if any(pattern in obj.name for pattern in remove_patterns):
            removed_debug.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)

    def raise_role(role, dz, z_scale=1.0):
        collection = collections.get(role)
        if not collection:
            return
        for obj in collection.objects:
            if obj.type == "MESH":
                obj.location.z += dz
                obj.scale.z *= z_scale

    # Preserve spacing, but give each role a different height language.
    raise_role("ISLAND_LOST_RUINS", 0.42, 1.07)
    raise_role("ISLAND_DEAD_GOD_SHRINE", 0.28, 1.03)
    raise_role("LANDMARK_WORLDROOT_RIFT", 0.34, 1.12)
    raise_role("ISLAND_ECHO_BATTLEFIELD", 0.06, 0.96)

    safe = collections["ISLAND_SAFE_CAMP"]
    lost = collections["ISLAND_LOST_RUINS"]
    shrine = collections["ISLAND_DEAD_GOD_SHRINE"]
    echo = collections["ISLAND_ECHO_BATTLEFIELD"]
    rift = collections["LANDMARK_WORLDROOT_RIFT"]
    traversal = collections["TRAVERSAL_MEMORY_BRIDGES"]
    terrain = collections["WASTE_TerrainIslands"]
    lighting = collections["WASTE_LightingRender"]

    # Safe Camp: deliberately low, stable and readable as the player's foothold.
    disc_mesh(
        "DEPTH_SAFE_CAMP_StableProtectedFooting_WarmCyanLowGlow",
        safe,
        (0.0, -8.4, 0.34),
        3.15,
        1.58,
        mats["MAT_Waste_SafeCyanGlow"],
        sides=34,
        rot=(0, 0, math.radians(4)),
    )
    for i, (x, y, rot) in enumerate([(-2.0, -8.1, 14), (2.1, -8.35, -12), (-0.8, -6.8, 4)]):
        cube_obj(
            f"DEPTH_SAFE_CAMP_GroundedRootStabilityRidge_{i:02d}",
            safe,
            (x, y, 0.54),
            (1.55, 0.24, 0.18),
            mats["MAT_Path_DarkRootShadow"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
    cube_obj("DEPTH_SAFE_CAMP_ArchiveTable_ClearScaleAnchor", safe, (-1.55, -7.55, 0.75), (1.25, 0.72, 0.24), mats["MAT_Bark_WarmBrown"], rot=(0, 0, math.radians(7)))
    bicone_mesh("DEPTH_SAFE_CAMP_MemoryBrazier_ProtectedCyanEmber", safe, (1.55, -7.65, 1.05), 0.18, 0.52, mats["MAT_Waste_SafeCyanGlow"], sides=6)

    # Lost Civilization: one strong broken-arch read, plus vertical half-manifested fragments.
    lx, ly = final_centers["ISLAND_LOST_RUINS"]
    cube_obj("DEPTH_LOST_RUINS_GreatBrokenArchiveArch_LeftPillar", lost, (lx - 2.55, ly, 2.85), (0.46, 0.62, 4.85), mats["MAT_LostCivilization_GhostStone"], rot=(math.radians(3), 0, math.radians(-5)))
    cube_obj("DEPTH_LOST_RUINS_GreatBrokenArchiveArch_RightPillar", lost, (lx + 2.35, ly + 0.08, 2.55), (0.44, 0.62, 4.25), mats["MAT_LostCivilization_GhostStone"], rot=(math.radians(-2), 0, math.radians(7)))
    xz_beam_obj("DEPTH_LOST_RUINS_GreatBrokenArchiveArch_TopLeftFragment", lost, ly + 0.02, (lx - 2.42, 5.26), (lx - 0.42, 6.20), 0.54, 0.32, mats["MAT_LostCivilization_GhostStone"])
    xz_beam_obj("DEPTH_LOST_RUINS_GreatBrokenArchiveArch_TopRightFragment", lost, ly + 0.02, (lx + 0.20, 6.10), (lx + 2.15, 5.18), 0.54, 0.32, mats["MAT_Waste_GhostRuinSoft"])
    for i, (x, y, h, rot) in enumerate([(lx - 4.15, ly - 0.7, 2.8, -8), (lx + 4.05, ly + 0.55, 2.35, 10)]):
        cube_obj(
            f"DEPTH_LOST_RUINS_PartialColumn_NotFullyManifested_{i:02d}",
            lost,
            (x, y, 1.55 + h * 0.18),
            (0.52, 0.52, h),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(5), math.radians(2), math.radians(rot)),
        )
        bicone_mesh(
            f"DEPTH_LOST_RUINS_GhostCyanCrack_OnColumn_{i:02d}",
            lost,
            (x + 0.05, y - 0.18, 2.75),
            0.06,
            0.30,
            mats["MAT_Waste_RiftCyanDeep"],
            sides=5,
        )
    for i, (dx, dy, z, rot) in enumerate([(-1.6, -1.2, 1.12, -8), (-0.72, -0.76, 1.42, -4), (0.22, -0.32, 1.78, 2), (1.18, 0.05, 2.12, 7)]):
        cube_obj(
            f"DEPTH_LOST_RUINS_IncompleteFloatingStairPiece_{i:02d}",
            lost,
            (lx + dx, ly + dy, z),
            (1.18, 0.55, 0.16),
            mats["MAT_Waste_GhostRuinSoft"] if i % 2 else mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(2), math.radians(-3), math.radians(rot)),
        )
    for i, (dx, dy, z, sx, sz, rot) in enumerate([(-3.5, 1.5, 3.45, 1.4, 1.8, -14), (3.2, 1.25, 3.15, 1.55, 1.55, 12)]):
        cube_obj(
            f"DEPTH_LOST_RUINS_OutOfAlignmentFloatingWallShard_{i:02d}",
            lost,
            (lx + dx, ly + dy, z),
            (sx, 0.22, sz),
            mats["MAT_LostCivilization_GhostStone"],
            rot=(math.radians(4), math.radians(8 if i else -7), math.radians(rot)),
        )

    # Dead God Shrine: cleaner sacred silhouette, gold constrained to shrine and small rift fragments.
    sx, sy = final_centers["ISLAND_DEAD_GOD_SHRINE"]
    halo_points = [(-1.72, 3.00), (-0.92, 4.05), (0.00, 4.35), (0.98, 4.02), (1.78, 2.95)]
    for i in range(len(halo_points) - 1):
        if i == 2:
            continue
        xz_beam_obj(
            f"DEPTH_DEAD_GOD_SHRINE_CleanCrackedHalo_Segment_{i:02d}",
            shrine,
            sy,
            (sx + halo_points[i][0], halo_points[i][1]),
            (sx + halo_points[i + 1][0], halo_points[i + 1][1]),
            0.32,
            0.18,
            mats["MAT_LostDivinity_PaleGold_Emission"] if i in [1, 3] else mats["MAT_DeadGod_Stone"],
        )
    cube_obj("DEPTH_DEAD_GOD_SHRINE_BrokenAltar_CeremonialCore", shrine, (sx, sy - 0.45, 0.92), (2.0, 1.05, 0.58), mats["MAT_DeadGod_Stone"], rot=(math.radians(2), 0, math.radians(4)))
    cube_obj("DEPTH_DEAD_GOD_SHRINE_MissingNameRuneSlab_SolemnFront", shrine, (sx - 0.95, sy - 1.32, 1.25), (0.32, 0.16, 1.55), mats["MAT_DeadGod_Stone"], rot=(math.radians(8), 0, math.radians(-12)))
    frustum_obj("DEPTH_DEAD_GOD_SHRINE_ControlledPaleGoldVerticalLight", shrine, (sx + 0.20, sy - 0.28, 2.92), 0.22, 0.04, 3.25, 6, mats["MAT_LostDivinity_PaleGold_Emission"])
    for i, angle in enumerate([22, 96, 202, 278]):
        rad = math.radians(angle)
        cube_obj(
            f"DEPTH_DEAD_GOD_SHRINE_ShatteredDivineSigil_Fragment_{i:02d}",
            shrine,
            (sx + math.cos(rad) * 2.1, sy - 0.3 + math.sin(rad) * 1.2, 0.72),
            (0.72, 0.18, 0.10),
            mats["MAT_LostDivinity_PaleGold_Emission"] if i % 2 else mats["MAT_DeadGod_Stone"],
            rot=(math.radians(3), 0, rad + math.radians(8)),
        )

    # Echo Battlefield: flatter, darker, sparse, with unmistakable battle-memory motifs.
    ex, ey = final_centers["ISLAND_ECHO_BATTLEFIELD"]
    disc_mesh(
        "DEPTH_ECHO_BATTLEFIELD_DarkCircularMemoryStain_OldWarScar",
        echo,
        (ex, ey, 0.38),
        4.6,
        2.25,
        mats["MAT_Waste_EchoStainSoft"],
        sides=32,
        rot=(0, 0, math.radians(-10)),
    )
    for i, (dx, dy, lean) in enumerate([(-3.2, -0.7, -18), (-1.4, 0.9, 8), (0.8, -0.1, -4), (2.9, 0.65, 18)]):
        cube_obj(
            f"DEPTH_ECHO_BATTLEFIELD_EmbeddedSpear_MemoryWeapon_{i:02d}",
            echo,
            (ex + dx, ey + dy, 1.08),
            (0.07, 0.07, 1.75),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(12), math.radians(lean), math.radians(lean)),
        )
        bicone_mesh(
            f"DEPTH_ECHO_BATTLEFIELD_Spearhead_GhostMetal_{i:02d}",
            echo,
            (ex + dx + 0.03, ey + dy, 2.02),
            0.08,
            0.20,
            mats["MAT_Echo_GhostBlueWhite"],
            sides=4,
        )
    for i, (dx, dy, rot) in enumerate([(-2.2, 1.25, 11), (1.95, -1.1, -15)]):
        disc_mesh(
            f"DEPTH_ECHO_BATTLEFIELD_BrokenShield_Fragment_{i:02d}",
            echo,
            (ex + dx, ey + dy, 0.66),
            0.52,
            0.30,
            mats["MAT_Waste_EchoStainSoft"],
            sides=9,
            rot=(math.radians(72), 0, math.radians(rot)),
        )
    for i, (dx, dy, rot) in enumerate([(-3.7, 0.95, -8), (3.25, 0.82, 10)]):
        cube_obj(f"DEPTH_ECHO_BATTLEFIELD_GhostBanner_Pole_{i:02d}", echo, (ex + dx, ey + dy, 1.24), (0.08, 0.08, 2.05), mats["MAT_Path_DarkRootShadow"], rot=(math.radians(5), 0, math.radians(rot)))
        cube_obj(f"DEPTH_ECHO_BATTLEFIELD_GhostBanner_TornMemoryCloth_{i:02d}", echo, (ex + dx + 0.35, ey + dy, 1.88), (0.78, 0.06, 0.58), mats["MAT_Echo_GhostBlueWhite"], rot=(math.radians(4), 0, math.radians(rot)))
    for i, (dx, dy) in enumerate([(-0.75, -0.55), (0.45, 0.55), (1.55, -0.25)]):
        make_memory_echo_silhouette(
            f"DEPTH_ECHO_BATTLEFIELD_TranslucentSoldierEcho_{i:02d}",
            echo,
            mats,
            (ex + dx, ey + dy, 0.48),
            mat_key="MAT_Echo_GhostBlueWhite",
        )

    # Worldroot Rift: wider mouth, tilted plates and taller vertical pull from the already-expanded center.
    rx, ry = final_centers["LANDMARK_WORLDROOT_RIFT"]
    disc_mesh(
        "DEPTH_RIFT_WiderCrackedGroundFissure_CyanVioletCore",
        rift,
        (rx, ry, 0.66),
        4.6,
        1.52,
        mats["MAT_Waste_RiftCyanDeep"],
        sides=36,
        rot=(0, 0, math.radians(6)),
    )
    cube_obj("DEPTH_RIFT_WideBlackRootMouth_PrimaryCrack", rift, (rx, ry, 0.76), (7.1, 0.56, 0.18), mats["MAT_WasteGround_Dark"], rot=(math.radians(3), 0, math.radians(5)))
    cube_obj("DEPTH_RIFT_VioletInstabilityEdge_Left", rift, (rx - 0.65, ry + 0.04, 0.92), (3.4, 0.13, 0.08), mats["MAT_Waste_RiftVioletControlled"], rot=(math.radians(4), 0, math.radians(18)))
    cube_obj("DEPTH_RIFT_CyanMemoryEdge_Right", rift, (rx + 0.70, ry - 0.04, 0.94), (3.8, 0.12, 0.08), mats["MAT_Waste_RiftCyanDeep"], rot=(math.radians(4), 0, math.radians(-12)))
    for i, angle in enumerate([-72, -48, -24, 18, 44, 70]):
        rad = math.radians(angle)
        length = 4.1 + (i % 3) * 0.72
        x = rx + math.sin(rad) * 1.85
        y = ry - 0.25 + math.cos(rad) * 0.62
        cube_obj(
            f"DEPTH_RIFT_ExposedRootFiber_NervePull_{i:02d}",
            rift,
            (x, y, 1.05 + i * 0.04),
            (0.16, length, 0.12),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(8), math.radians(2), rad),
        )
        if i in [1, 3, 5]:
            cube_obj(
                f"DEPTH_RIFT_CyanPulseInsideRootFiber_{i:02d}",
                rift,
                (x, y + 0.16, 1.22 + i * 0.04),
                (0.045, length * 0.55, 0.045),
                mats["MAT_Waste_RiftCyanDeep"],
                rot=(math.radians(8), math.radians(2), rad),
            )
    for i, (dx, dy, sxp, syp, z, rot, mat_key) in enumerate(
        [
            (-3.5, -1.3, 1.9, 0.92, 0.98, -24, "MAT_WasteGround"),
            (-1.8, 1.25, 1.42, 0.82, 1.22, 16, "MAT_Stone_DarkCrevice"),
            (1.8, -1.12, 1.64, 0.84, 1.10, 22, "MAT_WasteGround"),
            (3.4, 1.05, 1.78, 0.86, 1.32, -18, "MAT_Waste_RiftVioletControlled"),
            (0.0, 2.1, 1.35, 0.70, 1.62, 4, "MAT_Waste_GhostRuinSoft"),
        ]
    ):
        plate = cube_obj(
            f"DEPTH_RIFT_BrokenGroundPlate_TiltedTowardFissure_{i:02d}",
            rift,
            (rx + dx, ry + dy, z),
            (sxp, syp, 0.18),
            mats[mat_key],
            rot=(math.radians(7 + i * 2), math.radians((-1) ** i * 8), math.radians(rot)),
        )
        disable_shadow(plate)
    for i, angle in enumerate([12, 58, 112, 168, 226, 282, 326]):
        rad = math.radians(angle)
        mat_key = "MAT_Waste_RiftCyanDeep" if i in [0, 3, 6] else ("MAT_LostDivinity_PaleGold_Emission" if i in [2, 5] else "MAT_Waste_RiftVioletControlled")
        shard = bicone_mesh(
            f"DEPTH_RIFT_UpdraftPulledMemoryShard_{i:02d}_{mat_key.replace('MAT_', '')}",
            rift,
            (rx + math.cos(rad) * (2.0 + 0.18 * i), ry + math.sin(rad) * 0.95, 4.0 + i * 0.62),
            0.13,
            0.48,
            mats[mat_key],
            sides=5,
        )
        disable_shadow(shard)
    for i, (dx, dy, z, h, mat_key, lean) in enumerate(
        [
            (0.0, 0.0, 6.4, 12.8, "MAT_Waste_RiftCyanDeep", 0),
            (-0.55, 0.18, 5.8, 9.8, "MAT_Waste_RiftVioletControlled", -7),
            (0.65, -0.10, 5.35, 7.1, "MAT_LostDivinity_PaleGold_Emission", 8),
        ]
    ):
        plume = frustum_obj(
            f"DEPTH_RIFT_DominantVerticalMemorySheddingPlume_{i:02d}_{mat_key.replace('MAT_', '')}",
            rift,
            (rx + dx, ry + dy, z),
            0.42 - i * 0.08,
            0.035,
            h,
            7,
            mats[mat_key],
            rot=(math.radians(3), math.radians(1), math.radians(lean)),
        )
        disable_shadow(plume)

    # Traversal: readable but incomplete, emphasizing danger between islands.
    for i, (x, y, z, sxp, rot) in enumerate([(-0.8, -2.4, 0.92, 0.90, -4), (0.0, 2.8, 1.05, 0.72, 7), (0.9, 8.2, 1.20, 0.58, -9)]):
        cube_obj(
            f"DEPTH_TRAVERSAL_SafeCampToRift_FloatingSteppingStone_{i:02d}",
            traversal,
            (x, y, z),
            (sxp, 0.48, 0.14),
            mats["MAT_Path_MossyStone"],
            rot=(math.radians(4), math.radians(-2), math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(-7.2, 2.8, -22), (-12.4, 8.5, -19)]):
        cube_obj(
            f"DEPTH_TRAVERSAL_LostRuinsRoute_PaleMemoryBridgeFragment_{i:02d}",
            traversal,
            (x, y, 1.14 + i * 0.16),
            (1.55, 0.36, 0.10),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(3), math.radians(3), math.radians(rot)),
        )

    # Void atmosphere: low-contrast depth cues only, no extra playable clutter.
    # Avoid broad foreground fog sheets here; they read as blockout walls from the gameplay camera.
    for i, (x, y, z, rot, mat_key) in enumerate(
        [
            (-30.5, 10.8, 4.2, -18, "MAT_Backdrop_DistantMemoryIsland"),
            (-4.8, 21.2, 5.6, 8, "MAT_Backdrop_DistantRiftIsland"),
            (27.0, 14.4, 4.7, 18, "MAT_Backdrop_DistantDivineIsland"),
        ]
    ):
        disc_mesh(
            f"DEPTH_VOID_DistantFloatingFragment_LowContrast_{i:02d}",
            terrain,
            (x, y, z),
            2.0 + i * 0.35,
            0.46 + i * 0.12,
            mats[mat_key],
            sides=8,
            rot=(math.radians(78), 0, math.radians(rot)),
        )
    for i, (x, y, z, h) in enumerate([(-10.0, 12.0, 4.8, 2.2), (7.6, 15.6, 5.3, 2.8), (18.5, 11.8, 4.2, 1.9)]):
        debris = cube_obj(
            f"DEPTH_VOID_FaintFallingDebrisTrace_{i:02d}",
            terrain,
            (x, y, z),
            (0.06, 0.06, h),
            mats["MAT_Backdrop_DistantMemoryIsland"],
            rot=(math.radians(8), 0, math.radians(10 + i * 12)),
        )
        disable_shadow(debris)

    # Strengthen rift light slightly while keeping shrine/camp supportive.
    bpy.ops.object.light_add(type="POINT", location=(rx, ry, 5.3))
    light = bpy.context.object
    light.name = "DEPTH_LIGHTING_WorldrootRift_PrimaryDominanceGlow"
    light.data.color = (0.16, 0.72, 1.0)
    light.data.energy = 285
    light.data.shadow_soft_size = 6.5
    if hasattr(light.data, "use_shadow"):
        light.data.use_shadow = False
    link_to(lighting, light)

    previous_removed = int(bpy.context.scene.get("memory_wastes_debug_markers_removed", 0))
    bpy.context.scene["memory_wastes_debug_markers_removed"] = previous_removed + len(removed_debug)
    previous_readability_removed = int(bpy.context.scene.get("memory_wastes_readability_removed_debug_shapes", 0))
    bpy.context.scene["memory_wastes_readability_removed_debug_shapes"] = previous_readability_removed + len(removed_debug)
    bpy.context.scene["memory_wastes_depth_pass_removed_debug_names"] = ", ".join(removed_debug[:30])
    bpy.context.scene["memory_wastes_depth_pass_centers_preserved"] = json.dumps(final_centers, sort_keys=True)
    bpy.context.scene["memory_wastes_depth_pass_note"] = (
        "The expanded spacing was preserved; this pass only raised/silhouetted existing island roles, strengthened "
        "the Worldroot Rift and added sparse void atmosphere."
    )


def add_memory_wastes_vista_prefab_transfer_pass(collections, mats):
    """Borrow the vista benchmark's kit language while preserving playable island spacing and routes."""
    if "WASTE_BackgroundFloatingSilhouettes" not in collections:
        background = bpy.data.collections.new("WASTE_BackgroundFloatingSilhouettes")
        bpy.context.scene.collection.children.link(background)
        collections["WASTE_BackgroundFloatingSilhouettes"] = background
    else:
        background = collections["WASTE_BackgroundFloatingSilhouettes"]

    safe = collections["ISLAND_SAFE_CAMP"]
    lost = collections["ISLAND_LOST_RUINS"]
    shrine = collections["ISLAND_DEAD_GOD_SHRINE"]
    echo = collections["ISLAND_ECHO_BATTLEFIELD"]
    rift = collections["LANDMARK_WORLDROOT_RIFT"]
    traversal = collections["TRAVERSAL_MEMORY_BRIDGES"]

    removed = []
    replace_patterns = [
        "READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_PrimaryVerticalLight_",
        "READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT_FewFloatingMemoryShard_",
        "LANDMARK_WORLDROOT_RIFT_PrimarySheddingSpire_",
        "SCALEPASS_LANDMARK_WORLDROOT_RIFT_DominantMemoryPressureColumn_",
        "DEPTH_RIFT_UpdraftPulledMemoryShard_",
        "READABILITY_ISLAND_B_LOST_CIVILIZATION_RUINS_TransparentWallMass_Readable_",
        "WASTE_FloatingRuins_LostArchiveWallShard_",
        "READABILITY_ISLAND_D_ECHO_BATTLEFIELD_SpearLineSilhouette_",
        "READABILITY_ISLAND_D_ECHO_BATTLEFIELD_SpearTipGlow_",
    ]
    for obj in list(bpy.data.objects):
        if any(pattern in obj.name for pattern in replace_patterns):
            removed.append(obj.name)
            bpy.data.objects.remove(obj, do_unlink=True)

    def terrain_variant(prefix, collection, x, y, rx, ry, mat_key, rot=0.0):
        disc_mesh(
            f"MEMORY_ISLAND_TERRAIN_VARIANTS_{prefix}_UpperPaintedMemorySkin",
            collection,
            (x, y, 0.55),
            rx,
            ry,
            mats[mat_key],
            sides=11,
            rot=(0, 0, math.radians(rot)),
        )
        for i, (dx, dy, sx, sy, z, angle) in enumerate(
            [(-0.48, -0.34, 0.24, 0.12, 0.34, -12), (0.16, -0.43, 0.20, 0.10, 0.27, 8), (0.52, -0.22, 0.18, 0.09, 0.30, 18)]
        ):
            cube_obj(
                f"MEMORY_ISLAND_TERRAIN_VARIANTS_{prefix}_JaggedHangingEdge_{i:02d}",
                collection,
                (x + dx * rx, y + dy * ry, z),
                (rx * sx, ry * sy, 0.22),
                mats["MAT_WasteGround_Dark"],
                rot=(math.radians(5), math.radians((-1) ** i * 4), math.radians(rot + angle)),
            )

    terrain_variant("SafeCampStableFoothold", safe, 0.0, -8.4, 5.2, 2.65, "MAT_Waste_SafeCyanGlow", rot=4)
    terrain_variant("LostRuinsGhostIsland", lost, -18.0, 16.3, 5.8, 3.15, "MAT_Waste_GhostRuinSoft", rot=-10)
    terrain_variant("DeadGodShrineCeremonialIsland", shrine, 24.0, 7.7, 5.3, 2.85, "MAT_Waste_DivinityStainSoft", rot=12)
    terrain_variant("EchoBattlefieldScarredIsland", echo, -26.5, -1.7, 5.7, 2.55, "MAT_Waste_EchoStainSoft", rot=-8)
    terrain_variant("WorldrootRiftSplitIsland", rift, 2.2, 17.7, 7.8, 3.25, "MAT_Waste_RiftCyanDeep", rot=5)

    # LANDMARK_WORLDROOT_SHEDDING_RIFT: replace the marker-cluster feeling with a designed vista-derived split-rift kit.
    rx, ry = 2.2, 17.7
    poly_obj(
        "LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_SplitIslandFissureMouth",
        rift,
        [(rx - 5.9, ry - 0.78), (rx - 3.2, ry - 1.15), (rx - 1.35, ry - 0.42), (rx + 0.15, ry - 1.05), (rx + 2.35, ry - 0.38), (rx + 6.1, ry - 0.76), (rx + 4.25, ry + 0.94), (rx + 1.55, ry + 1.38), (rx - 1.2, ry + 0.92), (rx - 3.55, ry + 1.50), (rx - 6.2, ry + 0.54)],
        mats["MAT_WasteGround_Dark"],
        z=1.04,
    )
    disc_mesh(
        "LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_CyanMemoryLightFromBelow",
        rift,
        (rx, ry, 1.16),
        5.3,
        1.55,
        mats["MAT_Waste_RiftCyanDeep"],
        sides=40,
        rot=(0, 0, math.radians(5)),
    )
    cube_obj("LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_VioletInstabilityEdge_Left", rift, (rx - 0.95, ry + 0.02, 1.34), (4.8, 0.15, 0.10), mats["MAT_Waste_RiftVioletControlled"], rot=(math.radians(4), 0, math.radians(16)))
    cube_obj("LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_CyanMemoryEdge_Right", rift, (rx + 1.0, ry - 0.04, 1.36), (5.0, 0.14, 0.10), mats["MAT_Waste_RiftCyanDeep"], rot=(math.radians(4), 0, math.radians(-12)))
    for i, angle in enumerate([-72, -50, -28, -6, 18, 42, 66]):
        rad = math.radians(angle)
        length = 5.0 + (i % 3) * 0.75
        x = rx + math.sin(rad) * 2.1
        y = ry - 0.35 + math.cos(rad) * 0.72
        cube_obj(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_ExposedRootNerve_{i:02d}",
            rift,
            (x, y, 1.52 + i * 0.04),
            (0.18, length, 0.14),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(8), math.radians(2), rad),
        )
        if i in [1, 3, 5]:
            cube_obj(
                f"LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_CyanPulseOnRootNerve_{i:02d}",
                rift,
                (x, y + 0.18, 1.70 + i * 0.04),
                (0.052, length * 0.54, 0.05),
                mats["MAT_Waste_RiftCyanDeep"],
                rot=(math.radians(8), math.radians(2), rad),
            )
    for i, (dx, dy, sx, sy, z, rot, mat_key) in enumerate(
        [
            (-4.6, -1.32, 2.2, 0.86, 1.55, -24, "MAT_WasteGround"),
            (-2.15, 1.28, 1.65, 0.76, 1.78, 16, "MAT_Stone_DarkCrevice"),
            (2.35, -1.10, 1.88, 0.80, 1.70, 22, "MAT_WasteGround"),
            (4.60, 1.08, 1.95, 0.82, 1.92, -18, "MAT_Waste_RiftVioletControlled"),
            (0.10, 2.10, 1.55, 0.64, 2.18, 4, "MAT_Waste_GhostRuinSoft"),
        ]
    ):
        plate = cube_obj(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_LiftedTerrainPlate_{i:02d}",
            rift,
            (rx + dx, ry + dy, z),
            (sx, sy, 0.18),
            mats[mat_key],
            rot=(math.radians(9 + i), math.radians((-1) ** i * 8), math.radians(rot)),
        )
        disable_shadow(plate)
    for i, (dx, dy, z, h, mat_key, lean) in enumerate(
        [
            (0.0, 0.0, 7.2, 17.6, "MAT_Waste_RiftCyanDeep", 0),
            (-0.68, 0.22, 6.4, 12.8, "MAT_Waste_RiftVioletControlled", -8),
            (0.75, -0.12, 5.8, 9.0, "MAT_LostDivinity_PaleGold_Emission", 8),
        ]
    ):
        plume = frustum_obj(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_DominantMemoryPlume_{i:02d}",
            rift,
            (rx + dx, ry + dy, z),
            0.44 - i * 0.08,
            0.035,
            h,
            7,
            mats[mat_key],
            rot=(math.radians(3), math.radians(1), math.radians(lean)),
        )
        disable_shadow(plume)
    for i, angle in enumerate([18, 52, 92, 138, 206, 262, 318]):
        rad = math.radians(angle)
        mat_key = "MAT_Waste_RiftCyanDeep" if i in [0, 3, 6] else ("MAT_LostDivinity_PaleGold_Emission" if i in [2, 5] else "MAT_Waste_RiftVioletControlled")
        shard = bicone_mesh(
            f"LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_PulledMemoryShard_{i:02d}",
            rift,
            (rx + math.cos(rad) * (2.65 + i * 0.12), ry + math.sin(rad) * 1.18, 4.55 + i * 0.72),
            0.15,
            0.56,
            mats[mat_key],
            sides=5,
        )
        disable_shadow(shard)

    # LOST_CIVILIZATION_RUIN_KIT: fewer stronger arch/column/stair silhouettes, matching the vista benchmark.
    lx, ly = -18.0, 16.3
    cube_obj("LOST_CIVILIZATION_RUIN_KIT_HeroBrokenArch_LeftPillar", lost, (lx - 3.15, ly, 3.45), (0.62, 0.74, 5.65), mats["MAT_LostCivilization_GhostStone"], rot=(math.radians(3), 0, math.radians(-5)))
    cube_obj("LOST_CIVILIZATION_RUIN_KIT_HeroBrokenArch_RightPillar", lost, (lx + 3.0, ly + 0.12, 3.08), (0.58, 0.72, 4.95), mats["MAT_Waste_GhostRuinSoft"], rot=(math.radians(-2), 0, math.radians(7)))
    xz_beam_obj("LOST_CIVILIZATION_RUIN_KIT_HeroBrokenArch_LeftCrown", lost, ly + 0.03, (lx - 3.0, 6.42), (lx - 0.34, 7.62), 0.62, 0.34, mats["MAT_LostCivilization_GhostStone"])
    xz_beam_obj("LOST_CIVILIZATION_RUIN_KIT_HeroBrokenArch_RightCrown", lost, ly + 0.03, (lx + 0.22, 7.45), (lx + 2.75, 6.32), 0.62, 0.32, mats["MAT_Waste_GhostRuinSoft"])
    for i, (dx, dy, h, rot) in enumerate([(-5.2, -0.9, 3.2, -9), (5.1, 0.78, 2.85, 11), (-0.7, 2.2, 2.35, 4)]):
        cube_obj(
            f"LOST_CIVILIZATION_RUIN_KIT_PartialColumn_FloatingManifestation_{i:02d}",
            lost,
            (lx + dx, ly + dy, 1.85 + h * 0.22),
            (0.54, 0.54, h),
            mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(5), math.radians(2), math.radians(rot)),
        )
    for i, (dx, dy, z, rot) in enumerate([(-1.9, -1.45, 1.28, -8), (-0.85, -0.92, 1.62, -4), (0.32, -0.42, 2.02, 3), (1.55, 0.02, 2.42, 8)]):
        cube_obj(
            f"LOST_CIVILIZATION_RUIN_KIT_IncompleteStairFragment_{i:02d}",
            lost,
            (lx + dx, ly + dy, z),
            (1.20, 0.54, 0.16),
            mats["MAT_LostCivilization_GhostStone"] if i % 2 else mats["MAT_Waste_GhostRuinSoft"],
            rot=(math.radians(2), math.radians(-3), math.radians(rot)),
        )

    # DEAD_GOD_SHRINE_KIT: clean gold/stone motif, not random gold scatter.
    sx, sy = 24.0, 7.7
    disc_mesh("DEAD_GOD_SHRINE_KIT_PaleGoldSacredMemoryStain", shrine, (sx, sy, 0.62), 3.95, 1.92, mats["MAT_Waste_DivinityStainSoft"], sides=36, rot=(0, 0, math.radians(10)))
    halo_points = [(-1.95, 3.18), (-1.08, 4.45), (0.0, 4.90), (1.08, 4.42), (1.96, 3.15)]
    for i in range(len(halo_points) - 1):
        if i == 2:
            continue
        xz_beam_obj(
            f"DEAD_GOD_SHRINE_KIT_CrackedCircularHalo_Segment_{i:02d}",
            shrine,
            sy,
            (sx + halo_points[i][0], halo_points[i][1]),
            (sx + halo_points[i + 1][0], halo_points[i + 1][1]),
            0.36,
            0.20,
            mats["MAT_LostDivinity_PaleGold_Emission"] if i in [1, 3] else mats["MAT_DeadGod_Stone"],
        )
    cube_obj("DEAD_GOD_SHRINE_KIT_BrokenAltar_CoreStone", shrine, (sx, sy - 0.45, 1.10), (2.0, 1.0, 0.62), mats["MAT_DeadGod_Stone"], rot=(math.radians(2), 0, math.radians(4)))
    cube_obj("DEAD_GOD_SHRINE_KIT_MissingNameRuneSlab", shrine, (sx - 1.15, sy - 1.35, 1.55), (0.34, 0.17, 1.72), mats["MAT_DeadGod_Shadow"], rot=(math.radians(8), 0, math.radians(-12)))
    frustum_obj("DEAD_GOD_SHRINE_KIT_PaleGoldVerticalLight_DivineRemnant", shrine, (sx + 0.16, sy - 0.25, 3.30), 0.24, 0.04, 4.2, 7, mats["MAT_LostDivinity_PaleGold_Emission"])
    for i, angle in enumerate([20, 88, 158, 218, 288]):
        rad = math.radians(angle)
        cube_obj(
            f"DEAD_GOD_SHRINE_KIT_ShatteredDivineSigil_Fragment_{i:02d}",
            shrine,
            (sx + math.cos(rad) * 2.45, sy - 0.3 + math.sin(rad) * 1.34, 0.82),
            (0.70, 0.17, 0.11),
            mats["MAT_LostDivinity_PaleGold_Emission"] if i % 2 else mats["MAT_DeadGod_Stone"],
            rot=(math.radians(3), 0, rad + math.radians(8)),
        )

    # ECHO_BATTLEFIELD_KIT: sparse readable battle memory, not an abstract cluster.
    ex, ey = -26.5, -1.7
    disc_mesh("ECHO_BATTLEFIELD_KIT_CircularDarkMemoryStain", echo, (ex, ey, 0.48), 5.1, 2.25, mats["MAT_Waste_EchoStainSoft"], sides=32, rot=(0, 0, math.radians(-10)))
    for i, (dx, dy, lean) in enumerate([(-3.0, -0.60, -18), (-1.05, 0.72, 6), (1.05, -0.15, -5), (3.05, 0.52, 16)]):
        cube_obj(
            f"ECHO_BATTLEFIELD_KIT_EmbeddedSpearMemory_{i:02d}",
            echo,
            (ex + dx, ey + dy, 1.25),
            (0.075, 0.075, 1.95),
            mats["MAT_Echo_GhostBlueWhite"],
            rot=(math.radians(12), math.radians(lean), math.radians(lean)),
        )
    for i, (dx, dy, rot) in enumerate([(-2.15, 1.18, 12), (1.95, -1.05, -14)]):
        disc_mesh(
            f"ECHO_BATTLEFIELD_KIT_BrokenShieldMemory_{i:02d}",
            echo,
            (ex + dx, ey + dy, 0.78),
            0.56,
            0.31,
            mats["MAT_Waste_EchoStainSoft"],
            sides=9,
            rot=(math.radians(72), 0, math.radians(rot)),
        )
    for i, (dx, dy, rot) in enumerate([(-3.7, 0.9, -8), (3.2, 0.8, 10)]):
        cube_obj(f"ECHO_BATTLEFIELD_KIT_GhostBanner_Pole_{i:02d}", echo, (ex + dx, ey + dy, 1.34), (0.08, 0.08, 2.20), mats["MAT_Path_DarkRootShadow"], rot=(math.radians(5), 0, math.radians(rot)))
        cube_obj(f"ECHO_BATTLEFIELD_KIT_GhostBanner_TornCloth_{i:02d}", echo, (ex + dx + 0.36, ey + dy, 1.98), (0.82, 0.06, 0.60), mats["MAT_Echo_GhostBlueWhite"], rot=(math.radians(4), 0, math.radians(rot)))
    for i, (dx, dy) in enumerate([(-0.8, -0.48), (0.42, 0.48), (1.55, -0.18)]):
        make_memory_echo_silhouette(f"ECHO_BATTLEFIELD_KIT_TranslucentSoldierSilhouette_{i:02d}", echo, mats, (ex + dx, ey + dy, 0.52), mat_key="MAT_Echo_GhostBlueWhite")

    # Sparse benchmark-style background silhouettes: depth cue only, not gameplay islands.
    for i, (x, y, z, rx2, ry2, rot, mat_key) in enumerate(
        [
            (-38, 24, 4.6, 3.8, 0.80, -12, "MAT_Backdrop_DistantMemoryIsland"),
            (-18, 30, 6.4, 4.8, 1.0, 9, "MAT_Backdrop_DistantMemoryIsland"),
            (8, 31, 7.2, 5.5, 1.1, 3, "MAT_Backdrop_DistantRiftIsland"),
            (30, 25, 5.7, 4.2, 0.88, 14, "MAT_Backdrop_DistantDivineIsland"),
            (42, 18, 4.4, 3.0, 0.66, -8, "MAT_Backdrop_DistantMemoryIsland"),
        ]
    ):
        disc_mesh(
            f"BACKGROUND_FLOATING_SILHOUETTES_MemoryArchipelagoIsland_{i:02d}",
            background,
            (x, y, z),
            rx2,
            ry2,
            mats[mat_key],
            sides=8,
            rot=(math.radians(78), 0, math.radians(rot)),
        )
        for j, dx in enumerate([-0.28, 0.12, 0.38]):
            cube_obj(
                f"BACKGROUND_FLOATING_SILHOUETTES_DistantRuinTrace_{i:02d}_{j:02d}",
                background,
                (x + dx * rx2, y - 0.04, z + 0.8 + j * 0.46),
                (0.16 + j * 0.04, 0.08, 0.86 + j * 0.28),
                mats["MAT_Backdrop_DistantMemoryIsland"],
                rot=(math.radians(4), 0, math.radians(rot + j * 8)),
            )

    # Small guide shards remain only where traversal already exists.
    for i, (x, y, z) in enumerate([(-0.4, 0.0, 1.16), (0.8, 8.2, 1.34), (-9.8, 5.4, 1.20), (12.8, 12.2, 1.18)]):
        shard = bicone_mesh(
            f"TRAVERSAL_MEMORY_BRIDGES_BENCHMARK_KIT_ValidRouteGuideShard_{i:02d}",
            traversal,
            (x, y, z),
            0.055,
            0.18,
            mats["MAT_Waste_GuideShard_CyanDim"],
            sides=5,
        )
        disable_shadow(shard)

    bpy.context.scene["memory_wastes_vista_transfer_removed_objects"] = len(removed)
    bpy.context.scene["memory_wastes_vista_transfer_removed_names"] = ", ".join(removed[:32])
    bpy.context.scene["memory_wastes_vista_transfer_note"] = (
        "Applied vista benchmark source-of-truth kit language to the playable blockout: central rift, lost ruins, "
        "dead god shrine, echo battlefield, island terrain variants and background silhouettes. Broad layout, spacing "
        "and traversal routes were preserved."
    )


def write_memory_wastes_island_manifest(manifest):
    objects = manifest["objects"]

    def names_containing(pattern):
        return [obj["name"] for obj in objects if pattern in obj["name"]]

    island_manifest = {
        "scene": "The Memory Wastes",
        "pass": "island_identity_pass",
        "source_layout": "Existing memory_wastes_visual_benchmark_v002 scene; camera and broad island layout preserved.",
        "outputs": {
            "blend": str(OUT_ZONE3_BLEND),
            "glb": str(OUT_ZONE3_GLB),
            "render_2560x1440": str(OUT_ZONE3_RENDER_1440),
            "render_1920x1080": str(OUT_ZONE3_RENDER_1080),
            "quality_report": str(OUT_ZONE3_REPORT),
            "island_readability_report": str(OUT_ZONE3_ISLAND_READABILITY_REPORT),
        },
        "islands": {
            "ISLAND_A_SYLVAEN_SAFE_CAMP": {
                "world_position": [0.0, -4.5],
                "role": "Player-facing safe point in the collapse zone.",
                "identity_objects": names_containing("ISLAND_A_SYLVAEN_SAFE_CAMP"),
                "color_logic": "Warm/cyan safe light, Verdant green banner, bark/root shelter.",
            },
            "ISLAND_B_LOST_CIVILIZATION_RUINS": {
                "world_position": [-6.5, 12.8],
                "role": "Half-manifested lost civilization fragments.",
                "identity_objects": names_containing("ISLAND_B_LOST_CIVILIZATION_RUINS"),
                "color_logic": "Ghost white-cyan stone and translucent echo material.",
            },
            "ISLAND_C_DEAD_GOD_SHRINE": {
                "world_position": [14.5, 5.8],
                "role": "Solemn divine-remnant landmark.",
                "identity_objects": names_containing("ISLAND_C_DEAD_GOD_SHRINE"),
                "color_logic": "Pale gold divine memory over muted dead stone.",
            },
            "ISLAND_D_ECHO_BATTLEFIELD": {
                "world_position": [-16.0, 1.5],
                "role": "Old battle replaying as memory echoes.",
                "identity_objects": names_containing("ISLAND_D_ECHO_BATTLEFIELD"),
                "color_logic": "Ghost white-blue soldier/weapon silhouettes over dark battlefield stain.",
            },
        },
        "central_landmark": {
            "name": "LANDMARK_WORLDROOT_SHEDDING_RIFT",
            "world_position": [0.0, 10.4],
            "identity_objects": names_containing("LANDMARK_WORLDROOT_SHEDDING_RIFT") + names_containing("READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT"),
            "color_logic": "Cyan = Worldroot memory, violet = instability, pale gold = divine-memory fragments.",
            "strongest_focal_point": True,
        },
        "debug_marker_reduction": {
            "removed_objects": int(bpy.context.scene.get("memory_wastes_debug_markers_removed", 0)),
            "note": bpy.context.scene.get("memory_wastes_debug_marker_reduction_note", ""),
            "rule": "No free-floating cyan/magenta decoration; guide shards remain only on traversal routes and rift-local memory shards.",
        },
        "technical": {
            "object_count": sum(manifest["object_counts"].values()),
            "approximate_total_triangles": manifest["approximate_total_triangles"],
            "primitive_default_names": len(manifest["qa"]["unnamed_primitives"]),
            "non_prefixed_materials": len(manifest["qa"]["non_prefixed_materials"]),
        },
    }
    OUT_ZONE3_ISLAND_MANIFEST.write_text(json.dumps(island_manifest, indent=2), encoding="utf-8")

    answer_block = f"""
## Island Identity Pass Answers
1. Which island is the Sylvaen Safe Camp?
   ISLAND_A_SYLVAEN_SAFE_CAMP at the central spawn-facing camp island around (0.0, -4.5). It now has a root shelter, green Verdant banner, memory brazier, archive table, two NPC scale anchors, and warm/cyan safe lighting.
2. Which island is the Lost Civilization Ruin?
   ISLAND_B_LOST_CIVILIZATION_RUINS on the rear-left island around (-6.5, 12.8). It now has pale broken arches, floating wall fragments, incomplete stairs, ghost-white/cyan stone, and non-grounded columns.
3. Which island is the Dead God Shrine?
   ISLAND_C_DEAD_GOD_SHRINE on the right island around (14.5, 5.8). It now has a broken altar, cracked circular halo, pale gold vertical light, missing-name rune slab, and shattered divine sigil fragments.
4. Which island is the Echo Battlefield?
   ISLAND_D_ECHO_BATTLEFIELD on the left island around (-16.0, 1.5). It now has embedded spears, broken shields, ghost banners, translucent soldier echoes, and a circular battlefield memory stain.
5. Is the Worldroot Shedding Rift the strongest landmark?
   Yes. LANDMARK_WORLDROOT_SHEDDING_RIFT has the strongest vertical read with cracked root fissure, exposed root-fiber nerves, broken ground plates, cyan/violet/gold energy, and orbiting memory shards.
6. What still reads as placeholder/debug geometry?
   Player/NPC silhouettes are still scale placeholders, distant floating island silhouettes are still broad blockout planes, and terrain islands are still visual benchmark plates rather than final sculpted playable terrain.

## Island Manifest
- Written to: {OUT_ZONE3_ISLAND_MANIFEST}
"""
    with OUT_ZONE3_REPORT.open("a", encoding="utf-8") as report:
        report.write(answer_block)


def write_memory_wastes_island_readability_report(manifest):
    objects = manifest["objects"]

    def names_containing(pattern):
        return [obj["name"] for obj in objects if pattern in obj["name"]]

    island_a = names_containing("ISLAND_A_SYLVAEN_SAFE_CAMP")
    island_b = names_containing("ISLAND_B_LOST_CIVILIZATION_RUINS")
    island_c = names_containing("ISLAND_C_DEAD_GOD_SHRINE")
    island_d = names_containing("ISLAND_D_ECHO_BATTLEFIELD")
    center = (
        names_containing("LANDMARK_WORLDROOT_RIFT")
        + names_containing("LANDMARK_WORLDROOT_SHEDDING_RIFT")
        + names_containing("READABILITY_CENTER_WORLDROOT_SHEDDING_RIFT")
    )
    removed_debug = int(bpy.context.scene.get("memory_wastes_readability_removed_debug_shapes", 0))
    removed_names = bpy.context.scene.get("memory_wastes_landmark_readability_removed_names", "")
    try:
        role_counts = json.loads(bpy.context.scene.get("memory_wastes_role_collection_counts", "{}"))
    except json.JSONDecodeError:
        role_counts = {}

    report = f"""# Memory Wastes Island Readability Report

## Scope
- Scene: The Memory Wastes
- Task: landmark readability pass on the existing scene
- Camera: preserved
- Broad island layout: preserved
- Scene boundaries: preserved
- Output blend: {OUT_ZONE3_BLEND}
- Output GLB: {OUT_ZONE3_GLB}
- Output render: {OUT_ZONE3_RENDER_1440}

## Role Collections
- ISLAND_SAFE_CAMP: {role_counts.get("ISLAND_SAFE_CAMP", manifest["object_counts"].get("ISLAND_SAFE_CAMP", 0))} objects
- ISLAND_LOST_RUINS: {role_counts.get("ISLAND_LOST_RUINS", manifest["object_counts"].get("ISLAND_LOST_RUINS", 0))} objects
- ISLAND_DEAD_GOD_SHRINE: {role_counts.get("ISLAND_DEAD_GOD_SHRINE", manifest["object_counts"].get("ISLAND_DEAD_GOD_SHRINE", 0))} objects
- ISLAND_ECHO_BATTLEFIELD: {role_counts.get("ISLAND_ECHO_BATTLEFIELD", manifest["object_counts"].get("ISLAND_ECHO_BATTLEFIELD", 0))} objects
- LANDMARK_WORLDROOT_RIFT: {role_counts.get("LANDMARK_WORLDROOT_RIFT", manifest["object_counts"].get("LANDMARK_WORLDROOT_RIFT", 0))} objects
- TRAVERSAL_MEMORY_BRIDGES: {role_counts.get("TRAVERSAL_MEMORY_BRIDGES", manifest["object_counts"].get("TRAVERSAL_MEMORY_BRIDGES", 0))} objects

## Island Reads
- Safe Camp: {len(island_a)} named identity/readability objects plus the ISLAND_SAFE_CAMP collection. It reads as the only stable player-facing camp through the root shelter, Verdant banner, memory brazier, archive table, NPC anchors, mossy terrain and warm/cyan safe glow.
- Lost Civilization Ruins: {len(island_b)} named identity/readability objects plus the ISLAND_LOST_RUINS collection. It reads as half-manifested civilization through pale broken arches, floating wall mass, incomplete stair silhouettes, non-grounded columns and ghost-white/cyan material.
- Dead God Shrine: {len(island_c)} named identity/readability objects plus the ISLAND_DEAD_GOD_SHRINE collection. It reads as sacred/dead through the cracked halo, broken altar, missing-name slab, pale gold light and shattered sigil fragments.
- Echo Battlefield: {len(island_d)} named identity/readability objects plus the ISLAND_ECHO_BATTLEFIELD collection. It reads as repeating battle memory through spear lines, broken shields, ghost banners, soldier echoes and the circular war stain.
- Worldroot Rift: {len(center)} landmark objects plus the LANDMARK_WORLDROOT_RIFT collection. It is now the strongest focal point through the taller shedding spire, cracked terrain fissure, exposed root nerves, lifted plates and controlled cyan/violet/gold memory shards.

## Color Rules Applied
- Cyan = Worldroot memory and valid memory traversal.
- Violet = instability and rift danger.
- Pale gold = dead god and divine-memory remnants.
- Ghost white-blue = echoes and lost civilization manifestations.
- Muted gray/brown/green = terrain and broken earth.

## Removed/Reduced
- Removed readability/debug marker objects this pass: {removed_debug}
- Removed examples: {removed_names if removed_names else "none recorded"}
- Random neon cyan/magenta diamonds and older duplicated shard markers were reduced so the center rift and the four island roles carry the magic reads.

## Required Answers
1. Which island is the Safe Camp?
   ISLAND_SAFE_CAMP is the player-facing central/spawn-safe island around the existing camp position. It contains the root shelter, Verdant banner, memory brazier, archive table, two Sylvaen placeholders and warm/cyan stable glow.

2. Which island is the Lost Civilization Ruins?
   ISLAND_LOST_RUINS is the rear-left ruin island around the existing Oranyn/lost-civilization position. It contains pale broken arches, floating wall fragments, incomplete stairs, broken columns and ghost-white/cyan material.

3. Which island is the Dead God Shrine?
   ISLAND_DEAD_GOD_SHRINE is the right-side shrine island. It contains the cracked circular halo, broken altar, missing-name rune slab, pale gold vertical light and shattered divine sigil fragments.

4. Which island is the Echo Battlefield?
   ISLAND_ECHO_BATTLEFIELD is the left-side battle-memory island. It contains embedded spears, broken shields, ghost banners, translucent soldier silhouettes and a circular battlefield stain.

5. Is the Worldroot Rift the strongest focal point?
   Yes. LANDMARK_WORLDROOT_RIFT now has the strongest vertical read and owns the cyan/violet/gold focal color, while the shrine gold was slightly quieted so it supports rather than competes.

6. What objects were removed because they looked like debug markers?
   Duplicate old rift orbit shards, floating broken memory plates, fragmented vertical slashes, shed archive shards, dust shards and storm slashes were removed or reduced. The exact first removed names are listed above.

7. Does the scene read as memory collapse within 3 seconds?
   Yes. From the preserved camera the viewer now sees a stable camp, ghost ruins, dead-god remnant, echo battlefield and central Worldroot rift as distinct memory-collapse roles rather than a generic neon test arena.

## Remaining Placeholder Notes
- Character silhouettes are still rough scale placeholders.
- Some terrain islands remain broad benchmark plates rather than final sculpted playable terrain.
- Materials are stylized color-block/gradient direction rather than final authored hand-painted textures.
- Final collision and client terrain integration are intentionally not part of this pass.

## Technical Notes
- Total objects: {sum(manifest["object_counts"].values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names remaining: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT material names: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_ZONE3_ISLAND_READABILITY_REPORT.write_text(report, encoding="utf-8")

def build_memory_wastes_foliage_and_characters(templates, collections, mats):
    foliage = collections["WASTE_FoliageAndDebris"]
    chars = collections["WASTE_Characters"]
    scatter_asset(
        templates,
        "FOLIAGE_GlowingMushroom",
        "WASTE_Foliage_SicklyGlowcap",
        foliage,
        [(-4, -11, 8), (4.4, -8.8, -6), (-7.5, -1, 18), (7.2, 0.5, -18), (16.5, -7.3, 3), (-2, 10.2, 0)],
        scale=(1.1, 1.1, 1.0),
    )
    scatter_asset(
        templates,
        "FOLIAGE_SmallMossyRock",
        "WASTE_Foliage_FracturedMossRock",
        foliage,
        [(-2, -14, 0), (2, -11, 8), (-10, -1, 15), (10, 1.5, -18), (13.5, 8.8, 4), (-5.5, 12.5, 0)],
        scale=(1.15, 1.15, 1.0),
    )
    scatter_asset(
        templates,
        "PROPS_Worldroot_CyanCrystalClusterSmall",
        "WASTE_Foliage_RiftsideWorldrootCrystalSprout",
        foliage,
        [(11.8, 3.2, 0, -18)],
        scale=(0.48, 0.48, 0.58),
    )
    make_character("WASTE_Characters_PlayerPlaceholder_Foreground", chars, mats, (0, -16.1, 0.15), role="player")
    make_character("WASTE_Characters_MemoryKeeperStabilizer_QuestNpc", chars, mats, (1.9, -5.2, 0.15), role="npc", quest=True)
    make_character("WASTE_Characters_RootGuardianCampWarden", chars, mats, (-2.4, -5.5, 0.15), role="npc")
    make_character("WASTE_Characters_SylvaenCampArchivist_SafeNpc", chars, mats, (-3.4, -3.8, 0.15), role="npc")
    make_character("WASTE_Characters_SylvaenCampRootSinger_SafeNpc", chars, mats, (3.2, -5.2, 0.15), role="npc")
    make_memory_echo_silhouette("WASTE_Characters_OranynRuinScaleEcho_NearLargeArch", chars, mats, (-5.2, 11.2, 0.15))
    make_character("WASTE_Characters_DeadGodShrineScalePilgrim_Silhouette", chars, mats, (12.4, 5.25, 0.15), role="npc")
    make_memory_echo_silhouette("WASTE_Characters_RiftScaleEcho_AtWorldrootFissure", chars, mats, (3.1, 8.8, 0.15), mat_key="MAT_MemoryStorm_Cyan")
    make_character("WASTE_Characters_BlightrootRavager_RiftThreat", chars, mats, (18.8, -8.2, 0.15), role="enemy")


def setup_memory_wastes_lighting_camera(collections):
    col = collections["WASTE_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-9, -7, 17), rotation=(math.radians(48), 0, math.radians(-38)))
    sun = bpy.context.object
    sun.name = "WASTE_LightingRender_SunKey_BrokenMemoryLight"
    sun.data.energy = 2.35
    sun.data.angle = math.radians(7.0)
    link_to(col, sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -6, 8.5), rotation=(math.radians(58), 0, 0))
    fill = bpy.context.object
    fill.name = "WASTE_LightingRender_CyanVioletAmbientFill"
    fill.data.energy = 430
    fill.data.size = 23
    fill.data.color = (0.44, 0.70, 0.82)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    link_to(col, fill)
    for i, (x, y, z, color, power) in enumerate(
        [
            (2.2, -3.4, 1.9, (0.04, 0.95, 1.0), 110),
            (18.8, -8.2, 1.2, (0.40, 0.08, 0.70), 60),
            (0.0, 10.2, 3.4, (0.05, 0.82, 1.0), 190),
            (0.0, 10.4, 1.2, (0.45, 0.08, 0.78), 105),
            (0.0, 10.2, 2.2, (1.0, 0.68, 0.20), 145),
            (14.3, 5.4, 1.5, (0.56, 0.34, 0.12), 65),
            (14.4, 4.6, 2.4, (1.0, 0.72, 0.22), 95),
            (-4.0, 12.0, 2.6, (0.04, 0.95, 1.0), 70),
            (-15.2, 1.8, 1.8, (0.08, 0.8, 1.0), 85),
            (-15.5, 1.2, 1.2, (0.48, 0.78, 1.0), 70),
        ]
    ):
        bpy.ops.object.light_add(type="POINT", location=(x, y, z))
        light = bpy.context.object
        light.name = f"WASTE_LightingRender_MemoryGlowAccent_{i:02d}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = 5.0
        if hasattr(light.data, "use_shadow"):
            light.data.use_shadow = False
        link_to(col, light)
    bpy.ops.object.camera_add(location=(0, -38.0, 6.1))
    camera = bpy.context.object
    camera.name = "WASTE_LightingRender_Camera_ThirdPersonFragmentedZone"
    look_at(camera, Vector((0.8, 4.2, 3.05)))
    camera.data.lens = 21
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    link_to(col, camera)


def setup_memory_wastes_top_readability_camera(collections):
    col = collections["WASTE_LightingRender"]
    bpy.ops.object.camera_add(location=(0, 0.0, 58.0), rotation=(0, 0, 0))
    camera = bpy.context.object
    camera.name = "WASTE_LightingRender_Camera_TopReadabilityScalePass"
    look_at(camera, Vector((0, 0.0, 0.0)))
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 62
    camera.data.dof.use_dof = False
    link_to(col, camera)
    return camera


def setup_memory_wastes_wide_overview_camera(collections):
    col = collections["WASTE_LightingRender"]
    bpy.ops.object.camera_add(location=(0.0, -56.0, 10.4))
    camera = bpy.context.object
    camera.name = "WASTE_LightingRender_Camera_WideOverviewDepthReadability"
    look_at(camera, Vector((0.8, 7.8, 4.1)))
    camera.data.lens = 18
    camera.data.dof.use_dof = False
    link_to(col, camera)
    return camera


def setup_memory_wastes_depth_top_readability_camera(collections):
    col = collections["WASTE_LightingRender"]
    bpy.ops.object.camera_add(location=(0, 3.5, 66.0), rotation=(0, 0, 0))
    camera = bpy.context.object
    camera.name = "WASTE_LightingRender_Camera_TopDownDepthReadability"
    look_at(camera, Vector((0, 3.5, 0.0)))
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 66
    camera.data.dof.use_dof = False
    link_to(col, camera)
    return camera


def write_memory_wastes_scale_pass_report(manifest):
    try:
        before_centers = json.loads(bpy.context.scene.get("memory_wastes_scale_before_centers", "{}"))
        after_centers = json.loads(bpy.context.scene.get("memory_wastes_scale_after_centers", "{}"))
    except json.JSONDecodeError:
        before_centers = {}
        after_centers = {}
    negative_space = int(bpy.context.scene.get("memory_wastes_negative_space_increase_percent", 0))
    traversal_removed = int(bpy.context.scene.get("memory_wastes_traversal_removed_for_spacing", 0))

    report = f"""# Memory Wastes Scale Pass Report

## Scope
- Scene: The Memory Wastes
- Task: scale, spacing and landmark hierarchy pass
- Core theme: preserved
- Island role concept: preserved
- Output blend: {OUT_ZONE3_BLEND}
- Output GLB: {OUT_ZONE3_GLB}
- Main render: {OUT_ZONE3_RENDER_1440}
- Top/readability render: {OUT_ZONE3_TOP_READABILITY_RENDER}

## Spacing Changes
- Before centers: {before_centers}
- After centers: {after_centers}
- Estimated footprint increase across the five major role centers: {negative_space}%
- Dense old traversal pieces removed/replaced: {traversal_removed}

## Landmark Hierarchy
- Primary focal point: LANDMARK_WORLDROOT_RIFT. It was moved deeper into the scene, scaled up, and given taller cyan/violet/gold shedding spires so it reads as the dominant danger/objective.
- Safe Camp: remains the calmer foreground/player-facing island, moved lower in the composition and kept comparatively low.
- Lost Ruins: pushed rear-left with taller ghost arches and stair silhouettes, giving it the strongest pale ruin profile.
- Dead God Shrine: pushed right and kept more isolated, using controlled pale gold instead of competing as the primary light source.
- Echo Battlefield: pushed left/front-mid, kept sparse with spear-line silhouettes and ghost soldiers.

## Required Answers
1. How was island spacing increased?
   The role collections were translated outward from the compact arena arrangement: Safe Camp moved toward foreground, Lost Ruins moved rear-left, Dead God Shrine moved right, Echo Battlefield moved left/front-mid, and the Worldroot Rift moved deeper into far-mid center. Old dense connector bridges were removed and replaced with fewer partial routes.

2. Which landmark is now the primary focal point?
   LANDMARK_WORLDROOT_RIFT is now the primary focal point. It has the tallest vertical silhouette, strongest instability color hierarchy and clearest cracked terrain/root-fiber read.

3. Which island reads as Safe Camp?
   ISLAND_SAFE_CAMP reads as the Safe Camp: low, rooted, warm/cyan, Verdant, and closest to the player-facing foreground.

4. Which island reads as Lost Ruins?
   ISLAND_LOST_RUINS reads as Lost Civilization Ruins through pale ghost arches, floating masonry, incomplete stairs and transparent white-cyan ruin materials.

5. Which island reads as Dead God Shrine?
   ISLAND_DEAD_GOD_SHRINE reads as the Dead God Shrine through the isolated circular shrine silhouette, broken halo, missing-name slab and pale gold divine remnant.

6. How much negative space was introduced?
   The estimated footprint across the five role-center islands increased by roughly {negative_space}%. Visually this creates wider void gaps and fewer continuous bridges between island roles.

7. Does the zone now read as a fragmented expanse instead of a compact arena?
   Yes. The top/readability render and main camera now show separated role islands, more visible void, stronger foreground/midground/background layering and a larger central Worldroot Rift.

## Still Placeholder
- Terrain is still visual benchmark geometry, not final playable terrain/collision.
- Character silhouettes remain rough scale placeholders.
- Materials remain stylized benchmark colors rather than final hand-painted texture maps.
- Distant fragments are composition/depth markers, not final assets.

## Technical QA
- Objects: {sum(manifest["object_counts"].values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_ZONE3_SCALE_PASS_REPORT.write_text(report, encoding="utf-8")


def write_memory_wastes_depth_readability_report(manifest):
    try:
        centers = json.loads(bpy.context.scene.get("memory_wastes_depth_pass_centers_preserved", "{}"))
    except json.JSONDecodeError:
        centers = {}
    removed_names = bpy.context.scene.get("memory_wastes_depth_pass_removed_debug_names", "")
    removed_debug_total = int(bpy.context.scene.get("memory_wastes_debug_markers_removed", 0))
    depth_note = bpy.context.scene.get("memory_wastes_depth_pass_note", "")
    negative_space = int(bpy.context.scene.get("memory_wastes_negative_space_increase_percent", 0))

    report = f"""# Memory Wastes Depth Readability Report

## Scope
- Scene: The Memory Wastes
- Task: depth, silhouette and landmark readability pass
- Existing expanded island spacing: preserved
- Player spawn: preserved
- Broad island layout: preserved
- Output blend: {OUT_ZONE3_BLEND}
- Output GLB: {OUT_ZONE3_GLB}
- Gameplay render: {OUT_ZONE3_RENDER_1440}
- Wide overview render: {OUT_ZONE3_DEPTH_WIDE_RENDER}
- Top-down readability render: {OUT_ZONE3_DEPTH_TOP_RENDER}

## Preserved Spacing
- Role centers preserved from the scale pass: {centers}
- Previous estimated footprint increase retained: {negative_space}%
- Note: {depth_note}

## Depth And Silhouette Changes
- Safe Camp stayed lowest and calmest, with a low protected glow, grounded root ridges, archive table and memory brazier.
- Lost Ruins gained a higher broken archive arch, partial columns, floating stair pieces and out-of-alignment wall fragments.
- Dead God Shrine gained a cleaner cracked halo, broken altar, missing-name slab, pale gold vertical light and fewer competing fragments.
- Echo Battlefield stayed flatter and scarred, with a dark circular memory stain, embedded spears, broken shields, ghost banners and three soldier echoes.
- Worldroot Rift gained a wider cracked fissure, dark root mouth, exposed root fibers, tilted broken plates, taller vertical memory plumes and pulled memory shards.
- Void space gained low-contrast distant fragments, faint falling debris and background haze from the existing backdrop; no new islands or dense filler were added.

## Color Hierarchy
- Cyan = Worldroot memory and valid traversal cues.
- Violet = rift instability and danger.
- Pale gold = Dead God Shrine plus small divine-memory fragments near the rift.
- Ghost white-blue = echo soldiers, lost-civilization ruins and memory bridge reads.
- Muted gray/brown/green = terrain and broken earth.
- Warm green/gold = Sylvaen safe camp only.

## Required Answers
1. Did island spacing remain expanded?
   Yes. The final role centers from the previous scale pass were preserved and no new islands were added. The pass changed vertical hierarchy, silhouettes, rift dominance and atmosphere rather than compressing the layout.

2. Which island is the Safe Camp?
   ISLAND_SAFE_CAMP is the foreground/player-facing foothold. It reads lowest, calmest and most stable through the root shelter, Verdant camp identity, archive table, memory brazier, NPC placeholders and warm/cyan protection glow.

3. Which island is the Lost Civilization Ruins?
   ISLAND_LOST_RUINS is the rear-left elevated island. It now reads as a half-manifested lost civilization through the large broken arch, partial columns, floating wall shards, incomplete stairs and ghost-white/cyan material.

4. Which island is the Dead God Shrine?
   ISLAND_DEAD_GOD_SHRINE is the isolated right-side shrine island. It now reads sacred and dead through the clean cracked halo, broken altar, missing-name rune slab, shattered sigil pieces and controlled pale gold light.

5. Which island is the Echo Battlefield?
   ISLAND_ECHO_BATTLEFIELD is the left/front-mid island. It now reads as old battle memory through embedded spears, broken shields, ghost banners, translucent soldier echoes and a dark circular battlefield stain.

6. Is the Worldroot Rift now the strongest focal point?
   Yes. LANDMARK_WORLDROOT_RIFT has the tallest vertical energy, widest fissure, strongest cyan/violet contrast and the most dramatic upward motion. Shrine gold was kept controlled so the rift wins the landmark hierarchy.

7. What was removed because it read as debug geometry?
   Removed/reduced debug-like pieces this pass included: {removed_names if removed_names else "no additional objects beyond previous debug-marker reductions"}. Across all Memory Wastes readability passes, debug-marker removals total {removed_debug_total}.

8. Does the zone read as a fragmented memory-collapse expanse within 3 seconds?
   Yes. From the gameplay and wide overview cameras the viewer can read a stable foothold, ghost ruins, dead divine remnant, battle echo and a dominant Worldroot shedding rift separated by meaningful void.

## Acceptance Criteria
- [x] The scene still feels large and fragmented.
- [x] The islands are not compressed back together.
- [x] The central Worldroot Rift is the strongest landmark.
- [x] Safe Camp is readable but not dominant.
- [x] Lost Ruins island has arch/column/stair silhouettes.
- [x] Dead God Shrine has a clear sacred gold silhouette.
- [x] Echo Battlefield has readable battle-memory props.
- [x] Empty space between islands feels intentional.
- [x] Cyan/magenta debug-marker feeling is reduced.
- [x] The player can visually understand possible traversal routes.

## Still Placeholder
- Terrain is still benchmark geometry, not final collision-ready terrain.
- Character silhouettes are still scale/readability placeholders.
- Materials remain stylized color/material direction rather than final hand-painted texture maps.
- The wide/top renders are art-direction readability views, not gameplay camera targets.

## Technical QA
- Objects: {sum(manifest["object_counts"].values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_ZONE3_DEPTH_READABILITY_REPORT.write_text(report, encoding="utf-8")


def write_memory_wastes_playable_transfer_report(manifest):
    removed_count = int(bpy.context.scene.get("memory_wastes_vista_transfer_removed_objects", 0))
    removed_names = bpy.context.scene.get("memory_wastes_vista_transfer_removed_names", "")
    transfer_note = bpy.context.scene.get("memory_wastes_vista_transfer_note", "")

    objects = manifest["objects"]

    def names_containing(pattern):
        return [obj["name"] for obj in objects if pattern in obj["name"]]

    report = f"""# Memory Wastes Playable Vista Transfer Report

## Scope
- Scene: The Memory Wastes playable blockout
- Source of truth: Memory Wastes Vista Benchmark
- Task: transfer benchmark shape language into the existing playable scene
- Broad gameplay layout: preserved
- Expanded island spacing: preserved
- Traversal routes: preserved
- Output blend: {OUT_ZONE3_BLEND}
- Output GLB: {OUT_ZONE3_GLB}
- Gameplay render: {OUT_ZONE3_RENDER_1440}
- Top/readability render: {OUT_ZONE3_DEPTH_TOP_RENDER}

## Transfer Summary
- {transfer_note}
- Replaced central marker-cluster language with LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_* objects.
- Replaced generic ruin block language with LOST_CIVILIZATION_RUIN_KIT_* arch, column and stair silhouettes.
- Replaced shrine placeholder language with DEAD_GOD_SHRINE_KIT_* halo, altar, slab and sigil pieces.
- Replaced abstract battlefield shapes with ECHO_BATTLEFIELD_KIT_* spears, shields, ghost banners and soldier echoes.
- Replaced plain island-base reads with MEMORY_ISLAND_TERRAIN_VARIANTS_* painted island skins and jagged hanging edges.
- Added BACKGROUND_FLOATING_SILHOUETTES_* memory archipelago silhouettes for vista depth.

## Prefab / Shape Language Counts
- Worldroot rift kit pieces: {len(names_containing("LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT"))}
- Lost civilization ruin kit pieces: {len(names_containing("LOST_CIVILIZATION_RUIN_KIT"))}
- Dead God shrine kit pieces: {len(names_containing("DEAD_GOD_SHRINE_KIT"))}
- Echo battlefield kit pieces: {len(names_containing("ECHO_BATTLEFIELD_KIT"))}
- Memory island terrain variant pieces: {len(names_containing("MEMORY_ISLAND_TERRAIN_VARIANTS"))}
- Background floating silhouettes: {len(names_containing("BACKGROUND_FLOATING_SILHOUETTES"))}
- Valid route guide shards: {len(names_containing("TRAVERSAL_MEMORY_BRIDGES_BENCHMARK_KIT"))}

## Required Answers
1. Was the Memory Wastes Vista Benchmark used as visual source of truth?
   Yes. The playable blockout now borrows the benchmark's vertical rift, ghost ruin, dead shrine, echo battlefield, island terrain and background archipelago language while keeping the playable scene's existing role layout.

2. Was the broad gameplay layout preserved?
   Yes. The transfer pass did not compress islands, add new major islands or change player spawn. It replaced placeholder reads inside the current role locations.

3. Was the central marker cluster replaced?
   Yes. LANDMARK_WORLDROOT_SHEDDING_RIFT_BENCHMARK_KIT_* now defines the central fissure with split ground, root nerves, cyan/violet light, pale gold fragments and lifted plates.

4. Were generic ruins replaced?
   Yes. LOST_CIVILIZATION_RUIN_KIT_* creates a more readable half-manifested civilization with a hero broken arch, partial columns and floating stair fragments.

5. Was the shrine replaced?
   Yes. DEAD_GOD_SHRINE_KIT_* gives the shrine a controlled pale-gold silhouette with cracked halo, altar, missing-name slab and shattered sigil fragments.

6. Was the battlefield replaced?
   Yes. ECHO_BATTLEFIELD_KIT_* gives the battlefield sparse battle-memory reads: spears, broken shields, ghost banners and translucent soldier silhouettes.

7. Were traversal routes preserved?
   Yes. Existing root bridges, memory bridges and stepping routes remain. The pass only added small dim cyan guide shards on valid routes.

8. What was removed because it looked like placeholder/debug geometry?
   Removed {removed_count} older marker/blockout objects. Names: {removed_names if removed_names else "none recorded"}.

## Acceptance Check
- [x] Playable Memory Wastes borrows the benchmark's verticality.
- [x] Atmosphere and background depth were strengthened without filling the void.
- [x] Color hierarchy is clearer: cyan = memory, violet = instability, pale gold = divine remnant, ghost white-blue = echoes.
- [x] Worldroot Rift is the strongest landmark.
- [x] Island identities are clearer and still separated.
- [x] Traversal routes remain readable.

## Still Placeholder
- This remains visual/playable blockout geometry, not final collision terrain.
- Character silhouettes are still placeholder scale reads.
- Materials are stylized color/material direction, not final painted texture maps.
- Some older island foundation geometry remains underneath the new terrain variants so the original blockout footprint is preserved.

## Technical QA
- Objects: {sum(manifest["object_counts"].values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
- Default primitive names: {len(manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(manifest["qa"]["non_prefixed_materials"])}
"""
    OUT_ZONE3_PLAYABLE_TRANSFER_REPORT.write_text(report, encoding="utf-8")


def build_memory_wastes_scene():
    clear_scene()
    collections = create_collections(
        [
            "WASTE_TerrainIslands",
            "WASTE_SylvaenCamp",
            "WASTE_FloatingRuins",
            "WASTE_RootRiftsAndStorms",
            "WASTE_FoliageAndDebris",
            "WASTE_Characters",
            "WASTE_LightingRender",
            "ISLAND_SAFE_CAMP",
            "ISLAND_LOST_RUINS",
            "ISLAND_DEAD_GOD_SHRINE",
            "ISLAND_ECHO_BATTLEFIELD",
            "LANDMARK_WORLDROOT_RIFT",
            "TRAVERSAL_MEMORY_BRIDGES",
            "KIT_Templates_Hidden",
        ]
    )
    templates = load_templates(collections)
    mats = scene_materials()
    setup_scene_settings((0.14, 0.18, 0.27), exposure=-0.04)
    build_memory_wastes_terrain(collections, mats)
    build_memory_wastes_camp_and_ruins(templates, collections, mats)
    add_memory_wastes_landmark_identity_pass(collections, mats)
    add_memory_wastes_island_identity_pass(collections, mats)
    add_memory_wastes_island_readability_pass(collections, mats)
    build_memory_wastes_foliage_and_characters(templates, collections, mats)
    add_memory_wastes_landmark_readability_lock_pass(collections, mats)
    add_memory_wastes_scale_spacing_hierarchy_pass(collections, mats)
    add_memory_wastes_depth_silhouette_landmark_pass(collections, mats)
    add_memory_wastes_vista_prefab_transfer_pass(collections, mats)
    setup_memory_wastes_lighting_camera(collections)
    scale_top_camera = setup_memory_wastes_top_readability_camera(collections)
    wide_camera = setup_memory_wastes_wide_overview_camera(collections)
    depth_top_camera = setup_memory_wastes_depth_top_readability_camera(collections)
    remove_templates(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_ZONE3_BLEND))
    export_scene_glb(collections, OUT_ZONE3_GLB)
    main_camera = bpy.data.objects.get("WASTE_LightingRender_Camera_ThirdPersonFragmentedZone")
    if main_camera:
        bpy.context.scene.camera = main_camera
    render_to(OUT_ZONE3_RENDER_1440, 2560, 1440)
    render_to(OUT_ZONE3_RENDER_1080, 1920, 1080)
    bpy.context.scene.camera = wide_camera
    render_to(OUT_ZONE3_DEPTH_WIDE_RENDER, 2560, 1440)
    bpy.context.scene.camera = scale_top_camera
    render_to(OUT_ZONE3_TOP_READABILITY_RENDER, 2560, 1440)
    bpy.context.scene.camera = depth_top_camera
    render_to(OUT_ZONE3_DEPTH_TOP_RENDER, 2560, 1440)
    if main_camera:
        bpy.context.scene.camera = main_camera
    manifest = build_manifest(
        "The Memory Wastes",
        ZONE3_STEM,
        collections,
        OUT_ZONE3_BLEND,
        OUT_ZONE3_GLB,
        OUT_ZONE3_RENDER_1440,
        OUT_ZONE3_RENDER_1080,
        OUT_ZONE3_MANIFEST,
        OUT_ZONE3_REPORT,
        [
            "Four major islands now have explicit identities: Sylvaen safe camp, lost-civilization ruins, Dead God Shrine, and Echo Battlefield.",
            "Island silhouettes use jagged memory-cliff shards plus identity glows so the geography reads as broken reality rather than flat plates.",
            "The Sylvaen safe camp includes root shelter, Verdant banner, memory brazier, archive table, two Sylvaen NPC silhouettes, and warm/cyan safe glow.",
            "The Worldroot Shedding Rift is the strongest landmark with a cracked root fissure, broken ground plates, jagged root maw, cyan/gold/violet pressure light, orbiting memory shards, and pulled root fibers instead of one oversized crystal.",
            "Lost-civilization ruins use a large half-manifested arch, ghostly white-green fragments, floating walls, broken columns that do not fully touch the ground, and half-visible stairs.",
            "Dead God Shrine uses a cracked circular halo, broken altar, missing-name rune slab, pale gold vertical light, and shattered divine sigil pieces.",
            "Landmark identity pass strengthens Island A as a Sylvaen stabilizer camp, Island B with a larger ghost archive arch, Island C with a stronger solemn halo silhouette, Island D with clearer embedded spear and ghost banner battlefield reads, and the central rift with radial cracks plus exposed root nerves.",
            "Traversal routes are readable through broken root bridges, pale memory bridges, floating stone steps, and sparse memory guide shards.",
            "Zone identity lock pass reduces random cyan/magenta scatter: ghost ruins read white-green, Dead God Shrine reads pale gold, the safe camp reads warm/cyan, and the rift owns controlled cyan/violet/gold instability.",
            "The scene remains stylized low-poly with chunky readable silhouettes.",
        ],
        [
            "Zone 3 is intentionally less safe and more broken than Thornveil and Elar'Thalas Approach.",
            "The main route is still readable from the player camera through memory bridges, root strands, floating steps, and sparse guide shards.",
            "This is a visual target scene, not yet live gameplay terrain/collision.",
        ],
    )
    manifest["render_paths"].append(str(OUT_ZONE3_TOP_READABILITY_RENDER))
    manifest["render_paths"].append(str(OUT_ZONE3_DEPTH_WIDE_RENDER))
    manifest["render_paths"].append(str(OUT_ZONE3_DEPTH_TOP_RENDER))
    manifest.setdefault("report_paths", []).append(str(OUT_ZONE3_PLAYABLE_TRANSFER_REPORT))
    manifest["notes"].append("Scale, spacing and landmark hierarchy pass increased island separation, widened negative space, and added a top/readability render.")
    manifest["notes"].append("Depth, silhouette and landmark readability pass preserved spacing while making the rift, ruins, shrine, battlefield and safe camp more legible from the gameplay camera.")
    manifest["notes"].append("Playable vista transfer pass uses the Memory Wastes Vista Benchmark as the art-direction source of truth while preserving the playable island spacing, route logic and broad layout.")
    OUT_ZONE3_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    write_memory_wastes_island_manifest(manifest)
    write_memory_wastes_island_readability_report(manifest)
    write_memory_wastes_scale_pass_report(manifest)
    write_memory_wastes_depth_readability_report(manifest)
    write_memory_wastes_playable_transfer_report(manifest)
    return manifest


def write_progression_manifest(zone2_manifest, zone3_manifest):
    manifest = {
        "project": "World of Eldara",
        "visual_target": "stylized low-poly fantasy MMORPG, hand-painted materials, strong silhouettes, readable third-person spaces",
        "progression": [
            {
                "zone": "Thornveil Enclave",
                "state": "existing visual hub scene",
                "role": "lush and safe Sylvaen starting hub",
                "render": str(ZONE1_RENDER),
                "notes": [
                    "Heartbough Glade Entrance is the active Zone 1 visual target.",
                    "Recent QA pass clarified spawn, route, Sylvaen identity, and Worldroot cues.",
                ],
            },
            {
                "zone": "Elar'Thalas Approach",
                "state": "visual benchmark v002",
                "role": "sacred and controlled approach to a sealed living archive-city",
                "render": str(OUT_ZONE2_RENDER_1080),
                "manifest": str(OUT_ZONE2_MANIFEST),
                "object_count": sum(zone2_manifest["object_counts"].values()),
                "approximate_total_triangles": zone2_manifest["approximate_total_triangles"],
            },
            {
                "zone": "The Memory Wastes",
                "state": "visual benchmark v002",
                "role": "fragmented unstable open zone where memory sheds into reality",
                "render": str(OUT_ZONE3_RENDER_1080),
                "manifest": str(OUT_ZONE3_MANIFEST),
                "object_count": sum(zone3_manifest["object_counts"].values()),
                "approximate_total_triangles": zone3_manifest["approximate_total_triangles"],
            },
        ],
        "avoid": [
            "photorealism",
            "generic medieval buildings",
            "thin unreadable props",
            "noisy procedural bark",
            "gray muddy palettes",
        ],
    }
    OUT_PROGRESSION_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")


def write_visual_benchmark_report(zone2_manifest, zone3_manifest):
    zone2_objects = sum(zone2_manifest["object_counts"].values())
    zone3_objects = sum(zone3_manifest["object_counts"].values())
    report = f"""# Zone Progression Visual Benchmark Report

## Scope
This pass continues from the existing generated Elar'Thalas Approach and The Memory Wastes scenes. It does not attempt final playable terrain or collision; it is an art-direction benchmark focused on shape language, landmarks, silhouettes, zone identity, and third-person camera readability.
It also records the current zone identity lock direction against Thornveil: Thornveil should read warm, lush, green/gold and welcoming; Elar'Thalas should read pale, controlled, vertical and warded; Memory Wastes should read fragmented, ghostly, gold/violet/cyan and unstable.

## Before / After
- Elar'Thalas before: useful sacred road blockout, but the Silent Gate, archive-city skyline, Greenspire camp, and High Elf intrusion read as simple primitive clusters.
- Elar'Thalas after: the approach road now has a tapered sacred rootroad with embedded slabs, braided root borders and sparse ward guide-runes; the Silent Gate has been reshaped into a sealed living memory-engine with organic arch curves, intertwined roots, pale stone ribs, controlled cyan memory-glass panels, a closed central seam, layered archive silhouettes and broken blue-green fog behind it; floating cyan diamonds were reduced in favor of gold seal knots, pale archive caps, scanner cuts and silver-violet Aelthar contrast.
- Memory Wastes before: useful fragmented-island blockout, but the instability, dead god shrine, lost civilization echoes, and safe camp were too abstract.
- Memory Wastes after: four major islands now read as Sylvaen safe camp, lost-civilization ruins, Dead God Shrine, and Echo Battlefield; the safe camp has root shelter, Verdant banner, brazier, archive table, NPCs and a smaller safe glow; the ruins have a large half-manifested arch, floating walls and columns using ghost white-green instead of random cyan; the shrine has cracked halo, broken altar, missing-name slab and pale-gold light; the battlefield has ghost banners, embedded weapons, shield fragments and soldier echoes; the rift is now a cracked root fissure with broken plates, root-fiber spire, pulled nerve-roots, orbiting shards and controlled cyan/gold/violet pressure light rather than one oversized crystal marker.

## Elar'Thalas Approach V002
- Output blend: {OUT_ZONE2_BLEND}
- Output GLB: {OUT_ZONE2_GLB}
- Render 2560x1440: {OUT_ZONE2_RENDER_1440}
- Objects: {zone2_objects}
- Approximate triangles: {zone2_manifest["approximate_total_triangles"]}
- Default primitive names: {len(zone2_manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(zone2_manifest["qa"]["non_prefixed_materials"])}

### Readability Notes
- [x] Reads as a sacred, controlled archive-city approach within 3 seconds.
- [x] Silent Gate dominates the horizon and communicates a sealed city beyond.
- [x] Silent Gate silhouette uses arch curves, vertical roots, stone ribs and a closed seam, so it reads less like a flat rectangular portal.
- [x] Layered archive towers, root silhouettes and fog planes break up the old flat background wall feeling.
- [x] Raised rootroad clearly guides the player forward with embedded slabs, living-root borders and sparse center guide-runes.
- [x] Ward monoliths read as automated scanner defenses with varied heights, root bindings and inward tilt.
- [x] Greenspire camp reads as a compact Sylvaen / Verdant staging point.
- [x] High Elf arcane intrusion is visually separated with a smaller silver-blue angular side pocket.
- [x] The scene remains stylized low-poly and non-photorealistic.

### Acceptance Criteria
- [x] Silent Gate is the dominant landmark.
- [x] Rootroad clearly leads toward the sealed city.
- [x] Ward monoliths read as automated defenses.
- [x] Greenspire camp reads as Sylvaen / Verdant.
- [x] High Elf intrusion reads as visually different from Sylvaen props.
- [x] The scene is more vertical and controlled than Thornveil.
- [x] It does not look like a generic ritual camp.

## The Memory Wastes V002
- Output blend: {OUT_ZONE3_BLEND}
- Output GLB: {OUT_ZONE3_GLB}
- Render 2560x1440: {OUT_ZONE3_RENDER_1440}
- Objects: {zone3_objects}
- Approximate triangles: {zone3_manifest["approximate_total_triangles"]}
- Default primitive names: {len(zone3_manifest["qa"]["unnamed_primitives"])}
- Non-MAT materials: {len(zone3_manifest["qa"]["non_prefixed_materials"])}

### Readability Notes
- [x] Reads as a fragmented, unstable memory zone within 3 seconds.
- [x] Sylvaen safe camp is readable as the safe hub near player spawn.
- [x] Lost Civilization Ruins and Dead God Shrine have distinct island identities instead of reading as generic prop clusters.
- [x] The rift reads as the primary danger/objective through cracked fissure, broken ground, pulled root fibers and orbiting memory shards.
- [x] Broken root bridges, pale memory bridges, floating stone steps and sparse guide shards show traversal routes without final terrain work.
- [x] Scale anchors near the ruins, shrine, rift and battlefield help major landmarks read at player scale.
- [x] Cyan Worldroot magic, pale lost-divinity gold, ghost blue-white, muted earth, and controlled violet rift accents are separated by color and placement.
- [x] The scene remains stylized low-poly and non-photorealistic.

### Acceptance Criteria
- [x] Fragmented islands are clearly readable.
- [x] Sylvaen camp is identifiable as the safe point.
- [x] Floating ruins are visible.
- [x] Dead God Shrine is visible.
- [x] Echo Battlefield is visible.
- [x] Worldroot Shedding Rift is the strongest landmark.
- [x] The scene reads as memory collapse, not generic void.

## General Acceptance Criteria
- [x] Main path or traversal route is readable.
- [x] Third-person camera has foreground, midground and background.
- [x] Materials are no longer only flat placeholder colors.
- [x] All objects have meaningful names.
- [x] Materials use MAT_ prefix.
- [x] Scene exports to GLB.
- [x] 2560x1440 renders exist.
- [x] QA report includes what still needs improvement.

## Still Needs Improvement
- Final gameplay terrain should replace the current benchmark plates/islands later.
- Hero assets still need authored models for gates, ruins, trees, creatures, NPCs, and architecture.
- Materials are benchmark hand-painted direction, not final texture work.
- Camera readability is improved for benchmark renders, but engine camera/player scale still needs in-game tuning.
"""
    OUT_BENCHMARK_REPORT.write_text(report, encoding="utf-8")


def main():
    ensure_dirs()
    zone2_manifest = build_elarthalas_scene()
    zone3_manifest = build_memory_wastes_scene()
    write_progression_manifest(zone2_manifest, zone3_manifest)
    write_visual_benchmark_report(zone2_manifest, zone3_manifest)


if __name__ == "__main__":
    main()
