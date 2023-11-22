namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo;
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

            var heroCardId = draftManager["m_draftDeck"]?["<HeroCardID>k__BackingField"];
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
            var items = slots["_items"];
            for (var i = 0; i < size; i++)
            {
                var item = items[i];
                var cardId = item["m_cardId"];
                // Count is stored separately for normal + golden + diamond
                var cardCount = 0;
                var count = item["m_count"];
                var countSize = count["_size"];
                var countItems = count["_items"];
                for (var j = 0; j < countSize; j++)
                {
                    cardCount += countItems[j];
                }
                for (var j = 0; j < cardCount; j++)
                {
                    decklist.Add(cardId);
                }
            }

            var accountInfo = AccountInfoReader.ReadAccountInfo(image);
            var deckId = $"{accountInfo.Hi}-{accountInfo.Lo}-{draftDeck["ID"]}";
            return new Deck()
            {
                Id = deckId,
                DeckId = -1,
                DeckList = decklist,
                FormatType = draftDeck["<FormatType>k__BackingField"],
                HeroCardId = draftDeck["<HeroCardID>k__BackingField"],
                Name = null,
            };
        }
    }
}
