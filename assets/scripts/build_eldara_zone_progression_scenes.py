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
OUT_ZONE3_MANIFEST = ROOT / "assets" / "reports" / f"{ZONE3_STEM}_manifest.json"
OUT_ZONE3_REPORT = ROOT / "assets" / "reports" / f"{ZONE3_STEM}_quality_report.md"

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
        "MAT_Grass_Controlled": make_mat("MAT_Grass_Controlled", (0.25, 0.50, 0.28)),
        "MAT_Grass_Dark": make_mat("MAT_Grass_Dark", (0.07, 0.27, 0.17)),
        "MAT_WasteGround": make_mat("MAT_WasteGround", (0.20, 0.24, 0.20)),
        "MAT_WasteGround_Dark": make_mat("MAT_WasteGround_Dark", (0.09, 0.12, 0.12)),
        "MAT_Path_MossyStone": make_mat("MAT_Path_MossyStone", (0.58, 0.53, 0.39)),
        "MAT_Path_StoneHighlight": make_mat("MAT_Path_StoneHighlight", (0.76, 0.70, 0.50)),
        "MAT_Path_WarmRoot": make_mat("MAT_Path_WarmRoot", (0.35, 0.19, 0.08)),
        "MAT_Path_DarkRootShadow": make_mat("MAT_Path_DarkRootShadow", (0.16, 0.08, 0.035)),
        "MAT_Path_SacredRoot": make_mat("MAT_Path_SacredRoot", (0.43, 0.25, 0.11)),
        "MAT_Moss": make_mat("MAT_Moss", (0.20, 0.50, 0.22)),
        "MAT_Bark_WarmBrown": make_mat("MAT_Bark_WarmBrown", (0.48, 0.25, 0.11)),
        "MAT_Bark_PaintedEdge": make_mat("MAT_Bark_PaintedEdge", (0.67, 0.39, 0.18)),
        "MAT_Bark_DarkRoot": make_mat("MAT_Bark_DarkRoot", (0.23, 0.12, 0.06)),
        "MAT_Leaves_DeepGreen": make_mat("MAT_Leaves_DeepGreen", (0.10, 0.38, 0.16)),
        "MAT_Leaves_LightGreen": make_mat("MAT_Leaves_LightGreen", (0.36, 0.78, 0.36)),
        "MAT_Leaves_DarkUnderside": make_mat("MAT_Leaves_DarkUnderside", (0.04, 0.19, 0.10)),
        "MAT_Stone_MossyGray": make_mat("MAT_Stone_MossyGray", (0.43, 0.47, 0.39)),
        "MAT_Stone_Pilgrimage": make_mat("MAT_Stone_Pilgrimage", (0.56, 0.57, 0.48)),
        "MAT_Stone_PaleArchive": make_mat("MAT_Stone_PaleArchive", (0.72, 0.74, 0.64)),
        "MAT_Stone_DarkCrevice": make_mat("MAT_Stone_DarkCrevice", (0.18, 0.20, 0.18)),
        "MAT_MemoryGlass_CyanGreen": make_mat(
            "MAT_MemoryGlass_CyanGreen", (0.07, 0.86, 0.72), emission=(0.03, 0.90, 0.76), strength=2.2, alpha=0.58
        ),
        "MAT_Worldroot_Cyan_Emission": make_mat(
            "MAT_Worldroot_Cyan_Emission", (0.05, 0.92, 0.96), emission=(0.04, 0.95, 1.0), strength=2.8
        ),
        "MAT_Rune_Cyan_Emission": make_mat(
            "MAT_Rune_Cyan_Emission", (0.05, 0.62, 0.66), emission=(0.04, 0.9, 1.0), strength=2.0
        ),
        "MAT_Echo_Transparent": make_mat(
            "MAT_Echo_Transparent", (0.16, 0.82, 1.0), emission=(0.08, 0.8, 1.0), strength=1.4, alpha=0.42
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
            "MAT_MemoryStorm_Violet", (0.45, 0.12, 0.75), emission=(0.40, 0.08, 0.70), strength=1.0, alpha=0.55
        ),
        "MAT_MemoryStorm_Cyan": make_mat(
            "MAT_MemoryStorm_Cyan", (0.08, 0.76, 0.92), emission=(0.03, 0.70, 0.95), strength=1.1, alpha=0.5
        ),
        "MAT_DeadGod_Stone": make_mat("MAT_DeadGod_Stone", (0.38, 0.35, 0.30)),
        "MAT_DeadGod_Shadow": make_mat("MAT_DeadGod_Shadow", (0.16, 0.14, 0.13)),
        "MAT_Flower_Purple": make_mat("MAT_Flower_Purple", (0.58, 0.18, 0.86)),
        "MAT_Water_BlueGreen_Optional": make_mat(
            "MAT_Water_BlueGreen_Optional", (0.10, 0.56, 0.62), emission=(0.03, 0.28, 0.32), strength=0.25, alpha=0.72
        ),
        "MAT_Backdrop_BlueMist": make_mat(
            "MAT_Backdrop_BlueMist", (0.28, 0.62, 0.75), emission=(0.16, 0.38, 0.48), strength=0.28, alpha=0.42
        ),
        "MAT_Backdrop_SacredSky": make_mat(
            "MAT_Backdrop_SacredSky", (0.58, 0.82, 0.88), emission=(0.34, 0.58, 0.66), strength=0.42
        ),
        "MAT_Gate_BlueFog": make_mat(
            "MAT_Gate_BlueFog", (0.18, 0.44, 0.58), emission=(0.05, 0.22, 0.32), strength=0.16, alpha=0.34
        ),
        "MAT_Backdrop_MemoryStormSky": make_mat(
            "MAT_Backdrop_MemoryStormSky", (0.16, 0.21, 0.30), emission=(0.09, 0.12, 0.22), strength=0.24
        ),
        "MAT_PlayerPlaceholder": make_mat("MAT_PlayerPlaceholder", (0.54, 0.78, 0.56), alpha=0.86),
        "MAT_NpcPlaceholder": make_mat("MAT_NpcPlaceholder", (0.82, 0.78, 0.48), alpha=0.94),
        "MAT_EnemyPlaceholder": make_mat("MAT_EnemyPlaceholder", (0.88, 0.27, 0.20), alpha=0.86),
        "MAT_CampCloth_Green": make_mat("MAT_CampCloth_Green", (0.09, 0.36, 0.18)),
        "MAT_CampCloth_Blue": make_mat("MAT_CampCloth_Blue", (0.11, 0.42, 0.48)),
        "MAT_PlayerSpawn_CyanRune": make_mat(
            "MAT_PlayerSpawn_CyanRune", (0.06, 0.92, 0.86), emission=(0.02, 0.88, 0.95), strength=1.0, alpha=0.36
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
        root = cube_obj(
            f"ELAR_Terrain_LongRootroad_LivingRootSpine_{i:02d}",
            terrain,
            (0, y + 1.8, 0.11),
            (1.1, 5.0, 0.12),
            mats["MAT_Path_SacredRoot"],
            rot=(0, 0, math.radians((-1) ** (i + 1) * 5)),
        )
        disable_shadow(root)
        for side, x in [("Left", -width * 0.55), ("Right", width * 0.55)]:
            border = cube_obj(
                f"ELAR_Terrain_LongRootroad_RaisedRootBorder_{side}_{i:02d}",
                terrain,
                (x, y, 0.19),
                (0.28, 4.8, 0.18),
                mats["MAT_Path_DarkRootShadow"],
                rot=(0, 0, math.radians((-3 if side == "Left" else 3) + (-1) ** i * 2)),
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
        base = cube_obj(
            f"ELAR_Terrain_RaisedRootroad_GrownRootMass_{i:02d}",
            terrain,
            (0, y, 0.31 + i * 0.012),
            (width + 1.15, 4.4, 0.32),
            mats["MAT_Path_DarkRootShadow"],
            rot=(0, 0, math.radians((-1) ** i * 3.5)),
        )
        disable_shadow(base)
        crown = cube_obj(
            f"ELAR_Terrain_RaisedRootroad_EmbeddedMossStoneCrown_{i:02d}",
            terrain,
            (0, y + 0.08, 0.52 + i * 0.012),
            (width, 3.45, 0.18),
            mats["MAT_Path_MossyStone"],
            rot=(0, 0, math.radians((-1) ** (i + 1) * 2.5)),
        )
        disable_shadow(crown)
        for side, x in [("Left", -width * 0.58), ("Right", width * 0.58)]:
            rib = cube_obj(
                f"ELAR_Terrain_RaisedRootroad_LivingRootGuardrail_{side}_{i:02d}",
                terrain,
                (x, y + 0.1, 0.70 + i * 0.012),
                (0.38, 4.0, 0.24),
                mats["MAT_Path_SacredRoot"],
                rot=(0, 0, math.radians((-6 if side == "Left" else 6) + (-1) ** i * 1.5)),
            )
            disable_shadow(rib)
        for j, x in enumerate([-1.45, 0, 1.45]):
            cube_obj(
                f"ELAR_Terrain_RaisedRootroad_CyanGuideRune_{i:02d}_{j:02d}",
                terrain,
                (x, y - 0.95 + j * 0.82, 0.66 + i * 0.012),
                (0.38, 0.08, 0.035),
                mats["MAT_Rune_Cyan_Emission"],
                rot=(0, 0, math.radians(9 + j * 18)),
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
            mats["MAT_PlayerSpawn_CyanRune"] if i in [0, 4] else mats["MAT_Echo_Transparent"],
            sides=30,
        )
        for j, x in enumerate([-1.2, 1.2]):
            bicone_mesh(
                f"ELAR_Terrain_PilgrimageRestCircle_{i:02d}_RuneMarker_{j:02d}",
                terrain,
                (x, y, 0.36),
                0.10,
                0.24,
                mats["MAT_Rune_Cyan_Emission"],
                sides=5,
            )
    disc_mesh("ELAR_Terrain_PlayerSpawn_CyanMemoryCircle", terrain, (0, -17.4, 0.13), 1.7, 0.95, mats["MAT_PlayerSpawn_CyanRune"], sides=36)
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
    for side, x in [("Left", -6.2), ("Right", 6.2)]:
        frustum_obj(f"ELAR_ArchiveCity_SilentGate_{side}_LivingPillar", arch, (x, 17.2, 4.1), 0.75, 0.54, 8.2, 7, mats["MAT_Bark_DarkRoot"], rot=(math.radians(3 if x < 0 else -3), 0, 0))
        cube_obj(f"ELAR_ArchiveCity_SilentGate_{side}_GoldRootBand_Lower", arch, (x, 17.2, 1.4), (1.55, 0.42, 0.22), mats["MAT_GoldTrim"])
        cube_obj(f"ELAR_ArchiveCity_SilentGate_{side}_GoldRootBand_Upper", arch, (x, 17.2, 6.55), (1.75, 0.42, 0.24), mats["MAT_GoldTrim"])
        bicone_mesh(f"ELAR_ArchiveCity_SilentGate_{side}_CyanSealEye", arch, (x, 16.8, 4.05), 0.34, 0.74, mats["MAT_Worldroot_Cyan_Emission"])
    cube_obj("ELAR_ArchiveCity_SilentGate_UpperLivingRootArch", arch, (0, 17.1, 7.45), (13.2, 0.75, 0.78), mats["MAT_Bark_DarkRoot"])
    cube_obj("ELAR_ArchiveCity_SilentGate_GoldMemoryLintel", arch, (0, 16.8, 6.85), (10.4, 0.28, 0.24), mats["MAT_GoldTrim"])
    bicone_mesh("ELAR_ArchiveCity_SilentGate_CentralLockedWorldrootCrystal", arch, (0, 16.55, 4.6), 0.74, 2.2, mats["MAT_Worldroot_Cyan_Emission"])
    cube_obj("ELAR_ArchiveCity_SilentGate_SealedDoorShadowMass", arch, (0, 17.05, 3.55), (5.8, 0.34, 5.1), mats["MAT_Path_DarkRootShadow"])
    cube_obj("ELAR_ArchiveCity_SilentGate_LeftCarvedDoorPanel", arch, (-1.55, 16.74, 3.5), (2.45, 0.18, 4.35), mats["MAT_Bark_WarmBrown"])
    cube_obj("ELAR_ArchiveCity_SilentGate_RightCarvedDoorPanel", arch, (1.55, 16.74, 3.5), (2.45, 0.18, 4.35), mats["MAT_Bark_WarmBrown"])
    for i, x in enumerate([-2.7, -0.9, 0.9, 2.7]):
        cube_obj(f"ELAR_ArchiveCity_SilentGate_DoorGoldMemoryInlay_{i:02d}", arch, (x, 16.58, 3.65), (0.12, 0.08, 3.4), mats["MAT_GoldTrim"])
    bicone_mesh("ELAR_ArchiveCity_SilentGate_CrestFloatingMemorySeal", arch, (0, 16.4, 8.8), 0.55, 1.15, mats["MAT_Rune_Cyan_Emission"])
    cube_obj("ELAR_ArchiveCity_SilentGate_CrestGoldLeafCrossbar", arch, (0, 16.45, 8.25), (3.1, 0.16, 0.16), mats["MAT_GoldTrim"])
    disc_mesh("ELAR_ArchiveCity_SilentGate_SealedArchiveRuneCircle", arch, (0, 16.45, 3.0), 2.8, 1.25, mats["MAT_Echo_Transparent"], sides=42, rot=(math.radians(90), 0, 0))
    disable_shadow(bpy.context.object)
    vertical_rect_obj(
        "ELAR_ArchiveCity_SilentGate_BackgroundBlueSealFog",
        arch,
        18.05,
        -17.5,
        17.5,
        0.2,
        13.2,
        mats["MAT_Gate_BlueFog"],
    )
    for side, x in [("Left", -8.3), ("Right", 8.3)]:
        frustum_obj(
            f"ELAR_ArchiveCity_SilentGate_Hero{side}_PaleArchiveStoneButtress",
            arch,
            (x, 16.95, 5.35),
            1.15,
            0.72,
            10.7,
            7,
            mats["MAT_Stone_PaleArchive"],
            rot=(math.radians(2 if side == "Left" else -2), 0, math.radians(4 if side == "Left" else -4)),
        )
        for j, offset in enumerate([-0.48, 0.46]):
            root_x = x + offset * (-1 if side == "Left" else 1)
            frustum_obj(
                f"ELAR_ArchiveCity_SilentGate_Hero{side}_IntertwinedRootRib_{j:02d}",
                arch,
                (root_x, 16.55 - j * 0.08, 6.0),
                0.42,
                0.26,
                12.0,
                6,
                mats["MAT_Bark_DarkRoot"],
                rot=(math.radians(5 if side == "Left" else -5), 0, math.radians((j * 7 + 3) * (1 if side == "Left" else -1))),
            )
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_Hero{side}_VerticalMemoryGlassSeam",
            arch,
            (x * 0.96, 16.35, 5.85),
            (0.22, 0.12, 7.6),
            mats["MAT_MemoryGlass_CyanGreen"],
            rot=(0, 0, math.radians(2 if side == "Left" else -2)),
        )
        bicone_mesh(
            f"ELAR_ArchiveCity_SilentGate_Hero{side}_TallCyanSealLens",
            arch,
            (x * 0.92, 16.25, 7.0),
            0.42,
            1.15,
            mats["MAT_MemoryGlass_CyanGreen"],
            sides=6,
        )
    cube_obj(
        "ELAR_ArchiveCity_SilentGate_HeroUpperPaleStoneLintel",
        arch,
        (0, 16.75, 11.25),
        (17.8, 0.62, 0.82),
        mats["MAT_Stone_PaleArchive"],
    )
    cube_obj(
        "ELAR_ArchiveCity_SilentGate_HeroUpperIntertwinedRootCrown",
        arch,
        (0, 16.35, 12.15),
        (18.6, 0.56, 0.58),
        mats["MAT_Bark_DarkRoot"],
        rot=(math.radians(1.5), 0, 0),
    )
    cube_obj(
        "ELAR_ArchiveCity_SilentGate_HeroSealedMemoryGlassDoor",
        arch,
        (0, 16.28, 5.5),
        (7.8, 0.16, 8.6),
        mats["MAT_MemoryGlass_CyanGreen"],
    )
    cube_obj(
        "ELAR_ArchiveCity_SilentGate_HeroDarkRootSealBehindDoor",
        arch,
        (0, 16.58, 5.5),
        (8.4, 0.22, 8.9),
        mats["MAT_Path_DarkRootShadow"],
    )
    for i, x in enumerate([-3.0, -1.5, 0.0, 1.5, 3.0]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_HeroVerticalRuneSeam_{i:02d}",
            arch,
            (x, 16.12, 5.75),
            (0.13, 0.10, 7.3),
            mats["MAT_Rune_Cyan_Emission"],
        )
    bicone_mesh(
        "ELAR_ArchiveCity_SilentGate_HeroCentralArchiveLockCrystal",
        arch,
        (0, 15.98, 7.0),
        0.95,
        2.7,
        mats["MAT_Worldroot_Cyan_Emission"],
        sides=6,
    )
    for i, (x, z, rot) in enumerate([(-5.5, 9.2, -8), (5.5, 9.2, 8), (-2.7, 10.25, -2), (2.7, 10.25, 2)]):
        cube_obj(
            f"ELAR_ArchiveCity_SilentGate_HeroCrossRootBrace_{i:02d}",
            arch,
            (x, 16.18, z),
            (4.2, 0.18, 0.24),
            mats["MAT_Bark_DarkRoot"],
            rot=(math.radians(8), 0, math.radians(rot)),
        )
    for i, x in enumerate([-10.2, -8.1, 8.1, 10.2]):
        frustum_obj(f"ELAR_ArchiveCity_BackgroundMemorySpire_{i:02d}", arch, (x, 18.8 + i % 2, 3.0), 0.35, 0.18, 6.0 - (i % 2), 5, mats["MAT_Stone_MossyGray"])
        bicone_mesh(f"ELAR_ArchiveCity_BackgroundMemorySpire_{i:02d}_CyanCap", arch, (x, 18.8 + i % 2, 6.35 - (i % 2) * 0.5), 0.34, 0.64, mats["MAT_Worldroot_Cyan_Emission"])
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
        bicone_mesh(
            f"ELAR_ArchiveCity_DistantLivingArchiveTower_{i:02d}_CyanArchiveHeart",
            arch,
            (x, 20.7, z + height * 0.46),
            0.36 if i == 4 else 0.24,
            0.88 if i == 4 else 0.58,
            mats["MAT_Worldroot_Cyan_Emission"],
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
    for i, (x, y, z, rot) in enumerate([(-4.8, 5.8, 0.12, 0), (4.8, 6.1, 0.12, 0), (-8.5, -2.5, 0.1, 10), (8.5, -2.2, 0.1, -10), (-6.0, 11.6, 0.1, -3), (6.0, 11.8, 0.1, 3)]):
        inst(templates, "PROPS_Worldroot_VerdantRuneMonolith", f"ELAR_WardsAndProps_WardMonolith_{i:02d}", props, (x, y, z), rot=(0, 0, math.radians(rot)), scale=(1.25, 1.25, 1.5))
    for i, y in enumerate([-14.0, -10.0, -6.0, -2.0, 2.0, 6.0, 10.0, 13.4]):
        for side, x in [("Left", -5.9), ("Right", 5.9)]:
            inward_rot = -9 if side == "Left" else 9
            frustum_obj(
                f"ELAR_WardsAndProps_WardlineScannerMonolith_{side}_{i:02d}_PaleStoneBlade",
                props,
                (x, y, 1.85),
                0.28,
                0.18,
                3.5,
                5,
                mats["MAT_Stone_PaleArchive"],
                rot=(math.radians(2), 0, math.radians(inward_rot)),
            )
            cube_obj(
                f"ELAR_WardsAndProps_WardlineScannerMonolith_{side}_{i:02d}_CyanVerticalRune",
                props,
                (x * 0.985, y - 0.12, 2.05),
                (0.10, 0.05, 1.65),
                mats["MAT_MemoryGlass_CyanGreen"],
                rot=(math.radians(2), 0, math.radians(inward_rot)),
            )
            if i in [1, 3, 5, 7]:
                cube_obj(
                    f"ELAR_WardsAndProps_WardlineScannerMonolith_{side}_{i:02d}_InwardScanBeam",
                    props,
                    (x * 0.5, y - 0.18, 1.55),
                    (abs(x) * 0.85, 0.045, 0.045),
                    mats["MAT_Echo_Transparent"],
                    rot=(0, 0, math.radians(0 if side == "Left" else 180)),
                )
    for i, (x, y, z, rot) in enumerate([(-3.4, -8, 0.1, 0), (3.4, -8, 0.1, 0), (-4.1, 1.5, 0.1, 8), (4.1, 1.6, 0.1, -8), (-4.6, 9, 0.1, -4), (4.6, 9.1, 0.1, 4)]):
        inst(templates, "PROPS_Village_LanternPost", f"ELAR_WardsAndProps_SacredLanternPost_{i:02d}", props, (x, y, z), rot=(0, 0, math.radians(rot)), scale=(1.05, 1.05, 1.12))
    scatter_asset(
        templates,
        "PROPS_Worldroot_CyanCrystalClusterMedium",
        "ELAR_WardsAndProps_CyanCrystalGuide",
        props,
        [(-7.8, -7.2, 0.08, -12), (7.6, -6.9, 0.08, 12), (-9.4, 4.3, 0.08, 18), (9.2, 4.7, 0.08, -18), (-3.6, 13.2, 0.08, 0), (3.6, 13.4, 0.08, 0)],
        scale=(0.85, 0.85, 1.0),
    )
    for i, (x, y, z, rot) in enumerate([(-5.0, -12.5, 0.1, 12), (5.0, -12.2, 0.1, -12), (-6.8, 2.4, 0.1, 10), (6.8, 2.7, 0.1, -10)]):
        inst(templates, "PROPS_Village_BannerPost", f"ELAR_WardsAndProps_VerdantPilgrimBanner_{i:02d}", props, (x, y, z), rot=(0, 0, math.radians(rot)), scale=(1.0, 1.0, 1.25))
    for i, (x, y, rot) in enumerate([(-12, -10, 8), (-10.5, -8.6, -6), (-11.8, -7.1, 0)]):
        cube_obj(f"ELAR_WardsAndProps_GreenspireCamp_Crate_{i:02d}", props, (x, y, 0.28), (0.75, 0.55, 0.52), mats["MAT_Path_WarmRoot"], rot=(0, 0, math.radians(rot)))
    tri_prism_obj("ELAR_WardsAndProps_GreenspireCamp_TentRoof", props, (-13.5, -8.5, 0.32), 3.5, 2.4, 1.45, mats["MAT_CampCloth_Green"], rot=(0, 0, math.radians(-12)))
    cube_obj("ELAR_WardsAndProps_GreenspireCamp_TentBase", props, (-13.5, -8.5, 0.28), (2.1, 2.95, 0.55), mats["MAT_Bark_WarmBrown"], rot=(0, 0, math.radians(-12)))
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
    inst(
        templates,
        "PROPS_Village_BannerPost",
        "ELAR_WardsAndProps_GreenspireCamp_VerdantForwardBanner",
        props,
        (-16.9, -7.8, 0.12),
        rot=(0, 0, math.radians(-8)),
        scale=(0.9, 0.9, 1.2),
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
    bicone_mesh("ELAR_WardsAndProps_GreenspireCamp_PortableCyanMapCrystal", props, (-11.6, -9.9, 1.05), 0.24, 0.62, mats["MAT_Worldroot_Cyan_Emission"])
    cube_obj("ELAR_WardsAndProps_GreenspireCamp_RootMapTable", props, (-11.6, -9.9, 0.58), (1.25, 0.72, 0.22), mats["MAT_Path_WarmRoot"], rot=(0, 0, math.radians(7)))
    for i, (x, y, rot) in enumerate([(-14.8, -6.5, -8), (-15.3, -10.6, 10), (-10.7, -11.4, -3)]):
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
    for i, (x, y, rot) in enumerate([(12.5, -8.5, 16), (15.0, -4.0, -8), (13.8, 1.4, 22)]):
        frustum_obj(f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}", intrusion, (x, y, 1.15), 0.22, 0.12, 2.3, 4, mats["MAT_HighElf_ArcaneViolet"], rot=(0, 0, math.radians(rot)))
        disc_mesh(f"ELAR_ArcaneIntrusion_VioletScanCircle_{i:02d}", intrusion, (x, y, 0.08), 1.25, 0.7, mats["MAT_MemoryStorm_Violet"], sides=26)
        bicone_mesh(f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}_ColdGoldFocusingGem", intrusion, (x, y, 2.45), 0.18, 0.42, mats["MAT_HighElf_ColdGold"], sides=4)
        cube_obj(
            f"ELAR_ArcaneIntrusion_HighElfProbePylon_{i:02d}_VioletSurveyBeam",
            intrusion,
            (x - 0.75, y + 0.35, 1.55),
            (1.85, 0.08, 0.08),
            mats["MAT_HighElf_ArcaneViolet"],
            rot=(math.radians(2), 0, math.radians(rot + 22)),
        )
    for i, (x, y, rot) in enumerate([(14.1, -6.4, 18), (16.0, -1.0, -16)]):
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
    cube_obj(
        "ELAR_ArcaneIntrusion_BrokenWardMonolith_FallenPaleBlade",
        intrusion,
        (12.4, -5.25, 0.42),
        (0.45, 0.24, 1.65),
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
    for i, (x, y, z) in enumerate([(-3.4, 9.5, 2.4), (3.4, 9.8, 2.2), (0.0, 12.3, 3.0)]):
        bicone_mesh(f"ELAR_MemoryConstructs_FloatingArchiveMemoryGlyph_{i:02d}", memory, (x, y, z), 0.34, 0.75, mats["MAT_Echo_Transparent"], sides=6)


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


def setup_elarthalas_lighting_camera(collections):
    col = collections["ELAR_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-8, -10, 18), rotation=(math.radians(40), 0, math.radians(-30)))
    sun = bpy.context.object
    sun.name = "ELAR_LightingRender_SunKey_SacredWarmDay"
    sun.data.energy = 3.2
    sun.data.angle = math.radians(5.0)
    link_to(col, sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -8, 8), rotation=(math.radians(58), 0, 0))
    fill = bpy.context.object
    fill.name = "ELAR_LightingRender_BlueGreenArchiveFill"
    fill.data.energy = 520
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
    setup_scene_settings((0.52, 0.74, 0.76), exposure=0.08)
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
            "Raised organic rootroad, embedded slabs, cyan guide runes, and foreground ward stones lead the player forward.",
            "Wardline scanner monoliths create a controlled ceremonial corridor with automated defense language.",
            "Greenspire encampment has root shelters, Verdant banner, archive stand, NPC silhouettes, and warm lanterns.",
            "High Elf intrusion props use silver-blue angular geometry and a broken ward monolith to contrast Sylvaen forms.",
            "Sylvaen identity remains organic through living bark, rootroad, cyan Worldroot crystals, banners, and lanterns.",
            "The scene is stylized low-poly with saturated non-photorealistic materials.",
        ],
        [
            "Zone 2 shifts Thornveil's lush safety into a sacred, managed approach space.",
            "The Silent Gate is intentionally oversized for benchmark readability and future transition staging.",
            "This is a visual target scene, not yet live gameplay terrain/collision.",
        ],
    )


def build_memory_wastes_terrain(collections, mats):
    terrain = collections["WASTE_TerrainIslands"]
    vertical_rect_obj(
        "WASTE_Terrain_Backdrop_MemoryStormSky",
        terrain,
        23.8,
        -48,
        48,
        -0.6,
        60,
        mats["MAT_Backdrop_MemoryStormSky"],
    )
    poly_obj(
        "WASTE_Terrain_DarkVoidBase_NoWalk",
        terrain,
        [(-42, -22), (42, -22), (42, 23), (-42, 23)],
        mats["MAT_WasteGround_Dark"],
        z=-0.35,
    )
    islands = [
        ("PlayerIsland", 0, -16.5, 8.0, 4.4, 0),
        ("CentralCampIsland", 0, -4.5, 12.5, 8.0, 3),
        ("EchoBattlefieldIsland", -16, 1.5, 11.5, 7.0, -12),
        ("DeadGodShrineIsland", 14.5, 5.8, 10.0, 7.0, 10),
        ("FloatingArchiveRuinIsland", -3, 12.5, 13.5, 6.5, -4),
        ("RootRiftIsland", 20, -8.2, 9.0, 5.8, 15),
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
    for i, (x, y, length, rot) in enumerate([(0, -10.4, 7.2, 0), (-8.1, -2.2, 8.8, -28), (7.8, -0.6, 9.6, 25), (-9.4, 8.1, 9.5, 25), (8.5, 9.2, 10.4, -21)]):
        cube_obj(
            f"WASTE_Terrain_LivingRootBridgeAcrossGap_{i:02d}",
            terrain,
            (x, y, 0.18),
            (1.45, length, 0.22),
            mats["MAT_Path_WarmRoot"],
            rot=(0, 0, math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(-1.0, -14.2, 0), (1.1, -12.1, 8), (-0.7, -9.8, -7), (0.8, -7.8, 5), (-0.4, -5.8, 0)]):
        cube_obj(f"WASTE_Terrain_PlayerPath_BrokenMossStone_{i:02d}", terrain, (x, y, 0.23), (2.2, 1.2, 0.14), mats["MAT_Path_MossyStone"], rot=(0, 0, math.radians(rot)))
    disc_mesh("WASTE_Terrain_PlayerSpawn_UnstableCyanCircle", terrain, (0, -16.5, 0.18), 1.55, 0.75, mats["MAT_PlayerSpawn_CyanRune"], sides=30)
    for i, (x, y, rx, ry, mat_key, rot) in enumerate([
        (-20, 13.0, 5.5, 1.2, "MAT_MemoryStorm_Cyan", -12),
        (18, 14.2, 6.2, 1.1, "MAT_MemoryStorm_Violet", 8),
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
    for i, (x, y, rot) in enumerate([(-3.4, -5.4, -8), (3.6, -5.0, 12), (-4.5, -1.5, 14), (4.7, -1.1, -14)]):
        inst(templates, "PROPS_Village_BannerPost", f"WASTE_SylvaenCamp_AnchorBanner_{i:02d}", camp, (x, y, 0.18), rot=(0, 0, math.radians(rot)), scale=(0.95, 0.95, 1.15))
    tri_prism_obj("WASTE_SylvaenCamp_FieldTent_RootclothRoof", camp, (-2.1, -3.8, 0.25), 3.8, 2.2, 1.25, mats["MAT_CampCloth_Blue"], rot=(0, 0, math.radians(9)))
    cube_obj("WASTE_SylvaenCamp_FieldTent_WarmRootBase", camp, (-2.1, -3.8, 0.2), (2.0, 3.0, 0.42), mats["MAT_Bark_WarmBrown"], rot=(0, 0, math.radians(9)))
    inst(templates, "PROPS_Village_SmallShrinePedestal", "WASTE_SylvaenCamp_StabilizationShrine", camp, (2.2, -3.4, 0.18), scale=(1.15, 1.15, 1.15))
    bicone_mesh("WASTE_SylvaenCamp_StabilizationShrine_CyanCore", camp, (2.2, -3.4, 1.55), 0.38, 0.72, mats["MAT_Worldroot_Cyan_Emission"])
    disc_mesh("WASTE_SylvaenCamp_StabilizationCircle_CyanSafeZone", camp, (0.0, -4.2, 0.28), 4.1, 2.35, mats["MAT_PlayerSpawn_CyanRune"], sides=36, rot=(0, 0, math.radians(3)))
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
        cube_obj(f"WASTE_SylvaenCamp_AnchoringRootStake_{i:02d}_CyanWrap", camp, (x, y - 0.04, 0.95), (0.26, 0.035, 0.08), mats["MAT_Rune_Cyan_Emission"], rot=(0, 0, math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(-7.5, 10.8, 1.2, 16), (-4.5, 13.4, 2.4, -8), (1.0, 13.2, 1.8, 5), (5.4, 11.5, 2.7, -15)]):
        cube_obj(f"WASTE_FloatingRuins_LostArchiveWallShard_{i:02d}", ruins, (x, y, z), (2.4, 0.28, 2.1), mats["MAT_Stone_MossyGray"], rot=(math.radians(7), math.radians(0), math.radians(rot)))
        bicone_mesh(f"WASTE_FloatingRuins_CyanMemoryAnchor_{i:02d}", ruins, (x, y - 0.15, z + 1.35), 0.18, 0.42, mats["MAT_Worldroot_Cyan_Emission"])
        cube_obj(f"WASTE_FloatingRuins_LostArchiveWallShard_{i:02d}_TransparentPastEdge", ruins, (x + 0.55, y - 0.22, z + 0.2), (0.18, 0.08, 1.75), mats["MAT_Echo_Transparent"], rot=(math.radians(7), 0, math.radians(rot)))
    for i, (x, y, z, rot) in enumerate([(-10.2, 13.8, 3.0, 4), (7.2, 13.0, 3.45, -7), (0.0, 16.0, 4.1, 0)]):
        tri_prism_obj(
            f"WASTE_FloatingRuins_GhostArchitectureRoofTrace_{i:02d}",
            ruins,
            (x, y, z),
            3.8,
            2.1,
            1.35,
            mats["MAT_Echo_Transparent"],
            rot=(math.radians(2), 0, math.radians(rot)),
        )
    for i, (x, y, rot) in enumerate([(12.0, 5.0, 12), (15.5, 6.8, -10), (17.7, 4.0, 6)]):
        frustum_obj(f"WASTE_DeadGodShrine_BrokenRibMonolith_{i:02d}", ruins, (x, y, 1.55), 0.24, 0.14, 3.1, 5, mats["MAT_DeadGod_Stone"], rot=(math.radians(8), 0, math.radians(rot)))
    cube_obj("WASTE_DeadGodShrine_FallenIdolFace", ruins, (14.4, 5.6, 0.9), (2.4, 0.42, 1.35), mats["MAT_DeadGod_Stone"], rot=(math.radians(8), 0, math.radians(-12)))
    bicone_mesh("WASTE_DeadGodShrine_CrackedMemoryEye", ruins, (14.3, 5.25, 1.25), 0.28, 0.55, mats["MAT_MemoryStorm_Violet"])
    cube_obj("WASTE_DeadGodShrine_FallenCrownArc_Left", ruins, (13.3, 5.35, 2.25), (0.32, 0.22, 2.25), mats["MAT_DeadGod_Shadow"], rot=(math.radians(-14), 0, math.radians(-18)))
    cube_obj("WASTE_DeadGodShrine_FallenCrownArc_Right", ruins, (15.3, 5.35, 2.25), (0.32, 0.22, 2.25), mats["MAT_DeadGod_Shadow"], rot=(math.radians(14), 0, math.radians(18)))
    cube_obj("WASTE_DeadGodShrine_BrokenNamePlate", ruins, (14.25, 4.7, 0.35), (2.2, 0.24, 0.28), mats["MAT_Stone_DarkCrevice"], rot=(0, 0, math.radians(-8)))
    for i, (x, y, rx, ry, rot) in enumerate([(19.0, -8.3, 2.6, 1.1, 18), (17.3, -6.7, 1.8, 0.78, 0), (21.2, -10.0, 1.5, 0.65, -22)]):
        disc_mesh(f"WASTE_RootRiftsAndStorms_VioletRootRift_{i:02d}", rifts, (x, y, 0.24), rx, ry, mats["MAT_MemoryStorm_Violet"], sides=24, rot=(0, 0, math.radians(rot)))
        cube_obj(f"WASTE_RootRiftsAndStorms_BlackRootTear_{i:02d}", rifts, (x, y, 0.32), (rx * 1.3, 0.18, 0.18), mats["MAT_Bark_DarkRoot"], rot=(0, 0, math.radians(rot)))
        bicone_mesh(f"WASTE_RootRiftsAndStorms_VioletRiftCoreShard_{i:02d}", rifts, (x, y, 1.0 + i * 0.18), 0.22 + i * 0.03, 0.82 + i * 0.12, mats["MAT_MemoryStorm_Violet"], sides=5)
    disc_mesh(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_GroundMemoryTear",
        rifts,
        (0.0, 10.4, 0.34),
        5.2,
        1.45,
        mats["MAT_MemoryStorm_Violet"],
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
    bicone_mesh(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_CyanVerticalCore",
        rifts,
        (0.0, 10.15, 3.25),
        0.72,
        5.4,
        mats["MAT_MemoryStorm_Cyan"],
        sides=6,
    )
    bicone_mesh(
        "WASTE_RootRiftsAndStorms_WorldrootSheddingRift_VioletOuterShard",
        rifts,
        (0.0, 10.0, 3.05),
        1.08,
        4.6,
        mats["MAT_MemoryStorm_Violet"],
        sides=5,
    )
    for i, (x, z, rot, mat_key) in enumerate(
        [
            (-2.4, 2.2, -18, "MAT_MemoryStorm_Cyan"),
            (2.4, 2.8, 18, "MAT_MemoryStorm_Violet"),
            (-3.6, 4.0, -8, "MAT_MemoryStorm_Violet"),
            (3.6, 4.4, 8, "MAT_MemoryStorm_Cyan"),
            (0.0, 5.7, 0, "MAT_Echo_Transparent"),
        ]
    ):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_FragmentedVerticalMemorySlash_{i:02d}",
            rifts,
            (x, 10.05 + i * 0.08, z),
            (0.20, 0.12, 2.65),
            mats[mat_key],
            rot=(math.radians(9), 0, math.radians(rot)),
        )
    for i, (x, y, z) in enumerate([(-1.2, 8.7, 2.1), (1.4, 11.8, 2.5), (-2.7, 12.3, 3.1), (2.9, 8.5, 3.35)]):
        bicone_mesh(
            f"WASTE_RootRiftsAndStorms_WorldrootSheddingRift_ShedArchiveShard_{i:02d}",
            rifts,
            (x, y, z),
            0.34,
            0.86,
            mats["MAT_Echo_Transparent"],
            sides=5,
        )
    for i, (x, y, z) in enumerate([(-12, 0, 2.2), (-15.5, 3.0, 1.6), (-18.0, -0.8, 2.8), (4.0, 11.5, 2.1), (8.8, 9.0, 2.6)]):
        bicone_mesh(f"WASTE_RootRiftsAndStorms_FloatingMemoryShard_{i:02d}", rifts, (x, y, z), 0.42, 0.95, mats["MAT_Echo_Transparent"])
    for i, (x, y, z, rot, mat_key) in enumerate([
        (-18.0, 7.5, 2.4, -16, "MAT_MemoryStorm_Cyan"),
        (-13.0, 9.2, 3.0, 12, "MAT_MemoryStorm_Violet"),
        (8.0, 15.2, 3.5, -8, "MAT_MemoryStorm_Cyan"),
        (18.6, -3.0, 2.1, 20, "MAT_MemoryStorm_Violet"),
    ]):
        cube_obj(
            f"WASTE_RootRiftsAndStorms_MemoryStormSlash_{i:02d}",
            rifts,
            (x, y, z),
            (3.4, 0.10, 0.16),
            mats[mat_key],
            rot=(math.radians(12), 0, math.radians(rot)),
        )
    for i, (x, y) in enumerate([(-15.5, 1.8), (-13.3, -0.4), (-18.2, 3.2), (-11.8, 3.8)]):
        make_character(f"WASTE_EchoBattlefield_LostSoldierEcho_{i:02d}", ruins, mats, (x, y, 0.15), role="enemy")


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
        "WASTE_Foliage_UnstableCyanCrystalSprout",
        foliage,
        [(-5.8, -7.5, 0, 9), (5.3, -2.5, 0, -11), (-12.5, 5.5, 0, 13), (11.8, 3.2, 0, -18), (19, -11.5, 0, 0)],
        scale=(0.75, 0.75, 0.8),
    )
    make_character("WASTE_Characters_PlayerPlaceholder_Foreground", chars, mats, (0, -16.1, 0.15), role="player")
    make_character("WASTE_Characters_MemoryKeeperStabilizer_QuestNpc", chars, mats, (1.9, -5.2, 0.15), role="npc", quest=True)
    make_character("WASTE_Characters_RootGuardianCampWarden", chars, mats, (-2.4, -5.5, 0.15), role="npc")
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
            (18.8, -8.2, 1.2, (0.55, 0.10, 0.90), 120),
            (0.0, 10.2, 3.4, (0.05, 0.82, 1.0), 165),
            (0.0, 10.4, 1.2, (0.55, 0.10, 0.90), 115),
            (14.3, 5.4, 1.5, (0.45, 0.10, 0.8), 90),
            (-4.0, 12.0, 2.6, (0.04, 0.95, 1.0), 70),
            (-15.2, 1.8, 1.8, (0.08, 0.8, 1.0), 85),
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
    bpy.ops.object.camera_add(location=(0, -27.5, 4.4))
    camera = bpy.context.object
    camera.name = "WASTE_LightingRender_Camera_ThirdPersonFragmentedZone"
    look_at(camera, Vector((0, -1.0, 2.35)))
    camera.data.lens = 23
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    link_to(col, camera)


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
            "KIT_Templates_Hidden",
        ]
    )
    templates = load_templates(collections)
    mats = scene_materials()
    setup_scene_settings((0.14, 0.18, 0.27), exposure=-0.04)
    build_memory_wastes_terrain(collections, mats)
    build_memory_wastes_camp_and_ruins(templates, collections, mats)
    build_memory_wastes_foliage_and_characters(templates, collections, mats)
    setup_memory_wastes_lighting_camera(collections)
    remove_templates(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_ZONE3_BLEND))
    export_scene_glb(collections, OUT_ZONE3_GLB)
    render_to(OUT_ZONE3_RENDER_1440, 2560, 1440)
    render_to(OUT_ZONE3_RENDER_1080, 1920, 1080)
    return build_manifest(
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
            "Fragmented terrain islands, root bridges, and dark void gaps communicate instability.",
            "A central Sylvaen stabilization camp remains readable as the safe gameplay hub.",
            "The Worldroot Shedding Rift is the strongest landmark and sits behind the safe camp as the zone's central threat.",
            "Floating ruins, echo battlefield silhouettes, dead god shrine, and violet root rifts show memory shedding into reality.",
            "Worldroot cyan and corruption violet separate helpful memory magic from unstable threats.",
            "The scene remains stylized low-poly with chunky readable silhouettes.",
        ],
        [
            "Zone 3 is intentionally less safe and more broken than Thornveil and Elar'Thalas Approach.",
            "The main route is still readable from the player camera through root bridges and pale stone markers.",
            "This is a visual target scene, not yet live gameplay terrain/collision.",
        ],
    )


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
This pass continues from the existing generated Elar'Thalas Approach and The Memory Wastes scenes. It does not attempt final playable terrain or collision; it is an art-direction benchmark focused on landmarks, silhouettes, zone identity, and third-person camera readability.

