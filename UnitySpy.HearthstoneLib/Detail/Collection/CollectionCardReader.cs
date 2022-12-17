namespace HackF5.UnitySpy.HearthstoneLib.Detail.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class CollectionCardReader
    {
        public static IReadOnlyList<ICollectionCard> ReadCollection([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectionCards = new Dictionary<string, CollectionCard>();
            var collectibleCards = image["CollectionManager"]?["s_instance"]?["m_collectibleCards"];

            if (collectibleCards == null)
            {
                return collectionCards.Values.ToArray();
            }

            var items = collectibleCards["_items"];
            int size = collectibleCards["_size"];
            if (items == null)
            {
                Logger.Log("items are null in ReadCollection");
                return collectionCards.Values.ToArray();
            }

            for (var index = 0; index < size; index++)
            {
                string cardId = items[index]["m_EntityDef"]["m_cardIdInternal"];
                if (string.IsNullOrEmpty(cardId))
                {
                    continue;
                }

                int count = items[index]["<OwnedCount>k__BackingField"];
                int premium = items[index]["m_PremiumType"];

                if (!collectionCards.TryGetValue(cardId, out var card))
                {
                    card = new CollectionCard { CardId = cardId };
                    collectionCards.Add(cardId, card);
                }

                if (premium == 1)
                {
                    card.PremiumCount = count;
                }
                else if (premium == 2)
                {
                    card.DiamondCount = count;
                }
                else if (premium == 3)
                {
                    card.SignatureCount = count;
                }
                else if (premium == 4)
                {
                    card.MaxCount = count;
                }
                else
                {
                    // So that if other "premium" types are introduced, we don't override the base value
                    card.Count += count;
                }
            }

            return collectionCards.Values.ToArray();
        }

        public static IReadOnlyList<int> ReadBattlegroundsHeroSkins([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectionCards = new List<int>();
            var heroSkinIdToCardDbfId = new Dictionary<int, int>();
            var mappingObj = image["CollectionManager"]?["s_instance"]?["m_BattlegroundsHeroSkinIdToHeroSkinCardId"];
            if (mappingObj == null)
            {
                return collectionCards;
            }

            var mappingCount = mappingObj["count"];
            for (var i = 0; i < mappingCount; i++)
            {
                var skinId = mappingObj["keySlots"][i]["m_value"];
                var cardDbfId = mappingObj["valueSlots"][i];
                heroSkinIdToCardDbfId.Add(skinId, cardDbfId);
            }

            var skinService = image.GetNetCacheService("NetCacheBattlegroundsHeroSkins")?["<OwnedBattlegroundsSkins>k__BackingField"];
            if (skinService == null)
            {
                return collectionCards;
            }
            var skinCount = skinService["_count"];
            var ownedSkinIds = new List<int>();
            try
            {
                // Not sure when this happens, but we don't want to break the whole memory reading just for that
                for (var i = 0; i < skinCount; i++)
                {
                    var skinId = skinService["_slots"][i]["value"]?["m_value"];
                    ownedSkinIds.Add(skinId);
                }
            }
            catch (Exception e)
            {
                Logger.Log("Exception while getting BG hero skins info");
            }

            foreach (var ownedSkinId in ownedSkinIds)
            {
                collectionCards.Add(heroSkinIdToCardDbfId[ownedSkinId]);
            }


            return collectionCards;
        }

        public static int ReadCollectionSize([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var collectibleCards = image["CollectionManager"]["s_instance"]["m_collectibleCards"];
            var items = collectibleCards["_items"];
            int size = collectibleCards["_size"];
            var totalCards = 0;
            for (var index = 0; index < size; index++)
            {
                string cardId = items[index]["m_EntityDef"]["m_cardIdInternal"];
                if (string.IsNullOrEmpty(cardId))
                {
                    continue;
                }

                int count = items[index]["<OwnedCount>k__BackingField"] ?? 0;
                int premium = items[index]["m_PremiumType"] ?? 0;
                totalCards += count;
                totalCards += premium;
            }
            return totalCards;
        }

        public static IReadOnlyList<IDustInfoCard> ReadDustInfoCards([NotNull] HearthstoneImage image)
        {
            //Logger.Log("Getting collection");
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var service = image.GetNetCacheService("NetCacheCardValues")?["<Values>k__BackingField"];
            if (service == null)
            {
                return new List<IDustInfoCard>();
            }

            var count = service["count"];
            var keys = service["keySlots"];
            var values = service["valueSlots"];

            if (count == 0)
            {
                return new List<IDustInfoCard>();
            }

            var result = new List<IDustInfoCard>();
            for (int i = 0; i < count; i++)
            {
                var key = keys[i];
                var value = values[i];
                result.Add(new DustInfoCard()
                {
                    CardId = key["<Name>k__BackingField"],
                    Premium = key["<Premium>k__BackingField"],
                    BaseBuyValue = value["<BaseBuyValue>k__BackingField"],
                    BaseSellValue = value["<BaseSellValue>k__BackingField"],
                    OverrideEvent = value["<OverrideEvent>k__BackingField"],
                    BuyValueOverride = value["<BuyValueOverride>k__BackingField"],
                    SellValueOverride = value["<SellValueOverride>k__BackingField"],
                });
            }

            return result;
        }
    }
}