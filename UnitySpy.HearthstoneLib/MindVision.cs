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

        public bool IsRunning() => Sanity.IsRunning(this.image);

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
                if (name == "DraftManager")
                {
                    var hop = "";
                }
                if (name == "AdventureProgressMgr")
                {
                    var hop = "";
                }
                if (name == "GenericRewardChestNoticeManager")
                {
                    var hop = "";
                }
                if (name == "FixedRewardsMgr")
                {
                    var hop = "";
                }
                i++;
            }

            return;
        }
    }
}