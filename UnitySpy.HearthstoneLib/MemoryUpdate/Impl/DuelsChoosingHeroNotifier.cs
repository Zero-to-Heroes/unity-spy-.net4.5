using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsChoosingHeroNotifier
    {
        private bool lastChoosing;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.PVP_DUNGEON_RUN)
            {
                return;
            }

            try
            {
                var currentChoosing = mindVision.GetDuelsIsChoosingHero();
                if (lastChoosing != currentChoosing)
                {
                    result.HasUpdates = true;
                    result.IsDuelsChoosingHero = currentChoosing;
                    lastChoosing = currentChoosing;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsChoosingHeroNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}