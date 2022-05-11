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
    using HackF5.UnitySpy.HearthstoneLib.Detail.Mercenaries;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Achievement;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.MemoryUpdate;

    public class MindVision
    {
        public event EventHandler MessageReceived;

        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            EventHandler handler = MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        public MindVisionNotifier MemoryNotifier = new MindVisionNotifier();
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
            Logger.LogHandler = (string msg) =>
            {
                OnMessageReceived(new MessageEventArgs() { Message = msg });
            };
        }

        public IReadOnlyList<ICollectionCard> GetCollectionCards() => CollectionCardReader.ReadCollection(this.image);

        public IReadOnlyList<int> GetCollectionBattlegroundsHeroSkins() => CollectionCardReader.ReadBattlegroundsHeroSkins(this.image);

        public IReadOnlyList<IDustInfoCard> GetDustInfoCards() => CollectionCardReader.ReadDustInfoCards(this.image);

        public int GetCollectionSize() => CollectionCardReader.ReadCollectionSize(this.image);

        public IReadOnlyList<ICollectionCardBack> GetCollectionCardBacks() => CollectionCardBackReader.ReadCollection(this.image);

        public IReadOnlyList<ICollectionCoin> GetCollectionCoins() => CollectionCoinReader.ReadCollection(this.image);

        public IDungeonInfoCollection GetDungeonInfoCollection() => DungeonInfoReader.ReadCollection(this.image);

        public IAdventuresInfo GetAdventuresInfo() => DungeonInfoReader.ReadAdventuresInfo(this.image);

        //public bool GetCollectionCardRecords() => CollectionCardRecordReader.ReadCollectionCardRecords(this.image);

        public IDuelsInfo GetDuelsInfo() => DuelsInfoReader.ReadDuelsInfo(this.image);

        public bool GetDuelsIsOnMainScreen() => DuelsInfoReader.ReadDuelsIsOnMainScreen(this.image);

        public bool GetDuelsIsOnDeckBuildingLobbyScreen() => DuelsInfoReader.ReadDuelsIsOnDeckBuildingLobbyScreen(this.image);

        public InternalDuelsDeck GetDuelsDeck() => DuelsInfoReader.ReadDuelsDeck(this.image);

        public int? GetNumberOfCardsInDeck() => DuelsInfoReader.ReadNumberOfCardsInDeck(this.image);

        public bool GetDuelsIsChoosingHero() => DuelsInfoReader.ReadDuelsIsOnHeroPickerScreen(this.image);

        public IReadOnlyList<int> GetDuelsHeroOptions() => DuelsInfoReader.ReadDuelsHeroOptions(this.image);

        public IReadOnlyList<IDuelsHeroPowerOption> GetDuelsHeroPowerOptions() => DuelsInfoReader.ReadDuelsHeroPowerOptions(this.image);

        public IReadOnlyList<IDuelsHeroPowerOption> GetDuelsSignatureTreasureOptions() => DuelsInfoReader.ReadDuelsSignatureTreasureOptions(this.image);

        public int GetDuelsCurrentOptionSelection() => DuelsInfoReader.ReadDuelsCurrentOptionSelection(this.image);

        public IBattlegroundsInfo GetBattlegroundsInfo() => BattlegroundsInfoReader.ReadBattlegroundsInfo(this.image);

        public int GetBattlegroundsNewRating() => BattlegroundsInfoReader.ReadNewRating(this.image);

        public IMatchInfo GetMatchInfo() => MatchInfoReader.ReadMatchInfo(this.image);

        public int GetBoard() => MatchInfoReader.RetrieveBoardInfo(this.image);

        public IDeck GetActiveDeck(long? selectedDeckId) => ActiveDeckReader.ReadActiveDeck(this.image, selectedDeckId);

        public IDeck GetWhizbangDeck(long whizbangDeckId) => ActiveDeckReader.ReadWhizbangDeck(this.image, whizbangDeckId);

        public IReadOnlyList<IDeck> GetTemplateDecks() => ActiveDeckReader.ReadTemplateDecks(this.image);

        public long? GetSelectedDeckId() => ActiveDeckReader.GetSelectedDeckId(this.image);

        public IArenaInfo GetArenaInfo() => ArenaInfoReader.ReadArenaInfo(this.image);

        public IOpenPacksInfo GetOpenPacksInfo() => OpenPacksInfoReader.ReadOpenPacksInfo(this.image);

        public IPackInfo GetOpenedPack() => OpenPacksInfoReader.ReadOpenPackInfo(this.image);

        public bool IsOpeningPack() => OpenPacksInfoReader.ReadIsOpeningPack(this.image);

        public IBoostersInfo GetBoostersInfo() => BoostersInfoReader.ReadBoostersInfo(this.image);

        public IAccountInfo GetAccountInfo() => AccountInfoReader.ReadAccountInfo(this.image);

        public SceneModeEnum? GetSceneMode() => SceneModeReader.ReadSceneMode(this.image);

        public bool GetMercenariesIsSelectingTreasures() => SceneModeReader.ReadMercenariesIsSelectingTreasures(this.image);

        public IDuelsPendingTreasureSelection GetDuelsPendingTreasureSelection() => DuelsInfoReader.ReadPendingTreasureSelection(this.image);

        //public bool IsMaybeOnDuelsRewardsScreen() => SceneModeReader.IsMaybeOnDuelsRewardsScreen(this.image);

        public IRewardTrackInfo GetRewardTrackInfo() => RewardTrackInfoReader.ReadRewardTrack(this.image);

        public IReadOnlyList<IXpChange> GetXpChanges() => RewardTrackInfoReader.ReadXpChanges(this.image);

        public bool IsDuelsRewardsPending() => DuelsRewardsInfoReader.ReadDuelsRewardsPending(this.image);

        public IDuelsRewardsInfo GetDuelsRewardsInfo() => DuelsRewardsInfoReader.ReadDuelsRewardsInfo(this.image);

        public IAchievementsInfo GetAchievementsInfo() => AchievementsInfoReader.ReadAchievementsInfo(this.image);

        public IMercenariesInfo GetMercenariesInfo() => MercenariesInfoReader.ReadMercenariesInfo(this.image);

        public IMercenariesCollection GetMercenariesCollectionInfo() => MercenariesInfoReader.ReadMercenariesCollectionInfo(this.image);

        public IReadOnlyList<IMercenariesVisitor> GetMercenariesVisitors() => MercenariesInfoReader.ReadMercenariesVisitorsInfo(this.image);

        public IMercenariesPendingTreasureSelection GetMercenariesPendingTreasureSelection() => MercenariesInfoReader.ReadPendingTreasureSelection(this.image);

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
            var staticServices = image?["HearthstoneServices"]["s_runtimeServices"];
            var services = dynamicServices ?? staticServices;

            if (services == null)
            {
                return;
            }

            var serviceItems = services["m_services"]["entries"];

            var serviceNames = new List<string>();
            var i = 0;
            foreach (var service in serviceItems)
            {
                var name = service?["value"]?["<ServiceTypeName>k__BackingField"];
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
        public void ListNetCacheServices()
        {
            var i = 0;
            var serviceNames = new List<string>();
            var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];
            if (netCacheValues != null)
            {
                foreach (var netCache in netCacheValues)
                {
                    var name = netCache?.TypeDefinition.Name;
                    serviceNames.Add(name);
                    i++;
                }
            }

            return;
        }

        public void Test()
        {
            var result = new List<IAdventureTreasureInfo>();
            var achievementsInfo = this.GetAchievementsInfo();
            var treasures = image["GameDbf"]["AdventureLoadoutTreasures"]["m_records"];
            var count = treasures["_size"];
            var items = treasures["_items"];
            for (var i = 0; i < count; i++)
            {
                var treasure = items[i];
                var achievement = achievementsInfo.Achievements.Where(ach => ach.AchievementId == treasure["m_unlockAchievementId"]).FirstOrDefault();
                if (achievement == null)
                {
                    continue;
                }

                var complete = achievement.Status >= 2 && achievement.Status <= 4;
                result.Add(new AdventureTreasureInfo()
                {
                    Id = treasure["m_ID"],
                    AdventureId = treasure["m_adventureId"],
                    CardDbfId = treasure["m_cardId"],
                    HeroId = treasure["m_guestHeroId"],
                    Unlocked = treasure["m_unlockAchievementId"] == 0 || complete,
                });
            }
        }
    }
}