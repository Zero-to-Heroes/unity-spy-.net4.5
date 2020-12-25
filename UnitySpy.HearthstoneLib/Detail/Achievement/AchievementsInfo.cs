namespace HackF5.UnitySpy.HearthstoneLib.Detail.Achievement
{
    using System.Collections.Generic;
    
    internal class AchievementsInfo : IAchievementsInfo
    {
        public IReadOnlyList<IAchievementInfo> Achievements { get; set; }
    }
}