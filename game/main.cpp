/**
 * World of Eldara - Game Bootstrap using Henky3D Engine
 * 
 * This is the minimal entry point for the World of Eldara game.
 * It initializes the Henky3D rendering engine and displays a simple scene.
 */

#include "engine/core/Window.h"
#include "engine/core/Timer.h"
#include "engine/input/Input.h"
#include "engine/graphics/GraphicsDevice.h"
#include "engine/graphics/Renderer.h"
#include "engine/graphics/ConstantBuffers.h"
#include "engine/graphics/ShadowMap.h"
#include "engine/ecs/ECSWorld.h"
#include "engine/ecs/Components.h"
#include "engine/ecs/TransformSystem.h"
#include "engine/ecs/CullingSystem.h"
#include <imgui.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <memory>
#include <iostream>

using namespace Henky3D;

class WorldofEldaraGame {
public:
    WorldofEldaraGame() {
        std::cout << "==========================================" << std::endl;
        std::cout << "  World of Eldara - Henky3D Edition" << std::endl;
        std::cout << "  The Worldroot is bleeding. Memory awaits." << std::endl;
        std::cout << "==========================================" << std::endl;
        
        // Initialize window and graphics
        m_Window = std::make_unique<Window>("World of Eldara", 1280, 720);
        m_Device = std::make_unique<GraphicsDevice>(m_Window.get());
        m_Renderer = std::make_unique<Renderer>(m_Device.get());
        m_ECS = std::make_unique<ECSWorld>();
        
        InitializeImGui();
        InitializeScene();
        
        Input::Initialize(m_Window->GetHandle());
        
        m_Window->SetEventCallback([this]() {
            OnResize();
        });
        
        std::cout << "[INFO] Engine initialized successfully" << std::endl;
        std::cout << "[INFO] Resolution: " << m_Window->GetWidth() << "x" << m_Window->GetHeight() << std::endl;
    }

    ~WorldofEldaraGame() {
        m_Device->WaitForGPU();
        ImGui_ImplOpenGL3_Shutdown();
        ImGui_ImplGlfw_Shutdown();
        ImGui::DestroyContext();
    }

    void Run() {
        Timer timer;
        FPSCounter fpsCounter;
        m_TotalTime = 0.0f;

        std::cout << "[INFO] Game loop started" << std::endl;

        while (m_Running) {
            if (!m_Window->ProcessMessages()) {
                m_Running = false;
                break;
            }

            m_DeltaTime = timer.GetDeltaTime();
            fpsCounter.Update(m_DeltaTime);
            m_TotalTime += m_DeltaTime;
            
            Input::Update();
            Update(m_DeltaTime);
            Render(fpsCounter);
        }
        
        std::cout << "[INFO] Game loop ended" << std::endl;
    }

private:
    void InitializeImGui() {
        IMGUI_CHECKVERSION();
        ImGui::CreateContext();
        ImGuiIO& io = ImGui::GetIO();
        io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;

        ImGui::StyleColorsDark();

        ImGui_ImplGlfw_InitForOpenGL(m_Window->GetHandle(), true);
        ImGui_ImplOpenGL3_Init("#version 460 core");
    }

    void InitializeScene() {
        // Create camera entity
        auto cameraEntity = m_ECS->CreateEntity();
        auto& camera = m_ECS->AddComponent<Camera>(cameraEntity);
        camera.AspectRatio = static_cast<float>(m_Window->GetWidth()) / static_cast<float>(m_Window->GetHeight());
        camera.Position = { 0.0f, 2.0f, 5.0f };
        m_CameraEntity = cameraEntity;

        // Create light entity - representing Eldara's sun
        auto lightEntity = m_ECS->CreateEntity();
        auto& light = m_ECS->AddComponent<Light>(lightEntity);
        light.LightType = Light::Type::Directional;
        light.Direction = { -0.3f, -1.0f, 0.5f };
        light.Color = { 1.0f, 0.95f, 0.9f, 1.0f };  // Warm sunlight

        // Create a placeholder cube representing the Worldroot
        auto cubeEntity = m_ECS->CreateEntity();
        auto& cubeTransform = m_ECS->AddComponent<Transform>(cubeEntity);
        cubeTransform.Position = { 0.0f, 0.0f, 0.0f };
        cubeTransform.Scale = { 1.0f, 1.0f, 1.0f };
        m_ECS->AddComponent<Renderable>(cubeEntity);
        m_ECS->AddComponent<BoundingBox>(cubeEntity);
        m_CubeEntity = cubeEntity;

        std::cout << "[INFO] Scene initialized with placeholder cube (representing the Worldroot)" << std::endl;
    }

