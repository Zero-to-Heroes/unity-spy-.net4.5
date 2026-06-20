namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using HackF5.UnitySpy.Detail;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Benchmarks the memory reader against a live Hearthstone client.
    ///
    /// This deliberately measures the costs that matter to the end user:
    ///  - latency (wall-clock ms per operation, avg + p95) = responsiveness, and
    ///  - CPU time consumed (kernel + user, via GetThreadTimes) = the ongoing resource cost of polling.
    ///
    /// Syscall count is reported only as a sanity check (see <see cref="ProcessFacade.ReadProcessMemoryCallCount"/>);
    /// it is an implementation detail, not a headline metric.
    ///
    /// Requires Hearthstone to be running. BenchmarkDotNet is intentionally not used because these reads
    /// depend on live external game state, which breaks its repeatable-microbenchmark assumptions.
    /// </summary>
    [TestClass]
    public class PerformanceBenchmark
    {
        // Keep modest so a full run stays quick; raise locally for more stable percentiles.
        private const int Iterations = 50;

        [TestMethod]
        public void RunBenchmark()
        {
            var mindVision = new MindVision();

            var scenarios = new List<Scenario>
            {
                new Scenario("GetCollectionCards", () => mindVision.GetCollectionCards()?.Count ?? 0),
                new Scenario("GetCollectionCoins", () => mindVision.GetCollectionCoins()?.Count ?? 0),
                new Scenario("GetCollectionCardBacks", () => mindVision.GetCollectionCardBacks()?.Count ?? 0),
                new Scenario("GetCollectionSize", () => mindVision.GetCollectionSize()),
                new Scenario("GetBattlegroundsInfo", () => mindVision.GetBattlegroundsInfo() == null ? 0 : 1),
                new Scenario("GetSceneMode", () => mindVision.GetSceneMode() == null ? 0 : 1),
                new Scenario("GetAccountInfo", () => mindVision.GetAccountInfo() == null ? 0 : 1),
                // Aggregate "one polling tick": the kind of mixed read set the app pays for every ~200ms.
                new Scenario("PollingTick (aggregate)", () =>
                {
                    var n = 0;
                    n += mindVision.GetSceneMode() == null ? 0 : 1;
                    n += mindVision.GetCollectionSize();
                    n += mindVision.GetBattlegroundsInfo() == null ? 0 : 1;
                    n += mindVision.GetAccountInfo() == null ? 0 : 1;
                    return n;
                }),
            };

            // Warmup: pay for process attach + structural caches once, excluded from measurements.
            foreach (var scenario in scenarios)
            {
                TryRun(scenario);
            }

            var results = new List<ScenarioResult>();

            // Measure with and without the Tier 1a block-read optimization so the two can be compared directly.
            foreach (var useBlockReads in new[] { false, true })
            {
                ProcessFacade.UseBlockReads = useBlockReads;
                var label = useBlockReads ? " [block]" : string.Empty;

                // Re-warm so the first measured iteration of each mode isn't penalised by mode-specific setup.
                foreach (var scenario in scenarios)
                {
                    TryRun(scenario);
                }

                foreach (var scenario in scenarios)
                {
                    results.Add(Measure(scenario, label));
                }
            }

            ProcessFacade.UseBlockReads = false;
            Report(results);
        }

        private static ScenarioResult Measure(Scenario scenario, string label)
        {
            var latenciesMs = new double[Iterations];
            long lastResultSize = 0;
            string error = null;

            long cpuStart = ThreadCpuTime.CurrentThreadCpuTicks();
            long syscallStart = ProcessFacade.ReadProcessMemoryCallCount;
            var sw = new Stopwatch();

            for (var i = 0; i < Iterations; i++)
            {
                sw.Restart();
                try
                {
                    lastResultSize = scenario.Action();
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                sw.Stop();
                latenciesMs[i] = sw.Elapsed.TotalMilliseconds;
            }

            long cpuTicks = ThreadCpuTime.CurrentThreadCpuTicks() - cpuStart;
            long syscalls = ProcessFacade.ReadProcessMemoryCallCount - syscallStart;

            Array.Sort(latenciesMs);
            return new ScenarioResult
            {
                Name = scenario.Name + label,
                Iterations = Iterations,
                AvgLatencyMs = latenciesMs.Average(),
                P95LatencyMs = Percentile(latenciesMs, 95),
                MaxLatencyMs = latenciesMs[latenciesMs.Length - 1],
                // 1 tick == 100 ns, so /10000 -> ms. Reported per-iteration.
                CpuMsPerIteration = (cpuTicks / 10000.0) / Iterations,
                SyscallsPerIteration = (double)syscalls / Iterations,
                ResultSize = lastResultSize,
                Error = error,
            };
        }

        private static void TryRun(Scenario scenario)
        {
            try
            {
                scenario.Action();
            }
            catch
            {
                // Warmup failures are surfaced later by the measured run.
            }
        }

        private static double Percentile(double[] sortedAscending, double percentile)
        {
            if (sortedAscending.Length == 0)
            {
                return 0;
            }

            var rank = (percentile / 100.0) * (sortedAscending.Length - 1);
            var low = (int)Math.Floor(rank);
            var high = (int)Math.Ceiling(rank);
            if (low == high)
            {
                return sortedAscending[low];
            }

            var weight = rank - low;
            return (sortedAscending[low] * (1 - weight)) + (sortedAscending[high] * weight);
        }

        private static void Report(List<ScenarioResult> results)
        {
            var header = string.Format(
                "{0,-26} {1,8} {2,12} {3,12} {4,12} {5,14} {6,14} {7,8}",
                "Scenario", "Iters", "Avg ms", "p95 ms", "Max ms", "CPU ms/it", "Reads/it", "Size");
            Trace.WriteLine(header);
            Trace.WriteLine(new string('-', header.Length));
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));

            foreach (var r in results)
            {
                var line = string.Format(
                    "{0,-26} {1,8} {2,12:F3} {3,12:F3} {4,12:F3} {5,14:F3} {6,14:F1} {7,8}{8}",
                    r.Name, r.Iterations, r.AvgLatencyMs, r.P95LatencyMs, r.MaxLatencyMs,
                    r.CpuMsPerIteration, r.SyscallsPerIteration, r.ResultSize,
                    r.Error == null ? string.Empty : "  ERROR: " + r.Error);
                Trace.WriteLine(line);
                Console.WriteLine(line);
            }

            var outputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "perf-results-" + DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss") + ".json");
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(results, Formatting.Indented));
            Trace.WriteLine("Results written to " + outputPath);
            Console.WriteLine("Results written to " + outputPath);
        }

        private sealed class Scenario
        {
            public Scenario(string name, Func<long> action)
            {
                this.Name = name;
                this.Action = action;
            }

            public string Name { get; }

            public Func<long> Action { get; }
        }

        private sealed class ScenarioResult
        {
            public string Name { get; set; }

            public int Iterations { get; set; }

            public double AvgLatencyMs { get; set; }

            public double P95LatencyMs { get; set; }

            public double MaxLatencyMs { get; set; }

            public double CpuMsPerIteration { get; set; }

            public double SyscallsPerIteration { get; set; }

            public long ResultSize { get; set; }

            public string Error { get; set; }
        }

        private static class ThreadCpuTime
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool GetThreadTimes(
                IntPtr hThread,
                out long lpCreationTime,
                out long lpExitTime,
                out long lpKernelTime,
                out long lpUserTime);

            [DllImport("kernel32.dll")]
            private static extern IntPtr GetCurrentThread();

            /// <summary>
            /// CPU time (kernel + user) consumed by the calling thread, in 100ns ticks.
            /// </summary>
            public static long CurrentThreadCpuTicks()
            {
                if (GetThreadTimes(GetCurrentThread(), out _, out _, out var kernel, out var user))
                {
                    return kernel + user;
                }

                return 0;
            }
        }
    }
}
