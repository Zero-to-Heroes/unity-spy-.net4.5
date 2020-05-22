namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Match;
    using JetBrains.Annotations;

    internal static class ActiveDeckReader
    {
        public static IDeck ReadActiveDeck([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var selectedDeckId = image?["DeckPickerTrayDisplay"]?["s_instance"]?["m_selectedCustomDeckBox"]?["m_deckID"] ?? 0;
            if (selectedDeckId != 0)
            {
                var deckMemory = image["CollectionManager"]?["s_instance"]?["m_decks"];
                if (deckMemory != null)
                {
                    var count = deckMemory?["count"];
                    var deckIds = image?["CollectionManager"]["s_instance"]?["m_decks"]?["keySlots"];
                    var deckIndex = -1;
                    for (int i = 0; i < count; i++) {
                        if (deckIds[i] == selectedDeckId)
                        {
                            deckIndex = i;
                            break;
                        }
                    }
                    if (deckIndex >= 0)
                    {
                        var deck = deckMemory["valueSlots"][deckIndex];
                        var fullDeck = GetDynamicDeck(deck);
                        if (fullDeck != null)
                        {
                            return fullDeck;
                        }
                    }
                }
            }

            var gameState = image["GameState"]["s_instance"];
            if (gameState == null)
            {
                return null;
            }

            var matchInfo = MatchInfoReader.ReadMatchInfo(image);
            switch (matchInfo.GameType)
            {
                case GameType.GT_ARENA: return GetArenaDeck(image);
                case GameType.GT_CASUAL: return GetCasualDeck(image);
                case GameType.GT_RANKED: return GetRankedDeck(image);
                case GameType.GT_VS_AI: return GetSoloDeck(image, matchInfo.MissionId);
                case GameType.GT_VS_FRIEND: return GetFriendlyDeck(image);
                default: return null;
            }
        }

        private static IDeck GetDynamicDeck(dynamic deck)
        {
            if (deck == null)
            {
                return null;
            }
            var cardList = deck["m_slots"];
            var count = cardList["_size"];
            var cards = cardList["_items"];
            var deckList = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var card = cards[i];
                var copies = 0;
                var counts = card["m_count"];
                for (var j = 0; j < counts["_size"]; j++)
                {
                    copies += (int)counts["_items"][j];
                }
                for (int j = 0; j < copies; j++)
                {
                    deckList.Add(card["m_cardId"]);
                }
            }
            return new Deck
            {
                Name = deck["m_name"],
                DeckList = deckList,
            };
        }

        private static IDeck GetArenaDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetCasualDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetRankedDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetSoloDeck(HearthstoneImage image, int missionId)
        {
            Console.WriteLine("Getting solo deck for missionId: " + missionId);
            var deckList = GetSoloDeckList(image, missionId);
            if (deckList == null)
            {
                return null;
            }
            return new Deck
            {
                DeckList = deckList.Select(dbfId => dbfId.ToString()).ToList(),
            };
        }

        private static IReadOnlyList<int> GetSoloDeckList(HearthstoneImage image, int missionId)
        {
            var dungeonInfo = DungeonInfoReader.ReadCollection(image);
            switch (missionId)
            {
                case 2663:
                    return dungeonInfo?[DungeonKey.DungeonRun]?.DeckList;
                case 2706:
                case 2821:
                    return dungeonInfo?[DungeonKey.MonsterHunt]?.DeckList;
                case 2890:
                    return dungeonInfo?[DungeonKey.RumbleRun]?.DeckList;
                case 3005:
                case 3188:
                case 3189:
                case 3190:
                case 3191:
                case 3236:
                    return dungeonInfo?[DungeonKey.DalaranHeist]?.DeckList;
                case 3328:
                case 3329:
                case 3330:
                case 3331:
                case 3332:
                case 3359:
                    return dungeonInfo?[DungeonKey.DalaranHeistHeroic]?.DeckList;
                case 3428:
                case 3429:
                case 3430:
                case 3431:
                case 3432:
                case 3438:
                    return dungeonInfo?[DungeonKey.TombsOfTerror]?.DeckList;
                case 3433:
                case 3434:
                case 3435:
                case 3436:
                case 3437:
                case 3439:
                    return dungeonInfo?[DungeonKey.TombsOfTerrorHeroic]?.DeckList;
            }

            Console.WriteLine($"Unsupported scenario id: {missionId}.");
            return null;
        }

        private static IDeck GetFriendlyDeck(HearthstoneImage image)
        {
            return null;
        }
    }
}
