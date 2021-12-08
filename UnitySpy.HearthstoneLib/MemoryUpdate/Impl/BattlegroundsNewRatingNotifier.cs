using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class BattlegroundsNewRatingNotifier
    {
        private int? lastNewRating = null;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            var newRating = mindVision.GetBattlegroundsNewRating();
            if (lastNewRating != null && newRating != -1 && lastNewRating != newRating)
            {
                result.BattlegroundsNewRating = newRating;
                result.HasUpdates = true;
            }
            lastNewRating = newRating;
        }
    }
}