using MessagePack;

namespace WorldofEldara.Shared.Data.Character;

/// <summary>
///     Core character data synchronized between client and server.
///     This represents the persistent character state.
/// </summary>
[MessagePackObject]
public class CharacterData
{
    [Key(0)] public ulong CharacterId { get; set; }

    [Key(1)] public ulong AccountId { get; set; }

    [Key(2)] public string Name { get; set; } = string.Empty;

    [Key(3)] public Race Race { get; set; }

    [Key(4)] public Class Class { get; set; }

    [Key(5)] public Faction Faction { get; set; }

    [Key(6)] public int Level { get; set; } = 1;

    [Key(7)] public long ExperiencePoints { get; set; }

    [Key(8)] public CharacterStats Stats { get; set; } = new();

    [Key(9)] public CharacterPosition Position { get; set; } = new();

    [Key(10)] public CharacterAppearance Appearance { get; set; } = new();

    [Key(11)] public EquipmentSlots Equipment { get; set; } = new();

    [Key(12)] public Dictionary<Faction, int> FactionStandings { get; set; } = new();

    // Therakai-specific
    [Key(13)] public TotemSpirit? TotemSpirit { get; set; }

    // Created timestamp
    [Key(14)] public DateTime CreatedAt { get; set; }

    // Last played timestamp
    [Key(15)] public DateTime LastPlayedAt { get; set; }

    /// <summary>
    ///     Validate character data for lore consistency
    /// </summary>
    public bool IsValid()
    {
        // Check class-race compatibility
        if (!ClassInfo.IsClassAvailableForRace(Class, Race))
            return false;

        // Check faction-race compatibility
        if (!FactionInfo.IsRaceAvailableForFaction(Race, Faction))
            return false;

        // Therakai must have totem
        if (Race == Race.Therakai && (!TotemSpirit.HasValue || TotemSpirit == Character.TotemSpirit.None))
            return false;

        // Non-Therakai should not have totem
        if (Race != Race.Therakai && TotemSpirit.HasValue && TotemSpirit != Character.TotemSpirit.None)
            return false;

        return true;
    }
}

/// <summary>
///     Character stats derived from class, race, and equipment
/// </summary>
[MessagePackObject]
public class CharacterStats
{
    // Primary attributes
    [Key(0)] public int Strength { get; set; } = 10;

    [Key(1)] public int Agility { get; set; } = 10;

    [Key(2)] public int Intellect { get; set; } = 10;

    [Key(3)] public int Stamina { get; set; } = 10;

    [Key(4)] public int Willpower { get; set; } = 10;

    // Derived stats
    [Key(5)] public int MaxHealth { get; set; } = 100;

    [Key(6)] public int CurrentHealth { get; set; } = 100;

    [Key(7)] public int MaxMana { get; set; } = 100;

    [Key(8)] public int CurrentMana { get; set; } = 100;

    // Combat stats
    [Key(9)] public int AttackPower { get; set; } = 10;

    [Key(10)] public int SpellPower { get; set; } = 10;

    [Key(11)] public float CriticalChance { get; set; } = 0.05f;

    [Key(12)] public float CriticalDamage { get; set; } = 1.5f;

    [Key(13)] public int Armor { get; set; } = 0;

    [Key(14)] public Dictionary<DamageType, float> Resistances { get; set; } = new();

    // Movement
    [Key(15)] public float MovementSpeed { get; set; } = 7.0f; // meters per second
}

/// <summary>
///     Character position in the world
/// </summary>
[MessagePackObject]
public class CharacterPosition
{
    [Key(0)] public string ZoneId { get; set; } = string.Empty;

    [Key(1)] public float X { get; set; }

    [Key(2)] public float Y { get; set; }

    [Key(3)] public float Z { get; set; }

    [Key(4)] public float RotationYaw { get; set; }

    [Key(5)] public float RotationPitch { get; set; }
}

/// <summary>
///     Character visual appearance
/// </summary>
[MessagePackObject]
public class CharacterAppearance
{
    [Key(0)] public int FaceType { get; set; } = 1;

    [Key(1)] public int HairStyle { get; set; } = 1;

    [Key(2)] public int HairColor { get; set; } = 1;

    [Key(3)] public int SkinTone { get; set; } = 1;

    [Key(4)] public int EyeColor { get; set; } = 1;

    [Key(5)] public float Height { get; set; } = 1.0f;

    [Key(6)] public float BuildType { get; set; } = 1.0f; // 0.8 = slim, 1.0 = average, 1.2 = muscular

    // Therakai-specific
    [Key(7)] public int FurPattern { get; set; } = 0;

    [Key(8)] public int FurColor { get; set; } = 0;

    // Void-Touched specific
    [Key(9)] public float VoidIntensity { get; set; } = 0.0f; // 0 to 1, how much void corruption shows
}

/// <summary>
///     Equipment slots following MMO standard
/// </summary>
[MessagePackObject]
public class EquipmentSlots
{
    [Key(0)] public ulong? Head { get; set; }

    [Key(1)] public ulong? Shoulders { get; set; }

    [Key(2)] public ulong? Chest { get; set; }

    [Key(3)] public ulong? Hands { get; set; }

    [Key(4)] public ulong? Legs { get; set; }

    [Key(5)] public ulong? Feet { get; set; }

    [Key(6)] public ulong? Back { get; set; }

    [Key(7)] public ulong? MainHand { get; set; }

    [Key(8)] public ulong? OffHand { get; set; }

    [Key(9)] public ulong? Ranged { get; set; }

    [Key(10)] public ulong? Ring1 { get; set; }

    [Key(11)] public ulong? Ring2 { get; set; }

    [Key(12)] public ulong? Trinket1 { get; set; }

    [Key(13)] public ulong? Trinket2 { get; set; }

    [Key(14)] public ulong? Necklace { get; set; }
}

/// <summary>
///     Damage types based on Eldara's magic systems
/// </summary>
public enum DamageType
{
    Physical, // Standard weapon damage
    Nature, // Growth, poison, decay (Worldroot)
    Radiant, // Divine life energy (Verdaniel, Korrath)
    Holy, // Divine force (gods)
    Necrotic, // Death and decay (broken Nereth)
    Arcane, // Pure magical force (High Elves)
    Fire, // Elemental
    Frost, // Elemental
    Lightning, // Elemental
    Void, // Existence erasure (Null)
    Shadow, // Darkness and fear
    Spirit, // Consciousness attacks
    Temporal // Damage across timelines (rare, High Elf specialty)
}