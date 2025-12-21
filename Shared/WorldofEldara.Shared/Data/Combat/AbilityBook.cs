using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Constants;

namespace WorldofEldara.Shared.Data.Combat;

/// <summary>
///     Minimal, server-safe ability catalog for early gameplay and automated tests.
///     IDs are stable across client and server.
/// </summary>
public static class AbilityBook
{
    public const int BasicStrikeId = 1;
    public const int PiercingArrowId = 2;
    public const int ArcaneBoltId = 3;
    public const int HealingWaveId = 4;
    public const int VoidKnifeId = 5;
    public const int DominionBoltId = 6;
    public const int RadiantMendId = 7;
    public const int TemporalSliceId = 8;
    public const int FrenziedStrikeId = 9;
    public const int TotemPulseId = 10;
    public const int NPCMeleeId = 100;

    private static readonly Dictionary<int, Ability> Abilities = new()
    {
        [BasicStrikeId] = new Ability
        {
            AbilityId = BasicStrikeId,
            Name = "Basic Strike",
            Description = "A disciplined melee attack.",
            Type = AbilityType.MeleeDamage,
            MagicSource = MagicSource.None,
            DamageType = DamageType.Physical,
            TargetType = TargetType.SingleEnemy,
            Range = CombatConstants.MeleeRange,
            Radius = 0,
            ManaCost = 0,
            CastTime = 0,
            Cooldown = 1.0f,
            PowerScaling = 0.8f,
            BaseDamage = 18,
            AllowedClasses = new List<Class>
                { Class.UnboundWarrior, Class.ArchonKnight, Class.MemoryWarden, Class.Berserker, Class.FreeBlade }
        },
        [PiercingArrowId] = new Ability
        {
            AbilityId = PiercingArrowId,
            Name = "Piercing Arrow",
            Description = "A precise shot that ignores light armor.",
            Type = AbilityType.PhysicalDamage,
            MagicSource = MagicSource.None,
            DamageType = DamageType.Physical,
            TargetType = TargetType.SingleEnemy,
            Range = 25.0f,
            Radius = 0,
            ManaCost = 0,
            CastTime = 0,
            Cooldown = 1.5f,
            PowerScaling = 0.6f,
            BaseDamage = 16,
            AllowedClasses = new List<Class> { Class.FreeBlade, Class.VoidStalker }
        },
        [ArcaneBoltId] = new Ability
        {
            AbilityId = ArcaneBoltId,
            Name = "Arcane Bolt",
            Description = "A quick shard of unstable arcane force.",
            Type = AbilityType.SpellDamage,
            MagicSource = MagicSource.Arcane,
            DamageType = DamageType.Arcane,
            TargetType = TargetType.SingleEnemy,
            Range = GameConstants.DefaultSpellRange,
            Radius = 0,
            ManaCost = 10,
            CastTime = 0.25f,
            Cooldown = 1.2f,
            PowerScaling = 0.7f,
            BaseDamage = 20,
            AllowedClasses = new List<Class> { Class.TemporalMage, Class.MemoryThief }
        },
        [HealingWaveId] = new Ability
        {
            AbilityId = HealingWaveId,
            Name = "Healing Wave",
            Description = "Channel Worldroot vitality to mend wounds.",
            Type = AbilityType.Healing,
            MagicSource = MagicSource.Worldroot,
            DamageType = DamageType.Nature,
            TargetType = TargetType.SingleAlly,
            Range = GameConstants.DefaultSpellRange,
            ManaCost = 18,
            CastTime = 0,
            Cooldown = 1.8f,
            PowerScaling = 0.9f,
            BaseDamage = 22,
            AllowedClasses = new List<Class>
            {
                Class.TotemShaman, Class.RootDruid, Class.MemoryWarden, Class.GodSeekerCleric
            }
        },
        [VoidKnifeId] = new Ability
        {
            AbilityId = VoidKnifeId,
            Name = "Void Knife",
            Description = "A phase-shifted strike that erodes meaning.",
            Type = AbilityType.MeleeDamage,
            MagicSource = MagicSource.Void,
            DamageType = DamageType.Void,
            TargetType = TargetType.SingleEnemy,
            Range = CombatConstants.MeleeRange,
            Radius = 0,
            ManaCost = 0,
            CastTime = 0,
            Cooldown = 1.0f,
            PowerScaling = 0.7f,
            BaseDamage = 20,
            AllowedClasses = new List<Class> { Class.VoidStalker, Class.MemoryThief, Class.WitchKingAcolyte }
        },
        [DominionBoltId] = new Ability
        {
            AbilityId = DominionBoltId,
            Name = "Dominion Bolt",
            Description = "Unleash consumed divinity as searing force.",
            Type = AbilityType.SpellDamage,
            MagicSource = MagicSource.Dominion,
            DamageType = DamageType.Shadow,
            TargetType = TargetType.SingleEnemy,
            Range = GameConstants.DefaultSpellRange,
            Radius = 0,
            ManaCost = 14,
            CastTime = 0.2f,
            Cooldown = 1.3f,
            PowerScaling = 0.65f,
            BaseDamage = 24,
            AllowedClasses = new List<Class> { Class.WitchKingAcolyte }
        },
        [RadiantMendId] = new Ability
        {
            AbilityId = RadiantMendId,
            Name = "Radiant Mend",
            Description = "Call on fading gods to restore an ally.",
            Type = AbilityType.Healing,
            MagicSource = MagicSource.Divine,
            DamageType = DamageType.Radiant,
            TargetType = TargetType.SingleAlly,
            Range = GameConstants.DefaultSpellRange,
            ManaCost = 16,
            CastTime = 0,
            Cooldown = 1.8f,
            PowerScaling = 0.85f,
            BaseDamage = 26,
            AllowedClasses = new List<Class> { Class.GodSeekerCleric }
        },
        [TemporalSliceId] = new Ability
        {
            AbilityId = TemporalSliceId,
            Name = "Temporal Slice",
            Description = "Cut through a moment, damaging foes across timelines.",
            Type = AbilityType.SpellDamage,
            MagicSource = MagicSource.Arcane,
            DamageType = DamageType.Temporal,
            TargetType = TargetType.SingleEnemy,
            Range = GameConstants.DefaultSpellRange,
            Radius = 0,
            ManaCost = 12,
            CastTime = 0,
            Cooldown = 1.3f,
            PowerScaling = 0.75f,
            BaseDamage = 22,
            AllowedClasses = new List<Class> { Class.TemporalMage, Class.ArchonKnight }
        },
        [FrenziedStrikeId] = new Ability
        {
            AbilityId = FrenziedStrikeId,
            Name = "Frenzied Strike",
            Description = "A reckless blow fueled by raw instinct.",
            Type = AbilityType.MeleeDamage,
            MagicSource = MagicSource.Primal,
            DamageType = DamageType.Physical,
            TargetType = TargetType.SingleEnemy,
            Range = CombatConstants.MeleeRange,
            Radius = 0,
            ManaCost = 0,
            CastTime = 0,
            Cooldown = 0.9f,
            PowerScaling = 0.9f,
            BaseDamage = 24,
            AllowedClasses = new List<Class> { Class.Berserker, Class.UnboundWarrior }
        },
        [TotemPulseId] = new Ability
        {
            AbilityId = TotemPulseId,
            Name = "Totem Pulse",
            Description = "Send a pulse of primal energy to nearby allies and foes.",
            Type = AbilityType.SpellDamage,
            MagicSource = MagicSource.Primal,
            DamageType = DamageType.Nature,
            TargetType = TargetType.AreaOfEffect,
            Range = CombatConstants.MeleeRange,
            Radius = 8,
            ManaCost = 14,
            CastTime = 0,
            Cooldown = 2.0f,
            PowerScaling = 0.6f,
            BaseDamage = 18,
            AllowedClasses = new List<Class> { Class.TotemShaman, Class.RootDruid, Class.MemoryWarden }
        },
        [NPCMeleeId] = new Ability
        {
            AbilityId = NPCMeleeId,
            Name = "Claw",
            Description = "A simple melee swipe used by basic NPCs.",
            Type = AbilityType.MeleeDamage,
            MagicSource = MagicSource.None,
            DamageType = DamageType.Physical,
            TargetType = TargetType.SingleEnemy,
            Range = CombatConstants.MeleeRange,
            Radius = 0,
            ManaCost = 0,
            CastTime = 0,
            BaseDamage = 10,
            PowerScaling = 0.4f,
            Cooldown = 1.5f,
            AllowedClasses = new List<Class>()
        }
    };

    public static Ability GetAbility(int abilityId)
    {
        return Abilities.TryGetValue(abilityId, out var ability)
            ? ability
            : throw new KeyNotFoundException($"Ability {abilityId} is not defined.");
    }
}
