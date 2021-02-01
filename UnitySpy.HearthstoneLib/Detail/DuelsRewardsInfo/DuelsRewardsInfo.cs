using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo
{
    public class DuelsRewardsInfo : IDuelsRewardsInfo
    {
        public IReadOnlyList<IDuelsRewardInfo> Rewards { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DuelsRewardsInfo info &&
                    Rewards != null &&
                    info.Rewards != null && 
                    Rewards.All(info.Rewards.Contains) && Rewards.Count == info.Rewards.Count;
        }

        public override int GetHashCode()
        {
            return -1491387293 + EqualityComparer<IReadOnlyList<IDuelsRewardInfo>>.Default.GetHashCode(Rewards);
        }
    }
}
