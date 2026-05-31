import json
import math
from pathlib import Path

import bpy
from mathutils import Euler, Vector


ROOT = Path(__file__).resolve().parents[2]
OUT_BLEND = ROOT / "assets" / "blender" / "thornveil_modular_asset_kit_v001.blend"
OUT_GLB = ROOT / "assets" / "exports" / "thornveil_modular_asset_kit_v001.glb"
OUT_RENDER = ROOT / "assets" / "renders" / "thornveil_modular_asset_kit_v001_lineup_1920x1080.png"
OUT_MANIFEST = ROOT / "assets" / "reports" / "thornveil_asset_kit_manifest.json"

COLLECTIONS = [
    "KIT_ARCHITECTURE",
    "KIT_WORLDROOT_PROPS",
    "KIT_VILLAGE_PROPS",
    "KIT_FOLIAGE",
    "KIT_LINEUP_STAGE",
    "KIT_LIGHTING_RENDER",
]


def ensure_dirs():
    for path in [OUT_BLEND.parent, OUT_GLB.parent, OUT_RENDER.parent, OUT_MANIFEST.parent]:
        path.mkdir(parents=True, exist_ok=True)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    for material in list(bpy.data.materials):
        bpy.data.materials.remove(material)


def create_collections():
    collections = {}
    for name in COLLECTIONS:
        collection = bpy.data.collections.new(name)
        bpy.context.scene.collection.children.link(collection)
        collections[name] = collection
    return collections


def make_mat(name, color, roughness=0.82, emission=None, strength=0.0, alpha=1.0):
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


def create_materials():
    return {
        "MAT_Bark_WarmBrown": make_mat("MAT_Bark_WarmBrown", (0.48, 0.25, 0.11)),
        "MAT_Bark_DarkRoot": make_mat("MAT_Bark_DarkRoot", (0.23, 0.12, 0.06)),
        "MAT_Wood_GoldenTrim": make_mat("MAT_Wood_GoldenTrim", (0.78, 0.47, 0.18)),
        "MAT_Leaves_DeepGreen": make_mat("MAT_Leaves_DeepGreen", (0.10, 0.38, 0.16)),
        "MAT_Leaves_LightGreen": make_mat("MAT_Leaves_LightGreen", (0.35, 0.78, 0.36)),
        "MAT_Moss": make_mat("MAT_Moss", (0.20, 0.50, 0.22)),
        "MAT_Stone_MossyGray": make_mat("MAT_Stone_MossyGray", (0.43, 0.47, 0.39)),
        "MAT_Worldroot_Cyan_Emission": make_mat(
            "MAT_Worldroot_Cyan_Emission", (0.05, 0.92, 0.96), emission=(0.04, 0.95, 1.0), strength=2.8
        ),
        "MAT_Rune_Cyan_Emission": make_mat(
            "MAT_Rune_Cyan_Emission", (0.05, 0.62, 0.66), emission=(0.04, 0.9, 1.0), strength=2.0
        ),
        "MAT_Lantern_Warm_Emission": make_mat(
            "MAT_Lantern_Warm_Emission", (1.0, 0.68, 0.20), emission=(1.0, 0.55, 0.10), strength=2.3
        ),
        "MAT_Banner_VerdantGreen": make_mat("MAT_Banner_VerdantGreen", (0.04, 0.32, 0.15)),
        "MAT_GoldTrim": make_mat("MAT_GoldTrim", (0.95, 0.68, 0.20), roughness=0.55),
        "MAT_Flower_Purple": make_mat("MAT_Flower_Purple", (0.58, 0.18, 0.86)),
        "MAT_Flower_Blue": make_mat("MAT_Flower_Blue", (0.22, 0.56, 1.0)),
        "MAT_Mushroom_Pink": make_mat("MAT_Mushroom_Pink", (0.95, 0.42, 0.56)),
        "MAT_Echo_Transparent": make_mat(
            "MAT_Echo_Transparent", (0.16, 0.82, 1.0), emission=(0.08, 0.8, 1.0), strength=1.3, alpha=0.42
        ),
        "MAT_Lineup_Base": make_mat("MAT_Lineup_Base", (0.15, 0.26, 0.20)),
        "MAT_Label_Dark": make_mat("MAT_Label_Dark", (0.02, 0.06, 0.05)),
    }


