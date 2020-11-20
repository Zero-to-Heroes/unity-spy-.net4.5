namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDuelsRewardsInfo
    {
        IReadOnlyList<IDuelsRewardInfo> Rewards { get; }
    }

    [PublicAPI]
    public interface IDuelsRewardInfo
    {
        int Type { get; }

        long Amount { get; }

        int BoosterId { get; }
    }

}
