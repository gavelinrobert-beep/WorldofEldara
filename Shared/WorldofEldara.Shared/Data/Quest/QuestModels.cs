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
    public const int SapThatRemembersId = 10003;
    public const int HareThatDiedTwiceId = 10004;
    public const int ScarThatLearnsId = 10005;
    public const int SeedvaultThresholdId = 10006;
    public const int SeedvaultDescentId = 10007;
    public const int FootstepsInTheWoodId = 10008;
    public const int MossglassTestimonyQuestId = 10009;
    public const int NamesBeneathWaterId = 10010;
    public const int SilverInTheBarkId = 10011;
    public const int ScoutWhoShouldBeDeadId = 10012;
    public const int NoRootNamesThisWoundId = 10013;
    public const int ThreeKeysOfContinuityId = 10014;
    public const int SeedvaultVeyrId = 10015;
    public const int MemoryThatAsksToLiveId = 10016;
    public const int CircleDividedId = 10017;
    public const int WhatWillYouPreserveId = 10018;
    public const int RootRemembersChoiceId = 10019;
    public const int BeyondThornveilId = 10020;
    public const int VerdantOutskirtsCullId = 20001;
    public const int ChildRememberedTomorrowId = 20002;
    public const int ThreeNamesSameWolfId = 20003;
    public const int TraderBeneathFernsId = 20004;
    public const int PrayerWeakeningGodId = 20005;
    public const int RootsDoNotForgiveIronId = 20006;
    public const int BleedingRootBloomId = 21001;
    public const int AeltharBreachId = 21002;
    public const int NamelessPatchId = 21003;
    public const int BriarwatchWelcomeId = 30001;
    public const int BriarwatchSuppliesId = 30002;

    private static readonly IReadOnlyDictionary<int, QuestDefinition> Definitions =
        new Dictionary<int, QuestDefinition>
        {
            [AwakeningId] = new QuestDefinition
            {
                QuestId = AwakeningId,
                Title = "A Name for a Season",
                Description = "Elder Thaelir asks you to step into the Listener's Ring and let Memory Keeper Savaen test whether the Worldroot can hear your name.",
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
                        Description = "Speak with Memory Keeper Savaen in the Listener's Ring."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 250,
                    Gold = 5
                },
                AcceptDialogue = "The Worldroot is bleeding, and still it listens. Go to Savaen. Give the Ring your name and see what answers.",
                CompletionDialogue = "Good. The bark heard you. A thin beginning, but a true one."
            },
            [SapThatRemembersId] = new QuestDefinition
            {
                QuestId = SapThatRemembersId,
                Title = "Sap That Remembers",
                Description =
                    "Memory Keeper Savaen asks you to listen to three unstable echoes and compare what the Worldroot remembers against what the scar is trying to erase.",
                MinimumLevel = 1,
                IsMainStory = true,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1002,
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7001,
                        Description = "Listen to the Mossglass Testimony."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7002,
                        Description = "Read the Thornway Cutmark."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 3,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7003,
                        Description = "Touch the Green Scar Nullroot."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 320,
                    Gold = 6,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 25
                },
                AcceptDialogue =
                    "The sap is remembering more than one truth. Listen to the pool, the cutmark, and the nullroot. Bring me the pattern, not your fear.",
                CompletionDialogue =
                    "True memory, copied memory, missing memory. That is not rot. That is a hand learning our archive."
            },
            [HareThatDiedTwiceId] = new QuestDefinition
            {
                QuestId = HareThatDiedTwiceId,
                Title = "The Hare That Died Twice",
                Description =
                    "Druid Ylvhara has found a small creature whose death is being remembered before and after it happens.",
                MinimumLevel = 2,
                IsMainStory = true,
                GiverNpcTemplateId = 1003,
                TurnInNpcTemplateId = 1003,
                Prerequisites = new[] { SapThatRemembersId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 1,
                        TargetNpcTemplateId = 6003,
                        Description = "Defeat the Twice-Dead Hare near the Mossglass Pools."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7001,
                        Description = "Listen to the Mossglass Testimony after the hare falls."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 420,
                    Gold = 8,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 35
                },
                AcceptDialogue =
                    "A hare died beside the pools. Then it died again before I reached it. Find the echo wearing its shape and listen to what the water keeps.",
                CompletionDialogue =
                    "The same death, remembered twice, with different teeth marks. Something is editing the small things first."
            },
            [VerdantOutskirtsCullId] = new QuestDefinition
            {
                QuestId = VerdantOutskirtsCullId,
                Title = "First Pruning",
                Description = "Root Guardian Orenhusk wants the unstable growths around Heartbough Glade cut back before they learn to hunt in packs.",
                MinimumLevel = 1,
                GiverNpcTemplateId = 5001,
                TurnInNpcTemplateId = 5001,
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 5,
                        TargetNpcTemplateId = 6001,
                        TargetTag = "zone01_hostile",
                        Description = "Prune Hollow Saplings or Razor Ferns near Heartbough Glade."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 150
                },
                AcceptDialogue = "Do not hate the growth. It is sick, not evil. Cut cleanly, then return before the roots start copying your fear.",
                CompletionDialogue = "Clean cuts. The grove breathes easier for a little while. If the sickness regrows, we prune again."
            },
            [ScarThatLearnsId] = new QuestDefinition
            {
                QuestId = ScarThatLearnsId,
                Title = "The Scar That Learns",
                Description =
                    "Druid Ylvhara believes the Green Scar is learning from every creature and memory it touches. Test the scar-line, then carry the result to Elder Thaelir.",
                MinimumLevel = 3,
                IsMainStory = true,
                GiverNpcTemplateId = 1003,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { HareThatDiedTwiceId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7003,
                        Description = "Touch the Green Scar Nullroot at the Nullroot Verge."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 2,
                        TargetNpcTemplateId = 6002,
                        Description = "Cut down Razor Ferns that have grown along the scar-line."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 3,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1001,
                        Description = "Report the scar's response to Elder Thaelir."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 520,
                    Gold = 10,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 45
                },
                AcceptDialogue =
                    "The hare was not the wound. It was a lesson the wound copied. Touch the nullroot, cut the scar-fed ferns, then tell Thaelir what answered.",
                CompletionDialogue =
                    "So the scar listens, learns, and chooses what to repeat. Then we must decide what it is not allowed to hear next."
            },
            [SeedvaultThresholdId] = new QuestDefinition
            {
                QuestId = SeedvaultThresholdId,
                Title = "The Door Under the Roots",
                Description =
                    "Elder Thaelir wants the Seedvault Veyr tested before the grove risks opening a lower root-chamber.",
                MinimumLevel = 3,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 5001,
                Prerequisites = new[] { ScarThatLearnsId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 5001,
                        Description = "Ask Root Guardian Orenhusk about the Seedvault seal."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7004,
                        Description = "Inspect the Seedvault Gate at Seedvault Veyr."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 3,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 3,
                        TargetNpcTemplateId = 6004,
                        Description = "Clear Nullroot Sprouts gathered outside the seal."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 4,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7005,
                        Description = "Touch the Veyr Wardseal and listen for a response."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 620,
                    Gold = 12,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 55
                },
                AcceptDialogue =
                    "The scar is learning at the surface. The Seedvault remembers below it. Orenhusk will not open the way yet, but we can test whether the door is still ours.",
                CompletionDialogue =
                    "Good. The seal still answers the grove, but something is pressing from below. We have found the threshold of our first descent."
            },
            [SeedvaultDescentId] = new QuestDefinition
            {
                QuestId = SeedvaultDescentId,
                Title = "Below the Listening Door",
                Description =
                    "Root Guardian Orenhusk opens the outer threshold just enough for a short descent into Seedvault Veyr's first chamber.",
                MinimumLevel = 4,
                IsMainStory = true,
                GiverNpcTemplateId = 5001,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { SeedvaultThresholdId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7004,
                        Description = "Open the Seedvault Gate and step into the lower threshold."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 4,
                        TargetNpcTemplateId = 6005,
                        Description = "Cut down Ward-Eaten Rootlings in the Seedvault Antechamber."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 3,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 1,
                        TargetNpcTemplateId = 6006,
                        Description = "Defeat the Sealgnawer Matron guarding the memory core."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 4,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7006,
                        Description = "Read the Veyr Memory Core before the chamber seals again."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 760,
                    Gold = 16,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 70
                },
                AcceptDialogue =
                    "Only the first breath of the chamber, no deeper. If the door remembers you wrongly, leave. If the roots inside have teeth, cut quickly.",
                CompletionDialogue =
                    "The chamber is not lost, but it is occupied by a hunger that knows our wards. That is enough truth for one descent."
            },
            [FootstepsInTheWoodId] = new QuestDefinition
            {
                QuestId = FootstepsInTheWoodId,
                Title = "Footsteps in the Wood",
                Description =
                    "Initiate Vessa finds repeated footprints where no one has walked. Follow the echo before it learns a real path.",
                MinimumLevel = 2,
                IsMainStory = true,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1004,
                Prerequisites = new[] { HareThatDiedTwiceId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7007,
                        Description = "Read the Echo Footsteps near Whisperroot Paths."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 2,
                        TargetNpcTemplateId = 6007,
                        Description = "Disperse Rotbound Moths drawn to the repeated trail."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 3,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1004,
                        Description = "Tell Initiate Vessa what the footprints repeated."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 460,
                    Gold = 8,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 35
                },
                AcceptDialogue =
                    "A path has started walking itself. Vessa is brave enough to notice and too new to be afraid. Help her before the trail becomes a command.",
                CompletionDialogue =
                    "It walked the same panic until the moths believed it. Good. We break the smaller lies before they gather teeth."
            },
            [MossglassTestimonyQuestId] = new QuestDefinition
            {
                QuestId = MossglassTestimonyQuestId,
                Title = "Mossglass Testimony",
                Description =
                    "Vessa asks you to compare a pool reflection against the Oranyn memory it claims to preserve.",
                MinimumLevel = 4,
                IsMainStory = true,
                GiverNpcTemplateId = 1004,
                TurnInNpcTemplateId = 1002,
                Prerequisites = new[] { FootstepsInTheWoodId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7008,
                        Description = "Examine the Oranyn Reflection at Mossglass Pools."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 2,
                        TargetNpcTemplateId = 6009,
                        Description = "Defeat Drowned Echoes surfacing from the pools."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 620,
                    Gold = 12,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 45
                },
                AcceptDialogue =
                    "The water says Oranyn died cleanly. Water has lied before. Bring Savaen what the reflection cannot hide.",
                CompletionDialogue =
                    "The testimony is not false, but it is edited. That may be worse."
            },
            [NamesBeneathWaterId] = new QuestDefinition
            {
                QuestId = NamesBeneathWaterId,
                Title = "Names Beneath Water",
                Description =
                    "Savaen hears names under the Mossglass Pools. Pull them back before the scar learns how to erase them.",
                MinimumLevel = 4,
                IsMainStory = true,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1002,
                Prerequisites = new[] { MossglassTestimonyQuestId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7009,
                        Description = "Recover the Drowned Name Fragment."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 2,
                        TargetNpcTemplateId = 6010,
                        Description = "Still Mossglass Wraiths guarding the name-fragment."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 660,
                    Gold = 13,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 50
                },
                AcceptDialogue =
                    "Names are anchors. If the pools let one sink, follow it. If something answers with your voice, do not answer back.",
                CompletionDialogue =
                    "A drowned name is still a name. We have kept one thread from being cut."
            },
            [SilverInTheBarkId] = new QuestDefinition
            {
                QuestId = SilverInTheBarkId,
                Title = "Silver in the Bark",
                Description =
                    "Lysarien Vaelth claims the Aelthar pressure at Old Thornway can be read like a cut in living bark.",
                MinimumLevel = 5,
                IsMainStory = true,
                GiverNpcTemplateId = 1005,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { NamesBeneathWaterId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7011,
                        Description = "Inspect the Old Thornway Lens."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 3,
                        TargetNpcTemplateId = 6011,
                        Description = "Drive back Aelthar Scouts near Old Thornway."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 720,
                    Gold = 15,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 55
                },
                AcceptDialogue =
                    "Silver hands leave clean wounds. I know the shape. Look at the lens, then cut down the scouts before they learn our patrol names.",
                CompletionDialogue =
                    "Lysarien was right about the silver and wrong about how safe that makes him. We watch him, and the Thornway."
            },
            [ScoutWhoShouldBeDeadId] = new QuestDefinition
            {
                QuestId = ScoutWhoShouldBeDeadId,
                Title = "The Scout Who Should Be Dead",
                Description =
                    "A Verdant scout appears alive after a report of their death. The grove needs proof before it trusts the memory.",
                MinimumLevel = 5,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 1003,
                Prerequisites = new[] { SilverInTheBarkId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 2,
                        TargetNpcTemplateId = 6012,
                        Description = "Break Temporal Residue around the returned scout's trail."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1003,
                        Description = "Ask Druid Ylvhara what should have died."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 760,
                    Gold = 15,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 55
                },
                AcceptDialogue =
                    "A scout returned after we burned their death-token. Either grief lied, or time did. Ylvhara can tell which poison we are drinking.",
                CompletionDialogue =
                    "The scout was not spared. They were copied. That is not mercy."
            },
            [NoRootNamesThisWoundId] = new QuestDefinition
            {
                QuestId = NoRootNamesThisWoundId,
                Title = "No Root Names This Wound",
                Description =
                    "Nim Without-Echo marks a patch the Worldroot refuses to name. Test its edge without letting it test you back.",
                MinimumLevel = 6,
                IsMainStory = true,
                GiverNpcTemplateId = 1006,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { ScoutWhoShouldBeDeadId, ScarThatLearnsId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Interact,
                        TargetCount = 1,
                        TargetNpcTemplateId = 7012,
                        Description = "Touch the Nameless Patch at the Green Scar."
                    },
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 2,
                        ObjectiveType = QuestObjectiveType.Kill,
                        TargetCount = 3,
                        TargetNpcTemplateId = 6013,
                        Description = "Disperse Nameless Growth around the patch."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 840,
                    Gold = 17,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 65
                },
                AcceptDialogue =
                    "It has no name. Do not give it one. Touch the edge, cut the growth, then leave before it notices your outline.",
                CompletionDialogue =
                    "A wound without a root-name cannot be healed by habit. We will need the Seedvault's older memory."
            },
            [ThreeKeysOfContinuityId] = new QuestDefinition
            {
                QuestId = ThreeKeysOfContinuityId,
                Title = "Three Keys of Continuity",
                Description =
                    "Orenhusk needs three continuity keys aligned before Seedvault Veyr can be opened for a deeper descent.",
                MinimumLevel = 7,
                IsMainStory = true,
                GiverNpcTemplateId = 5001,
                TurnInNpcTemplateId = 5001,
                Prerequisites = new[] { NoRootNamesThisWoundId, SeedvaultThresholdId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7013, Description = "Attune the Growth Key." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7014, Description = "Attune the Memory Key." },
                    new QuestObjectiveDefinition { ObjectiveId = 3, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7015, Description = "Attune the Decay Key." }
                },
                Rewards = new QuestReward
                {
                    Experience = 900,
                    Gold = 18,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 70
                },
                AcceptDialogue =
                    "No living door opens to one truth. Growth, memory, decay. Touch each key, and do not let any one of them call itself master.",
                CompletionDialogue =
                    "The keys agree. That worries me more than if one had refused."
            },
            [SeedvaultVeyrId] = new QuestDefinition
            {
                QuestId = SeedvaultVeyrId,
                Title = "Seedvault Veyr",
                Description =
                    "The first mini-dungeon opens as a compressed run through old root-chambers and copied defenders.",
                MinimumLevel = 7,
                IsMainStory = true,
                GiverNpcTemplateId = 5001,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { ThreeKeysOfContinuityId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7016, Description = "Enter the Root Descent." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6016, Description = "Break Unchosen Branches in the first hall." },
                    new QuestObjectiveDefinition { ObjectiveId = 3, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 1, TargetNpcTemplateId = 6014, Description = "Defeat Amsa-Once, the copied guardian." },
                    new QuestObjectiveDefinition { ObjectiveId = 4, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7017, Description = "Read the Hall of Unchosen Branches." }
                },
                Rewards = new QuestReward
                {
                    Experience = 1040,
                    Gold = 22,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 85
                },
                AcceptDialogue =
                    "This is not yet a true dungeon, but it is a true warning. Step below, cut what must be cut, and bring back the shape of the old chamber.",
                CompletionDialogue =
                    "The Veyr remembers branches we never chose. Something below is asking which of them should live."
            },
            [MemoryThatAsksToLiveId] = new QuestDefinition
            {
                QuestId = MemoryThatAsksToLiveId,
                Title = "The Memory That Asks to Live",
                Description =
                    "A preserved Oranyn memory speaks from the Seedvault and asks not to be pruned.",
                MinimumLevel = 8,
                IsMainStory = true,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1005,
                Prerequisites = new[] { SeedvaultVeyrId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7018, Description = "Listen to the Oranyn Chorus Anchor." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 2, TargetNpcTemplateId = 6015, Description = "Silence hostile Chorus Fragments." }
                },
                Rewards = new QuestReward
                {
                    Experience = 1100,
                    Gold = 24,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 90
                },
                AcceptDialogue =
                    "A memory asking to live is either a person, a trap, or both. Listen before judgment. That is still our law.",
                CompletionDialogue =
                    "It speaks too clearly to dismiss and too strangely to trust. Good. You understand the problem."
            },
            [CircleDividedId] = new QuestDefinition
            {
                QuestId = CircleDividedId,
                Title = "Circle Divided",
                Description =
                    "The grove argues whether the Oranyn memory should be anchored, pruned, or studied by the Aelthar.",
                MinimumLevel = 9,
                IsMainStory = true,
                GiverNpcTemplateId = 1005,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { MemoryThatAsksToLiveId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Talk, TargetCount = 1, TargetNpcTemplateId = 1001, Description = "Hear Elder Thaelir's caution." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Talk, TargetCount = 1, TargetNpcTemplateId = 1003, Description = "Hear Druid Ylvhara's mercy." },
                    new QuestObjectiveDefinition { ObjectiveId = 3, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7019, Description = "Stand at the Circle Debate Root." }
                },
                Rewards = new QuestReward
                {
                    Experience = 1160,
                    Gold = 25,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 95
                },
                AcceptDialogue =
                    "Thaelir calls it a wound. Ylvhara calls it a life. I call it a question that learned our language. Hear them both.",
                CompletionDialogue =
                    "No circle is whole because everyone agrees. It is whole because the disagreement stays rooted."
            },
            [WhatWillYouPreserveId] = new QuestDefinition
            {
                QuestId = WhatWillYouPreserveId,
                Title = "What Will You Preserve?",
                Description =
                    "Consider the three Thornveil choices: anchor the memory, prune the memory, or let the Aelthar take a copy.",
                MinimumLevel = 10,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 1002,
                Prerequisites = new[] { CircleDividedId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7020, Description = "Study the Anchor the Memory choice." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7021, Description = "Study the Prune the Memory choice." },
                    new QuestObjectiveDefinition { ObjectiveId = 3, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7022, Description = "Study the Aelthar Copy Lens choice." }
                },
                Rewards = new QuestReward
                {
                    Experience = 1260,
                    Gold = 28,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 100
                },
                AcceptDialogue =
                    "One day this will be a true branching choice. Today, learn the weight of all three paths before you speak for the grove.",
                CompletionDialogue =
                    "You have looked at the choice without pretending it is simple. That is the first honest answer."
            },
            [RootRemembersChoiceId] = new QuestDefinition
            {
                QuestId = RootRemembersChoiceId,
                Title = "The Root Remembers the Choice",
                Description =
                    "The Worldroot marks the moment of decision so Thornveil can carry it forward into later zones.",
                MinimumLevel = 10,
                IsMainStory = true,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { WhatWillYouPreserveId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7023, Description = "Seal the decision at the Ceremony Root." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Talk, TargetCount = 1, TargetNpcTemplateId = 1001, Description = "Return to Elder Thaelir." }
                },
                Rewards = new QuestReward
                {
                    Experience = 1320,
                    Gold = 30,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 120
                },
                AcceptDialogue =
                    "A choice not remembered is only a mood. Let the root remember what you carry.",
                CompletionDialogue =
                    "Thornveil has a new memory now. Yours."
            },
            [BeyondThornveilId] = new QuestDefinition
            {
                QuestId = BeyondThornveilId,
                Title = "Beyond Thornveil",
                Description =
                    "Elder Thaelir sends you toward the next road, carrying Thornveil's unresolved answer into the wider world.",
                MinimumLevel = 10,
                IsMainStory = true,
                GiverNpcTemplateId = 1001,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { RootRemembersChoiceId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7024, Description = "Stand at the Road Beyond Thornveil." }
                },
                Rewards = new QuestReward
                {
                    Experience = 900,
                    Gold = 35,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 80
                },
                AcceptDialogue =
                    "The grove is not finished, but you are no longer only of the grove. Touch the road and listen to what waits past Thornveil.",
                CompletionDialogue =
                    "When the next zone opens, this is where the road begins."
            },
            [ChildRememberedTomorrowId] = new QuestDefinition
            {
                QuestId = ChildRememberedTomorrowId,
                Title = "The Child Who Remembered Tomorrow",
                Description =
                    "Vessa has seen tomorrow's campfire in yesterday's ash. Check the site before the memory becomes a prophecy.",
                MinimumLevel = 3,
                GiverNpcTemplateId = 1004,
                TurnInNpcTemplateId = 1004,
                Prerequisites = new[] { FootstepsInTheWoodId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7025, Description = "Inspect the Tomorrow Fire Site." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 2, TargetNpcTemplateId = 6012, Description = "Break the Temporal Residue around the ash." }
                },
                Rewards = new QuestReward { Experience = 480, Gold = 8, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 30 },
                AcceptDialogue = "I saw the ash before the fire. Please tell me that means something ordinary.",
                CompletionDialogue = "Not ordinary, then. But less alone."
            },
            [ThreeNamesSameWolfId] = new QuestDefinition
            {
                QuestId = ThreeNamesSameWolfId,
                Title = "Three Names of the Same Wolf",
                Description =
                    "Three wolf-memories circle one den, each answering to a different name.",
                MinimumLevel = 4,
                GiverNpcTemplateId = 1002,
                TurnInNpcTemplateId = 1002,
                Prerequisites = new[] { MossglassTestimonyQuestId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6008, Description = "Defeat Echo-Wolves near the memory den." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7026, Description = "Read the Wolf Memory Den." }
                },
                Rewards = new QuestReward { Experience = 560, Gold = 10, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 35 },
                AcceptDialogue = "A wolf can have many names. It should not have three deaths. Find the den.",
                CompletionDialogue = "Three names, one hunger. The archive is repeating itself with teeth."
            },
            [TraderBeneathFernsId] = new QuestDefinition
            {
                QuestId = TraderBeneathFernsId,
                Title = "The Trader Beneath the Ferns",
                Description =
                    "A hidden memory trader keeps a cache under the fernline and wants proof you can find what roots hide.",
                MinimumLevel = 4,
                GiverNpcTemplateId = 1007,
                TurnInNpcTemplateId = 1007,
                Prerequisites = new[] { SapThatRemembersId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7027, Description = "Find the Memory Trader Cache." }
                },
                Rewards = new QuestReward { Experience = 420, Gold = 18, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 20 },
                AcceptDialogue = "If you can find my cache, you can buy from it later. Fair root, fair trade.",
                CompletionDialogue = "Good eyes. Later, this becomes a proper vendor hook."
            },
            [PrayerWeakeningGodId] = new QuestDefinition
            {
                QuestId = PrayerWeakeningGodId,
                Title = "A Prayer for a Weakening God",
                Description =
                    "God-Seeker Irielle asks for a quiet prayer at a root-shrine losing its old answer.",
                MinimumLevel = 5,
                GiverNpcTemplateId = 1008,
                TurnInNpcTemplateId = 1008,
                Prerequisites = new[] { NoRootNamesThisWoundId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7028, Description = "Speak a prayer at the Weakening Prayer Stone." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 2, TargetNpcTemplateId = 6013, Description = "Clear Nameless Growth around the shrine." }
                },
                Rewards = new QuestReward { Experience = 620, Gold = 12, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 40 },
                AcceptDialogue = "A god does not have to be strong to deserve a gentle voice. Help me keep this one from fading alone.",
                CompletionDialogue = "The stone answered softly. Softly is still an answer."
            },
            [RootsDoNotForgiveIronId] = new QuestDefinition
            {
                QuestId = RootsDoNotForgiveIronId,
                Title = "Roots Do Not Forgive Iron",
                Description =
                    "Barksmith Harth wants Aelthar iron cut from living bark before it teaches the roots to scar shut.",
                MinimumLevel = 5,
                GiverNpcTemplateId = 1009,
                TurnInNpcTemplateId = 1009,
                Prerequisites = new[] { SilverInTheBarkId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6011, Description = "Defeat Aelthar Scouts carrying silver-iron hooks." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7029, Description = "Clean the Living Bark Forge." }
                },
                Rewards = new QuestReward { Experience = 680, Gold = 14, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 45 },
                AcceptDialogue = "Iron remembers impact. Root remembers injury. I need the two kept apart.",
                CompletionDialogue = "The forge breathes again. Later, this should become crafting."
            },
            [BleedingRootBloomId] = new QuestDefinition
            {
                QuestId = BleedingRootBloomId,
                Title = "Public Event: Bleeding Root Bloom",
                Description =
                    "A root-bloom opens and leaks corrupted sap until the grove cuts back the growth around it.",
                MinimumLevel = 4,
                IsRepeatable = true,
                GiverNpcTemplateId = 1003,
                TurnInNpcTemplateId = 1003,
                Prerequisites = new[] { FootstepsInTheWoodId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7030, Description = "Seal the Bleeding Root Bloom." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6007, Description = "Clear Rotbound Moths feeding on the bloom." }
                },
                Rewards = new QuestReward { Experience = 540, Gold = 10, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 25 },
                AcceptDialogue = "The bloom opens when the grove is stressed. Treat this like a public event placeholder: seal it, then cut the feeders.",
                CompletionDialogue = "The bloom closes. Thornveil breathes easier."
            },
            [AeltharBreachId] = new QuestDefinition
            {
                QuestId = AeltharBreachId,
                Title = "Public Event: Aelthar Breach",
                Description =
                    "A brief silver breach tests the Old Thornway defenses.",
                MinimumLevel = 6,
                IsRepeatable = true,
                GiverNpcTemplateId = 1005,
                TurnInNpcTemplateId = 1005,
                Prerequisites = new[] { SilverInTheBarkId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7031, Description = "Shatter the Aelthar Breach Lens." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6011, Description = "Defeat Aelthar Scouts pushing through the breach." }
                },
                Rewards = new QuestReward { Experience = 720, Gold = 14, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 35 },
                AcceptDialogue = "The silver edge opens quickly. Close it quicker.",
                CompletionDialogue = "The breach shuts. They will learn from this. So will we."
            },
            [NamelessPatchId] = new QuestDefinition
            {
                QuestId = NamelessPatchId,
                Title = "Public Event: The Nameless Patch",
                Description =
                    "A patch of ground loses its name and tries to pull nearby memories into the blank.",
                MinimumLevel = 6,
                IsRepeatable = true,
                GiverNpcTemplateId = 1006,
                TurnInNpcTemplateId = 1006,
                Prerequisites = new[] { NoRootNamesThisWoundId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition { ObjectiveId = 1, ObjectiveType = QuestObjectiveType.Interact, TargetCount = 1, TargetNpcTemplateId = 7032, Description = "Recover the Nameless Patch Names." },
                    new QuestObjectiveDefinition { ObjectiveId = 2, ObjectiveType = QuestObjectiveType.Kill, TargetCount = 3, TargetNpcTemplateId = 6013, Description = "Disperse Nameless Growth around the blank." }
                },
                Rewards = new QuestReward { Experience = 740, Gold = 14, ReputationFaction = Faction.VerdantCircles, ReputationAmount = 35 },
                AcceptDialogue = "Do not name the patch. Gather the names it stole.",
                CompletionDialogue = "Some names return. Some do not. That is enough for the event version."
            },
            [WorldrootsPainId] = new QuestDefinition
            {
                QuestId = WorldrootsPainId,
                Title = "The Listening Bark",
                Description =
                    "Elder Thaelir sends you to Druid Ylvhara, who has found bark that repeats a memory in the wrong order.",
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
                        Description = "Listen to Druid Ylvhara's report near the older roots."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 400,
                    Gold = 10,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 50
                },
                AcceptDialogue = "The bark repeats a death before the death happens. Find Ylvhara and listen carefully.",
                CompletionDialogue = "A memory out of order is a wound with intent. We will need more than pruning."
            },
            [BriarwatchWelcomeId] = new QuestDefinition
            {
                QuestId = BriarwatchWelcomeId,
                Title = "Welcome to Briarwatch",
                Description = "Keeper Aelwyn wants you to meet the quartermaster overseeing the Crossing's supply lines.",
                MinimumLevel = 6,
                GiverNpcTemplateId = 1101,
                TurnInNpcTemplateId = 1102,
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1102,
                        Description = "Speak with Quartermaster Liora in Briarwatch Crossing."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 320,
                    Gold = 8,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 20
                },
                AcceptDialogue = "We hold this Crossing for all who respect the Worldroot. Quartermaster Liora will outfit you.",
                CompletionDialogue = "Keeper Aelwyn keeps the roots calm. Let's get you supplied for the routes ahead."
            },
            [BriarwatchSuppliesId] = new QuestDefinition
            {
                QuestId = BriarwatchSuppliesId,
                Title = "Bridge to Thornveil",
                Description =
                    "Quartermaster Liora has packaged aid for Thornveil Enclave. Deliver it to Elder Tharivol.",
                MinimumLevel = 6,
                GiverNpcTemplateId = 1102,
                TurnInNpcTemplateId = 1001,
                Prerequisites = new[] { BriarwatchWelcomeId },
                Objectives = new[]
                {
                    new QuestObjectiveDefinition
                    {
                        ObjectiveId = 1,
                        ObjectiveType = QuestObjectiveType.Talk,
                        TargetCount = 1,
                        TargetNpcTemplateId = 1001,
                        Description = "Bring the sealed satchel to Elder Tharivol in Thornveil Enclave."
                    }
                },
                Rewards = new QuestReward
                {
                    Experience = 420,
                    Gold = 12,
                    ReputationFaction = Faction.VerdantCircles,
                    ReputationAmount = 35
                },
                AcceptDialogue =
                    "Our bridges stand because Thornveil stands. Carry this satchel to Elder Tharivol and let the Enclave know the Crossing is secure.",
                CompletionDialogue = "So Briarwatch stands ready. Their vigilance will keep the branches open."
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
            d.TurnInNpcTemplateId == npcTemplateId);
    }
}
