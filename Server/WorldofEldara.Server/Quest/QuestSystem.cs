using System.Collections.Concurrent;
using WorldofEldara.Server.Core;
using System.Linq;
using System;
using WorldofEldara.Shared.Constants;
using WorldofEldara.Shared.Data.Character;
using WorldofEldara.Shared.Data.Quest;
using WorldofEldara.Shared.Protocol;
using WorldofEldara.Shared.Protocol.Packets;

namespace WorldofEldara.Server.Quest;

/// <summary>
///     Server-authoritative quest system. Tracks per-player quest states, evaluates prerequisites,
///     applies progression triggers, and builds dialogue responses for quest givers.
/// </summary>
public class QuestSystem
{
    private const string FallbackNpcPrefix = "NPC_";
    private readonly ConcurrentDictionary<ulong, PlayerQuestLog> _playerQuests = new();
    private readonly Func<long> _serverTimeProvider;

    public QuestSystem(Func<long> serverTimeProvider)
    {
        _serverTimeProvider = serverTimeProvider;
    }

    private PlayerQuestLog GetLog(ulong characterId)
    {
        return _playerQuests.GetOrAdd(characterId, _ => new PlayerQuestLog());
    }

    public IReadOnlyList<QuestStateData> GetQuestStates(ulong characterId)
    {
        var log = GetLog(characterId);
        lock (log.Sync)
        {
            return log.Quests.Values
                .Where(state => !log.RewardedQuestIds.Contains(state.QuestId))
                .Select(CloneState)
                .ToList();
        }
    }

    public IReadOnlyList<QuestDefinition> GetDefinitionsForStates(IEnumerable<QuestStateData> states)
    {
        return QuestCatalog.GetDefinitions(states.Select(s => s.QuestId));
    }

    public QuestAcceptResult AcceptQuest(PlayerEntity player, int questId)
    {
        QuestDefinition definition;
        try
        {
            definition = QuestCatalog.Get(questId);
        }
        catch (KeyNotFoundException)
        {
            return new QuestAcceptResult(ResponseCode.NotFound, "Quest not found", null, null);
        }

        var log = GetLog(player.CharacterData.CharacterId);
        lock (log.Sync)
        {
            if (log.Quests.TryGetValue(questId, out var existing))
            {
                if (existing.State == QuestState.Active)
                    return new QuestAcceptResult(ResponseCode.AlreadyExists, "Quest already active", CloneState(existing),
                        definition);
                if (existing.State == QuestState.Completed && !log.RewardedQuestIds.Contains(questId))
                    return new QuestAcceptResult(ResponseCode.AlreadyExists, "Quest ready for turn-in",
                        CloneState(existing), definition);
                if (existing.State == QuestState.Completed && !definition.IsRepeatable)
                    return new QuestAcceptResult(ResponseCode.AlreadyExists, "Quest already completed",
                        CloneState(existing), definition);
            }

            if (!CanAccept(definition, player, log))
                return new QuestAcceptResult(ResponseCode.InvalidRequest, "Prerequisites not met or level too low",
                    null, definition);

            var now = DateTimeOffset.FromUnixTimeMilliseconds(_serverTimeProvider()).UtcDateTime;
            var newState = new QuestStateData
            {
                QuestId = definition.QuestId,
                State = QuestState.Active,
                Objectives = definition.Objectives
                    .Select(o => new QuestObjectiveProgress
                    {
                        ObjectiveId = o.ObjectiveId,
                        Current = 0,
                        Target = Math.Max(1, o.TargetCount),
                        Completed = o.TargetCount <= 0
                    })
                    .ToList(),
                AcceptedAt = now
            };

            log.Quests[questId] = newState;
            log.RewardedQuestIds.Remove(questId);
            return new QuestAcceptResult(ResponseCode.Success, "Quest accepted", CloneState(newState), definition);
        }
    }

