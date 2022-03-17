namespace HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardsInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using JetBrains.Annotations;

    internal static class DuelsRewardsInfoReader
    {
        public static bool ReadDuelsRewardsPending([NotNull] HearthstoneImage image)
        {
            var currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return false;
            }

            var playMat = image["PvPDungeonRunScene"]["m_instance"]?["m_dungeonCrawlDisplay"]?["m_dungeonCrawlPlayMatReference"]?["<Object>k__BackingField"];
            //var playMatState = playMat?["m_playMatState"];
            //if (playMatState != 6)
            //{
            //    return false;
            //}

            // Now we are in a state where we could be showing rewards, but the user may not have clicked on the lever yet
            var ready = playMat?["m_duelsReadyToShowRewards"];
            return ready == true;
        }

        // TODO: extract that as a generic ReadRewardFromNetCache with the right params
        public static IDuelsRewardsInfo ReadDuelsRewardsInfo([NotNull] HearthstoneImage image)
        {
            var currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return null;
            }

            // Avoid using the RewardBoxes object to avoid getting the issue where the prefabs are not instantiated yet
            var notices = image.GetNetCacheService("NetCacheProfileNotices")?["<Notices>k__BackingField"];
            if (notices == null)
            {
                return null;
            }

            var size = notices["_size"];
            var items = notices["_items"];
            var rewardsList = new List<IRewardInfo>();
            for (var i = 0; i < size; i++)
            {
                var notice = items[i];
                var type = notice["m_type"];
                if (type != 20) // NetCache.ProfileNotice.NoticeType.GENERIC_REWARD_CHEST
                {
                    continue;
                }

                var origin = notice["<Origin>k__BackingField"];
                if (origin != 29) // NetCache.ProfileNotice.NoticeOrigin.NOTICE_ORIGIN_DUELS
                {
                    continue;
                }

                var bag = notice["<RewardChest>k__BackingField"]?["_Bag"];
                if (bag == null)
                {
                    continue;
                }

                var bagCount = bag["_size"];
                var bagItems = bag["_items"];
                for (var j = 0; j < bagCount; j++)
                {
                    var bagItem = bagItems[j];
                    var rewardInfo = DuelsRewardsInfoReader.BuildRewardInfo(bagItem);
                    rewardsList.Add(rewardInfo);
                }
            }
            return new DuelsRewardsInfo
            {
                Rewards = rewardsList,
            };
            //try
            //{
            //    if (image["RewardBoxesDisplay"] == null || image["RewardBoxesDisplay"]["s_Instance"] == null)
            //    {
            //        return null;
            //    }
            //} catch (Exception e)
            //{
            //    return null;
            //}

            //var service = image["RewardBoxesDisplay"]["s_Instance"];
            //if (service["m_rewards"] == null || service["m_rewards"]["_size"] == 0)
            //{
            //    return null;
            //}

            //var rewardsList = RewardsInfoReader.ParseRewards(service["m_rewards"]["_items"]);
            //return new DuelsRewardsInfo
            //{
            //    Rewards = rewardsList,
            //};
        }

        private static IRewardInfo BuildRewardInfo(dynamic bagItem)
        {
            var booster = bagItem["_RewardBooster"];
            if (booster != null)
            {
                return new RewardInfo
                {
                    Type = 1,
                    Amount = booster["<BoosterCount>k__BackingField"],
                    BoosterId = booster["<BoosterType>k__BackingField"],
                };
            }

            var card = bagItem["_RewardCard"];

            var dust = bagItem["_RewardDust"];
            if (dust != null)
            {
                return new RewardInfo
                {
                    Type = 0,
                    Amount = dust["<Amount>k__BackingField"],
                };
            }

            var gold = bagItem["_RewardGold"];
            if (gold != null)
            {
                return new RewardInfo
                {
                    Type = 6,
                    Amount = gold["<Amount>k__BackingField"],
                };
            }

            return null;
        }
    }
}