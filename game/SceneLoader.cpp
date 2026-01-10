#include "SceneLoader.h"
#include "engine/ecs/Components.h"
#include <glm/gtc/random.hpp>
#include <iostream>

namespace Henky3D {

MeshHandle SceneLoader::CreateGroundPlane(float size, int subdivisions) {
    MeshAsset mesh;
    mesh.Name = "GroundPlane";
    
    float halfSize = size * 0.5f;
    float step = size / subdivisions;
    
    // Generate vertices
    for (int z = 0; z <= subdivisions; z++) {
        for (int x = 0; x <= subdivisions; x++) {
            Vertex v;
            v.Position = glm::vec3(
                -halfSize + x * step,
                0.0f,
                -halfSize + z * step
            );
            v.Normal = glm::vec3(0.0f, 1.0f, 0.0f);
            v.Tangent = glm::vec4(1.0f, 0.0f, 0.0f, 1.0f);
            v.UV = glm::vec2(
                static_cast<float>(x) / subdivisions * 10.0f,  // Tile 10 times
                static_cast<float>(z) / subdivisions * 10.0f
            );
            mesh.Vertices.push_back(v);
        }
    }
    
    // Generate indices
    for (int z = 0; z < subdivisions; z++) {
        for (int x = 0; x < subdivisions; x++) {
            int topLeft = z * (subdivisions + 1) + x;
            int topRight = topLeft + 1;
            int bottomLeft = (z + 1) * (subdivisions + 1) + x;
            int bottomRight = bottomLeft + 1;
            
            // First triangle
            mesh.Indices.push_back(topLeft);
            mesh.Indices.push_back(bottomLeft);
            mesh.Indices.push_back(topRight);
            
            // Second triangle
            mesh.Indices.push_back(topRight);
            mesh.Indices.push_back(bottomLeft);
            mesh.Indices.push_back(bottomRight);
        }
    }
    
    // Compute bounds
    mesh.BoundsMin = glm::vec3(-halfSize, 0.0f, -halfSize);
    mesh.BoundsMax = glm::vec3(halfSize, 0.0f, halfSize);
    
    // Create default material with grass texture if available
    MaterialAsset material;
    material.Name = "GroundMaterial";
    material.BaseColorFactor = glm::vec4(0.3f, 0.6f, 0.2f, 1.0f); // Green tint
    material.BaseColorTexture = m_AssetRegistry->GetDefaultWhiteTexture();
    material.NormalTexture = m_AssetRegistry->GetDefaultNormalTexture();
    material.RoughnessMetalnessTexture = m_AssetRegistry->GetDefaultRoughnessMetalnessTexture();
    material.RoughnessFactor = 0.8f;
    material.MetalnessFactor = 0.0f;
    mesh.MaterialIndex = m_AssetRegistry->CreateMaterial(material);
    
    std::cout << "Created ground plane: " << mesh.Vertices.size() << " vertices, " 
              << mesh.Indices.size() << " indices" << std::endl;
    
    return m_AssetRegistry->CreateMesh(mesh);
}

MeshHandle SceneLoader::CreateCube(float size) {
    MeshAsset mesh;
    mesh.Name = "Cube";
    
    float hs = size * 0.5f; // half size
    
    // Define cube vertices (24 vertices, 4 per face for proper normals)
    Vertex vertices[] = {
        // Front face (Z+)
        {{-hs, -hs, hs}, {0, 0, 1}, {1, 0, 0, 1}, {0, 0}},
        {{hs, -hs, hs}, {0, 0, 1}, {1, 0, 0, 1}, {1, 0}},
        {{hs, hs, hs}, {0, 0, 1}, {1, 0, 0, 1}, {1, 1}},
        {{-hs, hs, hs}, {0, 0, 1}, {1, 0, 0, 1}, {0, 1}},
        
        // Back face (Z-)
        {{hs, -hs, -hs}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 0}},
        {{-hs, -hs, -hs}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 0}},
        {{-hs, hs, -hs}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 1}},
        {{hs, hs, -hs}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 1}},
        
        // Right face (X+)
        {{hs, -hs, hs}, {1, 0, 0}, {0, 0, -1, 1}, {0, 0}},
        {{hs, -hs, -hs}, {1, 0, 0}, {0, 0, -1, 1}, {1, 0}},
        {{hs, hs, -hs}, {1, 0, 0}, {0, 0, -1, 1}, {1, 1}},
        {{hs, hs, hs}, {1, 0, 0}, {0, 0, -1, 1}, {0, 1}},
        
        // Left face (X-)
        {{-hs, -hs, -hs}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 0}},
        {{-hs, -hs, hs}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 0}},
        {{-hs, hs, hs}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 1}},
        {{-hs, hs, -hs}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 1}},
        
        // Top face (Y+)
        {{-hs, hs, hs}, {0, 1, 0}, {1, 0, 0, 1}, {0, 0}},
        {{hs, hs, hs}, {0, 1, 0}, {1, 0, 0, 1}, {1, 0}},
        {{hs, hs, -hs}, {0, 1, 0}, {1, 0, 0, 1}, {1, 1}},
        {{-hs, hs, -hs}, {0, 1, 0}, {1, 0, 0, 1}, {0, 1}},
        
        // Bottom face (Y-)
        {{-hs, -hs, -hs}, {0, -1, 0}, {1, 0, 0, 1}, {0, 0}},
        {{hs, -hs, -hs}, {0, -1, 0}, {1, 0, 0, 1}, {1, 0}},
        {{hs, -hs, hs}, {0, -1, 0}, {1, 0, 0, 1}, {1, 1}},
        {{-hs, -hs, hs}, {0, -1, 0}, {1, 0, 0, 1}, {0, 1}},
    };
    
    mesh.Vertices.assign(vertices, vertices + 24);
    
    // Define indices (6 faces * 2 triangles * 3 vertices)
    uint32_t indices[] = {
        0, 1, 2,  2, 3, 0,      // Front
        4, 5, 6,  6, 7, 4,      // Back
        8, 9, 10, 10, 11, 8,    // Right
        12, 13, 14, 14, 15, 12, // Left
        16, 17, 18, 18, 19, 16, // Top
        20, 21, 22, 22, 23, 20  // Bottom
    };
    
    mesh.Indices.assign(indices, indices + 36);
    
    // Compute bounds
    mesh.BoundsMin = glm::vec3(-hs, -hs, -hs);
    mesh.BoundsMax = glm::vec3(hs, hs, hs);
    
    // Create default material
    MaterialAsset material;
    material.Name = "CubeMaterial";
    material.BaseColorFactor = glm::vec4(1.0f, 1.0f, 1.0f, 1.0f);
    material.BaseColorTexture = m_AssetRegistry->GetDefaultWhiteTexture();
    material.NormalTexture = m_AssetRegistry->GetDefaultNormalTexture();
    material.RoughnessMetalnessTexture = m_AssetRegistry->GetDefaultRoughnessMetalnessTexture();
    mesh.MaterialIndex = m_AssetRegistry->CreateMaterial(material);
    
    return m_AssetRegistry->CreateMesh(mesh);
}

