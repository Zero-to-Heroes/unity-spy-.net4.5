namespace HackF5.UnitySpy.HearthstoneLib.Detail.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class CollectionCardBackReader
    {
        public static int ReadCollectionSize([NotNull] HearthstoneImage image)
        {
            return image.GetNetCacheService("NetCacheCardBacks")?["<CardBacks>k__BackingField"]?["_count"] ?? 0;
        }

        public static IReadOnlyList<ICollectionCardBack> ReadCollection([NotNull] HearthstoneImage image)
        {
            //Logger.Log("Getting card backs");
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectionCardBacks = new List<CollectionCardBack>();
            var cardBacks = image.GetNetCacheService("NetCacheCardBacks");

            var slots = cardBacks["<CardBacks>k__BackingField"]["_slots"];
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

            return collectionCardBacks;
        }
    }
}