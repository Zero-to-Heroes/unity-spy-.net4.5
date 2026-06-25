namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using HackF5.UnitySpy;
    using HackF5.UnitySpy.Detail;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DebugScanTests
    {
        private const string OutputPath = @"E:\Source\zerotoheroes\forks\unityspy-2\scan-output.txt";

        [TestMethod]
        public void DebugScan()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");

            using (var writer = new StreamWriter(OutputPath, false))
            {
                Action<string> log = line =>
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    Console.WriteLine(line);
                };

                log($"PID={process.Id} path={SafePath(process)}");
                try
                {
                    AssemblyImageFactory.DebugScan(process.Id, log);
                }
                catch (Exception e)
                {
                    log($"EXCEPTION: {e}");
                    throw;
                }
            }
        }

        [TestMethod]
        public void DebugNetCache()
        {
            using (var writer = new StreamWriter(@"E:\Source\zerotoheroes\forks\unityspy-2\netcache-output.txt", false))
            {
                void Log(string s)
                {
                    writer.WriteLine(s);
                    writer.Flush();
                    Console.WriteLine(s);
                }

                var process = FindHearthstoneX64();
                Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");
                Log($"PID={process.Id} path={SafePath(process)}");

                var mv = new MindVision(null, "Hearthstone", process.Id);
                var services = mv.ListServices();
                Log($"Services ({services?.Count ?? -1}): {(services == null ? "null" : string.Join(", ", services))}");

                var netCache = mv.ListNetCacheServices();
                Log($"NetCache services ({netCache?.Count ?? -1}): {(netCache == null ? "null" : string.Join(", ", netCache))}");

                var cardBacks = mv.GetCollectionCardBacks();
                Log($"CardBacks count={cardBacks?.Count ?? -1}");

                try
                {
                    var rt = mv.GetRewardTrackInfo();
                    Log($"RewardTrackInfo null? {rt == null}");
                }
                catch (Exception e)
                {
                    Log($"RewardTrackInfo EXCEPTION: {e.Message}");
                }
            }
        }

        [TestMethod]
        public void DebugCardBacks()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process);
            using (var writer = new StreamWriter(@"E:\Source\zerotoheroes\forks\unityspy-2\cardbacks-output.txt", false))
            {
                void Log(string s) { writer.WriteLine(s); writer.Flush(); }

                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var pf = ((AssemblyImage)image).Process;
                var addrProp = typeof(MemoryObject).GetProperty("Address", BindingFlags.NonPublic | BindingFlags.Instance);

                var mv = new MindVision(null, "Hearthstone", process.Id);
                dynamic dimage = image;
                dynamic netCacheValues = dimage["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"][0]?["m_serviceLocator"]?["m_services"]["_entries"];
                // Find NetCache service then NetCacheCardBacks - simpler to go via reader path:
                dynamic cardBacksField = null;
                dynamic netCache = null;
                int len = netCacheValues.Length;
                for (int i = 0; i < len; i++)
                {
                    var svc = netCacheValues[i]?["value"]?["<ServiceTypeName>k__BackingField"];
                    if (svc == "NetCache") { netCache = netCacheValues[i]["value"]["<Service>k__BackingField"]; break; }
                }
                dynamic valueSlots = netCache?["m_netCache"]?["valueSlots"];
                dynamic cb = null;
                foreach (var nc in valueSlots) { if (nc?.TypeDefinition.Name == "NetCacheCardBacks") { cb = nc; break; } }
                cardBacksField = cb?["<CardBacks>k__BackingField"];
                Log($"cardBacksField type={(cardBacksField == null ? "null" : (string)cardBacksField.TypeDefinition.FullName)}");
                ITypeDefinition hsTd = ((object)cardBacksField).GetType().GetProperty("TypeDefinition").GetValue((object)cardBacksField) as ITypeDefinition;
                Log($"HashSet fields: {string.Join(", ", hsTd.Fields.Where(f => !(f.TypeInfo?.IsStatic ?? false)).Select(f => f.Name))}");
                int liVal = -999;
                try { liVal = cardBacksField["_lastIndex"]; } catch (Exception ex) { Log($"_lastIndex read err: {ex.Message}"); }
                Log($"_count={cardBacksField?["_count"]} _lastIndex={liVal}");
                var slots = cardBacksField?["_slots"];
                int slen = slots == null ? -1 : (int)slots.Length;
                Log($"_slots length={slen}");
                // Print the Slot type field offsets.
                var slot0 = slots[0];
                ITypeDefinition slotTd = ((object)slot0).GetType().GetProperty("TypeDefinition").GetValue(slot0) as ITypeDefinition;
                Log($"Slot type={slotTd?.FullName}");
                foreach (var f in slotTd.Fields)
                {
                    Log($"   field {f.Name} static={f.TypeInfo?.IsStatic}");
                }

                int zeros = 0, nonzeros = 0;
                var zeroIndices = new List<int>();
                for (int i = 0; i < slen; i++)
                {
                    var slot = slots[i];
                    int val = slot["value"];
                    if (val == 0) { zeros++; zeroIndices.Add(i); } else nonzeros++;
                    if (i >= 372 && i <= 385)
                    {
                        var a = (IntPtr)addrProp.GetValue(slot);
                        var sb = new System.Text.StringBuilder($"[{i}] value={val} raw:");
                        for (int q = 0; q < 3; q++) { try { sb.Append($" +{q * 4}={pf.ReadInt32(a + (q * 4))}"); } catch { break; } }
                        Log(sb.ToString());
                    }
                }
                Log($"zeros={zeros} nonzeros={nonzeros}");
                Log($"zero indices: {string.Join(",", zeroIndices)}");
            }
        }

        [TestMethod]
        public void DebugQuests()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process);
            using (var writer = new StreamWriter(@"E:\Source\zerotoheroes\forks\unityspy-2\quests-output.txt", false))
            {
                void Log(string s) { writer.WriteLine(s); writer.Flush(); }

                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var pf = ((AssemblyImage)image).Process;
                Log($"PID={process.Id} path={SafePath(process)} Is64Bits={pf.Is64Bits} SizeOfPtr={pf.SizeOfPtr}");
                var addrProp = typeof(MemoryObject).GetProperty("Address", BindingFlags.NonPublic | BindingFlags.Instance);
                IntPtr AddrOf(object o) => o == null ? IntPtr.Zero : (IntPtr)addrProp.GetValue(o);

                dynamic dimage = image;
                dynamic services = dimage["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"][0]?["m_serviceLocator"]?["m_services"]?["_entries"];
                dynamic questMgr = null;
                int slen = services.Length;
                for (int i = 0; i < slen; i++)
                {
                    var nm = services[i]?["value"]?["<ServiceTypeName>k__BackingField"];
                    if (nm == "Hearthstone.Progression.QuestManager") { questMgr = services[i]["value"]["<Service>k__BackingField"]; break; }
                }
                Log($"questMgr null? {questMgr == null}");

                dynamic questState = questMgr["m_questState"];
                ITypeDefinition qsTd = ((object)questState).GetType().GetProperty("TypeDefinition").GetValue((object)questState) as ITypeDefinition;
                Log($"m_questState type={qsTd?.FullName}");
                int count = questState["_count"];
                Log($"_count={count}");

                dynamic entries = questState["_entries"];
                int elen = entries.Length;
                Log($"_entries length={elen}");

                // Inspect the Entry element type and its field offsets as UnitySpy computes them.
                var e0 = entries[0];
                TypeDefinition entryTd = ((object)e0).GetType().GetProperty("TypeDefinition").GetValue(e0) as TypeDefinition;
                Log($"Entry type={entryTd?.FullName} valueType={entryTd?.IsValueType} size={entryTd?.Size}");
                foreach (var f in entryTd.Fields)
                {
                    var fd = f as FieldDefinition;
                    Log($"   field {fd.Name} offset={fd.Offset} typeCode={fd.TypeInfo?.TypeCode} static={fd.TypeInfo?.IsStatic}");
                }

                // Dump raw bytes of the first few entries and show ptr reads at +12 and +16.
                for (int i = 0; i < Math.Min(elen, 6); i++)
                {
                    var a = AddrOf(entries[i]);
                    var sb = new System.Text.StringBuilder($"[{i}] @0x{a.ToInt64():X} bytes:");
                    for (int q = 0; q < 28; q += 4) { try { sb.Append($" +{q}=0x{pf.ReadUInt32(a + q):X8}"); } catch { break; } }
                    Log(sb.ToString());
                    try { Log($"     ReadPtr(+12)=0x{pf.ReadPtr(a + 12).ToInt64():X}  ReadPtr(+16)=0x{pf.ReadPtr(a + 16).ToInt64():X}"); } catch { }
                }
            }
        }

        [TestMethod]
        public void DebugServices()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process);

            using (var writer = new StreamWriter(@"E:\Source\zerotoheroes\forks\unityspy-2\services-output.txt", false))
            {
                void Log(string s)
                {
                    writer.WriteLine(s);
                    writer.Flush();
                    Console.WriteLine(s);
                }

                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var pf = ((AssemblyImage)image).Process;

                var addrProp = typeof(MemoryObject).GetProperty("Address", BindingFlags.NonPublic | BindingFlags.Instance);
                long AddrOf(object o) => o == null ? 0 : ((IntPtr)addrProp.GetValue(o)).ToInt64();
                string TypeNameOf(object o)
                {
                    if (o == null) return "<null>";
                    var td = o.GetType().GetProperty("TypeDefinition")?.GetValue(o);
                    return (td as ITypeDefinition)?.FullName ?? o.GetType().Name;
                }

                dynamic dimage = image;
                dynamic dep = dimage["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"];
                Log($"dep items length={(dep == null ? -1 : (int)dep.Length)}");
                dynamic loc = dep[0]?["m_serviceLocator"];
                dynamic services = loc?["m_services"];
                Log($"services type={TypeNameOf((object)services)}");

                dynamic entries = services?["_entries"];
                var arr = (object[])entries;
                Log($"_entries length={arr?.Length ?? -1}");

                // Dump the Entry[] array's MonoClass to locate element_size (expected 24).
                var entry0 = (IntPtr)addrProp.GetValue(arr[0]);
                var arrayObj = entry0 - 32; // start = arrayObj + SizeOfPtr*4
                var vtable = pf.ReadPtr(arrayObj);
                var arrayKlass = pf.ReadPtr(vtable);
                var elementKlass = pf.ReadPtr(arrayKlass); // element_class is first field
                Log($"Entry[] arrayObj=0x{arrayObj.ToInt64():X} arrayKlass=0x{arrayKlass.ToInt64():X} elementKlass=0x{elementKlass.ToInt64():X}");
                DumpKlass("Entry[] arrayKlass", arrayKlass);
                DumpKlass("Entry elementKlass", elementKlass);

                void DumpKlass(string label, IntPtr klass)
                {
                    Log($"== {label} @0x{klass.ToInt64():X} ==");
                    for (var q = 0; q < 24; q++)
                    {
                        try
                        {
                            var v = pf.ReadInt64(klass + (q * 8));
                            var lo = (int)(v & 0xffffffff);
                            var hi = (int)((v >> 32) & 0xffffffff);
                            Log($"   +{q * 8,3} (0x{q * 8:X2}): 0x{v:X16} ints({lo},{hi})");
                        }
                        catch { break; }
                    }
                }
            }
        }

        private static string SafePath(Process p)
        {
            try
            {
                return p.MainModule?.FileName;
            }
            catch
            {
                return "<denied>";
            }
        }

        private static Process FindHearthstoneX64()
        {
            var candidates = Process.GetProcessesByName("Hearthstone");
            // Prefer the Event build explicitly if present.
            var preferred = candidates.FirstOrDefault(p =>
            {
                try
                {
                    return (p.MainModule?.FileName ?? string.Empty).IndexOf("Hearthstone_Event_1", StringComparison.OrdinalIgnoreCase) >= 0;
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
