add_executable(WorldofEldaraGame 
    main.cpp
    SceneLoader.cpp
    SceneLoader.h
)

# Link against Henky3DEngine which transitively provides all necessary includes
target_link_libraries(WorldofEldaraGame PRIVATE
    Henky3DEngine
)

# The engine headers are exposed through Henky3DEngine's PUBLIC interface
# Additional include for engine source directory
target_include_directories(WorldofEldaraGame PRIVATE
    ${CMAKE_SOURCE_DIR}/external/Henky3D/src
)

target_compile_features(WorldofEldaraGame PRIVATE cxx_std_20)
