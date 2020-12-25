namespace HackF5.UnitySpy.HearthstoneLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
    using HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Match;
    using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Duels;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Achievement;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;

    public class MindVision
    {
        private readonly HearthstoneImage image;

        public MindVision()
        {
            var process = Process.GetProcessesByName("Hearthstone").FirstOrDefault();
            if (process == null)
            {
                throw new InvalidOperationException(
                    "Failed to find Hearthstone executable. Please check that Hearthstone is running.");
            }

            this.image = new HearthstoneImage(AssemblyImageFactory.Create(process.Id));
        }

        public IReadOnlyList<ICollectionCard> GetCollectionCards() => CollectionCardReader.ReadCollection(this.image);

        public IDungeonInfoCollection GetDungeonInfoCollection() => DungeonInfoReader.ReadCollection(this.image);

        public IDuelsInfo GetDuelsInfo() => DuelsInfoReader.ReadDuelsInfo(this.image);

        public IBattlegroundsInfo GetBattlegroundsInfo() => BattlegroundsInfoReader.ReadBattlegroundsInfo(this.image);

        public IMatchInfo GetMatchInfo() => MatchInfoReader.ReadMatchInfo(this.image);

        public IDeck GetActiveDeck() => ActiveDeckReader.ReadActiveDeck(this.image);

        public IArenaInfo GetArenaInfo() => ArenaInfoReader.ReadArenaInfo(this.image);

        public IOpenPacksInfo GetOpenPacksInfo() => OpenPacksInfoReader.ReadOpenPacksInfo(this.image);

        public IAccountInfo GetAccountInfo() => AccountInfoReader.ReadAccountInfo(this.image);

        public SceneModeEnum GetSceneMode() => SceneModeReader.ReadSceneMode(this.image);

        public IRewardTrackInfo GetRewardTrackInfo() => RewardTrackInfoReader.ReadRewardTrack(this.image);

        public IDuelsRewardsInfo GetDuelsRewardsInfo() => DuelsRewardsInfoReader.ReadDuelsRewardsInfo(this.image);

        public IAchievementsInfo GetAchievementsInfo() => AchievementsInfoReader.ReadAchievementsInfo(this.image);

        public IAchievementsInfo GetInGameAchievementsProgressInfo() => AchievementsInfoReader.ReadInGameAchievementsProgressInfo(this.image);

        public bool IsDisplayingAchievementToast() => AchievementsInfoReader.IsDisplayingAchievementToast(this.image);

        public bool IsRunning() => Sanity.IsRunning(this.image);

        // TODO: externalize that. For now, waiting to see if Brian can help with a more elegant solution
        private Timer timer;
        private IMemoryUpdate previousResult;

        public void ListenForChanges(int frequency, Action<object> callback)
        {
            if (timer != null)
            {
                timer.Close();
            }
            timer = new Timer();
            timer.Elapsed += delegate { OnTimedEvent(this, callback); };
            timer.Interval = frequency;
            timer.Enabled = true;
        }

        public void StopListening()
        {
            timer.Enabled = false;
            timer = null;
        }

        public static void OnTimedEvent(MindVision mindVision, Action<object> callback)
        {
            bool hasUpdates = false;
            // Build all the new memory info
            IMemoryUpdate currentResult = new MemoryUpdate()
            {
                DisplayingAchievementToast = mindVision.IsDisplayingAchievementToast(),
            };

            // Populate the changeset
            MemoryUpdate result = new MemoryUpdate();
            if (currentResult.DisplayingAchievementToast != mindVision.previousResult?.DisplayingAchievementToast)
            {
                result.DisplayingAchievementToast = currentResult.DisplayingAchievementToast;
                hasUpdates = true;
            }

            // Emit even if the changeset is not empty
            // Don't send an event the first time
            if ((mindVision.previousResult != null && hasUpdates))
            {
                callback(result);
            }

            mindVision.previousResult = currentResult;
        }

        public void ListServices()
        {
            var dynamicServices = image?["HearthstoneServices"]["s_dynamicServices"];
            var staticServices = image?["HearthstoneServices"]["s_services"];
            var services = dynamicServices ?? staticServices;

            if (services == null)
            {
                return;
            }

            var serviceItems = services["m_services"]["_items"];

            var serviceNames = new List<string>();
            var i = 0;
            foreach (var service in serviceItems)
            {
                var name = service?["<ServiceTypeName>k__BackingField"];
                serviceNames.Add(name);
                if (name == "AchieveManager")
                {
                    var hop = "";
                }
                if (name == "Hearthstone.Progression.AchievementManager")
                {
                    var hop = "";
                }
                if (name == "PopupDisplayManager")
                {
                    var hop = "";
                }
                i++;
            }

            return;
        }
    }
}