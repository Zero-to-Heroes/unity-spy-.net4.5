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
    using HackF5.UnitySpy.HearthstoneLib.Detail.TurnTimer;
    using System.Timers;
    using HackF5.UnitySpy.HearthstoneLib.MemoryUpdate;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Quests;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Friends;
    using HackF5.UnitySpy.HearthstoneLib.Detail.GameDbf;
    using HackF5.UnitySpy.HearthstoneLib.Detail.EventTimings;
    using HackF5.UnitySpy.HearthstoneLib.Detail.PlayerProfile;
    using HackF5.UnitySpy.HearthstoneLib.Detail.InputManager;

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

        public bool IsCollectionInit() => CollectionCardReader.IsCollectionInit(this.image);

        public IReadOnlyList<int> GetCollectionBattlegroundsHeroSkins() => CollectionBattlegroundsHeroSkinsReader.ReadBattlegroundsHeroSkins(this.image);

        public IReadOnlyList<IDustInfoCard> GetDustInfoCards() => CollectionCardReader.ReadDustInfoCards(this.image);

        public int GetCollectionSize() => CollectionCardReader.ReadCollectionSize(this.image);
        public int GetCollectionCardBacksSize() => CollectionCardBackReader.ReadCollectionSize(this.image);
        public int GetCollectionBgHeroSkinsSize() => CollectionBattlegroundsHeroSkinsReader.ReadCollectionSize(this.image);
        public int GetCollectionCoinsSize() => CollectionCoinReader.ReadCollectionSize(this.image);
        public int GetBoostersCount() => BoostersInfoReader.ReadBoostersCount(this.image);

        public IReadOnlyList<ICollectionCardBack> GetCollectionCardBacks() => CollectionCardBackReader.ReadCollection(this.image);

        public IReadOnlyList<ICollectionCoin> GetCollectionCoins() => CollectionCoinReader.ReadCollection(this.image);

        public IDungeonInfoCollection GetDungeonInfoCollection() => DungeonInfoReader.ReadCollection(this.image);

        public IAdventuresInfo GetAdventuresInfo() => DungeonInfoReader.ReadAdventuresInfo(this.image);

        //public bool GetCollectionCardRecords() => CollectionCardRecordReader.ReadCollectionCardRecords(this.image);

        public DuelsInfo GetDuelsInfo() => DuelsInfoReader.ReadDuelsInfo(this.image);
        public Deck GetDuelsDeckFromCollection() => DuelsInfoReader.ReadDuelsDeckFromCollection(this.image, debug: true);

        public bool GetDuelsIsOnMainScreen() => DuelsInfoReader.ReadDuelsIsOnMainScreen(this.image);

        public bool GetDuelsIsOnDeckBuildingLobbyScreen() => DuelsInfoReader.ReadDuelsIsOnDeckBuildingLobbyScreen(this.image);

        public Deck GetDuelsDeck() => DuelsInfoReader.ReadDuelsInfo(this.image)?.DuelsDeck;

        public int? GetNumberOfCardsInDeck() => DuelsInfoReader.ReadNumberOfCardsInDeck(this.image);

        public bool GetDuelsIsChoosingHero() => DuelsInfoReader.ReadDuelsIsOnHeroPickerScreen(this.image);

        public IReadOnlyList<DuelsHeroPowerOption> GetDuelsHeroOptions() => DuelsInfoReader.ReadDuelsHeroOptions(this.image);

        public IReadOnlyList<IDuelsHeroPowerOption> GetDuelsHeroPowerOptions() => DuelsInfoReader.ReadDuelsHeroPowerOptions(this.image);

        public IReadOnlyList<IDuelsHeroPowerOption> GetDuelsSignatureTreasureOptions() => DuelsInfoReader.ReadDuelsSignatureTreasureOptions(this.image);

        public int GetDuelsCurrentOptionSelection() => DuelsInfoReader.ReadDuelsCurrentOptionSelection(this.image);

        public BattlegroundsInfo GetBattlegroundsInfo() => BattlegroundsInfoReader.ReadBattlegroundsInfo(this.image);
        public int GetBattlegroundsNewRating() => BattlegroundsInfoReader.ReadNewRating(this.image);
        public string GetBattlegroundsSelectedGameMode() => BattlegroundsInfoReader.ReadSelectedGameMode(this.image);
        public BgsPlayerInfo GetBgsPlayerTeammateBoard() => BattlegroundsDuoInfoReader.ReadPlayerTeammateBoard(this.image);
        public BgsTeamInfo GetBgsPlayerBoard() => BattlegroundsPlayerInfoReader.ReadPlayerBoard(this.image);

        public IMatchInfo GetMatchInfo() => MatchInfoReader.ReadMatchInfo(this.image);

        public int GetBoard() => MatchInfoReader.RetrieveBoardInfo(this.image);

        public IDeck GetActiveDeck(long? selectedDeckId) => ActiveDeckReader.ReadActiveDeck(this.image, selectedDeckId);

        public IDeck GetWhizbangDeck(long whizbangDeckId) => ActiveDeckReader.ReadTemplateDeck(this.image, whizbangDeckId);

        public IReadOnlyList<IDeck> GetTemplateDecks() => ActiveDeckReader.ReadTemplateDecks(this.image);

        public IReadOnlyList<EventTiming> GetEventTimings() => EventTimingsReader.ReadEventTimings(this.image);

        public IList<RaceTag> GetRaceTags() => GameDbfReader.ReadRaceTags(this.image);

        public long? GetSelectedDeckId() => ActiveDeckReader.GetSelectedDeckId(this.image);

        public IArenaInfo GetArenaInfo() => ArenaInfoReader.ReadArenaInfo(this.image);
        public DraftSlotType? GetArenaDraftStep() => ArenaInfoReader.ReadDraftStep(this.image);
        public List<string> GetArenaHeroOptions() => ArenaInfoReader.ReadHeroOptions(this.image);
        public List<string> GetArenaCardOptions() => ArenaInfoReader.ReadCardOptions(this.image);
        public int? GetNumberOfCardsInArenaDraftDeck() => ArenaInfoReader.ReadNumberOfCardsInDeck(this.image);
        public IDeck GetArenaDeck() => ArenaInfoReader.ReadArenaDeck(this.image);

        public IOpenPacksInfo GetOpenPacksInfo() => OpenPacksInfoReader.ReadOpenPacksInfo(this.image);

        public List<PackInfo> GetOpenedPacks() => OpenPacksInfoReader.ReadOpenPackInfo(this.image);
        public List<PackInfo> GetMassOpenedPack() => OpenPacksInfoReader.ReadMassOpenPackInfo(this.image);

        public bool IsOpeningPack() => OpenPacksInfoReader.ReadIsOpeningPack(this.image);

        public IBoostersInfo GetBoostersInfo() => BoostersInfoReader.ReadBoostersInfo(this.image);

        public IAccountInfo GetAccountInfo() => AccountInfoReader.ReadAccountInfo(this.image);

        public SceneModeEnum? GetSceneMode() => SceneModeReader.ReadSceneMode(this.image);

        public int? GetMercenariesIsSelectingTreasures() => SceneModeReader.ReadMercenariesIsSelectingTreasures(this.image);

        public IDuelsPendingTreasureSelection GetDuelsPendingTreasureSelection() => DuelsInfoReader.ReadPendingTreasureSelection(this.image);

        //public bool IsMaybeOnDuelsRewardsScreen() => SceneModeReader.IsMaybeOnDuelsRewardsScreen(this.image);

        public RewardTrackInfos GetRewardTrackInfo() => RewardTrackInfoReader.ReadRewardTrack(this.image);

        //public IReadOnlyList<IXpChange> GetXpChanges() => RewardTrackInfoReader.ReadXpChanges(this.image);
        public bool HasXpChanges() => RewardTrackInfoReader.HasXpChanges(this.image);

        public bool IsDuelsRewardsPending() => DuelsRewardsInfoReader.ReadDuelsRewardsPending(this.image);

        public IDuelsRewardsInfo GetDuelsRewardsInfo() => DuelsRewardsInfoReader.ReadDuelsRewardsInfo(this.image);

        public int? GetNumberOfCompletedAchievements() => AchievementsInfoReader.ReadNumberOfCompletedAchievements(this.image);
        public IAchievementsInfo GetAchievementsInfo() => AchievementsInfoReader.ReadAchievementsInfo(this.image);
        public List<AchievementDbf> GetAchievementsDbf() => AchievementsInfoReader.ReadAchievementsDbf(this.image);
        public IList<AchievementCategory> GetAchievementCategories() => AchievementsInfoReader.ReadAchievementCategories(this.image);

        public IMercenariesInfo GetMercenariesInfo() => MercenariesInfoReader.ReadMercenariesInfo(this.image);

        public IMercenariesCollection GetMercenariesCollectionInfo() => MercenariesInfoReader.ReadMercenariesCollectionInfo(this.image);

        public IReadOnlyList<IMercenariesVisitor> GetMercenariesVisitors() => MercenariesInfoReader.ReadMercenariesVisitorsInfo(this.image);

        public IMercenariesPendingTreasureSelection GetMercenariesPendingTreasureSelection(int treasureIndex) => MercenariesInfoReader.ReadPendingTreasureSelection(this.image, treasureIndex);

        public IAchievementsInfo GetInGameAchievementsProgressInfo(int[] achievementIds) => 
            AchievementsInfoReader.ReadInGameAchievementsProgressInfo(this.image, achievementIds);
        public IAchievementsInfo GetInGameAchievementsProgressInfoByIndex(int[] indexes) => 
            AchievementsInfoReader.ReadInGameAchievementsProgressInfoByIndex(this.image, indexes);

        public bool IsDisplayingAchievementToast() => AchievementsInfoReader.IsDisplayingAchievementToast(this.image);

        public TurnTimer GetTurnTimer() => TurnTimerReader.ReadTurnTimer(this.image);

        public QuestsLog GetQuests() => QuestsReader.ReadQuests(this.image);

        public PlayerProfileInfo GetPlayerProfileInfo() => PlayerProfileInfoReader.ReadPlayerProfileInfo(this.image);

        public bool IsFriendsListOpen() => FriendsListReader.ReadFriendsListOpen(this.image);
        public MousedOverCard GetCurrentMousedOverCard() => InputManagerReader.ReadCurrentMousedOverCard(this.image);

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

        public List<string> ListServices()
        {
            // HearthstoneServices disappeared in 23.4, andI haven't found a better solution yet
            var dependencyBuilders = image["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"];
            if (dependencyBuilders == null)
            {
                return null;
            }

            var serviceLocator = dependencyBuilders[0]?["m_serviceLocator"];
            if (serviceLocator == null)
            {
                return null;
            }

            var services = serviceLocator["m_services"];
            if (services == null)
            {
                return null;
            }

            var serviceItems = services["_entries"];

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

            return serviceNames;
        }

        public List<string> ListNetCacheServices()
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

            return serviceNames;
        }

        public void ListObjects(string filter)
        {
            var result = image.TypeDefinitions
                .Where(t => HasField(t, "s_instance"))
                .Select(t => t.Name)
                .ToList();
            result.AddRange(ListServices());
            result.AddRange(ListNetCacheServices());
            result = result.Where(name => name != null).ToList();
            result.Sort();

            var filtered = filter == null ? result : result.Where(name => name.ToLower().Contains(filter.ToLower())).ToList();
            return;
        }

        private bool HasField(ITypeDefinition t, string fieldName)
        {
            try
            {
                var matchingFields = t.Fields.Where(f => f.Name.Contains(fieldName)).ToList();
                return matchingFields.Any(f => f.TypeInfo?.IsStatic ?? false);
                //var field = t.GetField(fieldName);
                //if (t.Name.Contains("CollectionManager"))
                //{
                //    return true;
                //}
                //return field?.TypeInfo?.IsStatic ?? false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}