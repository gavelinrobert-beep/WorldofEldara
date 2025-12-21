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
            Stats = StatSnapshot.Empty with { Armor = 10, MovementSpeed = 6.5f },
            Resources = new ResourceSnapshot(220, 220, 0, 0, 120, 120),
            AbilityIds = new[] { AbilityBook.NPCMeleeId }
        },
        [2] = new NpcDefinition
        {
            TemplateId = 2,
            Name = "Borderkeep Guard",
            MinLevel = 3,
            MaxLevel = 4,
            Faction = Faction.UnitedKingdoms,
            SpawnZoneId = ZoneConstants.Borderkeep,
            Stats = StatSnapshot.Empty with { Armor = 8, MovementSpeed = 7f },
            Resources = new ResourceSnapshot(200, 200, 0, 0, 130, 130),
            AbilityIds = new[] { AbilityBook.BasicStrikeId }
        }
    };
}
