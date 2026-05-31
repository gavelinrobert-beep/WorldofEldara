from __future__ import annotations

import json
import math
import re
from pathlib import Path
from typing import Iterable

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ASSETS = ROOT / "assets"
BLENDER_DIR = ASSETS / "blender"
PREFAB_DIR = ASSETS / "prefabs"
PREFAB_BLEND_DIR = PREFAB_DIR / "blender"
PREFAB_GLB_DIR = PREFAB_DIR / "glb"
PREFAB_PREVIEW_DIR = PREFAB_DIR / "previews"
REPORT_DIR = ASSETS / "reports"
MANIFEST_PATH = REPORT_DIR / "prefab_manifest.json"


SOURCE_SCENES = {
    "thornveil": BLENDER_DIR / "heartbough_glade_entrance_v001.blend",
    "elarthalas": BLENDER_DIR / "elarthalas_approach_visual_benchmark_v002.blend",
    "memory_wastes": BLENDER_DIR / "memory_wastes_visual_benchmark_v002.blend",
}


PREFABS = [
    {
        "id": "thornveil_heartbough_tree",
        "zone": "Thornveil Enclave",
        "category": "Hero Tree",
        "source": "thornveil",
        "prefixes": ["SCENE_Worldroot_HeartboughFocalTree_AncientMemoryRoot_"],
        "notes": "Central Heartbough / Worldroot focal tree extracted from the Thornveil hub scene.",
    },
    {
        "id": "thornveil_sylvaen_treehouse",
        "zone": "Thornveil Enclave",
        "category": "Architecture",
        "source": "thornveil",
        "prefixes": [
            "SCENE_Architecture_BackTreehouseMemoryArchive",
            "SCENE_Architecture_BackMemoryArchive_",
            "SCENE_Architecture_TreehouseVerdantMemorySigil_02",
        ],
        "notes": "Integrated Sylvaen treehouse module with grown bark supports and Verdant identity details.",
    },
    {
        "id": "thornveil_root_bridge",
        "zone": "Thornveil Enclave",
        "category": "Architecture",
        "source": "thornveil",
        "prefixes": [
            "SCENE_Architecture_MidRootBridge_LeftToCenter",
            "SCENE_Architecture_BackCurvedRootBridge",
        ],
        "notes": "Reusable organic root bridge pieces from the midground village path.",
    },
    {
        "id": "thornveil_verdant_banner",
        "zone": "Thornveil Enclave",
        "category": "Village Prop",
        "source": "thornveil",
        "prefixes": ["SCENE_VillageProps_VerdantBanner_00"],
        "notes": "Readable Verdant faction banner for path and hub dressing.",
    },
    {
        "id": "thornveil_crystal_cluster",
        "zone": "Thornveil Enclave",
        "category": "Worldroot Prop",
        "source": "thornveil",
        "prefixes": ["SCENE_Worldroot_MeaningfulWorldrootCrystal_02"],
        "notes": "Cyan Worldroot crystal cluster with moss stone placement base.",
    },
    {
        "id": "elarthalas_silent_gate",
        "zone": "Elar'Thalas Approach",
        "category": "Hero Landmark",
        "source": "elarthalas",
        "prefixes": [
            "ELAR_ArchiveCity_SilentGate_DarkRootSealedBackplate",
            "ELAR_ArchiveCity_SilentGate_Left",
            "ELAR_ArchiveCity_SilentGate_Right",
            "ELAR_ArchiveCity_SilentGate_Closed",
            "ELAR_ArchiveCity_SilentGate_Central",
            "ELAR_ArchiveCity_SilentGate_Locked",
            "ELAR_ArchiveCity_SilentGate_Pale",
            "ELAR_ArchiveCity_SilentGate_Narrow",
            "ELAR_ArchiveCity_SilentGate_Outer",
            "ELAR_ArchiveCity_SilentGate_Inner",
            "ELAR_ArchiveCity_SilentGate_Door",
            "ELAR_ArchiveCity_SilentGate_Crown",
        ],
        "notes": "Sealed living archive-city gate, kept as a full modular hero landmark group.",
    },
    {
        "id": "elarthalas_ward_monoliths",
        "zone": "Elar'Thalas Approach",
        "category": "Worldroot Ward",
        "source": "elarthalas",
        "prefixes": [
            "ELAR_WardsAndProps_WardlineScannerMonolith_Left_03",
            "ELAR_WardsAndProps_WardlineScannerMonolith_Right_03",
        ],
        "notes": "Matched ward monolith pair with inward scanning silhouettes, root bindings and cyan rune cuts.",
    },
    {
        "id": "elarthalas_rootroad",
        "zone": "Elar'Thalas Approach",
        "category": "Terrain Module",
        "source": "elarthalas",
        "prefixes": ["ELAR_Terrain_SacredRootroad_"],
        "notes": "Raised sacred rootroad module with embedded slabs, root borders and guide runes.",
    },
    {
        "id": "elarthalas_greenspire_shelter",
        "zone": "Elar'Thalas Approach",
        "category": "Camp Architecture",
        "source": "elarthalas",
        "prefixes": [
            "ELAR_WardsAndProps_GreenspireCamp_RootShelter",
            "ELAR_WardsAndProps_GreenspireCamp_VerdantForwardBanner",
            "ELAR_WardsAndProps_GreenspireCamp_WarmPerimeterLantern_00",
        ],
        "notes": "Compact Sylvaen forward shelter prefab with a banner and a warm camp lantern.",
    },
    {
        "id": "elarthalas_high_elf_intrusion_device",
        "zone": "Elar'Thalas Approach",
        "category": "Aelthar Intrusion",
        "source": "elarthalas",
        "prefixes": [
            "ELAR_ArcaneIntrusion_HighElfProbePylon_00",
            "ELAR_ArcaneIntrusion_VioletScanCircle_00",
            "ELAR_ArcaneIntrusion_HighElfTriangulationFrame_00",
            "ELAR_ArcaneIntrusion_Aelthar",
            "ELAR_ArcaneIntrusion_BrokenWardMonolith_",
        ],
        "notes": "Foreign angular silver-blue arcane intrusion device with broken ward contrast.",
    },
    {
        "id": "memory_wastes_dead_god_shrine",
        "zone": "The Memory Wastes",
        "category": "Hero Landmark",
        "source": "memory_wastes",
        "prefixes": ["WASTE_DeadGodShrine_"],
        "notes": "Dead God Shrine kit: broken halo, cracked altar, missing-name slab and pale divine leakage.",
    },
    {
        "id": "memory_wastes_lost_ruin_arch",
        "zone": "The Memory Wastes",
        "category": "Lost Civilization Ruin",
        "source": "memory_wastes",
        "prefixes": [
            "WASTE_OranynRuins_LargePartManifestedArchiveArch_",
            "WASTE_OranynRuins_LargeBrokenArchiveArch_",
        ],
        "notes": "Partially manifested Oranyn archive arch, useful for ghost ruin dressing.",
    },
    {
        "id": "memory_wastes_worldroot_rift",
        "zone": "The Memory Wastes",
        "category": "Hero Rift",
        "source": "memory_wastes",
        "prefixes": ["WASTE_RootRiftsAndStorms_WorldrootSheddingRift_"],
        "notes": "Worldroot shedding rift with cracked root fissure, root fibers, memory shards and controlled glow.",
    },
    {
        "id": "memory_wastes_echo_battlefield_props",
        "zone": "The Memory Wastes",
        "category": "Echo Battlefield",
        "source": "memory_wastes",
        "prefixes": ["WASTE_EchoBattlefield_"],
        "notes": "Echo battlefield dressing set: ghost banners, weapons, shield fragments and soldier echoes.",
    },
    {
        "id": "memory_wastes_memory_island_base",
        "zone": "The Memory Wastes",
        "category": "Terrain Module",
        "source": "memory_wastes",
        "prefixes": ["WASTE_Terrain_WorldrootSheddingRiftIsland_"],
        "notes": "Reusable fragmented memory island base with broken strata and void drop shadow.",
    },
]


