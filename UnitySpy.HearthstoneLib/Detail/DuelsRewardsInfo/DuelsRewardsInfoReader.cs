namespace HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
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

            var rewards = service["m_rewards"]["_items"];
            var rewardsCount = service["m_rewards"]["_size"];
            var rewardsList = new List<IDuelsRewardInfo>();
            for (int i = 0; i < rewardsCount; i++)
            {
                var rewardObject = rewards[i];
                rewardsList.Add(new DuelsRewardInfo
                {
                    Type = rewardObject["m_type"],
                    Amount = rewardObject["m_type"] == 1 ? rewardObject["<Count>k__BackingField"] : rewardObject["<Amount>k__BackingField"],
                    BoosterId = rewardObject["m_type"] == 1 ? rewardObject["<Id>k__BackingField"] : -1,
                });
            }
            return new DuelsRewardsInfo
            {
                Rewards = rewardsList,
            };
        }
    }
}