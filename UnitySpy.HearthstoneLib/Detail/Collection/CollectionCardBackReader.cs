namespace HackF5.UnitySpy.HearthstoneLib.Detail.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class CollectionCardBackReader
    {
        public static IReadOnlyList<ICollectionCardBack> ReadCollection([NotNull] HearthstoneImage image)
        {
            //Logger.Log("Getting card backs");
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectionCardBacks = new List<CollectionCardBack>();
            
            var netCache = image.GetService("NetCache");
            if (netCache == null 
                || netCache["m_netCache"] == null
                || netCache["m_netCache"]["valueSlots"] == null)
            {
                Logger.Log("Empty card backs");
                return collectionCardBacks;
            }

            var netCacheValues = netCache["m_netCache"]["valueSlots"];
            foreach (var value in netCacheValues)
            {
                // Last time, was i == 12
                if (value?.TypeDefinition?.Name == "NetCacheCardBacks")
                {
                    var cardBacks = value["<CardBacks>k__BackingField"];
                    var slots = cardBacks["_slots"];
                    for (var i = 0; i < slots.Length; i++)
                    {
                        var cardBack = slots[i];
                        var cardBackId = cardBack["value"];
                        //Logger.Log("Card back id " + cardBackId);
                        collectionCardBacks.Add(new CollectionCardBack()
                        {
                            CardBackId = cardBackId,
                        });
                    }
                }
            }

            return collectionCardBacks;
        }
    }
}