using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class SelectedDeckNotifier
    {
        private long? previousSelectedDeckId;
        private bool isInit;

        internal void HandleSelectedDeck(MindVision mindVision, IMemoryUpdate result)
        {
            var selectedDeckId = mindVision.GetSelectedDeckId();
            if (selectedDeckId != null && selectedDeckId != 0 && selectedDeckId != previousSelectedDeckId && isInit)
            {
                result.HasUpdates = true;
                result.SelectedDeckId = selectedDeckId;
                previousSelectedDeckId = selectedDeckId;
            }
            isInit = true;
        }
    }
}