namespace HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class BoostersInfoReader
    {
        public static IBoostersInfo ReadBoostersInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var boosterServices = image.GetNetCacheService("NetCacheBoosters");
            if (boosterServices == null || boosterServices["<BoosterStacks>k__BackingField"] == null)
            {
                return null;
            }

            var boosters = new List<IBoosterStack>();
            var itemCount = boosterServices["<BoosterStacks>k__BackingField"]["_size"];
            var items = boosterServices["<BoosterStacks>k__BackingField"]["_items"];
            for (int i = 0; i < itemCount; i++)
            {
                var booster = items[i];
                boosters.Add(new BoosterStack()
                {
                    BoosterId = booster["<Id>k__BackingField"],
                    Count = booster["<Count>k__BackingField"],
                    EverGrantedCount = booster["<EverGrantedCount>k__BackingField"],
                });
            }

            boosters.Sort((a, b) => a.BoosterId - b.BoosterId);
            return new BoostersInfo()
            {
                Boosters = boosters,
            };
        }
    }
}
