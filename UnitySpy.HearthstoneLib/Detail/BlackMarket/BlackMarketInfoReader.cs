namespace HackF5.UnitySpy.HearthstoneLib.Detail.BlackMarket
{
    using System;
    using JetBrains.Annotations;

    internal static class BlackMarketInfoReader
    {
        public static BlackMarketInfo Read([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var service = image.GetService("Hearthstone.BlackMarket.BlackMarketEventManager");
            if (service == null)
            {
                return null;
            }

            var currentEvent = service["m_currentBlackMarketEvent"];
            if (currentEvent == null)
            {
                return null;
            }

            var dataModel = service["m_blackMarketDataModel"];
            var dailyEarn = 0;
            var dailyEarnCap = 0;
            if (dataModel != null)
            {
                dailyEarn = dataModel["m_DailyEarn"];
                dailyEarnCap = dataModel["m_DailyEarnCap"];
            }
            else
            {
                dailyEarnCap = currentEvent["m_dailyEarnCap"];
            }

            return new BlackMarketInfo
            {
                DailyEarn = dailyEarn,
                DailyEarnCap = dailyEarnCap,
                EventId = currentEvent["m_ID"],
                IsAccessible = service["m_isBlackMarketAccessible"],
            };
        }
    }
}