class MeshBuilder:
    def __init__(self):
        self.verts = []
        self.faces = []
        self.face_mats = []

    def _transform(self, point, center, rotation):
        matrix = Euler(rotation, "XYZ").to_matrix()
        return tuple(Vector(center) + matrix @ Vector(point))

    def add_face(self, indices, material_name):
        self.faces.append(indices)
        self.face_mats.append(material_name)

    def add_box(self, center, size, material_name, rotation=(0, 0, 0)):
        sx, sy, sz = size
        raw = [
            (-sx / 2, -sy / 2, -sz / 2),
            (sx / 2, -sy / 2, -sz / 2),
            (sx / 2, sy / 2, -sz / 2),
            (-sx / 2, sy / 2, -sz / 2),
            (-sx / 2, -sy / 2, sz / 2),
            (sx / 2, -sy / 2, sz / 2),
            (sx / 2, sy / 2, sz / 2),
            (-sx / 2, sy / 2, sz / 2),
        ]
        start = len(self.verts)
        self.verts.extend([self._transform(point, center, rotation) for point in raw])
        for face in [(0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0)]:
            self.add_face(tuple(start + i for i in face), material_name)

    def add_frustum(self, center, radius_bottom, radius_top, height, sides, material_name, rotation=(0, 0, 0)):
        start = len(self.verts)
        for z, radius in [(-height / 2, radius_bottom), (height / 2, radius_top)]:
            for i in range(sides):
                angle = math.tau * i / sides
                point = (math.cos(angle) * radius, math.sin(angle) * radius, z)
                self.verts.append(self._transform(point, center, rotation))
        bottom_center = len(self.verts)
        self.verts.append(self._transform((0, 0, -height / 2), center, rotation))
        top_center = len(self.verts)
        self.verts.append(self._transform((0, 0, height / 2), center, rotation))
        for i in range(sides):
            j = (i + 1) % sides
            self.add_face((start + i, start + j, start + sides + j, start + sides + i), material_name)
            self.add_face((bottom_center, start + j, start + i), material_name)
            self.add_face((top_center, start + sides + i, start + sides + j), material_name)

    def add_bicone(self, center, radius, height, material_name, sides=6, rotation=(0, 0, 0)):
        start = len(self.verts)
        for i in range(sides):
            angle = math.tau * i / sides
            self.verts.append(self._transform((math.cos(angle) * radius, math.sin(angle) * radius, 0), center, rotation))
        top = len(self.verts)
        self.verts.append(self._transform((0, 0, height / 2), center, rotation))
        bottom = len(self.verts)
        self.verts.append(self._transform((0, 0, -height / 2), center, rotation))
        for i in range(sides):
            j = (i + 1) % sides
            self.add_face((top, start + i, start + j), material_name)
            self.add_face((bottom, start + j, start + i), material_name)

    def add_disc(self, center, radius_x, radius_y, material_name, sides=18, rotation=(0, 0, 0)):
        start = len(self.verts)
        self.verts.append(self._transform((0, 0, 0), center, rotation))
        for i in range(sides):
            angle = math.tau * i / sides
            self.verts.append(self._transform((math.cos(angle) * radius_x, math.sin(angle) * radius_y, 0), center, rotation))
        for i in range(1, sides + 1):
            self.add_face((start, start + i, start + (1 if i == sides else i + 1)), material_name)


