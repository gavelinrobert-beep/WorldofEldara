#!/bin/bash
# Quick build script for World of Eldara - Henky3D Edition

set -e

echo "=========================================="
echo "  World of Eldara - Build Script"
echo "=========================================="
echo ""

# Check for CMake
if ! command -v cmake &> /dev/null; then
    echo "[ERROR] CMake is not installed. Please install CMake 3.20 or higher."
    exit 1
fi

# Initialize submodules if needed
if [ ! -f "external/Henky3D/CMakeLists.txt" ]; then
    echo "[INFO] Initializing Henky3D submodule..."
    git submodule update --init
fi

# Create build directory
if [ ! -d "build" ]; then
    echo "[INFO] Creating build directory..."
    mkdir build
fi

cd build

# Configure
echo "[INFO] Configuring with CMake..."
cmake .. -DCMAKE_BUILD_TYPE=Release

# Build
echo "[INFO] Building WorldofEldaraGame..."
cmake --build . --config Release --parallel

echo ""
echo "=========================================="
echo "  Build Complete!"
echo "=========================================="
echo ""
echo "To run the game:"
echo "  ./build/bin/WorldofEldaraGame"
echo ""
echo "Note: Run from the repository root directory."
echo ""
