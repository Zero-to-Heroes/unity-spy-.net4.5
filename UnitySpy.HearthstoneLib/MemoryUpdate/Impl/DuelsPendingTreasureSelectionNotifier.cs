using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsPendingTreasureSelectionNotifier
    {
        private bool lastHasSelection;
        private IDuelsPendingTreasureSelection lastSelection;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var isOnMainScreen = mindVision.GetDuelsIsOnMainScreen();
                var selection = isOnMainScreen ? mindVision.GetDuelsPendingTreasureSelection() : null;
                var hasSelection = (selection?.Options?.Count ?? 0) > 0;
                if (hasSelection && !lastHasSelection && (lastSelection == null || !lastSelection.Equals(selection)))
                {
                    result.HasUpdates = true;
                    result.IsDuelsSelectingTreasure = true;
                    result.DuelsPendingTreasureSelection = selection;
                    lastHasSelection = true;
                    lastSelection = selection;
                }
                else if (!hasSelection && lastHasSelection)
                {
                    result.HasUpdates = true;
                    result.IsDuelsSelectingTreasure = false;
                    result.DuelsPendingTreasureSelection = null;
                    lastHasSelection = false;
                    lastSelection = null;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsPendingTreasureSelectionNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }

        }
    }
}