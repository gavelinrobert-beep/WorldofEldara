using WorldofEldara.Shared.Data.Combat;

namespace WorldofEldara.Shared.Data.Character;

/// <summary>
///     Lore-grounded class definitions with base stats, resources, and progression hooks.
///     Data-driven so future classes can be added without refactoring code.
/// </summary>
public sealed class ClassArchetype
{
    public Class Class { get; init; }
    public string Name { get; init; } = string.Empty;
    public ClassRole PrimaryRole { get; init; }
    public MagicSource MagicSource { get; init; }
    public EResourceType ResourceType { get; init; }
    public CharacterStats BaseStats { get; init; } = new();
    public StatProgression Progression { get; init; } = StatProgression.None;
    public IReadOnlyList<int> StartingAbilityIds { get; init; } = Array.Empty<int>();
    public IReadOnlyDictionary<int, IReadOnlyList<int>> AbilityUnlocks { get; init; } =
        new Dictionary<int, IReadOnlyList<int>>();
}

/// <summary>
///     Minimal stat growth definition to support leveling.
/// </summary>
public sealed record StatProgression(
    int HealthPerLevel = 0,
    int ManaPerLevel = 0,
    int StaminaResourcePerLevel = 0,
    int StrengthPerLevel = 0,
    int AgilityPerLevel = 0,
    int IntellectPerLevel = 0,
    int StaminaPerLevel = 0,
    int WillpowerPerLevel = 0,
    int AttackPowerPerLevel = 0,
    int SpellPowerPerLevel = 0)
{
    public static StatProgression None { get; } = new();
}

