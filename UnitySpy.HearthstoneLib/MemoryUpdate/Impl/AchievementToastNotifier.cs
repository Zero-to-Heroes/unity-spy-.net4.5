namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class AchievementToastNotifier
    {
        private bool lastToastStatus;
        private bool isInit;

        internal void HandleDisplayingAchievementToast(MindVision mindVision, IMemoryUpdate result)
        {
            var displayToast = mindVision.IsDisplayingAchievementToast();
            if (displayToast && displayToast != lastToastStatus && isInit)
            {
                result.HasUpdates = true;
                result.DisplayingAchievementToast = displayToast;
            }
            lastToastStatus = displayToast;
            isInit = true;
        }
    }
}