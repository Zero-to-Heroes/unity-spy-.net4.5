// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    using System;

    internal static class BattlegroundsInfoReader
    {
        public static IBattlegroundsInfo ReadBattlegroundsInfo(HearthstoneImage image)
        {
            image.GetService("GameMgr");
            var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];
            if (netCacheValues == null)
            {
                return null;
            }

            var battlegroundsInfo = new BattlegroundsInfo();
            foreach (var netCache in netCacheValues)
            {
                Console.WriteLine("" + netCache?.TypeDefinition.Name);
                if (netCache?.TypeDefinition.Name == "NetCacheBaconRatingInfo")
                {
                    battlegroundsInfo.Rating = netCache["<Rating>k__BackingField"] ?? -1;
                }
                else if (netCache?.TypeDefinition.Name == "NetCacheBaconPremiumStatus")
                {
                    Console.WriteLine(netCache);
                }
                //battlegroundsInfo.PreviousRating = netCache["<PreviousBaconRatingInfo>k__BackingField"]?["<Rating>k__BackingField"] ?? -1;
            }

            return battlegroundsInfo;
        }
    }
}