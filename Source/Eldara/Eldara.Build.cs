using System.IO;
using UnrealBuildTool;

public class Eldara : ModuleRules
{
    public Eldara(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicIncludePaths.AddRange(new[]
        {
            ModuleDirectory,
            Path.Combine(ModuleDirectory, "AI"),
            Path.Combine(ModuleDirectory, "Core"),
            Path.Combine(ModuleDirectory, "Characters"),
            Path.Combine(ModuleDirectory, "Data")
        });

        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core",
            "CoreUObject",
            "Engine",
            "InputCore",
            "EnhancedInput",
            "AIModule",
            "GameplayTasks",
            "UMG"
        });
    }
}
