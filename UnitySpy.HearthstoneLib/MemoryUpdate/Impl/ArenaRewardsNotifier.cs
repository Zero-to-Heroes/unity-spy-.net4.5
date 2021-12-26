namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class ArenaRewardsNotifier
    {
        private IReadOnlyList<IRewardInfo> lastRewards;
        private bool isInit;

        private bool sentExceptionMessage = false;

        internal void HandleArenaRewards(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var rewards = mindVision.GetArenaInfo()?.Rewards;
                if (rewards != null && rewards.Count > 0 && !AreEqual(lastRewards, rewards) && isInit)
                {
                    result.HasUpdates = true;
                    result.ArenaRewards = rewards;
                }
                lastRewards = rewards;
                isInit = true;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when HandleArenaRewards memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        private bool AreEqual(IReadOnlyList<IRewardInfo> lastRewards, IReadOnlyList<IRewardInfo> rewards)
        {
            if (lastRewards == null)
            {
                return false;
            }

            if (lastRewards.Count != rewards.Count)
            {
                return false;
            }

            for (var i = 0; i < rewards.Count; i++)
            {
                if (!lastRewards[i].Equals(rewards[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
