using System;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class AchievementToastNotifier
    {
        private bool lastToastStatus;
        private bool isInit;

        private bool sentExceptionMessage = false;

        internal void HandleDisplayingAchievementToast(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var displayToast = mindVision.IsDisplayingAchievementToast();
                if (displayToast && displayToast != lastToastStatus && isInit)
                {
                    result.HasUpdates = true;
                    result.DisplayingAchievementToast = displayToast;
                }
                lastToastStatus = displayToast;
                isInit = true;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when handling display achievement toast memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }
    }
}