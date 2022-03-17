using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsReceivedRewardsNotifier
    {
        private bool? lastReceivedRewards;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var rewardsPending = mindVision.IsDuelsRewardsPending();
                if (!rewardsPending && (lastReceivedRewards == null || lastReceivedRewards.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsRewardsPending = false;
                    lastReceivedRewards = false;
                }
                else if (rewardsPending && (lastReceivedRewards == null || !lastReceivedRewards.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsRewardsPending = true;
                    lastReceivedRewards = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsReceivedRewardsNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}