def make_asset(asset_name, collection, position, materials, build_func):
    builder = MeshBuilder()
    build_func(builder)
    mesh = bpy.data.meshes.new(f"{asset_name}_Mesh")
    mesh.from_pydata(builder.verts, [], builder.faces)
    mesh.update()
    obj = bpy.data.objects.new(asset_name, mesh)
    obj.location = position
    used = []
    for mat_name in builder.face_mats:
        if mat_name not in used:
            used.append(mat_name)
    for mat_name in used:
        obj.data.materials.append(materials[mat_name])
    index = {mat_name: i for i, mat_name in enumerate(used)}
    for poly, mat_name in zip(obj.data.polygons, builder.face_mats):
        poly.material_index = index[mat_name]
    collection.objects.link(obj)
    obj["asset_origin_note"] = "Origin is the module base/snap point at ground level."
    return obj


def build_treehouse(b):
    for angle in [0, math.pi / 2, math.pi, math.pi * 1.5]:
        b.add_box((math.cos(angle) * 0.55, math.sin(angle) * 0.55, 0.12), (1.25, 0.18, 0.18), "MAT_Bark_DarkRoot", (0.08, 0.0, angle))
    b.add_frustum((0, 0, 1.35), 0.55, 0.36, 2.7, 7, "MAT_Bark_WarmBrown", (0.04, -0.03, 0.1))
    b.add_box((0, 0, 2.58), (2.8, 2.2, 0.22), "MAT_Bark_DarkRoot")
    b.add_box((0, 0, 3.25), (1.7, 1.35, 1.15), "MAT_Bark_WarmBrown")
    b.add_frustum((0, 0, 4.05), 1.55, 0.18, 0.9, 4, "MAT_Leaves_DeepGreen", (0, 0, math.radians(45)))
    b.add_box((0, -0.71, 3.27), (0.45, 0.05, 0.42), "MAT_Worldroot_Cyan_Emission")
    b.add_box((0, -1.18, 2.84), (2.2, 0.12, 0.12), "MAT_Wood_GoldenTrim")


def build_arch_doorway(b):
    b.add_frustum((-0.78, 0, 1.2), 0.24, 0.18, 2.4, 7, "MAT_Bark_DarkRoot", (0.08, 0, 0.08))
    b.add_frustum((0.78, 0, 1.2), 0.24, 0.18, 2.4, 7, "MAT_Bark_DarkRoot", (-0.08, 0, -0.08))
    b.add_box((0, 0, 2.45), (1.9, 0.38, 0.32), "MAT_Bark_WarmBrown")
    b.add_bicone((0, -0.24, 2.55), 0.22, 0.58, "MAT_Worldroot_Cyan_Emission")
    b.add_box((0, -0.22, 0.18), (1.8, 0.32, 0.18), "MAT_Stone_MossyGray")


def build_balcony(b):
    b.add_box((0, 0, 0.18), (3.1, 1.5, 0.25), "MAT_Bark_WarmBrown")
    for x in [-1.35, 1.35]:
        b.add_frustum((x, -0.55, 0.85), 0.08, 0.07, 1.35, 6, "MAT_Bark_DarkRoot")
        b.add_frustum((x, 0.55, 0.85), 0.08, 0.07, 1.35, 6, "MAT_Bark_DarkRoot")
    b.add_box((0, -0.62, 1.18), (3.0, 0.08, 0.10), "MAT_Wood_GoldenTrim")
    b.add_box((0, 0.62, 1.18), (3.0, 0.08, 0.10), "MAT_Wood_GoldenTrim")


def build_root_bridge_straight(b):
    b.add_frustum((0, 0, 0.20), 0.28, 0.22, 4.2, 8, "MAT_Bark_DarkRoot", (0, math.radians(90), 0))
    b.add_box((0, 0, 0.42), (4.1, 1.05, 0.16), "MAT_Bark_WarmBrown")
    for y in [-0.62, 0.62]:
        b.add_box((0, y, 0.95), (4.2, 0.08, 0.10), "MAT_Wood_GoldenTrim")
        for x in [-1.7, -0.55, 0.55, 1.7]:
            b.add_box((x, y, 0.68), (0.08, 0.08, 0.68), "MAT_Bark_DarkRoot")


