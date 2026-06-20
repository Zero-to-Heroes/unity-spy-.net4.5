using System;
using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail
{
    internal class HearthstoneImage : IDisposable
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(2);

        private readonly IAssemblyImage image;
        private bool disposed;

        private dynamic cachedServiceItems;
        private DateTime serviceItemsCachedAt;

        // Structural hint only: remembers which slot of the (live, re-resolved) service-locator entries array
        // last held a given service. This lets us skip the linear scan over all entries. It is NOT a data cache:
        // the slot is re-validated against live memory and the service is read live every call, so it can never
        // return stale data the way caching the resolved instances did.
        private readonly Dictionary<string, int> serviceSlotHints = new Dictionary<string, int>();

        public HearthstoneImage(IAssemblyImage image)
        {
            this.image = image;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                image?.Dispose();
                disposed = true;
            }
        }

        public dynamic this[string fullTypeName] => this.image[fullTypeName];

        public IEnumerable<ITypeDefinition> TypeDefinitions => this.image.TypeDefinitions;

        /// <param name="retryWithoutCacheIfNotFound">
        /// When true, if the service is not found in the locator snapshot, the service list cache is bypassed
        /// and the locator is read again from memory before returning null.
        /// </param>
        public dynamic GetService(string name, bool retryWithoutCacheIfNotFound = false)
        {
            try
            {
                var found = this.FindServiceInItems(ResolveServiceItems(forceRefresh: false), name);
                if (found != null)
                {
                    return found;
                }

                if (retryWithoutCacheIfNotFound)
                {
                    return this.FindServiceInItems(ResolveServiceItems(forceRefresh: true), name);
                }
            }
            catch (Exception)
            {
                InvalidateCache();
                return null;
            }

            return null;
        }

        /// <param name="retryWithoutCacheIfNotFound">
        /// When true, <see cref="GetService"/> for <c>NetCache</c> uses a fresh locator read if the first lookup fails.
        /// </param>
        public dynamic GetNetCacheService(string serviceName, bool retryWithoutCacheIfNotFound = false)
        {
            var netCacheValues = GetService("NetCache", retryWithoutCacheIfNotFound)?["m_netCache"]?["valueSlots"];
            if (netCacheValues == null)
            {
                return null;
            }

            foreach (var netCache in netCacheValues)
            {
                var name = netCache?.TypeDefinition.Name;
                if (name == serviceName)
                {
                    return netCache;
                }
            }

            return null;
        }

        private void InvalidateCache()
        {
            cachedServiceItems = null;
            this.serviceSlotHints.Clear();
        }

        private dynamic FindServiceInItems(dynamic serviceItems, string name)
        {
            if (serviceItems == null)
            {
                return null;
            }

            int length = serviceItems.Length;

            // Fast path: re-validate the remembered slot for this service against live memory. Because the
            // service type name is re-read and compared here, and the service itself is read live below, a
            // successful hit is always fresh; a miss (rehash/resize/reclaim) just falls through to the scan.
            if (this.serviceSlotHints.TryGetValue(name, out var hintSlot) && hintSlot >= 0 && hintSlot < length)
            {
                try
                {
                    var hinted = serviceItems[hintSlot];
                    var hintedName = hinted?["value"]?["<ServiceTypeName>k__BackingField"];
                    if (hintedName == name)
                    {
                        return hinted["value"]["<Service>k__BackingField"];
                    }
                }
                catch (Exception)
                {
                    this.serviceSlotHints.Remove(name);
                }
            }

            for (int i = 0; i < length; i++)
            {
                var service = serviceItems[i];
                var serviceName = service?["value"]?["<ServiceTypeName>k__BackingField"];
                if (serviceName == name)
                {
                    this.serviceSlotHints[name] = i;
                    return service["value"]["<Service>k__BackingField"];
                }
            }

            return null;
        }

        private dynamic ResolveServiceItems(bool forceRefresh = false)
        {
            //if (!forceRefresh && cachedServiceItems != null && (DateTime.UtcNow - serviceItemsCachedAt) < CacheTtl)
            //{
            //    return cachedServiceItems;
            //}

            cachedServiceItems = null;

            var dependencyBuilders = image["Hearthstone.HearthstoneJobs"]?["s_dependencyBuilder"]?["_items"];
            if (dependencyBuilders == null)
            {
                return null;
            }

            var serviceLocator = dependencyBuilders[0]?["m_serviceLocator"];
            if (serviceLocator == null)
            {
                return null;
            }

            var services = serviceLocator["m_services"];
            if (services == null)
            {
                return null;
            }

            cachedServiceItems = services["_entries"];
            serviceItemsCachedAt = DateTime.UtcNow;
            return cachedServiceItems;
        }
    }
}