namespace HackF5.UnitySpy.HearthstoneLib.Detail.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class CollectionCoinReader
    {
        public static IReadOnlyList<ICollectionCoin> ReadCollection([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectionCoins = new List<CollectionCoin>();
            
            var netCache = image.GetService("NetCache");
            if (netCache == null 
                || netCache["m_netCache"] == null
                || netCache["m_netCache"]["valueSlots"] == null)
            {
                return collectionCoins;
            }

            var coinDbf = image["GameDbf"]["Coin"]["m_records"];
            var _size = coinDbf["_size"];
            var _items = coinDbf["_items"];
            var coinDic = new Dictionary<int, int>();
            for (int i = 0; i < _size; i++)
            {
                var coin = _items[i];
                coinDic.Add(coin["m_ID"], coin["m_cardId"]);
            }

            var netCacheValues = netCache["m_netCache"]["valueSlots"];
            foreach (var value in netCacheValues)
            {
                if (value?.TypeDefinition?.Name == "NetCacheCoins")
                {
                    var cardBacks = value["<Coins>k__BackingField"];
                    var slots = cardBacks["_slots"];
                    for (var i = 0; i < slots.Length; i++)
                    {
                        var coin = slots[i];
                        var coinId = coin["value"];
                        if (coinId != 0)
                        {
                            collectionCoins.Add(new CollectionCoin()
                            {
                                CoinId = coinDic[coinId],
                            });
                        }
                    }
                }
            }


            return collectionCoins;
        }
    }
}