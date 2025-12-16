namespace WorldofEldara.Shared.Data.Character;

/// <summary>
/// Playable classes in Eldara. Each class has lore justification tied to cosmic structure.
/// Classes represent philosophical stances on Eldara's crisis.
/// </summary>
public enum Class
{
    /// <summary>
    /// Memory Warden (Sylvaen Tank/Support)
    /// Defenders of Green Memory who anchor to Worldroot. Immovable protectors.
    /// Philosophy: Preserve what remains
    /// </summary>
    MemoryWarden = 1,

    /// <summary>
    /// Temporal Mage (High Elf DPS/Utility)
    /// Masters of Edit Nodes who fracture timelines in combat.
    /// Philosophy: Transcend current limits
    /// </summary>
    TemporalMage = 2,

    /// <summary>
    /// Unbound Warrior (Human Tank/DPS Hybrid)
    /// Exploit "unrecorded" nature to resist fate itself.
    /// Philosophy: Adapt and survive
    /// </summary>
    UnboundWarrior = 3,

    /// <summary>
    /// Totem Shaman (Therakai Healer/DPS)
    /// Pathbound Therakai who balance totem spirits with conscious choice.
    /// Philosophy: Reclaim primal truth with wisdom
    /// </summary>
    TotemShaman = 4,

    /// <summary>
    /// Berserker (Therakai DPS)
    /// Wildborn who fully surrender to instinct in combat.
    /// Philosophy: Reclaim pure instinct
    /// </summary>
    Berserker = 5,

    /// <summary>
    /// Witch-King Acolyte (Gronnak DPS/Support)
    /// Have consumed minor god-fragments, channel Dominion.
    /// Philosophy: Dominate through power
    /// </summary>
    WitchKingAcolyte = 6,

    /// <summary>
    /// Void Stalker (Void-Touched DPS/Stealth)
    /// Phase-shifted assassins who exist partially outside reality.
    /// Philosophy: Escape the system
    /// </summary>
    VoidStalker = 7,

    /// <summary>
    /// Root Druid (Sylvaen/Therakai Healer/DPS)
    /// Channel Worldroot growth and decay in equal measure.
    /// Philosophy: Preserve through balance
    /// </summary>
    RootDruid = 8,

    /// <summary>
    /// Archon Knight (High Elf/Human Tank)
    /// Wear armor phased across multiple timelines.
    /// Philosophy: Transcend through mastery
    /// </summary>
    ArchonKnight = 9,

    /// <summary>
    /// Free Blade (Human/Void-Touched DPS/Utility)
    /// Reject all factions, fight for coin or cause.
    /// Philosophy: Adapt through independence
    /// </summary>
    FreeBlade = 10,

    /// <summary>
    /// God-Seeker Cleric (Multi-Racial Healer)
    /// Worship fragment-gods, channel fading divine power.
    /// Philosophy: Serve divine order
    /// </summary>
    GodSeekerCleric = 11,

    /// <summary>
    /// Memory Thief (Multi-Racial DPS/Utility)
    /// Extract and implant memories (illegal everywhere).
    /// Philosophy: Exploit the crisis
    /// </summary>
    MemoryThief = 12
}

/// <summary>
/// Class roles for group composition
/// </summary>
public enum ClassRole
{
    Tank,
    Healer,
    DPS,
    Support,
    Hybrid
}

/// <summary>
/// Magic sources that power class abilities
/// </summary>
public enum MagicSource
{
    None,           // Pure martial
    Worldroot,      // Nature/Growth magic
    Divine,         // Fragment-god worship
    Arcane,         // Reality manipulation
    Primal,         // Totem/Instinct
    Dominion,       // Consumed divinity
    Void,           // Entropy magic
    Memory          // Forbidden memory weaving
}

/// <summary>
/// Class information and restrictions
/// </summary>
public static class ClassInfo
{
    public static ClassRole GetPrimaryRole(Class classType) => classType switch
    {
        Class.MemoryWarden => ClassRole.Tank,
        Class.TemporalMage => ClassRole.DPS,
        Class.UnboundWarrior => ClassRole.Hybrid,
        Class.TotemShaman => ClassRole.Healer,
        Class.Berserker => ClassRole.DPS,
        Class.WitchKingAcolyte => ClassRole.DPS,
        Class.VoidStalker => ClassRole.DPS,
        Class.RootDruid => ClassRole.Healer,
        Class.ArchonKnight => ClassRole.Tank,
        Class.FreeBlade => ClassRole.DPS,
        Class.GodSeekerCleric => ClassRole.Healer,
        Class.MemoryThief => ClassRole.Support,
        _ => ClassRole.DPS
    };

    public static MagicSource GetMagicSource(Class classType) => classType switch
    {
        Class.MemoryWarden => MagicSource.Worldroot,
        Class.TemporalMage => MagicSource.Arcane,
        Class.UnboundWarrior => MagicSource.None,
        Class.TotemShaman => MagicSource.Primal,
        Class.Berserker => MagicSource.Primal,
        Class.WitchKingAcolyte => MagicSource.Dominion,
        Class.VoidStalker => MagicSource.Void,
        Class.RootDruid => MagicSource.Worldroot,
        Class.ArchonKnight => MagicSource.Arcane,
        Class.FreeBlade => MagicSource.None,
        Class.GodSeekerCleric => MagicSource.Divine,
        Class.MemoryThief => MagicSource.Memory,
        _ => MagicSource.None
    };

    /// <summary>
    /// Check if a race can play a specific class (lore restrictions)
    /// </summary>
    public static bool IsClassAvailableForRace(Class classType, Race race)
    {
        return classType switch
        {
            // Race-locked classes
            Class.MemoryWarden => race == Race.Sylvaen,
            Class.TemporalMage => race == Race.HighElf,
            Class.UnboundWarrior => race == Race.Human,
            Class.TotemShaman => race == Race.Therakai,
            Class.Berserker => race == Race.Therakai,
            Class.WitchKingAcolyte => race == Race.Gronnak,
            Class.VoidStalker => race == Race.VoidTouched,

            // Multi-race classes
            Class.RootDruid => race == Race.Sylvaen || race == Race.Therakai,
            Class.ArchonKnight => race == Race.HighElf || race == Race.Human,
            Class.FreeBlade => race == Race.Human || race == Race.VoidTouched,
            Class.GodSeekerCleric => true, // Available to all races
            Class.MemoryThief => true, // Available to all races (illegal everywhere)

            _ => false
        };
    }

    public static string GetPhilosophy(Class classType) => classType switch
    {
        Class.MemoryWarden or Class.RootDruid => "Preserve what remains",
        Class.TemporalMage or Class.ArchonKnight => "Transcend current limits",
        Class.UnboundWarrior or Class.FreeBlade => "Adapt and survive",
        Class.TotemShaman or Class.Berserker => "Reclaim primal truth",
        Class.WitchKingAcolyte => "Dominate through power",
        Class.VoidStalker => "Escape the system",
        Class.GodSeekerCleric => "Serve divine order",
        Class.MemoryThief => "Exploit the crisis",
        _ => "Unknown"
    };
}
