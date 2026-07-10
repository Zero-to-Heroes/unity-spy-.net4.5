namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
    using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
    using HackF5.UnitySpy.HearthstoneLib.MemoryUpdate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Times MindVision.GetMemoryChanges against a live client. After the first (init) call, every call
    /// performs a full collection read plus a diff against the retained snapshot, so this exercises the
    /// CollectionInitNotifier diff path end to end. Requires Hearthstone to be running.
    /// </summary>
    [TestClass]
    public class MemoryChangesBenchmark
    {
        private const int Iterations = 20;

        [TestMethod]
        public void TimeGetMemoryChanges()
        {
            var mindVision = new MindVision();

            // First call initializes the retained snapshot; second warms caches.
            mindVision.GetMemoryChanges();
            mindVision.GetMemoryChanges();

            var sw = new Stopwatch();
            var latencies = new double[Iterations];
            for (var i = 0; i < Iterations; i++)
            {
                sw.Restart();
                mindVision.GetMemoryChanges();
                sw.Stop();
                latencies[i] = sw.Elapsed.TotalMilliseconds;
            }

            Array.Sort(latencies);
            double avg = 0;
            foreach (var l in latencies) { avg += l; }
            avg /= Iterations;
            Console.WriteLine($"GetMemoryChanges: {Iterations} iters, avg {avg:F2} ms, median {latencies[Iterations / 2]:F2} ms, max {latencies[Iterations - 1]:F2} ms");
        }

        /// <summary>
        /// CPU micro-benchmark for the collection diff itself (no live client needed): compares the previous
        /// O(N^2) per-card scan against the dictionary-based diff on a realistic 8000-card collection with a
        /// few additions, and checks both produce identical results.
        /// </summary>
        [TestMethod]
        public void CompareCollectionDiffImplementations()
        {
            const int cards = 8000;
            const int diffIterations = 20;

            var previous = MakeCollection(cards);
            var current = MakeCollection(cards);
            ((CollectionCard)current[1234]).Count += 1;
            ((CollectionCard)current[5678]).PremiumCount += 2;

            var notifier = new CollectionInitNotifier();

            // Warmup + correctness check.
            var newResult = notifier.GetNewCards(current, previous);
            var oldResult = GetNewCardsOld(current, previous);
            CollectionAssert.AreEqual(
                oldResult.Select(c => $"{c.CardId}|{c.Premium}|{c.TotalCount}").ToList(),
                newResult.Select(c => $"{c.CardId}|{c.Premium}|{c.TotalCount}").ToList(),
                "Old and new diffs must produce identical results.");

            var swNew = Stopwatch.StartNew();
            for (var i = 0; i < diffIterations; i++)
            {
                notifier.GetNewCards(current, previous);
            }
            swNew.Stop();

            var swOld = Stopwatch.StartNew();
            for (var i = 0; i < diffIterations; i++)
            {
                GetNewCardsOld(current, previous);
            }
            swOld.Stop();

            var newMs = swNew.Elapsed.TotalMilliseconds / diffIterations;
            var oldMs = swOld.Elapsed.TotalMilliseconds / diffIterations;
            Console.WriteLine($"old (per-card scan):  {oldMs:F2} ms/diff");
            Console.WriteLine($"new (dictionary):     {newMs:F2} ms/diff");
            Console.WriteLine($"speedup: {oldMs / Math.Max(newMs, 0.0001):F1}x");
            Assert.IsTrue(newMs < oldMs, $"Dictionary diff should not be slower (old {oldMs:F2} ms, new {newMs:F2} ms).");
        }

        /// <summary>The previous GetNewCards implementation, kept for comparison.</summary>
        private static List<ICardInfo> GetNewCardsOld(
            IReadOnlyList<ICollectionCard> newCollection, IReadOnlyList<ICollectionCard> previousCollection)
        {
            var result = new List<ICardInfo>();
            foreach (var newCard in newCollection)
            {
                var existingCard = previousCollection.Where(card => card.CardId == newCard.CardId).FirstOrDefault();
                if (existingCard == null)
                {
                    continue;
                }

                var newCount = Math.Max(0, newCard.Count - existingCard.Count);
                for (int i = 0; i < newCount; i++)
                {
                    result.Add(new CardInfo() { CardId = newCard.CardId, Premium = 0, TotalCount = newCard.Count });
                }

                var newPremiumCount = Math.Max(0, newCard.PremiumCount - existingCard.PremiumCount);
                for (int i = 0; i < newPremiumCount; i++)
                {
                    result.Add(new CardInfo() { CardId = newCard.CardId, Premium = 1, TotalCount = newCard.PremiumCount });
                }
            }

            return result;
        }

        private static IReadOnlyList<ICollectionCard> MakeCollection(int count)
        {
            var cards = new List<ICollectionCard>(count);
            for (var i = 0; i < count; i++)
            {
                cards.Add(new CollectionCard
                {
                    CardId = "CARD_" + i,
                    Count = 1 + (i % 2),
                    PremiumCount = i % 3 == 0 ? 1 : 0,
                });
            }

            return cards;
        }
    }
}
