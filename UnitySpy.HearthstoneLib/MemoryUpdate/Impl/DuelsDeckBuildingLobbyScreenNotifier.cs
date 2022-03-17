using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsDeckBuildingLobbyScreenNotifier
    {
        private bool? lastIsOnDeckBuildingLobbyScreen;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var isOnDeckBuildingLobbyScreen = mindVision.GetDuelsIsOnDeckBuildingLobbyScreen();
                if (!isOnDeckBuildingLobbyScreen && (lastIsOnDeckBuildingLobbyScreen == null || lastIsOnDeckBuildingLobbyScreen.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsDeckBuildingLobbyScreen = false;
                    lastIsOnDeckBuildingLobbyScreen = false;
                }
                else if (isOnDeckBuildingLobbyScreen && (lastIsOnDeckBuildingLobbyScreen == null || !lastIsOnDeckBuildingLobbyScreen.Value))
                {
                    result.HasUpdates = true;
                    result.IsDuelsDeckBuildingLobbyScreen = true;
                    lastIsOnDeckBuildingLobbyScreen = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsDeckBuildingLobbyScreenNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}