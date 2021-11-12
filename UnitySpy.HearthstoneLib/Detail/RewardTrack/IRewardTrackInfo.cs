namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IRewardTrackInfo
    {
        int Level { get; }

        int Xp { get; }

        int XpNeeded { get; }

        int XpBonusPercent { get; }
    }
}
