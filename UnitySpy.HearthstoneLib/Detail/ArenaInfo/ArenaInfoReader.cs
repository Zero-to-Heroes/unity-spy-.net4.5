namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
    using HackF5.UnitySpy.HearthstoneLib.Detail.GameDbf;
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

            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var draftDeck = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundDraftDeck"] : draftManager["m_draftDeck"];
            var heroCardId = draftDeck?["<HeroCardID>k__BackingField"];
            var wins = (gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundWins"] : draftManager["m_wins"]) ?? -1;
            var losses = (gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundLosses"] : draftManager["m_losses"]) ?? -1;
            return new ArenaInfo
            {
                GameType = gameType,
                Wins = wins,
                Losses = losses,
                HeroCardId = heroCardId,
                Deck = ReadArenaDeck(image),
                Rewards = RewardsInfoReader.ParseRewards(draftManager["m_chest"]?["<Rewards>k__BackingField"]?["_items"]),
            };
        }

        public static int? ReadArenaCurrentDraftSlot([NotNull] HearthstoneImage image)
        {
            return ReadCurrentDraftSlot(image, GameType.GT_ARENA);
        }
        public static int? ReadArenaUndergroundCurrentDraftSlot([NotNull] HearthstoneImage image)
        {
            return ReadCurrentDraftSlot(image, GameType.GT_UNDERGROUND_ARENA);
        }
        private static int? ReadCurrentDraftSlot([NotNull] HearthstoneImage image, GameType gameType)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            var currentMode = draftDisplay["m_currentMode"];
            if (currentMode != (int)DraftMode.DRAFTING && currentMode != (int)DraftMode.REDRAFTING)
            {
                return null;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            int slotType = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlotType"] : draftManager["m_currentSlotType"];
            if (currentMode != (int)DraftMode.REDRAFTING && slotType != (int)DraftSlotType.DRAFT_SLOT_CARD)
            {
                return null;
            }

            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlot"] : draftManager["m_currentSlot"];
            var currentRedraftSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundRedraftSlot"] : draftManager["m_currentRedraftSlot"];
            return currentSlot + Math.Max(0, currentRedraftSlot);
        }

        public static ArenaCardPick ReadArenaLatestCardPick([NotNull] HearthstoneImage image)
        {
            return ReadLatestCardPick(image, GameType.GT_ARENA);
        }

        public static ArenaCardPick ReadArenaUndergroundLatestCardPick([NotNull] HearthstoneImage image)
        {
            return ReadLatestCardPick(image, GameType.GT_UNDERGROUND_ARENA);
        }

        private static ArenaCardPick ReadLatestCardPick([NotNull] HearthstoneImage image, GameType gameType)
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

            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            var draftDeck = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundDraftDeck"] : draftManager["m_draftDeck"];
            var accountInfo = AccountInfoReader.ReadAccountInfo(image);
            var deckId = $"{accountInfo.Hi}-{accountInfo.Lo}-{draftDeck["ID"]}";
            var heroCardId = draftDeck["<HeroCardID>k__BackingField"];

            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlot"] : draftManager["m_currentSlot"];
            var currentRedraftSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundRedraftSlot"] : draftManager["m_currentRedraftSlot"];
            var losses = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundLosses"] : 0;
            // -1 because when we call this, the current slot has already changed to the next
            // i.e. we are picking card number 21, slot changes to 22, then triggers the last pick detection
            var pickNumber = (currentSlot - 1) + Math.Max(0, losses - 1) * 5 + Math.Max(0, currentRedraftSlot);

            var choices = ReadCardOptions(image);

            var pickIndex = draftManager["m_chosenIndex"];
            // It's 1-based
            var cardId = choices[pickIndex - 1]?.CardId;

            var cardPick = new ArenaCardPick()
            {
                GameType = gameType,
                RunId = deckId,
                PickNumber = pickNumber,
                CardId = cardId,
                Options = choices,
                HeroCardId = heroCardId,
            };
            return cardPick;
        }

        public static GameType? ReadArenaDraftGameType([NotNull] HearthstoneImage image)
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

            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            return gameType;
        }

        public static IDeck ReadArenaDeck([NotNull] HearthstoneImage image)
        {
            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var draftDeck = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundDraftDeck"] : draftManager["m_draftDeck"];
            if (draftDeck == null)
            {
                return null;
            }

            var slots = draftDeck["m_slots"];
            List<string> decklist = new List<string>();
            AddCardsFromSlots(slots, decklist);

            if (gameType == GameType.GT_UNDERGROUND_ARENA)
            {
                var isEditing = (draftManager["m_currentClientState"] == (int)ArenaClientStateType.Underground_DeckEdit
                    || draftManager["m_currentClientState"] == (int)ArenaClientStateType.Normal_DeckEdit)
                    || draftManager["m_undergroundSessionState"] == (int)ArenaSessionState.EDITING_DECK 
                    || draftManager["m_normalSessionState"] == (int)ArenaSessionState.EDITING_DECK;

                if (!isEditing)
                {
                    var redraftDeck = draftManager["m_undergroundRedraftDeck"];
                    var redraftSlots = redraftDeck?["m_slots"];
                    AddCardsFromSlots(redraftSlots, decklist);
                }
            }


            var accountInfo = AccountInfoReader.ReadAccountInfo(image);
            var deckId = $"{accountInfo.Hi}-{accountInfo.Lo}-{draftDeck["ID"]}";
            return new Deck()
            {
                Id = deckId,
                DeckList = decklist,
                FormatType = draftDeck["<FormatType>k__BackingField"],
                GameType = gameType,
                HeroCardId = draftDeck["<HeroCardID>k__BackingField"],
                HeroPowerCardId = draftDeck["HeroPowerCardID"],
                Name = null,
            };
        }

        private static void AddCardsFromSlots(dynamic slots, List<string> decklist)
        {
            if (slots == null)
            {
                return;
            }

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
        }

        public static bool IsShowingScreenOverDraft([NotNull] HearthstoneImage image)
        {
            try
            {
                var draftDisplay = image["DraftDisplay"]?["s_instance"];
                if (draftDisplay == null)
                {
                    return false;
                }

                var legendaryBucketDetailsPopup = draftDisplay["m_packageCardsPopup"]?["m_chooseButtonVisualController"]?["m_enabledInternally"] ?? false;
                if (legendaryBucketDetailsPopup)
                {
                    return true;
                }

                var draftManager = image.GetService("DraftManager");
                if (draftManager == null)
                {
                    return false;
                }

                int clientState = draftManager["m_currentClientState"];
                if (clientState == (int)ArenaClientStateType.Normal_Landing || clientState == (int)ArenaClientStateType.Underground_Landing)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Log($"Exception when trying to IsShowingScreenOverDraft: {e.ToString()}");
                return false;
            }
        }

        public static DraftSlotType? ReadDraftStep([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }

            var currentMode = draftDisplay["m_currentMode"];
            if (currentMode == (int)DraftMode.REDRAFTING)
            {
                return DraftSlotType.DRAFT_SLOT_CARD;
            }

            if (draftDisplay["m_currentMode"] != (int)DraftMode.DRAFTING)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return DraftSlotType.DRAFT_SLOT_NONE;
            }


            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlotType"] : draftManager["m_currentSlotType"];
            return (DraftSlotType)currentSlot;
        }

        public static DraftMode? ReadDraftMode([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            var currentMode = draftDisplay["m_currentMode"];
            return (DraftMode)currentMode;
        }

        public static ArenaClientStateType? ReadClientState([NotNull] HearthstoneImage image)
        {
            try
            {
                var draftDisplay = image["DraftDisplay"]?["s_instance"];
                if (draftDisplay == null)
                {
                    return null;
                }

                var draftManager = image.GetService("DraftManager");
                if (draftManager == null)
                {
                    return null;
                }

                int clientState = draftManager["m_currentClientState"];
                return (ArenaClientStateType)clientState;
            }
            catch (Exception e)
            {
                Logger.Log($"Exception when trying to ReadClientState: {e.ToString()}");
                return null;
            }
        }

        public static ArenaSessionState? ReadSessionState([NotNull] HearthstoneImage image)
        {
            try
            {
                var draftManager = image.GetService("DraftManager");
                if (draftManager == null)
                {
                    return null;
                }

                var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
                int sessionState = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundSessionState"] : draftManager["m_normalSessionState"];
                return (ArenaSessionState)sessionState;
            }
            catch (Exception e)
            {
                Logger.Log($"Exception when trying to ReadClientState: {e.ToString()}");
                return null;
            }
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

            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlotType"] : draftManager["m_currentSlotType"];
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

        public static List<ArenaCardOption> ReadCardOptions([NotNull] HearthstoneImage image)
        {
            var draftDisplay = image["DraftDisplay"]?["s_instance"];
            if (draftDisplay == null)
            {
                return null;
            }

            var currentMode = draftDisplay["m_currentMode"];
            if (currentMode != (int)DraftMode.DRAFTING && currentMode != (int)DraftMode.REDRAFTING)
            {
                return null;
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            // Issue: the slotType changes before the cards change
            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlotType"] : draftManager["m_currentSlotType"];
            if (currentMode != (int)DraftMode.REDRAFTING && currentSlot != (int)DraftSlotType.DRAFT_SLOT_CARD)
            {
                return null;
            }

            // Check that the hero and hero power have been chosen
            if (draftDisplay["m_chosenHero"] == null || draftDisplay["m_heroPower"] == null)
            {
                return null;
            }

            var choices = draftDisplay["m_choices"];
            var numberOfOptions = choices["_size"];
            var result = new List<ArenaCardOption>();
            for (int i = 0; i < numberOfOptions; i++)
            {
                var option = choices["_items"][i];
                var packageCardIds = new List<string>();
                var memCardIds = option["m_packageCardIds"];
                var count = memCardIds?["_size"] ?? 0;
                for (int j = 0; j < count; j++)
                {
                    packageCardIds.Add(memCardIds["_items"][j]);
                }
                result.Add(new ArenaCardOption()
                {
                    CardId = option["m_cardID"],
                    PackageCardIds = packageCardIds,
                });
            }
            return result;
        }

        public static List<string> ReadPackageCardOptions([NotNull] HearthstoneImage image)
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

            // Issue: the slotType changes before the cards change
            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var currentSlot = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_currentUndergroundSlotType"] : draftManager["m_currentSlotType"];
            if (currentSlot != (int)DraftSlotType.DRAFT_SLOT_CARD)
            {
                return null;
            }

            // Check that the hero and hero power have been chosen
            if (draftDisplay["m_chosenHero"] == null || draftDisplay["m_heroPower"] == null)
            {
                return null;
            }

            var isOpen = draftDisplay["m_packageCardsPopup"]?["m_chooseButtonVisualController"]?["m_enabledInternally"] ?? false;
            if (!isOpen)
            {
                return new List<string>();
            }

            var popup = draftDisplay["m_packageCardsPopup"]?["m_relatedCardsTray"];
            var memCards = popup["m_cardList"]?["m_list"];
            var numberOfOptions = memCards["_size"];
            var result = new List<string>();
            for (int i = 0; i < numberOfOptions; i++)
            {
                var option = memCards["_items"][i];
                result.Add(option["m_CardId"]);
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
            var gameType = draftManager["m_undergroundActive"] == true ? GameType.GT_UNDERGROUND_ARENA : GameType.GT_ARENA;
            var draftDeck = gameType == GameType.GT_UNDERGROUND_ARENA ? draftManager["m_undergroundDraftDeck"] : draftManager["m_draftDeck"];

            int numberOfCardsInDeck = 0;
            var slots = draftDeck?["m_slots"];
            int numberOfDifferentCardsInDeck = slots?["_size"] ?? 0;
            for (var i = 0; i < numberOfDifferentCardsInDeck; i++)
            {
                var slot = slots["_items"][i];
                var count = slot["m_count"];
                var countSize = count["_size"];
                for (var j = 0; j < countSize; j++)
                {
                    var countItem = count["_items"][j];
                    numberOfCardsInDeck += countItem;
                }
            }

            var numberOfCardsInSideboards = 0;
            int nbSideboards = draftDeck?["m_sideboardManager"]?["m_sideboards"]?["_count"] ?? 0;
            for (var i = 0; i < nbSideboards; i++)
            {
                var sideboard = draftDeck["m_sideboardManager"]["m_sideboards"]["_entries"][i]["value"];
                if (sideboard != null)
                {
                    var nbCardsInSideboard = sideboard["m_slots"]["_size"];
                    numberOfCardsInSideboards += nbCardsInSideboard;
                }
            }

            int numberOfCardsInRedraftDeck = 0;
            if (gameType == GameType.GT_UNDERGROUND_ARENA)
            {
                var redraftSlots = draftManager["m_undergroundRedraftDeck"]?["m_slots"];
                int numberOfDifferentCardsInRedraftDeck = redraftSlots?["_size"] ?? 0;
                for (var i = 0; i < numberOfDifferentCardsInRedraftDeck; i++)
                {
                    var slot = redraftSlots["_items"][i];
                    var count = slot["m_count"];
                    var countSize = count["_size"];
                    for (var j = 0; j < countSize; j++)
                    {
                        var countItem = count["_items"][j];
                        numberOfCardsInRedraftDeck += countItem;
                    }
                }
            }

            return numberOfCardsInDeck + numberOfCardsInSideboards + numberOfCardsInRedraftDeck;
        }
    }
}