    void Update(float deltaTime) {
        m_ECS->Update(deltaTime);
        
        // Update transform system
        TransformSystem::UpdateTransforms(m_ECS.get());
        
        // Rotate the cube slowly
        if (m_ECS->HasComponent<Transform>(m_CubeEntity)) {
            auto& transform = m_ECS->GetComponent<Transform>(m_CubeEntity);
            transform.Rotation.y += deltaTime * 0.5f;  // Rotate around Y axis
            transform.MarkDirty();
        }

        // Update camera controls if enabled
        if (m_EnableCameraControl) {
            UpdateCamera(deltaTime);
        }
    }

    void UpdateCamera(float deltaTime) {
        if (!m_ECS->HasComponent<Camera>(m_CameraEntity)) return;
        
        auto& camera = m_ECS->GetComponent<Camera>(m_CameraEntity);
        
        // Calculate camera forward, right, and up vectors
        glm::vec3 forward;
        forward.x = cos(glm::radians(camera.Yaw)) * cos(glm::radians(camera.Pitch));
        forward.y = sin(glm::radians(camera.Pitch));
        forward.z = sin(glm::radians(camera.Yaw)) * cos(glm::radians(camera.Pitch));
        forward = glm::normalize(forward);
        
        glm::vec3 right = glm::normalize(glm::cross(forward, glm::vec3(0.0f, 1.0f, 0.0f)));
        glm::vec3 up = glm::normalize(glm::cross(right, forward));
        
        // WASD movement
        float speed = m_CameraMoveSpeed * deltaTime;
        if (Input::IsKeyPressed(GLFW_KEY_W)) camera.Position += forward * speed;
        if (Input::IsKeyPressed(GLFW_KEY_S)) camera.Position -= forward * speed;
        if (Input::IsKeyPressed(GLFW_KEY_A)) camera.Position -= right * speed;
        if (Input::IsKeyPressed(GLFW_KEY_D)) camera.Position += right * speed;
        if (Input::IsKeyPressed(GLFW_KEY_Q)) camera.Position -= up * speed;
        if (Input::IsKeyPressed(GLFW_KEY_E)) camera.Position += up * speed;
        
        // Mouse look
        float mouseDeltaX = Input::GetMouseDeltaX();
        float mouseDeltaY = Input::GetMouseDeltaY();
        
        camera.Yaw += mouseDeltaX * m_CameraLookSpeed;
        camera.Pitch -= mouseDeltaY * m_CameraLookSpeed;
        
        // Clamp pitch to prevent flipping
        camera.Pitch = glm::clamp(camera.Pitch, -89.0f, 89.0f);
    }

    void Render(const FPSCounter& fpsCounter) {
        m_Renderer->BeginFrame();
        
        // Clear
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        
        // Set viewport
        glViewport(0, 0, m_Window->GetWidth(), m_Window->GetHeight());

        // Setup per-frame constants
        if (m_ECS->HasComponent<Camera>(m_CameraEntity)) {
            auto& camera = m_ECS->GetComponent<Camera>(m_CameraEntity);
            camera.AspectRatio = static_cast<float>(m_Window->GetWidth()) / static_cast<float>(m_Window->GetHeight());

            // Get directional light from scene
            glm::vec3 lightDirection = glm::vec3(0.5f, -1.0f, 0.3f);
            glm::vec4 lightColor = glm::vec4(1.0f, 1.0f, 0.9f, 1.0f);
            auto lightView = m_ECS->GetRegistry().view<Light>();
            for (auto entity : lightView) {
                auto& light = lightView.get<Light>(entity);
                if (light.LightType == Light::Type::Directional) {
                    lightDirection = light.Direction;
                    lightColor = light.Color;
                    break;
                }
            }
            
            // Compute light view-projection for shadows
            glm::vec3 sceneBoundsMin = glm::vec3(-5.0f, -5.0f, -5.0f);
            glm::vec3 sceneBoundsMax = glm::vec3(5.0f, 5.0f, 5.0f);
            glm::mat4 lightViewProj = ShadowMap::ComputeLightViewProjection(
                lightDirection, sceneBoundsMin, sceneBoundsMax);

            PerFrameConstants perFrameConstants;
            glm::mat4 view = camera.GetViewMatrix();
            glm::mat4 projection = camera.GetProjectionMatrix();
            
            perFrameConstants.ViewMatrix = view;
            perFrameConstants.ProjectionMatrix = projection;
            perFrameConstants.ViewProjectionMatrix = projection * view;
            perFrameConstants.LightViewProjectionMatrix = lightViewProj;
            perFrameConstants.CameraPosition = glm::vec4(camera.Position, 1.0f);
            perFrameConstants.LightDirection = glm::vec4(lightDirection, 0.0f);
            perFrameConstants.LightColor = lightColor;
            perFrameConstants.AmbientColor = glm::vec4(0.2f, 0.2f, 0.25f, 1.0f);
            perFrameConstants.Time = m_TotalTime;
            perFrameConstants.DeltaTime = m_DeltaTime;
            perFrameConstants.ShadowBias = m_ShadowBias;
            perFrameConstants.ShadowsEnabled = m_ShadowsEnabled ? 1.0f : 0.0f;

            m_Renderer->SetPerFrameConstants(perFrameConstants);

            // Render shadow pass if enabled
            if (m_ShadowsEnabled) {
                m_Renderer->RenderShadowPass(m_ECS.get());
                
                // Reset viewport after shadow pass
                glViewport(0, 0, m_Window->GetWidth(), m_Window->GetHeight());
            }

            // Render scene
            m_Renderer->RenderScene(m_ECS.get(), m_DepthPrepassEnabled, m_ShadowsEnabled);
        }
        
        // Render UI
        RenderUI(fpsCounter);
        
        m_Device->EndFrame();
    }

