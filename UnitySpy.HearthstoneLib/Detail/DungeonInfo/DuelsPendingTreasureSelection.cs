using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    internal class DuelsPendingTreasureSelection : IDuelsPendingTreasureSelection
    {
        public IReadOnlyList<int> Options { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DuelsPendingTreasureSelection other
                && this.Options.Count == other.Options.Count
                && this.Options.All(other.Options.Contains);
        }
    }
}