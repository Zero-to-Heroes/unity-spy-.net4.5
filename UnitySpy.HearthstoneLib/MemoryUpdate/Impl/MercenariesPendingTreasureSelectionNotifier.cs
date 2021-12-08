using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class MercenariesPendingTreasureSelectionNotifier
    {
        private bool lastSelectingTreasure;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            var isSelectingTreasure = mindVision.GetMercenariesIsSelectingTreasures();

            if (lastSelectingTreasure && !isSelectingTreasure)
            {
                result.HasUpdates = true;
                result.IsMercenariesSelectingTreasure = false;
                lastSelectingTreasure = false;
            } 
            else if (isSelectingTreasure && !lastSelectingTreasure)
            {
                var selection = mindVision.GetMercenariesPendingTreasureSelection();
                result.HasUpdates = true;
                result.IsMercenariesSelectingTreasure = true;
                result.MercenariesPendingTreasureSelection = selection;
                lastSelectingTreasure = true;
            }

        }
    }
}