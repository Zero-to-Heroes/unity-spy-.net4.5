using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class MercenariesPendingTreasureSelectionNotifier
    {
        private bool lastSelectingTreasure;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
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
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in MercenariesPendingTreasureSelectionNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }

        }
    }
}