using UnrealBuildTool;
using System.Collections.Generic;

public class EldaraEditorTarget : TargetRules
{
    public EldaraEditorTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Editor;
        DefaultBuildSettings = BuildSettingsVersion.V6;
        IncludeOrderVersion = EngineIncludeOrderVersion.Latest;

        ExtraModuleNames.AddRange(new List<string> { "Eldara" });
    }
}
