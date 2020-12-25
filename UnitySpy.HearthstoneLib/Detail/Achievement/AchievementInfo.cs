namespace HackF5.UnitySpy.HearthstoneLib.Detail.Achievement
{
    using System.Collections.Generic;
    
    internal class AchievementInfo : IAchievementInfo
    {
        public int AchievementId { get; set; }

        public int Progress { get; set; }

        public int Status { get; set; }
    }
}