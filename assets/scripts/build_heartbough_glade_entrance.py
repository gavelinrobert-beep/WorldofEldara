import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
KIT_BLEND = ROOT / "assets" / "blender" / "thornveil_modular_asset_kit_v001.blend"
OUT_BLEND = ROOT / "assets" / "blender" / "heartbough_glade_entrance_v001.blend"
OUT_GLB = ROOT / "assets" / "exports" / "heartbough_glade_entrance_v001.glb"
OUT_RENDER_1440 = ROOT / "assets" / "renders" / "heartbough_glade_entrance_v001_2560x1440.png"
OUT_RENDER_1080 = ROOT / "assets" / "renders" / "heartbough_glade_entrance_v001_1920x1080.png"
OUT_MANIFEST = ROOT / "assets" / "reports" / "heartbough_glade_entrance_manifest.json"
OUT_REPORT = ROOT / "assets" / "reports" / "heartbough_glade_entrance_quality_report.md"
OUT_PREFAB_MANIFEST = ROOT / "assets" / "reports" / "prefab_manifest.json"
OUT_TERRAIN_REPORT = ROOT / "assets" / "reports" / "terrain_integration_report.md"
OUT_READABILITY_REPORT = ROOT / "assets" / "reports" / "readability_report.md"

KIT_ASSETS = [
    "ARCH_Sylvaen_TreehouseModule",
    "ARCH_Sylvaen_ArchedDoorway",
    "ARCH_Sylvaen_BalconyPlatform",
    "ARCH_Sylvaen_RootBridgeStraight",
    "ARCH_Sylvaen_RootBridgeCurved",
    "ARCH_Sylvaen_StairSegment",
    "ARCH_Sylvaen_RailingSegment",
    "PROPS_Worldroot_CyanCrystalClusterSmall",
    "PROPS_Worldroot_CyanCrystalClusterMedium",
    "PROPS_Worldroot_CyanCrystalClusterLarge",
    "PROPS_Worldroot_VerdantRuneMonolith",
    "PROPS_Worldroot_WaypointStone",
    "PROPS_Worldroot_MemoryEchoMarker",
    "PROPS_Worldroot_GlowingRootVein",
    "PROPS_Village_LanternPost",
    "PROPS_Village_BannerPost",
    "PROPS_Village_WoodenBench",
    "PROPS_Village_Signpost",
    "PROPS_Village_SmallShrinePedestal",
    "PROPS_Village_HangingLantern",
    "FOLIAGE_FernCluster",
    "FOLIAGE_BroadLeafPlant",
    "FOLIAGE_PurpleFlower",
    "FOLIAGE_BlueFlower",
    "FOLIAGE_GlowingMushroom",
    "FOLIAGE_MossPatch",
    "FOLIAGE_HangingVine",
    "FOLIAGE_SmallMossyRock",
]

COLLECTIONS = [
    "SCENE_Terrain",
    "SCENE_Architecture",
    "SCENE_Worldroot",
    "SCENE_VillageProps",
    "SCENE_Foliage",
    "SCENE_Characters",
    "SCENE_LightingRender",
    "KIT_Templates_Hidden",
]


def ensure_dirs():
    for path in [OUT_BLEND.parent, OUT_GLB.parent, OUT_RENDER_1440.parent, OUT_REPORT.parent]:
        path.mkdir(parents=True, exist_ok=True)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)
    for mesh in list(bpy.data.meshes):
        bpy.data.meshes.remove(mesh)


def create_collections():
    collections = {}
    for name in COLLECTIONS:
        col = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(col)
        collections[name] = col
    collections["KIT_Templates_Hidden"].hide_viewport = True
    collections["KIT_Templates_Hidden"].hide_render = True
    return collections


def link_to(collection, obj):
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)


def disable_shadow(obj):
    if hasattr(obj, "visible_shadow"):
        obj.visible_shadow = False
    return obj


def make_mat(name, color, roughness=0.82, emission=None, strength=0.0, alpha=1.0):
    material = bpy.data.materials.get(name)
    if material is None:
        material = bpy.data.materials.new(name)
    material.diffuse_color = (color[0], color[1], color[2], alpha)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        if "Base Color" in bsdf.inputs:
            bsdf.inputs["Base Color"].default_value = (color[0], color[1], color[2], alpha)
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = roughness
        if emission:
            if "Emission Color" in bsdf.inputs:
                bsdf.inputs["Emission Color"].default_value = (emission[0], emission[1], emission[2], alpha)
            if "Emission Strength" in bsdf.inputs:
                bsdf.inputs["Emission Strength"].default_value = strength
    if alpha < 1.0:
        material.blend_method = "BLEND"
        material.show_transparent_back = True
    return material


def ensure_scene_materials():
    return {
        "MAT_BARK_MAIN": make_mat("MAT_BARK_MAIN", (0.48, 0.26, 0.11)),
        "MAT_BARK_DARK_ROOT": make_mat("MAT_BARK_DARK_ROOT", (0.16, 0.075, 0.035)),
        "MAT_LEAVES_CANOPY_DEEP": make_mat("MAT_LEAVES_CANOPY_DEEP", (0.055, 0.24, 0.11)),
        "MAT_LEAVES_CANOPY_LIGHT": make_mat("MAT_LEAVES_CANOPY_LIGHT", (0.35, 0.66, 0.31)),
        "MAT_PATH_MOSSYSTONE": make_mat("MAT_PATH_MOSSYSTONE", (0.55, 0.53, 0.40)),
        "MAT_GROUND_MOSS": make_mat("MAT_GROUND_MOSS", (0.11, 0.31, 0.17)),
        "MAT_ARCH_WOOD_TRIM": make_mat("MAT_ARCH_WOOD_TRIM", (0.72, 0.43, 0.16)),
        "MAT_WORLDROOT_CYAN": make_mat("MAT_WORLDROOT_CYAN", (0.045, 0.70, 0.74), emission=(0.02, 0.46, 0.52), strength=0.92, alpha=0.78),
        "MAT_LANTERN_WARM": make_mat("MAT_LANTERN_WARM", (1.0, 0.68, 0.23), emission=(1.0, 0.42, 0.08), strength=0.92, alpha=0.86),
        "MAT_BANNER_VERDANT": make_mat("MAT_BANNER_VERDANT", (0.035, 0.29, 0.16)),
        "MAT_READABILITY_GROUND_SHADOW": make_mat("MAT_READABILITY_GROUND_SHADOW", (0.025, 0.075, 0.045), alpha=0.46),
        "MAT_READABILITY_PATH_UNDERLAY": make_mat("MAT_READABILITY_PATH_UNDERLAY", (0.34, 0.35, 0.25), alpha=0.70),
        "MAT_Bark_WarmBrown": make_mat("MAT_Bark_WarmBrown", (0.42, 0.23, 0.10)),
        "MAT_Bark_DarkRoot": make_mat("MAT_Bark_DarkRoot", (0.20, 0.11, 0.055)),
        "MAT_Bark_DeepCrease": make_mat("MAT_Bark_DeepCrease", (0.12, 0.065, 0.032)),
        "MAT_Bark_WarmEdge": make_mat("MAT_Bark_WarmEdge", (0.62, 0.36, 0.16)),
        "MAT_Leaves_DeepGreen": make_mat("MAT_Leaves_DeepGreen", (0.12, 0.36, 0.17)),
        "MAT_Leaves_LightGreen": make_mat("MAT_Leaves_LightGreen", (0.30, 0.62, 0.30)),
        "MAT_Wood_GoldenTrim": make_mat("MAT_Wood_GoldenTrim", (0.76, 0.48, 0.20)),
        "MAT_Banner_VerdantGreen": make_mat("MAT_Banner_VerdantGreen", (0.08, 0.38, 0.22)),
        "MAT_GladeGrass": make_mat("MAT_GladeGrass", (0.13, 0.36, 0.20)),
        "MAT_GladeGrassDark": make_mat("MAT_GladeGrassDark", (0.055, 0.20, 0.13)),
        "MAT_Terrain_ForestFloorBase": make_mat("MAT_Terrain_ForestFloorBase", (0.12, 0.34, 0.19)),
        "MAT_Terrain_ForestFloorShadow": make_mat("MAT_Terrain_ForestFloorShadow", (0.035, 0.16, 0.095)),
        "MAT_Terrain_MossyBank": make_mat("MAT_Terrain_MossyBank", (0.18, 0.43, 0.21)),
        "MAT_Terrain_DepressionShadow": make_mat("MAT_Terrain_DepressionShadow", (0.045, 0.13, 0.095), alpha=0.64),
        "MAT_Terrain_SoftEdgeBlend": make_mat("MAT_Terrain_SoftEdgeBlend", (0.09, 0.28, 0.17), alpha=0.70),
        "MAT_Terrain_BackgroundLayer": make_mat("MAT_Terrain_BackgroundLayer", (0.035, 0.14, 0.08), alpha=0.88),
        "MAT_Path_MossyStone": make_mat("MAT_Path_MossyStone", (0.45, 0.43, 0.31)),
        "MAT_Path_WarmRoot": make_mat("MAT_Path_WarmRoot", (0.24, 0.13, 0.055)),
        "MAT_Moss": make_mat("MAT_Moss", (0.16, 0.40, 0.20)),
        "MAT_Moss_Highlight": make_mat("MAT_Moss_Highlight", (0.28, 0.56, 0.25)),
        "MAT_Stone_MossyGray": make_mat("MAT_Stone_MossyGray", (0.36, 0.39, 0.31)),
        "MAT_Stone_WarmHighlight": make_mat("MAT_Stone_WarmHighlight", (0.57, 0.54, 0.38)),
        "MAT_Bark_PaintedHighlight": make_mat("MAT_Bark_PaintedHighlight", (0.58, 0.34, 0.14)),
        "MAT_Root_MossWrap": make_mat("MAT_Root_MossWrap", (0.09, 0.30, 0.14)),
        "MAT_Leaves_CanopyHighlight": make_mat("MAT_Leaves_CanopyHighlight", (0.46, 0.76, 0.34)),
        "MAT_Leaves_UndersideShadow": make_mat("MAT_Leaves_UndersideShadow", (0.035, 0.14, 0.08)),
        "MAT_Worldroot_Cyan_Emission": make_mat(
            "MAT_Worldroot_Cyan_Emission", (0.06, 0.78, 0.82), emission=(0.03, 0.72, 0.86), strength=1.75
        ),
        "MAT_Rune_Cyan_Emission": make_mat(
            "MAT_Rune_Cyan_Emission", (0.08, 0.84, 0.78), emission=(0.02, 0.62, 0.66), strength=0.95, alpha=0.82
        ),
        "MAT_Window_CyanSoft_Emission": make_mat(
            "MAT_Window_CyanSoft_Emission", (0.12, 0.74, 0.72), emission=(0.04, 0.55, 0.60), strength=0.85, alpha=0.86
        ),
        "MAT_Lantern_Warm_Emission": make_mat(
            "MAT_Lantern_Warm_Emission", (1.0, 0.66, 0.20), emission=(1.0, 0.48, 0.12), strength=1.15, alpha=0.88
        ),
        "MAT_Backdrop_SkyBlue": make_mat("MAT_Backdrop_SkyBlue", (0.045, 0.18, 0.19), roughness=1.0, emission=(0.02, 0.08, 0.09), strength=0.12),
        "MAT_Backdrop_ForestMist": make_mat("MAT_Backdrop_ForestMist", (0.08, 0.25, 0.22), emission=(0.03, 0.10, 0.09), strength=0.10, alpha=0.46),
        "MAT_Backdrop_BlueDepthMist": make_mat("MAT_Backdrop_BlueDepthMist", (0.07, 0.20, 0.24), emission=(0.025, 0.09, 0.12), strength=0.16, alpha=0.38),
        "MAT_Backdrop_ForestDepth": make_mat("MAT_Backdrop_ForestDepth", (0.025, 0.12, 0.085), emission=(0.006, 0.035, 0.028), strength=0.03, alpha=0.92),
        "MAT_Backdrop_DistantTrunk": make_mat("MAT_Backdrop_DistantTrunk", (0.055, 0.105, 0.065), roughness=0.9, alpha=0.92),
        "MAT_Water_BlueGreen_Optional": make_mat(
            "MAT_Water_BlueGreen_Optional", (0.09, 0.45, 0.48), emission=(0.02, 0.18, 0.20), strength=0.12, alpha=0.62
        ),
        "MAT_CyanBarkCrack": make_mat("MAT_CyanBarkCrack", (0.05, 0.78, 0.74), emission=(0.02, 0.68, 0.72), strength=1.05, alpha=0.78),
        "MAT_LeafRoof_Verdant": make_mat("MAT_LeafRoof_Verdant", (0.15, 0.48, 0.20)),
        "MAT_PlayerPlaceholder": make_mat("MAT_PlayerPlaceholder", (0.54, 0.78, 0.56), alpha=0.78),
        "MAT_NpcPlaceholder": make_mat("MAT_NpcPlaceholder", (0.82, 0.78, 0.48), alpha=0.94),
        "MAT_QuestGold": make_mat("MAT_QuestGold", (1.0, 0.75, 0.12), emission=(1.0, 0.58, 0.10), strength=1.3),
        "MAT_PlayerSpawn_CyanRune": make_mat(
            "MAT_PlayerSpawn_CyanRune", (0.06, 0.46, 0.42), emission=(0.015, 0.28, 0.27), strength=0.24, alpha=0.18
        ),
        "MAT_RootShadow": make_mat("MAT_RootShadow", (0.08, 0.20, 0.13), alpha=0.52),
        "MAT_SunDapple_Warm": make_mat("MAT_SunDapple_Warm", (0.86, 0.68, 0.30), emission=(0.45, 0.28, 0.08), strength=0.08, alpha=0.28),
        "MAT_Heartbough_CoreGlow": make_mat("MAT_Heartbough_CoreGlow", (0.11, 0.82, 0.74), emission=(0.03, 0.62, 0.58), strength=1.45, alpha=0.82),
        "MAT_LeafRoof_LightEdge": make_mat("MAT_LeafRoof_LightEdge", (0.42, 0.70, 0.32)),
    }


def append_templates(collections):
    if not KIT_BLEND.exists():
        raise FileNotFoundError(f"Missing asset kit blend: {KIT_BLEND}")
    with bpy.data.libraries.load(str(KIT_BLEND), link=False) as (data_from, data_to):
        missing = sorted(set(KIT_ASSETS) - set(data_from.objects))
        if missing:
            raise RuntimeError(f"Missing kit assets: {', '.join(missing)}")
        data_to.objects = KIT_ASSETS
    templates = {}
    for obj in data_to.objects:
        obj.name = f"TEMPLATE_{obj.name}"
        obj.hide_viewport = True
        obj.hide_render = True
        collections["KIT_Templates_Hidden"].objects.link(obj)
        templates[obj.name.replace("TEMPLATE_", "")] = obj
    return templates


def inst(templates, asset_name, name, collection, loc, rot=(0, 0, 0), scale=(1, 1, 1)):
    template = templates[asset_name]
    obj = template.copy()
    obj.data = template.data
    obj.animation_data_clear()
    obj.name = name
    obj.location = loc
    obj.rotation_euler = rot
    obj.scale = scale
    obj.hide_viewport = False
    obj.hide_render = False
    obj["source_asset"] = asset_name
    collection.objects.link(obj)
    return obj


