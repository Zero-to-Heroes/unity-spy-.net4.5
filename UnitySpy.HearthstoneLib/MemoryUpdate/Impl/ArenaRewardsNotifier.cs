namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal class ArenaRewardsNotifier
    {
        private IReadOnlyList<IRewardInfo> lastRewards;
        private bool isInit;

        internal void HandleArenaRewards(MindVision mindVision, IMemoryUpdate result)
        {
            var rewards = mindVision.GetArenaInfo()?.Rewards;
            if (rewards != null && rewards.Count > 0 && !AreEqual(lastRewards, rewards) && isInit)
            {
                result.HasUpdates = true;
                result.ArenaRewards = rewards;
            }
            lastRewards = rewards;
            isInit = true;
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
