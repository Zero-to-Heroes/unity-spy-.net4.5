﻿using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class FriendsListOpenedNotifier
    {
        private bool? lastIsOpen;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
        {
            try
            {
                var isOpen = mindVision.IsFriendsListOpen();
                if (!isOpen && (lastIsOpen == null || lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isFriendsListOpen = false;
                    lastIsOpen = false;
                }
                else if (isOpen && (lastIsOpen == null || !lastIsOpen.Value))
                {
                    result.HasUpdates = true;
                    result.isFriendsListOpen = true;
                    lastIsOpen = true;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in FriendsListOpenedNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}