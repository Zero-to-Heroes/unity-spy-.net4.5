namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;

    public class MindVisionNotifier
    {
        private CollectionNotifier CollectionNotifier = new CollectionNotifier();
        private AchievementToastNotifier AchievementToastNotifier = new AchievementToastNotifier();
        private CurrentSceneNotifier CurrentSceneNotifier = new CurrentSceneNotifier();
        private OpenedPackNotifier OpenedPackNotifier = new OpenedPackNotifier();
        private XpChangeNotifier XpChangeNotifier = new XpChangeNotifier();
        // To avoid having to rely on short timings in friendly matches
        private SelectedDeckNotifier SelectedDeckNotifier = new SelectedDeckNotifier();

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
            try
            {
                IMemoryUpdate result = new MemoryUpdate();
                AchievementToastNotifier.HandleDisplayingAchievementToast(mindVision, result);
                OpenedPackNotifier.HandleOpenedPack(mindVision, result);
                //CollectionNotifier.HandleNewCards(mindVision, result);
                CurrentSceneNotifier.HandleSceneMode(mindVision, result);
                XpChangeNotifier.HandleXpChange(mindVision, result);
                SelectedDeckNotifier.HandleSelectedDeck(mindVision, result);

                if (result.HasUpdates)
                {
                    callback(result);
                }
            }
            catch (Exception e)
            {
                // Do nothing? So that the timer isn't broken if the initialization didn't work properly?
                callback(e.Message);
                callback(e.StackTrace);
                callback("reset");
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