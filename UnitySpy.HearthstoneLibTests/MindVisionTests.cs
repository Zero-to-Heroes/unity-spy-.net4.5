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

    [TestClass()]
    public class MindVisionTests
    {
        [TestMethod]
        public void ListAllTypes()
        {
            var process = Process.GetProcessesByName("Hearthstone").FirstOrDefault();
            var image = AssemblyImageFactory.Create(process.Id);

            Crawler crawler = new Crawler(image);
            crawler.DumpMemory();
        }

        [TestMethod]
        public void TestRetrieveCollection()
        {
            var collection = new MindVision().GetCollectionCards();
            Assert.IsNotNull(collection);
            Assert.IsTrue(collection.Count > 0, "Collection should not be empty.");
            //this.TestContext.WriteLine($"Collection has {collection.Count} cards.");
        }

        // You need to have a game running for this
        [TestMethod]
        public void TestRetrieveMatchInfo()
        {
            var matchInfo = new MindVision().GetMatchInfo();
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
        public void TestGetActiveDeck()
        {
            var deck = new MindVision().GetActiveDeck();
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
    }
}