SceneLoader::TreeHandles SceneLoader::CreateSimpleTree() {
    TreeHandles handles;
    
    // Create trunk (tall cylinder approximated as cube for now)
    MeshAsset trunk;
    trunk.Name = "TreeTrunk";
    
    float trunkWidth = 0.3f;
    float trunkHeight = 2.0f;
    float hw = trunkWidth * 0.5f;
    float hh = trunkHeight * 0.5f;
    
    // Simplified trunk as a rectangular prism
    Vertex trunkVerts[] = {
        // Front
        {{-hw, 0, hw}, {0, 0, 1}, {1, 0, 0, 1}, {0, 0}},
        {{hw, 0, hw}, {0, 0, 1}, {1, 0, 0, 1}, {1, 0}},
        {{hw, trunkHeight, hw}, {0, 0, 1}, {1, 0, 0, 1}, {1, 1}},
        {{-hw, trunkHeight, hw}, {0, 0, 1}, {1, 0, 0, 1}, {0, 1}},
        
        // Back
        {{hw, 0, -hw}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 0}},
        {{-hw, 0, -hw}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 0}},
        {{-hw, trunkHeight, -hw}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 1}},
        {{hw, trunkHeight, -hw}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 1}},
        
        // Right
        {{hw, 0, hw}, {1, 0, 0}, {0, 0, -1, 1}, {0, 0}},
        {{hw, 0, -hw}, {1, 0, 0}, {0, 0, -1, 1}, {1, 0}},
        {{hw, trunkHeight, -hw}, {1, 0, 0}, {0, 0, -1, 1}, {1, 1}},
        {{hw, trunkHeight, hw}, {1, 0, 0}, {0, 0, -1, 1}, {0, 1}},
        
        // Left
        {{-hw, 0, -hw}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 0}},
        {{-hw, 0, hw}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 0}},
        {{-hw, trunkHeight, hw}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 1}},
        {{-hw, trunkHeight, -hw}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 1}},
    };
    
    trunk.Vertices.assign(trunkVerts, trunkVerts + sizeof(trunkVerts) / sizeof(trunkVerts[0]));
    
    uint32_t trunkIndices[] = {
        0, 1, 2,  2, 3, 0,
        4, 5, 6,  6, 7, 4,
        8, 9, 10, 10, 11, 8,
        12, 13, 14, 14, 15, 12
    };
    
    trunk.Indices.assign(trunkIndices, trunkIndices + sizeof(trunkIndices) / sizeof(trunkIndices[0]));
    trunk.BoundsMin = glm::vec3(-hw, 0, -hw);
    trunk.BoundsMax = glm::vec3(hw, trunkHeight, hw);
    
    // Trunk material (brown)
    MaterialAsset trunkMaterial;
    trunkMaterial.Name = "TreeTrunkMaterial";
    trunkMaterial.BaseColorFactor = glm::vec4(0.4f, 0.25f, 0.1f, 1.0f); // Brown
    trunkMaterial.BaseColorTexture = m_AssetRegistry->GetDefaultWhiteTexture();
    trunkMaterial.NormalTexture = m_AssetRegistry->GetDefaultNormalTexture();
    trunkMaterial.RoughnessMetalnessTexture = m_AssetRegistry->GetDefaultRoughnessMetalnessTexture();
    trunkMaterial.RoughnessFactor = 0.9f;
    trunk.MaterialIndex = m_AssetRegistry->CreateMaterial(trunkMaterial);
    
    handles.Trunk = m_AssetRegistry->CreateMesh(trunk);
    
    // Create foliage (simple sphere/cube approximation)
    MeshAsset foliage;
    foliage.Name = "TreeFoliage";
    
    float foliageSize = 1.5f;
    float fs = foliageSize * 0.5f;
    
    // Simple cube for foliage
    Vertex foliageVerts[] = {
        {{-fs, -fs, fs}, {0, 0, 1}, {1, 0, 0, 1}, {0, 0}},
        {{fs, -fs, fs}, {0, 0, 1}, {1, 0, 0, 1}, {1, 0}},
        {{fs, fs, fs}, {0, 0, 1}, {1, 0, 0, 1}, {1, 1}},
        {{-fs, fs, fs}, {0, 0, 1}, {1, 0, 0, 1}, {0, 1}},
        
        {{fs, -fs, -fs}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 0}},
        {{-fs, -fs, -fs}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 0}},
        {{-fs, fs, -fs}, {0, 0, -1}, {-1, 0, 0, 1}, {1, 1}},
        {{fs, fs, -fs}, {0, 0, -1}, {-1, 0, 0, 1}, {0, 1}},
        
        {{fs, -fs, fs}, {1, 0, 0}, {0, 0, -1, 1}, {0, 0}},
        {{fs, -fs, -fs}, {1, 0, 0}, {0, 0, -1, 1}, {1, 0}},
        {{fs, fs, -fs}, {1, 0, 0}, {0, 0, -1, 1}, {1, 1}},
        {{fs, fs, fs}, {1, 0, 0}, {0, 0, -1, 1}, {0, 1}},
        
        {{-fs, -fs, -fs}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 0}},
        {{-fs, -fs, fs}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 0}},
        {{-fs, fs, fs}, {-1, 0, 0}, {0, 0, 1, 1}, {1, 1}},
        {{-fs, fs, -fs}, {-1, 0, 0}, {0, 0, 1, 1}, {0, 1}},
        
        {{-fs, fs, fs}, {0, 1, 0}, {1, 0, 0, 1}, {0, 0}},
        {{fs, fs, fs}, {0, 1, 0}, {1, 0, 0, 1}, {1, 0}},
        {{fs, fs, -fs}, {0, 1, 0}, {1, 0, 0, 1}, {1, 1}},
        {{-fs, fs, -fs}, {0, 1, 0}, {1, 0, 0, 1}, {0, 1}},
        
        {{-fs, -fs, -fs}, {0, -1, 0}, {1, 0, 0, 1}, {0, 0}},
        {{fs, -fs, -fs}, {0, -1, 0}, {1, 0, 0, 1}, {1, 0}},
        {{fs, -fs, fs}, {0, -1, 0}, {1, 0, 0, 1}, {1, 1}},
        {{-fs, -fs, fs}, {0, -1, 0}, {1, 0, 0, 1}, {0, 1}},
    };
    
    foliage.Vertices.assign(foliageVerts, foliageVerts + sizeof(foliageVerts) / sizeof(foliageVerts[0]));
    
    uint32_t foliageIndices[] = {
        0, 1, 2,  2, 3, 0,
        4, 5, 6,  6, 7, 4,
        8, 9, 10, 10, 11, 8,
        12, 13, 14, 14, 15, 12,
        16, 17, 18, 18, 19, 16,
        20, 21, 22, 22, 23, 20
    };
    
    foliage.Indices.assign(foliageIndices, foliageIndices + sizeof(foliageIndices) / sizeof(foliageIndices[0]));
    foliage.BoundsMin = glm::vec3(-fs, -fs, -fs);
    foliage.BoundsMax = glm::vec3(fs, fs, fs);
    
    // Foliage material (green)
    MaterialAsset foliageMaterial;
    foliageMaterial.Name = "TreeFoliageMaterial";
    foliageMaterial.BaseColorFactor = glm::vec4(0.2f, 0.6f, 0.2f, 1.0f); // Green
    foliageMaterial.BaseColorTexture = m_AssetRegistry->GetDefaultWhiteTexture();
    foliageMaterial.NormalTexture = m_AssetRegistry->GetDefaultNormalTexture();
    foliageMaterial.RoughnessMetalnessTexture = m_AssetRegistry->GetDefaultRoughnessMetalnessTexture();
    foliageMaterial.RoughnessFactor = 0.7f;
    foliage.MaterialIndex = m_AssetRegistry->CreateMaterial(foliageMaterial);
    
    handles.Foliage = m_AssetRegistry->CreateMesh(foliage);
    
    std::cout << "Created simple tree (trunk + foliage)" << std::endl;
    
    return handles;
}

