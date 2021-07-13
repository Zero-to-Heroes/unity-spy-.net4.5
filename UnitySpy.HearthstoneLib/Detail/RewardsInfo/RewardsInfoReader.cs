﻿namespace HackF5.UnitySpy.HearthstoneLib.Detail.RewardsInfo
{
    using HackF5.UnitySpy.Detail;
    using System;
    using System.Collections.Generic;

    internal static class RewardsInfoReader
    {

        public static IReadOnlyList<IRewardInfo> ParseRewards(dynamic rewards)
        {
            var rewardsList = new List<IRewardInfo>();
            if (rewards == null)
            {
                return rewardsList;
            }
            
            long amount;
            foreach (var rewardObject in rewards)
            {
                if (rewardObject == null)
                {
                    continue;
                }

                try
                {
                    // Not sure which field is used based on what context, so we try both
                    amount = rewardObject["<Count>k__BackingField"];
                }
                catch (Exception e)
                {
                    amount = rewardObject["<Amount>k__BackingField"];
                }

                var type = rewardObject["m_type"];
                rewardsList.Add(new RewardInfo
                {
                    Type = type,
                    Amount = type == 1 && amount == 0 ? 1 : amount,
                    BoosterId = type == 1 ? rewardObject["<Id>k__BackingField"] : -1,
                });
            }
            return rewardsList;
        }
    }
}