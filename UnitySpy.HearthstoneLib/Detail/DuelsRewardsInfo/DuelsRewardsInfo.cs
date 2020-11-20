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
    }
}
