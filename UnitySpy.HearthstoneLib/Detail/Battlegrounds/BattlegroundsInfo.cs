namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    internal class BattlegroundsInfo : IBattlegroundsInfo
    {
        public int Rating { get; set; }

        public IBattlegroundsGame Game { get; set; }
    }
}