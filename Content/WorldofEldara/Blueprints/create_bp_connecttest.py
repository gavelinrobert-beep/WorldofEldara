"""
BP_ConnectTest Blueprint Creation Script

This script creates the BP_ConnectTest Blueprint with automatic character creation flow.
Must be run from within Unreal Editor's Python console or via command line with Unreal's Python.

Usage in Unreal Editor:
1. Enable Python Editor Script Plugin
2. Window → Developer Tools → Output Log
3. Copy this file to your project's Content/Python directory
4. Run: py "path/to/create_bp_connecttest.py"

Or use Unreal's command line:
UnrealEditor.exe "path/to/Eldara.uproject" -run=pythonscript -script="path/to/create_bp_connecttest.py"
"""

import unreal

# Asset paths
BLUEPRINT_PATH = "/Game/WorldofEldara/Blueprints/BP_ConnectTest"
BLUEPRINT_NAME = "BP_ConnectTest"

def create_bp_connecttest():
    """Creates the BP_ConnectTest Blueprint with automatic character creation flow."""
    
    unreal.log("Starting BP_ConnectTest creation...")
    
    # Create the Blueprint asset
    asset_tools = unreal.AssetToolsHelpers.get_asset_tools()
    blueprint_factory = unreal.BlueprintFactory()
    blueprint_factory.set_editor_property("parent_class", unreal.Actor)
    
    # Create the asset
    blueprint = asset_tools.create_asset(
        asset_name=BLUEPRINT_NAME,
        package_path="/Game/WorldofEldara/Blueprints",
        asset_class=unreal.Blueprint,
        factory=blueprint_factory
    )
    
    if not blueprint:
        unreal.log_error("Failed to create Blueprint asset")
        return False
    
    unreal.log("Blueprint asset created successfully")
    unreal.log("⚠️  Note: Blueprint node graph must be manually configured in Unreal Editor")
    unreal.log("Please follow the instructions in BP_ConnectTest_Implementation_Guide.md")
    
    # Blueprint visual scripting cannot be fully automated via Python API
    # The user must open the Blueprint and add the nodes manually following the guide
    
    unreal.log("✅ BP_ConnectTest Blueprint created at: " + BLUEPRINT_PATH)
    unreal.log("📖 Next steps: Open the Blueprint and follow BP_ConnectTest_Implementation_Guide.md")
    
    return True

if __name__ == "__main__":
    create_bp_connecttest()
