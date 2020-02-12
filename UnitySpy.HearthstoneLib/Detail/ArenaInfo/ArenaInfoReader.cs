namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class ArenaInfoReader
    {
        public static IArenaInfo ReadArenaInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var draftManager = image.GetService("DraftManager");
            if (draftManager == null)
            {
                return null;
            }

            return new ArenaInfo
            {
                Wins = draftManager["m_wins"],
                Losses = draftManager["m_losses"]
            };
        }
    }
}
