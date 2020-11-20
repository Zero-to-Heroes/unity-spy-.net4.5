using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.DuelsRewardsInfo
{
    public class DuelsRewardInfo : IDuelsRewardInfo
    {
        public int Type { get; set;  }

        public long Amount { get; set; }

        public int BoosterId { get; set; }
    }
}
