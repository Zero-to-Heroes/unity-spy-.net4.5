namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Duels;
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

            var isValid = false;
            try
            {
                isValid = IsValid(image);
            } catch (Exception e)
            {
                // Do nothing
            }

            if (isValid)
            {
                var selectedDeckId = GetSelectedDeckId(image) ?? 0;
                if (selectedDeckId != 0)
                {
                    var deckMemory = GetDeckMemory(image);
                    if (deckMemory == null)
                    {
                        return null;
                    }
                    var count = deckMemory["count"];
                    var deckIds = GetDeckIds(image);
                    if (deckIds == null)
                    {
                        return null;
                    }
                    var deckIndex = -1;
                    for (int i = 0; i < count; i++)
                    {
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

            var matchInfo = MatchInfoReader.ReadMatchInfo(image);
            if (matchInfo == null)
            {
                return null;
            }

            switch (matchInfo.GameType)
            {
                case GameType.GT_ARENA:
                    return GetArenaDeck(image);
                case GameType.GT_CASUAL:
                    return GetCasualDeck(image);
                case GameType.GT_RANKED:
                    return GetRankedDeck(image);
                case GameType.GT_VS_AI:
                    return GetSoloDeck(image, matchInfo.MissionId);
                case GameType.GT_VS_FRIEND:
                    return GetFriendlyDeck(image);
                case GameType.GT_PVPDR:
                case GameType.GT_PVPDR_PAID:
                    return GetDuelsDeck(image);

                default: return null;
            }
        }

        private static dynamic GetDeckIds(HearthstoneImage image)
        {
            return image["CollectionManager"]["s_instance"]["m_decks"]["keySlots"];
        }

        private static dynamic GetDeckMemory(HearthstoneImage image)
        {
            return image["CollectionManager"]["s_instance"]["m_decks"];
        }

        private static long? GetSelectedDeckId(HearthstoneImage image)
        {
            return image["DeckPickerTrayDisplay"]["s_instance"]["m_selectedCustomDeckBox"]["m_deckID"];
        }

        private static bool IsValid(HearthstoneImage image)
        {
            var valid = IsBigValid(image) && image["DeckPickerTrayDisplay"]["s_instance"] != null;
            if (!valid)
            {
                return false;
            }

            var deckPicker = image["DeckPickerTrayDisplay"]["s_instance"];
            bool isSharing = deckPicker["m_usingSharedDecks"] ?? false;
            if (isSharing)
            {
                return false;
            }

            return deckPicker["m_selectedCustomDeckBox"] != null
                && image["CollectionManager"]["s_instance"] != null
                && image["CollectionManager"]["s_instance"]["m_decks"] != null;
        }

        private static bool IsBigValid(HearthstoneImage image)
        {
            return image["DeckPickerTrayDisplay"] != null && image["CollectionManager"] != null;
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
                HeroCardId = deck["HeroCardID"],
                IsWild = deck["m_isWild"],
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

        private static IDeck GetDuelsDeck(HearthstoneImage image)
        {
            return new Deck
            {
                DeckList = DuelsInfoReader.ReadDuelsInfo(image)?.DeckList?.Select(dbfId => dbfId.ToString())?.ToList(),
            };
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
