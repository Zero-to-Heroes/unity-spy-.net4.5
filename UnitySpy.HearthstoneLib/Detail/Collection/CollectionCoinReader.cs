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
            var collectionCoins = new List<CollectionCoin>();

            var coinDbf = image["GameDbf"]["CosmeticCoin"]["m_records"];
            var _size = coinDbf["_size"];
            var _items = coinDbf["_items"];
            var coinDic = new Dictionary<int, int>();
            for (int i = 0; i < _size; i++)
            {
                var coin = _items[i];
                coinDic.Add(coin["m_ID"], coin["m_cardId"]);
            }

            var cardBacks = image.GetNetCacheService("NetCacheCoins")["<Coins>k__BackingField"];
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


            return collectionCoins;
        }

        public static int ReadCollectionSize([NotNull] HearthstoneImage image)
        {
            return image.GetNetCacheService("NetCacheCoins")?["<Coins>k__BackingField"]?["_count"] ?? 0;
        }
    }
}