#!/usr/bin/env python3
"""Lightweight Unreal project sanity check (no engine required)."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
UPROJECT = ROOT / "Eldara.uproject"

errors: list[str] = []
warnings: list[str] = []


def fail(message: str) -> None:
    errors.append(message)


def require_file(path: Path, label: str) -> None:
    if not path.is_file():
        fail(f"{label} is missing at {path.relative_to(ROOT)}")


def get_ini_value(text: str, key: str) -> str | None:
    match = re.search(rf"^{re.escape(key)}=(.+)$", text, re.MULTILINE)
    return match.group(1).strip() if match else None


def main() -> int:
    try:
        project_data = json.loads(UPROJECT.read_text())
    except FileNotFoundError:
        fail("Eldara.uproject is missing")
        project_data = None
    except json.JSONDecodeError as exc:
        fail(f"Eldara.uproject is not valid JSON: {exc}")
        project_data = None

    if project_data:
        engine_association = project_data.get("EngineAssociation")
        if not engine_association:
            fail("EngineAssociation is not set in Eldara.uproject")

        modules = {
            module.get("Name"): module
            for module in project_data.get("Modules", [])
            if isinstance(module, dict) and module.get("Name")
        }

        eldara_module = modules.get("Eldara")
        if not eldara_module:
            fail("Eldara module is missing from Eldara.uproject")
        else:
            if eldara_module.get("Type") != "Runtime":
                fail("Eldara module should have Type=\"Runtime\"")

        target_platforms = set(project_data.get("TargetPlatforms", []))
        missing_platforms = {"Win64", "Linux"} - target_platforms
        if missing_platforms:
            fail(f"Eldara.uproject missing target platforms: {', '.join(sorted(missing_platforms))}")

    source_root = ROOT / "Source"
    require_file(source_root / "Eldara.Target.cs", "Game target file")
    require_file(source_root / "EldaraEditor.Target.cs", "Editor target file")
    require_file(source_root / "Eldara" / "Eldara.Build.cs", "Eldara.Build.cs")

    build_path = source_root / "Eldara" / "Eldara.Build.cs"
    if build_path.is_file():
        build_text = build_path.read_text()
        for dep in ("EnhancedInput", "AIModule", "UMG", "Networking", "Sockets"):
            if dep not in build_text:
                fail(f"Eldara.Build.cs should depend on {dep}")

    engine_ini_path = ROOT / "Config" / "DefaultEngine.ini"
    if engine_ini_path.is_file():
        engine_ini = engine_ini_path.read_text()
        default_map = get_ini_value(engine_ini, "GameDefaultMap")
        if not default_map:
            fail("GameDefaultMap is not configured in DefaultEngine.ini")

        expected_mode = "/Script/Eldara.EldaraGameModeBase"
        if get_ini_value(engine_ini, "GlobalDefaultGameMode") != expected_mode:
            fail(f"GlobalDefaultGameMode should be {expected_mode}")

        expected_instance = "/Script/Eldara.EldaraGameInstance"
        if get_ini_value(engine_ini, "GameInstanceClass") != expected_instance:
            fail(f"GameInstanceClass should be {expected_instance}")
    else:
        fail("Config/DefaultEngine.ini is missing")

    if errors:
        print("[FAIL] Unreal project check failed:", file=sys.stderr)
        for issue in errors:
            print(f" - {issue}", file=sys.stderr)
        if warnings:
            print("Warnings:", file=sys.stderr)
            for warning in warnings:
                print(f" - {warning}", file=sys.stderr)
        return 1

    print("[PASS] Unreal project check succeeded.")
    if warnings:
        for warning in warnings:
            print(f"Warning: {warning}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
