using Serilog;
using WorldofEldara.Shared.Data.World;

namespace WorldofEldara.Server.World;

/// <summary>
///     Manages all game zones and their state
/// </summary>
public class ZoneManager
{
    private readonly Dictionary<string, Zone> _loadedZones = new();

    public async Task LoadZones()
    {
        Log.Information("Loading zone definitions...");

        // Load all zones from static definitions
        foreach (var kvp in ZoneDefinitions.Zones)
        {
            _loadedZones[kvp.Key] = kvp.Value;
            Log.Debug($"Loaded zone: {kvp.Value.Name} ({kvp.Key})");
        }

        // TODO: Load additional zone data from database or files

        await Task.CompletedTask;
    }

    public Zone? GetZone(string zoneId)
    {
        _loadedZones.TryGetValue(zoneId, out var zone);
        return zone;
    }

    public int GetLoadedZoneCount()
    {
        return _loadedZones.Count;
    }

    public List<Zone> GetAllZones()
    {
        return _loadedZones.Values.ToList();
    }

    /// <summary>
    ///     Check if a position is within zone bounds (simplified - would need proper bounds checking)
    /// </summary>
    public bool IsPositionInZone(string zoneId, float x, float y, float z)
    {
        // TODO: Implement proper zone boundary checking
        return _loadedZones.ContainsKey(zoneId);
    }
}