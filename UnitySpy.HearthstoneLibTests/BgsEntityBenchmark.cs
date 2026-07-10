namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// CPU micro-benchmark for the BgsEntity tag-index optimization. This does not need a live client:
    /// the cost being measured is purely local (repeated linear Tags.Find scans and the O(N^2) enchantment
    /// matching), so it is simulated with a realistic Battlegrounds entity set (a few hundred entities,
    /// ~40 tags each) and the same access pattern the board readers use: partition by zone, filter the
    /// board, attach enchantments, sort. The "old" variant reproduces the previous implementation
    /// (Tags.Find per tag query, full entity scan per minion for enchantments) against the same data.
    /// </summary>
    [TestClass]
    public class BgsEntityBenchmark
    {
        private const int EntityCount = 400;
        private const int Iterations = 500;

        [TestMethod]
        public void CompareTagAccessPatterns()
        {
            var rng = new Random(42);
            var template = MakeEntities(EntityCount, rng);

            // Warmup both paths.
            RunNewPattern(CloneEntities(template));
            RunOldPattern(CloneEntities(template));

            long newChecksum = 0;
            var swNew = new Stopwatch();
            for (var i = 0; i < Iterations; i++)
            {
                // Fresh entities per iteration so the lazy tag-index build cost is always included,
                // mirroring a real polling tick which materializes new BgsEntity instances every time.
                var entities = CloneEntities(template);
                swNew.Start();
                newChecksum += RunNewPattern(entities);
                swNew.Stop();
            }

            long oldChecksum = 0;
            var swOld = new Stopwatch();
            for (var i = 0; i < Iterations; i++)
            {
                var entities = CloneEntities(template);
                swOld.Start();
                oldChecksum += RunOldPattern(entities);
                swOld.Stop();
            }

            var newMs = swNew.Elapsed.TotalMilliseconds / Iterations;
            var oldMs = swOld.Elapsed.TotalMilliseconds / Iterations;
            Console.WriteLine($"old (Tags.Find + per-minion scan): {oldMs:F4} ms/iter");
            Console.WriteLine($"new (tag index + lookup):          {newMs:F4} ms/iter");
            Console.WriteLine($"speedup: {oldMs / Math.Max(newMs, 0.0001):F1}x");

            Assert.AreEqual(oldChecksum, newChecksum, "Old and new access patterns must produce identical results.");
            Assert.IsTrue(newMs < oldMs, $"Tag index should not be slower (old {oldMs:F4} ms, new {newMs:F4} ms).");
        }

        /// <summary>The board-read pattern as implemented now: GetTag (dictionary) + pre-grouped lookup.</summary>
        private static long RunNewPattern(List<BgsEntity> entities)
        {
            var lookup = entities.ToLookup(e => (e.GetZone(), e.GetTag(GameTag.ATTACHED)));
            var board = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetController() == 1)
                .Where(e => e.IsOnBoard())
                .Select(e =>
                {
                    e.Enchantments = lookup[(e.GetZone(), e.GetTag(GameTag.ENTITY_ID))].ToList();
                    return e;
                })
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            return board.Count + board.Sum(b => b.Enchantments.Count);
        }

        /// <summary>The previous implementation: every tag query is a linear Tags.Find, and enchantments
        /// are matched by scanning all entities for every board minion.</summary>
        private static long RunOldPattern(List<BgsEntity> entities)
        {
            int FindTag(BgsEntity e, GameTag tag, int def = 0) => e.Tags.Find(t => t.Name == (int)tag)?.Value ?? def;
            Zone FindZone(BgsEntity e) => (Zone)FindTag(e, GameTag.ZONE, (int)Zone.INVALID);
            CardType FindCardType(BgsEntity e) => (CardType)FindTag(e, GameTag.CARDTYPE, (int)CardType.INVALID);
            bool IsOnBoard(BgsEntity e)
            {
                var ct = FindCardType(e);
                return ct == CardType.MINION || ct == CardType.BATTLEGROUND_SPELL || ct == CardType.LOCATION;
            }

            var board = entities
                .Where(e => FindZone(e) == Zone.PLAY)
                .Where(e => FindTag(e, GameTag.CONTROLLER) == 1)
                .Where(e => IsOnBoard(e))
                .Select(e =>
                {
                    e.Enchantments = entities
                        .Where(o => FindZone(o) == FindZone(e))
                        .Where(o => FindTag(o, GameTag.ATTACHED) == FindTag(e, GameTag.ENTITY_ID))
                        .ToList();
                    return e;
                })
                .OrderBy(e => FindTag(e, GameTag.ZONE_POSITION))
                .ToList();
            return board.Count + board.Sum(b => b.Enchantments.Count);
        }

        private static List<BgsEntity> MakeEntities(int count, Random rng)
        {
            var entities = new List<BgsEntity>(count);
            for (var id = 1; id <= count; id++)
            {
                var tags = new List<EntityTag>();
                for (var t = 0; t < 36; t++)
                {
                    tags.Add(new EntityTag { Name = 2000 + rng.Next(500), Value = rng.Next(100) });
                }

                Insert(tags, rng, (int)GameTag.ENTITY_ID, id);
                Insert(tags, rng, (int)GameTag.ZONE, rng.Next(3) == 0 ? (int)Zone.PLAY : (int)Zone.HAND);
                Insert(tags, rng, (int)GameTag.CONTROLLER, rng.Next(2) + 1);
                Insert(tags, rng, (int)GameTag.CARDTYPE, (int)CardType.MINION);
                Insert(tags, rng, (int)GameTag.ZONE_POSITION, rng.Next(7));
                if (rng.Next(4) == 0)
                {
                    Insert(tags, rng, (int)GameTag.ATTACHED, rng.Next(count) + 1);
                }

                entities.Add(new BgsEntity { CardId = "TB_BaconShop_" + id, Tags = tags });
            }

            return entities;
        }

        private static void Insert(List<EntityTag> tags, Random rng, int name, int value)
            => tags.Insert(rng.Next(tags.Count + 1), new EntityTag { Name = name, Value = value });

        private static List<BgsEntity> CloneEntities(List<BgsEntity> source)
            => source.Select(e => new BgsEntity { CardId = e.CardId, Tags = e.Tags }).ToList();
    }
}
