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
    using HackF5.UnitySpy.HearthstoneLib.MemoryUpdate;

    public class MindVision
    {
        private readonly HearthstoneImage image;
        public MindVisionNotifier MemoryNotifier = new MindVisionNotifier();

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

        public int GetCollectionSize() => CollectionCardReader.ReadCollectionSize(this.image);

        public IReadOnlyList<ICollectionCardBack> GetCollectionCardBacks() => CollectionCardBackReader.ReadCollection(this.image);

        public IReadOnlyList<ICollectionCoin> GetCollectionCoins() => CollectionCoinReader.ReadCollection(this.image);

        public IDungeonInfoCollection GetDungeonInfoCollection() => DungeonInfoReader.ReadCollection(this.image);

        //public bool GetCollectionCardRecords() => CollectionCardRecordReader.ReadCollectionCardRecords(this.image);

        public IDuelsInfo GetDuelsInfo() => DuelsInfoReader.ReadDuelsInfo(this.image);

        public IBattlegroundsInfo GetBattlegroundsInfo() => BattlegroundsInfoReader.ReadBattlegroundsInfo(this.image);

        public IMatchInfo GetMatchInfo() => MatchInfoReader.ReadMatchInfo(this.image);

        public IDeck GetActiveDeck() => ActiveDeckReader.ReadActiveDeck(this.image);

        public IArenaInfo GetArenaInfo() => ArenaInfoReader.ReadArenaInfo(this.image);

        public IOpenPacksInfo GetOpenPacksInfo() => OpenPacksInfoReader.ReadOpenPacksInfo(this.image);

        public IPackInfo GetOpenedPack() => OpenPacksInfoReader.ReadOpenPackInfo(this.image);

        public IBoostersInfo GetBoostersInfo() => BoostersInfoReader.ReadBoostersInfo(this.image);

        public IAccountInfo GetAccountInfo() => AccountInfoReader.ReadAccountInfo(this.image);

        public SceneModeEnum? GetSceneMode() => SceneModeReader.ReadSceneMode(this.image);

        public bool IsMaybeOnDuelsRewardsScreen() => SceneModeReader.IsMaybeOnDuelsRewardsScreen(this.image);

        public IRewardTrackInfo GetRewardTrackInfo() => RewardTrackInfoReader.ReadRewardTrack(this.image);

        public IDuelsRewardsInfo GetDuelsRewardsInfo() => DuelsRewardsInfoReader.ReadDuelsRewardsInfo(this.image);

        public IAchievementsInfo GetAchievementsInfo() => AchievementsInfoReader.ReadAchievementsInfo(this.image);

        public IAchievementsInfo GetInGameAchievementsProgressInfo() => AchievementsInfoReader.ReadInGameAchievementsProgressInfo(this.image);

        public bool IsDisplayingAchievementToast() => AchievementsInfoReader.IsDisplayingAchievementToast(this.image);

        public bool IsRunning() => Sanity.IsRunning(this.image);

        public void ListenForChanges(int frequency, Action<object> callback) => MemoryNotifier.ListenForChanges(frequency, this, callback);

        public IMemoryUpdate GetMemoryChanges() => MemoryNotifier.GetMemoryChanges(this);

        public void StopListening() => MemoryNotifier.StopListening();

        //public void ShowPlayerRecordsForBg()
        //{
        //    var service = image.GetNetCacheService("NetCachePlayerRecords");
        //    var size = service["<Records>k__BackingField"]["_size"];
        //    var playerRecords = service["<Records>k__BackingField"]["_items"];
        //    for (int i = 0; i < size; i++)
        //    {
        //        var record = playerRecords[i];
        //        var gameType = record["<RecordType>k__BackingField"];
        //        if (gameType == (int)GameType.GT_BATTLEGROUNDS)
        //        {
        //            var data = record["<Data>k__BackingField"];
        //            var losses = record["<Losses>k__BackingField"];
        //            var wins = record["<Wins>k__BackingField"];
        //            var ties = record["<Ties>k__BackingField"];
        //        }
        //    }
        //}

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