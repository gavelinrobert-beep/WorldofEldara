using MessagePack;
using WorldofEldara.Shared.Constants;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Combat;

namespace WorldofEldara.Shared.Data.World;

/// <summary>
///     Immutable NPC template usable by both Unreal client and .NET server.
/// </summary>
[MessagePackObject]
public sealed record NpcDefinition
{
    [Key(0)] public int TemplateId { get; init; }

    [Key(1)] public string Name { get; init; } = string.Empty;

    [Key(2)] public int MinLevel { get; init; }

    [Key(3)] public int MaxLevel { get; init; }

    [Key(4)] public Faction Faction { get; init; } = Faction.Neutral;

    [Key(5)] public ResourceSnapshot Resources { get; init; } = ResourceSnapshot.Empty;

    [Key(6)] public StatSnapshot Stats { get; init; } = StatSnapshot.Empty;

    [Key(7)] public IReadOnlyList<int> AbilityIds { get; init; } = Array.Empty<int>();

    [Key(8)] public string SpawnZoneId { get; init; } = string.Empty;
}

/// <summary>
///     Canonical NPC templates aligned with lore zones.
/// </summary>
public static class NpcDefinitions
{
    private const int SentinelArmor = 10;
    private const int GuardArmor = 8;
    private const float SentinelMovementSpeed = GameConstants.BaseRunSpeed - 0.5f;
    private const float GuardMovementSpeed = GameConstants.BaseRunSpeed;
    private const int SentinelHealth = 220;
    private const int GuardHealth = 200;
    private const int SentinelStamina = 120;
    private const int GuardStamina = 130;
    private static readonly IReadOnlyList<int> SentinelAbilities = new[] { AbilityBook.NPCMeleeId };
    private static readonly IReadOnlyList<int> GuardAbilities = new[] { AbilityBook.BasicStrikeId };

    private static ResourceSnapshot CreateResourcePool(int health, int stamina)
    {
        return new ResourceSnapshot(health, health, 0, 0, stamina, stamina);
    }

    public static readonly IReadOnlyDictionary<int, NpcDefinition> Templates = new Dictionary<int, NpcDefinition>
    {
        [1] = new NpcDefinition
        {
            TemplateId = 1,
            Name = "Worldroot Sentinel",
            MinLevel = 5,
            MaxLevel = 6,
            Faction = Faction.VerdantCircles,
            SpawnZoneId = ZoneConstants.ThornveilEnclave,
            Stats = StatSnapshot.Empty with { Armor = SentinelArmor, MovementSpeed = SentinelMovementSpeed },
            Resources = CreateResourcePool(SentinelHealth, SentinelStamina),
            AbilityIds = SentinelAbilities
        },
        [2] = new NpcDefinition
        {
            TemplateId = 2,
            Name = "Borderkeep Guard",
            MinLevel = 3,
            MaxLevel = 4,
            Faction = Faction.UnitedKingdoms,
            SpawnZoneId = ZoneConstants.Borderkeep,
            Stats = StatSnapshot.Empty with { Armor = GuardArmor, MovementSpeed = GuardMovementSpeed },
            Resources = CreateResourcePool(GuardHealth, GuardStamina),
            AbilityIds = GuardAbilities
        },
        [1101] = new NpcDefinition
        {
            TemplateId = 1101,
            Name = "Keeper Aelwyn",
            MinLevel = 8,
            MaxLevel = 10,
            Faction = Faction.VerdantCircles,
            SpawnZoneId = ZoneConstants.BriarwatchCrossing,
            Stats = StatSnapshot.Empty with { MovementSpeed = GuardMovementSpeed },
            Resources = CreateResourcePool(GuardHealth, GuardStamina)
        },
        [1102] = new NpcDefinition
        {
            TemplateId = 1102,
            Name = "Quartermaster Liora",
            MinLevel = 8,
            MaxLevel = 12,
            Faction = Faction.VerdantCircles,
            SpawnZoneId = ZoneConstants.BriarwatchCrossing,
            Stats = StatSnapshot.Empty with { MovementSpeed = GuardMovementSpeed },
            Resources = CreateResourcePool(GuardHealth + 20, GuardStamina + 10)
        }
    };
}
