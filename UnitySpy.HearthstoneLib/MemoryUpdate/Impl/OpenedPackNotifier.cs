using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;
using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class OpenedPackNotifier
    {
        private List<PackInfo> lastOpenedPacks;
        private List<PackInfo> lastMassOpenedPacks;

        private bool sentExceptionMessage = false;

        internal void HandleOpenedPack(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene = null)
        {
            if (currentScene != null && currentScene != SceneModeEnum.PACKOPENING)
            {
                lastOpenedPacks = null;
                lastMassOpenedPacks = null;
                return;
            }

            try
            {
                var openedPacks = mindVision.GetOpenedPacks();
                if (openedPacks != null && openedPacks.Count > 0 && !SamePacks(openedPacks, lastOpenedPacks))
                {
                    result.HasUpdates = true;
                    result.OpenedPacks = openedPacks;
                    lastOpenedPacks = openedPacks;
                    Logger.Log("OpenedPackNotifier published " + openedPacks.Count + " pack(s)");
                }

                var massOpenedPacks = mindVision.GetMassOpenedPack();
                if (massOpenedPacks != null && massOpenedPacks.Count > 0 && !SamePacks(massOpenedPacks, lastMassOpenedPacks))
                {
                    result.HasUpdates = true;
                    result.MassOpenedPacks = massOpenedPacks;
                    lastMassOpenedPacks = massOpenedPacks;
                    Logger.Log("OpenedPackNotifier published " + massOpenedPacks.Count + " mass pack(s)");
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

        private bool SamePacks(List<PackInfo> massOpenedPacks, List<PackInfo> lastMassOpenedPacks)
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