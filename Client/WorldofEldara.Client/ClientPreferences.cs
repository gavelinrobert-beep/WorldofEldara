using System.Text.Json;

namespace WorldofEldara.Client;

public sealed class ClientPreferences
{
    private const int DefaultWindowWidth = 1280;
    private const int DefaultWindowHeight = 720;
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public int WindowWidth { get; set; } = DefaultWindowWidth;
    public int WindowHeight { get; set; } = DefaultWindowHeight;
    public float CameraZoom { get; set; } = 1f;
    public ulong? LastCharacterId { get; set; }

    public static ClientPreferences Load()
    {
        try
        {
            var path = GetPreferencePath();
            if (!File.Exists(path))
            {
                return new ClientPreferences();
            }

            return JsonSerializer.Deserialize<ClientPreferences>(File.ReadAllText(path), SerializerOptions)
                   ?? new ClientPreferences();
        }
        catch
        {
            return new ClientPreferences();
        }
    }

    public void Save()
    {
        try
        {
            var path = GetPreferencePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(this, SerializerOptions));
        }
        catch
        {
            // Preferences should never be able to break the game session.
        }
    }

    public Size GetWindowSize()
    {
        return new Size(
            Math.Clamp(WindowWidth, 960, 3840),
            Math.Clamp(WindowHeight, 540, 2160));
    }

    private static string GetPreferencePath()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(folder, "WorldofEldara", "client-preferences.json");
    }
}