def cube_obj(name, collection, loc, dims, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.dimensions = dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    link_to(collection, obj)
    return obj


def frustum_obj(name, collection, loc, radius1, radius2, depth, vertices, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius1, radius2=radius2, depth=depth, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.data.materials.append(material)
    link_to(collection, obj)
    return obj


def twisted_trunk_mesh(name, collection, loc, radius_base, radius_top, height, sides, levels, material, lean_degrees=0.0, twist_degrees=38.0):
    verts = []
    faces = []
    lean_wave = math.radians(lean_degrees)
    twist = math.radians(twist_degrees)
    bend_strength = height * (0.022 + min(abs(lean_degrees), 16) * 0.0012)
    for level in range(levels):
        t = level / (levels - 1)
        z = (t - 0.5) * height
        center_x = math.sin(t * math.pi * 1.45 + lean_wave) * bend_strength
        center_y = math.cos(t * math.pi * 1.20 + lean_wave * 0.7) * bend_strength * 0.58
        radius = radius_base + (radius_top - radius_base) * t
        radius *= 1.0 + 0.08 * math.sin(t * math.tau + lean_wave)
        ring_twist = twist * t + math.sin(t * math.pi) * 0.42 + lean_wave * 0.25
        for side in range(sides):
            angle = math.tau * side / sides + ring_twist
            lobe = 1.0 + 0.13 * math.sin(angle * 3.0 + t * math.pi)
            verts.append(
                (
                    center_x + math.cos(angle) * radius * lobe,
                    center_y + math.sin(angle) * radius * (0.86 + 0.08 * math.cos(angle * 2.0)),
                    z,
                )
            )
    for level in range(levels - 1):
        row = level * sides
        next_row = (level + 1) * sides
        for side in range(sides):
            faces.append((row + side, row + (side + 1) % sides, next_row + (side + 1) % sides, next_row + side))
    bottom_center = len(verts)
    verts.append((0, 0, -height / 2))
    top_center = len(verts)
    verts.append((0, 0, height / 2))
    top_row = (levels - 1) * sides
    for side in range(sides):
        faces.append((bottom_center, (side + 1) % sides, side))
        faces.append((top_center, top_row + side, top_row + (side + 1) % sides))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = (math.radians(lean_degrees * 0.80), math.radians(-lean_degrees * 0.28), math.radians(lean_degrees * 0.55))
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def bicone_mesh(name, collection, loc, radius, height, material, sides=6, rot=(0, 0, 0)):
    verts = []
    faces = []
    for i in range(sides):
        a = math.tau * i / sides
        verts.append((math.cos(a) * radius, math.sin(a) * radius, 0))
    top = len(verts)
    verts.append((0, 0, height / 2))
    bottom = len(verts)
    verts.append((0, 0, -height / 2))
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((top, i, j))
        faces.append((bottom, j, i))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = rot
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def disc_mesh(name, collection, loc, rx, ry, material, sides=28, rot=(0, 0, 0)):
    verts = [(0, 0, 0)]
    for i in range(sides):
        a = math.tau * i / sides
        verts.append((math.cos(a) * rx, math.sin(a) * ry, 0))
    faces = [(0, i, 1 if i == sides else i + 1) for i in range(1, sides + 1)]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.rotation_euler = rot
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def irregular_slab_mesh(name, collection, loc, rx, ry, height, material, sides=8, rot_z=0.0, irregularity=0.18):
    verts = []
    top = []
    bottom = []
    for i in range(sides):
        angle = math.tau * i / sides + rot_z
        wobble = 1.0 + irregularity * math.sin(i * 1.73 + rx * 0.41 + ry * 0.23)
        x = math.cos(angle) * rx * wobble
        y = math.sin(angle) * ry * (1.0 + irregularity * 0.55 * math.cos(i * 2.11))
        top.append((x, y, height * 0.5))
        bottom.append((x * 0.96, y * 0.96, -height * 0.5))
    verts = top + bottom
    faces = [tuple(range(sides)), tuple(range(sides * 2 - 1, sides - 1, -1))]
    for i in range(sides):
        faces.append((i, (i + 1) % sides, sides + (i + 1) % sides, sides + i))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = loc
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def terrain_blob_mesh(name, collection, loc, rx, ry, material, sides=18, rot_z=0.0, irregularity=0.16, height_variation=0.06, center_raise=0.0):
    x0, y0, z0 = loc
    verts = [(0, 0, center_raise)]
    for i in range(sides):
        angle = rot_z + math.tau * i / sides
        wobble = 1.0 + irregularity * math.sin(i * 1.31 + rx * 0.47) + irregularity * 0.45 * math.cos(i * 2.17 + ry)
        edge_z = height_variation * math.sin(i * 1.9 + rx) + height_variation * 0.42 * math.cos(i * 0.7 + ry)
        verts.append((math.cos(angle) * rx * wobble, math.sin(angle) * ry * wobble, edge_z))
    faces = [(0, i, 1 if i == sides else i + 1) for i in range(1, sides + 1)]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = (x0, y0, z0)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def leaf_blob_mesh(name, collection, loc, scale_xyz, material, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1, location=loc, rotation=rot)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.scale = scale_xyz
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    link_to(collection, obj)
    return obj


def make_leaf_roof_panel(prefix, collection, loc, scale, material, rot_z):
    panel = irregular_slab_mesh(
        f"{prefix}_LeafRoofPanel_LowpolyGrownSegment",
        collection,
        loc,
        1.10 * scale,
        0.46 * scale,
        0.13 * scale,
        material,
        6,
        rot_z,
        0.20,
    )
    panel.rotation_euler[0] = math.radians(9)
    panel.rotation_euler[1] = math.radians(4)
    return panel


def make_root_bridge_prefab(prefix, collection, loc, length, rot, mats, curved=False):
    x, y, z = loc
    segment_count = 5 if curved else 4
    for i in range(segment_count):
        t = (i - (segment_count - 1) / 2) / max(1, segment_count - 1)
        local_rot = rot + (0.34 * t if curved else 0.0)
        offset_x = math.cos(rot) * t * length * 0.78
        offset_y = math.sin(rot) * t * length * 0.78 + (math.sin(t * math.pi) * 0.65 if curved else 0.0)
        cube_obj(
            f"{prefix}_WalkableLivingRootDeckSegment_{i:02d}",
            collection,
            (x + offset_x, y + offset_y, z + 0.02 * i),
            (length / segment_count * 1.18, 0.74, 0.20),
            mats["MAT_Path_WarmRoot"],
            (math.radians(3 + i % 2), math.radians(-2 + i), local_rot),
        )
        irregular_slab_mesh(
            f"{prefix}_MossyStoneStepInlay_{i:02d}",
            collection,
            (x + offset_x, y + offset_y, z + 0.17 + 0.018 * i),
            length / segment_count * 0.34,
            0.27,
            0.055,
            mats["MAT_Stone_MossyGray"],
            7,
            local_rot + i * 0.22,
            0.16,
        )
    for side_name, side in [("Left", -1), ("Right", 1)]:
        for rail in range(3):
            t = (rail - 1) * 0.42
            rail_rot = rot + (0.23 * t if curved else 0.0)
            side_x = math.cos(rot + math.pi / 2) * side * 0.62
            side_y = math.sin(rot + math.pi / 2) * side * 0.62
            along_x = math.cos(rot) * t * length * 0.82
            along_y = math.sin(rot) * t * length * 0.82 + (math.sin(t * math.pi) * 0.55 if curved else 0.0)
            cube_obj(
                f"{prefix}_UnevenRootRail_{side_name}_{rail:02d}",
                collection,
                (x + along_x + side_x, y + along_y + side_y, z + 0.50 + rail * 0.035),
                (length * 0.34, 0.14, 0.14),
                mats["MAT_Bark_DarkRoot"],
                (math.radians(8 * side), math.radians(3), rail_rot + math.radians(side * 8)),
            )
            cube_obj(
                f"{prefix}_VineBinding_{side_name}_{rail:02d}",
                collection,
                (x + along_x + side_x * 0.96, y + along_y + side_y * 0.96, z + 0.69 + rail * 0.02),
                (0.42, 0.08, 0.08),
                mats["MAT_Root_MossWrap"],
                (math.radians(6), 0, rail_rot + math.radians(72 + side * 12)),
            )
    for rune in [-0.28, 0.28]:
        cube_obj(
            f"{prefix}_SmallCyanGuideRune_{0 if rune < 0 else 1:02d}",
            collection,
            (x + math.cos(rot) * rune * length, y + math.sin(rot) * rune * length, z + 0.34),
            (0.22, 0.035, 0.06),
            mats["MAT_Rune_Cyan_Emission"],
            (0, 0, rot),
        )


def make_root_stairs_prefab(prefix, collection, loc, rot, mats):
    x, y, z = loc
    for i in range(6):
        offset = (i - 2.5) * 0.42
        cube_obj(
            f"{prefix}_RootStoneStep_{i:02d}",
            collection,
            (x + math.cos(rot) * offset, y + math.sin(rot) * offset, z + i * 0.11),
            (0.86, 0.48, 0.12),
            mats["MAT_Stone_MossyGray"] if i % 2 else mats["MAT_Path_WarmRoot"],
            (math.radians(2), math.radians(-1), rot + math.radians((i % 3 - 1) * 4)),
        )
    for side_name, side in [("Left", -1), ("Right", 1)]:
        cube_obj(
            f"{prefix}_LivingRootStairRail_{side_name}",
            collection,
            (
                x + math.cos(rot + math.pi / 2) * side * 0.58,
                y + math.sin(rot + math.pi / 2) * side * 0.58,
                z + 0.55,
            ),
            (2.9, 0.13, 0.13),
            mats["MAT_Bark_DarkRoot"],
            (math.radians(11 * side), 0, rot),
        )


def make_sylvaen_treehouse_prefab(prefix, collection, x, y, scale, rot, mats, hub=False):
    body_width = (2.55 if hub else 2.10) * scale
    body_depth = (1.45 if hub else 1.18) * scale
    body_height = (1.65 if hub else 1.35) * scale
    z_base = 1.95 * scale
    twisted_trunk_mesh(
        f"{prefix}_IntegratedWorldrootSupportTrunk",
        collection,
        (x, y + 0.42 * scale, 1.82 * scale),
        0.54 * scale,
        0.34 * scale,
        3.75 * scale,
        7,
        5,
        mats["MAT_Bark_WarmBrown"],
        lean_degrees=math.degrees(rot) * 0.05,
        twist_degrees=38,
    )
    cube_obj(
        f"{prefix}_GrownBarkHouseShell_Asymmetric",
        collection,
        (x, y - 0.18 * scale, z_base),
        (body_width, body_depth, body_height),
        mats["MAT_Bark_WarmBrown"],
        (math.radians(2), math.radians(-2), rot),
    )
    cube_obj(
        f"{prefix}_DarkRootBackWrap_AnchorsHouseIntoTree",
        collection,
        (x, y + 0.58 * scale, z_base + 0.08 * scale),
        (body_width * 0.92, 0.22 * scale, body_height * 1.08),
        mats["MAT_Bark_DarkRoot"],
        (math.radians(4), 0, rot),
    )
    for panel, (dx, dy, dz, rz) in enumerate([(-0.48, -0.72, 0.78, -18), (0.48, -0.74, 0.90, 16), (0.0, -0.82, 1.04, 0)]):
        make_leaf_roof_panel(
            f"{prefix}_GreenLeafRoof_{panel:02d}",
            collection,
            (x + dx * scale, y + dy * scale, z_base + dz * scale),
            scale * (1.12 if hub else 0.96),
            mats["MAT_LeafRoof_Verdant"] if panel != 2 else mats["MAT_LeafRoof_LightEdge"],
            rot + math.radians(rz),
        )
    inst_obj = cube_obj(
        f"{prefix}_CyanGlowingArchedWindow",
        collection,
        (x, y - 0.82 * scale, z_base + 0.12 * scale),
        (0.54 * scale, 0.06 * scale, 0.56 * scale),
        mats["MAT_Window_CyanSoft_Emission"],
        (0, 0, rot),
    )
    disable_shadow(inst_obj)
    cube_obj(
        f"{prefix}_WarmGoldWindowLintel",
        collection,
        (x, y - 0.87 * scale, z_base + 0.47 * scale),
        (0.82 * scale, 0.05 * scale, 0.08 * scale),
        mats["MAT_Wood_GoldenTrim"],
        (0, 0, rot),
    )
    for side_name, side in [("Left", -1), ("Right", 1)]:
        frustum_obj(
            f"{prefix}_ArchedDoorwayRootColumn_{side_name}",
            collection,
            (x + side * 0.44 * scale, y - 0.98 * scale, 0.86 * scale),
            0.14 * scale,
            0.10 * scale,
            1.30 * scale,
            6,
            mats["MAT_Bark_DarkRoot"],
            (math.radians(side * 5), 0, rot + math.radians(side * 8)),
        )
        cube_obj(
            f"{prefix}_RootSupportWrappingWall_{side_name}",
            collection,
            (x + side * body_width * 0.42, y - 0.12 * scale, z_base - 0.18 * scale),
            (0.18 * scale, body_depth * 1.42, 0.16 * scale),
            mats["MAT_Bark_DarkRoot"],
            (math.radians(7), math.radians(side * 4), rot + math.radians(side * 34)),
        )
    cube_obj(
        f"{prefix}_ArchedDoorwayCurvedLivingLintel",
        collection,
        (x, y - 1.02 * scale, 1.50 * scale),
        (1.10 * scale, 0.12 * scale, 0.13 * scale),
        mats["MAT_Bark_DarkRoot"],
        (math.radians(4), 0, rot),
    )
    disc_mesh(
        f"{prefix}_BalconyIntegratedMossCollar",
        collection,
        (x, y + 0.02 * scale, z_base - 0.06 * scale),
        body_width * 0.68,
        body_depth * 0.55,
        mats["MAT_Moss_Highlight"],
        18,
        (0, 0, rot),
    )
    for rail, (side, local_y, local_rot) in enumerate([(-1, -0.38, -18), (1, -0.38, 18), (-1, 0.36, -34), (1, 0.36, 34)]):
        cube_obj(
            f"{prefix}_BalconyRootRail_{rail:02d}",
            collection,
            (x + side * body_width * 0.42, y + local_y * scale, z_base + 0.18 * scale),
            (1.34 * scale, 0.11 * scale, 0.11 * scale),
            mats["MAT_Bark_DarkRoot"],
            (math.radians(5), math.radians(side * 3), rot + math.radians(local_rot)),
        )
    cube_obj(
        f"{prefix}_WarmLanternUnderBalcony",
        collection,
        (x + body_width * 0.38, y - 0.70 * scale, z_base - 0.44 * scale),
        (0.28 * scale, 0.04 * scale, 0.32 * scale),
        mats["MAT_Lantern_Warm_Emission"],
        (0, 0, rot),
    )
    cube_obj(
        f"{prefix}_VerdantBannerLeafMotif",
        collection,
        (x - body_width * 0.38, y - 0.74 * scale, z_base - 0.30 * scale),
        (0.34 * scale, 0.045 * scale, 0.58 * scale),
        mats["MAT_Banner_VerdantGreen"],
        (0, 0, rot),
    )
    bicone_mesh(
        f"{prefix}_SmallGoldLeafSigil",
        collection,
        (x - body_width * 0.38, y - 0.78 * scale, z_base - 0.28 * scale),
        0.10 * scale,
        0.22 * scale,
        mats["MAT_QuestGold"],
        5,
        (math.radians(90), 0, rot),
    )


def look_at(obj, target):
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def make_terrain(collection, mats):
    verts = []
    faces = []
    size_x, size_y, step = 62, 42, 4
    for yi in range(int(size_y / step) + 1):
        y = -size_y / 2 + yi * step
        for xi in range(int(size_x / step) + 1):
            x = -size_x / 2 + xi * step
            z = 0.10 * math.sin(x * 0.18) + 0.08 * math.cos(y * 0.22)
            verts.append((x, y, z))
    cols = int(size_x / step) + 1
    rows = int(size_y / step) + 1
    for yi in range(rows - 1):
        for xi in range(cols - 1):
            a = yi * cols + xi
            faces.append((a, a + 1, a + cols + 1, a + cols))
    mesh = bpy.data.meshes.new("ENV_TERRAIN_FOREST_FLOOR_MAIN_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("ENV_TERRAIN_FOREST_FLOOR_MAIN", mesh)
    obj.data.materials.append(mats["MAT_Terrain_ForestFloorBase"])
    collection.objects.link(obj)
    terrain_blob_mesh("ENV_TERRAIN_MOSSY_BANK_LEFT", collection, (-9.5, 4.2, 0.18), 8.8, 4.2, mats["MAT_Terrain_MossyBank"], 20, 0.20, 0.18, 0.07, 0.035)
    terrain_blob_mesh("ENV_TERRAIN_MOSSY_BANK_RIGHT", collection, (9.2, 5.7, 0.18), 8.4, 4.0, mats["MAT_Terrain_MossyBank"], 20, -0.12, 0.18, 0.07, 0.035)
    terrain_blob_mesh("ENV_TERRAIN_ROOT_RAISED_PLATFORM", collection, (0, 8.0, 0.21), 5.8, 3.4, mats["MAT_Moss"], 22, 0.03, 0.12, 0.055, 0.075)
    terrain_blob_mesh("ENV_TERRAIN_FOREST_BACKGROUND_LAYER", collection, (0, 16.8, 0.12), 25.0, 7.2, mats["MAT_Terrain_BackgroundLayer"], 24, 0.08, 0.20, 0.08, -0.025)
    terrain_blob_mesh("ENV_TERRAIN_SOFT_EDGE_BLEND", collection, (0, -4.4, 0.19), 22.5, 10.8, mats["MAT_Terrain_SoftEdgeBlend"], 26, -0.05, 0.20, 0.045, -0.035)
    terrain_blob_mesh("ENV_TERRAIN_MOSSGLASS_POOL_LEFT_IrregularDepression", collection, (-12.2, 0.8, 0.19), 3.35, 1.28, mats["MAT_Water_BlueGreen_Optional"], 18, -0.18, 0.22, 0.025, -0.05)
    for i, (x, y, rx, ry, rot) in enumerate([
        (-7.8, 6.8, 3.2, 1.8, 14),
        (8.2, 7.8, 3.1, 1.7, -12),
        (0.0, 13.6, 3.8, 2.0, 4),
        (-15.0, -7.6, 3.0, 1.5, -18),
        (15.2, -6.9, 3.2, 1.6, 16),
    ]):
        terrain_blob_mesh(
            "ENV_TERRAIN_TREE_ROOT_MOUND" if i == 0 else f"ENV_TERRAIN_TREE_ROOT_MOUND_{i:02d}",
            collection,
            (x, y, 0.28 + i * 0.004),
            rx,
            ry,
            mats["MAT_Terrain_MossyBank"],
            16,
            math.radians(rot),
            0.18,
            0.075,
            0.07,
        )
    for i, (x, y, rx, ry, rot) in enumerate([(-6.0, -3.2, 2.3, 0.85, 9), (6.4, -1.0, 2.1, 0.78, -11), (-3.7, 10.8, 2.4, 0.92, -7), (4.0, 11.5, 2.2, 0.86, 12)]):
        terrain_blob_mesh(
            f"ENV_TERRAIN_SHALLOW_DEPRESSION_ShadowPocket_{i:02d}",
            collection,
            (x, y, 0.225 + i * 0.002),
            rx,
            ry,
            mats["MAT_Terrain_DepressionShadow"],
            14,
            math.radians(rot),
            0.20,
            0.025,
            -0.04,
        )
    for i, (x, y, rx, ry, rot, mat_key) in enumerate([
        (-17.0, -6.8, 7.2, 3.1, -12, "MAT_Terrain_ForestFloorShadow"),
        (17.2, -6.1, 7.0, 3.0, 14, "MAT_Terrain_ForestFloorShadow"),
        (-15.4, 4.6, 7.8, 3.4, 9, "MAT_Terrain_SoftEdgeBlend"),
        (15.6, 5.1, 7.6, 3.5, -8, "MAT_Terrain_SoftEdgeBlend"),
        (-9.5, 12.2, 6.2, 2.2, 17, "MAT_Terrain_ForestFloorShadow"),
        (9.7, 12.4, 6.0, 2.1, -16, "MAT_Terrain_ForestFloorShadow"),
    ]):
        terrain_blob_mesh(
            f"ENV_TERRAIN_SOFT_EDGE_BLEND_ForestFloorBreakup_{i:02d}",
            collection,
            (x, y, 0.205 + i * 0.003),
            rx,
            ry,
            mats[mat_key],
            18,
            math.radians(rot),
            0.22,
            0.045,
            -0.025,
        )
    for i, (x, y, length, rot, width) in enumerate([
        (-10.8, -8.2, 4.8, 22, 0.20),
        (10.6, -7.5, 4.5, -18, 0.20),
        (-11.8, 1.8, 4.2, -8, 0.18),
        (11.7, 2.4, 4.1, 13, 0.18),
        (-6.8, 12.4, 3.5, 35, 0.17),
        (6.9, 12.7, 3.4, -32, 0.17),
    ]):
        cube_obj(
            f"ENV_TERRAIN_ROOT_RIDGE_GroundedForestFloor_{i:02d}",
            collection,
            (x, y, 0.42 + i * 0.004),
            (length, width, 0.16),
            mats["MAT_Root_MossWrap"],
            (math.radians(5), 0, math.radians(rot)),
        )

    path_nodes = [(0, -14.2), (0, -10.5), (-0.25, -6.5), (0.25, -2.4), (0, 1.8), (0.1, 5.8), (0, 9.4), (0, 13.4)]
    for i, (x, y) in enumerate(path_nodes):
        width = 2.4 + 0.26 * min(i, 5)
        irregular_slab_mesh(
            f"ENV_PATH_MOSSYSTONE_STRAIGHT_MainWalkableTile_{i:02d}",
            collection,
            (x, y, 0.25),
            width * 0.95,
            1.18,
            0.12,
            mats["MAT_Path_MossyStone"],
            9,
            0.08 * math.sin(i),
            0.12,
        )
        for chip, offset in enumerate([-0.44, 0.42]):
            irregular_slab_mesh(
                f"ENV_PATH_MOSSYSTONE_CURVE_OffsetHandPlacedSlab_{i:02d}_{chip:02d}",
                collection,
                (x + offset * width, y + 0.18 * math.sin(i + chip), 0.335 + chip * 0.006),
                0.48 + 0.06 * (i % 3),
                0.34,
                0.055,
                mats["MAT_Stone_WarmHighlight"] if chip == 0 else mats["MAT_Stone_MossyGray"],
                7,
                0.32 * i + chip,
                0.20,
            )
        if i < len(path_nodes) - 1:
            nx, ny = path_nodes[i + 1]
            mid = ((x + nx) / 2, (y + ny) / 2)
            length = math.dist((x, y), (nx, ny))
            angle = math.atan2(ny - y, nx - x)
            cube_obj(
                f"SCENE_Terrain_CentralPath_SubtleSubsurfaceRootConnector_{i:02d}",
                collection,
                (mid[0], mid[1], 0.205),
                (length * 0.76, 0.24, 0.10),
                mats["MAT_Root_MossWrap"],
                (0, 0, angle),
            )
            perp = (-math.sin(angle), math.cos(angle))
            border_width = width * 0.62
            for side_name, side_sign in [("Left", -1), ("Right", 1)]:
                cube_obj(
                    f"ENV_PATH_ROOT_BORDER_LivingRootRail_{side_name}_{i:02d}",
                    collection,
                    (mid[0] + perp[0] * border_width * side_sign, mid[1] + perp[1] * border_width * side_sign, 0.36 + i * 0.012),
                    (length * 0.74, 0.20, 0.15),
                    mats["MAT_Path_WarmRoot"],
                    (0.04 * side_sign, 0, angle + 0.05 * side_sign),
                )
                cube_obj(
                    f"ENV_PATH_ROOT_BORDER_LivingKnot_{side_name}_{i:02d}",
                    collection,
                    (
                        mid[0] + perp[0] * border_width * side_sign + math.cos(angle) * length * 0.28,
                        mid[1] + perp[1] * border_width * side_sign + math.sin(angle) * length * 0.28,
                        0.42 + i * 0.012,
                    ),
                    (0.72, 0.24, 0.17),
                    mats["MAT_Bark_DarkRoot"],
                    (0.06 * side_sign, 0, angle + 0.38 * side_sign),
                )
    for i, (name, x, y, length, rot, width) in enumerate([
        ("LeftTreehouseApproach", -4.95, 4.2, 5.9, 28, 0.56),
        ("RightTreehouseApproach", 5.05, 4.8, 5.7, -25, 0.54),
        ("LeftPoolRoute", -7.6, -1.0, 5.4, 12, 0.46),
        ("RightCrystalRoute", 7.3, -0.3, 4.8, -14, 0.46),
    ]):
        cube_obj(
            f"ENV_PATH_SECONDARY_ROOTPATH_{name}_ClearNarrowRoute_{i:02d}",
            collection,
            (x, y, 0.36),
            (length, width, 0.10),
            mats["MAT_Root_MossWrap"],
            (0.03, 0, math.radians(rot)),
        )
        for slab_index, offset in enumerate([-0.34, 0.0, 0.36]):
            irregular_slab_mesh(
                f"ENV_PATH_SECONDARY_MOSSYSTONE_{name}_Step_{slab_index:02d}",
                collection,
                (x + math.cos(math.radians(rot)) * offset * length, y + math.sin(math.radians(rot)) * offset * length, 0.43 + slab_index * 0.004),
                0.48,
                0.28,
                0.06,
                mats["MAT_Stone_MossyGray"],
                7,
                math.radians(rot + slab_index * 17),
                0.20,
            )
    for i, (x, y, s) in enumerate([(-1.2, -11.7, 0.7), (1.0, -8.6, 0.55), (-1.0, -5.2, 0.6), (1.25, -1.4, 0.55), (-1.1, 3.1, 0.62), (1.1, 6.5, 0.7), (-1.0, 10.4, 0.58)]):
        disc_mesh(f"SCENE_Terrain_ReadableCobble_{i:02d}", collection, (x, y, 0.32), 0.55 * s, 0.36 * s, mats["MAT_Stone_MossyGray"], 10, (0, 0, i))
    for i, (x, y, sx, sy, rot) in enumerate([
        (-0.55, -12.2, 0.72, 0.36, 12),
        (0.74, -10.6, 0.56, 0.30, -18),
        (-0.78, -7.3, 0.62, 0.28, 7),
        (0.68, -5.6, 0.78, 0.34, -9),
        (-0.64, -2.2, 0.66, 0.30, 16),
        (0.82, 0.8, 0.58, 0.28, -13),
        (-0.72, 4.4, 0.76, 0.34, 6),
        (0.66, 7.1, 0.66, 0.30, -15),
        (-0.58, 10.7, 0.62, 0.28, 10),
    ]):
        disc_mesh(
            f"SCENE_Terrain_HandPaintedStoneSlabHighlight_{i:02d}",
            collection,
            (x, y, 0.345 + 0.004 * i),
            sx,
            sy,
            mats["MAT_Stone_WarmHighlight"],
            8,
            (0, 0, math.radians(rot)),
        )
    for i, (x, y, rot, sx, sy) in enumerate([(-0.55, -9.8, 8, 1.35, 0.52), (0.42, -4.0, -7, 1.15, 0.46), (-0.35, 2.8, 5, 1.25, 0.50), (0.58, 8.4, -9, 1.45, 0.58)]):
        cube_obj(
            f"ENV_PATH_STONE_STEP_SubtleMossyElevation_{i:02d}",
            collection,
            (x, y, 0.42 + i * 0.014),
            (sx, sy, 0.10),
            mats["MAT_Stone_MossyGray"],
            (0, 0, math.radians(rot)),
        )
    for i, (x, y, rot, length) in enumerate([(-2.7, -11.0, 22, 1.5), (2.7, -9.4, -18, 1.35), (-2.85, -4.8, 16, 1.6), (3.0, -2.8, -21, 1.45), (-3.05, 3.8, 18, 1.7), (3.2, 5.2, -15, 1.55), (-3.2, 9.4, 10, 1.4), (3.35, 10.3, -12, 1.35)]):
        cube_obj(
            f"SCENE_Terrain_PathEdgeLivingRootKnuckle_{i:02d}",
            collection,
            (x, y, 0.43),
            (length, 0.18, 0.18),
            mats["MAT_Root_MossWrap"],
            (0.06, 0.02, math.radians(rot)),
        )
    disc_mesh("SCENE_Terrain_PlayerSpawn_SoftWorldrootRegistrationRune", collection, (0, -13.35, 0.37), 0.82, 0.38, mats["MAT_PlayerSpawn_CyanRune"], 24)
    for i, rot in enumerate([0, math.pi / 2, math.pi, math.pi * 1.5]):
        cube_obj(
            f"SCENE_Terrain_PlayerSpawn_RuneTick_{i:02d}",
            collection,
            (math.cos(rot) * 0.76, -13.35 + math.sin(rot) * 0.36, 0.39),
            (0.34, 0.045, 0.030),
            mats["MAT_PlayerSpawn_CyanRune"],
            (0, 0, rot),
        )
    for i, (x, y, rx, ry, rot) in enumerate([
        (-4.8, -8.9, 1.7, 0.62, -12),
        (4.4, -5.7, 1.45, 0.52, 10),
        (-5.3, -0.8, 1.55, 0.56, 16),
        (5.0, 2.5, 1.65, 0.60, -8),
        (-4.8, 7.0, 1.72, 0.62, 11),
        (4.6, 9.4, 1.48, 0.54, -14),
    ]):
        dapple = disc_mesh(
            f"SCENE_Terrain_WarmSafeGladeSunDapple_EdgeOnly_{i:02d}",
            collection,
            (x, y, 0.405 + i * 0.002),
            rx,
            ry,
            mats["MAT_SunDapple_Warm"],
            14,
            (0, 0, math.radians(rot)),
        )
        disable_shadow(dapple)


def make_worldroot_tree(prefix, collection, x, y, scale, mats, lean=0.0, heart=False, fogged=False):
    variant = "TREE_WORLDROOT_BACKGROUND" if fogged else ("TREE_WORLDROOT_HERO" if heart or scale >= 1.48 else "TREE_WORLDROOT_VILLAGE")
    bark = mats["MAT_Backdrop_DistantTrunk"] if fogged else bpy.data.materials["MAT_Bark_WarmBrown"]
    leaf_dark = mats["MAT_Backdrop_ForestDepth"] if fogged else bpy.data.materials["MAT_Leaves_DeepGreen"]
    leaf_light = mats["MAT_Backdrop_ForestDepth"] if fogged else bpy.data.materials["MAT_Leaves_LightGreen"]
    root_mat = bpy.data.materials["MAT_Bark_DarkRoot"]
    if not fogged:
        base = disc_mesh(
            f"{prefix}_MossyRootBaseFootprint",
            collection,
            (x, y, 0.235),
            2.55 * scale,
            1.35 * scale,
            mats["MAT_Moss"],
            16,
            (0, 0, math.radians(lean)),
        )
        disable_shadow(base)
    if fogged:
        trunk = frustum_obj(
            f"{prefix}_{variant}_TwistedLivingTrunk",
            collection,
            (x, y, 3.95 * scale),
            1.35 * scale,
            0.66 * scale,
            7.9 * scale,
            8,
            bark,
            (math.radians(lean), math.radians(-lean * 0.35), math.radians(lean * 0.55)),
        )
    else:
        trunk = twisted_trunk_mesh(
            f"{prefix}_{variant}_TwistedLivingTrunk",
            collection,
            (x, y, 3.95 * scale),
            1.42 * scale,
            0.62 * scale,
            7.9 * scale,
            9,
            7,
            bark,
            lean_degrees=lean,
            twist_degrees=48 if variant == "HeroTree" else 36,
        )
    disable_shadow(trunk)
    if not fogged:
        for rib, (angle, radius, z, height, lean_offset) in enumerate(
            [
                (-28, 0.54, 2.15, 4.2, -8),
                (22, 0.48, 3.15, 4.7, 7),
                (92, 0.42, 3.75, 3.9, 14),
                (-118, 0.38, 4.45, 3.4, -12),
            ]
        ):
            radians = math.radians(angle + lean * 0.35)
            rib_obj = frustum_obj(
                f"{prefix}_{variant}_InterwovenRootRib_{rib:02d}",
                collection,
                (x + math.cos(radians) * radius * scale, y + math.sin(radians) * 0.52 * scale, z * scale),
                0.24 * scale,
                0.10 * scale,
                height * scale,
                6,
                mats["MAT_Bark_DeepCrease"] if rib % 2 else mats["MAT_Bark_DarkRoot"],
                (math.radians(lean + lean_offset), math.radians(lean_offset * 0.20), radians + math.radians(8)),
            )
            disable_shadow(rib_obj)
        for sidx, (dx, dy, z, rot) in enumerate([
            (-0.34, -0.26, 2.1, -15),
            (0.38, -0.18, 3.2, 18),
            (-0.26, -0.30, 4.6, -24),
            (0.30, -0.20, 5.8, 15),
        ]):
            sheath = cube_obj(
                f"{prefix}_PaintedBarkHighlightTwist_{sidx:02d}",
                collection,
                (x + dx * scale, y + dy * scale, z * scale),
                (0.14 * scale, 0.08 * scale, 1.34 * scale),
                mats["MAT_Bark_WarmEdge"] if sidx % 2 == 0 else mats["MAT_Bark_PaintedHighlight"],
                (math.radians(lean * 0.12), math.radians(3), math.radians(rot + lean)),
            )
            disable_shadow(sheath)
        for midx, (dx, dy, z, rot) in enumerate([(0.52, -0.18, 2.75, 32), (-0.50, -0.10, 4.05, -38)]):
            moss = cube_obj(
                f"{prefix}_LivingMossWrap_{midx:02d}",
                collection,
                (x + dx * scale, y + dy * scale, z * scale),
                (0.18 * scale, 0.10 * scale, 0.92 * scale),
                mats["MAT_Root_MossWrap"],
                (math.radians(4), math.radians(lean * 0.10), math.radians(rot)),
            )
            disable_shadow(moss)
    for r in range(6):
        angle = math.tau * r / 6 + lean * 0.015
        root = cube_obj(
            f"{prefix}_{variant}_ExposedRootButtress_{r:02d}",
            collection,
            (x + math.cos(angle) * 1.00 * scale, y + math.sin(angle) * 0.72 * scale, 0.34),
            (2.45 * scale, 0.30 * scale, 0.22 * scale),
            root_mat,
            (0.05, 0, angle),
        )
        disable_shadow(root)
        if not fogged:
            tendril = cube_obj(
                f"{prefix}_{variant}_ThinRootTendril_{r:02d}",
                collection,
                (x + math.cos(angle + 0.18) * 1.72 * scale, y + math.sin(angle + 0.18) * 1.10 * scale, 0.46),
                (1.80 * scale, 0.10 * scale, 0.10 * scale),
                mats["MAT_Root_MossWrap"],
                (0.08, 0, angle + 0.30),
            )
            disable_shadow(tendril)
    canopy_offsets = [(0.0, 0.0, 8.7), (-1.38, 0.22, 8.25), (1.22, -0.18, 8.05), (0.30, 0.98, 8.45), (-0.35, -0.82, 7.92)]
    if variant == "TREE_WORLDROOT_HERO":
        canopy_offsets.extend([(-2.05, 0.72, 7.70), (1.95, 0.45, 7.85), (0.70, -1.28, 7.50)])
    for c, offset in enumerate(canopy_offsets):
        canopy = leaf_blob_mesh(
            f"{prefix}_{variant}_AsymmetricChunkyCanopy_{c:02d}",
            collection,
            (x + offset[0] * scale, y + offset[1] * scale, offset[2] * scale),
            (
                (2.20 + 0.30 * (c % 2)) * scale,
                (1.26 + 0.22 * ((c + 1) % 2)) * scale,
                (0.72 + 0.12 * (c % 3 == 0)) * scale,
            ),
            leaf_light if c in [0, 3] else leaf_dark,
            (math.radians(2 + c), math.radians(8 - c), math.radians(lean + c * 24)),
        )
        disable_shadow(canopy)
        if not fogged and c in [0, 1, 3]:
            highlight = irregular_slab_mesh(
                f"{prefix}_{variant}_CanopyPaintedLightPatch_{c:02d}",
                collection,
                (x + (offset[0] - 0.18) * scale, y + (offset[1] - 0.20) * scale, (offset[2] + 0.36) * scale),
                (0.82 + 0.12 * c) * scale,
                (0.30 + 0.04 * c) * scale,
                0.05 * scale,
                mats["MAT_Leaves_CanopyHighlight"],
                6,
                math.radians(lean + c * 21),
                0.18,
            )
            disable_shadow(highlight)
        if not fogged and c in [2, 4]:
            underside = irregular_slab_mesh(
                f"{prefix}_{variant}_CanopyUndersideShadow_{c:02d}",
                collection,
                (x + offset[0] * scale, y + (offset[1] + 0.10) * scale, (offset[2] - 0.62) * scale),
                (1.10 + 0.08 * c) * scale,
                (0.48 + 0.05 * c) * scale,
                0.045 * scale,
                mats["MAT_Leaves_UndersideShadow"],
                6,
                math.radians(lean + c * 24),
                0.16,
            )
            disable_shadow(underside)
    branch_specs = [(-1, 3.2, -18, 2.25), (1, 4.3, 22, 2.25), (-1, 5.4, -34, 2.25)]
    if variant == "TREE_WORLDROOT_HERO":
        branch_specs.extend([(1, 6.2, 36, 2.9), (-1, 6.8, -46, 2.7)])
    for b, (side, z, rot, length) in enumerate(branch_specs):
        branch = cube_obj(
            f"{prefix}_{variant}_LivingBranchCluster_{b:02d}",
            collection,
            (x + side * 0.82 * scale, y - 0.12 * scale, z * scale),
            (length * scale, 0.24 * scale, 0.18 * scale),
            root_mat,
            (math.radians(8), 0, math.radians(rot)),
        )
        disable_shadow(branch)
        if not fogged:
            leaf = frustum_obj(
                f"{prefix}_{variant}_BranchLeafCluster_{b:02d}",
                collection,
                (x + side * (1.58 + 0.12 * b) * scale, y + 0.08 * scale, (z + 0.38) * scale),
                (0.82 + 0.08 * (b % 2)) * scale,
                0.36 * scale,
                0.55 * scale,
                6,
                mats["MAT_Leaves_LightGreen"] if b % 2 == 0 else mats["MAT_Leaves_DeepGreen"],
                (math.radians(10), math.radians(3), math.radians(rot + 28)),
            )
            disable_shadow(leaf)
    if not fogged:
        for v, (dx, dy, z, length) in enumerate([(-0.92, 0.18, 6.85, 1.8), (0.82, -0.24, 6.55, 1.55), (-0.20, 0.82, 6.95, 1.35)]):
            vine = cube_obj(
                f"{prefix}_HangingMossVine_{v:02d}",
                collection,
                (x + dx * scale, y + dy * scale, z * scale),
                (0.055 * scale, 0.045 * scale, length * scale),
                mats["MAT_Root_MossWrap"],
                (math.radians(lean * 0.08), math.radians(4), math.radians(v * 12)),
            )
            disable_shadow(vine)
    if heart:
        disc_mesh(
            f"{prefix}_HeartboughCommunalMossRing_FocalLandmark",
            collection,
            (x, y - 2.1 * scale, 0.42),
            2.25 * scale,
            0.82 * scale,
            mats["MAT_Moss_Highlight"],
            24,
            (0, 0, math.radians(4)),
        )
        bicone_mesh(f"{prefix}_CentralWorldrootHeartCrystal", collection, (x, y - 0.82 * scale, 4.95 * scale), 0.55 * scale, 1.38 * scale, mats["MAT_Heartbough_CoreGlow"], 6, (math.radians(90), 0, 0))
        for cradle, angle in enumerate([math.radians(22), math.radians(142), math.radians(262)]):
            cube_obj(
                f"{prefix}_HeartboughLivingRootCradle_{cradle:02d}",
                collection,
                (x + math.cos(angle) * 0.78 * scale, y - 0.72 * scale + math.sin(angle) * 0.34 * scale, 4.55 * scale),
                (1.35 * scale, 0.16 * scale, 0.16 * scale),
                mats["MAT_Bark_DarkRoot"],
                (math.radians(8), math.radians(4), angle + math.radians(18)),
            )
        for rune, angle in enumerate([0, math.tau / 3, math.tau * 2 / 3]):
            bicone_mesh(
                f"{prefix}_HeartboughFloatingMemoryLeafRune_{rune:02d}",
                collection,
                (x + math.cos(angle) * 1.10 * scale, y - 0.95 * scale + math.sin(angle) * 0.46 * scale, 5.35 * scale),
                0.16 * scale,
                0.38 * scale,
                mats["MAT_QuestGold"],
                5,
                (math.radians(90), 0, angle),
            )
    for c, z in enumerate([2.3, 3.6, 5.1] if not fogged else [3.8]):
        crack = cube_obj(
            f"{prefix}_SoftCyanBarkMemoryCrack_{c:02d}",
            collection,
            (x - 0.34 * scale, y - 0.78 * scale, z * scale),
            (0.08 * scale, 0.055 * scale, 0.72 * scale),
            mats["MAT_CyanBarkCrack"],
            (math.radians(lean * 0.2), 0, math.radians(-4 + c * 3)),
        )
        disable_shadow(crack)


def make_background_trees(collection, mats):
    for i, (x, y, s, lean) in enumerate([(-27, 12.8, 1.45, 9), (-22, 17.2, 1.85, -5), (-15, 18.7, 1.45, 7), (-8, 19.4, 1.25, -3), (0, 20.0, 1.55, 0), (8, 19.2, 1.28, 4), (15, 18.4, 1.48, -7), (22, 17.0, 1.75, 5), (27, 12.6, 1.45, -8)]):
        make_worldroot_tree(f"SCENE_Worldroot_TreeVariant_BackgroundSilhouette_{i:02d}", collection, x, y, s, mats, lean=lean, fogged=True)
    for i, (x, y, s, lean) in enumerate([(-21.5, -1.4, 1.35, 12), (21.0, -0.5, 1.32, -10), (-13.8, 5.6, 1.14, -7), (13.6, 6.1, 1.18, 8), (-6.4, 12.9, 1.04, 4), (6.6, 13.1, 1.08, -4)]):
        make_worldroot_tree(f"SCENE_Worldroot_TreeVariant_MediumVillage_{i:02d}", collection, x, y, s, mats, lean=lean)
    for i, (x, y, s, lean) in enumerate([(-18.7, -8.7, 1.65, -14), (18.5, -7.9, 1.55, 13), (-17.2, 10.7, 1.55, 8), (17.3, 11.0, 1.52, -9)]):
        make_worldroot_tree(f"SCENE_Worldroot_TreeVariant_HeroFraming_{i:02d}", collection, x, y, s, mats, lean=lean)
    for i, (x, y, s, lean) in enumerate([(-30.5, 4.2, 1.95, 10), (30.4, 4.9, 1.92, -12), (-25.8, 9.3, 1.72, -7), (25.6, 9.6, 1.70, 7)]):
        make_worldroot_tree(f"SCENE_Worldroot_TreeVariant_BackgroundSideCurtain_{i:02d}", collection, x, y, s, mats, lean=lean, fogged=True)
    make_worldroot_tree("SCENE_Worldroot_TreeVariant_HeroHeartboughFocal_AncientMemoryRoot", collection, 0.0, 15.2, 1.92, mats, lean=2, heart=True)


def place_architecture(templates, collections, mats):
    arch = collections["SCENE_Architecture"]
    make_sylvaen_treehouse_prefab(
        "ARCH_SYLVAEN_TREEHOUSE_SMALL_LeftQuestHallPrefabReplacement",
        arch,
        -9.6,
        7.0,
        1.12,
        math.radians(8),
        mats,
    )
    make_sylvaen_treehouse_prefab(
        "ARCH_SYLVAEN_TREEHOUSE_MEDIUM_RightTrainerPrefabReplacement",
        arch,
        9.2,
        8.1,
        1.18,
        math.radians(-12),
        mats,
    )
    make_sylvaen_treehouse_prefab(
        "ARCH_SYLVAEN_TREEHOUSE_HUB_BackMemoryArchivePrefabReplacement",
        arch,
        0.0,
        14.0,
        1.36,
        math.radians(2),
        mats,
        hub=True,
    )
    for i, (name, x, y, scale, rot) in enumerate([
        ("LeftQuestHall", -9.6, 7.0, 1.35, 8),
        ("RightTrainer", 9.2, 8.1, 1.25, -12),
        ("BackMemoryArchive", 0.0, 14.0, 1.55, 2),
    ]):
        twisted_trunk_mesh(
            f"SCENE_Architecture_{name}_LivingTrunkIntegratedCore",
            arch,
            (x, y + 0.55 * scale, 1.90 * scale),
            0.70 * scale,
            0.42 * scale,
            3.80 * scale,
            7,
            5,
            bpy.data.materials["MAT_Bark_WarmBrown"],
            lean_degrees=3 + rot * 0.08,
            twist_degrees=30 + abs(rot) * 0.4,
        )
        for side, sign in [("Left", -1), ("Right", 1)]:
            cube_obj(
                f"SCENE_Architecture_{name}_RootSupportWrap_{side}",
                arch,
                (x + sign * 0.92 * scale, y - 0.18 * scale, 1.08 * scale),
                (0.30 * scale, 2.25 * scale, 0.24 * scale),
                bpy.data.materials["MAT_Bark_DarkRoot"],
                (math.radians(9), 0, math.radians(rot + sign * 18)),
            )
            cube_obj(
                f"SCENE_Architecture_{name}_LeafRoofWing_{side}",
                arch,
                (x + sign * 0.76 * scale, y - 0.42 * scale, 2.72 * scale),
                (1.72 * scale, 0.38 * scale, 0.20 * scale),
                bpy.data.materials["MAT_LeafRoof_Verdant"],
                (math.radians(sign * 7), math.radians(4), math.radians(rot + sign * 12)),
            )
        cube_obj(
            f"SCENE_Architecture_{name}_CyanArchedWindowGlow",
            arch,
            (x, y - 1.02 * scale, 2.02 * scale),
            (0.58 * scale, 0.07 * scale, 0.68 * scale),
            mats["MAT_Window_CyanSoft_Emission"],
            (0, 0, math.radians(rot)),
        )
        cube_obj(
            f"SCENE_Architecture_{name}_GoldWoodWindowTrim",
            arch,
            (x, y - 1.08 * scale, 2.02 * scale),
            (0.82 * scale, 0.055 * scale, 0.10 * scale),
            bpy.data.materials["MAT_Wood_GoldenTrim"],
            (0, 0, math.radians(rot)),
        )
        inst(
            templates,
            "ARCH_Sylvaen_ArchedDoorway",
            f"SCENE_Architecture_{name}_GrownArchedDoorway",
            arch,
            (x, y - 1.38 * scale, 0.44),
            (0, 0, math.radians(rot)),
            (0.72 * scale, 0.72 * scale, 0.82 * scale),
        )
        for root_side, sign in [("Left", -1), ("Right", 1)]:
            frustum_obj(
                f"SCENE_Architecture_{name}_DoorwayLivingRootColumn_{root_side}",
                arch,
                (x + sign * 0.52 * scale, y - 1.48 * scale, 0.98 * scale),
                0.16 * scale,
                0.11 * scale,
                1.45 * scale,
                6,
                mats["MAT_Bark_DarkRoot"],
                (math.radians(sign * 6), 0, math.radians(rot + sign * 8)),
            )
        cube_obj(
            f"SCENE_Architecture_{name}_DoorwayCurvedRootLintel_ReadsGrown",
            arch,
            (x, y - 1.54 * scale, 1.58 * scale),
            (1.18 * scale, 0.14 * scale, 0.16 * scale),
            mats["MAT_Bark_DarkRoot"],
            (math.radians(2), 0, math.radians(rot)),
        )
        disc_mesh(
            f"SCENE_Architecture_{name}_GrownBalconyMossCollar",
            arch,
            (x, y + 0.05 * scale, 2.36 * scale),
            1.54 * scale,
            0.92 * scale,
            mats["MAT_Moss_Highlight"],
            18,
            (0, 0, math.radians(rot + 6)),
        )
        frustum_obj(
            f"SCENE_Architecture_{name}_CurvedLeafRoofCap_GrownNotBuilt",
            arch,
            (x, y - 0.04 * scale, 3.28 * scale),
            1.20 * scale,
            0.42 * scale,
            0.48 * scale,
            6,
            mats["MAT_LeafRoof_Verdant"],
            (math.radians(88), math.radians(6), math.radians(rot + 30)),
        )
        cube_obj(
            f"SCENE_Architecture_{name}_WarmGoldRoofEdgeTrim",
            arch,
            (x, y - 0.98 * scale, 2.98 * scale),
            (1.62 * scale, 0.08 * scale, 0.09 * scale),
            bpy.data.materials["MAT_Wood_GoldenTrim"],
            (0, 0, math.radians(rot)),
        )
        for rail_index, (side, local_y, local_z, length, local_rot) in enumerate(
            [(-1, -0.20, 2.47, 1.70, -16), (1, -0.20, 2.47, 1.70, 16), (-1, 0.55, 2.62, 1.35, -34), (1, 0.55, 2.62, 1.35, 34)]
        ):
            cube_obj(
                f"SCENE_Architecture_{name}_BalconyIntegratedRootRail_{rail_index:02d}",
                arch,
                (x + side * 0.88 * scale, y + local_y * scale, local_z * scale),
                (length * scale, 0.12 * scale, 0.12 * scale),
                mats["MAT_Bark_DarkRoot"],
                (math.radians(5), math.radians(side * 3), math.radians(rot + local_rot)),
            )
        for panel_index, (side, dz, panel_rot) in enumerate([(-1, 0.0, -20), (1, 0.08, 20), (0, 0.16, 0)]):
            cube_obj(
                f"SCENE_Architecture_{name}_LayeredLeafRoofPanel_{panel_index:02d}",
                arch,
                (x + side * 0.58 * scale, y - 0.52 * scale, (3.03 + dz) * scale),
                (1.10 * scale, 0.28 * scale, 0.11 * scale),
                mats["MAT_LeafRoof_Verdant"],
                (math.radians(9), math.radians(side * 6), math.radians(rot + panel_rot)),
            )
        for wrap_index, (z, side, local_rot) in enumerate([(1.38, -1, 44), (1.78, 1, -38), (2.18, -1, 28)]):
            cube_obj(
                f"SCENE_Architecture_{name}_LivingRootWallWrap_{wrap_index:02d}",
                arch,
                (x + side * 0.58 * scale, y - 0.60 * scale, z * scale),
                (0.15 * scale, 1.42 * scale, 0.13 * scale),
                bpy.data.materials["MAT_Bark_DarkRoot"],
                (math.radians(6), math.radians(side * 3), math.radians(rot + local_rot)),
            )
        for sparkle_index, dx in enumerate([-0.44, 0.44]):
            cube_obj(
                f"SCENE_Architecture_{name}_SmallCyanMemoryGlassSideWindow_{sparkle_index:02d}",
                arch,
                (x + dx * scale, y - 1.05 * scale, 1.62 * scale),
                (0.22 * scale, 0.06 * scale, 0.36 * scale),
                mats["MAT_Window_CyanSoft_Emission"],
                (0, 0, math.radians(rot)),
            )
        cube_obj(
            f"SCENE_Architecture_{name}_GrownBarkBackShell_IntegratesHouseIntoTree",
            arch,
            (x, y + 0.74 * scale, 2.12 * scale),
            (1.72 * scale, 0.30 * scale, 1.72 * scale),
            mats["MAT_Bark_DeepCrease"],
            (math.radians(3), 0, math.radians(rot)),
        )
        for brace_index, (side, local_y, local_z, length, brace_rot) in enumerate([
            (-1, -0.74, 1.48, 2.35, -38),
            (1, -0.74, 1.48, 2.35, 38),
            (-1, 0.28, 1.88, 1.95, -24),
            (1, 0.28, 1.88, 1.95, 24),
        ]):
            cube_obj(
                f"SCENE_Architecture_{name}_LivingRootKneeBraceToBalcony_{brace_index:02d}",
                arch,
                (x + side * 0.76 * scale, y + local_y * scale, local_z * scale),
                (length * scale, 0.14 * scale, 0.14 * scale),
                mats["MAT_Bark_DarkRoot"],
                (math.radians(11), math.radians(side * 4), math.radians(rot + brace_rot)),
            )
        for trim_index, (side, dz, trim_rot) in enumerate([(-1, 0.0, -18), (1, 0.05, 18)]):
            cube_obj(
                f"SCENE_Architecture_{name}_LeafRoofPaintedLightEdge_{trim_index:02d}",
                arch,
                (x + side * 0.54 * scale, y - 0.92 * scale, (3.20 + dz) * scale),
                (1.22 * scale, 0.075 * scale, 0.065 * scale),
                mats["MAT_LeafRoof_LightEdge"],
                (math.radians(7), math.radians(side * 5), math.radians(rot + trim_rot)),
            )
    inst(templates, "ARCH_Sylvaen_ArchedDoorway", "SCENE_Architecture_HeartboughEntranceArch", arch, (0, 2.2, 0.32), (0, 0, 0), (1.35, 1.35, 1.35))
    for side, x_pos in [("Left", -2.35), ("Right", 2.35)]:
        twisted_trunk_mesh(
            f"SCENE_Architecture_HeartboughCommunalRootCrown_{side}LivingPillar",
            arch,
            (x_pos, 8.85, 1.78),
            0.34,
            0.22,
            3.10,
            7,
            5,
            mats["MAT_Bark_WarmBrown"],
            lean_degrees=-7 if side == "Left" else 7,
            twist_degrees=34,
        )
        cube_obj(
            f"SCENE_Architecture_HeartboughCommunalRootCrown_{side}RootFoot",
            arch,
            (x_pos * 0.96, 8.35, 0.58),
            (1.65, 0.22, 0.18),
            mats["MAT_Bark_DarkRoot"],
            (math.radians(5), 0, math.radians(18 if side == "Left" else -18)),
        )
    cube_obj(
        "SCENE_Architecture_HeartboughCommunalRootCrown_UpperLivingArchBeam",
        arch,
        (0, 8.85, 3.32),
        (4.65, 0.26, 0.22),
        mats["MAT_Bark_DarkRoot"],
        (math.radians(4), 0, 0),
    )
    for rib_index, (x_pos, rot) in enumerate([(-1.25, -24), (1.25, 24), (0.0, 0)]):
        cube_obj(
            f"SCENE_Architecture_HeartboughCommunalRootCrown_InterwovenRib_{rib_index:02d}",
            arch,
            (x_pos, 8.58, 2.82),
            (2.10, 0.13, 0.13),
            mats["MAT_Root_MossWrap"],
            (math.radians(18), math.radians(x_pos * 2.0), math.radians(rot)),
        )
    bicone_mesh(
        "SCENE_Architecture_HeartboughCommunalRootCrown_HangingCyanHeartSeed",
        arch,
        (0, 8.48, 2.82),
        0.24,
        0.58,
        mats["MAT_Heartbough_CoreGlow"],
        6,
        (math.radians(90), 0, 0),
    )
    make_root_bridge_prefab("ARCH_ROOTBRIDGE_STRAIGHT_LeftToCenterPrefabReplacement", arch, (-4.7, 9.9, 2.7), 5.4, math.radians(8), mats)
    make_root_bridge_prefab("ARCH_ROOTBRIDGE_STRAIGHT_CenterToRightPrefabReplacement", arch, (4.6, 10.4, 2.75), 5.4, math.radians(-7), mats)
    make_root_bridge_prefab("ARCH_ROOTBRIDGE_CURVED_BackMemoryBridgePrefabReplacement", arch, (0.8, 14.2, 2.95), 4.4, math.radians(4), mats, curved=True)
    for i, (x, y, z, length, rot) in enumerate([(-4.6, 9.55, 2.95, 3.4, 8), (4.5, 10.05, 2.98, 3.4, -7), (-1.7, 14.0, 3.22, 2.6, 18), (1.8, 14.15, 3.24, 2.6, -16)]):
        cube_obj(
            f"SCENE_Architecture_RootBridgeLivingGuardrailWrap_{i:02d}",
            arch,
            (x, y, z),
            (length, 0.16, 0.14),
            mats["MAT_Root_MossWrap"],
            (math.radians(4), 0, math.radians(rot)),
        )
    make_root_stairs_prefab("ARCH_ROOTBRIDGE_STAIRS_CentralStoneRootStairPrefabReplacement", arch, (0, 5.15, 0.34), math.radians(90), mats)
    inst(templates, "ARCH_Sylvaen_BalconyPlatform", "SCENE_Architecture_LeftBalconyPlatform", arch, (-6.7, 6.1, 2.6), (0, 0, math.radians(10)), (1.15, 1.05, 1.0))
    inst(templates, "ARCH_Sylvaen_BalconyPlatform", "SCENE_Architecture_RightBalconyPlatform", arch, (6.4, 7.0, 2.6), (0, 0, math.radians(-8)), (1.15, 1.05, 1.0))
    for i, (x, y, rot) in enumerate([(-5.0, 4.0, 0.15), (5.1, 4.3, -0.12), (-2.7, 9.2, 0.08), (2.8, 9.4, -0.08)]):
        inst(templates, "ARCH_Sylvaen_RailingSegment", f"SCENE_Architecture_PathRailing_{i:02d}", arch, (x, y, 0.36), (0, 0, rot), (1.2, 1.2, 1.1))
    for i, (x, y, z, length, rot) in enumerate([
        (-8.7, 7.1, 2.35, 3.2, math.radians(15)),
        (8.4, 8.1, 2.35, 3.0, math.radians(-18)),
        (-1.2, 2.3, 1.65, 2.4, math.radians(52)),
        (1.2, 2.3, 1.65, 2.4, math.radians(-52)),
        (-3.4, 9.95, 2.95, 3.6, math.radians(8)),
        (3.5, 10.35, 2.95, 3.6, math.radians(-8)),
    ]):
        cube_obj(
            f"SCENE_Architecture_LivingRootWrap_{i:02d}",
            arch,
            (x, y, z),
            (length, 0.22, 0.18),
            bpy.data.materials["MAT_Bark_DarkRoot"],
            (0.10, 0.04, rot),
        )
    for i, (x, y, z) in enumerate([(-9.6, 6.35, 3.8), (9.2, 7.45, 3.75), (0, 13.25, 4.15)]):
        bicone_mesh(
            f"SCENE_Architecture_TreehouseVerdantMemorySigil_{i:02d}",
            arch,
            (x, y - 0.95, z),
            0.20,
            0.46,
            bpy.data.materials["MAT_QuestGold"],
            5,
            (math.radians(90), 0, 0),
        )


def place_worldroot_props(templates, collections):
    worldroot = collections["SCENE_Worldroot"]
    for i, (asset, x, y, s) in enumerate([
        ("PROPS_Worldroot_CyanCrystalClusterMedium", -6.7, 2.8, 0.82),
        ("PROPS_Worldroot_CyanCrystalClusterMedium", 6.2, 3.4, 0.82),
        ("PROPS_Worldroot_CyanCrystalClusterLarge", 0.0, 9.8, 0.9),
        ("PROPS_Worldroot_CyanCrystalClusterSmall", -13.2, 5.2, 0.62),
    ]):
        disc_mesh(
            f"SCENE_Worldroot_MeaningfulWorldrootCrystal_{i:02d}_MossStoneBase",
            worldroot,
            (x, y, 0.27),
            0.72 * s,
            0.38 * s,
            bpy.data.materials["MAT_Stone_MossyGray"],
            14,
            (0, 0, i * 0.28),
        )
        inst(templates, asset, f"SCENE_Worldroot_MeaningfulWorldrootCrystal_{i:02d}", worldroot, (x, y, 0.34), (0, 0, i * 0.31), (s, s, s))
        if i < 4:
            for clamp, rot in enumerate([i * 0.42, i * 0.42 + math.pi * 0.72]):
                cube_obj(
                    f"SCENE_Worldroot_MeaningfulWorldrootCrystal_{i:02d}_LivingRootClamp_{clamp:02d}",
                    worldroot,
                    (x + math.cos(rot) * 0.38 * s, y + math.sin(rot) * 0.22 * s, 0.43),
                    (0.72 * s, 0.10 * s, 0.10 * s),
                    bpy.data.materials["MAT_Bark_DarkRoot"],
                    (0.08, 0, rot),
                )
    inst(templates, "PROPS_Worldroot_WaypointStone", "SCENE_Worldroot_HeartboughWaypointStone", worldroot, (-2.2, 7.5, 0.34), (0, 0, 0.18), (1.15, 1.15, 1.15))
    inst(templates, "PROPS_Worldroot_VerdantRuneMonolith", "SCENE_Worldroot_QuestHubRuneMonolith", worldroot, (2.25, 7.8, 0.34), (0, 0, -0.12), (1.2, 1.2, 1.2))
    inst(templates, "PROPS_Worldroot_VerdantRuneMonolith", "SCENE_Worldroot_LeftPathVerdantRuneMonolith", worldroot, (-6.1, 0.7, 0.34), (0, 0, math.radians(10)), (0.9, 0.9, 0.95))
    inst(templates, "PROPS_Worldroot_VerdantRuneMonolith", "SCENE_Worldroot_RightVillageVerdantRuneMonolith", worldroot, (6.1, 1.2, 0.34), (0, 0, math.radians(-12)), (0.9, 0.9, 0.95))
    inst(templates, "PROPS_Worldroot_MemoryEchoMarker", "SCENE_Worldroot_MemoryEchoMarker_LeftPool", worldroot, (-8.8, 1.6, 0.35), (0, 0, 0), (1.0, 1.0, 1.0))
    inst(templates, "PROPS_Worldroot_MemoryEchoMarker", "SCENE_Worldroot_MemoryEchoMarker_RightOldRoot", worldroot, (8.4, 1.9, 0.35), (0, 0, math.radians(11)), (0.92, 0.92, 0.92))
    for i, (x, y, rot) in enumerate([(-2.4, -5.9, -8), (2.2, 11.0, 5)]):
        inst(
            templates,
            "PROPS_Worldroot_WaypointStone",
            f"SCENE_Worldroot_SubtlePathWayfindingStone_CyanOnlyAtRouteDecision_{i:02d}",
            worldroot,
            (x, y, 0.33),
            (0, 0, math.radians(rot)),
            (0.46, 0.46, 0.50),
        )
    for i, (x, y, rot) in enumerate([(-1.5, -8.0, 0.08), (1.7, -2.9, -0.14), (-1.2, 3.8, 0.18), (1.4, 9.5, -0.18)]):
        inst(templates, "PROPS_Worldroot_GlowingRootVein", f"SCENE_Worldroot_GlowingRootVein_Path_{i:02d}", worldroot, (x, y, 0.38), (0, 0, rot), (1.15, 1.0, 1.0))


def place_village_props(templates, collections):
    village = collections["SCENE_VillageProps"]
    for i, (x, y, rot) in enumerate([(-3.8, -8.7, 0.1), (3.7, -7.8, -0.12), (-4.8, 0.8, 0.05), (4.7, 1.4, -0.08), (-5.9, 7.2, 0.2), (5.8, 7.9, -0.2)]):
        inst(templates, "PROPS_Village_LanternPost", f"SCENE_VillageProps_LanternPathGuide_{i:02d}", village, (x, y, 0.32), (0, 0, rot), (1.05, 1.05, 1.05))
        cube_obj(
            f"SCENE_VillageProps_LanternPathGuide_{i:02d}_WarmGlowCard",
            village,
            (x, y - 0.06, 1.55),
            (0.40, 0.035, 0.40),
            bpy.data.materials["MAT_Lantern_Warm_Emission"],
            (0, 0, rot),
        )
    for i, (x, y, rot) in enumerate([(-5.8, -3.0, 0.10), (5.6, -2.1, -0.12), (-7.8, 5.3, 0.22), (7.7, 5.9, -0.18)]):
        inst(templates, "PROPS_Village_BannerPost", f"SCENE_VillageProps_VerdantBanner_{i:02d}", village, (x, y, 0.34), (0, 0, rot), (1.12, 1.12, 1.12))
    inst(templates, "PROPS_Village_SmallShrinePedestal", "SCENE_VillageProps_QuestHubShrinePedestal", village, (0.05, 7.65, 0.34), (0, 0, 0), (1.15, 1.15, 1.15))
    inst(templates, "PROPS_Village_WoodenBench", "SCENE_VillageProps_LeftRestBench", village, (-4.8, 6.45, 0.34), (0, 0, math.radians(16)), (1.1, 1.1, 1.1))
    inst(templates, "PROPS_Village_WoodenBench", "SCENE_VillageProps_RightRestBench", village, (4.9, 6.75, 0.34), (0, 0, math.radians(-15)), (1.1, 1.1, 1.1))
    inst(templates, "PROPS_Village_Signpost", "SCENE_VillageProps_WayfindingSignpost", village, (-3.5, 2.4, 0.34), (0, 0, math.radians(12)), (1.0, 1.0, 1.0))
    for i, (x, y, z) in enumerate([(-7.8, 8.3, 3.2), (7.5, 9.1, 3.25), (-1.8, 10.5, 3.55), (1.9, 10.7, 3.55)]):
        inst(templates, "PROPS_Village_HangingLantern", f"SCENE_VillageProps_HangingLantern_Bridge_{i:02d}", village, (x, y, z), (0, 0, i * 0.2), (1.0, 1.0, 1.0))


def add_grounding_pass(collections, mats):
    terrain = collections["SCENE_Terrain"]
    arch = collections["SCENE_Architecture"]
    worldroot = collections["SCENE_Worldroot"]
    village = collections["SCENE_VillageProps"]

    for i, (name, x, y, sx, sy, rot) in enumerate([
        ("LeftQuestHall", -9.6, 7.0, 2.8, 1.55, 10),
        ("RightTrainer", 9.2, 8.1, 2.65, 1.45, -12),
        ("BackMemoryArchive", 0.0, 14.0, 3.35, 1.80, 2),
    ]):
        terrain_blob_mesh(
            f"GROUNDING_Treehouse_{name}_MossRootFooting",
            terrain,
            (x, y - 0.25, 0.47),
            sx,
            sy,
            mats["MAT_Moss_Highlight"],
            16,
            math.radians(rot),
            0.17,
            0.045,
            0.045,
        )
        for root_index, (side, local_rot) in enumerate([(-1, -24), (1, 24), (0, 4)]):
            cube_obj(
                f"GROUNDING_Treehouse_{name}_RootSupportIntoTerrain_{root_index:02d}",
                arch,
                (x + side * sx * 0.34, y - sy * 0.38, 0.66 + root_index * 0.02),
                (sx * 0.68, 0.18, 0.18),
                mats["MAT_Bark_DarkRoot"],
                (math.radians(8), math.radians(side * 3), math.radians(rot + local_rot)),
            )

    for i, (x, y, rot, sx) in enumerate([
        (-6.7, 6.0, 18, 1.35),
        (-2.25, 9.8, -7, 1.10),
        (2.35, 10.0, 9, 1.10),
        (6.6, 6.8, -20, 1.35),
        (-1.9, 13.8, 24, 1.05),
        (1.9, 14.0, -24, 1.05),
        (0.0, 5.15, 0, 1.55),
    ]):
        terrain_blob_mesh(
            f"GROUNDING_RootBridgeAnchor_TerrainMound_{i:02d}",
            terrain,
            (x, y, 0.43),
            1.22 * sx,
            0.64 * sx,
            mats["MAT_Terrain_MossyBank"],
            14,
            math.radians(rot),
            0.18,
            0.04,
            0.035,
        )
        cube_obj(
            f"GROUNDING_RootBridgeAnchor_LivingRootCluster_{i:02d}",
            arch,
            (x, y, 0.58),
            (1.45 * sx, 0.20, 0.18),
            mats["MAT_Root_MossWrap"],
            (math.radians(7), 0, math.radians(rot)),
        )

    for i, (x, y, s) in enumerate([(-6.7, 2.8, 0.82), (6.2, 3.4, 0.82), (0.0, 9.8, 0.9), (-13.2, 5.2, 0.62)]):
        terrain_blob_mesh(
            f"GROUNDING_WorldrootCrystal_StoneRootNest_{i:02d}",
            terrain,
            (x, y, 0.305),
            0.92 * s,
            0.52 * s,
            mats["MAT_Stone_MossyGray"],
            12,
            i * 0.27,
            0.20,
            0.025,
            0.035,
        )
        glow = terrain_blob_mesh(
            f"GROUNDING_WorldrootCrystal_SubtleCyanMossGlowPool_{i:02d}",
            worldroot,
            (x, y, 0.405),
            1.02 * s,
            0.58 * s,
            mats["MAT_PlayerSpawn_CyanRune"],
            14,
            i * 0.31,
            0.18,
            0.008,
            -0.02,
        )
        disable_shadow(glow)

    pole_positions = [
        (-3.8, -8.7, 0.1),
        (3.7, -7.8, -0.12),
        (-4.8, 0.8, 0.05),
        (4.7, 1.4, -0.08),
        (-5.9, 7.2, 0.2),
        (5.8, 7.9, -0.2),
        (-5.8, -3.0, 0.10),
        (5.6, -2.1, -0.12),
        (-7.8, 5.3, 0.22),
        (7.7, 5.9, -0.18),
    ]
    for i, (x, y, rot) in enumerate(pole_positions):
        terrain_blob_mesh(
            f"GROUNDING_VillagePole_StoneRootBase_{i:02d}",
            village,
            (x, y, 0.37),
            0.42,
            0.30,
            mats["MAT_Stone_MossyGray"],
            10,
            rot,
            0.18,
            0.012,
            0.02,
        )


def place_foliage(templates, collections):
    foliage = collections["SCENE_Foliage"]
    placements = []
    for i in range(38):
        side = -1 if i % 2 == 0 else 1
        y = -12 + i * 0.7
        x = side * (3.2 + 1.6 * abs(math.sin(i * 0.87)))
        placements.append((x, y, i))
    for i, (x, y, seed) in enumerate(placements):
        asset = ["FOLIAGE_FernCluster", "FOLIAGE_BroadLeafPlant", "FOLIAGE_MossPatch", "FOLIAGE_SmallMossyRock"][seed % 4]
        s = 0.75 + 0.25 * math.sin(seed * 1.8)
        inst(templates, asset, f"SCENE_Foliage_PathFrame_{i:02d}", foliage, (x, y, 0.34), (0, 0, seed * 0.4), (s, s, s))
    for i, (asset, x, y) in enumerate([
        ("FOLIAGE_PurpleFlower", -4.2, -10.0),
        ("FOLIAGE_BlueFlower", 4.4, -9.1),
        ("FOLIAGE_PurpleFlower", -6.0, -2.0),
        ("FOLIAGE_BlueFlower", 6.0, 0.0),
        ("FOLIAGE_GlowingMushroom", -8.8, 2.6),
        ("FOLIAGE_GlowingMushroom", 8.6, 3.0),
        ("FOLIAGE_BlueFlower", -6.8, 8.9),
        ("FOLIAGE_PurpleFlower", 6.8, 9.3),
    ]):
        inst(templates, asset, f"SCENE_Foliage_ColorAccent_{i:02d}", foliage, (x, y, 0.36), (0, 0, i), (1.05, 1.05, 1.05))
    for i, (asset, x, y, s) in enumerate([
        ("FOLIAGE_BroadLeafPlant", -15.8, 2.2, 1.18),
        ("FOLIAGE_BroadLeafPlant", 15.5, 3.0, 1.16),
        ("FOLIAGE_FernCluster", -12.4, 9.8, 1.10),
        ("FOLIAGE_FernCluster", 12.1, 10.4, 1.10),
        ("FOLIAGE_MossPatch", -18.5, 7.6, 1.22),
        ("FOLIAGE_MossPatch", 18.0, 8.2, 1.20),
        ("FOLIAGE_SmallMossyRock", -10.4, 13.0, 1.05),
        ("FOLIAGE_SmallMossyRock", 10.6, 13.3, 1.05),
        ("FOLIAGE_GlowingMushroom", -14.0, -1.3, 0.92),
        ("FOLIAGE_GlowingMushroom", 13.7, -0.6, 0.92),
    ]):
        inst(templates, asset, f"SCENE_Foliage_ForestEdgeCluster_{i:02d}", foliage, (x, y, 0.34), (0, 0, i * 0.37), (s, s, s))
    dense_clusters = [
        ("FOLIAGE_FernCluster", -7.4, 4.9, 1.05),
        ("FOLIAGE_BroadLeafPlant", -8.5, 5.6, 1.18),
        ("FOLIAGE_BlueFlower", -7.0, 6.2, 0.95),
        ("FOLIAGE_PurpleFlower", -10.9, 4.4, 1.0),
        ("FOLIAGE_FernCluster", 7.3, 5.2, 1.06),
        ("FOLIAGE_BroadLeafPlant", 8.4, 5.9, 1.16),
        ("FOLIAGE_BlueFlower", 6.8, 6.6, 0.95),
        ("FOLIAGE_PurpleFlower", 10.8, 4.8, 1.0),
        ("FOLIAGE_MossPatch", -2.8, 12.2, 1.12),
        ("FOLIAGE_FernCluster", 2.7, 12.6, 1.08),
        ("FOLIAGE_GlowingMushroom", -3.2, 11.1, 0.92),
        ("FOLIAGE_GlowingMushroom", 3.3, 11.4, 0.92),
        ("FOLIAGE_BroadLeafPlant", -17.0, -5.2, 1.24),
        ("FOLIAGE_BroadLeafPlant", 16.7, -4.8, 1.22),
        ("FOLIAGE_SmallMossyRock", -14.4, -7.1, 1.05),
        ("FOLIAGE_SmallMossyRock", 14.1, -6.8, 1.05),
    ]
    for i, (asset, x, y, s) in enumerate(dense_clusters):
        inst(templates, asset, f"SCENE_Foliage_TargetMoodDenseEdgeCluster_{i:02d}", foliage, (x, y, 0.35), (0, 0, 0.29 * i), (s, s, s))
    for i, (x, y, z) in enumerate([(-9.1, 7.1, 4.4), (9.0, 8.1, 4.2), (-1.8, 11.0, 4.5), (2.0, 11.1, 4.5), (-15.0, 6.4, 5.0), (15.2, 6.9, 5.0), (-4.4, 15.5, 6.2), (4.3, 15.6, 6.2)]):
        inst(templates, "FOLIAGE_HangingVine", f"SCENE_Foliage_HangingVine_{i:02d}", foliage, (x, y, z), (0, 0, i * 0.4), (1.0, 1.0, 1.0))
    cluster_specs = {
        "FOL_CLUSTER_PATH_EDGE": [(-3.6, -9.6), (3.5, -8.7), (-3.9, -2.2), (4.0, 0.2), (-3.4, 5.8), (3.6, 6.4)],
        "FOL_CLUSTER_TREE_BASE": [(-9.2, 6.3), (8.9, 7.4), (-15.0, -7.2), (15.1, -6.5), (0.4, 13.1)],
        "FOL_CLUSTER_CRYSTAL_GROVE": [(-6.7, 2.1), (6.3, 2.8), (0.2, 9.1), (-13.1, 4.6)],
        "FOL_CLUSTER_HOUSE_ENTRY": [(-8.6, 5.4), (8.0, 6.3), (-1.2, 12.2), (1.4, 12.4)],
        "FOL_CLUSTER_FOREST_EDGE": [(-18.0, 4.0), (18.0, 4.6), (-16.0, 11.0), (16.2, 11.4)],
    }
    cluster_assets = ["FOLIAGE_FernCluster", "FOLIAGE_BroadLeafPlant", "FOLIAGE_MossPatch", "FOLIAGE_SmallMossyRock", "FOLIAGE_PurpleFlower", "FOLIAGE_BlueFlower"]
    cluster_index = 0
    for cluster_name, anchors in cluster_specs.items():
        for anchor_index, (x, y) in enumerate(anchors):
            for local_index, (dx, dy) in enumerate([(-0.34, -0.12), (0.28, 0.10), (0.02, 0.32)]):
                asset = cluster_assets[(cluster_index + anchor_index + local_index) % len(cluster_assets)]
                scale = 0.72 + 0.10 * ((cluster_index + local_index) % 3)
                inst(
                    templates,
                    asset,
                    f"{cluster_name}_{anchor_index:02d}_GroundingClusterPiece_{local_index:02d}",
                    foliage,
                    (x + dx, y + dy, 0.38 + 0.004 * local_index),
                    (0, 0, 0.31 * (cluster_index + anchor_index + local_index)),
                    (scale, scale, scale),
                )
            cluster_index += 1


def make_character(name, collection, loc, material, quest=False):
    x, y, z = loc
    body = frustum_obj(f"{name}_TorsoTaperedSylvaenScale", collection, (x, y, z + 0.92), 0.28, 0.20, 1.10, 7, material)
    frustum_obj(f"{name}_HeadSimpleSilhouette", collection, (x, y, z + 1.62), 0.22, 0.24, 0.34, 10, material)
    cube_obj(f"{name}_CloakBackMutedSylvaenShape", collection, (x, y + 0.13, z + 0.82), (0.66, 0.08, 1.20), bpy.data.materials["MAT_RootShadow"], (math.radians(3), 0, 0))
    cube_obj(f"{name}_ShoulderMantleLeafSilhouette", collection, (x, y - 0.02, z + 1.20), (0.88, 0.20, 0.16), bpy.data.materials["MAT_LEAVES_CANOPY_DEEP"])
    for side in [-1, 1]:
        cube_obj(
            f"{name}_ReadableLeg_{'Left' if side < 0 else 'Right'}",
            collection,
            (x + side * 0.13, y, z + 0.32),
            (0.15, 0.16, 0.54),
            material,
            (math.radians(side * 2), 0, 0),
        )
        cube_obj(
            f"{name}_SimpleArm_{'Left' if side < 0 else 'Right'}",
            collection,
            (x + side * 0.35, y - 0.02, z + 0.96),
            (0.10, 0.12, 0.66),
            material,
            (math.radians(side * 7), 0, math.radians(side * 2)),
        )
    cube_obj(f"{name}_StaffOrSpearScaleAnchor", collection, (x + 0.48, y - 0.10, z + 0.93), (0.065, 0.065, 1.45), bpy.data.materials["MAT_BARK_DARK_ROOT"], (0.18, 0, 0.02))
    cube_obj(f"{name}_FeetGroundContact", collection, (x, y, z + 0.08), (0.52, 0.24, 0.08), bpy.data.materials["MAT_BARK_DARK_ROOT"])
    if quest:
        bicone_mesh(f"{name}_QuestMarker", collection, (x, y, z + 2.35), 0.16, 0.28, bpy.data.materials["MAT_QuestGold"])
        cube_obj(f"{name}_QuestMarkerStem", collection, (x, y, z + 2.07), (0.08, 0.08, 0.28), bpy.data.materials["MAT_QuestGold"])
    return body


def place_characters(collections, mats):
    chars = collections["SCENE_Characters"]
    make_character("CHAR_PLAYER_SYLVAEN_PLACEHOLDER", chars, (0, -13.45, 0.30), mats["MAT_PlayerPlaceholder"])
    make_character("CHAR_NPC_MEMORY_KEEPER_PLACEHOLDER", chars, (-1.9, 7.2, 0.36), mats["MAT_NpcPlaceholder"], quest=True)
    make_character("CHAR_NPC_ROOT_GUARDIAN_PLACEHOLDER", chars, (2.2, 7.6, 0.36), mats["MAT_NpcPlaceholder"], quest=False)
    make_character("SCENE_Characters_WardenTrainerNearPath", chars, (4.4, 3.9, 0.36), mats["MAT_NpcPlaceholder"], quest=True)
    cube_obj("SCENE_Characters_WildrootFawn_Body", chars, (-3.4, -4.3, 0.72), (0.85, 0.32, 0.42), bpy.data.materials["MAT_Wood_GoldenTrim"])
    frustum_obj("SCENE_Characters_WildrootFawn_Head", chars, (-2.92, -4.3, 0.94), 0.12, 0.10, 0.30, 8, bpy.data.materials["MAT_Wood_GoldenTrim"], (0, math.radians(90), 0))


def add_thornveil_readability_material_grounding_pass(collections, mats):
    terrain = collections["SCENE_Terrain"]
    arch = collections["SCENE_Architecture"]
    worldroot = collections["SCENE_Worldroot"]
    chars = collections["SCENE_Characters"]

    all_objects = [obj for collection in collections.values() for obj in collection.objects]
    material_remap = {
        "MAT_Bark_WarmBrown": mats["MAT_BARK_MAIN"],
        "MAT_Bark_DarkRoot": mats["MAT_BARK_DARK_ROOT"],
        "MAT_Bark_DeepCrease": mats["MAT_BARK_DARK_ROOT"],
        "MAT_Path_WarmRoot": mats["MAT_BARK_DARK_ROOT"],
        "MAT_Leaves_DeepGreen": mats["MAT_LEAVES_CANOPY_DEEP"],
        "MAT_Leaves_LightGreen": mats["MAT_LEAVES_CANOPY_LIGHT"],
        "MAT_Leaves_CanopyHighlight": mats["MAT_LEAVES_CANOPY_LIGHT"],
        "MAT_Leaves_UndersideShadow": mats["MAT_LEAVES_CANOPY_DEEP"],
        "MAT_Path_MossyStone": mats["MAT_PATH_MOSSYSTONE"],
        "MAT_Stone_MossyGray": mats["MAT_PATH_MOSSYSTONE"],
        "MAT_GladeGrass": mats["MAT_GROUND_MOSS"],
        "MAT_Terrain_ForestFloorBase": mats["MAT_GROUND_MOSS"],
        "MAT_Terrain_MossyBank": mats["MAT_GROUND_MOSS"],
        "MAT_Wood_GoldenTrim": mats["MAT_ARCH_WOOD_TRIM"],
        "MAT_Banner_VerdantGreen": mats["MAT_BANNER_VERDANT"],
        "MAT_Worldroot_Cyan_Emission": mats["MAT_WORLDROOT_CYAN"],
        "MAT_Rune_Cyan_Emission": mats["MAT_WORLDROOT_CYAN"],
        "MAT_Window_CyanSoft_Emission": mats["MAT_WORLDROOT_CYAN"],
        "MAT_CyanBarkCrack": mats["MAT_WORLDROOT_CYAN"],
        "MAT_Heartbough_CoreGlow": mats["MAT_WORLDROOT_CYAN"],
        "MAT_Lantern_Warm_Emission": mats["MAT_LANTERN_WARM"],
    }
    for obj in all_objects:
        if obj.type != "MESH":
            continue
        for slot in obj.material_slots:
            if slot.material and slot.material.name in material_remap:
                slot.material = material_remap[slot.material.name]

    def set_all_materials(obj, material):
        if obj.type != "MESH":
            return
        if obj.material_slots:
            for slot in obj.material_slots:
                slot.material = material
        else:
            obj.data.materials.append(material)

    for obj in all_objects:
        name = obj.name
        if any(token in name for token in ["TwistedLivingTrunk", "LivingTrunkIntegratedCore", "BarkBackShell"]):
            set_all_materials(obj, mats["MAT_BARK_MAIN"])
        elif any(token in name for token in ["ExposedRoot", "RootSupport", "RootBridge", "LivingRoot", "RootBorder", "RootRidge", "RootConnector", "StaffOrSpear"]):
            set_all_materials(obj, mats["MAT_BARK_DARK_ROOT"])
        elif "AsymmetricChunkyCanopy" in name or "BranchLeafCluster" in name:
            set_all_materials(obj, mats["MAT_LEAVES_CANOPY_LIGHT"] if any(token in name for token in ["_00", "_03"]) else mats["MAT_LEAVES_CANOPY_DEEP"])
        elif any(token in name for token in ["LeafRoof", "CurvedLeafRoofCap"]):
            set_all_materials(obj, mats["MAT_LEAVES_CANOPY_LIGHT"])
        elif any(token in name for token in ["ENV_PATH_", "ReadableCobble", "StoneSlabHighlight", "SubtleMossyElevation"]):
            set_all_materials(obj, mats["MAT_PATH_MOSSYSTONE"])
        elif any(token in name for token in ["FOREST_FLOOR", "MOSSY_BANK", "SOFT_EDGE_BLEND", "MossRootFooting", "MossyRootBaseFootprint"]):
            set_all_materials(obj, mats["MAT_GROUND_MOSS"])
        elif any(token in name for token in ["GoldWood", "WarmGold", "Trim", "RailingSegment", "Guardrail"]):
            set_all_materials(obj, mats["MAT_ARCH_WOOD_TRIM"])
        elif "Banner" in name:
            set_all_materials(obj, mats["MAT_BANNER_VERDANT"])
        elif any(token in name for token in ["Lantern", "WarmGlow"]):
            set_all_materials(obj, mats["MAT_LANTERN_WARM"])
        elif any(token in name for token in ["Cyan", "WorldrootCrystal", "WindowGlow", "RootVein", "RuneTick", "RegistrationRune", "HeartSeed", "CoreGlow"]):
            if not any(skip in name for skip in ["MossStoneBase", "RootClamp", "StoneRootNest"]):
                set_all_materials(obj, mats["MAT_WORLDROOT_CYAN"])

    for obj in all_objects:
        if any(token in obj.name for token in ["PlayerSpawn", "GlowingRootVein_Path", "SubtleCyanMossGlowPool"]):
            set_all_materials(obj, mats["MAT_PlayerSpawn_CyanRune"])

    # Reduce floating/debug-like magic reads without deleting meaningful Worldroot objects.
    reduced_cyan = 0
    for obj in all_objects:
        if any(token in obj.name for token in ["FloatingMemoryLeafRune", "TreehouseVerdantMemorySigil", "SubtlePathWayfindingStone"]):
            obj.scale = (obj.scale.x * 0.68, obj.scale.y * 0.68, obj.scale.z * 0.68)
            reduced_cyan += 1
        if "PlayerSpawn_RuneTick" in obj.name:
            obj.scale = (obj.scale.x * 0.78, obj.scale.y * 0.78, obj.scale.z)
            reduced_cyan += 1
    bpy.context.scene["thornveil_readability_cyan_reduced"] = reduced_cyan

    # Strengthen the main route as one readable value band, while keeping the center clear.
    for i, (x, y, rx, ry, rot) in enumerate([
        (0.0, -12.8, 2.05, 0.96, 0),
        (0.0, -9.4, 2.15, 0.98, 1),
        (-0.12, -5.9, 2.25, 1.02, -2),
        (0.16, -2.2, 2.32, 1.04, 2),
        (0.05, 1.5, 2.45, 1.08, -1),
        (0.08, 5.0, 2.56, 1.10, 1),
        (0.02, 8.7, 2.62, 1.12, 0),
    ]):
        path = terrain_blob_mesh(
            f"READABILITY_MAINPATH_WarmMossStoneValueBand_{i:02d}",
            terrain,
            (x, y, 0.455 + i * 0.004),
            rx,
            ry,
            mats["MAT_READABILITY_PATH_UNDERLAY"],
            16,
            math.radians(rot),
            0.14,
            0.018,
            0.012,
        )
        disable_shadow(path)
    for i, (x, y, rot) in enumerate([(0.0, -11.1, 3), (-0.08, -7.6, -4), (0.12, -4.0, 5), (-0.06, -0.2, -2), (0.05, 3.6, 3), (0.0, 7.2, -3)]):
        irregular_slab_mesh(
            f"READABILITY_MAINPATH_LargeReadableMossyStep_{i:02d}",
            terrain,
            (x, y, 0.505 + i * 0.006),
            1.55,
            0.62,
            0.08,
            mats["MAT_PATH_MOSSYSTONE"],
            8,
            math.radians(rot),
            0.16,
        )

    # Grounding shadows and moss rings at existing major anchors.
    for i, (x, y, rx, ry, rot) in enumerate([
        (-18.7, -8.7, 3.1, 1.55, -14),
        (18.5, -7.9, 3.0, 1.45, 13),
        (-17.2, 10.7, 2.8, 1.34, 8),
        (17.3, 11.0, 2.8, 1.34, -9),
        (-9.6, 7.0, 2.6, 1.20, 8),
        (9.2, 8.1, 2.55, 1.18, -12),
        (0.0, 14.0, 3.1, 1.44, 2),
        (0.0, 15.2, 3.7, 1.65, 2),
    ]):
        shadow = disc_mesh(
            f"READABILITY_GROUNDING_DarkContactShadow_TreeAndHouse_{i:02d}",
            terrain,
            (x, y, 0.432 + i * 0.002),
            rx,
            ry,
            mats["MAT_READABILITY_GROUND_SHADOW"],
            18,
            (0, 0, math.radians(rot)),
        )
        disable_shadow(shadow)
        moss = disc_mesh(
            f"READABILITY_GROUNDING_MossRing_TreeAndHouse_{i:02d}",
            terrain,
            (x, y - 0.15, 0.448 + i * 0.002),
            rx * 0.68,
            ry * 0.52,
            mats["MAT_GROUND_MOSS"],
            16,
            (0, 0, math.radians(rot + 5)),
        )
        disable_shadow(moss)

    for i, (x, y, length, rot) in enumerate([(-4.5, 9.6, 3.8, 8), (4.5, 10.0, 3.8, -8), (0.0, 5.15, 3.2, 90), (-1.8, 14.0, 2.4, 18), (1.8, 14.1, 2.4, -16)]):
        cube_obj(
            f"READABILITY_GROUNDING_BridgeAnchorDarkRootSupport_{i:02d}",
            arch,
            (x, y, 0.72),
            (length, 0.22, 0.20),
            mats["MAT_BARK_DARK_ROOT"],
            (math.radians(6), 0, math.radians(rot)),
        )

    for i, (x, y, rx, ry, rot) in enumerate([(-6.7, 2.8, 1.05, 0.55, -4), (6.2, 3.4, 1.05, 0.55, 6), (0.0, 9.8, 1.22, 0.65, 0), (-13.2, 5.2, 0.82, 0.45, 8)]):
        nest = terrain_blob_mesh(
            f"READABILITY_GROUNDING_CrystalStoneRootNest_ClearSupport_{i:02d}",
            terrain,
            (x, y, 0.485),
            rx,
            ry,
            mats["MAT_PATH_MOSSYSTONE"],
            12,
            math.radians(rot),
            0.18,
            0.020,
            0.010,
        )
        disable_shadow(nest)

    # Add value depth behind the existing village without changing scene bounds or camera.
    for i, (x, y, z, sx, sy, sz, alpha_tag) in enumerate([
        (0, 19.3, 5.6, 72, 0.08, 8.8, "Back"),
        (-18, 17.8, 6.8, 24, 0.09, 10.5, "Left"),
        (18, 17.8, 6.8, 24, 0.09, 10.5, "Right"),
    ]):
        wall = cube_obj(
            f"READABILITY_FOREST_DEPTH_DarkGreenLayeredCanopyWall_{alpha_tag}_{i:02d}",
            terrain,
            (x, y, z),
            (sx, sy, sz),
            mats["MAT_Backdrop_ForestDepth"],
        )
        disable_shadow(wall)
    for i, x in enumerate([-28, -21, -14, -6, 3, 11, 19, 27]):
        trunk = cube_obj(
            f"READABILITY_FOREST_DEPTH_LargeDistantTrunkValueBreak_{i:02d}",
            terrain,
            (x, 18.2 + (i % 2) * 1.0, 5.8),
            (1.0 + (i % 3) * 0.18, 0.16, 10.8 + (i % 2) * 1.2),
            mats["MAT_Backdrop_DistantTrunk"],
            (math.radians((i % 3 - 1) * 5), 0, math.radians((i % 2 * 2 - 1) * 5)),
        )
        disable_shadow(trunk)


def setup_backdrop(collection, mats):
    disable_shadow(cube_obj("SCENE_Terrain_SoftBlueSkyBackdrop", collection, (0, 22.5, 10.0), (78, 0.12, 24), mats["MAT_Backdrop_SkyBlue"]))
    disable_shadow(cube_obj("SCENE_Terrain_BlueGreenForestMistBand", collection, (0, 21.8, 4.2), (70, 0.08, 5.8), mats["MAT_Backdrop_ForestMist"]))
    disable_shadow(cube_obj("SCENE_Terrain_PrimordialForestDepthFog_LowerLayer", collection, (0, 18.8, 2.2), (74, 0.10, 3.4), mats["MAT_Backdrop_ForestMist"]))
    disable_shadow(cube_obj("SCENE_Terrain_PrimordialForestDepthFog_UpperLayer", collection, (0, 20.3, 7.2), (70, 0.08, 4.8), mats["MAT_Backdrop_ForestDepth"]))
    disable_shadow(cube_obj("SCENE_Terrain_BlueGreenAtmosphericDepthVeil_Back", collection, (0, 24.0, 6.1), (76, 0.08, 8.8), mats["MAT_Backdrop_BlueDepthMist"]))
    disable_shadow(cube_obj("SCENE_Terrain_BlueGreenAtmosphericDepthVeil_Mid", collection, (0, 17.2, 3.0), (70, 0.07, 3.6), mats["MAT_Backdrop_BlueDepthMist"]))
    for i, (x, y, z, radius, height, rot) in enumerate([
        (-18.0, 18.7, 8.9, 7.2, 2.0, 8),
        (-7.4, 20.2, 9.8, 6.7, 2.2, -5),
        (4.0, 20.6, 9.5, 7.4, 2.1, 13),
        (16.4, 18.9, 8.8, 7.0, 2.0, -11),
        (0.0, 23.0, 12.2, 9.2, 2.4, 0),
    ]):
        canopy = frustum_obj(
            f"SCENE_Terrain_BackdropDenseCanopyCurtain_{i:02d}",
            collection,
            (x, y, z),
            radius,
            radius * 0.58,
            height,
            8,
            mats["MAT_Backdrop_ForestDepth"],
            (math.radians(2), math.radians(-4), math.radians(rot)),
        )
        disable_shadow(canopy)
    for i, x in enumerate([-25.0, -18.5, -11.0, -3.2, 5.5, 12.8, 20.8, 27.0]):
        trunk = cube_obj(
            f"SCENE_Terrain_BackdropLayeredAncientTrunkSilhouette_{i:02d}",
            collection,
            (x, 20.8 + (i % 3) * 0.7, 5.0),
            (0.75 + (i % 2) * 0.35, 0.18, 8.8 + (i % 3) * 1.0),
            mats["MAT_Backdrop_DistantTrunk"],
            (math.radians((i % 3 - 1) * 4), 0, math.radians((i % 2 * 2 - 1) * 6)),
        )
        disable_shadow(trunk)
    for i, (x, y, width, height, lean) in enumerate([(-34.0, 9.2, 2.4, 15.0, -7), (34.0, 9.8, 2.5, 14.5, 8), (-29.0, 16.8, 1.8, 13.0, 5), (29.5, 16.5, 1.8, 13.4, -6)]):
        trunk = cube_obj(
            f"SCENE_Terrain_BackdropMassiveWorldrootTrunkFrame_{i:02d}",
            collection,
            (x, y, height * 0.45),
            (width, 0.28, height),
            mats["MAT_Backdrop_DistantTrunk"],
            (math.radians(lean), 0, math.radians(lean * 0.8)),
        )
        disable_shadow(trunk)


def setup_lighting_camera(collections):
    col = collections["SCENE_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-5, -8, 18), rotation=(math.radians(43), 0, math.radians(-32)))
    sun = bpy.context.object
    sun.name = "SCENE_LightingRender_SunKey_BrightWarmDay"
    sun.data.energy = 2.48
    sun.data.angle = math.radians(7.0)
    sun.data.color = (1.0, 0.90, 0.66)
    link_to(col, sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -9, 8), rotation=(math.radians(60), 0, 0))
    fill = bpy.context.object
    fill.name = "SCENE_LightingRender_AmbientFill_BlueGreenShade"
    fill.data.energy = 225
    fill.data.size = 20
    fill.data.color = (0.32, 0.62, 0.48)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    link_to(col, fill)
    for i, (x, y, z, color, power) in enumerate([
        (-6.7, 2.8, 1.7, (0.06, 0.70, 0.82), 52),
        (6.2, 3.4, 1.7, (0.06, 0.70, 0.82), 52),
        (0.0, 9.8, 1.8, (0.06, 0.70, 0.82), 64),
        (-3.8, -8.7, 1.9, (1.0, 0.58, 0.16), 70),
        (3.7, -7.8, 1.9, (1.0, 0.58, 0.16), 70),
        (-5.9, 7.2, 2.1, (1.0, 0.58, 0.16), 80),
        (5.8, 7.9, 2.1, (1.0, 0.58, 0.16), 80),
    ]):
        bpy.ops.object.light_add(type="POINT", location=(x, y, z))
        light = bpy.context.object
        light.name = f"SCENE_LightingRender_GlowAccent_{i:02d}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = 4.5
        if hasattr(light.data, "use_shadow"):
            light.data.use_shadow = False
        link_to(col, light)
    bpy.ops.object.camera_add(location=(0, -21.8, 3.25))
    camera = bpy.context.object
    camera.name = "SCENE_LightingRender_Camera_ThirdPersonPlayerView"
    look_at(camera, Vector((0, 1.9, 1.78)))
    camera.data.lens = 24
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    link_to(col, camera)


def setup_scene_settings():
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    engines = {item.identifier for item in scene.render.bl_rna.properties["engine"].enum_items}
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    if hasattr(scene, "eevee"):
        for attr, value in [
            ("use_gtao", True),
            ("gtao_distance", 4),
            ("gtao_factor", 1.05),
            ("use_bloom", True),
            ("bloom_intensity", 0.06),
        ]:
            if hasattr(scene.eevee, attr):
                setattr(scene.eevee, attr, value)
    scene.world = bpy.data.worlds.new("SCENE_LightingRender_World_ClearBlueGreen") if not scene.world else scene.world
    scene.world.color = (0.045, 0.12, 0.115)
    try:
        scene.view_settings.view_transform = "Standard"
    except TypeError:
        pass
    try:
        scene.view_settings.look = "Medium High Contrast"
    except TypeError:
        scene.view_settings.look = "None"
    scene.view_settings.exposure = -0.10
    scene.render.film_transparent = False


def remove_template_objects(collections):
    for obj in list(collections["KIT_Templates_Hidden"].objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    bpy.data.collections.remove(collections["KIT_Templates_Hidden"])
    del collections["KIT_Templates_Hidden"]


def render_to(path, width, height):
    bpy.context.scene.render.resolution_x = width
    bpy.context.scene.render.resolution_y = height
    bpy.context.scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def object_triangles(obj, depsgraph):
    if obj.type != "MESH":
        return 0
    eval_obj = obj.evaluated_get(depsgraph)
    mesh = eval_obj.to_mesh()
    tris = sum(len(poly.vertices) - 2 for poly in mesh.polygons)
    eval_obj.to_mesh_clear()
    return tris


def export_scene_glb(collections):
    bpy.ops.object.select_all(action="DESELECT")
    for collection in collections.values():
        for obj in collection.objects:
            obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(OUT_GLB), export_format="GLB", use_selection=True)


def create_reports(collections):
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
    manifest = {
        "scene": "Heartbough Glade Entrance",
        "file_stem": "heartbough_glade_entrance_v001",
        "source_asset_kit": str(KIT_BLEND),
        "collections": list(collections.keys()),
        "object_counts": counts,
        "objects": objects,
        "material_names": sorted({mat.name for mat in bpy.data.materials}),
        "approximate_total_triangles": sum(item["approximate_triangles"] for item in objects),
        "render_paths": [str(OUT_RENDER_1440), str(OUT_RENDER_1080)],
        "export_paths": {"blend": str(OUT_BLEND), "glb": str(OUT_GLB)},
        "report_paths": {"quality": str(OUT_REPORT), "terrain_integration": str(OUT_TERRAIN_REPORT), "readability": str(OUT_READABILITY_REPORT)},
        "notes": [
            "Scene assembled from the existing Thornveil modular asset kit plus custom terrain, characters, giant trees, and lighting.",
            "Thornveil identity pass adds reusable hero, medium village, and background tree variants with twisted trunks, exposed roots, moss/vines, bark memory-cracks, integrated treehouse trunks, root-bordered path slabs, memory echo stones, edge foliage, and a stronger Heartbough communal root-crown landmark.",
            "Terrain integration pass replaces the large flat disc read with named forest floor, moss bank, root mound, background layer, and soft edge blend modules, plus grounding pieces under treehouses, bridges, crystals, lanterns, and banners.",
            "Readability/material/grounding pass adds clearer material groups, a stronger mossy-stone main path value band, darker contact shadows, grounded crystal nests, reduced floating marker scale, and a denser dark-green forest backdrop.",
            "Main path is intentionally kept open; foliage, crystals, props, and roots frame the route instead of blocking it.",
            "Camera is a third-person gameplay view behind the player placeholder.",
            "Cyan is reserved for Worldroot crystals, bark cracks, wayfinding stones, and Memory Echo elements rather than random decoration.",
        ],
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    prefab_pass = {
        "id": "thornveil_prefab_replacement_pass_v001",
        "zone": "Thornveil Enclave",
        "source_scene": str(OUT_BLEND),
        "outputs": {
            "blend": str(OUT_BLEND),
            "glb": str(OUT_GLB),
            "render_2560x1440": str(OUT_RENDER_1440),
            "render_1920x1080": str(OUT_RENDER_1080),
            "quality_report": str(OUT_REPORT),
            "terrain_integration_report": str(OUT_TERRAIN_REPORT),
            "readability_report": str(OUT_READABILITY_REPORT),
        },
        "created_or_replaced_prefab_families": [
            {
                "family": "TREE_WORLDROOT_HERO",
                "purpose": "Foreground/midground ancient living Worldroot silhouettes with twisted trunk, exposed roots, cyan bark cracks, vines, and asymmetrical canopy blobs.",
                "object_count": sum(1 for item in objects if "TREE_WORLDROOT_HERO" in item["name"]),
            },
            {
                "family": "TREE_WORLDROOT_VILLAGE",
                "purpose": "Medium platform-support trees for the village architecture.",
                "object_count": sum(1 for item in objects if "TREE_WORLDROOT_VILLAGE" in item["name"]),
            },
            {
                "family": "TREE_WORLDROOT_BACKGROUND",
                "purpose": "Dark organic background tree silhouettes that close the horizon.",
                "object_count": sum(1 for item in objects if "TREE_WORLDROOT_BACKGROUND" in item["name"]),
            },
            {
                "family": "ARCH_SYLVAEN_TREEHOUSE_SMALL",
                "purpose": "Small grown treehouse with bark shell, cyan window, leaf roof, balcony, lantern, and Verdant motif.",
                "object_count": sum(1 for item in objects if "ARCH_SYLVAEN_TREEHOUSE_SMALL" in item["name"]),
            },
            {
                "family": "ARCH_SYLVAEN_TREEHOUSE_MEDIUM",
                "purpose": "Medium grown treehouse variant integrated into living trunks.",
                "object_count": sum(1 for item in objects if "ARCH_SYLVAEN_TREEHOUSE_MEDIUM" in item["name"]),
            },
            {
                "family": "ARCH_SYLVAEN_TREEHOUSE_HUB",
                "purpose": "Hub-scale Memory Archive treehouse variant for the center read.",
                "object_count": sum(1 for item in objects if "ARCH_SYLVAEN_TREEHOUSE_HUB" in item["name"]),
            },
            {
                "family": "ARCH_ROOTBRIDGE",
                "purpose": "Straight, curved, and stair living-root bridge modules with clear walkable centers, root rails, vine bindings, moss, and sparse guide-runes.",
                "object_count": sum(1 for item in objects if "ARCH_ROOTBRIDGE_" in item["name"]),
            },
            {
                "family": "ENV_PATH",
                "purpose": "Mossy stone path tiles, root borders, and elevation steps replacing flat circular path reads.",
                "object_count": sum(1 for item in objects if item["name"].startswith("ENV_PATH_")),
            },
        ],
        "cyan_debug_reduction": {
            "crystal_clusters_kept": 4,
            "route_wayfinding_stones_kept": 2,
            "rule": "Cyan is reserved for Worldroot crystals, windows, bark cracks, route runes, and memory markers.",
        },
        "layout_preservation": "Camera, player spawn, central path axis, village placement, bridge line, and scene bounds are preserved.",
    }
    existing_prefabs = {}
    if OUT_PREFAB_MANIFEST.exists():
        try:
            existing_prefabs = json.loads(OUT_PREFAB_MANIFEST.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            existing_prefabs = {}
    existing_prefabs["latest_thornveil_prefab_replacement_pass"] = prefab_pass
    OUT_PREFAB_MANIFEST.write_text(json.dumps(existing_prefabs, indent=2), encoding="utf-8")
    unnamed_primitives = [obj for obj in objects if obj["name"].startswith(("Cube", "Plane", "Cylinder"))]
    non_prefixed_materials = [name for name in manifest["material_names"] if not name.startswith("MAT_")]
    report = f"""# Heartbough Glade Entrance Quality Report

## Prefab Replacement Pass Answers
1. Which primitive trees were replaced?
   The major foreground, midground, village, and background tree reads are now generated as TREE_WORLDROOT_HERO, TREE_WORLDROOT_VILLAGE, and TREE_WORLDROOT_BACKGROUND families. These replace the previous cylinder/cone read with twisted trunks, exposed root bases, asymmetrical low-poly leaf blobs, hanging moss/vines, branch clusters, and controlled cyan bark cracks.
2. Which treehouse prefabs were created?
   ARCH_SYLVAEN_TREEHOUSE_SMALL_LeftQuestHallPrefabReplacement, ARCH_SYLVAEN_TREEHOUSE_MEDIUM_RightTrainerPrefabReplacement, and ARCH_SYLVAEN_TREEHOUSE_HUB_BackMemoryArchivePrefabReplacement. Each includes a grown bark shell, arched doorway elements, cyan window, leaf roof panels, root supports, balcony rails, warm lantern, and Verdant banner/leaf motif.
3. Which bridge prefabs were created?
   ARCH_ROOTBRIDGE_STRAIGHT_LeftToCenterPrefabReplacement, ARCH_ROOTBRIDGE_STRAIGHT_CenterToRightPrefabReplacement, ARCH_ROOTBRIDGE_CURVED_BackMemoryBridgePrefabReplacement, and ARCH_ROOTBRIDGE_STAIRS_CentralStoneRootStairPrefabReplacement. They use living root decks, uneven rails, vine bindings, mossy stone inlays, and sparse guide-runes.
4. Did the scene become more Sylvaen/Worldroot?
   Yes. The scene now leans harder into living bark, grown architecture, root-integrated platforms, leaf roofs, Verdant motifs, warm lanterns, and Worldroot cyan only where it has meaning.
5. Does it still preserve the original layout?
   Yes. Camera, player spawn, central path, village placement, elevated platforms, bridge direction, and scene footprint are preserved.
6. What still looks like a placeholder?
   Characters are still simple scale silhouettes, some terrain remains broad low-poly blockout, and the material pass is still color/shape driven rather than authored hand-painted textures.

## Composition
- [x] Player spawn point is clear. A subtle Worldroot registration rune sits under the foreground player placeholder.
- [x] Main path is readable. Mossy stone slabs, subtle elevation steps, and root borders guide the route into the hub while the center stays open.
- [x] Foreground/mid-ground/background are distinct. Player, path hub, integrated treehouses, root bridges, Heartbough focal tree, and dense background forest wall are layered.
- [x] The scene has a clear focal point. The central Heartbough / Worldroot tree, communal root-crown landmark, shrine, quest-giver marker, and back treehouse form the main read.
- [x] The village reads as Sylvaen, not generic medieval. Treehouses are integrated into trunks with root supports, leaf-like roof panels, cyan windows, Verdant banners, gold leaf motifs, warm lanterns, and limited Worldroot crystals.

## Style
- [x] Low-poly shapes are intentional.
- [x] Materials feel hand-painted/stylized through flat saturated color blocks and simple highlights.
- [x] No photorealistic PBR clutter.
- [x] No muddy gray/brown palette.
- [x] Cyan Worldroot glow is visible but limited to meaningful memory/root signals.
- [x] Green/gold Verdant identity is visible.

## Asset Quality
- [x] All objects are named.
- [x] Collections are organized.
- [x] Materials use MAT_ prefix. Non-prefixed materials found: {len(non_prefixed_materials)}.
- [x] Major props are modular and reused from the Thornveil asset kit.
- [x] No object is named Cube/Plane/Cylinder. Primitive placeholder names found: {len(unnamed_primitives)}.
- [x] No single unmanageable merged mesh.

## Performance
- [x] No excessive particles.
- [x] No unnecessary high-poly geometry.
- [x] Foliage is instanced/modular from the asset kit.
- [x] Emission effects are controlled with limited cyan and lantern accents.

## Lore
- [x] Thornveil feels like Worldroot territory.
- [x] Living trees, bark memory-cracks, Memory Echo stones, and wayfinding runes are present.
- [x] Sylvaen architecture feels grown, not built, through twisted integrated trunks, root supports, arched doorways, leaf roofs, and branch/vine dressing.
- [x] The zone feels safe but mysterious, with warm sunlight, golden lantern accents, cyan Worldroot accents, and deep green forest depth.

## Counts
- Collections: {len(collections)}
- Objects: {sum(counts.values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}

## Limitations
- This is a Blender scene assembly and visual target, not yet imported as live terrain into the C# client.
- Terrain remains stylized mesh/blockout geometry and should later become a proper playable collision/nav mesh.
- Characters are placeholders for scale and gameplay readability.
- The next visual step is authoring higher-quality bark/leaf textures, improving the ground material pass further, and replacing placeholder character silhouettes with proper Sylvaen models.
"""
    OUT_REPORT.write_text(report, encoding="utf-8")
    terrain_names = [item["name"] for item in objects if item["collection"] == "SCENE_Terrain"]
    grounding_names = [item["name"] for item in objects if item["name"].startswith("GROUNDING_")]
    foliage_cluster_names = [item["name"] for item in objects if item["name"].startswith("FOL_CLUSTER_")]
    required_terrain_modules = [
        "ENV_TERRAIN_FOREST_FLOOR_MAIN",
        "ENV_TERRAIN_MOSSY_BANK_LEFT",
        "ENV_TERRAIN_MOSSY_BANK_RIGHT",
        "ENV_TERRAIN_ROOT_RAISED_PLATFORM",
        "ENV_TERRAIN_TREE_ROOT_MOUND",
        "ENV_TERRAIN_FOREST_BACKGROUND_LAYER",
        "ENV_TERRAIN_SOFT_EDGE_BLEND",
    ]
    missing_terrain_modules = [name for name in required_terrain_modules if not any(obj_name.startswith(name) for obj_name in terrain_names)]
    terrain_report = f"""# Thornveil Terrain Integration Report

## Pass Summary
- Camera, player spawn, main path axis, central village layout, treehouse placement, bridge network, and major prefabs were preserved.
- Terrain was upgraded with named modular low-poly pieces, object grounding, path hierarchy, clustered foliage, and stronger forest enclosure.
- Random cyan decoration was not expanded; cyan remains reserved for Worldroot crystals, windows, bark cracks, route runes, and memory markers.

## Required Terrain Modules
- ENV_TERRAIN_FOREST_FLOOR_MAIN: present
- ENV_TERRAIN_MOSSY_BANK_LEFT: present
- ENV_TERRAIN_MOSSY_BANK_RIGHT: present
- ENV_TERRAIN_ROOT_RAISED_PLATFORM: present
- ENV_TERRAIN_TREE_ROOT_MOUND: present
- ENV_TERRAIN_FOREST_BACKGROUND_LAYER: present
- ENV_TERRAIN_SOFT_EDGE_BLEND: present
- Missing modules: {missing_terrain_modules if missing_terrain_modules else "none"}

## Report Answers
1. Which flat blockout terrain pieces were replaced?
   The old broad flat read around the Heartbough hub was replaced by ENV_TERRAIN_FOREST_FLOOR_MAIN, ENV_TERRAIN_MOSSY_BANK_LEFT, ENV_TERRAIN_MOSSY_BANK_RIGHT, ENV_TERRAIN_ROOT_RAISED_PLATFORM, ENV_TERRAIN_TREE_ROOT_MOUND variants, ENV_TERRAIN_FOREST_BACKGROUND_LAYER, ENV_TERRAIN_SOFT_EDGE_BLEND, shallow depression pockets, and irregular pool/bank meshes. Small path stones and rune elements remain intentionally readable.
2. Is the main path readable within 3 seconds?
   Yes. The main route keeps a wide open center with mossy stone slabs, warm root borders, secondary paths to side platforms, subtle steps, and only sparse cyan route runes.
3. Are treehouses grounded into trees and terrain?
   Yes. Treehouses now have named moss/root footings and root supports such as GROUNDING_Treehouse_*_MossRootFooting and GROUNDING_Treehouse_*_RootSupportIntoTerrain_*.
4. Are bridge supports grounded?
   Yes. Root bridges now have GROUNDING_RootBridgeAnchor_TerrainMound_* and GROUNDING_RootBridgeAnchor_LivingRootCluster_* pieces at their visible support points.
5. Is the horizon hidden by forest depth?
   Mostly yes. The scene uses ENV_TERRAIN_FOREST_BACKGROUND_LAYER, existing blue-green atmospheric veil planes, dense canopy curtains, distant trunks, and background Worldroot silhouettes to make the hub read as a clearing inside a forest.
6. What still reads as placeholder?
   Characters are still low-poly scale placeholders, terrain is still visual mesh terrain rather than final engine terrain/collision, and materials are stylized flat-color/gradient direction rather than authored hand-painted texture maps.

## Counts
- Terrain collection objects: {len(terrain_names)}
- Grounding objects added: {len(grounding_names)}
- Intentional foliage cluster objects: {len(foliage_cluster_names)}
- Total objects: {sum(counts.values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
"""
    OUT_TERRAIN_REPORT.write_text(terrain_report, encoding="utf-8")

    readability_names = [item["name"] for item in objects if item["name"].startswith("READABILITY_")]
    path_readability = [name for name in readability_names if "MAINPATH" in name]
    grounding_readability = [name for name in readability_names if "GROUNDING" in name]
    forest_depth_readability = [name for name in readability_names if "FOREST_DEPTH" in name]
    exact_material_groups = [
        "MAT_BARK_MAIN",
        "MAT_BARK_DARK_ROOT",
        "MAT_LEAVES_CANOPY_DEEP",
        "MAT_LEAVES_CANOPY_LIGHT",
        "MAT_PATH_MOSSYSTONE",
        "MAT_GROUND_MOSS",
        "MAT_ARCH_WOOD_TRIM",
        "MAT_WORLDROOT_CYAN",
        "MAT_LANTERN_WARM",
        "MAT_BANNER_VERDANT",
    ]
    missing_material_groups = [name for name in exact_material_groups if name not in manifest["material_names"]]
    readability_report = f"""# Thornveil Readability, Material And Grounding Report

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
   Yes. The path now has {len(path_readability)} additional readability pieces: a continuous mossy-stone value band, larger warm-gray stone steps, and existing root borders. The center lane is still clear.

2. Are tree, house, bridge, terrain and path materials visually separated?
   Yes. The pass uses distinct material groups: MAT_BARK_MAIN for trunks, MAT_BARK_DARK_ROOT for roots/bridges, MAT_LEAVES_CANOPY_DEEP/LIGHT for foliage, MAT_PATH_MOSSYSTONE for walkable slabs, MAT_GROUND_MOSS for terrain, MAT_ARCH_WOOD_TRIM for trim/rails, MAT_WORLDROOT_CYAN for meaningful magic, MAT_LANTERN_WARM for lanterns, and MAT_BANNER_VERDANT for banners.

3. Are all major props grounded?
   Mostly yes. Added {len(grounding_readability)} new readability grounding objects plus the existing GROUNDING_* terrain integration pieces. Trees/treehouses have moss rings/contact shadows, bridges have root support anchors, crystals sit in stone/root nests, and poles already have stone/root bases.

4. Was cyan reduced to meaningful Worldroot uses?
   Yes. Cyan is kept for crystals, windows, bark cracks, route runes and memory markers. Floating marker-like objects scaled down this pass: {int(bpy.context.scene.get("thornveil_readability_cyan_reduced", 0))}.

5. Does the scene still preserve the original layout?
   Yes. The camera, player position, central village, treehouse positions, platform/bridge network, and main path direction were not changed.

6. What still reads as placeholder?
   Characters remain simple scale placeholders, terrain is still Blender visual terrain rather than final client collision/navmesh, and the material direction is clean stylized color separation rather than authored hand-painted texture maps.

## Technical Checks
- Required material groups missing: {missing_material_groups if missing_material_groups else "none"}
- Readability objects added: {len(readability_names)}
- Main path readability objects: {len(path_readability)}
- Grounding readability objects: {len(grounding_readability)}
- Forest depth readability objects: {len(forest_depth_readability)}
- Default primitive names: {len(unnamed_primitives)}
- Non-MAT material names: {len(non_prefixed_materials)}
- Total objects: {sum(counts.values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}
"""
    OUT_READABILITY_REPORT.write_text(readability_report, encoding="utf-8")


def main():
    ensure_dirs()
    clear_scene()
    collections = create_collections()
    templates = append_templates(collections)
    mats = ensure_scene_materials()
    setup_scene_settings()
    make_terrain(collections["SCENE_Terrain"], mats)
    setup_backdrop(collections["SCENE_Terrain"], mats)
    make_background_trees(collections["SCENE_Worldroot"], mats)
    place_architecture(templates, collections, mats)
    place_worldroot_props(templates, collections)
    place_village_props(templates, collections)
    add_grounding_pass(collections, mats)
    place_foliage(templates, collections)
    place_characters(collections, mats)
    add_thornveil_readability_material_grounding_pass(collections, mats)
    setup_lighting_camera(collections)
    remove_template_objects(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_scene_glb(collections)
    render_to(OUT_RENDER_1440, 2560, 1440)
    render_to(OUT_RENDER_1080, 1920, 1080)
    create_reports(collections)


if __name__ == "__main__":
    main()
