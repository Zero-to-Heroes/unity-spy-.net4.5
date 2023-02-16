using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class MercenariesPendingTreasureSelectionNotifier
    {
        private int? lastTreasureSelectionIndex = null;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                int? treasureSelectionIndex = mindVision.GetMercenariesIsSelectingTreasures();
                if (treasureSelectionIndex != null && (lastTreasureSelectionIndex == null || treasureSelectionIndex.Value != lastTreasureSelectionIndex.Value))
                {
                    var selection = mindVision.GetMercenariesPendingTreasureSelection(treasureSelectionIndex.Value);
                    Logger.Log($"Getting mercs treasure selection for treasure {treasureSelectionIndex}");
                    result.HasUpdates = true;
                    result.MercenariesTreasureSelectionIndex = treasureSelectionIndex;
                    result.MercenariesPendingTreasureSelection = selection;
                    lastTreasureSelectionIndex = treasureSelectionIndex;
                }
                else if (lastTreasureSelectionIndex != null && treasureSelectionIndex == null)
                {
                    result.HasUpdates = true;
                    result.MercenariesTreasureSelectionIndex = -1;
                    lastTreasureSelectionIndex = null;
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