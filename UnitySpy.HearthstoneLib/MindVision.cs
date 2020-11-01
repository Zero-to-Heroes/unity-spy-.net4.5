﻿namespace HackF5.UnitySpy.HearthstoneLib
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
    }
}