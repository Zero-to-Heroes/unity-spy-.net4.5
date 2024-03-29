﻿namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.GameDbf;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using JetBrains.Annotations;
    using static System.Net.Mime.MediaTypeNames;

    internal static class DuelsInfoReader
    {
        // TODO: DuelsDeck, HeroPowerCardDbfId, PlayerClass all are from pervious deck
        // The DungeonInfo isn't updated at this point, apparently
        // Could it be possible to get the info from somewhere else?
        public static DuelsInfo ReadDuelsInfo([NotNull] HearthstoneImage image)
        {
            var adventureConfig = image["AdventureConfig"]?["s_instance"];
            var selectedAdventure = adventureConfig?["m_SelectedAdventure"]; // service?["m_data"]?["m_selectedAdventure"];
            var selectedMode = adventureConfig?["m_SelectedMode"]; // service?["m_data"]?["m_selectedMode"];

            AdventureData adventureData = GameDbfReader.FindAdventureData(image, selectedAdventure ?? 910, selectedMode ?? 5);
            var dungeonKey = adventureData?.GameSaveDataServerKeyId;
            if (dungeonKey == null || dungeonKey == 0) 
            {
                dungeonKey = adventureData?.GameSaveDataClientKeyId;
            }
            if (dungeonKey == null || dungeonKey == 0)
            {
                dungeonKey = (int)DungeonKey.Duels;
            }

            var savesMap = image["GameSaveDataManager"]?["s_instance"]?["m_gameSaveDataMapByKey"];
            DungeonInfo dungeonInfo = DungeonInfoReader.BuildDungeonInfo(image, (DungeonKey)dungeonKey, savesMap, true);
            if (dungeonInfo == null)
            {
                return null;
            }

            var duelsDeckFromCollection = ReadDuelsDeckFromCollection(image, dungeonInfo);

            var duelsNetCacheService = image.GetNetCacheService("NetCachePVPDRStatsInfo");
            var displayDataModel = image["PvPDungeonRunDisplay"]?["m_instance"]?["m_dataModel"];

            var result = new DuelsInfo
            {
                IsPaidEntry = displayDataModel?["m_IsPaidEntry"],
                HeroCardDbfId = dungeonInfo.HeroCardId, //duelsMetaInfo.HeroCardDbfId ?? heroPowerCardDbfId,
                HeroPowerCardDbfId = dungeonInfo.StartingHeroPower, //duelsMetaInfo.HeroPowerDbfId ?? heroPowerCardDbfId,
                SignatureTreasureCardDbfId = dungeonInfo.StartingTreasure, //duelsMetaInfo.LoadoutTreasureDbfId ?? signatureTreasureCardDbfId,
                PlayerClass = dungeonInfo.PlayerClass,
                Wins = displayDataModel?["m_Wins"], //duelsMetaInfo?.Wins ?? -1,
                Losses = displayDataModel?["m_Losses"], // duelsMetaInfo?.Losses ?? -1,
                Rating = duelsNetCacheService?["<Rating>k__BackingField"], // duelsMetaInfo?.Rating ?? -1,
                PaidRating = duelsNetCacheService?["<PaidRating>k__BackingField"], // duelsMetaInfo?.PaidRating ?? -1,
                LastRatingChange = displayDataModel?["m_LastRatingChange"], // duelsMetaInfo?.LastRatingChange ?? -1,
                RunActive = dungeonInfo.RunActive == 1,
                //SessionActive = duelsMetaInfo?.SessionActive,

                LootOptionBundles = dungeonInfo.RunActive == 1 ? dungeonInfo.LootOptionBundles : new List<DungeonOptionBundle>(),
                ChosenLoot = dungeonInfo.RunActive == 1 ? dungeonInfo.ChosenLoot : 0,
                TreasureOption = dungeonInfo.RunActive == 1 ? dungeonInfo.TreasureOption : new List<int>(),
                ChosenTreasure = dungeonInfo.RunActive == 1 ? dungeonInfo.ChosenTreasure : 0,

                //DeckList = dungeonInfo?.DeckList,
                //Sideboards = duelsDeck?.Sideboards,
                //StartingHeroPower = dungeonInfo?.StartingHeroPower ?? -1,
                //StartingHeroPowerCardId = duelsDeck?.HeroPowerCardId,
            };


            // Can happen when we selected the hero, but nothing else
            if (result.HeroCardDbfId == 0)
            {
                try
                {
                    var dungeonRunScene = image["PvPDungeonRunScene"]["m_instance"];
                    var service = dungeonRunScene["m_services"]?["<DungeonCrawlData>k__BackingField"];
                    var heroCardDbfId = service?["<SelectedHeroCardDbId>k__BackingField"];
                    result.HeroCardDbfId = heroCardDbfId;
                } 
                catch (Exception ex)
                {
                    Logger.Log("Could not get HeroCardDbfId from PvPDungeonRunScene.m_services");
                    Logger.Log(ex.Message);
                    Logger.Log(ex.StackTrace);
                }
            }
            if (result.HeroPowerCardDbfId == 0)
            {
                var heroPowerDbfIf = adventureConfig?["<SelectedHeroPowerDbId>k__BackingField"];
                result.HeroPowerCardDbfId = heroPowerDbfIf;
            }
            if (duelsDeckFromCollection != null)
            {
                var accountInfo = AccountInfoReader.ReadAccountInfo(image);
                // Don't include signature treasure / hero power, as the data is sometimes not present, thus messing up the run ID
                result.DeckId = $"{accountInfo.Hi}-{accountInfo.Lo}-{duelsDeckFromCollection.DeckId}";
            }

            //TODO: doesn't give the partial decklist (it probably only pesists once the deck is full)
            var decklist = dungeonInfo.DeckList
                    .Select(dbfId => CollectionCardReader.TranslateDbfIdToCardId(image, dbfId))
                    .ToList();
            if (dungeonInfo.StartingTreasure > 0 && !dungeonInfo.DeckList.Contains(dungeonInfo.StartingTreasure))
            {
                var treasureCardId = CollectionCardReader.TranslateDbfIdToCardId(image, dungeonInfo.StartingTreasure);
                decklist.Add(treasureCardId);
            }
            result.DuelsDeck = new Deck()
            {
                DeckList = decklist,
                HeroCardId = CollectionCardReader.TranslateDbfIdToCardId(image, (int)result.HeroCardDbfId),
                HeroPowerCardId = CollectionCardReader.TranslateDbfIdToCardId(image, (int)result.HeroPowerCardDbfId),
                HeroClass = result.PlayerClass ?? 0,
                Sideboards = duelsDeckFromCollection?.Sideboards,
            };
            // Happens when building the list
            if (result.DuelsDeck.DeckList.Count <= 1 && duelsDeckFromCollection != null)
            {
                result.DuelsDeck.DeckList = duelsDeckFromCollection.DeckList;
            }
            return result;
        }

        private static void AugmentDuelsDungeonInfo(HearthstoneImage image, DungeonInfo dungeonInfo)
        {
            if (dungeonInfo != null && dungeonInfo.StartingTreasure == 0)
            {
                var duelsLoadoutTreasures = GetDuelsLoadoutTreasures(image);
                var loadoutDbfIds = duelsLoadoutTreasures.Select(x => x.CardId).ToList();
                foreach (var dbfId in dungeonInfo.DeckList)
                {
                    if (loadoutDbfIds.Contains(dbfId))
                    {
                        dungeonInfo.StartingTreasure = dbfId;
                    }
                }
            }
        }

        private static List<dynamic> loadoutTreasures = new List<dynamic>();
        public static List<dynamic> GetDuelsLoadoutTreasures(HearthstoneImage image)
        {
            if (loadoutTreasures.Count > 0)
            {
                return loadoutTreasures;
            }

            var records = image["GameDbf"]["AdventureLoadoutTreasures"]["m_records"];
            var size = records["_size"];
            var items = records["_items"];
            for (var i = 0; i < size; i++)
            {
                var treasure = items[i];
                dynamic loadout = new
                {
                    CardId = treasure["m_cardId"],
                };
                // Should filter by adventure / mode here
                loadoutTreasures.Add(loadout);
            }
            return loadoutTreasures;
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

        public static IReadOnlyList<DuelsHeroPowerOption> ReadDuelsHeroOptions(HearthstoneImage image)
        {
            var result = new List<DuelsHeroPowerOption>();
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
                result.Add(new DuelsHeroPowerOption()
                {
                    DatabaseId = hero["m_cardId"],
                    Selected = heroButton["m_isSelected"],
                    Locked = heroButton["m_locked"],
                });
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

        // Cherry-pick the elements we're interested in, so that we don't build the full dungeonInfo model 
        // each time
        public static IDuelsPendingTreasureSelection ReadPendingTreasureSelection(HearthstoneImage image)
        {
            var savesMap = image["GameSaveDataManager"]?["s_instance"]?["m_gameSaveDataMapByKey"];
            var index = DungeonInfoReader.GetKeyIndex(savesMap, (int)DungeonKey.Duels);
            var dungeonMap = savesMap["valueSlots"][index];
            var chosenTreasure = DungeonInfoReader.ExtractValue(dungeonMap, (int)DungeonFieldKey.ChosenTreasure);
            if (chosenTreasure == -1)
            {
                return null;
            }

            var treasureOptions = DungeonInfoReader.ExtractValues(dungeonMap, (int)DungeonFieldKey.TreasureOption);
            if (treasureOptions == null)
            {
                return null;
            }

            return new DuelsPendingTreasureSelection()
            {
                Options = treasureOptions,
            };
        }

        private static Deck ReadDuelsDeck(HearthstoneImage image, DungeonInfo dungeonInfo)
        {
            if (IsPVPDRSessionComplete(image))
            {
                return null;
            }




            //if (dungeonInfo == null)
            //{
            //    dungeonInfo = BuildDungeonInfo(image);
            //    AugmentDuelsDungeonInfo(image, dungeonInfo);
            //}

            // Issue: this rolls over between runs, so if we restart a run after ending the previous one,
            // we will still get the deck
            // However, we can simply decide to hide that temp deck while we're on the treasure select screen
            var memDeck = image["PvPDungeonRunScene"]?["m_instance"]?["m_dungeonCrawlDisplay"]?["m_dungeonCrawlDeck"];

            var decklist = new List<string>();
            var sideboards = new List<DeckSideboard>();
            string heroCardId = null;
            string heroPowerCardId = null;
            if (memDeck != null)
            {
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
                sideboards = ActiveDeckReader.BuildSideboards(memDeck);
                heroCardId = memDeck["<HeroCardID>k__BackingField"];
                heroPowerCardId = memDeck["HeroPowerCardID"];
            } 
            //else if (dungeonInfo != null)
            //{
            //    decklist = dungeonInfo.DeckList.Select(dbfId => CollectionCardReader.TranslateDbfIdToCardId(image, dbfId)).ToList();
            //    heroCardId = CollectionCardReader.TranslateDbfIdToCardId(image, dungeonInfo.HeroCardId);
            //    heroPowerCardId = CollectionCardReader.TranslateDbfIdToCardId(image, dungeonInfo.StartingHeroPower);
            //}

            var startingDeck = ReadDuelsDeckFromCollection(image, dungeonInfo);
            if (startingDeck != null)
            {
                if (decklist.Count == 0)
                {
                    decklist = startingDeck.DeckList.ToList();
                }
                if (sideboards.Count == 0)
                {
                    sideboards = startingDeck.Sideboards;
                }
                if (heroCardId == null)
                {
                    heroCardId = startingDeck.HeroCardId;
                }
                if (heroPowerCardId == null)
                {
                    heroPowerCardId = startingDeck.HeroPowerCardId;
                }
            }

            return new Deck()
            {
                HeroCardId = heroCardId,
                HeroPowerCardId = heroPowerCardId,
                DeckList = decklist,
                Sideboards = sideboards,
            };
        }

        // DungeonCrawlUtils.IsPVPDRSessionComplete
        private static bool IsPVPDRSessionComplete(HearthstoneImage image)
        {
            var dataModel = image["PvPDungeonRunDisplay"]?["m_instance"]?["m_dataModel"];
            if (dataModel == null)
            {
                return true;
            }

            var hasSession = dataModel["m_HasSession"];
            if (!hasSession)
            {
                return true;
            }

            var wins = dataModel["m_Wins"];
            var losses = dataModel["m_Losses"];
            var isSessionActive = dataModel["m_IsSessionActive"];
            if ((wins <= 0 && losses <= 0) || isSessionActive)
            {
                var isSessionRolledOver = dataModel["m_IsSessionRolledOver"];
                return isSessionRolledOver;
            }

            return true;
        }

        public static Deck ReadDuelsDeckFromCollection(HearthstoneImage image, DungeonInfo dungeonInfo = null, bool debug = false)
        {
            var currentDuelsDeckId = image["CollectionManager"]?["s_instance"]?["m_currentPVPDRDeckId"];
            var decksInstance = image["CollectionManager"]?["s_instance"]?["m_decks"];
            var deckCount = decksInstance?["count"] ?? 0;
            if (debug)
            {
                Logger.Log($"Will consider deckCount={deckCount} decks");
            }
            Deck duelsDeck = null;
            for (var i = 0; i < deckCount; i++)
            {
                var deck = decksInstance["valueSlots"][i];
                if (deck?["ID"] != currentDuelsDeckId)
                {
                    continue;
                }
                //if (debug)
                //{
                //    Logger.Log($"Considering deck i={i}, isNull={deck == null}, deckType={deck?["<Type>k__BackingField"]}");
                //}
                //if (deck?["<Type>k__BackingField"] != (int)DeckType.PVPDR_DECK)
                //{
                //    continue;
                //}
                duelsDeck = ActiveDeckReader.GetDynamicDeck(deck, debug);
            }

            if (duelsDeck == null)
            {
                if (debug)
                {
                    Logger.Log($"No duels decks from memory");
                }
                return null;
            }

            if (dungeonInfo == null)
            {
                dungeonInfo = BuildDungeonInfo(image);
                AugmentDuelsDungeonInfo(image, dungeonInfo);
            }

            if (dungeonInfo?.StartingTreasure != null
                && dungeonInfo.StartingTreasure != 0
                && !duelsDeck.DeckList.Contains("" + dungeonInfo?.StartingTreasure))
            {
                duelsDeck.DeckList.Add("" + dungeonInfo.StartingTreasure);
            }

            return duelsDeck;
        }

        private static DungeonInfo BuildDungeonInfo([NotNull] HearthstoneImage image)
        {
            var savesMap = image["GameSaveDataManager"]?["s_instance"]?["m_gameSaveDataMapByKey"];
            if (savesMap != null)
            {
                AdventureData adventureData = null;
                try
                {
                    var dungeonRunScene = image["PvPDungeonRunScene"]?["m_instance"];
                    var service = dungeonRunScene?["m_services"]?["<DungeonCrawlData>k__BackingField"];
                    var selectedAdventure = service?["m_data"]?["m_selectedAdventure"] ;
                    var selectedMode = service?["m_data"]?["m_selectedMode"] ;
                    adventureData = GameDbfReader.FindAdventureData(image, selectedAdventure ?? 910, selectedMode ?? 5);
                } 
                catch (Exception ex)
                {
                    Logger.Log("Failed to find adventure info for Duels");
                }

                var dungeonKey = adventureData?.GameSaveDataClientKeyId ?? (int)DungeonKey.Duels;
                return DungeonInfoReader.BuildDungeonInfo(image, (DungeonKey)dungeonKey, savesMap);
            }
            return null;
        }


        //private static DuelsMetaInfo BuildDuelsMetaInfo([NotNull] HearthstoneImage image)
        //{
        //    var adventureConfig = image["AdventureConfig"]?["s_instance"];
        //    var selectedAdventure = adventureConfig?["m_SelectedAdventure"]; // service?["m_data"]?["m_selectedAdventure"];
        //    var selectedMode = adventureConfig?["m_SelectedMode"]; // service?["m_data"]?["m_selectedMode"];
        //    //var heroCardDbfId = service?["<SelectedHeroCardDbId>k__BackingField"];
        //    var heroPowerDbfIf = adventureConfig?["<SelectedHeroPowerDbId>k__BackingField"]; // service?["<SelectedHeroPowerDbId>k__BackingField"];
        //    var loadoutTreasureDbfId = adventureConfig?["<SelectedLoadoutTreasureDbId>k__BackingField"]; // service?["<SelectedLoadoutTreasureDbId>k__BackingField"];

        //    var adventureData = GameDbfReader.FindAdventureData(image, selectedAdventure ?? 910, selectedMode ?? 5);
        //    var dungeonKey = adventureData?.GameSaveDataClientKeyId ?? (int)DungeonKey.Duels;
        //    var dungeonInfo = DungeonInfoReader.BuildDungeonInfo(image, (DungeonKey)dungeonKey, savesMap);


        //    var dungeonRunScene = image["PvPDungeonRunScene"]["m_instance"];
        //    var wins = -1;
        //    var losses = -1;
        //    var rating = -1;
        //    var paidRating = -1;
        //    var lastRatingChange = -1;
        //    bool? isPaidEntry = null;
        //    bool? sessionActive = null;
        //    if (dungeonRunScene["m_display"] != null && dungeonRunScene["m_display"]["m_dataModel"] != null)
        //    {
        //        var display = dungeonRunScene["m_display"]["m_dataModel"];
        //        wins = display["m_Wins"];
        //        losses = display["m_Losses"];
        //        rating = display["m_Rating"];
        //        paidRating = display["m_PaidRating"];
        //        lastRatingChange = display["m_LastRatingChange"];
        //        isPaidEntry = display["m_IsPaidEntry"];
        //        sessionActive = display["m_IsSessionActive"];
        //    }
        //    // Works even in-game, while the dungeonRunScene is unavailable in-game
        //    var adventureConfig = image["AdventureConfig"]?["s_instance"];
        //    var service = dungeonRunScene["m_services"]?["<DungeonCrawlData>k__BackingField"];
        //    var selectedAdventure = adventureConfig?["m_SelectedAdventure"]; // service?["m_data"]?["m_selectedAdventure"];
        //    var selectedMode = adventureConfig?["m_SelectedMode"]; // service?["m_data"]?["m_selectedMode"];
        //    var heroCardDbfId = service?["<SelectedHeroCardDbId>k__BackingField"];
        //    var heroPowerDbfIf = adventureConfig?["<SelectedHeroPowerDbId>k__BackingField"]; // service?["<SelectedHeroPowerDbId>k__BackingField"];
        //    var loadoutTreasureDbfId = adventureConfig?["<SelectedLoadoutTreasureDbId>k__BackingField"]; // service?["<SelectedLoadoutTreasureDbId>k__BackingField"];

        //    return new DuelsMetaInfo
        //    {
        //        IsPaidEntry = isPaidEntry,
        //        Wins = wins,
        //        Losses = losses,
        //        Rating = rating,
        //        PaidRating = paidRating,
        //        SessionActive = sessionActive,
        //        LastRatingChange = lastRatingChange,
        //        //GameSaveClientKey = gameSaveClientKey,
        //        HeroCardDbfId = heroCardDbfId,
        //        HeroPowerDbfId = heroPowerDbfIf,
        //        LoadoutTreasureDbfId = loadoutTreasureDbfId,
        //        SelectedAdventure = selectedAdventure,
        //        SelectedMode = selectedMode,
        //    };
        //}

        //private static DuelsMetaInfo ReadDuelsInfoFromDisplay([NotNull] HearthstoneImage image)
        //{
        //    if (image["PvPDungeonRunDisplay"] == null
        //        || image["PvPDungeonRunDisplay"]["m_instance"] == null
        //        || image["PvPDungeonRunDisplay"]["m_instance"]["m_dataModel"] == null)
        //    {
        //        return null;
        //    }

        //    var display = image["PvPDungeonRunDisplay"]["m_instance"]["m_dataModel"];
        //    var wins = display["m_Wins"];
        //    var losses = display["m_Losses"];
        //    var rating = display["m_Rating"];
        //    var paidRating = display["m_PaidRating"];
        //    var lastRatingChange = display["m_LastRatingChange"];
        //    var isPaidEntry = display["m_IsPaidEntry"];

        //    var adventureConfig = image["AdventureConfig"]?["s_instance"];
        //    var selectedAdventure = adventureConfig?["m_SelectedAdventure"]; // service?["m_data"]?["m_selectedAdventure"];
        //    var selectedMode = adventureConfig?["m_SelectedMode"]; // service?["m_data"]?["m_selectedMode"];
        //    //var heroCardDbfId = service?["<SelectedHeroCardDbId>k__BackingField"];
        //    var heroPowerDbfIf = adventureConfig?["<SelectedHeroPowerDbId>k__BackingField"]; // service?["<SelectedHeroPowerDbId>k__BackingField"];
        //    var loadoutTreasureDbfId = adventureConfig?["<SelectedLoadoutTreasureDbId>k__BackingField"]; // service?["<SelectedLoadoutTreasureDbId>k__BackingField"];


        //    return new DuelsMetaInfo
        //    {
        //        IsPaidEntry = isPaidEntry,
        //        Wins = wins,
        //        Losses = losses,
        //        Rating = rating,
        //        PaidRating = paidRating,
        //        LastRatingChange = lastRatingChange,
        //        //HeroCardDbfId = heroCardDbfId,
        //        HeroPowerDbfId = heroPowerDbfIf,
        //        LoadoutTreasureDbfId = loadoutTreasureDbfId,
        //        SelectedAdventure = selectedAdventure,
        //        SelectedMode = selectedMode,
        //    };
        //}
    }

    internal class DuelsMetaInfo
    {
        public bool? IsPaidEntry { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Rating { get; set; }
        public int LastRatingChange { get; set; }
        public int PaidRating { get; set; }
        public bool? SessionActive { get; set; }
        public long? HeroCardDbfId { get; set; }
        public long? HeroPowerDbfId { get; set; }
        public long? LoadoutTreasureDbfId { get; set; }
        //public int? GameSaveClientKey { get; set; }
        public int? SelectedAdventure { get; set; }
        public int? SelectedMode { get; set; }
    }

    public class InternalDuelsDeck
    {
        public string HeroCardId { get; set; }

        public string HeroPowerCardId { get; set; }

        public IReadOnlyList<string> Decklist { get; set; }

        public List<DeckSideboard> Sideboards { get; set; }
    }
}