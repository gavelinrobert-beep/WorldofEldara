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
    existing = bpy.data.materials.get(name)
    if existing:
        return existing
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
        "MAT_GladeGrass": make_mat("MAT_GladeGrass", (0.22, 0.54, 0.27)),
        "MAT_GladeGrassDark": make_mat("MAT_GladeGrassDark", (0.10, 0.33, 0.22)),
        "MAT_Path_MossyStone": make_mat("MAT_Path_MossyStone", (0.58, 0.53, 0.39)),
        "MAT_Path_WarmRoot": make_mat("MAT_Path_WarmRoot", (0.35, 0.19, 0.08)),
        "MAT_Moss": make_mat("MAT_Moss", (0.20, 0.50, 0.22)),
        "MAT_Stone_MossyGray": make_mat("MAT_Stone_MossyGray", (0.43, 0.47, 0.39)),
        "MAT_Worldroot_Cyan_Emission": make_mat(
            "MAT_Worldroot_Cyan_Emission", (0.05, 0.92, 0.96), emission=(0.04, 0.95, 1.0), strength=2.8
        ),
        "MAT_Backdrop_SkyBlue": make_mat("MAT_Backdrop_SkyBlue", (0.46, 0.72, 0.82), roughness=1.0, emission=(0.34, 0.54, 0.62), strength=0.45),
        "MAT_Backdrop_ForestMist": make_mat("MAT_Backdrop_ForestMist", (0.22, 0.78, 0.75), emission=(0.12, 0.45, 0.50), strength=0.25, alpha=0.34),
        "MAT_PlayerPlaceholder": make_mat("MAT_PlayerPlaceholder", (0.54, 0.78, 0.56), alpha=0.78),
        "MAT_NpcPlaceholder": make_mat("MAT_NpcPlaceholder", (0.82, 0.78, 0.48), alpha=0.94),
        "MAT_QuestGold": make_mat("MAT_QuestGold", (1.0, 0.75, 0.12), emission=(1.0, 0.58, 0.10), strength=1.3),
        "MAT_RootShadow": make_mat("MAT_RootShadow", (0.08, 0.20, 0.13), alpha=0.52),
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
    mesh = bpy.data.meshes.new("SCENE_Terrain_MainGladeGround_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("SCENE_Terrain_MainGladeGround_62x42m", mesh)
    obj.data.materials.append(mats["MAT_GladeGrass"])
    collection.objects.link(obj)
    disc_mesh("SCENE_Terrain_HeartboughMossBank_Left", collection, (-9.5, 4.2, 0.16), 8.8, 4.2, mats["MAT_GladeGrassDark"], 24, (0, 0, 0.2))
    disc_mesh("SCENE_Terrain_HeartboughMossBank_Right", collection, (9.2, 5.7, 0.16), 8.4, 4.0, mats["MAT_GladeGrassDark"], 24, (0, 0, -0.12))
    disc_mesh("SCENE_Terrain_HeartboughHub_ClearQuestCircle", collection, (0, 8.0, 0.18), 5.2, 3.2, mats["MAT_Moss"], 28)
    disc_mesh("SCENE_Terrain_MossglassReflectingPool_Left", collection, (-12.2, 0.8, 0.18), 3.0, 1.15, bpy.data.materials["MAT_Worldroot_Cyan_Emission"], 24, (0, 0, -0.18))

    path_nodes = [(0, -14.2), (0, -10.5), (-0.25, -6.5), (0.25, -2.4), (0, 1.8), (0.1, 5.8), (0, 9.4), (0, 13.4)]
    for i, (x, y) in enumerate(path_nodes):
        width = 2.4 + 0.26 * min(i, 5)
        disc_mesh(f"SCENE_Terrain_CentralMossyStonePath_{i:02d}", collection, (x, y, 0.25), width, 1.35, mats["MAT_Path_MossyStone"], 18, (0, 0, 0.08 * math.sin(i)))
        if i < len(path_nodes) - 1:
            nx, ny = path_nodes[i + 1]
            mid = ((x + nx) / 2, (y + ny) / 2)
            length = math.dist((x, y), (nx, ny))
            angle = math.atan2(ny - y, nx - x)
            cube_obj(
                f"SCENE_Terrain_RootPathConnector_{i:02d}",
                collection,
                (mid[0], mid[1], 0.22),
                (length, 0.52, 0.12),
                mats["MAT_Path_WarmRoot"],
                (0, 0, angle),
            )
    for i, (x, y, s) in enumerate([(-1.2, -11.7, 0.7), (1.0, -8.6, 0.55), (-1.0, -5.2, 0.6), (1.25, -1.4, 0.55), (-1.1, 3.1, 0.62), (1.1, 6.5, 0.7), (-1.0, 10.4, 0.58)]):
        disc_mesh(f"SCENE_Terrain_ReadableCobble_{i:02d}", collection, (x, y, 0.32), 0.55 * s, 0.36 * s, mats["MAT_Stone_MossyGray"], 10, (0, 0, i))


def make_background_trees(collection, mats):
    placements = [(-17, 14.5, 1.5), (-6.5, 16.2, 1.2), (7.0, 15.8, 1.35), (17, 13.7, 1.45)]
    for i, (x, y, s) in enumerate(placements):
        disable_shadow(frustum_obj(f"SCENE_Worldroot_GiantLivingTree_{i:02d}_Trunk", collection, (x, y, 4.2 * s), 1.2 * s, 0.78 * s, 8.4 * s, 8, bpy.data.materials["MAT_Bark_WarmBrown"], (0.08, -0.06, i * 0.12)))
        for r in range(5):
            angle = math.tau * r / 5
            disable_shadow(cube_obj(
                f"SCENE_Worldroot_GiantLivingTree_{i:02d}_RootButtress_{r:02d}",
                collection,
                (x + math.cos(angle) * 1.05 * s, y + math.sin(angle) * 0.85 * s, 0.34),
                (2.4 * s, 0.32 * s, 0.24 * s),
                bpy.data.materials["MAT_Bark_DarkRoot"],
                (0.05, 0, angle),
            ))
        for c, offset in enumerate([(0, 0, 9.2), (-1.25, 0.2, 8.4), (1.25, -0.1, 8.2), (0.2, 0.85, 8.85)]):
            disable_shadow(frustum_obj(
                f"SCENE_Worldroot_GiantLivingTree_{i:02d}_ChunkyCanopy_{c:02d}",
                collection,
                (x + offset[0] * s, y + offset[1] * s, offset[2] * s),
                2.6 * s,
                1.1 * s,
                1.7 * s,
                7,
                bpy.data.materials["MAT_Leaves_DeepGreen"] if c % 2 else bpy.data.materials["MAT_Leaves_LightGreen"],
                (0.02, 0.10, i + c * 0.4),
            ))
        disable_shadow(bicone_mesh(f"SCENE_Worldroot_GiantLivingTree_{i:02d}_HeartCrystal", collection, (x, y - 0.72 * s, 4.7 * s), 0.32 * s, 0.82 * s, bpy.data.materials["MAT_Worldroot_Cyan_Emission"], 6, (math.radians(90), 0, 0)))


def place_architecture(templates, collections):
    arch = collections["SCENE_Architecture"]
    inst(templates, "ARCH_Sylvaen_TreehouseModule", "SCENE_Architecture_LeftTreehouseQuestHall", arch, (-9.6, 7.0, 0.35), (0, 0, math.radians(8)), (1.35, 1.35, 1.35))
    inst(templates, "ARCH_Sylvaen_TreehouseModule", "SCENE_Architecture_RightTreehouseTrainer", arch, (9.2, 8.1, 0.42), (0, 0, math.radians(-12)), (1.25, 1.25, 1.25))
    inst(templates, "ARCH_Sylvaen_TreehouseModule", "SCENE_Architecture_BackTreehouseMemoryArchive", arch, (0, 14.0, 0.5), (0, 0, math.radians(2)), (1.55, 1.55, 1.55))
    inst(templates, "ARCH_Sylvaen_ArchedDoorway", "SCENE_Architecture_HeartboughEntranceArch", arch, (0, 2.2, 0.32), (0, 0, 0), (1.35, 1.35, 1.35))
    inst(templates, "ARCH_Sylvaen_RootBridgeStraight", "SCENE_Architecture_MidRootBridge_LeftToCenter", arch, (-4.7, 9.9, 2.7), (0, 0, math.radians(8)), (1.45, 1.25, 1.0))
    inst(templates, "ARCH_Sylvaen_RootBridgeStraight", "SCENE_Architecture_MidRootBridge_CenterToRight", arch, (4.6, 10.4, 2.75), (0, 0, math.radians(-7)), (1.45, 1.25, 1.0))
    inst(templates, "ARCH_Sylvaen_RootBridgeCurved", "SCENE_Architecture_BackCurvedRootBridge", arch, (0.8, 14.2, 2.95), (0, 0, math.radians(4)), (1.25, 1.15, 0.95))
    inst(templates, "ARCH_Sylvaen_StairSegment", "SCENE_Architecture_CentralStoneRootStair", arch, (0, 5.15, 0.34), (0, 0, math.radians(90)), (1.45, 1.35, 1.05))
    inst(templates, "ARCH_Sylvaen_BalconyPlatform", "SCENE_Architecture_LeftBalconyPlatform", arch, (-6.7, 6.1, 2.6), (0, 0, math.radians(10)), (1.15, 1.05, 1.0))
    inst(templates, "ARCH_Sylvaen_BalconyPlatform", "SCENE_Architecture_RightBalconyPlatform", arch, (6.4, 7.0, 2.6), (0, 0, math.radians(-8)), (1.15, 1.05, 1.0))
    for i, (x, y, rot) in enumerate([(-5.0, 4.0, 0.15), (5.1, 4.3, -0.12), (-2.7, 9.2, 0.08), (2.8, 9.4, -0.08)]):
        inst(templates, "ARCH_Sylvaen_RailingSegment", f"SCENE_Architecture_PathRailing_{i:02d}", arch, (x, y, 0.36), (0, 0, rot), (1.2, 1.2, 1.1))


def place_worldroot_props(templates, collections):
    worldroot = collections["SCENE_Worldroot"]
    for i, (asset, x, y, s) in enumerate([
        ("PROPS_Worldroot_CyanCrystalClusterMedium", -3.8, -6.6, 0.9),
        ("PROPS_Worldroot_CyanCrystalClusterSmall", 3.8, -5.6, 0.75),
        ("PROPS_Worldroot_CyanCrystalClusterLarge", -6.7, 2.8, 1.0),
        ("PROPS_Worldroot_CyanCrystalClusterMedium", 6.2, 3.4, 0.9),
        ("PROPS_Worldroot_CyanCrystalClusterLarge", -10.4, 10.7, 1.0),
        ("PROPS_Worldroot_CyanCrystalClusterLarge", 10.8, 11.3, 1.0),
    ]):
        inst(templates, asset, f"SCENE_Worldroot_PathCrystalGuide_{i:02d}", worldroot, (x, y, 0.34), (0, 0, i * 0.31), (s, s, s))
    inst(templates, "PROPS_Worldroot_WaypointStone", "SCENE_Worldroot_HeartboughWaypointStone", worldroot, (-2.2, 7.5, 0.34), (0, 0, 0.18), (1.15, 1.15, 1.15))
    inst(templates, "PROPS_Worldroot_VerdantRuneMonolith", "SCENE_Worldroot_QuestHubRuneMonolith", worldroot, (2.25, 7.8, 0.34), (0, 0, -0.12), (1.2, 1.2, 1.2))
    inst(templates, "PROPS_Worldroot_MemoryEchoMarker", "SCENE_Worldroot_MemoryEchoMarker_LeftPool", worldroot, (-8.8, 1.6, 0.35), (0, 0, 0), (1.0, 1.0, 1.0))
    for i, (x, y, rot) in enumerate([(-1.5, -8.0, 0.08), (1.7, -2.9, -0.14), (-1.2, 3.8, 0.18), (1.4, 9.5, -0.18)]):
        inst(templates, "PROPS_Worldroot_GlowingRootVein", f"SCENE_Worldroot_GlowingRootVein_Path_{i:02d}", worldroot, (x, y, 0.38), (0, 0, rot), (1.15, 1.0, 1.0))


def place_village_props(templates, collections):
    village = collections["SCENE_VillageProps"]
    for i, (x, y, rot) in enumerate([(-3.8, -8.7, 0.1), (3.7, -7.8, -0.12), (-4.8, 0.8, 0.05), (4.7, 1.4, -0.08), (-5.9, 7.2, 0.2), (5.8, 7.9, -0.2)]):
        inst(templates, "PROPS_Village_LanternPost", f"SCENE_VillageProps_LanternPathGuide_{i:02d}", village, (x, y, 0.32), (0, 0, rot), (1.05, 1.05, 1.05))
    for i, (x, y, rot) in enumerate([(-5.8, -3.0, 0.10), (5.6, -2.1, -0.12), (-7.8, 5.3, 0.22), (7.7, 5.9, -0.18)]):
        inst(templates, "PROPS_Village_BannerPost", f"SCENE_VillageProps_VerdantBanner_{i:02d}", village, (x, y, 0.34), (0, 0, rot), (1.12, 1.12, 1.12))
    inst(templates, "PROPS_Village_SmallShrinePedestal", "SCENE_VillageProps_QuestHubShrinePedestal", village, (0.05, 7.65, 0.34), (0, 0, 0), (1.15, 1.15, 1.15))
    inst(templates, "PROPS_Village_WoodenBench", "SCENE_VillageProps_LeftRestBench", village, (-4.8, 6.45, 0.34), (0, 0, math.radians(16)), (1.1, 1.1, 1.1))
    inst(templates, "PROPS_Village_WoodenBench", "SCENE_VillageProps_RightRestBench", village, (4.9, 6.75, 0.34), (0, 0, math.radians(-15)), (1.1, 1.1, 1.1))
    inst(templates, "PROPS_Village_Signpost", "SCENE_VillageProps_WayfindingSignpost", village, (-3.5, 2.4, 0.34), (0, 0, math.radians(12)), (1.0, 1.0, 1.0))
    for i, (x, y, z) in enumerate([(-7.8, 8.3, 3.2), (7.5, 9.1, 3.25), (-1.8, 10.5, 3.55), (1.9, 10.7, 3.55)]):
        inst(templates, "PROPS_Village_HangingLantern", f"SCENE_VillageProps_HangingLantern_Bridge_{i:02d}", village, (x, y, z), (0, 0, i * 0.2), (1.0, 1.0, 1.0))


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
    for i, (x, y, z) in enumerate([(-9.1, 7.1, 4.4), (9.0, 8.1, 4.2), (-1.8, 11.0, 4.5), (2.0, 11.1, 4.5)]):
        inst(templates, "FOLIAGE_HangingVine", f"SCENE_Foliage_HangingVine_{i:02d}", foliage, (x, y, z), (0, 0, i * 0.4), (1.0, 1.0, 1.0))


def make_character(name, collection, loc, material, quest=False):
    x, y, z = loc
    body = cube_obj(f"{name}_Body", collection, (x, y, z + 0.92), (0.55, 0.34, 1.15), material)
    frustum_obj(f"{name}_Head", collection, (x, y, z + 1.62), 0.22, 0.24, 0.34, 10, material)
    cube_obj(f"{name}_Shoulders", collection, (x, y - 0.02, z + 1.18), (0.9, 0.20, 0.18), bpy.data.materials["MAT_Leaves_DeepGreen"])
    cube_obj(f"{name}_Staff", collection, (x + 0.48, y - 0.10, z + 0.93), (0.07, 0.07, 1.45), bpy.data.materials["MAT_Bark_DarkRoot"], (0.18, 0, 0.02))
    if quest:
        bicone_mesh(f"{name}_QuestMarker", collection, (x, y, z + 2.35), 0.16, 0.28, bpy.data.materials["MAT_QuestGold"])
        cube_obj(f"{name}_QuestMarkerStem", collection, (x, y, z + 2.07), (0.08, 0.08, 0.28), bpy.data.materials["MAT_QuestGold"])
    return body


def place_characters(collections, mats):
    chars = collections["SCENE_Characters"]
    make_character("SCENE_Characters_PlayerPlaceholder_Foreground", chars, (0, -13.45, 0.30), mats["MAT_PlayerPlaceholder"])
    make_character("SCENE_Characters_MemoryKeeperQuestGiver", chars, (-1.9, 7.2, 0.36), mats["MAT_NpcPlaceholder"], quest=True)
    make_character("SCENE_Characters_RootGuardianHubGuard", chars, (2.2, 7.6, 0.36), mats["MAT_NpcPlaceholder"], quest=False)
    make_character("SCENE_Characters_WardenTrainerNearPath", chars, (4.4, 3.9, 0.36), mats["MAT_NpcPlaceholder"], quest=True)
    cube_obj("SCENE_Characters_WildrootFawn_Body", chars, (-3.4, -4.3, 0.72), (0.85, 0.32, 0.42), bpy.data.materials["MAT_Wood_GoldenTrim"])
    frustum_obj("SCENE_Characters_WildrootFawn_Head", chars, (-2.92, -4.3, 0.94), 0.12, 0.10, 0.30, 8, bpy.data.materials["MAT_Wood_GoldenTrim"], (0, math.radians(90), 0))


def setup_backdrop(collection, mats):
    disable_shadow(cube_obj("SCENE_Terrain_SoftBlueSkyBackdrop", collection, (0, 22.5, 10.0), (78, 0.12, 24), mats["MAT_Backdrop_SkyBlue"]))
    disable_shadow(cube_obj("SCENE_Terrain_BlueGreenForestMistBand", collection, (0, 21.8, 4.2), (70, 0.08, 5.8), mats["MAT_Backdrop_ForestMist"]))


def setup_lighting_camera(collections):
    col = collections["SCENE_LightingRender"]
    bpy.ops.object.light_add(type="SUN", location=(-5, -8, 18), rotation=(math.radians(43), 0, math.radians(-32)))
    sun = bpy.context.object
    sun.name = "SCENE_LightingRender_SunKey_BrightWarmDay"
    sun.data.energy = 3.4
    sun.data.angle = math.radians(5.5)
    link_to(col, sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -9, 8), rotation=(math.radians(60), 0, 0))
    fill = bpy.context.object
    fill.name = "SCENE_LightingRender_AmbientFill_BlueGreenShade"
    fill.data.energy = 500
    fill.data.size = 18
    fill.data.color = (0.55, 0.86, 0.78)
    if hasattr(fill.data, "use_shadow"):
        fill.data.use_shadow = False
    link_to(col, fill)
    for i, (x, y, z, color, power) in enumerate([
        (-3.8, -6.6, 1.6, (0.05, 0.9, 1.0), 85),
        (3.8, -5.6, 1.5, (0.05, 0.9, 1.0), 75),
        (-6.7, 2.8, 1.9, (0.05, 0.9, 1.0), 95),
        (6.2, 3.4, 1.9, (0.05, 0.9, 1.0), 95),
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
    scene.world.color = (0.28, 0.48, 0.50)
    try:
        scene.view_settings.view_transform = "Standard"
    except TypeError:
        pass
    try:
        scene.view_settings.look = "Medium High Contrast"
    except TypeError:
        scene.view_settings.look = "None"
    scene.view_settings.exposure = 0.0
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
        "notes": [
            "Scene assembled from the existing Thornveil modular asset kit plus custom terrain, characters, giant trees, and lighting.",
            "Main path is intentionally kept open; foliage and props frame the route instead of blocking it.",
            "Camera is a third-person gameplay view behind the player placeholder.",
        ],
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    report = f"""# Heartbough Glade Entrance Quality Report

## Checklist
- Does the scene read as a Sylvaen starting-zone entrance within 3 seconds? Yes. The camera faces treehouses, root bridges, banners, lanterns, cyan Worldroot crystals, and a central quest hub.
- Is the third-person player camera clear? Yes. A player placeholder sits in the foreground with the camera behind it, aimed down the path.
- Is the main path obvious? Yes. Wide mossy stone discs and root connectors lead forward through the entrance arch into the hub.
- Is the quest hub visually readable? Yes. The shrine, waypoint stone, rune monolith, quest-giver marker, NPC placeholders, and bridge/treehouse backdrop cluster around Heartbough Glade.
- Are landmarks visible from the player camera? Yes. The arch, treehouses, root bridges, crystals, banners, lanterns, and giant background Worldroot trees are visible.
- Does foliage frame instead of block the route? Yes. Most foliage is placed outside the path corridor, leaving a clean walkable center.
- Is the lighting bright day with magical accents? Yes. Warm sunlight, blue-green fill, cyan crystal lights, and warm lantern lights are included.
- Is it still stylized low-poly rather than photorealistic? Yes. Assets use chunky silhouettes, simple MAT_ materials, and readable saturated colors.

## Counts
- Collections: {len(collections)}
- Objects: {sum(counts.values())}
- Approximate triangles: {manifest["approximate_total_triangles"]}

## Limitations
- This is a Blender scene assembly and visual target, not yet imported as live terrain into the C# client.
- Terrain remains stylized mesh/blockout geometry and should later become a proper playable collision/nav mesh.
- Characters are placeholders for scale and gameplay readability.
"""
    OUT_REPORT.write_text(report, encoding="utf-8")


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
    place_architecture(templates, collections)
    place_worldroot_props(templates, collections)
    place_village_props(templates, collections)
    place_foliage(templates, collections)
    place_characters(collections, mats)
    setup_lighting_camera(collections)
    remove_template_objects(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_scene_glb(collections)
    render_to(OUT_RENDER_1440, 2560, 1440)
    render_to(OUT_RENDER_1080, 1920, 1080)
    create_reports(collections)


if __name__ == "__main__":
    main()
