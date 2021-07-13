namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardsInfo;
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

            var heroCardId = draftManager["m_draftDeck"]?["HeroCardID"];
            return new ArenaInfo
            {
                Wins = draftManager["m_wins"] ?? -1,
                Losses = draftManager["m_losses"] ?? -1,
                HeroCardId = heroCardId,
                Rewards = RewardsInfoReader.ParseRewards(draftManager["m_chest"]?["<Rewards>k__BackingField"]?["_items"])
            };
        }
    }
}
