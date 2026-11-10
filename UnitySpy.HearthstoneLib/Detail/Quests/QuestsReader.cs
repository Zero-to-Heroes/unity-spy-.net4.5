namespace HackF5.UnitySpy.HearthstoneLib.Detail.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class QuestsReader
    {
        public static QuestsLog ReadQuests([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var service = image.GetService("Hearthstone.Progression.QuestManager");
            if (service == null)
            {
                return null;
            }

            var questState = service["m_questState"];
            var count = questState["_count"];
            // Hoisted out of the loop: each indexer access re-reads the whole array from process memory.
            var entries = questState["_entries"];

            var quests = new List<QuestInfo>();
            for (var i = 0; i < count; i++)
            {
                // Strongly typed field reads: skips the DLR dispatch of the dynamic indexer in this hot loop.
                if (!(entries[i] is IManagedObjectInstance entry))
                {
                    continue;
                }

                // _entries can contain free/removed slots (whose value is null) within [0, _count), so skip them.
                var questModel = entry.GetValue<IManagedObjectInstance>("value");
                if (questModel == null)
                {
                    continue;
                }

                quests.Add(new QuestInfo()
                {
                    Id = questModel.GetValue<int>("_QuestId"),
                    Progress = questModel.GetValue<int>("_Progress"),
                    Status = questModel.GetValue<int>("_Status"),
                });
            }


            return new QuestsLog()
            {
                Quests = quests,
                JourneyQuests = ReadJourneyQuests(image),
            };
        }

        // Seasonal Player Journey missions live on NetCache.NetCacheSpjState, not QuestManager.m_questState.
        // NetCache.SpjStoryState.SpjMissionState: IN_PROGRESS = 1, COMPLETION_AVAILABLE_SUCCESS = 2,
        // COMPLETION_AVAILABLE_FAILURE = 3. Later values are choice / completed / finalized.
        private const int JourneyMissionInProgress = 1;
        private const int JourneyMissionCompletionAvailableFailure = 3;
        private const int StatObjectiveStat = 0;
        private const int StatObjectiveValidator = 1;

        private static List<JourneyQuestInfo> ReadJourneyQuests(HearthstoneImage image)
        {
            var journeyQuests = new List<JourneyQuestInfo>();
            try
            {
                var spjState = image.GetNetCacheService("NetCacheSpjState");
                var stories = spjState?["<Stories>k__BackingField"];
                if (stories == null)
                {
                    return journeyQuests;
                }

                var count = stories["_count"];
                var entries = count > 0 ? stories["_entries"] : null;
                if (entries == null)
                {
                    return journeyQuests;
                }

                var missions = IndexDbfById(image, "SpjMission");
                var objectives = IndexDbfById(image, "StatObjective");
                var stats = IndexDbfById(image, "Stat");
                var thresholds = ReadDbfRecords(image, "StatThresholds");
                var goals = ReadDbfRecords(image, "StatGoals");

                for (var i = 0; i < count; i++)
                {
                    var story = entries[i]?["value"];
                    if (story == null)
                    {
                        continue;
                    }

                    int missionState = story["<MissionState>k__BackingField"];
                    if (missionState < JourneyMissionInProgress || missionState > JourneyMissionCompletionAvailableFailure)
                    {
                        continue;
                    }

                    int missionId = story["<MissionId>k__BackingField"];
                    if (!missions.TryGetValue(missionId, out dynamic mission))
                    {
                        continue;
                    }

                    int objectiveId = mission["m_completionObjectiveId"];
                    if (!objectives.TryGetValue(objectiveId, out dynamic objective))
                    {
                        continue;
                    }

                    var progressByStat = ReadStatProgress(story);
                    string missionDescription = Locale(mission["m_description"]);
                    int objectiveType = objective["m_statObjectiveType"];
                    if (objectiveType == StatObjectiveStat)
                    {
                        int statId = objective["m_statId"];
                        int quota = FirstThresholdQuota(thresholds, objectiveId);
                        AddJourneyQuest(journeyQuests, missionDescription, progressByStat, statId, quota);
                    }
                    else if (objectiveType == StatObjectiveValidator)
                    {
                        int validatorId = objective["m_validatorId"];
                        var matchingGoals = goals.Where(goal => (int)goal["m_statValidatorId"] == validatorId).ToList();
                        var several = matchingGoals.Count > 1;
                        foreach (var goal in matchingGoals)
                        {
                            int statId = goal["m_statId"];
                            int quota = goal["m_quota"];
                            string description = missionDescription;
                            if (several && stats.TryGetValue(statId, out dynamic stat))
                            {
                                description = Locale(stat["m_statProgressDescriptor"]) ?? missionDescription;
                            }

                            AddJourneyQuest(journeyQuests, description, progressByStat, statId, quota);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return journeyQuests;
            }

            return journeyQuests;
        }

        private static void AddJourneyQuest(
            List<JourneyQuestInfo> journeyQuests,
            string description,
            Dictionary<int, int> progressByStat,
            int statId,
            int quota)
        {
            if (quota <= 0 || string.IsNullOrWhiteSpace(description))
            {
                return;
            }

            var text = description.Replace("$q", quota.ToString()).Replace("[x]", string.Empty);

            progressByStat.TryGetValue(statId, out var progress);
            journeyQuests.Add(new JourneyQuestInfo
            {
                Description = text,
                Progress = progress,
                Quota = quota,
            });
        }

        private static Dictionary<int, int> ReadStatProgress(dynamic story)
        {
            var progressByStat = new Dictionary<int, int>();
            dynamic stats = story["<Stats>k__BackingField"];
            if (stats == null)
            {
                return progressByStat;
            }

            int size = stats["_size"];
            dynamic items = size > 0 ? stats["_items"] : null;
            if (items == null)
            {
                return progressByStat;
            }

            for (var i = 0; i < size; i++)
            {
                dynamic stat = items[i];
                if (stat == null)
                {
                    continue;
                }

                int statId = stat["<StatId>k__BackingField"];
                int value = stat["<Value>k__BackingField"];
                progressByStat[statId] = value;
            }

            return progressByStat;
        }

        private static int FirstThresholdQuota(IReadOnlyList<dynamic> thresholds, int objectiveId)
        {
            foreach (var threshold in thresholds)
            {
                if ((int)threshold["m_statObjectiveId"] == objectiveId)
                {
                    return threshold["m_quota"];
                }
            }

            return 0;
        }

        private static Dictionary<int, dynamic> IndexDbfById(HearthstoneImage image, string dbfName)
        {
            var indexed = new Dictionary<int, dynamic>();
            foreach (var record in ReadDbfRecords(image, dbfName))
            {
                int id = record["m_ID"];
                indexed[id] = record;
            }

            return indexed;
        }

        private static List<dynamic> ReadDbfRecords(HearthstoneImage image, string dbfName)
        {
            var records = new List<dynamic>();
            dynamic list = image["GameDbf"]?[dbfName]?["m_records"];
            if (list == null)
            {
                return records;
            }

            int size = list["_size"];
            dynamic items = size > 0 ? list["_items"] : null;
            if (items == null)
            {
                return records;
            }

            for (var i = 0; i < size; i++)
            {
                dynamic record = items[i];
                if (record != null)
                {
                    records.Add(record);
                }
            }

            return records;
        }

        private static string Locale(dynamic locValue)
        {
            if (locValue == null)
            {
                return null;
            }

            string value = locValue["m_currentLocaleValue"];
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        // From decompiled Achievements.cs
        private static bool CanShowInQuestLog(ClientFlags m_clientFlags, AchieveType m_type)
        {
            if ((m_clientFlags & ClientFlags.SHOW_IN_QUEST_LOG) != 0)
            {
                return true;
            }
            switch (m_type)
            {
                case AchieveType.STARTER:
                case AchieveType.DAILY:
                case AchieveType.NORMAL_QUEST:
                    return true;
                default:
                    return false;
            }
        }

        // Achieve.cs
        private enum AchieveType
        {
            INVALID,
            STARTER,
            HERO,
            GOLDHERO,
            DAILY,
            DAILY_REPEATABLE,
            HIDDEN,
            INTERNAL_ACTIVE,
            INTERNAL_INACTIVE,
            LOGIN_ACTIVATED,
            NORMAL_QUEST,
            LOGIN_AND_CHAIN_ACTIVATED,
            PREMIUMHERO
        }
        private enum ClientFlags
        {
            NONE = 0x0,
            IS_LEGENDARY = 0x1,
            SHOW_IN_QUEST_LOG = 0x2,
            IS_AFFECTED_BY_FRIEND_WEEK = 0x4,
            IS_AFFECTED_BY_DOUBLE_GOLD = 0x8
        }
    }
}
