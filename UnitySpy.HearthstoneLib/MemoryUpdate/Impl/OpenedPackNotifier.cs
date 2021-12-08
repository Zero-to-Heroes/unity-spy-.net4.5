using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class OpenedPackNotifier
    {
        private IPackInfo lastOpenedPack;
        private bool isInit;

        internal void HandleOpenedPack(MindVision mindVision, IMemoryUpdate result)
        {
            var openedPack = mindVision.GetOpenedPack();
            //Logger.Log("Found opened pack " + openedPack?.BoosterId + " and last " + lastOpenedPack?.BoosterId + ", isInit " + isInit);
            //Logger.Log("Old pack card Ids " + lastOpenedPack?.Cards?.Select(c => c.CardId).ToArray());
            //Logger.Log("New pack card Ids " + openedPack?.Cards?.Select(c => c.CardId).ToArray());
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