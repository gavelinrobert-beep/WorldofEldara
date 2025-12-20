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
            AllowedClasses = new List<Class> { Class.UnboundWarrior, Class.ArchonKnight, Class.MemoryWarden }
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
            AllowedClasses = new List<Class> { Class.TemporalMage }
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