    public QuestTurnInResult TurnInQuest(PlayerEntity player, int questId, int? npcTemplateId)
    {
        QuestDefinition definition;
        try
        {
            definition = QuestCatalog.Get(questId);
        }
        catch (KeyNotFoundException)
        {
            return new QuestTurnInResult(ResponseCode.NotFound, "Quest not found", null, null);
        }

        var log = GetLog(player.CharacterData.CharacterId);
        lock (log.Sync)
        {
            if (!log.Quests.TryGetValue(questId, out var state))
                return new QuestTurnInResult(ResponseCode.NotFound, "Quest is not in your log", null, definition);

            if (state.State != QuestState.Completed)
                return new QuestTurnInResult(ResponseCode.InvalidRequest, "Quest objectives are not complete",
                    CloneState(state), definition);

            if (definition.TurnInNpcTemplateId.HasValue && npcTemplateId != definition.TurnInNpcTemplateId.Value)
                return new QuestTurnInResult(ResponseCode.InvalidRequest, "Wrong turn-in NPC", CloneState(state),
                    definition);

            if (log.RewardedQuestIds.Contains(questId))
                return new QuestTurnInResult(ResponseCode.AlreadyExists, "Quest already turned in", CloneState(state),
                    definition);

            ApplyRewards(definition, player);
            log.RewardedQuestIds.Add(questId);

            return new QuestTurnInResult(ResponseCode.Success, "Quest completed", CloneState(state), definition);
        }
    }

    public QuestPackets.QuestDialogueResponse BuildDialogue(PlayerEntity player, NPCEntity? npc,
        int? npcTemplateOverride = null)
    {
        var npcTemplateId = npcTemplateOverride ?? npc?.NPCTemplateId;
        var npcName = npc?.Name ?? GetWorldObjectName(npcTemplateId);

        var updatedStates = npcTemplateId.HasValue
            ? ApplyDialogueProgress(player, npcTemplateId.Value)
            : Array.Empty<QuestStateData>();

        var options = new List<QuestDialogueOption>();
        if (npcTemplateId.HasValue)
        {
            var log = GetLog(player.CharacterData.CharacterId);
            lock (log.Sync)
            {
                foreach (var offer in QuestCatalog.GetByGiver(npcTemplateId.Value))
                    if (CanAccept(offer, player, log))
                        options.Add(new QuestDialogueOption
                        {
                            Type = DialogueOptionType.OfferQuest,
                            QuestId = offer.QuestId,
                            Quest = offer,
                            Text = offer.AcceptDialogue ?? $"Accept \"{offer.Title}\""
                        });

                foreach (var state in log.Quests.Values.Where(q =>
                             q.State == QuestState.Completed && !log.RewardedQuestIds.Contains(q.QuestId)))
                {
                    var definition = QuestCatalog.Get(state.QuestId);
                    if (definition.TurnInNpcTemplateId == npcTemplateId.Value)
                        options.Add(new QuestDialogueOption
                        {
                            Type = DialogueOptionType.TurnInQuest,
                            QuestId = definition.QuestId,
                            Quest = definition,
                            Text = definition.CompletionDialogue ?? $"Complete \"{definition.Title}\""
                        });
                }
            }
        }

        return new QuestPackets.QuestDialogueResponse
        {
            NpcEntityId = npc?.EntityId,
            NpcName = npcName,
            Options = options,
            UpdatedStates = updatedStates
        };
    }

    private static string GetWorldObjectName(int? npcTemplateId)
    {
        return npcTemplateId switch
        {
            7001 => "Mossglass Testimony",
            7002 => "Thornway Cutmark",
            7003 => "Green Scar Nullroot",
            7004 => "Seedvault Gate",
            7005 => "Veyr Wardseal",
            7006 => "Veyr Memory Core",
            7007 => "Echo Footsteps",
            7008 => "Oranyn Reflection",
            7009 => "Drowned Name Fragment",
            7011 => "Old Thornway Lens",
            7012 => "Nameless Patch",
            7013 => "Growth Key",
            7014 => "Memory Key",
            7015 => "Decay Key",
            7016 => "Root Descent",
            7017 => "Hall of Unchosen Branches",
            7018 => "Oranyn Chorus Anchor",
            7019 => "Circle Debate Root",
            7020 => "Anchor the Memory",
            7021 => "Prune the Memory",
            7022 => "Aelthar Copy Lens",
            7023 => "Ceremony Root",
            7024 => "Road Beyond Thornveil",
            7025 => "Tomorrow Fire Site",
            7026 => "Wolf Memory Den",
            7027 => "Memory Trader Cache",
            7028 => "Weakening Prayer Stone",
            7029 => "Living Bark Forge",
            7030 => "Bleeding Root Bloom",
            7031 => "Aelthar Breach Lens",
            7032 => "Nameless Patch Names",
            _ => npcTemplateId.HasValue ? $"{FallbackNpcPrefix}{npcTemplateId}" : "Unknown NPC"
        };
    }

