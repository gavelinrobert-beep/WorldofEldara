namespace WorldofEldara.Shared.Data.Character;

/// <summary>
///     Major factions in Eldara. Each represents a philosophical approach to the Worldroot crisis.
/// </summary>
public enum Faction
{
    /// <summary>
    ///     The Verdant Circles - Sylvaen Traditionalists
    ///     Capital: Thornveil Enclave (Elar'Thalas sealed)
    ///     Philosophy: Preserve the Worldroot, maintain natural order
    /// </summary>
    VerdantCircles = 1,

    /// <summary>
    ///     The Ascendant League - High Elf Exiles
    ///     Capital: Spire of Echoes
    ///     Philosophy: Master reality through knowledge; death is optional
    /// </summary>
    AscendantLeague = 2,

    /// <summary>
    ///     The United Kingdoms - Human Realms
    ///     Capital: Various (alliance of city-states)
    ///     Philosophy: Survival through adaptation and cooperation
    /// </summary>
    UnitedKingdoms = 3,

    /// <summary>
    ///     The Totem Clans (Wildborn) - Instinct-First Therakai
    ///     Capital: Vharos (contested, Wildborn half)
    ///     Philosophy: Embrace pure instinct, memory enslaves
    /// </summary>
    TotemClansWildborn = 4,

    /// <summary>
    ///     The Totem Clans (Pathbound) - Choice-First Therakai
    ///     Capital: Vharos (contested, Pathbound half)
    ///     Philosophy: Forge own path, instinct without memory repeats extinction
    /// </summary>
    TotemClansPathbound = 5,

    /// <summary>
    ///     The Dominion Warhost - Gronnak Empire
    ///     Capital: Krag'Thuun
    ///     Philosophy: Devour gods, claim dominion through strength
    /// </summary>
    DominionWarhost = 6,

    /// <summary>
    ///     The Void Compact - Outcasts and Seekers
    ///     Capital: Blackwake Haven
    ///     Philosophy: Understand and survive the Void
    /// </summary>
    VoidCompact = 7,

    /// <summary>
    ///     Neutral / Independent
    /// </summary>
    Neutral = 0
}

/// <summary>
///     Faction relationships and hostility
/// </summary>
public enum FactionStanding
{
    Hated = -3,
    Hostile = -2,
    Unfriendly = -1,
    Neutral = 0,
    Friendly = 1,
    Honored = 2,
    Exalted = 3
}

/// <summary>
///     Faction information and relationships
/// </summary>
public static class FactionInfo
{
    public static string GetCapital(Faction faction)
    {
        return faction switch
        {
            Faction.VerdantCircles => "Thornveil Enclave",
            Faction.AscendantLeague => "Spire of Echoes",
            Faction.UnitedKingdoms => "Council of Crowns (mobile)",
            Faction.TotemClansWildborn => "Vharos (Wildborn District)",
            Faction.TotemClansPathbound => "Vharos (Pathbound District)",
            Faction.DominionWarhost => "Krag'Thuun",
            Faction.VoidCompact => "Blackwake Haven",
            _ => "Unknown"
        };
    }

    public static string GetPhilosophy(Faction faction)
    {
        return faction switch
        {
            Faction.VerdantCircles => "Preserve the Worldroot, maintain natural order",
            Faction.AscendantLeague => "Master reality; death is optional",
            Faction.UnitedKingdoms => "Survival through adaptation",
            Faction.TotemClansWildborn => "Embrace pure instinct",
            Faction.TotemClansPathbound => "Balance instinct with choice",
            Faction.DominionWarhost => "Devour gods, rule through dominion",
            Faction.VoidCompact => "Understand and exploit the Void",
            _ => "Unknown"
        };
    }

    /// <summary>
    ///     Get default faction standing between two factions
    /// </summary>
    public static FactionStanding GetDefaultStanding(Faction from, Faction to)
    {
        if (from == to) return FactionStanding.Exalted;
        if (from == Faction.Neutral || to == Faction.Neutral) return FactionStanding.Neutral;

        // Verdant Circles relationships
        if (from == Faction.VerdantCircles)
            return to switch
            {
                Faction.AscendantLeague => FactionStanding.Hostile, // High Elves broke the Worldroot
                Faction.TotemClansPathbound => FactionStanding.Friendly, // Respect balance
                Faction.DominionWarhost => FactionStanding.Unfriendly, // God-eating disrupts order
                Faction.VoidCompact => FactionStanding.Unfriendly, // Void is anti-life
                _ => FactionStanding.Neutral
            };

        // Ascendant League relationships
        if (from == Faction.AscendantLeague)
            return to switch
            {
                Faction.VerdantCircles => FactionStanding.Hostile, // Sylvaen exiled them
                Faction.UnitedKingdoms => FactionStanding.Friendly, // Humans are curious
                Faction.VoidCompact => FactionStanding.Neutral, // Mutual research interests
                _ => FactionStanding.Neutral
            };

        // United Kingdoms relationships
        if (from == Faction.UnitedKingdoms)
            return to switch
            {
                Faction.DominionWarhost => FactionStanding.Unfriendly, // Threat to humanity
                _ => FactionStanding.Neutral // Pragmatic with everyone
            };

        // Wildborn vs Pathbound (civil war)
        if (from == Faction.TotemClansWildborn && to == Faction.TotemClansPathbound)
            return FactionStanding.Hostile;
        if (from == Faction.TotemClansPathbound && to == Faction.TotemClansWildborn)
            return FactionStanding.Hostile;

        // Dominion Warhost relationships
        if (from == Faction.DominionWarhost)
            return to switch
            {
                Faction.VerdantCircles => FactionStanding.Unfriendly,
                Faction.AscendantLeague => FactionStanding.Unfriendly,
                Faction.VoidCompact => FactionStanding.Neutral, // Both study dangerous powers
                _ => FactionStanding.Hostile // Aggressive to most
            };

        // Void Compact relationships
        if (from == Faction.VoidCompact) return FactionStanding.Neutral; // Outcasts maintain neutrality

        return FactionStanding.Neutral;
    }

    /// <summary>
    ///     Get starter zone for faction
    /// </summary>
    public static string GetStarterZone(Faction faction)
    {
        return faction switch
        {
            Faction.VerdantCircles => "Thornveil Enclave",
            Faction.AscendantLeague => "Temporal Steppes",
            Faction.UnitedKingdoms => "Borderkeep",
            Faction.TotemClansWildborn => "The Untamed Reaches",
            Faction.TotemClansPathbound => "The Carved Valleys",
            Faction.DominionWarhost => "The Scarred Highlands",
            Faction.VoidCompact => "Blackwake Haven",
            _ => "Unknown"
        };
    }

    /// <summary>
    ///     Can this race join this faction?
    /// </summary>
    public static bool IsRaceAvailableForFaction(Race race, Faction faction)
    {
        return faction switch
        {
            Faction.VerdantCircles => race == Race.Sylvaen,
            Faction.AscendantLeague => race == Race.HighElf,
            Faction.UnitedKingdoms => race == Race.Human,
            Faction.TotemClansWildborn => race == Race.Therakai,
            Faction.TotemClansPathbound => race == Race.Therakai,
            Faction.DominionWarhost => race == Race.Gronnak,
            Faction.VoidCompact => true, // All races can join (outcasts)
            Faction.Neutral => true,
            _ => false
        };
    }
}