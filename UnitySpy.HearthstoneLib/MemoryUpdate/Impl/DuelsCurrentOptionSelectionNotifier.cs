using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsCurrentOptionSelectionNotifier
    {
        private int lastOption;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return;
            }

            try
            {
                var currentOption = mindVision.GetDuelsCurrentOptionSelection();
                if (lastOption != currentOption)
                {
                    result.HasUpdates = true;
                    result.DuelsCurrentOptionSelection = currentOption;
                    lastOption = currentOption;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsCurrentOptionSelectionNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}