    void RenderUI(const FPSCounter& fpsCounter) {
        ImGui_ImplOpenGL3_NewFrame();
        ImGui_ImplGlfw_NewFrame();
        ImGui::NewFrame();

        // Main info panel
        ImGui::Begin("World of Eldara - Debug Info");
        
        ImGui::Text("Application: %.1f FPS (%.2f ms)", 
                    fpsCounter.GetFPS(), 
                    1000.0f / (fpsCounter.GetFPS() > 0 ? fpsCounter.GetFPS() : 1.0f));
        
        ImGui::Separator();
        ImGui::Text("Scene Stats:");
        const auto& stats = m_Renderer->GetStats();
        ImGui::Text("  Draw Calls: %d", stats.DrawCount);
        ImGui::Text("  Triangles: %d", stats.TriangleCount);
        ImGui::Text("  Culled: %d", stats.CulledCount);
        
        ImGui::Separator();
        ImGui::Checkbox("Enable Camera Control", &m_EnableCameraControl);
        if (m_EnableCameraControl) {
            ImGui::SliderFloat("Move Speed", &m_CameraMoveSpeed, 0.1f, 20.0f);
            ImGui::SliderFloat("Look Speed", &m_CameraLookSpeed, 0.01f, 0.5f);
        }
        
        ImGui::Separator();
        ImGui::Checkbox("Depth Prepass", &m_DepthPrepassEnabled);
        ImGui::Checkbox("Shadows", &m_ShadowsEnabled);
        if (m_ShadowsEnabled) {
            ImGui::SliderFloat("Shadow Bias", &m_ShadowBias, 0.0f, 0.01f, "%.5f");
        }
        
        ImGui::Separator();
        ImGui::TextWrapped("Welcome to World of Eldara!");
        ImGui::TextWrapped("This is a minimal bootstrap using the Henky3D engine.");
        ImGui::TextWrapped("The cube represents the Worldroot - the core of Eldara.");
        
        ImGui::End();

        ImGui::Render();
        ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
    }

    void OnResize() {
        if (m_ECS->HasComponent<Camera>(m_CameraEntity)) {
            auto& camera = m_ECS->GetComponent<Camera>(m_CameraEntity);
            camera.AspectRatio = static_cast<float>(m_Window->GetWidth()) / static_cast<float>(m_Window->GetHeight());
        }
    }

    std::unique_ptr<Window> m_Window;
    std::unique_ptr<GraphicsDevice> m_Device;
    std::unique_ptr<Renderer> m_Renderer;
    std::unique_ptr<ECSWorld> m_ECS;
    
    entt::entity m_CameraEntity;
    entt::entity m_CubeEntity;
    
    bool m_Running = true;
    float m_DeltaTime = 0.0f;
    float m_TotalTime = 0.0f;
    
    // Camera controls
    bool m_EnableCameraControl = false;
    float m_CameraMoveSpeed = 5.0f;
    float m_CameraLookSpeed = 0.1f;
    
    // Rendering options
    bool m_DepthPrepassEnabled = true;
    bool m_ShadowsEnabled = true;
    float m_ShadowBias = 0.0005f;
};

int main() {
    try {
        WorldofEldaraGame game;
        game.Run();
        return 0;
    }
    catch (const std::exception& e) {
        std::cerr << "[ERROR] Exception: " << e.what() << std::endl;
        return 1;
    }
}
