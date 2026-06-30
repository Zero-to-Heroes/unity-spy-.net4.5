namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using HackF5.UnitySpy;
    using HackF5.UnitySpy.Detail;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Non-regression tests for the 64-bit <c>Dictionary&lt;int, int&gt;</c> value-offset bug.
    ///
    /// Background: Mono reports field offsets against the OPEN generic definition, where generic-parameter
    /// fields occupy a full pointer-sized (8 byte) slot. In the real instantiated <c>Dictionary&lt;int,int&gt;+Entry</c>
    /// the fields pack to 16 bytes (hashCode@0, next@4, key@8, value@12). Before the fix UnitySpy read
    /// <c>value</c> at offset 16, which on a 16-byte stride is the NEXT entry's <c>hashCode</c>. Since mono stores
    /// <c>hashCode == key</c> for int keys, every <c>value[i]</c> came back as <c>key[i+1]</c>. This surfaced as
    /// Battlegrounds leaderboard hovers returning a neighbouring tag id (e.g. HEALTH=45, LEADERBOARD_PLACE=1373)
    /// instead of a PLAYER_ID in the 1-8 range. The fix lives in <c>UnitySpy/Detail/FieldDefinition.cs</c>
    /// (<c>TryGetInflatedValueTypeOffset</c>), which recomputes field offsets from the real instantiated layout.
    ///
    /// HOW TO RUN: these tests read a LIVE Hearthstone (x64) process, so the game must be running. The
    /// <see cref="DictionaryIntInt_ValueIsReadFromCorrectOffset"/> test needs to be inside any game/match (so a
    /// game entity tag map exists); it self-marks Inconclusive otherwise. The
    /// <see cref="BattlegroundsLeaderboardHover_ReturnsValidPlayerId"/> test additionally requires hovering a hero
    /// portrait in the Battlegrounds leaderboard while the test runs.
    ///
    /// Example (from the repo root, after building the test project):
    ///   "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" ^
    ///       UnitySpy.HearthstoneLibTests\bin\Debug\UnitySpy.HearthstoneLibTests.dll ^
    ///       /Tests:DictionaryIntInt_ValueIsReadFromCorrectOffset /Platform:x64
    /// </summary>
    [TestClass]
    public class DictionaryReadRegressionTests
    {
        // Real inflated Dictionary<int,int>+Entry layout (x64): hashCode@0, next@4, key@8, value@12 (16 byte stride).
        private const int KeyDataOffset = 8;
        private const int ValueDataOffset = 12;
        private const int BuggyValueDataOffset = 16;

        [TestMethod]
        [TestCategory("Regression")]
        public void DictionaryIntInt_ValueIsReadFromCorrectOffset()
        {
            var process = FindHearthstoneX64();
            if (process == null)
            {
                Assert.Inconclusive("No running 64-bit Hearthstone process found. Start the game to run this non-regression test.");
            }

            var image = AssemblyImageFactory.Create(process.Id, _ => { });
            var pf = ((AssemblyImage)image).Process;
            dynamic dimage = image;

            // A Dictionary<int,int>: GameState.s_instance.m_gameEntity.m_tags.m_values. Present in any game/match.
            dynamic values = dimage["GameState"]?["s_instance"]?["m_gameEntity"]?["m_tags"]?["m_values"];
            if (values == null)
            {
                Assert.Inconclusive("No game-entity tag map found. Enter any game/match so a Dictionary<int,int> exists, then re-run.");
            }

            int count = values["_count"];
            dynamic entries = values["_entries"];
            Assert.IsTrue(count > 0, "Expected the game entity to expose at least one tag.");

            var addrProp = typeof(MemoryObject).GetProperty("Address", BindingFlags.NonPublic | BindingFlags.Instance);

            var chainMatches = 0;
            var comparisons = 0;
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i];
                var entryAddr = (IntPtr)addrProp.GetValue(entry);

                int unitySpyKey = entry["key"];
                int unitySpyValue = entry["value"];

                // Independent ground-truth reads straight from the entry's bytes.
                var rawKey = (int)pf.ReadUInt32(entryAddr + KeyDataOffset);
                var rawValueCorrect = (int)pf.ReadUInt32(entryAddr + ValueDataOffset);
                var rawValueBuggy = (int)pf.ReadUInt32(entryAddr + BuggyValueDataOffset);

                Assert.AreEqual(rawKey, unitySpyKey, $"entry[{i}] key read from the wrong offset.");
                Assert.AreEqual(
                    rawValueCorrect,
                    unitySpyValue,
                    $"entry[{i}] value must be read from the packed offset {ValueDataOffset}, not the open-generic offset {BuggyValueDataOffset}. This is the regressed behavior.");

                comparisons++;
                if (unitySpyValue == rawValueBuggy && rawValueCorrect != rawValueBuggy)
                {
                    chainMatches++;
                }
            }

            // Extra safety net for the specific regression signature (value bleeding into the next entry's hashCode).
            Assert.AreEqual(
                0,
                chainMatches,
                $"{chainMatches}/{comparisons} entries read value from the buggy offset {BuggyValueDataOffset} (the old 'value[i]==key[i+1]' chain).");
        }

        [TestMethod]
        [TestCategory("Regression")]
        public void DictionaryIntReference_ValuesReadCorrectly()
        {
            // Guards the OTHER branch of the FieldDefinition offset fix: a Dictionary<int, QuestModel> stores its
            // value as an 8-byte pointer at the 8-aligned data offset 16 (not the int-packed offset 12). This is
            // the case commit 5517319 fixed; the offset recompute must not re-break it. If the value pointer were
            // read from the wrong offset, the QuestModel pointers would be garbage and GetQuests would either throw
            // or surface implausible ids/progress.
            if (FindHearthstoneX64() == null)
            {
                Assert.Inconclusive("No running 64-bit Hearthstone process found. Start the game to run this non-regression test.");
            }

            var quests = new MindVision().GetQuests();
            if (quests?.Quests == null || quests.Quests.Count == 0)
            {
                Assert.Inconclusive("No quests on this account right now, so the reference-valued dictionary cannot be verified.");
            }

            foreach (var quest in quests.Quests)
            {
                Assert.IsTrue(quest.Id > 0, $"Decoded an implausible quest id {quest.Id} - reference value likely read from the wrong offset.");
                Assert.IsTrue(quest.Progress >= 0, $"Decoded a negative quest progress {quest.Progress} - reference value likely read from the wrong offset.");
            }
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Regression")]
        public void BattlegroundsLeaderboardHover_ReturnsValidPlayerId()
        {
            var process = FindHearthstoneX64();
            if (process == null)
            {
                Assert.Inconclusive("No running 64-bit Hearthstone process found. Start the game to run this non-regression test.");
            }

            var image = AssemblyImageFactory.Create(process.Id, _ => { });
            dynamic dimage = image;

            dynamic tile = dimage["PlayerLeaderboardManager"]?["s_instance"]?["m_currentlyMousedOverTile"];
            if (tile == null)
            {
                Assert.Inconclusive("No leaderboard tile under the cursor. Hover a hero portrait in the Battlegrounds leaderboard while the test runs.");
            }

            dynamic entity = tile["m_playerHeroEntity"] ?? tile["m_entity"];
            Assert.IsNotNull(entity, "Moused-over leaderboard tile has no hero entity.");

            dynamic values = entity["m_tags"]?["m_values"];
            Assert.IsNotNull(values, "Hero entity has no tag map.");

            int count = values["_count"];
            dynamic entries = values["_entries"];

            int? playerId = null;
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i];
                if (entry["key"] == (int)GameTag.PLAYER_ID)
                {
                    playerId = entry["value"];
                    break;
                }
            }

            Assert.IsTrue(playerId.HasValue, "PLAYER_ID tag was not found on the moused-over hero entity.");
            Assert.IsTrue(
                playerId.Value >= 1 && playerId.Value <= 8,
                $"PLAYER_ID should be in the 1-8 range, but was {playerId.Value} (regressed behavior reads a neighbouring tag such as HEALTH=45 or LEADERBOARD_PLACE).");
        }

        private static Process FindHearthstoneX64()
        {
            var candidates = Process.GetProcessesByName("Hearthstone");
            var preferred = candidates.FirstOrDefault(p =>
            {
                try
                {
                    return (p.MainModule?.FileName ?? string.Empty)
                        .IndexOf("Hearthstone_Event_1", StringComparison.OrdinalIgnoreCase) >= 0;
                }
                catch
                {
                    return false;
                }
            });

            return preferred ?? candidates.FirstOrDefault();
        }
    }
}
