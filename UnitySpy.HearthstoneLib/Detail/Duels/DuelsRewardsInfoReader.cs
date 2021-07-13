namespace HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardsInfo;
    using JetBrains.Annotations;

    internal static class DuelsRewardsInfoReader
    {
        public static IDuelsRewardsInfo ReadDuelsRewardsInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            try
            {
                if (image["RewardBoxesDisplay"] == null || image["RewardBoxesDisplay"]["s_Instance"] == null)
                {
                    return null;
                }
            } catch (Exception e)
            {
                return null;
            }

            var service = image["RewardBoxesDisplay"]["s_Instance"];
            if (service["m_rewards"] == null || service["m_rewards"]["_size"] == 0)
            {
                return null;
            }

            var rewardsList = RewardsInfoReader.ParseRewards(service["m_rewards"]["_items"]);
            return new DuelsRewardsInfo
            {
                Rewards = rewardsList,
            };
        }
    }
}