    public IReadOnlyList<QuestProgressResult> RegisterKill(PlayerEntity player, NPCEntity npc)
    {
        if (player == null || npc == null) return Array.Empty<QuestProgressResult>();

        var log = GetLog(player.CharacterData.CharacterId);
        var updates = new List<QuestProgressResult>();

        lock (log.Sync)
        {
            foreach (var (questId, state) in log.Quests.ToList())
            {
                if (state.State != QuestState.Active) continue;

                var definition = QuestCatalog.Get(questId);
                var matchingObjectives = definition.Objectives
                    .Where(o => ObjectiveMatchesKill(definition, o, npc))
                    .ToList();

                if (!matchingObjectives.Any()) continue;

                var matchingIds = matchingObjectives.Select(o => o.ObjectiveId).ToHashSet();
                var progressedObjectives = state.Objectives
                    .Select(progress =>
                    {
                        if (!matchingIds.Contains(progress.ObjectiveId) || progress.Completed)
                            return progress;

                        var current = Math.Min(progress.Target, progress.Current + 1);
                        return progress with { Current = current, Completed = current >= progress.Target };
                    })
                    .ToList();

                var isComplete = AllRequiredObjectivesComplete(progressedObjectives, definition);
                var updatedState = state with
                {
                    Objectives = progressedObjectives,
                    State = isComplete ? QuestState.Completed : QuestState.Active,
                    CompletedAt = isComplete
                        ? DateTimeOffset.FromUnixTimeMilliseconds(_serverTimeProvider()).UtcDateTime
                        : state.CompletedAt
                };

                log.Quests[questId] = updatedState;

                updates.Add(new QuestProgressResult(CloneState(updatedState), definition));
            }
        }

        return updates;
    }

