﻿using System;
using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail
{
    internal class HearthstoneImage
    {
        private readonly IAssemblyImage image;

        //private dynamic netCache;
        //private dynamic serviceItems;

        public HearthstoneImage(IAssemblyImage image)
        {
            this.image = image;
        }

        public dynamic this[string fullTypeName] => this.image[fullTypeName];

        public IEnumerable<ITypeDefinition> TypeDefinitions => this.image.TypeDefinitions;

        public dynamic GetService(string name)
        {
            try
            {
                //if (this.serviceItems == null)
                //{
                    // HearthstoneServices disappeared in 23.4, andI haven't found a better solution yet
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

                    var serviceItems = services["_entries"];
                    if (serviceItems == null)
                    {
                        return null;
                    }
                    //this.serviceItems = serviceItems;
                //}

                var i = 0;
                foreach (var service in serviceItems)
                {
                    var serviceName = service?["value"]?["<ServiceTypeName>k__BackingField"];
                    if (serviceName == name)
                    {
                        var result = service["value"]["<Service>k__BackingField"];
                        return result;
                    }

                    i++;
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return null;
        }

        public dynamic GetNetCacheService(string serviceName)
        {

            var netCacheValues = GetService("NetCache")?["m_netCache"]?["valueSlots"];
            if (netCacheValues == null)
            {
                return null;
            }

            //this.netCache = netCacheValues;
            var i = 0;
            foreach (var netCache in netCacheValues)
            {
                var name = netCache?.TypeDefinition.Name;
                if (name == serviceName)
                {
                    return netCache;
                }
                i++;
            }

            return null;
        }
    }
}