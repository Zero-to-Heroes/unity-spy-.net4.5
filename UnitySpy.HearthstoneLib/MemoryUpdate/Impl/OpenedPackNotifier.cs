using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class OpenedPackNotifier
    {
        private IPackInfo lastOpenedPack;
        private bool isInit;

        private bool sentExceptionMessage = false;

        internal void HandleOpenedPack(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var openedPack = mindVision.GetOpenedPack();
                if (openedPack != null && !openedPack.Equals(lastOpenedPack) && isInit)
                {
                    result.HasUpdates = true;
                    result.OpenedPack = openedPack;
                    lastOpenedPack = openedPack;
                }
                isInit = true;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in OpenedPackNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }
    }
}