def build_root_bridge_curved(b):
    for i in range(5):
        angle = math.radians(-28 + i * 14)
        x = (i - 2) * 0.82
        y = math.sin(angle) * 0.9
        b.add_box((x, y, 0.32), (0.95, 1.0, 0.16), "MAT_Bark_WarmBrown", (0, 0, angle))
        b.add_frustum((x, y, 0.18), 0.18, 0.15, 1.0, 7, "MAT_Bark_DarkRoot", (0, math.radians(90), angle))
    for x, y in [(-1.65, -0.8), (0, -0.95), (1.65, -0.8), (-1.65, 0.8), (0, 0.95), (1.65, 0.8)]:
        b.add_box((x, y, 0.82), (0.08, 0.08, 0.75), "MAT_Bark_DarkRoot")


def build_stairs(b):
    for i in range(6):
        b.add_box((0, i * 0.42, 0.09 + i * 0.13), (1.65, 0.38, 0.18), "MAT_Stone_MossyGray")
    b.add_frustum((-0.93, 1.05, 0.55), 0.08, 0.08, 2.6, 6, "MAT_Bark_DarkRoot", (math.radians(20), 0, 0))
    b.add_frustum((0.93, 1.05, 0.55), 0.08, 0.08, 2.6, 6, "MAT_Bark_DarkRoot", (math.radians(20), 0, 0))


def build_railing(b):
    for x in [-1.2, 0, 1.2]:
        b.add_box((x, 0, 0.55), (0.10, 0.10, 1.1), "MAT_Bark_DarkRoot")
    b.add_box((0, 0, 1.08), (2.65, 0.10, 0.12), "MAT_Wood_GoldenTrim")
    b.add_box((0, 0, 0.55), (2.65, 0.08, 0.08), "MAT_Bark_WarmBrown")


def build_crystal_cluster(scale):
    def builder(b):
        b.add_bicone((0, 0, 0.55 * scale), 0.28 * scale, 1.1 * scale, "MAT_Worldroot_Cyan_Emission")
        b.add_bicone((0.33 * scale, 0.12 * scale, 0.36 * scale), 0.17 * scale, 0.72 * scale, "MAT_Rune_Cyan_Emission", rotation=(0.08, 0, 0.2))
        b.add_bicone((-0.28 * scale, -0.16 * scale, 0.28 * scale), 0.13 * scale, 0.56 * scale, "MAT_Rune_Cyan_Emission", rotation=(-0.06, 0.05, -0.25))
        b.add_disc((0, 0, 0.02), 0.55 * scale, 0.35 * scale, "MAT_Moss", sides=12)
    return builder


def build_rune_monolith(b):
    b.add_box((0, 0, 1.05), (0.58, 0.36, 2.1), "MAT_Stone_MossyGray", (0.03, 0, 0.12))
    b.add_bicone((0, -0.21, 1.22), 0.18, 0.52, "MAT_Rune_Cyan_Emission", rotation=(math.radians(90), 0, 0))
    b.add_box((0, -0.22, 0.62), (0.36, 0.04, 0.08), "MAT_GoldTrim")


def build_waypoint_stone(b):
    b.add_frustum((0, 0, 0.42), 0.48, 0.38, 0.82, 8, "MAT_Stone_MossyGray")
    b.add_bicone((0, 0, 1.05), 0.26, 0.62, "MAT_Worldroot_Cyan_Emission")
    b.add_disc((0, 0, 0.86), 0.42, 0.42, "MAT_Rune_Cyan_Emission", sides=18)


def build_memory_echo_marker(b):
    b.add_disc((0, 0, 0.02), 0.86, 0.86, "MAT_Echo_Transparent", sides=24)
    b.add_box((0, 0, 0.7), (0.32, 0.20, 0.95), "MAT_Echo_Transparent")
    b.add_bicone((0, 0, 1.35), 0.22, 0.34, "MAT_Echo_Transparent")
    b.add_box((0, -0.02, 0.9), (1.0, 0.04, 0.08), "MAT_Rune_Cyan_Emission")


