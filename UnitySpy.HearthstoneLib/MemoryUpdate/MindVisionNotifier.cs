namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Diagnostics;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;

    public class MindVisionNotifier
    {
        private AchievementToastNotifier AchievementToastNotifier = new AchievementToastNotifier();
        private CurrentSceneNotifier CurrentSceneNotifier = new CurrentSceneNotifier();
        private XpChangeNotifier XpChangeNotifier = new XpChangeNotifier();
        private ArenaRewardsNotifier ArenaRewardsNotifier = new ArenaRewardsNotifier();
        // To avoid having to rely on short timings in friendly matches
        private SelectedDeckNotifier SelectedDeckNotifier = new SelectedDeckNotifier();
        private IsOpeningPackNotifier UnopenedPacksCountNotifier = new IsOpeningPackNotifier();
        private MercenariesPendingTreasureSelectionNotifier MercenariesPendingTreasureSelectionNotifier = new MercenariesPendingTreasureSelectionNotifier();
        private MercenariesTasksUpdatedNotifier MercenariesTasksUpdatedNotifier = new MercenariesTasksUpdatedNotifier();
        private BattlegroundsNewRatingNotifier BattlegroundsNewRatingNotifier = new BattlegroundsNewRatingNotifier();
        private DuelsPendingTreasureSelectionNotifier DuelsPendingTreasureSelectionNotifier = new DuelsPendingTreasureSelectionNotifier();
        private DuelsMainRunScreenNotifier DuelsMainRunScreenNotifier = new DuelsMainRunScreenNotifier();
        private DuelsDeckBuildingLobbyScreenNotifier DuelsDeckBuildingLobbyScreenNotifier = new DuelsDeckBuildingLobbyScreenNotifier();
        private DuelsCurrentOptionSelectionNotifier DuelsCurrentOptionSelectionNotifier = new DuelsCurrentOptionSelectionNotifier();
        private DuelsChoosingHeroNotifier DuelsChoosingHeroNotifier = new DuelsChoosingHeroNotifier();
        private DuelsCardsInDeckChangeNotifier DuelsCardsInDeckChangeNotifier = new DuelsCardsInDeckChangeNotifier();
        private DuelsReceivedRewardsNotifier DuelsReceivedRewardsNotifier = new DuelsReceivedRewardsNotifier();
        private FriendsListOpenedNotifier FriendsListOpenedNotifier = new FriendsListOpenedNotifier();
        private CollectionCardsCountNotifier CollectionCardsCountNotifier = new CollectionCardsCountNotifier();

        private OpenedPackNotifier OpenedPackNotifier = new OpenedPackNotifier();
        private CollectionInitNotifier CollectionNotifier = new CollectionInitNotifier();

        private Timer timer;
        public IMemoryUpdate previousResult;

        public void ListenForChanges(int frequency, MindVision mindVision, Action<object> callback)
        {
            Logger.Log("ListenForChanges");
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
            Logger.Log("Stop Listening");
            if (timer != null)
            {
                timer.Stop();
                timer.Enabled = false;
            }
            else
            {
                Logger.Log("Timer was null");
            }
            timer = null;
        }

        public void OnTimedEvent(MindVision mindVision, Action<object> callback)
        {
            try
            {
                IMemoryUpdate result = new MemoryUpdate();

                CurrentSceneNotifier.HandleSceneMode(mindVision, result);
                XpChangeNotifier.HandleXpChange(mindVision, result);
                UnopenedPacksCountNotifier.HandleIsOpeningPack(mindVision, result);
                AchievementToastNotifier.HandleDisplayingAchievementToast(mindVision, result);
                SelectedDeckNotifier.HandleSelectedDeck(mindVision, result);
                BattlegroundsNewRatingNotifier.HandleSelection(mindVision, result);
                ArenaRewardsNotifier.HandleArenaRewards(mindVision, result);
                MercenariesPendingTreasureSelectionNotifier.HandleSelection(mindVision, result);
                MercenariesTasksUpdatedNotifier.HandleSelection(mindVision, result);
                DuelsMainRunScreenNotifier.HandleSelection(mindVision, result);
                DuelsDeckBuildingLobbyScreenNotifier.HandleSelection(mindVision, result);
                DuelsCardsInDeckChangeNotifier.HandleSelection(mindVision, result);
                DuelsCurrentOptionSelectionNotifier.HandleSelection(mindVision, result);
                DuelsChoosingHeroNotifier.HandleSelection(mindVision, result);
                DuelsReceivedRewardsNotifier.HandleSelection(mindVision, result);
                CollectionNotifier.HandleCollectionInit(mindVision, result);
                CollectionCardsCountNotifier.HandleCollectionCardsCount(mindVision, result);
                CollectionCardsCountNotifier.HandleBoostersCount(mindVision, result);
                CollectionCardsCountNotifier.HandleCollectionCardBacksCount(mindVision, result);
                CollectionCardsCountNotifier.HandleCollectionBattlegroundsHeroSkinsCount(mindVision, result);
                CollectionCardsCountNotifier.HandleCollectionCoinsCount(mindVision, result);
                FriendsListOpenedNotifier.HandleSelection(mindVision, result);

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