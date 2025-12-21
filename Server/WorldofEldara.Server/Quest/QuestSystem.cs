using System.Collections.Concurrent;
using WorldofEldara.Server.Core;
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
            return log.Quests.Values.Select(CloneState).ToList();
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
            return new QuestAcceptResult(ResponseCode.Success, "Quest accepted", CloneState(newState), definition);
        }
    }

    public QuestPackets.QuestDialogueResponse BuildDialogue(PlayerEntity player, NPCEntity? npc,
        int? npcTemplateOverride = null)
    {
        var npcTemplateId = npcTemplateOverride ?? npc?.NPCTemplateId;
        var npcName = npc?.Name ?? (npcTemplateId.HasValue ? $"NPC_{npcTemplateId}" : "Unknown NPC");

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

                foreach (var state in log.Quests.Values.Where(q => q.State == QuestState.Completed))
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

                var progressedObjectives = state.Objectives
                    .Select(progress =>
                    {
                        if (matchingObjectives.Any(o => o.ObjectiveId == progress.ObjectiveId) && !progress.Completed)
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
        return progress.All(p =>
        {
            var objective = definition.Objectives.First(o => o.ObjectiveId == p.ObjectiveId);
            return p.Completed || objective.Optional;
        });
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
            if (existing.State == QuestState.Completed && !definition.IsRepeatable) return false;
        }

        var prerequisitesMet = definition.Prerequisites.All(id =>
            log.Quests.TryGetValue(id, out var prereqState) && prereqState.State == QuestState.Completed);

        return prerequisitesMet;
    }

    private sealed class PlayerQuestLog
    {
        public Dictionary<int, QuestStateData> Quests { get; } = new();
        public object Sync { get; } = new();
    }
}

public sealed record QuestAcceptResult(
    ResponseCode Code,
    string Message,
    QuestStateData? State,
    QuestDefinition? Definition);
