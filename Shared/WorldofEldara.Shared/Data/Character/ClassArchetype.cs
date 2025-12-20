using WorldofEldara.Shared.Data.Combat;

namespace WorldofEldara.Shared.Data.Character;

/// <summary>
///     Lightweight, extensible class archetype definitions used by both server and client.
///     These map the broader lore classes to a minimal, MMO-friendly trio for bootstrapping:
///     Warrior, Ranger, Mage.
/// </summary>
public enum CoreClassId
{
    Warrior = 1,
    Ranger = 2,
    Mage = 3
}

/// <summary>
///     Base stats and starting abilities for a class archetype.
/// </summary>
public sealed class ClassArchetype
{
    public CoreClassId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public EResourceType ResourceType { get; init; }
    public CharacterStats BaseStats { get; init; } = new();
    public IReadOnlyList<int> StartingAbilityIds { get; init; } = Array.Empty<int>();
}

/// <summary>
///     Canonical class archetype table.
/// </summary>
public static class ClassArchetypes
{
    private static readonly Dictionary<CoreClassId, ClassArchetype> Archetypes = new()
    {
        [CoreClassId.Warrior] = new ClassArchetype
        {
            Id = CoreClassId.Warrior,
            Name = "Warrior",
            ResourceType = EResourceType.Rage,
            BaseStats = new CharacterStats
            {
                Strength = 14,
                Agility = 10,
                Intellect = 8,
                Stamina = 14,
                Willpower = 10,
                MaxHealth = 160,
                CurrentHealth = 160,
                MaxMana = 0,
                CurrentMana = 0,
                AttackPower = 20,
                SpellPower = 0,
                MovementSpeed = 6.5f
            },
            StartingAbilityIds = new[] { AbilityBook.BasicStrikeId }
        },
        [CoreClassId.Ranger] = new ClassArchetype
        {
            Id = CoreClassId.Ranger,
            Name = "Ranger",
            ResourceType = EResourceType.Focus,
            BaseStats = new CharacterStats
            {
                Strength = 10,
                Agility = 14,
                Intellect = 10,
                Stamina = 12,
                Willpower = 10,
                MaxHealth = 140,
                CurrentHealth = 140,
                MaxMana = 0,
                CurrentMana = 0,
                AttackPower = 16,
                SpellPower = 0,
                MovementSpeed = 7.2f
            },
            StartingAbilityIds = new[] { AbilityBook.PiercingArrowId }
        },
        [CoreClassId.Mage] = new ClassArchetype
        {
            Id = CoreClassId.Mage,
            Name = "Mage",
            ResourceType = EResourceType.Mana,
            BaseStats = new CharacterStats
            {
                Strength = 8,
                Agility = 9,
                Intellect = 16,
                Stamina = 11,
                Willpower = 14,
                MaxHealth = 130,
                CurrentHealth = 130,
                MaxMana = 160,
                CurrentMana = 160,
                AttackPower = 8,
                SpellPower = 22,
                MovementSpeed = 6.8f
            },
            StartingAbilityIds = new[] { AbilityBook.ArcaneBoltId }
        }
    };

    /// <summary>
    ///     Map a lore class to a core archetype to keep both systems aligned.
    ///     This keeps compatibility with existing lore-rich classes while enabling
    ///     streamlined server logic for early gameplay.
    /// </summary>
    public static CoreClassId MapToArchetype(Class classType)
    {
        return classType switch
        {
            Class.TemporalMage => CoreClassId.Mage,
            Class.MemoryWarden => CoreClassId.Warrior,
            Class.UnboundWarrior => CoreClassId.Warrior,
            Class.ArchonKnight => CoreClassId.Warrior,
            Class.VoidStalker => CoreClassId.Ranger,
            Class.FreeBlade => CoreClassId.Ranger,
            _ => CoreClassId.Warrior
        };
    }

    public static ClassArchetype Get(CoreClassId id)
    {
        return Archetypes[id];
    }
}
