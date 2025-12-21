using MessagePack;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Shared.Data.World;

/// <summary>
///     Represents a game zone/area in Eldara
/// </summary>
[MessagePackObject]
public class Zone
{
    [Key(0)] public string ZoneId { get; set; } = string.Empty;

    [Key(1)] public string Name { get; set; } = string.Empty;

    [Key(2)] public string Description { get; set; } = string.Empty;

    [Key(3)] public int MinLevel { get; set; } = 1;

    [Key(4)] public int MaxLevel { get; set; } = 10;

    [Key(5)] public ZoneType Type { get; set; }

    [Key(6)] public Faction ControllingFaction { get; set; } = Faction.Neutral;

    [Key(7)] public float WorldrootDensity { get; set; } = 0.5f; // 0.0 to 1.0

    [Key(8)] public bool IsPvPEnabled { get; set; }

    [Key(9)] public bool IsContestedTerritory { get; set; }

    [Key(10)] public List<string> ConnectedZones { get; set; } = new();

    [Key(11)] public WorldPosition SafeSpawnPoint { get; set; } = new();

    [Key(12)] public string LoreDescription { get; set; } = string.Empty;
}

public enum ZoneType
{
    StarterZone,
    OpenWorld,
    Dungeon,
    Raid,
    PvPBattleground,
    City,
    Instanced
}

[MessagePackObject]
public struct WorldPosition
{
    [Key(0)] public float X { get; set; }

    [Key(1)] public float Y { get; set; }

    [Key(2)] public float Z { get; set; }

    public WorldPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

/// <summary>
///     Zone definitions with lore justification
/// </summary>
public static class ZoneDefinitions
{
    public static readonly Dictionary<string, Zone> Zones = new()
    {
        ["zone_thornveil_enclave"] = new Zone
        {
            ZoneId = "zone_thornveil_enclave",
            Name = "Thornveil Enclave",
            Description = "Ancient forest where the Worldroot's presence is strongest",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.VerdantCircles,
            WorldrootDensity = 1.0f,
            IsPvPEnabled = false,
            LoreDescription = "Trees remember every footstep. Younger Sylvaen train here to hear the Green Memory.",
            SafeSpawnPoint = new WorldPosition(0, 0, 0)
        },

        ["zone_temporal_steppes"] = new Zone
        {
            ZoneId = "zone_temporal_steppes",
            Name = "Temporal Steppes",
            Description = "Reality is thin here. Time flows inconsistently.",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.AscendantLeague,
            WorldrootDensity = 0.3f,
            IsPvPEnabled = false,
            LoreDescription = "Ruins phase in and out of existence. High Elves study Edit Nodes.",
            SafeSpawnPoint = new WorldPosition(10, 5, 0)
        },

        ["zone_borderkeep"] = new Zone
        {
            ZoneId = "zone_borderkeep",
            Name = "Borderkeep",
            Description = "A frontier fortress-town where humanity makes its stand",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.UnitedKingdoms,
            WorldrootDensity = 0.4f,
            IsPvPEnabled = false,
            LoreDescription = "Humans from all walks of life gather here, united by their mysterious nature.",
            SafeSpawnPoint = new WorldPosition(-8, -4, 0)
        },

        ["zone_untamed_reaches"] = new Zone
        {
            ZoneId = "zone_untamed_reaches",
            Name = "The Untamed Reaches",
            Description = "Wildborn Therakai hunting grounds infused with totem spirits.",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.TotemClansWildborn,
            WorldrootDensity = 0.6f,
            IsPvPEnabled = false,
            LoreDescription = "Instinct rules here. Totem echoes stalk every ridge.",
            SafeSpawnPoint = new WorldPosition(6, -12, 0)
        },

        ["zone_carved_valleys"] = new Zone
        {
            ZoneId = "zone_carved_valleys",
            Name = "The Carved Valleys",
            Description = "Pathbound Therakai refuge that balances instinct with chosen memory.",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.TotemClansPathbound,
            WorldrootDensity = 0.55f,
            IsPvPEnabled = false,
            LoreDescription = "Stone totems mark choices remembered; spirits watch silently.",
            SafeSpawnPoint = new WorldPosition(-12, 3, 0)
        },

        ["zone_scarred_highlands"] = new Zone
        {
            ZoneId = "zone_scarred_highlands",
            Name = "The Scarred Highlands",
            Description = "Gronnak warfront built atop a reality wound.",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.DominionWarhost,
            WorldrootDensity = 0.2f,
            IsPvPEnabled = true,
            IsContestedTerritory = true,
            LoreDescription = "Dominion Warhost drills here, consuming god-fragments to harden themselves.",
            SafeSpawnPoint = new WorldPosition(4, 14, 0)
        },

        ["zone_blackwake_haven"] = new Zone
        {
            ZoneId = "zone_blackwake_haven",
            Name = "Blackwake Haven",
            Description = "Void Compact enclave where outcasts study entropy safely.",
            MinLevel = 1,
            MaxLevel = 10,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.VoidCompact,
            WorldrootDensity = 0.1f,
            IsPvPEnabled = false,
            LoreDescription = "Phase-shifted caverns hum with Null-static; survival requires discipline.",
            SafeSpawnPoint = new WorldPosition(-2, 18, 0)
        },

        ["zone_greenwatch_training"] = new Zone
        {
            ZoneId = "zone_greenwatch_training",
            Name = "Greenwatch Training Grounds",
            Description = "Controlled grove where Verdant sentinels learn to respond to threats.",
            MinLevel = 1,
            MaxLevel = 5,
            Type = ZoneType.StarterZone,
            ControllingFaction = Faction.VerdantCircles,
            WorldrootDensity = 0.9f,
            IsPvPEnabled = false,
            LoreDescription = "The Worldroot hums quietly here while trainees drill under Greenwatch oversight.",
            SafeSpawnPoint = new WorldPosition(20, -8, 0)
        },

        ["zone_vharos_warfront"] = new Zone
        {
            ZoneId = "zone_vharos_warfront",
            Name = "Vharos Warfront",
            Description = "The Therakai civil war rages here",
            MinLevel = 20,
            MaxLevel = 30,
            Type = ZoneType.OpenWorld,
            ControllingFaction = Faction.Neutral,
            WorldrootDensity = 0.5f,
            IsPvPEnabled = true,
            IsContestedTerritory = true,
            LoreDescription = "Wildborn and Pathbound clash constantly. Other factions exploit the chaos."
        },

        ["zone_root_core"] = new Zone
        {
            ZoneId = "zone_root_core",
            Name = "The Root Core",
            Description = "The Worldroot's heart",
            MinLevel = 60,
            MaxLevel = 60,
            Type = ZoneType.Raid,
            ControllingFaction = Faction.Neutral,
            WorldrootDensity = 2.0f, // Beyond maximum
            IsPvPEnabled = false,
            LoreDescription = "Players can see the full Green Memory, including their own absence from early layers."
        }
    };

    public static Zone? GetZone(string zoneId)
    {
        return Zones.TryGetValue(zoneId, out var zone) ? zone : null;
    }
}
