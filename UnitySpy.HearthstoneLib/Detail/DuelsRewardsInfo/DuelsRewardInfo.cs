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

        public override bool Equals(object obj)
        {
            return obj is DuelsRewardInfo info &&
                   Type == info.Type &&
                   Amount == info.Amount &&
                   BoosterId == info.BoosterId;
        }

        public override int GetHashCode()
        {
            int hashCode = -2049393372;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Amount.GetHashCode();
            hashCode = hashCode * -1521134295 + BoosterId.GetHashCode();
            return hashCode;
        }
    }
}
