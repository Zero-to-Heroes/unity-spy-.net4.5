namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// A/B timing harness that depends ONLY on the public <see cref="MindVision"/> API.
    ///
    /// Because it references none of the internal performance instrumentation, this same file
    /// compiles and runs against both the optimized tree and the original (stashed) tree, so the
    /// two runs can be compared apples-to-apples. Measures wall-clock latency (avg/p95/max ms) and
    /// CPU time consumed (kernel + user, GetThreadTimes) - the costs the end user actually pays.
    ///
    /// Set the BASELINE_LABEL environment variable to tag the output file (e.g. "before"/"after").
    /// Requires Hearthstone to be running.
    /// </summary>
    [TestClass]
    public class BaselineTiming
    {
        private const int Iterations = 30;

        [TestMethod]
        public void RunBaselineTiming()
        {
            var mindVision = new MindVision();

            var scenarios = new List<Scenario>
            {
                new Scenario("GetCollectionCards", () => mindVision.GetCollectionCards()?.Count ?? 0),
                new Scenario("GetCollectionCoins", () => mindVision.GetCollectionCoins()?.Count ?? 0),
                new Scenario("GetCollectionCardBacks", () => mindVision.GetCollectionCardBacks()?.Count ?? 0),
                new Scenario("GetCollectionSize", () => mindVision.GetCollectionSize()),
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

            // Warmup (process attach + structural caches), excluded from measurements.
            foreach (var scenario in scenarios)
            {
                TryRun(scenario);
                TryRun(scenario);
            }

            var results = scenarios.Select(Measure).ToList();
            Report(results);
        }

        private static ScenarioResult Measure(Scenario scenario)
        {
            var latenciesMs = new double[Iterations];
            long lastResultSize = 0;
            string error = null;

            long cpuStart = ThreadCpuTime.CurrentThreadCpuTicks();
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

            Array.Sort(latenciesMs);
            return new ScenarioResult
            {
                Name = scenario.Name,
                Iterations = Iterations,
                AvgLatencyMs = latenciesMs.Average(),
                P95LatencyMs = Percentile(latenciesMs, 95),
                MaxLatencyMs = latenciesMs[latenciesMs.Length - 1],
                CpuMsPerIteration = (cpuTicks / 10000.0) / Iterations,
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
                // Surfaced later by the measured run.
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
            var label = Environment.GetEnvironmentVariable("BASELINE_LABEL");
            if (string.IsNullOrEmpty(label))
            {
                label = "run";
            }

            var header = string.Format(
                "{0,-26} {1,8} {2,12} {3,12} {4,12} {5,14} {6,8}",
                "Scenario", "Iters", "Avg ms", "p95 ms", "Max ms", "CPU ms/it", "Size");
            Console.WriteLine(label);
            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));
            foreach (var r in results)
            {
                Console.WriteLine(string.Format(
                    "{0,-26} {1,8} {2,12:F3} {3,12:F3} {4,12:F3} {5,14:F3} {6,8}{7}",
                    r.Name, r.Iterations, r.AvgLatencyMs, r.P95LatencyMs, r.MaxLatencyMs,
                    r.CpuMsPerIteration, r.ResultSize, r.Error == null ? string.Empty : "  ERROR: " + r.Error));
            }

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "baseline-" + label + ".json");
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(results, Formatting.Indented));
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
