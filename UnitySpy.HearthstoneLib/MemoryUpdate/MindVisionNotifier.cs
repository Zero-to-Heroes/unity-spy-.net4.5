namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;

    public class MindVisionNotifier
    {
        private CollectionNotifier CollectionNotifier = new CollectionNotifier();
        private AchievementToastNotifier AchievementToastNotifier = new AchievementToastNotifier();
        private CurrentSceneNotifier CurrentSceneNotifier = new CurrentSceneNotifier();
        private OpenedPackNotifier OpenedPackNotifier = new OpenedPackNotifier();

        private Timer timer;
        public IMemoryUpdate previousResult;

        public void ListenForChanges(int frequency, MindVision mindVision, Action<object> callback)
        {
            if (timer != null)
            {
                timer.Close();
            }
            timer = new Timer();
            timer.Elapsed += delegate { OnTimedEvent(mindVision, callback); };
            timer.Interval = frequency;
            timer.Enabled = true;
        }

        public void StopListening()
        {
            timer.Enabled = false;
            timer = null;
        }

        public void OnTimedEvent(MindVision mindVision, Action<object> callback)
        {
            IMemoryUpdate result = new MemoryUpdate();

            AchievementToastNotifier.HandleDisplayingAchievementToast(mindVision, result);
            //OpenedPackNotifier.HandleOpenedPack(mindVision, result);
            //CollectionNotifier.HandleNewCards(mindVision, result);
            CurrentSceneNotifier.HandleSceneMode(mindVision, result);

            if (result.HasUpdates)
            {
                callback(result);
            }
        }

        public IMemoryUpdate GetMemoryChanges(MindVision mindVision)
        {
            IMemoryUpdate result = new MemoryUpdate();
            OpenedPackNotifier.HandleOpenedPack(mindVision, result);
            CollectionNotifier.HandleNewCards(mindVision, result);

            if (result.HasUpdates)
            {
                return result;
            }

            return null;
        }
    }
}