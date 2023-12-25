using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class ArenaCurrentCardsInDeckNotifier
    {
        private int? lastCardsInDeck;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var currentCards = mindVision.GetNumberOfCardsInArenaDraftDeck();
                if (lastCardsInDeck != currentCards && currentCards != null)
                {
                    result.HasUpdates = true;
                    result.ArenaCurrentCardsInDeck = currentCards.Value;
                    lastCardsInDeck = currentCards;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in ArenaCurrentCardsInDeckNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}