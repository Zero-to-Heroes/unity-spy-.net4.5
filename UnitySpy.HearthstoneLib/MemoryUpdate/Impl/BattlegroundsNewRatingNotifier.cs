using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class BattlegroundsNewRatingNotifier
    {
        private int? lastNewRating = null;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var currentScene = mindVision.GetSceneMode();
                var newRating = currentScene == SceneModeEnum.GAMEPLAY ? mindVision.GetBattlegroundsNewRating() : -1;
                if (lastNewRating != null && newRating != -1 && lastNewRating != newRating)
                {
                    result.BattlegroundsNewRating = newRating;
                    result.HasUpdates = true;
                }
                lastNewRating = newRating;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in BattlegroundsNewRatingNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }
    }
}