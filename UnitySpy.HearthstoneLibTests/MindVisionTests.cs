namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using HackF5.UnitySpy.HearthstoneLib;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using HackF5.UnitySpy.Crawler;
    using System.Threading;
    using Newtonsoft.Json;

    [TestClass()]
    public class MindVisionTests
    {
        [TestMethod]
        public void SanityTests()
        {
            TestRetrieveCollection();
            TestRetrieveCardBacks();
            TestRetrieveCoins();
        }

        [TestMethod]
        public void TestRetrieveCollection()
        {
            var collection = new MindVision().GetCollectionCards();
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 0, "Collection should not be empty.");
            var duelsUnlockedHp = collection.Where(card => card.CardId == "RLK_572").FirstOrDefault();
            Assert.IsNotNull(duelsUnlockedHp);
            //this.TestContext.WriteLine($"Collection has {collection.Count} cards.");
        }

        [TestMethod]
        public void TestRetrieveBattlegroundsHeroSkins()
        {
            var collection = new MindVision().GetCollectionBattlegroundsHeroSkins();
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 0, "Collection should not be empty.");
            //this.TestContext.WriteLine($"Collection has {collection.Count} cards.");
        }

        [TestMethod]
        public void TestRetrieveCurrentFullDustCards()
        {
            var collection = new MindVision().GetDustInfoCards();
            Assert.IsNotNull(collection);
            // The override event does not say that the event is currently active, but simply that
            // it has been overridden at some point
            var withOverride = collection.Where(info => info.OverrideEvent >= 0).ToList();
            Assert.IsNotNull(withOverride);
            //this.TestContext.WriteLine($"Collection has {collection.Count} cards.");
        }

        [TestMethod]
        public void TestRetrieveCardBacks()
        {
            var cardBacks = new MindVision().GetCollectionCardBacks();
            Assert.IsNotNull(cardBacks);
            Assert.IsTrue(cardBacks.Count > 0, "Card backs should not be empty.");
            var empty = cardBacks.Where(c => c.CardBackId == 0).ToList();
            Assert.IsTrue(empty.Count == 1, "There should be only one card back (Classic) with id == 0");
        }

        [TestMethod]
        public void TestRetrieveCoins()
        {
            var coins = new MindVision().GetCollectionCoins();
            Assert.IsNotNull(coins);
            Assert.IsTrue(coins.Count > 0, "Coins should not be empty.");
        }

        //[TestMethod]
        //public void TestRetrieveCardRecords()
        //{
        //    var coins = new MindVision().GetCollectionCardRecords();
        //    Assert.IsNotNull(coins);
        //}


        // You need to have a game running for this
        [TestMethod]
        public void TestRetrieveMatchInfo()
        {
            var matchInfo = new MindVision().GetMatchInfo();
             Assert.IsNotNull(matchInfo);
            //this.TestContext.WriteLine($"Local player's standard rank is {matchInfo.LocalPlayer.StandardRank}.");
        }

        [TestMethod]
        public void TestRetrieveBoardInfo()
        {
            var matchInfo = new MindVision().GetBoard();
             Assert.IsNotNull(matchInfo);
            //this.TestContext.WriteLine($"Local player's standard rank is {matchInfo.LocalPlayer.StandardRank}.");
        }

        // You need to have a solo run (Dungeon Run, Monster Hunt, Rumble Run, Dalaran Heist, Tombs of Terror) ongoing
        [TestMethod]
        public void TestRetrieveFullDungeonInfo()
        {
            var dungeonInfo = new MindVision().GetDungeonInfoCollection();
            Assert.IsNotNull(dungeonInfo);
        }

        [TestMethod]
        public void TestRetrieveDuelsInfo()
        {
            var duelsInfo = new MindVision().GetDuelsInfo();
            Assert.IsNotNull(duelsInfo);
        }

        [TestMethod]
        public void TestRetrieveDuelsPendingTreasure()
        {
            var duelsInfo = new MindVision().GetDuelsPendingTreasureSelection();
            Assert.IsNotNull(duelsInfo);
        }

        [TestMethod]
        public void TestRetrieveDuelsCurrentOption()
        {
            var duelsInfo = new MindVision().GetDuelsCurrentOptionSelection();
            Assert.IsNotNull(duelsInfo);
        }

        [TestMethod]
        public void TestRetrieveCurrentSceneMode()
        {
            var sceneMode = new MindVision().GetSceneMode();
            Assert.IsNotNull(sceneMode);
        }

        [TestMethod]
        public void TestGetActiveDeck()
        {
            var deck = new MindVision().GetActiveDeck(2266634173);
            Assert.IsNotNull(deck);
        }

        [TestMethod]
        public void TestGetSelectedDeckId()
        {
            var deck = new MindVision().GetSelectedDeckId();
            Assert.IsNotNull(deck);
        }

        [TestMethod]
        public void TestGetBattlegroundsInfo()
        {
            var info = new MindVision().GetBattlegroundsInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetArenaInfo()
        {
            var info = new MindVision().GetArenaInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetOpenPacksInfo()
        {
            var openPacksInfo = new MindVision().GetOpenPacksInfo();
            Assert.IsNotNull(openPacksInfo);
        }

        [TestMethod]
        public void TestGetAccountId()
        {
            var info = new MindVision().GetAccountInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetRewardTrackInfo()
        {
            var info = new MindVision().GetRewardTrackInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetDuelsRewards()
        {
            var info = new MindVision().GetDuelsRewardsInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetDuelsIsOnMainScreen()
        {
            var info = new MindVision().GetDuelsIsOnMainScreen();
            Assert.IsNotNull(info);
        }
        
        [TestMethod]
        public void TestGetDuelsIsOnDeckBuildingLobbyScreen()
        {
            var info = new MindVision().GetDuelsIsOnDeckBuildingLobbyScreen();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetDuelsIsChoosingHero()
        {
            var info = new MindVision().GetDuelsIsChoosingHero();
            Assert.IsNotNull(info);
        }
        
        [TestMethod]
        public void TestGetDuelsNumberOfCardsInDeck()
        {
            var info = new MindVision().GetNumberOfCardsInDeck();
            Assert.IsNotNull(info);
        }
        
        [TestMethod]
        public void TestGetDuelsHeroOptions()
        {
            var info = new MindVision().GetDuelsHeroOptions();
            Assert.IsNotNull(info);
        }
        
        [TestMethod]
        public void TestGetDuelsHeroPowerOptions()
        {
            var info = new MindVision().GetDuelsHeroPowerOptions();
            Assert.IsNotNull(info);
        }
        
        [TestMethod]
        public void TestGetDuelsSignatureTreasureOptions()
        {
            var info = new MindVision().GetDuelsSignatureTreasureOptions();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetAdventuresInfo()
        {
            var info = new MindVision().GetAdventuresInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetAchievements()
        {
            var info = new MindVision().GetAchievementsInfo();
            var test = info.Achievements.Where(ach => ach.AchievementId == 1695).FirstOrDefault();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetInGameAchievementProgress()
        {
            var info = new MindVision().GetInGameAchievementsProgressInfo();
            var test = info.Achievements.Where(ach => ach.AchievementId == 1695).FirstOrDefault();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestIsDisplayingAchievementToast()
        {
            var info = new MindVision().IsDisplayingAchievementToast();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestDuelsRewardsPending()
        {
            var info = new MindVision().IsDuelsRewardsPending();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestBoostersInfo()
        {
            var info = new MindVision().GetBoostersInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetOpenedPack()
        {
            var info = new MindVision().GetOpenedPack();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetCollectionSize()
        {
            var info = new MindVision().GetCollectionSize();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetWhizbangDeck()
        {
            var info = new MindVision().GetWhizbangDeck(4713);
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestXpChanges()
        {
            var info = new MindVision().GetXpChanges();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMercenariesInfo()
        {
            var info = new MindVision().GetMercenariesInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMercenariesVisitors()
        {
            var info = new MindVision().GetMercenariesVisitors();
            var test = info.Where(i => i.ProceduralMercenaryId != 0 || i.ProceduralBountyId != 0).ToList();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMercenariesPendingTreasureSelection()
        {
            var info = new MindVision().GetMercenariesPendingTreasureSelection(0);
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMercenariesIsSelectingTreasure()
        {
            var info = new MindVision().GetMercenariesIsSelectingTreasures();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMercenariesCollection()
        {
            var info = new MindVision().GetMercenariesCollectionInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetMemoryChanges()
        {
            var mindVision = new MindVision();
            mindVision.GetMemoryChanges();
            var info = mindVision.GetMemoryChanges();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestListenForChanges()
        {
            int count = 0;
            Console.WriteLine("Starting");
            //new MindVision().MemoryNotifier.OnTimedEvent(new MindVision(), (result) => { });
            new MindVision().ListenForChanges(200, (result) => { });
            while (true)
            {
                count++;
                Thread.Sleep(2000);
            }
        }

        [TestMethod]
        public void TestGetTurnTimer()
        {
            var info = new MindVision().GetTurnTimer();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetQuests()
        {
            var info = new MindVision().GetQuests();
            var progresses = string.Join(", ", info.Quests.Select(q => q.Progress).ToList());
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestIsFriendsListOpen()
        {
            var info = new MindVision().IsFriendsListOpen();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void ListServices()
        {
            new MindVision().ListServices();
        }

        [TestMethod]
        public void ListNetCacheServices()
        {
            new MindVision().ListNetCacheServices();
        }


        [TestMethod]
        public void ListObjects()
        {
            new MindVision().ListObjects("friend");
        }

        // ##################################

        [TestMethod]
        public void GenerateTemplateDecks()
        {
            var info = new MindVision().GetTemplateDecks();
            Assert.IsNotNull(info);
            var jsonInfo = JsonConvert.SerializeObject(info);
            Assert.IsNotNull(jsonInfo);
        }

        [TestMethod]
        public void GenerateSecondaryRaceTags()
        {
            var info = new MindVision().GetRaceTags();
            Assert.IsNotNull(info);
            var jsonInfo = JsonConvert.SerializeObject(info);
            Assert.IsNotNull(jsonInfo);
        }

        [TestMethod]
        public void GenerateEventTimings()
        {
            var info = new MindVision().GetEventTimings();
            Assert.IsNotNull(info);
            var jsonInfo = JsonConvert.SerializeObject(info);
            Assert.IsNotNull(jsonInfo);
        }
    }
}