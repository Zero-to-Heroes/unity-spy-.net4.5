namespace HackF5.UnitySpy.HearthstoneLib.Detail.Collection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using JetBrains.Annotations;

    internal static class CollectionBattlegroundsHeroSkinsReader
    {
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
            return image.GetNetCacheService("NetCacheBattlegroundsHeroSkins")?["<OwnedBattlegroundsSkins>k__BackingField"]?["_count"] ?? 0;
        }
    }
}