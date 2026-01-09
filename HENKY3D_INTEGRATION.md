# Henky3D Integration Guide

This document describes the integration of the Henky3D graphics engine into the World of Eldara project.

## Overview

Henky3D is a minimal PC graphics engine using OpenGL 4.6 Core profile. It has been integrated as an alternative rendering/engine layer to the Unreal Engine prototype, providing:

- Fast build times (no massive engine compilation)
- Full control over rendering pipeline
- Modern OpenGL 4.6 Core features
- ECS architecture via EnTT
- Immediate-mode GUI via ImGui
- Real-time lighting and shadows

## Architecture

```
WorldofEldara/
├── external/Henky3D/        # Engine submodule (OpenGL renderer, ECS, etc.)
│   ├── src/engine/          # Engine library code
│   │   ├── core/            # Window, Timer
│   │   ├── graphics/        # Renderer, GraphicsDevice, Shaders
│   │   ├── input/           # Input handling
│   │   └── ecs/             # ECS components and systems
│   ├── shaders/             # GLSL shaders (loaded at runtime)
│   └── CMakeLists.txt       # Engine build configuration
├── game/                    # World of Eldara game client
│   ├── main.cpp             # Game bootstrap and application logic
│   └── CMakeLists.txt       # Game executable configuration
├── CMakeLists.txt           # Root build orchestration
└── build/                   # Generated build artifacts
    ├── bin/                 # Compiled executables
    └── lib/                 # Static libraries
```

## Integration Approach

1. **Submodule**: Henky3D is included as a git submodule at `external/Henky3D`
2. **CMake Build System**: Root CMakeLists.txt orchestrates building both the engine and game
3. **Static Linking**: Game executable links against Henky3DEngine static library
4. **Shared Output**: All binaries output to `build/bin/` for consistent paths

## Building

### First Time Setup

```bash
# Clone with submodules
git clone --recursive https://github.com/gavelinrobert-beep/WorldofEldara.git

# Or initialize submodules after cloning
git submodule update --init --recursive
```

### Quick Build (Linux/macOS)

```bash
./build_game.sh
```

### Quick Build (Windows)

```batch
build_game.bat
```

### Manual Build

```bash
# Configure
mkdir build && cd build
cmake .. -DCMAKE_BUILD_TYPE=Release

# Build
cmake --build . --config Release --parallel

# Run (from repository root)
cd ..
./build/bin/WorldofEldaraGame
```

## Current Features

### Implemented
- ✅ Window creation and event handling (GLFW)
- ✅ OpenGL 4.6 Core context
- ✅ Modern rendering pipeline (depth prepass, forward shading)
- ✅ Real-time directional shadows with shadow mapping
- ✅ ECS architecture (EnTT)
- ✅ Basic 3D camera with fly-through controls
- ✅ Cube geometry rendering (placeholder for Worldroot)
- ✅ ImGui debug overlay
- ✅ FPS counter and performance stats
- ✅ Configurable rendering options (shadows, depth prepass, bias)

### In Progress
- 🚧 Asset loading system (GLTF models)
- 🚧 Material system
- 🚧 Texture loading (KTX2 format)
- 🚧 Network integration with .NET server

### Planned
- 📋 PBR material system
- 📋 Multiple light types (point, spot)
- 📋 Post-processing effects
- 📋 Particle systems
- 📋 UI system (beyond debug overlay)
- 📋 Audio system

## Game Bootstrap (main.cpp)

The game's `main.cpp` provides a minimal bootstrap that:

1. **Initializes Engine**: Creates Window, GraphicsDevice, Renderer, ECS
2. **Loads Scene**: Sets up camera, light, and placeholder geometry
3. **Game Loop**: 
   - Process input
   - Update scene (rotate cube, update camera)
   - Render frame with shadows
   - Display debug UI
4. **Cleanup**: Proper shutdown and resource release

### Scene Setup

The initial scene includes:
- **Camera**: Positioned at (0, 2, 5) looking at origin
- **Light**: Directional light representing Eldara's sun (warm color)
- **Cube**: Rotating cube representing the Worldroot (core of Eldara)
- **ImGui UI**: Debug panel with FPS, stats, and controls

### Controls

- **W/A/S/D**: Move camera forward/left/back/right
- **Q/E**: Move camera down/up
- **Mouse**: Look around (when camera control enabled)
- **ImGui Panel**: Toggle options, adjust speeds, enable/disable features

