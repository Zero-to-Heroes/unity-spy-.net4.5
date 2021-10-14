namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
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
                Deck = ReadArenaDeck(image),
                Rewards = RewardsInfoReader.ParseRewards(draftManager["m_chest"]?["<Rewards>k__BackingField"]?["_items"])
            };
        }

        public static IDeck ReadArenaDeck([NotNull] HearthstoneImage image)
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

            List<string> decklist = new List<string>();
            var draftDeck = draftManager["m_draftDeck"];
            if (draftDeck == null)
            {
                return null;
            }

            var slots = draftDeck["m_slots"];
            var size = slots["_size"];
            for (var i = 0; i < size; i++)
            {
                var slot = slots["_items"][i];
                var cardId = slot["m_cardId"];
                // Normal + golden + diamond, I imagine
                var count = slot["m_count"]["_items"][0] + slot["m_count"]["_items"][1] + slot["m_count"]["_items"][2];
                for (var j = 0; j < count; j++)
                {
                    decklist.Add(cardId);
                }
            }
            return new Deck()
            {
                Id = draftDeck["ID"],
                DeckId = -1,
                DeckList = decklist as IReadOnlyList<string>,
                FormatType = draftDeck["<FormatType>k__BackingField"],
                HeroCardId = draftDeck["HeroCardID"],
                Name = null,
            };
        }
    }
}