def build_root_vein(b):
    b.add_box((0, 0, 0.05), (2.7, 0.22, 0.08), "MAT_Bark_DarkRoot", (0, 0, 0.05))
    b.add_box((-0.45, 0.0, 0.12), (1.55, 0.06, 0.04), "MAT_Rune_Cyan_Emission", (0, 0, 0.08))
    b.add_box((0.72, 0.10, 0.13), (0.85, 0.05, 0.04), "MAT_Rune_Cyan_Emission", (0, 0, -0.32))


def build_lantern_post(b):
    b.add_frustum((0, 0, 1.05), 0.08, 0.07, 2.1, 6, "MAT_Bark_DarkRoot")
    b.add_box((0.42, 0, 1.94), (0.92, 0.08, 0.08), "MAT_Wood_GoldenTrim")
    b.add_box((0.86, 0, 1.58), (0.28, 0.28, 0.38), "MAT_Lantern_Warm_Emission")
    b.add_box((0.86, 0, 1.85), (0.34, 0.34, 0.08), "MAT_Bark_WarmBrown")


def build_banner_post(b):
    b.add_frustum((0, 0, 1.2), 0.08, 0.07, 2.4, 6, "MAT_Bark_DarkRoot")
    b.add_box((0.42, -0.02, 1.62), (0.72, 0.05, 0.95), "MAT_Banner_VerdantGreen")
    b.add_box((0.42, -0.055, 1.72), (0.18, 0.03, 0.28), "MAT_GoldTrim")
    b.add_box((0.42, -0.055, 1.36), (0.30, 0.03, 0.06), "MAT_GoldTrim")


def build_bench(b):
    b.add_box((0, 0, 0.45), (1.7, 0.52, 0.18), "MAT_Bark_WarmBrown")
    b.add_box((0, 0.28, 0.82), (1.7, 0.12, 0.52), "MAT_Bark_DarkRoot", (math.radians(10), 0, 0))
    for x in [-0.65, 0.65]:
        b.add_box((x, -0.16, 0.22), (0.16, 0.18, 0.44), "MAT_Bark_DarkRoot")


def build_signpost(b):
    b.add_frustum((0, 0, 0.9), 0.08, 0.07, 1.8, 6, "MAT_Bark_DarkRoot")
    b.add_box((0.44, -0.02, 1.25), (0.9, 0.10, 0.34), "MAT_Bark_WarmBrown", (0, 0, 0.05))
    b.add_box((0.15, -0.08, 1.25), (0.22, 0.04, 0.06), "MAT_GoldTrim")


def build_shrine_pedestal(b):
    b.add_frustum((0, 0, 0.28), 0.52, 0.44, 0.55, 9, "MAT_Stone_MossyGray")
    b.add_box((0, 0, 0.72), (0.75, 0.75, 0.26), "MAT_Bark_WarmBrown")
    b.add_bicone((0, 0, 1.13), 0.24, 0.72, "MAT_Worldroot_Cyan_Emission")


def build_hanging_lantern(b):
    b.add_box((0, 0, 0.7), (0.04, 0.04, 1.35), "MAT_Bark_DarkRoot")
    b.add_box((0, 0, 0.08), (0.38, 0.38, 0.18), "MAT_Bark_WarmBrown")
    b.add_box((0, 0, 0.30), (0.30, 0.30, 0.34), "MAT_Lantern_Warm_Emission")
    b.add_box((0, 0, 0.54), (0.34, 0.34, 0.08), "MAT_GoldTrim")


def build_fern(b):
    for angle in [-0.7, -0.35, 0, 0.35, 0.7]:
        b.add_box((math.sin(angle) * 0.22, math.cos(angle) * 0.1, 0.38), (0.14, 0.04, 0.78), "MAT_Leaves_LightGreen", (0.45, angle, angle))


