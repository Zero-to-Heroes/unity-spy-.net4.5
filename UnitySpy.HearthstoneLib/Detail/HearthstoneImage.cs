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
                var found = FindServiceInItems(ResolveServiceItems(forceRefresh: false), name);
                if (found != null)
                {
                    return found;
                }

                if (retryWithoutCacheIfNotFound)
                {
                    return FindServiceInItems(ResolveServiceItems(forceRefresh: true), name);
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
        }

        private static dynamic FindServiceInItems(dynamic serviceItems, string name)
        {
            if (serviceItems == null)
            {
                return null;
            }

            foreach (var service in serviceItems)
            {
                var serviceName = service?["value"]?["<ServiceTypeName>k__BackingField"];
                if (serviceName == name)
                {
                    return service["value"]["<Service>k__BackingField"];
                }
            }

            return null;
        }

        private dynamic ResolveServiceItems(bool forceRefresh = false)
        {
            if (!forceRefresh && cachedServiceItems != null && (DateTime.UtcNow - serviceItemsCachedAt) < CacheTtl)
            {
                return cachedServiceItems;
            }

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