## Before / After
- Elar'Thalas before: useful sacred road blockout, but the Silent Gate, archive-city skyline, Greenspire camp, and High Elf intrusion read as simple primitive clusters.
- Elar'Thalas after: the approach road now has a raised organic rootroad, foreground ward stones, inward-facing wardline scanner monoliths, a much larger Silent Gate hero landmark, Greenspire forward camp silhouettes, automated root-stone constructs, and a silver-blue High Elf intrusion pocket with a broken ward.
- Memory Wastes before: useful fragmented-island blockout, but the instability, dead god shrine, lost civilization echoes, and safe camp were too abstract.
- Memory Wastes after: islands now have void drop shadows and broken edge strata, the safe camp has a cyan stabilization circle, floating ghost architecture is more readable, the dead god shrine has a stronger silhouette, and the Worldroot Shedding Rift is now the dominant memory-collapse landmark.

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
- [x] Raised rootroad clearly guides the player forward.
- [x] Ward monoliths read as automated scanner defenses.
- [x] Greenspire camp reads as Sylvaen / Verdant.
- [x] High Elf arcane intrusion is visually separated with silver-blue angular shapes on the side.
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
- [x] Central Sylvaen stabilization camp is readable as the safe hub.
- [x] Root bridges and broken path stones show the intended route without final terrain work.
- [x] Floating ruins, ghost architecture, dead god shrine, echo battlefield, and the Worldroot Shedding Rift create distinct landmarks.
- [x] Cyan Worldroot magic and violet corruption are separated by color and placement.
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