def build_broad_leaf(b):
    for i, angle in enumerate([0, 1.45, 2.8, 4.2]):
        b.add_frustum((math.cos(angle) * 0.18, math.sin(angle) * 0.18, 0.24), 0.05, 0.03, 0.48, 5, "MAT_Leaves_DeepGreen", (0.7, 0.2, angle))
        b.add_box((math.cos(angle) * 0.34, math.sin(angle) * 0.34, 0.42), (0.34, 0.12, 0.08), "MAT_Leaves_LightGreen", (0.4, 0.18, angle))


def build_flower(color_mat):
    def builder(b):
        b.add_frustum((0, 0, 0.22), 0.035, 0.025, 0.44, 5, "MAT_Leaves_DeepGreen")
        for angle in [0, math.pi / 2, math.pi, math.pi * 1.5]:
            b.add_box((math.cos(angle) * 0.12, math.sin(angle) * 0.12, 0.48), (0.18, 0.08, 0.04), color_mat, (0.25, 0, angle))
        b.add_bicone((0, 0, 0.49), 0.055, 0.08, "MAT_GoldTrim")
    return builder


def build_glowing_mushroom(b):
    b.add_frustum((0, 0, 0.24), 0.09, 0.06, 0.48, 7, "MAT_Stone_MossyGray")
    b.add_frustum((0, 0, 0.58), 0.38, 0.14, 0.26, 12, "MAT_Mushroom_Pink")
    b.add_bicone((0, 0, 0.72), 0.11, 0.14, "MAT_Rune_Cyan_Emission")


def build_moss_patch(b):
    b.add_disc((0, 0, 0.02), 0.88, 0.42, "MAT_Moss", sides=14, rotation=(0, 0, 0.2))
    b.add_box((0.18, 0.04, 0.08), (0.42, 0.10, 0.06), "MAT_Leaves_DeepGreen", (0, 0, 0.25))


def build_hanging_vine(b):
    for x in [-0.16, 0, 0.16]:
        b.add_box((x, 0, -0.6), (0.045, 0.045, 1.2 + abs(x)), "MAT_Leaves_DeepGreen", (0.06, 0.02, x * 2))
    b.add_box((0, 0, 0.04), (0.55, 0.08, 0.08), "MAT_Bark_DarkRoot")


def build_mossy_rock(b):
    b.add_frustum((0, 0, 0.28), 0.55, 0.38, 0.56, 7, "MAT_Stone_MossyGray", (0.08, -0.04, 0.2))
    b.add_disc((0, 0, 0.58), 0.34, 0.22, "MAT_Moss", sides=10)


def add_label(collection, text, location, materials):
    bpy.ops.object.text_add(location=location, rotation=(math.radians(72), 0, 0))
    obj = bpy.context.object
    obj.name = f"KIT_LINEUP_Label_{text.replace(' ', '_')}"
    obj.data.name = f"{obj.name}_Curve"
    obj.data.body = text
    obj.data.align_x = "CENTER"
    obj.data.align_y = "CENTER"
    obj.data.size = 0.28
    obj.data.materials.append(materials["MAT_Label_Dark"])
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    collection.objects.link(obj)
    return obj


