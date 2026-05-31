namespace WorldofEldara.Shared.Data.Character;

public static class Leveling
{
    public const int MaxPrototypeLevel = 12;

    public static long GetExperienceRequiredForLevel(int level)
    {
        if (level <= 1)
        {
            return 0;
        }

        long total = 0;
        for (var currentLevel = 1; currentLevel < level; currentLevel++)
        {
            total += GetExperienceRequiredForNextLevel(currentLevel);
        }

        return total;
    }

    public static long GetExperienceRequiredForNextLevel(int currentLevel)
    {
        currentLevel = Math.Max(1, currentLevel);
        return 50 + currentLevel * 35L + Math.Max(0, currentLevel - 1) * 15L;
    }

    public static long GetExperienceRequiredForNextLevel(CharacterData character)
    {
        return character.Level >= MaxPrototypeLevel
            ? GetExperienceRequiredForLevel(MaxPrototypeLevel)
            : GetExperienceRequiredForLevel(character.Level + 1);
    }
}
