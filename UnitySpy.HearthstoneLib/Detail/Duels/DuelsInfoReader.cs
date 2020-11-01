namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using JetBrains.Annotations;

    internal static class DuelsInfoReader
    {
        public static IDuelsInfo ReadDuelsInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var duelsMetaInfo = BuildDuelsMetaInfo(image);
            var dungeonInfo = BuildDungeonInfo(image);

            return new DuelsInfo
            {
                Wins = duelsMetaInfo?.Wins ?? -1,
                Losses = duelsMetaInfo?.Losses ?? -1,
                Rating = duelsMetaInfo?.Rating ?? -1,
                PaidRating = duelsMetaInfo?.PaidRating ?? -1,
                LastRatingChange = duelsMetaInfo?.LastRatingChange ?? -1,
                DeckList = dungeonInfo?.DeckList,
                StartingHeroPower = dungeonInfo?.StartingHeroPower ?? -1,
                LootOptionBundles = dungeonInfo?.LootOptionBundles,
                ChosenLoot = dungeonInfo?.ChosenLoot ?? -1,
                TreasureOption = dungeonInfo?.TreasureOption,
                ChosenTreasure = dungeonInfo?.ChosenTreasure ?? -1,
            };
        }

        private static IDungeonInfo BuildDungeonInfo([NotNull] HearthstoneImage image)
        {
            var savesMap = image["GameSaveDataManager"]?["s_instance"]?["m_gameSaveDataMapByKey"];
            if (savesMap != null)
            {
                return DungeonInfoReader.BuildDungeonInfo(image, DungeonKey.Duels, savesMap);
            }
            return null;

            //var deckList = new List<string>();
            //if (dungeonInfo["m_dungeonCrawlDisplay"] != null
            //    && dungeonInfo["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"] != null
            //    && dungeonInfo["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"]["m_slots"] != null)
            //{
            //    var slots = dungeonInfo["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"]["m_slots"];
            //    var size = slots["_size"];
            //    var items = slots["_items"];
            //    for (var i = 0; i < size; i++)
            //    {
            //        var card = items[i];
            //        var cardId = card["m_cardId"];
            //        var numberOfCards = 0;
            //        var count = card["m_count"];
            //        var countItems = count["_items"];
            //        var countSize = count["_size"];
            //        for (int j = 0; j < countSize; j++)
            //        {
            //            if (countItems[j] > 0)
            //            {
            //                numberOfCards = countItems[j];
            //            }
            //        }
            //        for (int j = 0; j < numberOfCards; j++)
            //        {
            //            deckList.Add(cardId);
            //        }
            //    }
            //}
        }


        private static DuelsMetaInfo BuildDuelsMetaInfo([NotNull] HearthstoneImage image)
        {
            if (image["PvPDungeonRunScene"] == null || image["PvPDungeonRunScene"]["m_instance"] == null)
            {
                return ReadDuelsInfoFromDisplay(image);
            }

            var dungeonInfo = image["PvPDungeonRunScene"]["m_instance"];
            if (dungeonInfo == null)
            {
                return ReadDuelsInfoFromDisplay(image);
            }
            var wins = -1;
            var losses = -1;
            var rating = -1;
            var paidRating = -1;
            var lastRatingChange = -1;
            if (dungeonInfo["m_display"] != null && dungeonInfo["m_display"]["m_dataModel"] != null)
            {
                var display = dungeonInfo["m_display"]["m_dataModel"];
                wins = display["m_Wins"];
                losses = display["m_Losses"];
                rating = display["m_Rating"];
                paidRating = display["m_PaidRating"];
                lastRatingChange = display["m_LastRatingChange"];
            }
            return new DuelsMetaInfo
            {
                Wins = wins,
                Losses = losses,
                Rating = rating,
                PaidRating = paidRating,
                LastRatingChange = lastRatingChange,
            };
        }

        private static DuelsMetaInfo ReadDuelsInfoFromDisplay([NotNull] HearthstoneImage image)
        {
            if (image["PvPDungeonRunDisplay"] == null
                || image["PvPDungeonRunDisplay"]["m_instance"] == null
                || image["PvPDungeonRunDisplay"]["m_instance"]["m_dataModel"] == null)
            {
                return null;
            }

            var display = image["PvPDungeonRunDisplay"]["m_instance"]["m_dataModel"];
            var wins = display["m_Wins"];
            var losses = display["m_Losses"];
            var rating = display["m_Rating"];
            var paidRating = display["m_PaidRating"];
            var lastRatingChange = display["m_LastRatingChange"];

            return new DuelsMetaInfo
            {
                Wins = wins,
                Losses = losses,
                Rating = rating,
                PaidRating = paidRating,
                LastRatingChange = lastRatingChange,
            };
        }
    }

    internal class DuelsMetaInfo
    {

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Rating { get; set; }

        public int LastRatingChange { get; set; }

        public int PaidRating { get; set; }
    }
}