def build_assets(collections, materials):
    assets = []
    rows = {
        "architecture": (collections["KIT_ARCHITECTURE"], -9.0),
        "worldroot": (collections["KIT_WORLDROOT_PROPS"], -2.8),
        "village": (collections["KIT_VILLAGE_PROPS"], 3.2),
        "foliage": (collections["KIT_FOLIAGE"], 8.0),
    }
    specs = [
        ("architecture", "ARCH_Sylvaen_TreehouseModule", -9.6, build_treehouse),
        ("architecture", "ARCH_Sylvaen_ArchedDoorway", -6.4, build_arch_doorway),
        ("architecture", "ARCH_Sylvaen_BalconyPlatform", -3.3, build_balcony),
        ("architecture", "ARCH_Sylvaen_RootBridgeStraight", -0.1, build_root_bridge_straight),
        ("architecture", "ARCH_Sylvaen_RootBridgeCurved", 3.2, build_root_bridge_curved),
        ("architecture", "ARCH_Sylvaen_StairSegment", 6.2, build_stairs),
        ("architecture", "ARCH_Sylvaen_RailingSegment", 9.1, build_railing),
        ("worldroot", "PROPS_Worldroot_CyanCrystalClusterSmall", -9.6, build_crystal_cluster(0.75)),
        ("worldroot", "PROPS_Worldroot_CyanCrystalClusterMedium", -6.6, build_crystal_cluster(1.05)),
        ("worldroot", "PROPS_Worldroot_CyanCrystalClusterLarge", -3.5, build_crystal_cluster(1.35)),
        ("worldroot", "PROPS_Worldroot_VerdantRuneMonolith", -0.4, build_rune_monolith),
        ("worldroot", "PROPS_Worldroot_WaypointStone", 2.4, build_waypoint_stone),
        ("worldroot", "PROPS_Worldroot_MemoryEchoMarker", 5.3, build_memory_echo_marker),
        ("worldroot", "PROPS_Worldroot_GlowingRootVein", 8.5, build_root_vein),
        ("village", "PROPS_Village_LanternPost", -9.6, build_lantern_post),
        ("village", "PROPS_Village_BannerPost", -6.4, build_banner_post),
        ("village", "PROPS_Village_WoodenBench", -3.2, build_bench),
        ("village", "PROPS_Village_Signpost", -0.1, build_signpost),
        ("village", "PROPS_Village_SmallShrinePedestal", 3.0, build_shrine_pedestal),
        ("village", "PROPS_Village_HangingLantern", 6.0, build_hanging_lantern),
        ("foliage", "FOLIAGE_FernCluster", -9.6, build_fern),
        ("foliage", "FOLIAGE_BroadLeafPlant", -7.0, build_broad_leaf),
        ("foliage", "FOLIAGE_PurpleFlower", -4.4, build_flower("MAT_Flower_Purple")),
        ("foliage", "FOLIAGE_BlueFlower", -2.1, build_flower("MAT_Flower_Blue")),
        ("foliage", "FOLIAGE_GlowingMushroom", 0.2, build_glowing_mushroom),
        ("foliage", "FOLIAGE_MossPatch", 2.8, build_moss_patch),
        ("foliage", "FOLIAGE_HangingVine", 5.4, build_hanging_vine),
        ("foliage", "FOLIAGE_SmallMossyRock", 8.2, build_mossy_rock),
    ]
    for category, name, x, build_func in specs:
        collection, y = rows[category]
        obj = make_asset(name, collection, (x, y, 0), materials, build_func)
        obj["asset_category"] = category
        assets.append(obj)
    return assets


def setup_stage(collections, materials):
    stage = collections["KIT_LINEUP_STAGE"]
    builder = MeshBuilder()
    builder.add_box((0, -0.3, -0.04), (23.5, 22.0, 0.08), "MAT_Lineup_Base")
    obj = make_asset("KIT_LINEUP_AssetSheetBase", stage, (0, 0, 0), materials, lambda b: b.__dict__.update(builder.__dict__))
    obj["asset_category"] = "lineup_stage"


def look_at(obj, target):
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_lighting(collections):
    col = collections["KIT_LIGHTING_RENDER"]
    bpy.ops.object.light_add(type="SUN", location=(-6, -8, 12), rotation=(math.radians(45), 0, math.radians(-38)))
    sun = bpy.context.object
    sun.name = "KIT_LIGHTING_SunKey_Warm"
    sun.data.energy = 2.7
    sun.data.angle = math.radians(5.0)
    for current in list(sun.users_collection):
        current.objects.unlink(sun)
    col.objects.link(sun)
    bpy.ops.object.light_add(type="AREA", location=(0, -10, 8), rotation=(math.radians(60), 0, 0))
    area = bpy.context.object
    area.name = "KIT_LIGHTING_AreaFill_Soft"
    area.data.energy = 560
    area.data.size = 14
    area.data.color = (0.70, 0.90, 0.82)
    if hasattr(area.data, "use_shadow"):
        area.data.use_shadow = False
    for current in list(area.users_collection):
        current.objects.unlink(area)
    col.objects.link(area)
    bpy.ops.object.camera_add(location=(0, -27.0, 19.5))
    camera = bpy.context.object
    camera.name = "KIT_LIGHTING_Camera_AssetLineup"
    look_at(camera, Vector((0, -0.55, 1.0)))
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 27.0
    camera.data.dof.use_dof = False
    bpy.context.scene.camera = camera
    for current in list(camera.users_collection):
        current.objects.unlink(camera)
    col.objects.link(camera)


