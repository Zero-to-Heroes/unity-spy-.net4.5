using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsMainRunScreenNotifier
    {
        private bool? lastIsOnMainScreen;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return;
            }

            try
            {
                var isOnMainScreen = mindVision.GetDuelsIsOnMainScreen();
                if (!isOnMainScreen && (lastIsOnMainScreen == null || lastIsOnMainScreen.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsMainRunScreen = false;
                    lastIsOnMainScreen = false;
                }
                else if (isOnMainScreen && (lastIsOnMainScreen == null || !lastIsOnMainScreen.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsMainRunScreen = true;
                    lastIsOnMainScreen = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsMainRunScreenNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}