namespace HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class OpenPacksInfoReader
    {
        public static IOpenPacksInfo ReadOpenPacksInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var packOpeningMgr = image["PackOpening"]?["s_instance"];
            if (packOpeningMgr == null)
            {
                return null;
            }

            var unopenedPacks = new List<IBoosterStack>();
            if (packOpeningMgr["m_unopenedPacks"] != null)
            {
                int numberOfStacks = packOpeningMgr["m_unopenedPacks"]?["count"] ?? 0;
                for (int i = 0; i < numberOfStacks; i++)
                {
                    var memUnopenedStack = packOpeningMgr["m_unopenedPacks"]["valueSlots"][i];
                    unopenedPacks.Add(new BoosterStack
                    {
                        BoosterId = memUnopenedStack["m_boosterStack"]["<Id>k__BackingField"],
                        Count = memUnopenedStack["m_boosterStack"]["<Count>k__BackingField"],
                        EverGrantedCount = memUnopenedStack["m_boosterStack"]["<EverGrantedCount>k__BackingField"],
                    });
                }
            }

            IPackOpening packOpening = null;
            if (packOpeningMgr["m_director"] != null)
            {
                var director = packOpeningMgr["m_director"];
                var cardsPendingReveal = director?["m_cardsPendingReveal"] ?? 0;
                var cards = new List<IPackCard>();

                if (cardsPendingReveal > 0)
                {
                    var hiddenCards = director["m_hiddenCards"];
                    if (hiddenCards != null)
                    {
                        var numberOfCards = hiddenCards["_size"];
                        for (int i = 0; i < numberOfCards; i++)
                        {
                            var memCard = hiddenCards["_items"][i];
                            cards.Add(new PackCard
                            {
                                CardId = memCard["m_boosterCard"]["<Def>k__BackingField"]["<Name>k__BackingField"],
                                Premium = memCard["m_premium"] == 1,
                                IsNew = memCard["m_isNew"],
                                Revealed = memCard["m_revealed"],
                            });
                        }
                    }
                }
                packOpening = new PackOpening
                {
                    CardsPendingReveal = cardsPendingReveal,
                    Cards = cards
                };
            }

            return new OpenPacksInfo
            {
                LastOpenedBoosterId = packOpeningMgr["m_lastOpenedBoosterId"],
                UnopenedPacks = unopenedPacks,
                PackOpening = packOpening,
            };
        }

        public static IPackInfo ReadOpenPackInfo([NotNull] HearthstoneImage image)
        {
            var openPacksInfo = OpenPacksInfoReader.ReadOpenPacksInfo(image);
            if (openPacksInfo?.PackOpening?.Cards == null)
            {
                return null ;
            }

            var cards = openPacksInfo.PackOpening.Cards;
            if (cards?.Count == 5)
            {
                return new PackInfo
                {
                    BoosterId = openPacksInfo.LastOpenedBoosterId,
                    Cards = cards
                        .Select(card => new CardInfo
                        {
                            CardId = card.CardId,
                            Premium = card.Premium,
                        } as ICardInfo)
                        .ToList(),
                };
            }

            return null;
        }
    }
}