def setup_scene_settings():
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    engines = {item.identifier for item in scene.render.bl_rna.properties["engine"].enum_items}
    scene.render.engine = "BLENDER_EEVEE_NEXT" if "BLENDER_EEVEE_NEXT" in engines else "BLENDER_EEVEE"
    if hasattr(scene, "eevee"):
        for attr, value in [("use_gtao", True), ("gtao_distance", 4), ("gtao_factor", 1.2), ("use_bloom", True), ("bloom_intensity", 0.06)]:
            if hasattr(scene.eevee, attr):
                setattr(scene.eevee, attr, value)
    scene.world = bpy.data.worlds.new("KIT_LIGHTING_World_SoftBlueGreen") if not scene.world else scene.world
    scene.world.color = (0.15, 0.26, 0.25)
    try:
        scene.view_settings.view_transform = "Standard"
    except TypeError:
        pass
    try:
        scene.view_settings.look = "Medium High Contrast"
    except TypeError:
        scene.view_settings.look = "None"
    scene.view_settings.exposure = 0.0
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080


def object_triangles(obj):
    return sum(len(poly.vertices) - 2 for poly in obj.data.polygons)


def create_manifest(collections, materials, asset_objects):
    objects = []
    for obj in asset_objects:
        mat_names = [slot.material.name for slot in obj.material_slots if slot.material]
        objects.append(
            {
                "name": obj.name,
                "category": obj.get("asset_category", ""),
                "collection": next((c.name for c in obj.users_collection), ""),
                "materials": mat_names,
                "approximate_triangles": object_triangles(obj),
                "origin": [round(value, 3) for value in obj.location[:]],
            }
        )
    manifest = {
        "kit": "thornveil_modular_asset_kit_v001",
        "style": "stylized low-poly MMORPG, hand-painted color blocks, non-photorealistic",
        "collections": list(collections.keys()),
        "object_count": len(objects),
        "objects": objects,
        "material_names": sorted(materials.keys()),
        "approximate_total_triangles": sum(item["approximate_triangles"] for item in objects),
        "outputs": {
            "blend": str(OUT_BLEND),
            "glb": str(OUT_GLB),
            "lineup_render": str(OUT_RENDER),
        },
        "notes": [
            "Each requested asset is a separate mesh object with a base/snap-point origin.",
            "Architecture pieces are modular but still intentionally chunky and readable.",
            "Materials use MAT_ names and simple saturated colors with emission for magical props.",
        ],
    }
    OUT_MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")


def render_lineup():
    bpy.context.scene.render.filepath = str(OUT_RENDER)
    bpy.ops.render.render(write_still=True)


def export_asset_glb(asset_objects):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in asset_objects:
        obj.select_set(True)
    bpy.ops.export_scene.gltf(filepath=str(OUT_GLB), export_format="GLB", use_selection=True)


def main():
    ensure_dirs()
    clear_scene()
    collections = create_collections()
    materials = create_materials()
    setup_scene_settings()
    setup_stage(collections, materials)
    assets = build_assets(collections, materials)
    setup_lighting(collections)
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_BLEND))
    export_asset_glb(assets)
    render_lineup()
    create_manifest(collections, materials, assets)


if __name__ == "__main__":
    main()
