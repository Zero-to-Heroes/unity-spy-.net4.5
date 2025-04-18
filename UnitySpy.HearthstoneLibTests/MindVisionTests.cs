﻿namespace HackF5.UnitySpy.HearthstoneLib.Tests
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
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;

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
        public void TestRetrieveCurrentSceneMode()
        {
            var sceneMode = new MindVision().GetSceneMode();
            Assert.IsNotNull(sceneMode);
        }

        [TestMethod]
        public void TestGetActiveDeck()
        {
            var deck = new MindVision().GetActiveDeck(null);
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
        public void TestGetBattlegroundsTeammateBoard()
        {
            var info = new MindVision().GetBgsPlayerTeammateBoard();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetBattlegroundsPlayerBoard()
        {
            var info = new MindVision().GetBgsPlayerBoard();
            Assert.IsNotNull(info);
        } 

        [TestMethod]
        public void TestGetBattlegroundsSelectedGameMode()
        {
            var info = new MindVision().GetBattlegroundsSelectedGameMode();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetArenaInfo()
        {
            var info = new MindVision().GetArenaInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetArenaDraftStep()
        {
            var info = new MindVision().GetArenaDraftStep();
            Assert.IsNotNull(info);
        }
        [TestMethod]
        public void TestGetArenaDraftHeroOptions()
        {
            var info = new MindVision().GetArenaHeroOptions();
            Assert.IsNotNull(info);
        }
        [TestMethod]
        public void TestGetArenaDraftCardOptions()
        {
            var info = new MindVision().GetArenaCardOptions();
            Assert.IsNotNull(info);
        }
        [TestMethod]
        public void TestGetNumberOfCardsInArenaDraftDeck()
        {
            var mindvision = new MindVision();
            var info = mindvision.GetNumberOfCardsInArenaDraftDeck();
            Assert.IsNotNull(info);
        }
        [TestMethod]
        public void TestGetArenaDeck()
        {
            var info = new MindVision().GetArenaDeck();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetOpenPacksInfo()
        {
            var openPacksInfo = new MindVision().GetOpenPacksInfo();
            Assert.IsNotNull(openPacksInfo);
        }

        [TestMethod]
        public void TestReadMassPackOpening()
        {
            var openPacksInfo = new MindVision().GetMassOpenedPack();
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
        public void TestGetAdventuresInfo()
        {
            var info = new MindVision().GetAdventuresInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetAchievementsDbf()
        {
            var info = new MindVision().GetAchievementsDbf();
            var test = info.Where(ach => ach.AchievementId == 4980).FirstOrDefault();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetAchievements()
        {
            var info = new MindVision().GetAchievementsInfo();
            var test = info.Achievements.Where(ach => ach.AchievementId == 5666).FirstOrDefault();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetNumberOfCompletedAchievements()
        {
            var info = new MindVision().GetNumberOfCompletedAchievements();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetAchievementCategoriess()
        {
            var info = new MindVision().GetAchievementCategories();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetInGameAchievementProgress()
        {
            var mindVision = new MindVision();
            var totalIterations = 100;
            var startTime = DateTime.Now;
            IAchievementsInfo info = null;
            for (var i = 0; i < totalIterations; i++)
            {
                info = mindVision.GetInGameAchievementsProgressInfo(new int[] { 2924, 488, 498 });
            }
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            var durationByIteration = duration.TotalMilliseconds / totalIterations;
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestGetInGameAchievementProgressByIndex()
        {
            var mindVision = new MindVision();
            var totalIterations = 100;
            var startTime = DateTime.Now;
            IAchievementsInfo info = null;
            for (var i = 0; i < totalIterations; i++)
            {
                info = mindVision.GetInGameAchievementsProgressInfoByIndex(new int[] { 103, 146, 420 });
            }
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            var durationByIteration = duration.TotalMilliseconds / totalIterations;
            Assert.IsNotNull(duration);
        }

        [TestMethod]
        public void TestIsDisplayingAchievementToast()
        {
            var info = new MindVision().IsDisplayingAchievementToast();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestBoostersInfo()
        {
            var info = new MindVision().GetBoostersInfo();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetOpenedPacks()
        {
            var info = new MindVision().GetOpenedPacks();
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
            var info = new MindVision().GetWhizbangDeck(625);
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetGameUniqueId()
        {
            var info = new MindVision().GetGameUniqueId();
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
            var info = mindVision.GetMemoryChanges();
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestListenForChanges()
        {
            int count = 0;
            Console.WriteLine("Starting");
            //new MindVision().MemoryNotifier.OnTimedEvent(new MindVision(), (result) => { });
            long totalElapsed = 0;
            int loops = 0;
            new MindVision().ListenForChanges(50, (result) => { 
                totalElapsed += (result as MemoryUpdate).TotalTimeElapsed;
                loops++;
            });
            while (true)
            {
                count++;
                Thread.Sleep(2000);
            }
            Console.WriteLine($"Average time per loop: {totalElapsed / loops} ticks, after {loops} loops");
        }

        [TestMethod]
        public void TestMemoryResetIssues()
        {
            var mindVision = new MindVision();
            while (true)
            {
                //var info2 = mindVision.GetSceneMode();
                //var info = mindVision.GetDuelsInfo();
                var info3 = mindVision.GetMercenariesCollectionInfo();
                var info4 = mindVision.GetCollectionCards();
                Thread.Sleep(20);
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
        public void TestGetPlayerProfileInfo()
        {
            var info = new MindVision().GetPlayerProfileInfo();
            var arenaRecords = info.PlayerRecords.Where(i => i.RecordType == 5).ToList();
            var rankedRecords = info.PlayerRecords.Where(i => i.RecordType == 7).ToList();
            var casualRecords = info.PlayerRecords.Where(i => i.RecordType == 8).ToList();
            // Record.Data seems to be the heroCardId, at least for Arena wins
            var wins = info.PlayerRecords.Where(i => (i.RecordType == 28 || i.RecordType == 29) && i.Data != 0).Sum(i => i.Wins);
            var losses = info.PlayerRecords.Where(i => (i.RecordType == 28 || i.RecordType == 29) && i.Data != 0).Sum(i => i.Losses);
            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestGetCurrentRegion()
        {
            var info = new MindVision().GetCurrentRegion();
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