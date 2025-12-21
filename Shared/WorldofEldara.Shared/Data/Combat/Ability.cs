using MessagePack;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Shared.Data.Combat;

/// <summary>
///     Represents a class ability or spell.
///     All abilities have lore-justified magic sources.
/// </summary>
[MessagePackObject]
public class Ability
{
    [Key(0)] public int AbilityId { get; set; }

    [Key(1)] public string Name { get; set; } = string.Empty;

    [Key(2)] public string Description { get; set; } = string.Empty;

    [Key(3)] public AbilityType Type { get; set; }

    [Key(4)] public MagicSource MagicSource { get; set; }

    [Key(5)] public DamageType DamageType { get; set; }

    [Key(6)] public TargetType TargetType { get; set; }

    [Key(7)] public float Range { get; set; } // meters

    [Key(8)] public float Radius { get; set; } // for AoE abilities

    [Key(9)] public int ManaCost { get; set; }

    [Key(10)] public float CastTime { get; set; } // seconds, 0 = instant

    [Key(11)] public float Cooldown { get; set; } // seconds

    [Key(12)] public float GlobalCooldown { get; set; } = 1.5f; // seconds

    [Key(13)] public bool CanCrit { get; set; } = true;

    [Key(14)] public bool TriggersGlobalCooldown { get; set; } = true;

    [Key(15)] public bool Interruptible { get; set; } = true;

    // Damage/healing values
    [Key(16)] public int BaseDamage { get; set; }

    [Key(17)] public float PowerScaling { get; set; } // multiplier for attack/spell power

    // Status effects
    [Key(18)] public List<StatusEffectData> AppliesEffects { get; set; } = new();

    // Requirements
    [Key(19)] public int RequiredLevel { get; set; } = 1;

    [Key(20)] public List<Class> AllowedClasses { get; set; } = new();

    // Lore justification (for design documentation)
    [Key(21)] public string LoreJustification { get; set; } = string.Empty;

    /// <summary>
    ///     Calculate actual damage/healing based on stats
    /// </summary>
    public int CalculateValue(CharacterStats casterStats, bool isCrit)
    {
        float power = Type switch
        {
            AbilityType.PhysicalDamage or AbilityType.MeleeDamage => casterStats.AttackPower,
            AbilityType.SpellDamage or AbilityType.Healing => casterStats.SpellPower,
            _ => 0
        };

        var value = BaseDamage + power * PowerScaling;

        if (isCrit && CanCrit) value *= casterStats.CriticalDamage;

        return (int)value;
    }
}

[MessagePackObject]
public sealed record AbilitySummary(
    [property: Key(0)] int AbilityId,
    [property: Key(1)] string Name,
    [property: Key(2)] AbilityType Type,
    [property: Key(3)] MagicSource MagicSource,
    [property: Key(4)] DamageType DamageType,
    [property: Key(5)] TargetType TargetType,
    [property: Key(6)] float Range,
    [property: Key(7)] float Cooldown,
    [property: Key(8)] int ManaCost)
{
    public static AbilitySummary From(Ability ability)
    {
        return new AbilitySummary(ability.AbilityId, ability.Name, ability.Type, ability.MagicSource, ability.DamageType,
            ability.TargetType, ability.Range, ability.Cooldown, ability.ManaCost);
    }
}

public enum AbilityType
{
    PhysicalDamage,
    MeleeDamage,
    SpellDamage,
    Healing,
    Buff,
    Debuff,
    Utility,
    Summon,
    Shapeshift,
    Teleport,
    Resurrection
}

public enum TargetType
{
    Self,
    SingleEnemy,
    SingleAlly,
    SingleTarget,
    AreaOfEffect,
    Cone,
    Line,
    GroundTarget,
    AllAllies,
    AllEnemies
}

/// <summary>
///     Status effect (buff/debuff) data
/// </summary>
[MessagePackObject]
public class StatusEffectData
{
    [Key(0)] public int EffectId { get; set; }

    [Key(1)] public string Name { get; set; } = string.Empty;

    [Key(2)] public StatusEffectType Type { get; set; }

    [Key(3)] public float Duration { get; set; } // seconds, -1 = permanent until removed

    [Key(4)] public int StackCount { get; set; } = 1;

    [Key(5)] public int MaxStacks { get; set; } = 1;

    [Key(6)] public bool IsBuff { get; set; } // true = buff, false = debuff

    [Key(7)] public bool Dispellable { get; set; } = true;

    // Periodic effects
    [Key(8)] public int PeriodicValue { get; set; } // damage/healing per tick

    [Key(9)] public float TickInterval { get; set; } // seconds between ticks

    // Stat modifications
    [Key(10)] public Dictionary<string, float> StatModifiers { get; set; } = new();

    [Key(11)] public string LoreJustification { get; set; } = string.Empty;
}

public enum StatusEffectType
{
    DamageOverTime,
    HealOverTime,
    StatBoost,
    StatReduction,
    Stun,
    Root,
    Silence,
    Disarm,
    Fear,
    Charm,
    Shield,
    Invisibility,
    PhaseShift, // Void-Touched specific
    TemporalSlow, // High Elf specific
    TotemFury, // Therakai specific
    GodFragment, // Gronnak specific
    MemoryLock // Memory Thief specific
}

/// <summary>
///     Combat constants for MMO-style combat
/// </summary>
public static class CombatConstants
{
    // Global cooldown (standard MMO)
    public const float DefaultGlobalCooldown = 1.5f;

    // Threat/aggro multipliers
    public const float TankThreatMultiplier = 3.0f;
    public const float HealerThreatMultiplier = 0.5f;
    public const float DPSThreatMultiplier = 1.0f;

    // Critical hit
    public const float BaseCritChance = 0.05f;
    public const float BaseCritDamage = 1.5f;

    // Range constants (meters)
    public const float MeleeRange = 5.0f;
    public const float MaxSpellRange = 40.0f;
    public const float MaxTargetRange = 50.0f;
    public const float ArmorMitigationConstant = 400.0f;

    // Damage reduction caps
    public const float MaxArmorReduction = 0.75f; // 75% max physical reduction
    public const float MaxResistanceReduction = 0.75f; // 75% max magical reduction

    // Combat states
    public const float OutOfCombatDelay = 5.0f; // seconds after last combat action
}