/// <summary>
///     Canonical class table keyed by lore classes.
/// </summary>
public static class ClassArchetypes
{
    private static readonly Dictionary<Class, ClassArchetype> Archetypes = new()
    {
        [Class.MemoryWarden] = new ClassArchetype
        {
            Class = Class.MemoryWarden,
            Name = "Memory Warden",
            PrimaryRole = ClassRole.Tank,
            MagicSource = MagicSource.Worldroot,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 15,
                Agility = 9,
                Intellect = 10,
                Stamina = 16,
                Willpower = 12,
                MaxHealth = 190,
                CurrentHealth = 190,
                MaxMana = 90,
                CurrentMana = 90,
                MaxStamina = 110,
                CurrentStamina = 110,
                AttackPower = 20,
                SpellPower = 10,
                MovementSpeed = 6.2f
            },
            Progression = new StatProgression(HealthPerLevel: 12, ManaPerLevel: 5, StaminaResourcePerLevel: 3,
                StrengthPerLevel: 1, StaminaPerLevel: 1, WillpowerPerLevel: 1, AttackPowerPerLevel: 2,
                SpellPowerPerLevel: 1),
            StartingAbilityIds = new[] { AbilityBook.BasicStrikeId, AbilityBook.HealingWaveId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.TotemPulseId }
            }
        },
        [Class.TemporalMage] = new ClassArchetype
        {
            Class = Class.TemporalMage,
            Name = "Temporal Mage",
            PrimaryRole = ClassRole.DPS,
            MagicSource = MagicSource.Arcane,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 8,
                Agility = 10,
                Intellect = 18,
                Stamina = 11,
                Willpower = 15,
                MaxHealth = 130,
                CurrentHealth = 130,
                MaxMana = 180,
                CurrentMana = 180,
                MaxStamina = 90,
                CurrentStamina = 90,
                AttackPower = 8,
                SpellPower = 24,
                MovementSpeed = 6.8f
            },
            Progression = new StatProgression(HealthPerLevel: 8, ManaPerLevel: 12, StaminaResourcePerLevel: 2,
                IntellectPerLevel: 2, WillpowerPerLevel: 1, SpellPowerPerLevel: 3),
            StartingAbilityIds = new[] { AbilityBook.ArcaneBoltId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.TemporalSliceId }
            }
        },
        [Class.UnboundWarrior] = new ClassArchetype
        {
            Class = Class.UnboundWarrior,
            Name = "Unbound Warrior",
            PrimaryRole = ClassRole.Hybrid,
            MagicSource = MagicSource.None,
            ResourceType = EResourceType.Stamina,
            BaseStats = new CharacterStats
            {
                Strength = 14,
                Agility = 12,
                Intellect = 10,
                Stamina = 14,
                Willpower = 10,
                MaxHealth = 165,
                CurrentHealth = 165,
                MaxMana = 0,
                CurrentMana = 0,
                MaxStamina = 120,
                CurrentStamina = 120,
                AttackPower = 18,
                SpellPower = 6,
                MovementSpeed = 6.7f
            },
            Progression = new StatProgression(HealthPerLevel: 10, StaminaResourcePerLevel: 3, StrengthPerLevel: 1,
                AgilityPerLevel: 1, StaminaPerLevel: 1, AttackPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.BasicStrikeId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.FrenziedStrikeId }
            }
        },
        [Class.TotemShaman] = new ClassArchetype
        {
            Class = Class.TotemShaman,
            Name = "Totem Shaman",
            PrimaryRole = ClassRole.Healer,
            MagicSource = MagicSource.Primal,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 10,
                Agility = 11,
                Intellect = 15,
                Stamina = 13,
                Willpower = 14,
                MaxHealth = 145,
                CurrentHealth = 145,
                MaxMana = 170,
                CurrentMana = 170,
                MaxStamina = 100,
                CurrentStamina = 100,
                AttackPower = 12,
                SpellPower = 18,
                MovementSpeed = 6.6f
            },
            Progression = new StatProgression(HealthPerLevel: 9, ManaPerLevel: 10, StaminaResourcePerLevel: 2,
                IntellectPerLevel: 1, WillpowerPerLevel: 1, SpellPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.HealingWaveId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.TotemPulseId }
            }
        },
        [Class.Berserker] = new ClassArchetype
        {
            Class = Class.Berserker,
            Name = "Berserker",
            PrimaryRole = ClassRole.DPS,
            MagicSource = MagicSource.Primal,
            ResourceType = EResourceType.Rage,
            BaseStats = new CharacterStats
            {
                Strength = 17,
                Agility = 13,
                Intellect = 8,
                Stamina = 15,
                Willpower = 9,
                MaxHealth = 175,
                CurrentHealth = 175,
                MaxMana = 0,
                CurrentMana = 0,
                MaxStamina = 130,
                CurrentStamina = 130,
                AttackPower = 22,
                SpellPower = 0,
                MovementSpeed = 7.0f
            },
            Progression = new StatProgression(HealthPerLevel: 11, StaminaResourcePerLevel: 4, StrengthPerLevel: 1,
                AgilityPerLevel: 1, StaminaPerLevel: 1, AttackPowerPerLevel: 3),
            StartingAbilityIds = new[] { AbilityBook.FrenziedStrikeId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.BasicStrikeId }
            }
        },
        [Class.WitchKingAcolyte] = new ClassArchetype
        {
            Class = Class.WitchKingAcolyte,
            Name = "Witch-King Acolyte",
            PrimaryRole = ClassRole.DPS,
            MagicSource = MagicSource.Dominion,
            ResourceType = EResourceType.Corruption,
            BaseStats = new CharacterStats
            {
                Strength = 12,
                Agility = 10,
                Intellect = 16,
                Stamina = 12,
                Willpower = 14,
                MaxHealth = 150,
                CurrentHealth = 150,
                MaxMana = 160,
                CurrentMana = 160,
                MaxStamina = 95,
                CurrentStamina = 95,
                AttackPower = 12,
                SpellPower = 20,
                MovementSpeed = 6.5f
            },
            Progression = new StatProgression(HealthPerLevel: 9, ManaPerLevel: 9, IntellectPerLevel: 1,
                WillpowerPerLevel: 1, SpellPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.DominionBoltId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.VoidKnifeId }
            }
        },
        [Class.VoidStalker] = new ClassArchetype
        {
            Class = Class.VoidStalker,
            Name = "Void Stalker",
            PrimaryRole = ClassRole.DPS,
            MagicSource = MagicSource.Void,
            ResourceType = EResourceType.Energy,
            BaseStats = new CharacterStats
            {
                Strength = 12,
                Agility = 17,
                Intellect = 10,
                Stamina = 13,
                Willpower = 10,
                MaxHealth = 150,
                CurrentHealth = 150,
                MaxMana = 0,
                CurrentMana = 0,
                MaxStamina = 115,
                CurrentStamina = 115,
                AttackPower = 17,
                SpellPower = 0,
                MovementSpeed = 7.4f
            },
            Progression = new StatProgression(HealthPerLevel: 9, StaminaResourcePerLevel: 3, StrengthPerLevel: 1,
                AgilityPerLevel: 2, AttackPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.VoidKnifeId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.PiercingArrowId }
            }
        },
        [Class.RootDruid] = new ClassArchetype
        {
            Class = Class.RootDruid,
            Name = "Root Druid",
            PrimaryRole = ClassRole.Healer,
            MagicSource = MagicSource.Worldroot,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 9,
                Agility = 11,
                Intellect = 16,
                Stamina = 13,
                Willpower = 15,
                MaxHealth = 150,
                CurrentHealth = 150,
                MaxMana = 170,
                CurrentMana = 170,
                MaxStamina = 100,
                CurrentStamina = 100,
                AttackPower = 9,
                SpellPower = 20,
                MovementSpeed = 6.7f
            },
            Progression = new StatProgression(HealthPerLevel: 9, ManaPerLevel: 10, StaminaResourcePerLevel: 2,
                IntellectPerLevel: 1, WillpowerPerLevel: 1, SpellPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.HealingWaveId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.TotemPulseId }
            }
        },
        [Class.ArchonKnight] = new ClassArchetype
        {
            Class = Class.ArchonKnight,
            Name = "Archon Knight",
            PrimaryRole = ClassRole.Tank,
            MagicSource = MagicSource.Arcane,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 15,
                Agility = 10,
                Intellect = 12,
                Stamina = 16,
                Willpower = 12,
                MaxHealth = 185,
                CurrentHealth = 185,
                MaxMana = 100,
                CurrentMana = 100,
                MaxStamina = 115,
                CurrentStamina = 115,
                AttackPower = 20,
                SpellPower = 10,
                MovementSpeed = 6.4f
            },
            Progression = new StatProgression(HealthPerLevel: 11, ManaPerLevel: 6, StaminaResourcePerLevel: 3,
                StrengthPerLevel: 1, StaminaPerLevel: 1, WillpowerPerLevel: 1, AttackPowerPerLevel: 2,
                SpellPowerPerLevel: 1),
            StartingAbilityIds = new[] { AbilityBook.BasicStrikeId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.TemporalSliceId }
            }
        },
        [Class.FreeBlade] = new ClassArchetype
        {
            Class = Class.FreeBlade,
            Name = "Free Blade",
            PrimaryRole = ClassRole.DPS,
            MagicSource = MagicSource.None,
            ResourceType = EResourceType.Focus,
            BaseStats = new CharacterStats
            {
                Strength = 13,
                Agility = 15,
                Intellect = 10,
                Stamina = 13,
                Willpower = 11,
                MaxHealth = 150,
                CurrentHealth = 150,
                MaxMana = 0,
                CurrentMana = 0,
                MaxStamina = 115,
                CurrentStamina = 115,
                AttackPower = 16,
                SpellPower = 0,
                MovementSpeed = 7.1f
            },
            Progression = new StatProgression(HealthPerLevel: 9, StaminaResourcePerLevel: 3, StrengthPerLevel: 1,
                AgilityPerLevel: 1, AttackPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.PiercingArrowId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.BasicStrikeId }
            }
        },
        [Class.GodSeekerCleric] = new ClassArchetype
        {
            Class = Class.GodSeekerCleric,
            Name = "God-Seeker Cleric",
            PrimaryRole = ClassRole.Healer,
            MagicSource = MagicSource.Divine,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 10,
                Agility = 9,
                Intellect = 15,
                Stamina = 14,
                Willpower = 16,
                MaxHealth = 155,
                CurrentHealth = 155,
                MaxMana = 180,
                CurrentMana = 180,
                MaxStamina = 95,
                CurrentStamina = 95,
                AttackPower = 9,
                SpellPower = 18,
                MovementSpeed = 6.4f
            },
            Progression = new StatProgression(HealthPerLevel: 9, ManaPerLevel: 11, IntellectPerLevel: 1,
                WillpowerPerLevel: 2, SpellPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.RadiantMendId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.HealingWaveId }
            }
        },
        [Class.MemoryThief] = new ClassArchetype
        {
            Class = Class.MemoryThief,
            Name = "Memory Thief",
            PrimaryRole = ClassRole.Support,
            MagicSource = MagicSource.Memory,
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 11,
                Agility = 14,
                Intellect = 14,
                Stamina = 12,
                Willpower = 13,
                MaxHealth = 145,
                CurrentHealth = 145,
                MaxMana = 150,
                CurrentMana = 150,
                MaxStamina = 100,
                CurrentStamina = 100,
                AttackPower = 12,
                SpellPower = 16,
                MovementSpeed = 7.0f
            },
            Progression = new StatProgression(HealthPerLevel: 8, ManaPerLevel: 10, StaminaResourcePerLevel: 2,
                AgilityPerLevel: 1, IntellectPerLevel: 1, SpellPowerPerLevel: 2),
            StartingAbilityIds = new[] { AbilityBook.VoidKnifeId },
            AbilityUnlocks = new Dictionary<int, IReadOnlyList<int>>
            {
                [3] = new[] { AbilityBook.ArcaneBoltId }
            }
        }
    };

    public static ClassArchetype Get(Class classType)
    {
        if (Archetypes.TryGetValue(classType, out var archetype)) return archetype;
        throw new KeyNotFoundException($"Class definition not found for {classType}");
    }

    public static CharacterStats BuildStatsForLevel(Class classType, int level)
    {
        var definition = Get(classType);
        var stats = Clone(definition.BaseStats);
        ApplyProgression(stats, definition.Progression, level);
        stats.CurrentHealth = stats.MaxHealth;
        stats.CurrentMana = stats.MaxMana;
        stats.CurrentStamina = stats.MaxStamina;
        return stats;
    }

    public static IReadOnlyCollection<int> GetAbilitiesForLevel(Class classType, int level)
    {
        var definition = Get(classType);
        var abilities = new HashSet<int>(definition.StartingAbilityIds);
        foreach (var unlock in definition.AbilityUnlocks)
            if (unlock.Key <= level)
                abilities.UnionWith(unlock.Value);
        return abilities;
    }

    private static CharacterStats Clone(CharacterStats source)
    {
        return new CharacterStats
        {
            Strength = source.Strength,
            Agility = source.Agility,
            Intellect = source.Intellect,
            Stamina = source.Stamina,
            Willpower = source.Willpower,
            MaxHealth = source.MaxHealth,
            CurrentHealth = source.CurrentHealth,
            MaxMana = source.MaxMana,
            CurrentMana = source.CurrentMana,
            MaxStamina = source.MaxStamina,
            CurrentStamina = source.CurrentStamina,
            AttackPower = source.AttackPower,
            SpellPower = source.SpellPower,
            CriticalChance = source.CriticalChance,
            CriticalDamage = source.CriticalDamage,
            Armor = source.Armor,
            Resistances = new Dictionary<DamageType, float>(source.Resistances),
            MovementSpeed = source.MovementSpeed
        };
    }

    private static void ApplyProgression(CharacterStats stats, StatProgression progression, int level)
    {
        var levelsGained = Math.Max(0, level - 1);
        if (levelsGained == 0) return;

        stats.MaxHealth += progression.HealthPerLevel * levelsGained;
        stats.MaxMana += progression.ManaPerLevel * levelsGained;
        stats.MaxStamina += progression.StaminaResourcePerLevel * levelsGained;
        stats.Strength += progression.StrengthPerLevel * levelsGained;
        stats.Agility += progression.AgilityPerLevel * levelsGained;
        stats.Intellect += progression.IntellectPerLevel * levelsGained;
        stats.Stamina += progression.StaminaPerLevel * levelsGained;
        stats.Willpower += progression.WillpowerPerLevel * levelsGained;
        stats.AttackPower += progression.AttackPowerPerLevel * levelsGained;
        stats.SpellPower += progression.SpellPowerPerLevel * levelsGained;
    }
}