    private static bool ObjectiveMatchesKill(QuestDefinition definition, QuestObjectiveDefinition objective, NPCEntity npc)
    {
        if (objective.ObjectiveType != QuestObjectiveType.Kill)
        {
            return false;
        }

        if (objective.TargetNpcTemplateId == npc.NPCTemplateId)
        {
            return true;
        }

        if (definition.QuestId == QuestCatalog.HareThatDiedTwiceId &&
            objective.ObjectiveId == 1 &&
            (npc.NPCTemplateId == 6003 ||
             string.Equals(npc.Tag, "memory_anomaly", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(objective.TargetTag) && !string.IsNullOrWhiteSpace(npc.Tag) &&
            string.Equals(objective.TargetTag, npc.Tag, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (definition.QuestId == QuestCatalog.VerdantOutskirtsCullId && objective.ObjectiveId == 1)
        {
            return npc.NPCTemplateId is 6001 or 6002 ||
                   string.Equals(npc.Tag, "zone01_hostile", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(npc.ZoneId, ZoneConstants.Zone01, StringComparison.OrdinalIgnoreCase) &&
                   npc.IsHostile &&
                   (npc.Name.Contains("Sapling", StringComparison.OrdinalIgnoreCase) ||
                    npc.Name.Contains("Fern", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private IReadOnlyList<QuestStateData> ApplyDialogueProgress(PlayerEntity player, int npcTemplateId)
    {
        var log = GetLog(player.CharacterData.CharacterId);
        var updates = new List<QuestStateData>();

        lock (log.Sync)
        {
            foreach (var (questId, state) in log.Quests.ToList())
            {
                if (state.State != QuestState.Active) continue;

                var definition = QuestCatalog.Get(questId);
                var matchingObjectives = definition.Objectives
                    .Where(o =>
                        (o.ObjectiveType == QuestObjectiveType.Talk || o.ObjectiveType == QuestObjectiveType.Interact) &&
                        o.TargetNpcTemplateId == npcTemplateId)
                    .ToList();

                if (!matchingObjectives.Any()) continue;

                var matchingObjectiveIds = matchingObjectives.Select(o => o.ObjectiveId).ToHashSet();

                var progressedObjectives = state.Objectives
                    .Select(progress =>
                    {
                        if (matchingObjectiveIds.Contains(progress.ObjectiveId) && !progress.Completed)
                        {
                            var current = Math.Min(progress.Target, progress.Current + 1);
                            return progress with { Current = current, Completed = current >= progress.Target };
                        }

                        return progress;
                    })
                    .ToList();

                var isComplete = AllRequiredObjectivesComplete(progressedObjectives, definition);
                var updatedState = state with
                {
                    Objectives = progressedObjectives,
                    State = isComplete ? QuestState.Completed : QuestState.Active,
                    CompletedAt = isComplete
                        ? DateTimeOffset.FromUnixTimeMilliseconds(_serverTimeProvider()).UtcDateTime
                        : state.CompletedAt
                };

                log.Quests[questId] = updatedState;
                updates.Add(CloneState(updatedState));
            }
        }

        return updates;
    }

    private static bool AllRequiredObjectivesComplete(IEnumerable<QuestObjectiveProgress> progress,
        QuestDefinition definition)
    {
        var objectivesById = definition.Objectives.ToDictionary(o => o.ObjectiveId);

        return progress.All(p =>
        {
            var objective = objectivesById[p.ObjectiveId];
            return p.Completed || objective.Optional;
        });
    }

    private static void ApplyRewards(QuestDefinition definition, PlayerEntity player)
    {
        if (definition.Rewards.Experience > 0)
            player.CharacterData.ExperiencePoints += definition.Rewards.Experience;
        if (definition.Rewards.Gold > 0)
            player.CharacterData.Gold += definition.Rewards.Gold;
        if (definition.Rewards.ReputationFaction is Faction faction && definition.Rewards.ReputationAmount != 0)
        {
            player.CharacterData.FactionStandings.TryGetValue(faction, out var standing);
            player.CharacterData.FactionStandings[faction] = standing + definition.Rewards.ReputationAmount;
        }
    }

    private static QuestStateData CloneState(QuestStateData state)
    {
        return new QuestStateData
        {
            QuestId = state.QuestId,
            State = state.State,
            Objectives = state.Objectives
                .Select(o => new QuestObjectiveProgress
                {
                    ObjectiveId = o.ObjectiveId,
                    Current = o.Current,
                    Target = o.Target,
                    Completed = o.Completed
                })
                .ToList(),
            AcceptedAt = state.AcceptedAt,
            CompletedAt = state.CompletedAt
        };
    }

    private static bool CanAccept(QuestDefinition definition, PlayerEntity player, PlayerQuestLog log)
    {
        if (player.CharacterData.Level < definition.MinimumLevel)
            return false;

        if (log.Quests.TryGetValue(definition.QuestId, out var existing))
        {
            if (existing.State == QuestState.Active) return false;
            if (existing.State == QuestState.Completed && !log.RewardedQuestIds.Contains(definition.QuestId))
                return false;
            if (existing.State == QuestState.Completed && !definition.IsRepeatable) return false;
        }

        var prerequisitesMet = definition.Prerequisites.All(id =>
            log.Quests.TryGetValue(id, out var prereqState) && prereqState.State == QuestState.Completed);

        return prerequisitesMet;
    }

    private sealed class PlayerQuestLog
    {
        public Dictionary<int, QuestStateData> Quests { get; } = new();
        public HashSet<int> RewardedQuestIds { get; } = new();
        public object Sync { get; } = new();
    }
}

public sealed record QuestAcceptResult(
    ResponseCode Code,
    string Message,
    QuestStateData? State,
    QuestDefinition? Definition);

public sealed record QuestTurnInResult(
    ResponseCode Code,
    string Message,
    QuestStateData? State,
    QuestDefinition? Definition);

public sealed record QuestProgressResult(QuestStateData State, QuestDefinition Definition);
