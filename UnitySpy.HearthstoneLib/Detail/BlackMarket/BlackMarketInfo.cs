namespace HackF5.UnitySpy.HearthstoneLib.Detail.BlackMarket
{
    public class BlackMarketInfo
    {
        public int DailyEarn { get; set; }

        /// <summary>
        /// Daily earn cap from the active Black Market event. A value of 0 or less means unlimited.
        /// </summary>
        public int DailyEarnCap { get; set; }

        /// <summary>
        /// Shop UI string written from player-state DailyEarn (e.g. "100/900").
        /// </summary>
        public string DailyEarnCapText { get; set; }

        public bool IsFetchingPlayerState { get; set; }

        /// <summary>
        /// Last end-of-game BMEC payout. Not the daily total.
        /// </summary>
        public int LastMatchEarned { get; set; }

        public int EventId { get; set; }

        public bool IsAccessible { get; set; }
    }
}
