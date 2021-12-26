using System;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class CurrentSceneNotifier
    {
        private SceneModeEnum? lastScene;

        private bool sentExceptionMessage = false;

        internal void HandleSceneMode(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var scene = mindVision.GetSceneMode();
                if (scene != null && scene != SceneModeEnum.INVALID && scene != 0)
                {
                    if (scene != lastScene)
                    {
                        result.HasUpdates = true;
                        result.CurrentScene = scene;
                    }
                    lastScene = scene;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in CurrentSceneNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }
    }
}