def ensure_dirs() -> None:
    for directory in (PREFAB_BLEND_DIR, PREFAB_GLB_DIR, PREFAB_PREVIEW_DIR, REPORT_DIR):
        directory.mkdir(parents=True, exist_ok=True)


def safe_name(value: str) -> str:
    cleaned = re.sub(r"[^A-Za-z0-9_]+", "_", value)
    cleaned = re.sub(r"_+", "_", cleaned).strip("_")
    return cleaned or "Unnamed"


def object_matches(name: str, spec: dict) -> bool:
    return any(name.startswith(prefix) for prefix in spec.get("prefixes", [])) or name in spec.get("exact", [])


def matched_mesh_objects(spec: dict) -> list[bpy.types.Object]:
    return [
        obj
        for obj in bpy.context.scene.objects
        if obj.type == "MESH" and object_matches(obj.name, spec)
    ]


def world_bbox(objects: Iterable[bpy.types.Object]) -> tuple[Vector, Vector]:
    points: list[Vector] = []
    for obj in objects:
        for corner in obj.bound_box:
            points.append(obj.matrix_world @ Vector(corner))
    if not points:
        zero = Vector((0.0, 0.0, 0.0))
        return zero, zero
    min_corner = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    max_corner = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return min_corner, max_corner


def center_prefab(objects: list[bpy.types.Object]) -> dict:
    min_corner, max_corner = world_bbox(objects)
    origin = Vector(((min_corner.x + max_corner.x) * 0.5, (min_corner.y + max_corner.y) * 0.5, min_corner.z))
    for obj in objects:
        world = obj.matrix_world.copy()
        obj.parent = None
        obj.matrix_world = world
        obj.location -= origin

    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    if objects:
        bpy.context.view_layer.objects.active = objects[0]
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)

    min_after, max_after = world_bbox(objects)
    size = max_after - min_after
    return {
        "origin_strategy": "Centered on prefab XY bounds; Z origin placed at lowest bounding point for placement.",
        "bounds_min": [round(v, 3) for v in min_after],
        "bounds_max": [round(v, 3) for v in max_after],
        "dimensions_m": [round(size.x, 3), round(size.y, 3), round(size.z, 3)],
    }


