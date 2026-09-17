using System;
using System.Collections.Generic;
using System.Linq;
using HackF5.UnitySpy;

namespace HackF5.UnitySpy.Gui.Services
{
    public sealed class HearthstoneShortcut
    {
        public HearthstoneShortcut(string name, PathRootKind kind, ITypeDefinition? type)
        {
            this.Name = name;
            this.Kind = kind;
            this.Type = type;
        }

        public string Name { get; }

        public PathRootKind Kind { get; }

        public ITypeDefinition? Type { get; }

        public string FullName => this.Type?.FullName ?? this.Name;
    }

    public static class HearthstoneShortcutService
    {
        public static IReadOnlyList<HearthstoneShortcut> Build(IAssemblyImage image, IReadOnlyList<ITypeDefinition> types)
        {
            var byName = new Dictionary<string, ITypeDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var type in types)
            {
                if (!string.IsNullOrEmpty(type.Name) && !byName.ContainsKey(type.Name))
                {
                    byName[type.Name] = type;
                }

                if (!string.IsNullOrEmpty(type.FullName) && !byName.ContainsKey(type.FullName))
                {
                    byName[type.FullName] = type;
                }
            }

            var results = new List<HearthstoneShortcut>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in types)
            {
                if (!HasStaticInstance(type))
                {
                    continue;
                }

                if (seen.Add(type.FullName))
                {
                    results.Add(new HearthstoneShortcut(type.FullName, PathRootKind.Type, type));
                }
            }

            foreach (var name in ListServiceNames(image))
            {
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                {
                    continue;
                }

                byName.TryGetValue(name, out var type);
                results.Add(new HearthstoneShortcut(name, PathRootKind.Service, type));
            }

            foreach (var name in ListNetCacheNames(image))
            {
                if (string.IsNullOrEmpty(name) || !seen.Add(name))
                {
                    continue;
                }

                byName.TryGetValue(name, out var type);
                results.Add(new HearthstoneShortcut(name, PathRootKind.NetCache, type));
            }

            return results
                .OrderBy(s => s.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static object? TryResolve(IAssemblyImage image, HearthstoneShortcut shortcut)
        {
            if (shortcut.Kind == PathRootKind.Type && shortcut.Type != null)
            {
                return shortcut.Type;
            }

            if (shortcut.Kind == PathRootKind.Service)
            {
                return TryGetService(image, shortcut.Name);
            }

            if (shortcut.Kind == PathRootKind.NetCache)
            {
                return TryGetNetCacheService(image, shortcut.Name);
            }

            return shortcut.Type;
        }

        public static bool HasStaticInstance(ITypeDefinition type)
        {
            try
            {
                return type.Fields.Any(f =>
                    f.TypeInfo != null
                    && f.TypeInfo.IsStatic
                    && !f.TypeInfo.IsConstant
                    && f.Name != null
                    && f.Name.IndexOf("s_instance", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch
            {
                return false;
            }
        }

        private static IEnumerable<string> ListServiceNames(IAssemblyImage image)
        {
            try
            {
                var services = ResolveServices(image);
                var entries = services?["_entries"];
                if (entries == null)
                {
                    return Array.Empty<string>();
                }

                var names = new List<string>();
                foreach (dynamic entry in entries)
                {
                    var name = entry?["value"]?["<ServiceTypeName>k__BackingField"] as string;
                    if (!string.IsNullOrEmpty(name))
                    {
                        names.Add(name!);
                    }
                }

                return names;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static IEnumerable<string> ListNetCacheNames(IAssemblyImage image)
        {
            try
            {
                dynamic? netCache = TryGetService(image, "NetCache");
                var slots = netCache?["m_netCache"]?["valueSlots"];
                if (slots == null)
                {
                    return Array.Empty<string>();
                }

                var names = new List<string>();
                foreach (dynamic slot in slots)
                {
                    var name = slot?.TypeDefinition?.Name as string;
                    if (!string.IsNullOrEmpty(name))
                    {
                        names.Add(name!);
                    }
                }

                return names;
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static object? TryGetService(IAssemblyImage image, string name)
        {
            try
            {
                var services = ResolveServices(image);
                var entries = services?["_entries"];
                if (entries == null)
                {
                    return null;
                }

                foreach (dynamic entry in entries)
                {
                    var serviceName = entry?["value"]?["<ServiceTypeName>k__BackingField"] as string;
                    if (string.Equals(serviceName, name, StringComparison.Ordinal))
                    {
                        return entry?["value"]?["<Service>k__BackingField"];
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static object? TryGetNetCacheService(IAssemblyImage image, string name)
        {
            try
            {
                dynamic? netCache = TryGetService(image, "NetCache");
                var slots = netCache?["m_netCache"]?["valueSlots"];
                if (slots == null)
                {
                    return null;
                }

                foreach (dynamic slot in slots)
                {
                    var slotName = slot?.TypeDefinition?.Name as string;
                    if (string.Equals(slotName, name, StringComparison.Ordinal))
                    {
                        return slot;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static dynamic? ResolveServices(IAssemblyImage image)
        {
            var dependencyBuilders = image["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"];
            if (dependencyBuilders == null)
            {
                return null;
            }

            var serviceLocator = dependencyBuilders[0]?["m_serviceLocator"];
            return serviceLocator?["m_services"];
        }
    }
}
