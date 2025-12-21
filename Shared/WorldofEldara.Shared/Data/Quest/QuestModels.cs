using MessagePack;
using WorldofEldara.Shared.Data.Character;

namespace WorldofEldara.Shared.Data.Quest;

public enum QuestState : byte
{
    Inactive = 0,
    Available = 1,
    Active = 2,
    Completed = 3,
    Failed = 4
}

public enum QuestObjectiveType : byte
{
    Kill = 0,
    Collect = 1,
    Interact = 2,
    Explore = 3,
    Escort = 4,
    Defend = 5,
    Talk = 6
}

public enum DialogueOptionType : byte
{
    Gossip = 0,
    OfferQuest = 1,
    TurnInQuest = 2,
    ProgressUpdate = 3
}

[MessagePackObject]
public sealed record QuestObjectiveDefinition
{
    [Key(0)] public int ObjectiveId { get; init; }

    [Key(1)] public QuestObjectiveType ObjectiveType { get; init; }

    [Key(2)] public string Description { get; init; } = string.Empty;

    [Key(3)] public int TargetCount { get; init; } = 1;

    [Key(4)] public int? TargetNpcTemplateId { get; init; }

    [Key(5)] public string? TargetTag { get; init; }

    [Key(6)] public bool Optional { get; init; }
}

[MessagePackObject]
public sealed record QuestReward
{
    [Key(0)] public int Experience { get; init; }

    [Key(1)] public int Gold { get; init; }

    [Key(2)] public Faction? ReputationFaction { get; init; }

    [Key(3)] public int ReputationAmount { get; init; }
}

[MessagePackObject]
public sealed record QuestDefinition
{
    [Key(0)] public int QuestId { get; init; }

    [Key(1)] public string Title { get; init; } = string.Empty;

    [Key(2)] public string Description { get; init; } = string.Empty;

    [Key(3)] public int MinimumLevel { get; init; } = 1;

    [Key(4)] public bool IsRepeatable { get; init; }

    [Key(5)] public bool IsMainStory { get; init; }

    [Key(6)] public IReadOnlyList<int> Prerequisites { get; init; } = Array.Empty<int>();

    [Key(7)] public IReadOnlyList<QuestObjectiveDefinition> Objectives { get; init; } =
        Array.Empty<QuestObjectiveDefinition>();

    [Key(8)] public QuestReward Rewards { get; init; } = new();

    [Key(9)] public int? GiverNpcTemplateId { get; init; }

    [Key(10)] public int? TurnInNpcTemplateId { get; init; }

    [Key(11)] public string? AcceptDialogue { get; init; }

    [Key(12)] public string? CompletionDialogue { get; init; }
}

[MessagePackObject]
public sealed record QuestObjectiveProgress
{
    [Key(0)] public int ObjectiveId { get; init; }

    [Key(1)] public int Current { get; init; }

    [Key(2)] public int Target { get; init; }

    [Key(3)] public bool Completed { get; init; }
}

[MessagePackObject]
public sealed record QuestStateData
{
    [Key(0)] public int QuestId { get; init; }

    [Key(1)] public QuestState State { get; init; }

    [Key(2)] public IReadOnlyList<QuestObjectiveProgress> Objectives { get; init; } =
        Array.Empty<QuestObjectiveProgress>();

    [Key(3)] public DateTime? AcceptedAt { get; init; }

    [Key(4)] public DateTime? CompletedAt { get; init; }
}

[MessagePackObject]
public sealed record QuestDialogueOption
{
    [Key(0)] public DialogueOptionType Type { get; init; }

    [Key(1)] public string Text { get; init; } = string.Empty;

    [Key(2)] public int? QuestId { get; init; }

    [Key(3)] public QuestDefinition? Quest { get; init; }
}

public static class QuestCatalog
{
    public const int AwakeningId = 10001;
    public const int WorldrootsPainId = 10002;

    private static readonly IReadOnlyDictionary<int, QuestDefinition> Definitions =
        new Dictionary<int, QuestDefinition>
        {
            [AwakeningId] = new QuestDefinition
            {
                QuestId = AwakeningId,
                Title = "Awakening",
                Description = "Answer Elder Tharivol's summons and speak with Instructor Lethril to begin your path.",
                MinimumLevel = 1,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 1002,
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1002,
                        Description = "Speak with Instructor Lethril."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 250,
                    Gold = 5
                },
                AcceptDialogue = "The worldroot stirs. Go, meet Instructor Lethril and steel yourself."
            },
            [WorldrootsPainId] = new QuestDefinition
            {
                QuestId = WorldrootsPainId,
                Title = "The Worldroot's Pain",
                Description =
                    "Elder Tharivol needs fresh eyes on the corruption. Meet Scout Maerith at the breach and report back.",
                MinimumLevel = 1,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { AwakeningId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1003,
                        Description = "Hear Scout Maerith's report."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 400,
                    Gold = 10,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 50
                },
                AcceptDialogue = "The Worldroot aches. Learn what Scout Maerith has seen.",
                CompletionDialogue = "Maerith's report is grim. We must act before the rot spreads further."
            }
        };

    public static QuestDefinition Get(int questId)
    {
        if (!Definitions.TryGetValue(questId, out var def))
            throw new KeyNotFoundException($"Quest definition {questId} not found");

        return def;
    }

    public static IReadOnlyList<QuestDefinition> GetByGiver(int npcTemplateId)
    {
        return Definitions.Values
            .Where(d => d.GiverNpcTemplateId == npcTemplateId)
            .ToList();
    }

    public static IReadOnlyList<QuestDefinition> GetByTurnIn(int npcTemplateId)
    {
        return Definitions.Values
            .Where(d => d.TurnInNpcTemplateId == npcTemplateId)
            .ToList();
    }

    public static IReadOnlyList<QuestDefinition> GetDefinitions(IEnumerable<int> questIds)
    {
        return questIds.Distinct()
            .Select(Get)
            .ToList();
    }

    public static bool IsQuestNpc(int npcTemplateId)
    {
        return Definitions.Values.Any(d =>
            d.GiverNpcTemplateId == npcTemplateId ||
            d.TurnInNpcTemplateId == npcTemplateId ||
            d.Objectives.Any(o => o.TargetNpcTemplateId == npcTemplateId));
    }
}
