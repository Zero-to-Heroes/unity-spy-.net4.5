namespace HackF5.UnitySpy.HearthstoneLib.Detail.Achievement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.Crawler;
    using JetBrains.Annotations;

    internal static class AchievementsInfoReader
    {
        public static IAchievementsInfo ReadAchievementsInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var manager = image.GetService("Hearthstone.Progression.AchievementManager");
            if (manager == null || manager["m_playerState"] == null || manager["m_playerState"]["m_playerState"] == null)
            {
                return null;
            }

            var playerState = manager["m_playerState"]["m_playerState"];
            var count = playerState["count"];
            var values = playerState["valueSlots"];

            var achievements = new List<IAchievementInfo>();
            for (int i = 0; i < count; i++)
            {
                var info = values[i];
                var achievementInfo = new AchievementInfo()
                {
                    AchievementId = info["_AchievementId"],
                    Progress = info["_Progress"],
                    Status = info["_Status"],
                };
                achievements.Add(achievementInfo);
            }

            return new AchievementsInfo()
            {
                Achievements = achievements,
            };
        }

        public static IAchievementsInfo ReadInGameAchievementsProgressInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var manager = image.GetService("Hearthstone.Progression.AchievementManager");
            if (manager == null || manager["m_achievementInGameProgress"] == null)
            {
                return null;
            }

            var progressInfo = manager["m_achievementInGameProgress"];
            var count = progressInfo["count"];
            var entries = progressInfo["entries"];

            var achievements = new List<IAchievementInfo>();
            for (int i = 0; i < count; i++)
            {
                var entry = entries[i];
                var achievementId = entry["key"];
                var progress = entry["value"];
                var achievementInfo = new AchievementInfo()
                {
                    AchievementId = achievementId,
                    Progress = progress,
                };
                achievements.Add(achievementInfo);
            }

            return new AchievementsInfo()
            {
                Achievements = achievements,
            };
        }

        public static bool IsDisplayingAchievementToast([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var manager = image.GetService("Hearthstone.Progression.AchievementManager");
            //var debugState = manager["m_playerState"];
            //debugState.TypeDefinition
            if (manager == null || manager["m_achievementToast"] == null)
            {
                return false;
            }

            var toast = manager["m_achievementToast"];
            if (toast != null)
            {
                //processing = true;
                //TypeDefinitionContentViewModel model = new TypeDefinitionContentViewModel(manager.TypeDefinition);
                //List<string> dump = new List<string>();
                //var addresses = new List<uint>();
                //model.DumpMemory("", dump, addresses, manager);
                return true;
            }

            return true;
        }
    }
}