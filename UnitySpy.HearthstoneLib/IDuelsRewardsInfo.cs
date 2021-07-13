namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDuelsRewardsInfo
    {
        IReadOnlyList<IRewardInfo> Rewards { get; }
    }

    [PublicAPI]
    public interface IRewardInfo
    {
        int Type { get; }

        long Amount { get; }

        int BoosterId { get; }
    }

}
