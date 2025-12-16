using MessagePack;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Shared.Data.World;

/// <summary>
/// Represents a game zone/area in Eldara
/// </summary>
[MessagePackObject]
public class Zone
{
    [Key(0)]
    public string ZoneId { get; set; } = string.Empty;

    [Key(1)]
    public string Name { get; set; } = string.Empty;

    [Key(2)]
    public string Description { get; set; } = string.Empty;

    [Key(3)]
    public int MinLevel { get; set; } = 1;

    [Key(4)]
    public int MaxLevel { get; set; } = 10;

    [Key(5)]
    public ZoneType Type { get; set; }

    [Key(6)]
    public Faction ControllingFaction { get; set; } = Faction.Neutral;

    [Key(7)]
    public float WorldrootDensity { get; set; } = 0.5f; // 0.0 to 1.0

    [Key(8)]
    public bool IsPvPEnabled { get; set; }

    [Key(9)]
    public bool IsContestedTerritory { get; set; }

    [Key(10)]
    public List<string> ConnectedZones { get; set; } = new();

    [Key(11)]
    public WorldPosition SafeSpawnPoint { get; set; } = new();

    [Key(12)]
    public string LoreDescription { get; set; } = string.Empty;
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
    [Key(0)]
    public float X { get; set; }

    [Key(1)]
    public float Y { get; set; }

    [Key(2)]
    public float Z { get; set; }

    public WorldPosition(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

/// <summary>
/// Zone definitions with lore justification
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
            LoreDescription = "Trees remember every footstep. Younger Sylvaen train here to hear the Green Memory."
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
            LoreDescription = "Ruins phase in and out of existence. High Elves study Edit Nodes."
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
            LoreDescription = "Humans from all walks of life gather here, united by their mysterious nature."
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
