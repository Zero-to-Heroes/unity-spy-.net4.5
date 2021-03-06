﻿using System;

namespace HackF5.UnitySpy.HearthstoneLib.Detail
{
    internal class HearthstoneImage
    {
        private readonly IAssemblyImage image;

        public HearthstoneImage(IAssemblyImage image)
        {
            this.image = image;
        }

        public dynamic this[string fullTypeName] => this.image[fullTypeName];

        public dynamic GetService(string name)
        {
            try
            {

                var dynamicServices = this.image?["HearthstoneServices"]["s_dynamicServices"];
                var staticServices = this.image?["HearthstoneServices"]["s_runtimeServices"];
                var services = dynamicServices ?? staticServices;

                if (services == null)
                {
                    return null;
                }

                var serviceItems = services["m_services"]["_items"];

                var i = 0;
                foreach (var service in serviceItems)
                {
                    if (service?["<ServiceTypeName>k__BackingField"] == name)
                    {
                        var result = service["<Service>k__BackingField"];
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
    }
}