## Asset and Shader Paths

### Shaders
- Location: `external/Henky3D/shaders/`
- Format: GLSL 4.6 Core
- Files:
  - `Forward.vs.glsl` / `Forward.ps.glsl` - Forward rendering pass
  - `DepthPrepass.vs.glsl` / `DepthPrepass.ps.glsl` - Early Z-pass
  - `Shadow.vs.glsl` / `Shadow.ps.glsl` - Shadow map generation
  - `Common.glsl` - Shared shader utilities

### Assets
- Location: `assets/` (currently placeholder)
- Future formats: GLTF, KTX2, OGG/FLAC
- Working directory: Must run executable from repository root

## Performance Characteristics

### Memory Usage
- Engine overhead: ~10-20 MB
- Per-entity: ~200 bytes (Transform + Renderable + BoundingBox)
- Cube geometry: ~1 KB

### Rendering Performance
- Depth prepass: Optional, ~5% overhead, prevents overdraw
- Shadow mapping: ~15-20% overhead, 2048x2048 shadow map
- Target: 60+ FPS on modern GPUs with typical game scenes

### Build Times
- Clean build: ~30-60 seconds (includes dependency fetch)
- Incremental build: ~5-10 seconds
- Engine-only rebuild: ~15 seconds

## Customization

### Scene Constants

Edit `game/main.cpp` to adjust:
```cpp
namespace {
    constexpr glm::vec3 SCENE_BOUNDS_MIN = glm::vec3(-5.0f, -5.0f, -5.0f);
    constexpr glm::vec3 SCENE_BOUNDS_MAX = glm::vec3(5.0f, 5.0f, 5.0f);
}
```

### Camera Settings

Initial camera position and controls:
```cpp
camera.Position = { 0.0f, 2.0f, 5.0f };
bool m_EnableCameraControl = true;
float m_CameraMoveSpeed = 5.0f;
float m_CameraLookSpeed = 0.1f;
```

### Rendering Options

Toggle rendering features:
```cpp
bool m_DepthPrepassEnabled = true;
bool m_ShadowsEnabled = true;
float m_ShadowBias = 0.0005f;
```

## Troubleshooting

### Build Issues

**CMake not found**
```bash
# Ubuntu/Debian
sudo apt-get install cmake

# macOS
brew install cmake
```

**Missing X11 libraries (Linux)**
```bash
sudo apt-get install libx11-dev libxrandr-dev libxinerama-dev libxcursor-dev libxi-dev libgl1-mesa-dev
```

**Missing OpenGL (Windows)**
- Ensure GPU drivers are up to date
- Install Visual Studio with C++ development tools

### Runtime Issues

**Shader load errors**
- Verify you're running from repository root
- Check that `external/Henky3D/shaders/*.glsl` files exist

**OpenGL context creation failed**
- Update GPU drivers to support OpenGL 4.5+
- Check GPU capabilities with `glxinfo` (Linux) or GPU-Z (Windows)

**Black screen / No rendering**
- Enable debug output in GraphicsDevice
- Check console for OpenGL errors
- Verify shaders compiled successfully

## Integration with .NET Server

Future work will integrate the Henky3D client with the existing .NET server:

1. **Network Layer**: Implement TCP client using .NET's network protocol
2. **MessagePack**: Deserialize server packets
3. **Entity Sync**: Map server entities to ECS components
4. **Prediction**: Implement client-side prediction with server reconciliation
5. **Interpolation**: Smooth entity movement between server updates

## Contributing

When contributing to the Henky3D integration:

1. **Keep it minimal**: Avoid feature creep in the engine layer
2. **Test both platforms**: Verify Linux and Windows builds
3. **Document changes**: Update this guide for significant changes
4. **Performance first**: Profile before optimizing, maintain 60+ FPS target
5. **Lore alignment**: Ensure visual representation matches Eldara's lore

## References

- [Henky3D Repository](https://github.com/gavelinrobert-beep/Henky3D)
- [OpenGL 4.6 Core Specification](https://www.khronos.org/opengl/)
- [EnTT Documentation](https://github.com/skypjack/entt)
- [ImGui Documentation](https://github.com/ocornut/imgui)
- [CMake Documentation](https://cmake.org/documentation/)

---

*"The Worldroot is bleeding. Memory awaits. Now rendered in modern OpenGL."*
