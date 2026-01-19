@echo off
REM Quick build script for World of Eldara - Henky3D Edition (Windows)

echo ==========================================
echo   World of Eldara - Build Script
echo ==========================================
echo.

REM Check for CMake
where cmake >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] CMake is not installed. Please install CMake 3.20 or higher.
    exit /b 1
)

REM Initialize submodules if needed
if not exist "external\Henky3D\CMakeLists.txt" (
    echo [INFO] Initializing Henky3D submodule...
    git submodule update --init --recursive
)

REM Create build directory
if not exist "build" (
    echo [INFO] Creating build directory...
    mkdir build
)

cd build

REM Configure
echo [INFO] Configuring with CMake...
cmake .. -DCMAKE_BUILD_TYPE=Release

REM Build
echo [INFO] Building WorldofEldaraGame...
cmake --build . --config Release

echo.
echo ==========================================
echo   Build Complete!
echo ==========================================
echo.
echo To run the game:
echo   cd build\bin\Release ^&^& WorldofEldaraGame.exe
echo.
echo Note: Run from build\bin\Release so shader paths resolve.
echo.

cd ..
