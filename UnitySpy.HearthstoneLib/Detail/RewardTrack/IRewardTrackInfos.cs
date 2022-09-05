namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IRewardTrackInfos
    {
        IReadOnlyList<IRewardTrackInfo> TrackEntries { get; }
    }


    [PublicAPI]
    public interface IRewardTrackInfo
    {
        int TrackType { get; }

        int Level { get; }

        int Xp { get; }

        int XpNeeded { get; }

        int XpBonusPercent { get; }
    }
}
