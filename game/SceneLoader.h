#pragma once
#include "engine/graphics/AssetRegistry.h"
#include "engine/ecs/ECSWorld.h"
#include <memory>

namespace Henky3D {

class SceneLoader {
public:
    SceneLoader(AssetRegistry* assetRegistry, ECSWorld* ecs)
        : m_AssetRegistry(assetRegistry), m_ECS(ecs) {}

    // Create a procedural ground plane
    MeshHandle CreateGroundPlane(float size, int subdivisions);
    
    // Create a procedural cube
    MeshHandle CreateCube(float size);
    
    // Create a procedural tree (simple trunk + foliage)
    struct TreeHandles {
        MeshHandle Trunk;
        MeshHandle Foliage;
    };
    TreeHandles CreateSimpleTree();
    
    // Create demo scene entities
    void LoadDemoScene();

private:
    AssetRegistry* m_AssetRegistry;
    ECSWorld* m_ECS;
};

} // namespace Henky3D
