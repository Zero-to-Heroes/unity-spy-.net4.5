using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;
using System;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class GameMenuOpenedNotifier
    {
        private bool? lastIsOpen;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            try
            {
                var isOpen = mindVision.IsGameMenuOpen();
                if (!isOpen && (lastIsOpen == null || lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isGameMenuOpen = false;
                    lastIsOpen = false;
                }
                else if (isOpen && (lastIsOpen == null || !lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isGameMenuOpen = true;
                    lastIsOpen = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in GameMenuOpenedNotifier memory read " + e.Message + " " + e.StackTrace);
                }
            }
        }
    }
}