void SceneLoader::LoadDemoScene() {
    std::cout << "Loading demo scene..." << std::endl;
    
    // Create ground plane
    MeshHandle groundMesh = CreateGroundPlane(50.0f, 20);
    auto groundEntity = m_ECS->CreateEntity();
    auto& groundTransform = m_ECS->AddComponent<Transform>(groundEntity);
    groundTransform.Position = glm::vec3(0.0f, 0.0f, 0.0f);
    m_ECS->AddComponent<Renderable>(groundEntity);
    auto& groundMeshComp = m_ECS->AddComponent<Mesh>(groundEntity);
    groundMeshComp.MeshIndex = groundMesh.Index;
    auto& groundBounds = m_ECS->AddComponent<BoundingBox>(groundEntity);
    groundBounds.Min = glm::vec3(-25.0f, 0.0f, -25.0f);
    groundBounds.Max = glm::vec3(25.0f, 0.0f, 25.0f);
    
    std::cout << "Created ground plane" << std::endl;
    
    // Create trees
    auto treeHandles = CreateSimpleTree();
    
    // Instance multiple trees at random positions
    std::vector<glm::vec3> treePositions = {
        {-10.0f, 0.0f, -5.0f},
        {-5.0f, 0.0f, -10.0f},
        {5.0f, 0.0f, -8.0f},
        {10.0f, 0.0f, -3.0f},
        {-8.0f, 0.0f, 5.0f},
        {3.0f, 0.0f, 8.0f},
        {12.0f, 0.0f, 10.0f},
        {-12.0f, 0.0f, 12.0f}
    };
    
    for (size_t i = 0; i < treePositions.size(); i++) {
        // Create trunk entity
        auto trunkEntity = m_ECS->CreateEntity();
        auto& trunkTransform = m_ECS->AddComponent<Transform>(trunkEntity);
        trunkTransform.Position = treePositions[i];
        trunkTransform.Rotation = glm::vec3(0.0f, glm::linearRand(0.0f, glm::two_pi<float>()), 0.0f);
        trunkTransform.Scale = glm::vec3(glm::linearRand(0.8f, 1.2f));
        m_ECS->AddComponent<Renderable>(trunkEntity);
        auto& trunkMeshComp = m_ECS->AddComponent<Mesh>(trunkEntity);
        trunkMeshComp.MeshIndex = treeHandles.Trunk.Index;
        m_ECS->AddComponent<BoundingBox>(trunkEntity);
        
        // Create foliage entity (offset above trunk)
        auto foliageEntity = m_ECS->CreateEntity();
        auto& foliageTransform = m_ECS->AddComponent<Transform>(foliageEntity);
        foliageTransform.Position = treePositions[i] + glm::vec3(0.0f, 2.0f, 0.0f);
        foliageTransform.Rotation = trunkTransform.Rotation;
        foliageTransform.Scale = trunkTransform.Scale;
        m_ECS->AddComponent<Renderable>(foliageEntity);
        auto& foliageMeshComp = m_ECS->AddComponent<Mesh>(foliageEntity);
        foliageMeshComp.MeshIndex = treeHandles.Foliage.Index;
        m_ECS->AddComponent<BoundingBox>(foliageEntity);
    }
    
    std::cout << "Created " << treePositions.size() << " trees" << std::endl;
    std::cout << "Demo scene loaded successfully!" << std::endl;
}

} // namespace Henky3D
