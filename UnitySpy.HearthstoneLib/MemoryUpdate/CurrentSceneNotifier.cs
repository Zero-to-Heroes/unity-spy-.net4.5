namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class CurrentSceneNotifier
    {
        private SceneModeEnum lastScene;
        private bool isInit;

        internal void HandleSceneMode(MindVision mindVision, IMemoryUpdate result)
        {
            var scene = mindVision.GetSceneMode();
            if (scene != SceneModeEnum.INVALID && scene != 0 && scene != lastScene && isInit)
            {
                result.HasUpdates = true;
                result.CurrentScene = scene;
            }
            lastScene = scene;
            isInit = true;
        }
    }
}