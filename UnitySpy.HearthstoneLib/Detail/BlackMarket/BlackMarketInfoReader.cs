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
            string dailyEarnCapText = null;
            if (dataModel != null)
            {
                dailyEarn = dataModel["m_DailyEarn"] ?? 0;
                dailyEarnCap = dataModel["m_DailyEarnCap"] ?? 0;
                dailyEarnCapText = dataModel["m_DailyEarnCapText"];
            }

            if (dailyEarnCap <= 0)
            {
                dailyEarnCap = currentEvent["m_dailyEarnCap"] ?? 0;
            }

            var lastMatchEarned = 0;
            try
            {
                var hasReward = service["m_hasBmecReward"] ?? false;
                var bmec = service["m_bmecRewardData"];
                if (hasReward && bmec != null)
                {
                    lastMatchEarned = bmec["TotalEarnedAmount"] ?? 0;
                }
            }
            catch (Exception)
            {
                // Last-match payout is diagnostic only.
            }

            return new BlackMarketInfo
            {
                DailyEarn = dailyEarn,
                DailyEarnCap = dailyEarnCap,
                DailyEarnCapText = dailyEarnCapText,
                IsFetchingPlayerState = service["m_isFetchingPlayerState"] ?? false,
                LastMatchEarned = lastMatchEarned,
                EventId = currentEvent["m_ID"],
                IsAccessible = service["m_isBlackMarketAccessible"],
            };
        }
    }
}
