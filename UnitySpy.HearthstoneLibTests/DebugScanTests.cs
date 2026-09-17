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
    using Newtonsoft.Json;

    [TestClass]
    public class DebugScanTests
    {
        private static readonly string OutputDir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            "debug-scan");

        private static string OutputFile(string name)
        {
            Directory.CreateDirectory(OutputDir);
            return Path.Combine(OutputDir, name);
        }

        [TestMethod]
        public void DebugBlackMarket()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");

            using (var writer = new StreamWriter(OutputFile("black-market.txt"), false))
            {
                Action<string> log = line =>
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    Console.WriteLine(line);
                };

                log($"PID={process.Id} path={SafePath(process)}");
                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var mindVision = new HackF5.UnitySpy.HearthstoneLib.MindVision();
                var info = mindVision.GetBlackMarketInfo();
                log($"GetBlackMarketInfo={JsonConvert.SerializeObject(info)}");

                var types = image.TypeDefinitions
                    .Where(t =>
                    {
                        var n = t.Name ?? string.Empty;
                        var f = t.FullName ?? string.Empty;
                        return n.IndexOf("BlackMarket", StringComparison.OrdinalIgnoreCase) >= 0
                            || f.IndexOf("BlackMarket", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Bmec", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .OrderBy(t => t.FullName)
                    .ToList();
                log($"black-market types ({types.Count}):");
                foreach (var t in types)
                {
                    log($"  {t.FullName}");
                    foreach (var field in t.Fields.Where(f =>
                    {
                        var fn = f.Name ?? string.Empty;
                        return fn.IndexOf("Daily", StringComparison.OrdinalIgnoreCase) >= 0
                            || fn.IndexOf("Earn", StringComparison.OrdinalIgnoreCase) >= 0
                            || fn.IndexOf("Balance", StringComparison.OrdinalIgnoreCase) >= 0;
                    }))
                    {
                        log($"    field {field.Name}");
                    }
                }

                foreach (var typeName in new[]
                {
                    "Hearthstone.DataModels.BlackMarketGlobalDataModel",
                    "Hearthstone.DataModels.BlackMarketDataModel",
                    "CurrencyManager",
                    "BlackMarketEventManager+BmecRewardData",
                    "NetCache+NetCacheBmecBalance",
                })
                {
                    DumpType(image, typeName, log);
                }

                DumpServiceInstance(image, "Hearthstone.BlackMarket.BlackMarketEventManager", log);
            }
        }

        private static void DumpServiceInstance(IAssemblyImage image, string serviceName, Action<string> log)
        {
            log($"== service {serviceName} ==");
            try
            {
                var mindVision = new HackF5.UnitySpy.HearthstoneLib.MindVision();
                // Re-read via GetBlackMarketInfo fields by walking the same service locator the reader uses.
                dynamic dimage = image;
                dynamic dep = dimage["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"];
                dynamic loc = dep?[0]?["m_serviceLocator"];
                dynamic services = loc?["m_services"];
                dynamic entries = services?["_entries"];
                if (entries == null)
                {
                    log("  no service locator entries");
                    return;
                }

                IManagedObjectInstance service = null;
                foreach (var entry in entries)
                {
                    var name = entry?["value"]?["<ServiceTypeName>k__BackingField"] as string;
                    if (name == serviceName)
                    {
                        service = entry?["value"]?["<Service>k__BackingField"] as IManagedObjectInstance;
                        break;
                    }
                }

                if (service == null)
                {
                    log("  service instance not found");
                    return;
                }

                log($"  instance type={service.TypeDefinition?.FullName}");
                DumpManagedInstance(service, log, "  ");
                var currentEvent = service.GetValue<object>("m_currentBlackMarketEvent", false) as IManagedObjectInstance;
                log($"  m_currentBlackMarketEvent={(currentEvent == null ? "null" : currentEvent.TypeDefinition?.FullName)}");
                if (currentEvent != null)
                {
                    DumpManagedInstance(currentEvent, log, "    event.");
                }

                var dataModel = service.GetValue<object>("m_blackMarketDataModel", false) as IManagedObjectInstance;
                log($"  m_blackMarketDataModel={(dataModel == null ? "null" : dataModel.TypeDefinition?.FullName)}");
                if (dataModel != null)
                {
                    DumpManagedInstance(dataModel, log, "    data.");
                }

                var globalModel = service.GetValue<object>("m_blackMarketGlobalDataModel", false) as IManagedObjectInstance;
                log($"  m_blackMarketGlobalDataModel={(globalModel == null ? "null" : globalModel.TypeDefinition?.FullName)}");
                if (globalModel != null)
                {
                    DumpManagedInstance(globalModel, log, "    global.");
                }

                var currency = service.GetValue<object>("m_currencyManager", false) as IManagedObjectInstance;
                log($"  m_currencyManager={(currency == null ? "null" : currency.TypeDefinition?.FullName)}");
                if (currency != null)
                {
                    DumpManagedInstance(currency, log, "    currency.");
                }

                var reward = service.GetValue<object>("m_bmecRewardData", false) as IManagedObjectInstance;
                log($"  m_bmecRewardData={(reward == null ? "null" : reward.TypeDefinition?.FullName)}");
                if (reward != null)
                {
                    DumpManagedInstance(reward, log, "    bmec.");
                }

                var wallet = currency?.GetValue<object>("m_walletDataModel", false) as IManagedObjectInstance;
                log($"  wallet={(wallet == null ? "null" : wallet.TypeDefinition?.FullName)}");
                if (wallet != null)
                {
                    DumpManagedInstance(wallet, log, "    wallet.");
                }

                var caches = currency?.GetValue<object>("m_currencyCaches", false) as IManagedObjectInstance;
                log($"  caches={(caches == null ? "null" : caches.TypeDefinition?.FullName)}");
                if (caches != null)
                {
                    DumpManagedInstance(caches, log, "    caches.");
                    try
                    {
                        var keys = caches["keySlots"];
                        var values = caches["valueSlots"];
                        int len = keys?.Length ?? 0;
                        log($"    cacheCount={len}");
                        for (int i = 0; i < len; i++)
                        {
                            log($"    cache[{i}] key={RenderValue(keys[i])} value={RenderValue(values[i])}");
                            if (values[i] is IManagedObjectInstance cache)
                            {
                                DumpManagedInstance(cache, log, $"      cache[{i}].");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        log($"    cache enum threw: {e.Message}");
                    }
                }

                try
                {
                    var names = mindVision.ListNetCacheServices();
                    log($"  netCache services ({names?.Count ?? 0}):");
                    foreach (var n in names ?? new List<string>())
                    {
                        if (n != null && (n.IndexOf("Black", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Currenc", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Bmec", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Wallet", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            log($"    {n}");
                        }
                    }

                    var netCache = service.GetValue<object>("m_netCache", false) as IManagedObjectInstance;
                    var slots = netCache?["m_netCache"]?["valueSlots"];
                    int slotLen = slots?.Length ?? 0;
                    for (int i = 0; i < slotLen; i++)
                    {
                        if (slots[i] is IManagedObjectInstance slot
                            && (slot.TypeDefinition?.Name ?? string.Empty).IndexOf("Bmec", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            log($"  netCache[{i}] {slot.TypeDefinition.FullName}");
                            DumpManagedInstance(slot, log, "    netBmec.");
                        }
                    }
                }
                catch (Exception e)
                {
                    log($"  netCache list threw: {e.Message}");
                }

                var bmecBalance = wallet?.GetValue<object>("m_BmecBalance", false) as IManagedObjectInstance;
                log($"  bmecBalance={(bmecBalance == null ? "null" : bmecBalance.TypeDefinition?.FullName)}");
                if (bmecBalance != null)
                {
                    DumpManagedInstance(bmecBalance, log, "    bmecBal.");
                }
            }
            catch (Exception e)
            {
                log($"  threw: {e}");
            }
        }

        private static void DumpManagedInstance(IManagedObjectInstance managed, Action<string> log, string prefix)
        {
            if (managed?.TypeDefinition == null)
            {
                return;
            }

            foreach (var field in managed.TypeDefinition.Fields.Where(f => f.TypeInfo != null && !f.TypeInfo.IsStatic))
            {
                try
                {
                    var value = managed.GetValue<object>(field.Name, false);
                    log($"{prefix}{field.Name}={RenderValue(value)}");
                }
                catch (Exception e)
                {
                    log($"{prefix}{field.Name} threw: {e.Message}");
                }
            }
        }

        [TestMethod]
        public void DebugHistory()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");

            using (var writer = new StreamWriter(OutputFile("history.txt"), false))
            {
                Action<string> log = line =>
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    Console.WriteLine(line);
                };

                log($"PID={process.Id} path={SafePath(process)}");
                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var mindVision = new HackF5.UnitySpy.HearthstoneLib.MindVision();
                log($"MindVision.IsHistoryInspectOpen()={mindVision.IsHistoryInspectOpen()}");

                var historyTypes = image.TypeDefinitions
                    .Where(t =>
                    {
                        var n = t.Name ?? string.Empty;
                        var f = t.FullName ?? string.Empty;
                        return n.IndexOf("History", StringComparison.OrdinalIgnoreCase) >= 0
                            || f.IndexOf("History", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .OrderBy(t => t.FullName)
                    .ToList();
                log($"history-like types ({historyTypes.Count}):");
                foreach (var t in historyTypes)
                {
                    log($"  {t.FullName}");
                }

                foreach (var typeName in new[] { "HistoryManager", "HistoryCard", "HistoryItem", "HistoryTile" })
                {
                    DumpType(image, typeName, log);
                }
            }
        }

        [TestMethod]
        public void DebugGameMenu()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");

            using (var writer = new StreamWriter(OutputFile("game-menu.txt"), false))
            {
                Action<string> log = line =>
                {
                    writer.WriteLine(line);
                    writer.Flush();
                    Console.WriteLine(line);
                };

                log($"PID={process.Id} path={SafePath(process)}");
                var image = AssemblyImageFactory.Create(process.Id, _ => { });
                var mindVision = new HackF5.UnitySpy.HearthstoneLib.MindVision();
                log($"MindVision.IsGameMenuOpen()={mindVision.IsGameMenuOpen()}");
                log($"MindVision.IsFriendsListOpen()={mindVision.IsFriendsListOpen()}");

                var menuTypes = image.TypeDefinitions
                    .Where(t =>
                    {
                        var n = t.Name ?? string.Empty;
                        var f = t.FullName ?? string.Empty;
                        return n.IndexOf("Menu", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Pause", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("Popup", StringComparison.OrdinalIgnoreCase) >= 0
                            || f.IndexOf("GameMenu", StringComparison.OrdinalIgnoreCase) >= 0
                            || f.IndexOf("OptionsMenu", StringComparison.OrdinalIgnoreCase) >= 0
                            || f.IndexOf("ButtonListMenu", StringComparison.OrdinalIgnoreCase) >= 0;
                    })
                    .OrderBy(t => t.FullName)
                    .ToList();
                log($"menu-like types ({menuTypes.Count}):");
                foreach (var t in menuTypes)
                {
                    log($"  {t.FullName}");
                }

                foreach (var typeName in new[] { "GameMenu", "OptionsMenu", "ButtonListMenu", "UIBPopup", "ChatMgr" })
                {
                    DumpType(image, typeName, log);
                }
            }
        }

        private static void DumpType(IAssemblyImage image, string typeName, Action<string> log)
        {
            log($"== {typeName} ==");
            var type = image.GetTypeDefinition(typeName);
            if (type == null)
            {
                log("  TYPE NOT FOUND");
                return;
            }

            var concrete = type as TypeDefinition;
            log($"  FullName={type.FullName} Parent={concrete?.Parent?.FullName}");

            foreach (var field in type.Fields)
            {
                var typeCode = field.TypeInfo?.TypeCode;
                var fieldType = field.TypeInfo != null && field.TypeInfo.TryGetTypeDefinition(out var td)
                    ? td.FullName
                    : typeCode.ToString();
                log($"  field {field.DeclaringType?.Name}.{field.Name} static={field.TypeInfo?.IsStatic} type={fieldType}");
            }

            object instance = null;
            try
            {
                instance = type["s_instance"];
                log($"  s_instance={(instance == null ? "null" : instance.ToString())}");
            }
            catch (Exception e)
            {
                log($"  s_instance threw: {e.Message}");
            }

            if (instance is IManagedObjectInstance managed)
            {
                foreach (var field in type.Fields.Where(f => f.TypeInfo != null && !f.TypeInfo.IsStatic))
                {
                    try
                    {
                        var value = managed.GetValue<object>(field.Name, false);
                        log($"    {field.Name}={RenderValue(value)}");
                    }
                    catch (Exception e)
                    {
                        log($"    {field.Name} threw: {e.Message}");
                    }
                }
            }
        }

        private static string RenderValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is bool || value is byte || value is int || value is uint || value is long || value is float || value is double || value is string)
            {
                return value.ToString();
            }

            if (value is IManagedObjectInstance managed)
            {
                return managed.TypeDefinition?.FullName ?? managed.ToString();
            }

            return value.ToString();
        }

        [TestMethod]
        public void DebugScan()
        {
            var process = FindHearthstoneX64();
            Assert.IsNotNull(process, "Could not find a 64-bit Hearthstone process.");

            using (var writer = new StreamWriter(OutputFile("scan-output.txt"), false))
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
            using (var writer = new StreamWriter(OutputFile("netcache-output.txt"), false))
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
            using (var writer = new StreamWriter(OutputFile("cardbacks-output.txt"), false))
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
            using (var writer = new StreamWriter(OutputFile("quests-output.txt"), false))
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

            using (var writer = new StreamWriter(OutputFile("services-output.txt"), false))
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
