using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;
using System;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class HistoryInspectOpenedNotifier
    {
        private bool? lastIsOpen;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            try
            {
                var isOpen = mindVision.IsHistoryInspectOpen();
                if (!isOpen && (lastIsOpen == null || lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isHistoryInspectOpen = false;
                    lastIsOpen = false;
                }
                else if (isOpen && (lastIsOpen == null || !lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isHistoryInspectOpen = true;
                    lastIsOpen = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in HistoryInspectOpenedNotifier memory read " + e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}
