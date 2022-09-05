using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack
{
    internal class RewardTrackInfos : IRewardTrackInfos
    {
        public IReadOnlyList<IRewardTrackInfo> TrackEntries { get; set; }
    }

    internal class RewardTrackInfo : IRewardTrackInfo { 
        public int TrackType { get; set; }

        public int Level { get; set; }

        public int Xp { get; set; }

        public int XpNeeded { get; set; }

        public int XpBonusPercent { get; set; }

    }
}
