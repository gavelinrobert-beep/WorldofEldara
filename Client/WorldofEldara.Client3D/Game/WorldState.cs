using System.Numerics;
using System.Drawing;

namespace WorldofEldara.Client3D.Game;

public sealed class WorldState
{
    public Vector3 PlayerPosition { get; set; }
    public float PlayerYaw { get; set; }
    public bool PlayerIsMoving { get; set; }
    public float PlayerHealth { get; set; } = 190f;
    public float PlayerMaxHealth { get; set; } = 190f;
    public float PlayerHitFlash { get; set; }
    public bool PlayerDefeated { get; set; }
    public float RespawnTimer { get; set; }
    public float AggroGraceTimer { get; set; } = 2.5f;
    public string? TargetName { get; set; }
    public float PrimaryCooldown { get; set; }
    public float StrikeFlash { get; set; }
    public Vector3 StrikeStart { get; set; }
    public Vector3 StrikeEnd { get; set; }
    public string StatusText { get; set; } = "Primary ability ready.";
    public int Level { get; set; } = 1;
    public int Experience { get; set; }
    public int Gold { get; set; }
    public int FirstPruningKills { get; set; }
    public bool FirstPruningTurnedIn { get; set; }
    public Dictionary<string, Vector3> ActorPositions { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Vector3> ActorRuntimePositions { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Vector3> ActorSpawnPositions { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, float> ActorHealth { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, float> ActorHitFlash { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, float> ActorAttackCooldown { get; } = new(StringComparer.Ordinal);
    public HashSet<string> AggroActors { get; } = new(StringComparer.Ordinal);
    public List<CombatFloatText> CombatTexts { get; } = [];
}

public readonly record struct CombatFloatText(string Text, Vector3 Position, Color Color, float Age, float Lifetime);