def remove_unmatched_objects(keep: list[bpy.types.Object]) -> None:
    keep_set = set(keep)
    for obj in list(bpy.context.scene.objects):
        if obj not in keep_set:
            bpy.data.objects.remove(obj, do_unlink=True)


def rename_prefab_objects(objects: list[bpy.types.Object], prefab_id: str) -> list[str]:
    names = []
    for index, obj in enumerate(objects):
        original = safe_name(obj.name)
        obj.name = f"PREFAB_{safe_name(prefab_id)}_{index:02d}_{original}"
        obj.data.name = f"{obj.name}_Mesh"
        names.append(obj.name)
    return names


def ensure_material_prefixes(objects: Iterable[bpy.types.Object]) -> list[str]:
    names: set[str] = set()
    for obj in objects:
        for slot in obj.material_slots:
            material = slot.material
            if material is None:
                continue
            if not material.name.startswith("MAT_"):
                material.name = f"MAT_{safe_name(material.name)}"
            names.add(material.name)
    return sorted(names)


def approximate_triangles(objects: Iterable[bpy.types.Object]) -> int:
    triangles = 0
    for obj in objects:
        if obj.type != "MESH":
            continue
        mesh = obj.data
        triangles += sum(max(1, len(poly.vertices) - 2) for poly in mesh.polygons)
    return triangles


def add_preview_material(name: str, color: tuple[float, float, float, float]) -> bpy.types.Material:
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    material.diffuse_color = color
    return material


def look_at(obj: bpy.types.Object, target: Vector) -> None:
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_preview_scene(objects: list[bpy.types.Object], preview_path: Path) -> None:
    min_corner, max_corner = world_bbox(objects)
    center = (min_corner + max_corner) * 0.5
    size = max_corner - min_corner
    largest = max(size.x, size.y, size.z, 1.0)

    ground_material = add_preview_material("MAT_Preview_NeutralGround", (0.06, 0.075, 0.07, 1.0))
    bpy.ops.mesh.primitive_plane_add(size=largest * 1.75, location=(center.x, center.y, min_corner.z - 0.025))
    ground = bpy.context.object
    ground.name = "PREVIEW_NeutralGround_NotExported"
    ground.data.name = "PREVIEW_NeutralGround_NotExported_Mesh"
    ground.data.materials.append(ground_material)

    bpy.ops.object.light_add(type="AREA", location=(center.x - largest * 0.7, center.y - largest * 1.1, center.z + largest * 1.4))
    key = bpy.context.object
    key.name = "PREVIEW_KeyLight_NotExported"
    key.data.energy = 450.0
    key.data.size = largest * 0.65

    bpy.ops.object.light_add(type="POINT", location=(center.x + largest * 0.7, center.y + largest * 0.4, center.z + largest * 0.7))
    fill = bpy.context.object
    fill.name = "PREVIEW_CyanFill_NotExported"
    fill.data.energy = 60.0
    fill.data.color = (0.45, 0.95, 0.9)

    bpy.ops.object.camera_add(location=(center.x + largest * 0.8, center.y - largest * 1.15, center.z + largest * 0.72))
    camera = bpy.context.object
    camera.name = "PREVIEW_AssetCamera_NotExported"
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = largest * 1.35
    look_at(camera, center + Vector((0.0, 0.0, size.z * 0.08)))
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.resolution_x = 1200
    scene.render.resolution_y = 900
    scene.render.film_transparent = False
    scene.render.filepath = str(preview_path)
    scene.world = scene.world or bpy.data.worlds.new("World")
    scene.world.color = (0.025, 0.035, 0.035)

    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        scene.render.engine = "BLENDER_EEVEE"

    if hasattr(scene, "eevee"):
        if hasattr(scene.eevee, "use_bloom"):
            scene.eevee.use_bloom = True
        if hasattr(scene.eevee, "use_gtao"):
            scene.eevee.use_gtao = True

    if hasattr(scene.view_settings, "view_transform"):
        scene.view_settings.view_transform = "Filmic"
    scene.view_settings.look = "Medium High Contrast"
    scene.view_settings.exposure = 0.0
    scene.view_settings.gamma = 1.0

    bpy.ops.render.render(write_still=True)


