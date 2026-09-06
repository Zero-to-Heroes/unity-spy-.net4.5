namespace HackF5.UnitySpy.HearthstoneLib.Detail.HistoryInspect
{
    using System;
    using JetBrains.Annotations;

    internal static class HistoryInspectReader
    {
        public static bool ReadHistoryInspectOpen([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            try
            {
                var instance = image["HistoryManager"]?["s_instance"];
                if (instance == null)
                {
                    return false;
                }

                // Hovering a history tile sets m_currentlyMousedOverTile (HistoryCard).
                // Do not use m_showingBigCard — that is also true during play-from-hand
                // big-card animations, which are not "inspect a card in the history".
                var mousedOverTile = instance["m_currentlyMousedOverTile"];
                return mousedOverTile != null;
            }
            catch (Exception)
            {
                // Don't log the error to avoid triggering memory reading resets
                return false;
            }
        }
    }
}
