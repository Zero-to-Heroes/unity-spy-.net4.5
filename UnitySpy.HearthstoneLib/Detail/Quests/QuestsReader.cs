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

            var quests = new List<QuestInfo>();
            for (var i = 0; i < count; i++)
            {
                var questModel = questState["_entries"][i]["value"];
                quests.Add(new QuestInfo()
                {
                    Id = questModel["_QuestId"],
                    Progress = questModel["_Progress"],
                    Status = questModel["_Status"],

                });
            }


            return new QuestsLog()
            {
                Quests = quests,
            };
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