def export_selected_glb(objects: list[bpy.types.Object], path: Path) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    if objects:
        bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.gltf(
        filepath=str(path),
        export_format="GLB",
        use_selection=True,
        export_apply=True,
    )


def extract_prefab(spec: dict) -> dict:
    source_path = SOURCE_SCENES[spec["source"]]
    if not source_path.exists():
        raise FileNotFoundError(f"Missing source scene: {source_path}")

    bpy.ops.wm.open_mainfile(filepath=str(source_path))
    objects = matched_mesh_objects(spec)
    if not objects:
        raise RuntimeError(f"No objects matched prefab {spec['id']} from {source_path.name}")

    for obj in objects:
        world = obj.matrix_world.copy()
        obj.parent = None
        obj.matrix_world = world

    remove_unmatched_objects(objects)
    renamed = rename_prefab_objects(objects, spec["id"])
    material_names = ensure_material_prefixes(objects)
    transform_info = center_prefab(objects)
    triangle_count = approximate_triangles(objects)

    blend_path = PREFAB_BLEND_DIR / f"{spec['id']}.blend"
    glb_path = PREFAB_GLB_DIR / f"{spec['id']}.glb"
    preview_path = PREFAB_PREVIEW_DIR / f"{spec['id']}_preview.png"

    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    export_selected_glb(objects, glb_path)
    setup_preview_scene(objects, preview_path)

    return {
        "id": spec["id"],
        "zone": spec["zone"],
        "category": spec["category"],
        "source_blend": str(source_path.relative_to(ROOT)).replace("\\", "/"),
        "blend_path": str(blend_path.relative_to(ROOT)).replace("\\", "/"),
        "glb_path": str(glb_path.relative_to(ROOT)).replace("\\", "/"),
        "preview_path": str(preview_path.relative_to(ROOT)).replace("\\", "/"),
        "object_count": len(objects),
        "object_names": renamed,
        "material_names": material_names,
        "approximate_triangle_count": triangle_count,
        "origin_strategy": transform_info["origin_strategy"],
        "bounds_min": transform_info["bounds_min"],
        "bounds_max": transform_info["bounds_max"],
        "dimensions_m": transform_info["dimensions_m"],
        "matching_prefixes": spec.get("prefixes", []),
        "notes": spec["notes"],
    }


def build_manifest(entries: list[dict]) -> dict:
    return {
        "manifest": "World of Eldara modular prefab extraction",
        "version": "v001",
        "generated_from": {key: str(path.relative_to(ROOT)).replace("\\", "/") for key, path in SOURCE_SCENES.items()},
        "output_root": str(PREFAB_DIR.relative_to(ROOT)).replace("\\", "/"),
        "prefab_count": len(entries),
        "total_objects": sum(entry["object_count"] for entry in entries),
        "total_approximate_triangles": sum(entry["approximate_triangle_count"] for entry in entries),
        "prefabs": entries,
        "limitations": [
            "Prefabs are extracted from current zone-scene geometry and are not final collision-ready game assets.",
            "Origins are centered for placement at the prefab footprint base; some large landmark groups may still need manual pivot tuning in-engine.",
            "Preview renders use neutral studio lighting and are not intended to match final in-zone lighting.",
        ],
    }


def main() -> None:
    ensure_dirs()
    entries = []
    for spec in PREFABS:
        print(f"[prefab] Extracting {spec['id']}")
        entries.append(extract_prefab(spec))

    manifest = build_manifest(entries)
    MANIFEST_PATH.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(f"[prefab] Wrote {MANIFEST_PATH}")


if __name__ == "__main__":
    main()
