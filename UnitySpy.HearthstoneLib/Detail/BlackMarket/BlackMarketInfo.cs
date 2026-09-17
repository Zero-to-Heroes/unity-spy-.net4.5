namespace HackF5.UnitySpy.HearthstoneLib.Detail.BlackMarket
{
    public class BlackMarketInfo
    {
        public int DailyEarn { get; set; }

        /// <summary>
        /// Daily earn cap from the active Black Market event. A value of 0 or less means unlimited.
        /// </summary>
        public int DailyEarnCap { get; set; }

        public int EventId { get; set; }

        public bool IsAccessible { get; set; }
    }
}
