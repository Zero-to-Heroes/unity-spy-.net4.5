using System;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class DuelsCardsInDeckChangeNotifier
    {
        private int? lastCardsInDeck;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var currentCards = mindVision.GetNumberOfCardsInDeck();
                if (lastCardsInDeck != currentCards && currentCards != null)
                {
                    result.HasUpdates = true;
                    result.DuelsCurrentCardsInDeck = currentCards.Value;
                    lastCardsInDeck = currentCards;
                }
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in DuelsCardsInDeckChangeNotifier memory read " + e.Message + " " + e.StackTrace);
                    //sentExceptionMessage = true;
                }
            }

        }
    }
}