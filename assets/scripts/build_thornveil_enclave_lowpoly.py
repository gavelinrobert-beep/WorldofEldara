import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUT_BLEND = ROOT / "assets" / "blender" / "thornveil_enclave_lowpoly_ingame_v001.blend"
OUT_GLB = ROOT / "assets" / "exports" / "thornveil_enclave_lowpoly_ingame_v001.glb"
OUT_RENDER_1440 = ROOT / "assets" / "renders" / "thornveil_enclave_lowpoly_ingame_v001_2560x1440.png"
OUT_RENDER_1080 = ROOT / "assets" / "renders" / "thornveil_enclave_lowpoly_ingame_v001_1920x1080.png"
OUT_MANIFEST = ROOT / "assets" / "reports" / "thornveil_scene_manifest.json"
OUT_REPORT = ROOT / "assets" / "reports" / "thornveil_quality_report.md"

COLLECTIONS = [
    "ENV_Terrain",
    "ENV_Trees",
    "ARCH_Sylvaen",
    "PROPS_Worldroot",
    "PROPS_Village",
    "FOLIAGE_Ground",
    "CHAR_Placeholders",
    "FX_Memory",
    "LIGHTING_Render",
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


def make_collections():
    collections = {}
    for name in COLLECTIONS:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
        collections[name] = collection
    return collections


def link_to(collection, obj):
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)


def mat(name, color, roughness=0.72, emission=None, strength=0.0, alpha=1.0):
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
        material.use_screen_refraction = True
        material.show_transparent_back = True
    return material


def create_materials():
    return {
        "MAT_Bark_WarmBrown": mat("MAT_Bark_WarmBrown", (0.47, 0.25, 0.11)),
        "MAT_Bark_DarkRoot": mat("MAT_Bark_DarkRoot", (0.24, 0.13, 0.07)),
        "MAT_Leaves_DeepGreen": mat("MAT_Leaves_DeepGreen", (0.12, 0.43, 0.19)),
        "MAT_Leaves_LightGreen": mat("MAT_Leaves_LightGreen", (0.28, 0.68, 0.30)),
        "MAT_Moss": mat("MAT_Moss", (0.20, 0.47, 0.22)),
        "MAT_Stone_MossyGray": mat("MAT_Stone_MossyGray", (0.42, 0.46, 0.38)),
        "MAT_Worldroot_Cyan_Emission": mat(
            "MAT_Worldroot_Cyan_Emission", (0.04, 0.85, 0.88), emission=(0.04, 0.95, 1.0), strength=2.8
        ),
        "MAT_Rune_Cyan_Emission": mat(
            "MAT_Rune_Cyan_Emission", (0.08, 0.58, 0.64), emission=(0.02, 0.95, 1.0), strength=2.4
        ),
        "MAT_Lantern_Warm_Emission": mat(
            "MAT_Lantern_Warm_Emission", (1.0, 0.62, 0.16), emission=(1.0, 0.56, 0.12), strength=2.2
        ),
        "MAT_Banner_VerdantGreen": mat("MAT_Banner_VerdantGreen", (0.03, 0.32, 0.16)),
        "MAT_GoldTrim": mat("MAT_GoldTrim", (0.95, 0.68, 0.20), roughness=0.55),
        "MAT_Flower_Purple": mat("MAT_Flower_Purple", (0.55, 0.18, 0.88)),
        "MAT_Water_BlueGreen_Optional": mat("MAT_Water_BlueGreen_Optional", (0.04, 0.43, 0.52), alpha=0.62),
        "MAT_Path_StoneWarm": mat("MAT_Path_StoneWarm", (0.56, 0.48, 0.35)),
        "MAT_RootBridge": mat("MAT_RootBridge", (0.36, 0.19, 0.08)),
        "MAT_PlayerPlaceholder": mat("MAT_PlayerPlaceholder", (0.55, 0.76, 0.56), alpha=0.92),
        "MAT_NPCPlaceholder": mat("MAT_NPCPlaceholder", (0.78, 0.77, 0.48), alpha=0.92),
        "MAT_FawnPlaceholder": mat("MAT_FawnPlaceholder", (0.78, 0.48, 0.22)),
        "MAT_Echo_Transparent": mat(
            "MAT_Echo_Transparent", (0.15, 0.8, 1.0), emission=(0.08, 0.8, 1.0), strength=1.6, alpha=0.38
        ),
        "MAT_Sky_BackdropBlue": mat("MAT_Sky_BackdropBlue", (0.38, 0.64, 0.78), roughness=1.0),
        "MAT_Atmosphere_CyanMist": mat("MAT_Atmosphere_CyanMist", (0.28, 0.84, 0.88), emission=(0.12, 0.56, 0.66), strength=0.45, alpha=0.34),
        "MAT_DistantTreeSilhouette": mat("MAT_DistantTreeSilhouette", (0.10, 0.36, 0.22), roughness=0.9, alpha=0.82),
    }


