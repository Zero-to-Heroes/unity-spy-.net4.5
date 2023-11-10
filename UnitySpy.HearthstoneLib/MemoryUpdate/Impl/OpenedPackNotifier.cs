using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class OpenedPackNotifier
    {
        private IPackInfo lastOpenedPack;
        private List<PackInfo> lastMassOpenedPacks;

        private bool sentExceptionMessage = false;

        internal void HandleOpenedPack(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var openedPack = mindVision.GetOpenedPack();
                if (openedPack != null && !openedPack.Equals(lastOpenedPack))
                {
                    result.HasUpdates = true;
                    result.OpenedPack = openedPack;
                    lastOpenedPack = openedPack;
                }

                var massOpenedPacks = mindVision.GetMassOpenedPack();
                if (massOpenedPacks != null && massOpenedPacks.Count > 0 && !SameMassOpenedPacks(massOpenedPacks, lastMassOpenedPacks))
                {
                    result.HasUpdates = true;
                    result.MassOpenedPacks = massOpenedPacks;
                    lastMassOpenedPacks = massOpenedPacks;
                }
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

        private bool SameMassOpenedPacks(List<PackInfo> massOpenedPacks, List<PackInfo> lastMassOpenedPacks)
        {
            if (massOpenedPacks == null || lastMassOpenedPacks == null)
            {
                return false;
            }
            if (massOpenedPacks.Count != lastMassOpenedPacks.Count)
            {
                return false;
            }
            for (var i = 0; i < massOpenedPacks.Count; i++)
            {
                if (!massOpenedPacks[i].Equals(lastMassOpenedPacks[i]))
                {

                    return false;
                }
            }
            return true;
        }
    }
}