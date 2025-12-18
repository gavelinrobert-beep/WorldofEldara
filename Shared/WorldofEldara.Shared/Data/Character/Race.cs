namespace WorldofEldara.Shared.Data.Character;

/// <summary>
///     Playable races in Eldara, each with distinct cosmic origins and lore justification.
///     Based on canonical lore: Architects, Worldroot, and the Null.
/// </summary>
public enum Race
{
    /// <summary>
    ///     Sylvaen (Wood Elves) - Grown from Worldroot seed-consciousness.
    ///     First people, immune system of Eldara, feel imbalance instinctively.
    /// </summary>
    Sylvaen = 1,

    /// <summary>
    ///     High Elves (Aelthar) - Sylvaen who rejected Worldroot authority.
    ///     Discovered Edit Nodes, broke death, now exiled from Elar'Thalas.
    /// </summary>
    HighElf = 2,

    /// <summary>
    ///     Humans - The Unrecorded Variable. Do not appear in early Worldroot layers.
    ///     Immune to full overwrite, resist god-compulsion. Origin unknown.
    /// </summary>
    Human = 3,

    /// <summary>
    ///     Therakai (Beastkin) - Descended from Vael's choice-consciousness.
    ///     Born from fragments when Vael shattered itself. Embody instinct vs choice.
    /// </summary>
    Therakai = 4,

    /// <summary>
    ///     Gronnak (Orcs) - Living atop reality scar where gods can die permanently.
    ///     God-Eater heritage, consume divine fragments, exposed to Null whispers.
    /// </summary>
    Gronnak = 5,

    /// <summary>
    ///     Void-Touched - Any race exposed to Void rifts and fundamentally changed.
    ///     Partially phased between states, see cracks in reality, unstable existence.
    /// </summary>
    VoidTouched = 6
}

/// <summary>
///     Racial bonuses and traits grounded in lore.
/// </summary>
public static class RacialTraits
{
    public static string GetRacialAbility(Race race)
    {
        return race switch
        {
            Race.Sylvaen => "Memory Echo - Sense recent history of a location",
            Race.HighElf => "Temporal Resistance - Reduced crowd control duration",
            Race.Human => "Adaptive - Gain bonuses from different environments",
            Race.Therakai => "Totem Rage - Temporary power boost tied to totem",
            Race.Gronnak => "God-Eater's Will - Resistance to divine magic",
            Race.VoidTouched => "Phase Shift - Brief intangibility",
            _ => "Unknown"
        };
    }

    public static string GetLoreOrigin(Race race)
    {
        return race switch
        {
            Race.Sylvaen => "Worldroot seed-consciousness",
            Race.HighElf => "Sylvaen who rejected finality",
            Race.Human => "Unknown - not in Worldroot records",
            Race.Therakai => "Vael's choice-fragments",
            Race.Gronnak => "Reality scar survivors",
            Race.VoidTouched => "Void rift exposure",
            _ => "Unknown"
        };
    }
}

/// <summary>
///     Totem spirits for Therakai characters.
///     Each represents a fragment of Vael's shattered consciousness.
/// </summary>
public enum TotemSpirit
{
    None = 0,

    /// <summary>
    ///     Wolf/Tiger - Predator, hunter, pack tactics
    /// </summary>
    Fanged = 1,

    /// <summary>
    ///     Bull/Ram - Strength, endurance, unstoppable force
    /// </summary>
    Horned = 2,

    /// <summary>
    ///     Bear/Badger - Ferocity, protection, defensive rage
    /// </summary>
    Clawed = 3,

    /// <summary>
    ///     Hawk/Raven - Vision, freedom, aerial superiority
    /// </summary>
    Winged = 4
}