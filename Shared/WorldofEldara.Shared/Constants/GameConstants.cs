namespace WorldofEldara.Shared.Constants;

/// <summary>
///     Core game constants grounded in Eldara lore
/// </summary>
public static class GameConstants
{
    // Level progression
    public const int MaxLevel = 60;
    public const int StartingLevel = 1;

    // Experience constants
    public const long BaseExperiencePerLevel = 1000;
    public const float ExperienceScalingFactor = 1.15f; // Each level requires 15% more XP

    // Stats
    public const int BaseStatValue = 10;
    public const int StatPointsPerLevel = 5;

    // Health and Mana
    public const int BaseHealthPerStamina = 10;
    public const int BaseManaPerIntellect = 10;

    // Combat
    public const float DefaultAttackRange = 5.0f; // meters, melee range
    public const float DefaultSpellRange = 30.0f; // meters
    public const float DefaultCastTime = 1.5f; // seconds
    public const float GlobalCooldown = 1.5f; // seconds

    // Movement
    public const float BaseWalkSpeed = 4.0f; // m/s
    public const float BaseRunSpeed = 7.0f; // m/s
    public const float BaseSwimSpeed = 3.0f; // m/s
    public const float BaseFlySpeed = 10.0f; // m/s
    public const float SprintMultiplier = 1.4f;

    // World
    public const float WorldTickRate = 20.0f; // Server updates per second
    public const float WorldTickDelta = 1.0f / WorldTickRate;

    // Character creation
    public const int MinNameLength = 3;
    public const int MaxNameLength = 16;
    public const string NameAllowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ'-";

    // Inventory
    public const int DefaultInventorySlots = 48;
    public const int DefaultBankSlots = 96;

    // Groups and raids
    public const int MaxPartySize = 5;
    public const int MaxRaidSize = 40;
    public const int MaxGuildSize = 500;

    // Lore-specific constants
    public const string WorldrootConnectionCheck = "PlayerNearWorldroot";
    public const float WorldrootDensityHigh = 1.0f; // Thornveil, Sylvaen territories
    public const float WorldrootDensityMedium = 0.5f; // Most zones
    public const float WorldrootDensityLow = 0.2f; // Human kingdoms
    public const float WorldrootDensityNone = 0.0f; // Krag'Thuun, Blackwake Haven
}

/// <summary>
///     Lore-based world zones
/// </summary>
public static class ZoneConstants
{
    // Vertical slice starter zone
    public const string Zone01 = "zone_01";

    // Starter Zones (Level 1-10)
    public const string ThornveilEnclave = "zone_thornveil_enclave";
    public const string TemporalSteppes = "zone_temporal_steppes";
    public const string Borderkeep = "zone_borderkeep";
    public const string UntamedReaches = "zone_untamed_reaches";
    public const string CarvedValleys = "zone_carved_valleys";
    public const string ScarredHighlands = "zone_scarred_highlands";
    public const string BlackwakeHaven = "zone_blackwake_haven";
    public const string BriarwatchCrossing = "zone_briarwatch_crossing";

    // Mid-level Zones (Level 10-40)
    public const string ElarThalasApproach = "zone_elarthalas_approach";
    public const string VharosWarfront = "zone_vharos_warfront";
    public const string MemoryWastes = "zone_memory_wastes";
    public const string KragThuunDepths = "zone_kragthuun_depths";

    // High-level Zones (Level 40-60)
    public const string IsleOfGiants = "zone_isle_of_giants";
    public const string BeneathTheLeviathan = "zone_beneath_leviathan";
    public const string TemporalMaze = "zone_temporal_maze";

    // Endgame Zones (Level 60+)
    public const string RootCore = "zone_root_core";
    public const string NullThreshold = "zone_null_threshold";
}
