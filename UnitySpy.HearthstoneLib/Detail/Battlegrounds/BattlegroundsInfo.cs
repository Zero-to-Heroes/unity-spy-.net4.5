namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    internal class BattlegroundsInfo : IBattlegroundsInfo
    {
        public int Rating { get; set; } = -1;
        
        public int NewRating { get; set; } = -1;

        public IBattlegroundsGame Game { get; set; }
    }
}