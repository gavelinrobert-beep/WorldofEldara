using Serilog;

namespace WorldofEldara.Server.World;

/// <summary>
/// Manages world time in Eldara.
/// 
/// Lore: Time in Eldara flows according to the Worldroot's perception.
/// One real day = One Eldara week (7 days).
/// Allows for day/night cycle and time-based events.
/// </summary>
public class TimeManager
{
    private const float RealSecondsPerEldaraDay = 24f * 60f * 60f / 7f; // ~3.43 hours per Eldara day
    private const float EldaraDaysPerRealSecond = 1f / RealSecondsPerEldaraDay;

    private float _elapsedEldaraDays;
    private DateTime _serverStartTime;

    public void Initialize()
    {
        _serverStartTime = DateTime.UtcNow;
        _elapsedEldaraDays = 0;

        Log.Information($"World time initialized. Current Eldara date: {GetCurrentWorldTime()}");
    }

    public void Update(float deltaTime)
    {
        _elapsedEldaraDays += deltaTime * EldaraDaysPerRealSecond;
    }

    /// <summary>
    /// Get current time of day (0.0 = midnight, 0.5 = noon, 1.0 = midnight again)
    /// </summary>
    public float GetTimeOfDay()
    {
        return _elapsedEldaraDays % 1.0f;
    }

    /// <summary>
    /// Check if it's currently day time (6am to 6pm Eldara time)
    /// </summary>
    public bool IsDaytime()
    {
        float timeOfDay = GetTimeOfDay();
        return timeOfDay >= 0.25f && timeOfDay < 0.75f; // 6am to 6pm
    }

    /// <summary>
    /// Get total elapsed Eldara days since server start
    /// </summary>
    public int GetElapsedDays()
    {
        return (int)_elapsedEldaraDays;
    }

    /// <summary>
    /// Get formatted world time string
    /// </summary>
    public string GetCurrentWorldTime()
    {
        int days = GetElapsedDays();
        float timeOfDay = GetTimeOfDay();
        int hours = (int)(timeOfDay * 24);
        int minutes = (int)((timeOfDay * 24 - hours) * 60);

        return $"Day {days}, {hours:D2}:{minutes:D2} ({(IsDaytime() ? "Day" : "Night")})";
    }

    /// <summary>
    /// Get Worldroot strain level based on time (lore: memory overflow increases over time)
    /// </summary>
    public float GetWorldrootStrainLevel()
    {
        // Base strain increases slowly over time (endgame narrative driver)
        float baseStrain = 0.3f + (_elapsedEldaraDays / 10000f); // Very slow increase

        // Fluctuates with time of day (memory is more active at night)
        float timeOfDay = GetTimeOfDay();
        float dailyVariance = IsDaytime() ? 0.0f : 0.1f;

        return Math.Min(baseStrain + dailyVariance, 1.0f);
    }
}