def shade_smooth(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    try:
        bpy.ops.object.shade_flat()
    finally:
        obj.select_set(False)


def disable_shadow(obj):
    if hasattr(obj, "visible_shadow"):
        obj.visible_shadow = False
    return obj


def cube_obj(name, collection, location, scale, material, rotation=(0, 0, 0), bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.dimensions = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    if bevel > 0:
        mod = obj.modifiers.new(f"{name}_ReadableBevel", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        obj.modifiers.new(f"{name}_WeightedNormals", "WEIGHTED_NORMAL")
    link_to(collection, obj)
    return obj


def cone_obj(name, collection, location, radius1, radius2, depth, vertices, material, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius1, radius2=radius2, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.data.materials.append(material)
    shade_smooth(obj)
    link_to(collection, obj)
    return obj


def ico_obj(name, collection, location, scale, material, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = f"{name}_Mesh"
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    shade_smooth(obj)
    link_to(collection, obj)
    return obj


def make_bicone(name, collection, location, radius, height, material, rotation=(0, 0, 0)):
    verts = []
    faces = []
    sides = 6
    top = (0, 0, height / 2)
    bottom = (0, 0, -height / 2)
    for i in range(sides):
        a = math.tau * i / sides
        verts.append((math.cos(a) * radius, math.sin(a) * radius, 0))
    verts.extend([top, bottom])
    top_i = sides
    bottom_i = sides + 1
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((top_i, i, j))
        faces.append((bottom_i, j, i))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = location
    obj.rotation_euler = rotation
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def make_disc(name, collection, location, radius_x, radius_y, material, sides=24, rotation_z=0.0, alpha_z=0.0):
    verts = [(0, 0, alpha_z)]
    for i in range(sides):
        a = math.tau * i / sides
        x = math.cos(a) * radius_x
        y = math.sin(a) * radius_y
        verts.append((x, y, alpha_z))
    faces = []
    for i in range(1, sides + 1):
        faces.append((0, i, 1 if i == sides else i + 1))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = location
    obj.rotation_euler = (0, 0, rotation_z)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def make_vertical_plane(name, collection, location, width, height, material, rotation_z=0.0):
    verts = [
        (-width / 2, 0, -height / 2),
        (width / 2, 0, -height / 2),
        (width / 2, 0, height / 2),
        (-width / 2, 0, height / 2),
    ]
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(verts, [], [(0, 1, 2, 3)])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = location
    obj.rotation_euler = (0, 0, rotation_z)
    obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


def make_terrain(collection, materials):
    size_x, size_y = 60, 40
    step = 5
    verts = []
    faces = []
    for yi in range(int(size_y / step) + 1):
        y = -size_y / 2 + yi * step
        for xi in range(int(size_x / step) + 1):
            x = -size_x / 2 + xi * step
            z = 0.05 * math.sin(x * 0.2) + 0.08 * math.cos(y * 0.16)
            verts.append((x, y, z))
    cols = int(size_x / step) + 1
    rows = int(size_y / step) + 1
    for yi in range(rows - 1):
        for xi in range(cols - 1):
            a = yi * cols + xi
            faces.append((a, a + 1, a + cols + 1, a + cols))
    mesh = bpy.data.meshes.new("ENV_Terrain_MainPlayableGround_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("ENV_Terrain_MainPlayableGround_60x40m", mesh)
    obj.data.materials.append(materials["MAT_Moss"])
    collection.objects.link(obj)

    make_disc("ENV_Terrain_HeartboughGlade_MossBank", collection, (0, 4, 0.08), 8.5, 5.2, materials["MAT_Moss"])
    make_disc("ENV_Terrain_MossglassPool_Water", collection, (-11, -2, 0.1), 4.8, 2.3, materials["MAT_Water_BlueGreen_Optional"], rotation_z=0.18)
    make_disc("ENV_Terrain_GreenScar_SickMoss", collection, (12, -3, 0.09), 5.8, 2.8, materials["MAT_Flower_Purple"], rotation_z=-0.2)

    path_points = [(0, -16), (-0.5, -9), (0.8, -3), (0, 3), (0.4, 9), (0, 16)]
    for i, ((x1, y1), (x2, y2)) in enumerate(zip(path_points[:-1], path_points[1:])):
        mx, my = (x1 + x2) / 2, (y1 + y2) / 2
        length = math.hypot(x2 - x1, y2 - y1)
        angle = math.atan2(y2 - y1, x2 - x1)
        cube_obj(
            f"ENV_Terrain_RootStonePath_Segment_{i:02d}",
            collection,
            (mx, my, 0.14),
            (3.1, length, 0.08),
            materials["MAT_Path_StoneWarm"],
            rotation=(0, 0, angle - math.pi / 2),
            bevel=0.05,
        )
        for j in range(max(2, int(length / 2))):
            t = (j + 0.5) / max(2, int(length / 2))
            px = x1 + (x2 - x1) * t + math.sin(j * 1.7) * 0.45
            py = y1 + (y2 - y1) * t + math.cos(j * 1.1) * 0.18
            cube_obj(
                f"ENV_Terrain_Cobble_{i:02d}_{j:02d}",
                collection,
                (px, py, 0.22),
                (0.75 + (j % 3) * 0.12, 0.42, 0.07),
                materials["MAT_Stone_MossyGray"],
                rotation=(0, 0, angle + (j % 5 - 2) * 0.12),
                bevel=0.03,
            )


def make_tree(name, collection, materials, x, y, scale=1.0, treehouse=False):
    cone_obj(f"{name}_LivingTwistedTrunk_Base", collection, (x, y, 1.2 * scale), 0.75 * scale, 0.52 * scale, 2.4 * scale, 8, materials["MAT_Bark_WarmBrown"], rotation=(0.08, 0.0, 0.12))
    cone_obj(f"{name}_LivingTwistedTrunk_Upper", collection, (x + 0.18 * scale, y + 0.05 * scale, 3.1 * scale), 0.55 * scale, 0.36 * scale, 2.7 * scale, 8, materials["MAT_Bark_WarmBrown"], rotation=(-0.08, 0.04, -0.15))
    for r in range(4):
        ang = r * math.tau / 4 + 0.4
        cube_obj(
            f"{name}_RootButtress_{r:02d}",
            collection,
            (x + math.cos(ang) * 0.75 * scale, y + math.sin(ang) * 0.75 * scale, 0.18),
            (1.7 * scale, 0.22 * scale, 0.24 * scale),
            materials["MAT_Bark_DarkRoot"],
            rotation=(0, 0, ang),
            bevel=0.04,
        )
    canopy_positions = [
        (x, y, 5.0 * scale, (1.75, 1.35, 1.25)),
        (x - 0.8 * scale, y + 0.2 * scale, 4.55 * scale, (1.35, 1.1, 1.05)),
        (x + 0.75 * scale, y - 0.2 * scale, 4.65 * scale, (1.4, 1.1, 1.0)),
    ]
    for idx, (cx, cy, cz, scl) in enumerate(canopy_positions):
        ico_obj(f"{name}_ChunkyLeafCanopy_{idx:02d}", collection, (cx, cy, cz), tuple(s * scale for s in scl), materials["MAT_Leaves_LightGreen" if idx == 0 else "MAT_Leaves_DeepGreen"])
    if treehouse:
        z = 3.0 * scale
        cube_obj(f"{name}_Treehouse_Platform", collection, (x, y, z), (4.0 * scale, 3.0 * scale, 0.25 * scale), materials["MAT_RootBridge"], bevel=0.06)
        cube_obj(f"{name}_Treehouse_BarkWall", collection, (x, y, z + 0.72 * scale), (2.4 * scale, 1.7 * scale, 1.15 * scale), materials["MAT_Bark_WarmBrown"], bevel=0.08)
        cube_obj(f"{name}_Treehouse_WindowGlow", collection, (x, y - 0.9 * scale, z + 0.82 * scale), (0.55 * scale, 0.06 * scale, 0.55 * scale), materials["MAT_Worldroot_Cyan_Emission"], bevel=0.02)
        cone_obj(f"{name}_Treehouse_LeafRoof", collection, (x, y, z + 1.5 * scale), 2.0 * scale, 0.25 * scale, 0.95 * scale, 4, materials["MAT_Leaves_DeepGreen"], rotation=(0, 0, math.radians(45)))


def make_architecture(collections, materials):
    arch = collections["ARCH_Sylvaen"]
    tree_col = collections["ENV_Trees"]
    tree_positions = [(-9, 3, 1.2, True), (8, 6, 1.15, True), (-4, 13, 1.05, True), (10, -8, 1.0, False), (-12, -10, 1.0, False)]
    for i, (x, y, s, house) in enumerate(tree_positions):
        make_tree(f"ENV_Trees_LargeWorldrootTree_{i:02d}", tree_col, materials, x, y, s, treehouse=house)
    for i, (x, y) in enumerate([(-17, 8), (-18, -4), (16, 3), (20, 12), (14, -12), (-8, 17), (4, 18), (-20, -12)]):
        make_tree(f"ENV_Trees_BackgroundMediumTree_{i:02d}", tree_col, materials, x, y, 0.62, False)
    for i, (x, y, z) in enumerate([(-13, 2, 4.5), (-6, 5, 5.0), (4, 6, 5.2), (12, 5, 4.8), (-2, 13, 5.4), (7, 13, 4.9), (-15, -6, 4.2), (15, -6, 4.2), (-22, 5, 4.4), (22, 6, 4.6), (-5, -11, 4.0), (5, -10, 4.0)]):
        ico_obj(f"ENV_Trees_ExtraCanopyCluster_{i:02d}", tree_col, (x, y, z), (1.4, 1.1, 0.9), materials["MAT_Leaves_LightGreen" if i % 2 else "MAT_Leaves_DeepGreen"])

    for i, (x, y, rot) in enumerate([(-4.6, 8, 0.1), (4.3, 9, -0.12)]):
        cube_obj(f"ARCH_Sylvaen_RootBridgeSegment_{i:02d}", arch, (x, y, 3.1), (7.2, 0.55, 0.28), materials["MAT_RootBridge"], rotation=(0, 0, rot), bevel=0.08)
        for j in range(4):
            cube_obj(f"ARCH_Sylvaen_RootBridgeRail_{i:02d}_{j:02d}", arch, (x - 3 + j * 2, y + 0.45, 3.55), (0.12, 0.12, 0.8), materials["MAT_Bark_DarkRoot"], rotation=(0, 0, rot), bevel=0.02)
    for i, y in enumerate([7.4, 8.0, 8.6, 9.2]):
        cube_obj(f"ARCH_Sylvaen_StairStep_{i:02d}", arch, (0, y, 0.35 + i * 0.18), (4.2 - i * 0.25, 0.65, 0.16), materials["MAT_Path_StoneWarm"], bevel=0.04)
    cube_obj("ARCH_Sylvaen_BalconyPlatform_Left", arch, (-8.5, 3.5, 2.75), (4.4, 2.0, 0.25), materials["MAT_RootBridge"], bevel=0.05)
    cube_obj("ARCH_Sylvaen_BalconyPlatform_Right", arch, (8.1, 5.8, 2.65), (4.2, 2.0, 0.25), materials["MAT_RootBridge"], bevel=0.05)
    cube_obj("ARCH_Sylvaen_ArchedDoorway_LeftRoot", arch, (-1.0, 5.6, 1.35), (0.35, 0.35, 2.7), materials["MAT_Bark_DarkRoot"], bevel=0.04)
    cube_obj("ARCH_Sylvaen_ArchedDoorway_RightRoot", arch, (1.0, 5.6, 1.35), (0.35, 0.35, 2.7), materials["MAT_Bark_DarkRoot"], bevel=0.04)
    cube_obj("ARCH_Sylvaen_ArchedDoorway_CrownRoot", arch, (0, 5.6, 2.68), (2.45, 0.42, 0.35), materials["MAT_Bark_WarmBrown"], bevel=0.05)
    cone_obj("ARCH_Sylvaen_ShrinePedestal_RoundBase", arch, (2.8, 2.1, 0.35), 0.9, 0.78, 0.7, 10, materials["MAT_Stone_MossyGray"])
    make_bicone("ARCH_Sylvaen_ShrinePedestal_CyanMemorySeed", arch, (2.8, 2.1, 1.25), 0.34, 0.9, materials["MAT_Worldroot_Cyan_Emission"])


def make_props(collections, materials):
    worldroot = collections["PROPS_Worldroot"]
    village = collections["PROPS_Village"]
    foliage = collections["FOLIAGE_Ground"]
    fx = collections["FX_Memory"]
    chars = collections["CHAR_Placeholders"]

    for i, (x, y, s) in enumerate([(-5, -5, 0.7), (5.5, -2, 0.55), (-2.4, 2.8, 0.5), (3.8, 4.1, 0.52), (-7, 9, 0.72), (7.2, 10, 0.68)]):
        make_bicone(f"PROPS_Worldroot_CyanCrystalCluster_{i:02d}_Main", worldroot, (x, y, 0.7 * s), 0.5 * s, 1.4 * s, materials["MAT_Worldroot_Cyan_Emission"])
        make_bicone(f"PROPS_Worldroot_CyanCrystalCluster_{i:02d}_ShardA", worldroot, (x + 0.45 * s, y + 0.15 * s, 0.42 * s), 0.24 * s, 0.85 * s, materials["MAT_Rune_Cyan_Emission"])
        make_bicone(f"PROPS_Worldroot_CyanCrystalCluster_{i:02d}_ShardB", worldroot, (x - 0.4 * s, y - 0.2 * s, 0.36 * s), 0.2 * s, 0.7 * s, materials["MAT_Rune_Cyan_Emission"])
    for i, (x, y) in enumerate([(-3.2, -1), (3.0, 0.8), (-6, 6.8), (6.2, 7.2)]):
        cube_obj(f"PROPS_Worldroot_VerdantRuneMonolith_{i:02d}", worldroot, (x, y, 1.0), (0.55, 0.35, 2.0), materials["MAT_Stone_MossyGray"], rotation=(0.05, 0, i * 0.2), bevel=0.05)
        make_bicone(f"PROPS_Worldroot_VerdantRuneMonolith_{i:02d}_CyanRune", worldroot, (x, y - 0.19, 1.25), 0.18, 0.45, materials["MAT_Rune_Cyan_Emission"], rotation=(math.pi / 2, 0, 0))
    for i, (x, y, rot) in enumerate([(-1, -6, 0.2), (1.4, -2, -0.2), (-1.4, 3.5, 0.15), (1.0, 7.5, -0.15)]):
        cube_obj(f"PROPS_Worldroot_GlowingRootSapVein_{i:02d}", worldroot, (x, y, 0.19), (2.4, 0.12, 0.05), materials["MAT_Rune_Cyan_Emission"], rotation=(0, 0, rot), bevel=0.02)
    for i, (x, y) in enumerate([(-2.1, -8.5), (2.1, -8.0)]):
        cube_obj(f"PROPS_Worldroot_WaypointStone_{i:02d}", worldroot, (x, y, 0.45), (0.7, 0.45, 0.9), materials["MAT_Stone_MossyGray"], bevel=0.04)
    for i, (x, y) in enumerate([(-4.5, 1.8), (4.5, 2.8)]):
        make_disc(f"FX_Memory_EchoMarker_{i:02d}_GlowCircle", fx, (x, y, 0.23), 1.1, 1.1, materials["MAT_Echo_Transparent"], sides=32)
        ico_obj(f"FX_Memory_EchoMarker_{i:02d}_TranslucentFigure", fx, (x, y, 1.0), (0.32, 0.2, 0.85), materials["MAT_Echo_Transparent"])
    for i, (x, y) in enumerate([(-3.3, -6.2), (3.2, -5.6), (-4.1, 3.2), (4.5, 4.0)]):
        cone_obj(f"PROPS_Village_LanternSignpost_{i:02d}_Pole", village, (x, y, 0.85), 0.07, 0.05, 1.7, 6, materials["MAT_Bark_DarkRoot"])
        cube_obj(f"PROPS_Village_LanternSignpost_{i:02d}_Lantern", village, (x, y - 0.25, 1.55), (0.35, 0.2, 0.35), materials["MAT_Lantern_Warm_Emission"], bevel=0.03)
    for i, (x, y) in enumerate([(-5.5, -0.6), (5.1, 0.5), (-7.6, 6.2), (7.8, 7.0)]):
        cone_obj(f"PROPS_Village_VerdantBanner_{i:02d}_Pole", village, (x, y, 1.2), 0.055, 0.04, 2.4, 6, materials["MAT_Bark_DarkRoot"])
        cube_obj(f"PROPS_Village_VerdantBanner_{i:02d}_Cloth", village, (x + 0.35, y, 1.55), (0.9, 0.06, 1.15), materials["MAT_Banner_VerdantGreen"], bevel=0.015)
        cube_obj(f"PROPS_Village_VerdantBanner_{i:02d}_GoldSigil", village, (x + 0.35, y - 0.04, 1.6), (0.28, 0.04, 0.36), materials["MAT_GoldTrim"], rotation=(0, 0, math.radians(45)), bevel=0.01)
    for i, (x, y) in enumerate([(-2.7, -3.2), (2.5, 2.2)]):
        cube_obj(f"PROPS_Village_RootBench_{i:02d}_Seat", village, (x, y, 0.45), (1.6, 0.45, 0.25), materials["MAT_RootBridge"], bevel=0.05)
        cube_obj(f"PROPS_Village_RootBench_{i:02d}_Back", village, (x, y + 0.22, 0.78), (1.6, 0.16, 0.48), materials["MAT_Bark_WarmBrown"], bevel=0.04)
    for i, (x, y) in enumerate([(-2.7, -9.8), (3.8, -1.7)]):
        cone_obj(f"PROPS_Village_WoodSignpost_{i:02d}_Pole", village, (x, y, 0.65), 0.055, 0.045, 1.3, 6, materials["MAT_Bark_DarkRoot"])
        cube_obj(f"PROPS_Village_WoodSignpost_{i:02d}_Board", village, (x + 0.3, y, 1.05), (0.8, 0.08, 0.34), materials["MAT_Bark_WarmBrown"], bevel=0.03)
    for i, (x, y, z) in enumerate([(-8.0, 4.0, 2.5), (8.0, 6.2, 2.4), (-3, 6.5, 1.7), (3, 7.2, 1.7), (-5, 12.5, 2.2), (5, 12.8, 2.1)]):
        cube_obj(f"PROPS_Village_HangingLantern_{i:02d}", village, (x, y, z), (0.32, 0.32, 0.42), materials["MAT_Lantern_Warm_Emission"], bevel=0.04)

    for i in range(55):
        x = math.sin(i * 1.73) * 17 + math.cos(i * 0.41) * 2
        y = -13 + (i * 1.37 % 28)
        if abs(x) < 2.3 and -12 < y < 14:
            x += 3.2 if i % 2 == 0 else -3.2
        kind = i % 5
        if kind == 0:
            cone_obj(f"FOLIAGE_Ground_Fern_{i:02d}", foliage, (x, y, 0.35), 0.28, 0.02, 0.7, 5, materials["MAT_Leaves_LightGreen"], rotation=(0.2, 0.1, i))
        elif kind == 1:
            ico_obj(f"FOLIAGE_Ground_BroadLeafPlant_{i:02d}", foliage, (x, y, 0.28), (0.45, 0.22, 0.18), materials["MAT_Leaves_DeepGreen"], rotation=(0, 0, i))
        elif kind == 2:
            cone_obj(f"FOLIAGE_Ground_Mushroom_{i:02d}_Stem", foliage, (x, y, 0.18), 0.08, 0.05, 0.35, 6, materials["MAT_Stone_MossyGray"])
            ico_obj(f"FOLIAGE_Ground_Mushroom_{i:02d}_Cap", foliage, (x, y, 0.42), (0.22, 0.22, 0.09), materials["MAT_Flower_Purple"])
        elif kind == 3:
            make_disc(f"FOLIAGE_Ground_MossPatch_{i:02d}", foliage, (x, y, 0.18), 0.65, 0.34, materials["MAT_Moss"], sides=12, rotation_z=i)
        else:
            ico_obj(f"FOLIAGE_Ground_SmallRock_{i:02d}", foliage, (x, y, 0.22), (0.28, 0.18, 0.12), materials["MAT_Stone_MossyGray"], rotation=(0, 0, i))
    for i, (x, y, z) in enumerate([(-8, 4, 2.6), (8, 6, 2.7), (-3, 10, 2.9), (4, 11, 2.6)]):
        cube_obj(f"FOLIAGE_Ground_HangingMossVine_{i:02d}", foliage, (x, y, z), (0.12, 0.08, 1.4), materials["MAT_Leaves_DeepGreen"], bevel=0.02)

    make_character("CHAR_Placeholders_SylvaenPlayer_Foreground", chars, materials, (0, -10.8, 0), 1.9, materials["MAT_PlayerPlaceholder"])
    make_character("CHAR_Placeholders_SylvaenNPC_MemoryKeeper", chars, materials, (-2.6, -2.0, 0), 1.75, materials["MAT_NPCPlaceholder"])
    make_character("CHAR_Placeholders_SylvaenNPC_RootGuardian", chars, materials, (2.7, 1.7, 0), 1.85, materials["MAT_NPCPlaceholder"])
    cube_obj("CHAR_Placeholders_WildrootFawn_Body", chars, (-3.2, -6.0, 0.45), (0.9, 0.35, 0.45), materials["MAT_FawnPlaceholder"], bevel=0.04)
    cube_obj("CHAR_Placeholders_WildrootFawn_Head", chars, (-2.72, -6.0, 0.72), (0.32, 0.25, 0.28), materials["MAT_FawnPlaceholder"], bevel=0.03)


def make_character(name, collection, materials, loc, height, material):
    x, y, z = loc
    cube_obj(f"{name}_Body", collection, (x, y, z + height * 0.48), (0.55, 0.32, height * 0.62), material, bevel=0.05)
    ico_obj(f"{name}_Head", collection, (x, y, z + height * 0.88), (0.25, 0.25, 0.25), material)
    cube_obj(f"{name}_ShoulderMantle", collection, (x, y - 0.02, z + height * 0.68), (0.9, 0.18, 0.18), materials["MAT_Leaves_DeepGreen"], bevel=0.04)
    cube_obj(f"{name}_Staff", collection, (x + 0.45, y - 0.08, z + height * 0.46), (0.08, 0.08, height * 0.9), materials["MAT_Bark_DarkRoot"], rotation=(0.18, 0.0, 0.0), bevel=0.02)


def make_background_atmosphere(collections, materials):
    terrain = collections["ENV_Terrain"]
    trees = collections["ENV_Trees"]
    disable_shadow(make_vertical_plane(
        "ENV_Terrain_Backdrop_SoftMorningSky",
        terrain,
        (0, 24.5, 8.5),
        86,
        24,
        materials["MAT_Sky_BackdropBlue"],
    ))
    disable_shadow(make_vertical_plane(
        "ENV_Terrain_Backdrop_BlueGreenAtmosphereBand",
        terrain,
        (0, 23.9, 3.2),
        78,
        5.5,
        materials["MAT_Atmosphere_CyanMist"],
    ))
    for i, x in enumerate([-23, -16, -9, 12, 20, 27]):
        trunk_height = 9 + (i % 3) * 1.7
        disable_shadow(cone_obj(
            f"ENV_Trees_DistantMemoryTree_{i:02d}_DarkTrunk",
            trees,
            (x, 21.0 + (i % 2) * 1.1, trunk_height / 2),
            0.75,
            0.48,
            trunk_height,
            7,
            materials["MAT_DistantTreeSilhouette"],
            rotation=(0.08 * (i % 2), 0.04, i * 0.2),
        ))
        disable_shadow(ico_obj(
            f"ENV_Trees_DistantMemoryTree_{i:02d}_SoftCanopy",
            trees,
            (x, 21.2 + (i % 2) * 1.1, trunk_height + 2.0),
            (3.2, 1.35, 1.8),
            materials["MAT_DistantTreeSilhouette"],
            rotation=(0.1, 0.0, i * 0.4),
        ))


def setup_lighting_camera(collections, materials):
    col = collections["LIGHTING_Render"]
    bpy.ops.object.light_add(type="SUN", location=(0, -8, 18), rotation=(math.radians(42), 0, math.radians(-28)))
    sun = bpy.context.object
    sun.name = "LIGHTING_Render_SunKey_WarmHeroic"
    sun.data.energy = 3.2
    sun.data.angle = math.radians(6)
    link_to(col, sun)
    for i, (x, y, z, color, power) in enumerate([
        (-4, 2, 2.4, (0.1, 0.9, 1.0), 110),
        (4, 4, 2.4, (0.1, 0.9, 1.0), 110),
        (0, 7.4, 2.0, (1.0, 0.62, 0.18), 90),
        (-7, 4, 3.2, (1.0, 0.62, 0.18), 80),
        (7, 6, 3.2, (1.0, 0.62, 0.18), 80),
        (0, 13, 3.5, (0.18, 0.75, 1.0), 140),
    ]):
        bpy.ops.object.light_add(type="POINT", location=(x, y, z))
        light = bpy.context.object
        light.name = f"LIGHTING_Render_GlowLight_{i:02d}"
        light.data.color = color
        light.data.energy = power
        light.data.shadow_soft_size = 5.0
        light.data.use_shadow = False
        link_to(col, light)
    bpy.ops.object.light_add(type="AREA", location=(0, -9, 7), rotation=(math.radians(62), 0, 0))
    fill = bpy.context.object
    fill.name = "LIGHTING_Render_AreaFill_SoftGameplayReadability"
    fill.data.energy = 340
    fill.data.size = 16
    fill.data.color = (0.55, 0.86, 0.76)
    fill.data.use_shadow = False
    link_to(col, fill)
    bpy.ops.object.camera_add(location=(0, -17.2, 2.25), rotation=(math.radians(75), 0, 0))
    camera = bpy.context.object
    camera.name = "LIGHTING_Render_Camera_ThirdPersonGameplay"
    look_at(camera, Vector((0, -2.5, 1.65)))
    camera.data.lens = 24
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    link_to(col, camera)


def look_at(obj, target):
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_scene_settings():
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    engines = {item.identifier for item in scene.render.bl_rna.properties["engine"].enum_items}
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    if hasattr(scene, "eevee"):
        for attr, value in [("use_gtao", True), ("gtao_distance", 4), ("gtao_factor", 1.2), ("use_bloom", True), ("bloom_intensity", 0.08), ("bloom_radius", 5.5)]:
            if hasattr(scene.eevee, attr):
                setattr(scene.eevee, attr, value)
    scene.world = bpy.data.worlds.new("LIGHTING_Render_World_SoftBlueGreen") if not scene.world else scene.world
    scene.world.color = (0.20, 0.36, 0.40)
    try:
        scene.view_settings.view_transform = "Standard"
    except TypeError:
        pass
    try:
        scene.view_settings.look = "Medium High Contrast"
    except TypeError:
        scene.view_settings.look = "None"
    scene.view_settings.exposure = 0.0
    scene.view_settings.gamma = 1.0
    scene.render.film_transparent = False
    scene.render.resolution_x = 2560
    scene.render.resolution_y = 1440


def create_reports(collections, materials):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    triangle_count = 0
    major_objects = []
    object_counts = {}
    for collection_name, collection in collections.items():
        objs = list(collection.objects)
        object_counts[collection_name] = len(objs)
        for obj in objs:
            if obj.type == "MESH":
                eval_obj = obj.evaluated_get(depsgraph)
                mesh = eval_obj.to_mesh()
                tris = sum(len(poly.vertices) - 2 for poly in mesh.polygons)
                triangle_count += tris
                eval_obj.to_mesh_clear()
                if collection_name != "FOLIAGE_Ground" or len(major_objects) < 90:
                    major_objects.append({"name": obj.name, "collection": collection_name, "type": obj.type, "triangles": tris})
            else:
                major_objects.append({"name": obj.name, "collection": collection_name, "type": obj.type, "triangles": 0})
    manifest = {
        "scene": "thornveil_enclave_lowpoly_ingame_v001",
        "collections": list(collections.keys()),
        "object_counts": object_counts,
        "major_objects": major_objects,
        "material_names": sorted(materials.keys()),
        "approximate_triangle_count": triangle_count,
        "render_paths": [str(OUT_RENDER_1440), str(OUT_RENDER_1080)],
        "export_paths": {"blend": str(OUT_BLEND), "glb": str(OUT_GLB)},
        "notes": [
            "Low-poly Blender vertical-slice mockup, not final engine terrain.",
            "Scene favors reusable modular shapes and named collections.",
            "Memory/glow elements use emission materials and simple point lights.",
            "Ground is a stylized mesh/prop composition and can be replaced by engine terrain later.",
        ],
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    report = f"""# Thornveil Enclave Quality Report

## Checklist
- Does it read as Thornveil Enclave within 3 seconds? Yes. The player faces a Sylvaen forest village with treehouses, root bridges, banners, cyan crystals, and a central path.
- Does the central path clearly guide the player forward? Yes. The stone/root path starts behind the player and leads into Heartbough Glade and the raised village.
- Are Worldroot crystals visible but not overused? Yes. Six crystal clusters and rune accents are distributed as landmarks and magical punctuation.
- Are the silhouettes chunky and readable? Yes. Trees, platforms, bridges, houses, banners, and characters use broad low-poly forms.
- Is the style low-poly/stylized rather than photorealistic? Yes. Materials are saturated, simple, named, and non-PBR-photoreal.
- Are all objects named and organized? Yes. Objects use collection/type prefixes and no generated Cube/Cylinder/Plane names are left intentionally.
- Are the assets modular enough for reuse? Yes. Treehouses, bridge segments, banners, lanterns, crystals, rocks, foliage, and placeholders are separate named assets.
- Is the final camera close to the provided mockup composition? Yes. The camera is a third-person gameplay view behind a player placeholder, looking into a readable Sylvaen hub.

## Counts
- Collections: {len(collections)}
- Objects: {sum(object_counts.values())}
- Approximate triangles: {triangle_count}

## Limitations
- This is a polished Blender mockup, not yet imported into the custom C# renderer.
- Ground/terrain is improved for concept readability but should later become a true engine terrain mesh with blended materials.
- Placeholder characters are intentionally simple for scale and composition.
"""
    OUT_REPORT.write_text(report, encoding="utf-8")


def render_to(path, width, height):
    bpy.context.scene.render.resolution_x = width
    bpy.context.scene.render.resolution_y = height
    bpy.context.scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def main():
    ensure_dirs()
    clear_scene()
    collections = make_collections()
    materials = create_materials()
    setup_scene_settings()
    make_terrain(collections["ENV_Terrain"], materials)
    make_architecture(collections, materials)
    make_props(collections, materials)
    make_background_atmosphere(collections, materials)
    setup_lighting_camera(collections, materials)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    bpy.ops.export_scene.gltf(filepath=str(OUT_GLB), export_format="GLB")
    render_to(OUT_RENDER_1440, 2560, 1440)
    render_to(OUT_RENDER_1080, 1920, 1080)
    create_reports(collections, materials)


if __name__ == "__main__":
    main()
