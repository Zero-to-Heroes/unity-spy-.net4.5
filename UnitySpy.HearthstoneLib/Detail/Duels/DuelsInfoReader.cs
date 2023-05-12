namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
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
            // Works even before the first match?
            var duelsDeck = ReadDuelsDeck(image);

            return new DuelsInfo
            {
                Wins = duelsMetaInfo?.Wins ?? -1,
                Losses = duelsMetaInfo?.Losses ?? -1,
                Rating = duelsMetaInfo?.Rating ?? -1,
                PaidRating = duelsMetaInfo?.PaidRating ?? -1,
                LastRatingChange = duelsMetaInfo?.LastRatingChange ?? -1,
                DeckList = dungeonInfo?.DeckList,
                DeckListWithCardIds = duelsDeck?.Decklist,
                Sideboards = duelsDeck?.Sideboards,
                HeroCardId = duelsDeck?.HeroCardId,
                StartingHeroPower = dungeonInfo?.StartingHeroPower ?? -1,
                StartingHeroPowerCardId = duelsDeck?.HeroPowerCardId,
                LootOptionBundles = dungeonInfo?.LootOptionBundles,
                ChosenLoot = dungeonInfo?.ChosenLoot ?? -1,
                TreasureOption = dungeonInfo?.TreasureOption,
                ChosenTreasure = dungeonInfo?.ChosenTreasure ?? -1,
                PlayerClass = dungeonInfo?.PlayerClass ?? -1,
                RunActive = dungeonInfo.RunActive,
            };
        }

        public static bool ReadDuelsIsOnMainScreen(HearthstoneImage image)
        {

            return image["PvPDungeonRunScene"] != null
                && image["PvPDungeonRunScene"]["m_instance"] != null
                && image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"] != null
                && image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"] != null;
        }

        public static bool ReadDuelsIsOnHeroPickerScreen(HearthstoneImage image)
        {
            var currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return false;
            }

            if (image["PvPDungeonRunScene"]?["m_instance"] == null)
            {
                return false;
            }

            var playButton = image["PvPDungeonRunScene"]["m_instance"]?["m_display"]?["m_playButton"];
            if (playButton == null)
            {
                return false;
            }

            // The play button is enabled on the "landing page" screen with the Play/Resume button, and disabled 
            // on the hero picker screen
            var playButtonEnabled = playButton["m_enabled"];
            if (playButtonEnabled)
            {
                return false;
            }

            // If yoiu're on the Casual / Heroic selection screen, you can have an exception as the HeroPicker is 
            // not created yet
            var storeOpen = image["PvPDungeonRunScene"]["m_instance"]["m_PopupManager"]?["m_isStoreOpen"] ?? false;
            if (storeOpen)
            {
                return false;
            }

            // Now the hero picker should be initialized
            if (image["GuestHeroPickerDisplay"]?["s_instance"] == null)
            {
                return false;
            }

            // Check if the "resume" button is visible?
            // m_CachedPtr is 0 when the picker is hidden?
            if (image["GuestHeroPickerDisplay"]["s_instance"]["m_CachedPtr"] == 0)
            {
                return false;
            }

            return true;
        }

        public static bool ReadDuelsIsOnDeckBuildingLobbyScreen(HearthstoneImage image)
        {
            var currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return false;
            }

            if (image["PvPDungeonRunScene"]?["m_instance"] == null)
            {
                return false;
            }

            var display = image["PvPDungeonRunScene"]["m_instance"]?["m_display"];
            var playButton = display?["m_playButton"];
            // The active / inactive is not reliable - if we come come the signature treasure selection, it's
            // enabled, but if we come from the Game Modes hub, it's disabled
            if (playButton == null)
            {
                return false;
            }

            var sessionActive = display["m_dataModel"]["m_IsSessionActive"];
            if (sessionActive)
            {
                return false;
            }

            var dungeonCrawlDisplay = image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"];
            if (dungeonCrawlDisplay == null)
            {
                return false;
            }

            // Check that the current deck is not complete
            var numberOfCardsInDeck = dungeonCrawlDisplay["m_dungeonCrawlDeck"]?["m_slots"]?["_size"] ?? 0;
            if (numberOfCardsInDeck == 0 || numberOfCardsInDeck >= 16)
            {
                return false;
            }

            // How to rule out "actively building a deck"?
            var presence = image["PresenceMgr"]["s_instance"]?["m_currentStatus"]?["_PresenceId"];
            if (presence != 77) // DUELS_IDLE
            {
                return false;
            }

            return true;
        }

        public static int? ReadNumberOfCardsInDeck(HearthstoneImage image)
        {
            var currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return null;
            }

            if (image["PvPDungeonRunScene"]?["m_instance"] == null)
            {
                return null;
            }

            var dungeonCrawlDisplay = image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"];
            if (dungeonCrawlDisplay == null)
            {
                return null;
            }

            // Check that the current deck is not complete
            var numberOfCardsInDeck = dungeonCrawlDisplay["m_dungeonCrawlDeck"]?["m_slots"]?["_size"] ?? null;
            var numberOfCardsInSideboards = 0;
            int nbSideboards = dungeonCrawlDisplay["m_dungeonCrawlDeck"]?["m_sideboardManager"]?["m_sideboards"]?["_count"] ?? 0;
            for (var i = 0; i < nbSideboards; i++)
            {
                var sideboard = dungeonCrawlDisplay["m_dungeonCrawlDeck"]["m_sideboardManager"]["m_sideboards"]["_entries"][i]["value"];
                if (sideboard != null)
                {
                    var nbCardsInSideboard = sideboard["m_slots"]["_size"];
                    numberOfCardsInSideboards += nbCardsInSideboard;
                }
            }
            return numberOfCardsInDeck + numberOfCardsInSideboards;
        }

        public static IReadOnlyList<int> ReadDuelsHeroOptions(HearthstoneImage image)
        {
            var result = new List<int>();
            if (image["GuestHeroPickerDisplay"]?["s_instance"] == null)
            {
                return result;
            }

            var heroPicker = image["GuestHeroPickerDisplay"]["s_instance"];
            var heroButtons = heroPicker["m_heroPickerTray"]?["m_heroButtons"];
            if (heroButtons == null)
            {
                return result;
            }

            var buttonsCount = heroButtons["_size"];
            var items = heroButtons["_items"];
            for (var i = 0; i < buttonsCount; i++)
            {
                var heroButton = items[i];
                var hero = heroButton["m_guestHero"];
                result.Add(hero["m_cardId"]);
            }
            return result;
        }

        public static IReadOnlyList<IDuelsHeroPowerOption> ReadDuelsHeroPowerOptions(HearthstoneImage image)
        {
            var result = new List<IDuelsHeroPowerOption>();
            var playMat = image["PvPDungeonRunScene"]?["m_instance"]?["m_dungeonCrawlDisplay"]?["m_dungeonCrawlPlayMatReference"]?["<Object>k__BackingField"];
            if (playMat == null)
            {
                return result;
            }

            var options = playMat["m_heroPowerOptions"];
            var size = options?["_size"] ?? 0;
            if (size == 0)
            {
                return result;
            }

            var items = options["_items"];
            for (var i = 0; i < size; i++)
            {
                var option = items[i];
                var dataModel = option["m_dataModel"];
                result.Add(new DuelsHeroPowerOption()
                {
                    DatabaseId = option["m_databaseId"],
                    Enabled = option["m_isEnabled"],
                    Visible = option["m_isVisible"],
                    Completed = dataModel["m_Completed"],
                    Locked = dataModel["m_Locked"],
                    Selected = dataModel["m_IsSelectedOption"],
                });
            }

            return result;
        }


        public static IReadOnlyList<IDuelsHeroPowerOption> ReadDuelsSignatureTreasureOptions(HearthstoneImage image)
        {
            var result = new List<IDuelsHeroPowerOption>();
            var playMat = image["PvPDungeonRunScene"]?["m_instance"]?["m_dungeonCrawlDisplay"]?["m_dungeonCrawlPlayMatReference"]?["<Object>k__BackingField"];
            if (playMat == null)
            {
                return result;
            }

            var options = playMat["m_treasureSatchelOptions"];
            var size = options?["_size"] ?? 0;
            if (size == 0)
            {
                return result;
            }

            var items = options["_items"];
            for (var i = 0; i < size; i++)
            {
                var option = items[i];
                var dataModel = option["m_dataModel"];
                result.Add(new DuelsHeroPowerOption()
                {
                    DatabaseId = option["m_databaseId"],
                    Enabled = option["m_isEnabled"],
                    Visible = option["m_isVisible"],
                    Completed = dataModel["m_Completed"],
                    Locked = dataModel["m_Locked"],
                    Selected = dataModel["m_IsSelectedOption"],
                });
            }

            return result;
        }

        public static int ReadDuelsCurrentOptionSelection(HearthstoneImage image)
        {
            var playMat = image["PvPDungeonRunScene"]?["m_instance"]?["m_dungeonCrawlDisplay"]?["m_dungeonCrawlPlayMatReference"]?["<Object>k__BackingField"];
            if (playMat == null)
            {
                return 0;
            }
            // If the user is on the "build deck" screen (ie has selected a signature treasure), forcefully return 0
            var currentOption = playMat["m_currentOptionType"];
            if (currentOption == 6)
            {
                var playMatState = playMat["m_playMatState"];
                if (playMatState == 5)
                {
                    return 0;
                }
                // Any way to do this cheaper?
                // While on deck building screen, I have
                // m_deckOptions size = 4
                // m_playButton enabled
                // m_playMatState 5
                // m_duelsPlayMat[m_livesWidgetLoaded] true



                //var sigTreasureOptions = ReadDuelsSignatureTreasureOptions(image);
                //var hasSignatureTreasureBeenSelected = sigTreasureOptions.Any(option => option.Selected);
                //if (hasSignatureTreasureBeenSelected)
                //{
                //    return 0;
                //}
            }


            return currentOption;
        }

        public static IDuelsPendingTreasureSelection ReadPendingTreasureSelection(HearthstoneImage image)
        {
            var dungeonInfo = BuildDungeonInfo(image);
            if (dungeonInfo.ChosenTreasure == -1)
            {
                return null;
            }

            var options = dungeonInfo?.TreasureOption;
            if (options == null)
            {
                return null;
            }

            return new DuelsPendingTreasureSelection()
            {
                Options = options,
            };
        }

        public static InternalDuelsDeck ReadDuelsDeck(HearthstoneImage image)
        {
            if (image["PvPDungeonRunScene"] == null
                || image["PvPDungeonRunScene"]["m_instance"] == null
                || image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"] == null
                || image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"] == null)
            {
                return null;
            }

            var memDeck = image["PvPDungeonRunScene"]["m_instance"]["m_dungeonCrawlDisplay"]["m_dungeonCrawlDeck"];

            var decklist = new List<string>();
            var slots = memDeck["m_slots"];
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

            var sideboards = ActiveDeckReader.BuildSideboards(memDeck);
            return new InternalDuelsDeck()
            {
                HeroCardId = memDeck["<HeroCardID>k__BackingField"],
                HeroPowerCardId = memDeck["HeroPowerCardID"],
                Decklist = decklist,
                Sideboards = sideboards,
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

    public class InternalDuelsDeck
    {
        public string HeroCardId { get; set; }

        public string HeroPowerCardId { get; set; }

        public IReadOnlyList<string> Decklist { get; set; }

        public List<DeckSideboard> Sideboards { get; set; }
    }
}