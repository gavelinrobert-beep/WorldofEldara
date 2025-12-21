using System.Collections.Generic;
using MessagePack;
using WorldofEldara.Shared.Protocol;

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

    [IgnoreMember]
    public ResourceSnapshot Resources => ResourceSnapshot.FromStats(Stats);

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

    // Stamina resource (movement/combat endurance)
    [Key(16)] public int MaxStamina { get; set; } = 100;

    [Key(17)] public int CurrentStamina { get; set; } = 100;
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

[MessagePackObject]
public sealed record ResourceSnapshot(
    [property: Key(0)] int MaxHealth,
    [property: Key(1)] int CurrentHealth,
    [property: Key(2)] int MaxMana,
    [property: Key(3)] int CurrentMana,
    [property: Key(4)] int MaxStamina,
    [property: Key(5)] int CurrentStamina)
{
    public static ResourceSnapshot Empty { get; } = new(0, 0, 0, 0, 0, 0);

    public static ResourceSnapshot FromStats(CharacterStats stats)
    {
        return new ResourceSnapshot(stats.MaxHealth, stats.CurrentHealth, stats.MaxMana, stats.CurrentMana,
            stats.MaxStamina, stats.CurrentStamina);
    }
}

[MessagePackObject]
public sealed record StatSnapshot(
    [property: Key(0)] int Strength,
    [property: Key(1)] int Agility,
    [property: Key(2)] int Intellect,
    [property: Key(3)] int Stamina,
    [property: Key(4)] int Willpower,
    [property: Key(5)] int AttackPower,
    [property: Key(6)] int SpellPower,
    [property: Key(7)] float CriticalChance,
    [property: Key(8)] float CriticalDamage,
    [property: Key(9)] int Armor,
    [property: Key(10)] float MovementSpeed)
{
    public static StatSnapshot Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static StatSnapshot From(CharacterStats stats)
    {
        return new StatSnapshot(stats.Strength, stats.Agility, stats.Intellect, stats.Stamina, stats.Willpower,
            stats.AttackPower, stats.SpellPower, stats.CriticalChance, stats.CriticalDamage, stats.Armor,
            stats.MovementSpeed);
    }
}

[MessagePackObject]
public sealed record CharacterSnapshot
{
    [Key(0)] public ulong CharacterId { get; init; }

    [Key(1)] public string Name { get; init; } = string.Empty;

    [Key(2)] public Race Race { get; init; }

    [Key(3)] public Class Class { get; init; }

    [Key(4)] public Faction Faction { get; init; }

    [Key(5)] public int Level { get; init; }

    [Key(6)] public StatSnapshot Stats { get; init; } = StatSnapshot.Empty;

    [Key(7)] public ResourceSnapshot Resources { get; init; } = ResourceSnapshot.Empty;

    [Key(8)] public CharacterPosition Position { get; init; } = new();

    [Key(9)] public IReadOnlyList<int> KnownAbilities { get; init; } = Array.Empty<int>();

    [Key(10)] public string ZoneId { get; init; } = string.Empty;

    [Key(11)] public DateTime LastPlayedAt { get; init; }

    [Key(12)] public string Version { get; init; } = ProtocolVersions.Current;

    public static CharacterSnapshot FromCharacter(CharacterData character, IReadOnlyList<int>? abilities = null)
    {
        return new CharacterSnapshot
        {
            CharacterId = character.CharacterId,
            Name = character.Name,
            Race = character.Race,
            Class = character.Class,
            Faction = character.Faction,
            Level = character.Level,
            Stats = StatSnapshot.From(character.Stats),
            Resources = ResourceSnapshot.FromStats(character.Stats),
            Position = character.Position,
            KnownAbilities = abilities ?? Array.Empty<int>(),
            ZoneId = character.Position.ZoneId,
            LastPlayedAt = character.LastPlayedAt
        };
    }
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

/// <summary>
///     Resource types used by classes and abilities. Mirrors client-side enumeration.
/// </summary>
public enum EResourceType : byte
{
    Health = 0,
    Mana = 1,
    Rage = 2,
    Energy = 3,
    Focus = 4,
    Corruption = 5,
    Stamina = 6
}
