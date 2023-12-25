namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardsInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using JetBrains.Annotations;

    internal static class ArenaInfoReader
    {
        public static IArenaInfo ReadArenaInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            var heroCardId = draftManager["m_draftDeck"]?["<HeroCardID>k__BackingField"];
            return new ArenaInfo
            {
                Wins = draftManager["m_wins"] ?? -1,
                Losses = draftManager["m_losses"] ?? -1,
                HeroCardId = heroCardId,
                Deck = ReadArenaDeck(image),
                Rewards = RewardsInfoReader.ParseRewards(draftManager["m_chest"]?["<Rewards>k__BackingField"]?["_items"])
            };
        }

        public static IDeck ReadArenaDeck([NotNull] HearthstoneImage image)
        {
            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            List<string> decklist = new List<string>();
            var draftDeck = draftManager["m_draftDeck"];
            if (draftDeck == null)
            {
                return null;
            }

            var slots = draftDeck["m_slots"];
            var size = slots["_size"];
            var items = slots["_items"];
            for (var i = 0; i < size; i++)
            {
                var item = items[i];
                var cardId = item["m_cardId"];
                // Count is stored separately for normal + golden + diamond
                var cardCount = 0;
                var count = item["m_count"];
                var countSize = count["_size"];
                var countItems = count["_items"];
                for (var j = 0; j < countSize; j++)
                {
                    cardCount += countItems[j];
                }
                for (var j = 0; j < cardCount; j++)
                {
                    decklist.Add(cardId);
                }
            }

            var accountInfo = AccountInfoReader.ReadAccountInfo(image);
            var deckId = $"{accountInfo.Hi}-{accountInfo.Lo}-{draftDeck["ID"]}";
            return new Deck()
            {
                Id = deckId,
                DeckList = decklist,
                FormatType = draftDeck["<FormatType>k__BackingField"],
                HeroCardId = draftDeck["<HeroCardID>k__BackingField"],
                HeroPowerCardId = draftDeck["HeroPowerCardID"],
                Name = null,
            };
        }

        public static DraftSlotType? ReadDraftStep([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }

            if (draftDisplay["m_currentMode"] != 2)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }

            var currentSlot = draftManager["m_currentSlotType"];
            return (DraftSlotType)currentSlot;
        }

        public static List<string> ReadHeroOptions([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            if (draftDisplay["m_currentMode"] != 2)
            {
                return null;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            var currentSlot = draftManager["m_currentSlotType"];
            if (currentSlot != (int)DraftSlotType.DRAFT_SLOT_HERO)
            {
                return null;
            }

            var choices = draftDisplay["m_choices"];
            var numberOfOptions = choices["_size"];
            var result = new List<string>();
            for (int i = 0; i < numberOfOptions; i++)
            {
                var option = choices["_items"][i];
                result.Add(option["m_cardID"]);
            }
            return result;
        }

        public static List<string> ReadCardOptions([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            if (draftDisplay["m_currentMode"] != 2)
            {
                return null;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            var currentSlot = draftManager["m_currentSlotType"];
            if (currentSlot != (int)DraftSlotType.DRAFT_SLOT_CARD)
            {
                return null;
            }

            var choices = draftDisplay["m_choices"];
            var numberOfOptions = choices["_size"];
            var result = new List<string>();
            for (int i = 0; i < numberOfOptions; i++)
            {
                var option = choices["_items"][i];
                result.Add(option["m_cardID"]);
            }
            return result;
        }

        public static int? ReadNumberOfCardsInDeck(HearthstoneImage image)
        {
            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            // Check that the current deck is not complete
            var numberOfCardsInDeck = draftManager["m_draftDeck"]?["m_slots"]?["_size"] ?? null;
            var numberOfCardsInSideboards = 0;
            int nbSideboards = draftManager["m_draftDeck"]?["m_sideboardManager"]?["m_sideboards"]?["_count"] ?? 0;
            for (var i = 0; i < nbSideboards; i++)
            {
                var sideboard = draftManager["m_draftDeck"]["m_sideboardManager"]["m_sideboards"]["_entries"][i]["value"];
                if (sideboard != null)
                {
                    var nbCardsInSideboard = sideboard["m_slots"]["_size"];
                    numberOfCardsInSideboards += nbCardsInSideboard;
                }
            }
            return numberOfCardsInDeck + numberOfCardsInSideboards;
        }

        //public static Deck ReadArenaDeck(HearthstoneImage image)
        //{
        //    var draftManager = image.GetService("DraftManager");
        //    if (draftManager == null)
        //    {
        //        return null;
        //    }

        //    var memDeck = draftManager["m_draftDeck"];
        //    var decklist = new List<string>();
        //    string heroCardId = null;
        //    string heroPowerCardId = null;
        //    if (memDeck != null)
        //    {
        //        var slots = memDeck["m_slots"];
        //        var size = slots["_size"];
        //        var items = slots["_items"];
        //        for (var i = 0; i < size; i++)
        //        {
        //            var item = items[i];
        //            var cardId = item["m_cardId"];
        //            // Count is stored separately for normal + golden + diamond
        //            var cardCount = 0;
        //            var count = item["m_count"];
        //            var countSize = count["_size"];
        //            var countItems = count["_items"];
        //            for (var j = 0; j < countSize; j++)
        //            {
        //                cardCount += countItems[j];
        //            }
        //            for (var j = 0; j < cardCount; j++)
        //            {
        //                decklist.Add(cardId);
        //            }
        //        }
        //        heroCardId = memDeck["<HeroCardID>k__BackingField"];
        //        heroPowerCardId = memDeck["HeroPowerCardID"];
        //    }

        //    return new Deck()
        //    {
        //        HeroCardId = heroCardId,
        //        HeroPowerCardId = heroPowerCardId,
        //        DeckList = decklist,
        //    };
        //}
    }
}
