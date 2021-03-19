namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class OpenedPackNotifier
    {
        private IPackInfo lastOpenedPack;
        private bool isInit;

        internal void HandleOpenedPack(MindVision mindVision, IMemoryUpdate result)
        {
            var openedPack = mindVision.GetOpenedPack();
            if (openedPack != null && !openedPack.Equals(lastOpenedPack) && isInit)
            {
                result.HasUpdates = true;
                result.OpenedPack = openedPack;
                lastOpenedPack = openedPack;
            }
            isInit = true;
        }
    }
}