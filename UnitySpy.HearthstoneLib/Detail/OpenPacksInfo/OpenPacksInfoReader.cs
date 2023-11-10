namespace HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class OpenPacksInfoReader
    {
        // m_draggedPack

        public static IOpenPacksInfo ReadOpenPacksInfo([NotNull] HearthstoneImage image)
        {
            if (image["PackOpening"] == null)
            {
                return null;
            }

            // For some reason I still get NPEs here
            try
            {
                var temp = image["PackOpening"]["s_instance"];
            }
            catch (Exception e)
            {
                return null;
            }

            var packOpeningMgr = image["PackOpening"]["s_instance"];
            if (packOpeningMgr == null)
            {
                return null;
            }

            var unopenedPacks = new List<IBoosterStack>();
            if (packOpeningMgr["m_unopenedPacks"] != null)
            {
                int numberOfStacks = packOpeningMgr["m_unopenedPacks"]["count"] ?? 0;
                for (int i = 0; i < numberOfStacks; i++)
                {
                    var memUnopenedStack = packOpeningMgr["m_unopenedPacks"]["valueSlots"][i];
                    unopenedPacks.Add(new BoosterStack
                    {
                        BoosterId = memUnopenedStack["m_boosterDbId"],
                        Count = memUnopenedStack["m_count"],
                        //EverGrantedCount = memUnopenedStack["m_boosterStack"]["<EverGrantedCount>k__BackingField"],
                    });
                }
            }

            IPackOpening packOpening = null;
            if (packOpeningMgr["m_director"] != null)
            {
                var director = packOpeningMgr["m_director"];
                var cardsPendingReveal = director["m_cardsPendingReveal"] ?? 0;
                var cards = new List<IPackCard>();

                if (cardsPendingReveal > 0)
                {
                    var hiddenCards = director["m_hiddenCards"]["m_cards"];
                    if (hiddenCards != null)
                    {
                        var numberOfCards = hiddenCards["_size"];
                        for (int i = 0; i < numberOfCards; i++)
                        {
                            var memCard = hiddenCards["_items"][i];
                            if (memCard == null)
                            {
                                return null;
                            }
                            var mercenaryPack = memCard["m_mercenaryPackComponent"];
                            if (mercenaryPack == null)
                            {
                                cards.Add(new PackCard()
                                {
                                    CardId = memCard["m_boosterCard"]?["<Def>k__BackingField"]?["<Name>k__BackingField"],
                                    Premium = memCard["m_premium"],
                                    IsNew = memCard["m_isNew"],
                                    Revealed = memCard["m_revealed"],
                                });
                            }
                            else
                            {
                                cards.Add(new PackCard()
                                {
                                    Premium = memCard["m_premium"],
                                    IsNew = memCard["m_isNew"],
                                    Revealed = memCard["m_revealed"],
                                    CurrencyAmount = mercenaryPack["_CurrencyAmount"],
                                    MercenaryArtVariationId = mercenaryPack["_MercenaryArtVariationId"],
                                    MercenaryArtVariationPremium = mercenaryPack["_MercenaryArtVariationPremium"],
                                    MercenaryId = mercenaryPack["_MercenaryId"],
                                });
                            }
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
            if (openPacksInfo?.PackOpening?.Cards == null
                || openPacksInfo.PackOpening.Cards.Any(card => card == null || (card.CardId == null && card.MercenaryId == -1)))
            {
                return null;
            }

            var cards = openPacksInfo.PackOpening.Cards;
            return new PackInfo
            {
                BoosterId = openPacksInfo.LastOpenedBoosterId,
                Cards = cards
                    .Select(card => new CardInfo
                    {
                        CardId = card.CardId,
                        Premium = card.Premium,
                        IsNew = card.IsNew,
                        CurrencyAmount = card.CurrencyAmount,
                        MercenaryArtVariationId = card.MercenaryArtVariationId,
                        MercenaryArtVariationPremium = card.MercenaryArtVariationPremium,
                        MercenaryId = card.MercenaryId,
                    } as ICardInfo)
                    .ToList(),
            };
        }

        public static List<PackInfo> ReadMassOpenPackInfo([NotNull] HearthstoneImage image)
        {
            var packOpeningMgr = image["PackOpening"]?["s_instance"];
            var massSummary = packOpeningMgr?["m_director"]?["m_massPackOpeningSummary"];
            if (massSummary == null)
            {
                return null;
            }

            var result = new List<PackInfo>();
            var numberOfPacks = massSummary["m_numPacksOpened"];
            var cardsStructure = massSummary["m_cards"];
            if (cardsStructure == null) { 
                return null; 
            }

            var cardItems = cardsStructure["_items"];
            var totalCards = cardsStructure["_size"];
            var boosterId = packOpeningMgr["m_packOpeningId"];
            var lastOpenedBoosterId = packOpeningMgr["m_lastOpenedBoosterId"];
            for (var currentPackIndex = 0; currentPackIndex < numberOfPacks; currentPackIndex++)
            {
                // To accomodate variable size packs
                var numberOfCardsPerPack = Math.Ceiling(totalCards / numberOfPacks);
                var cards = new List<ICardInfo>();
                for (var currentCardInPackIndex = currentPackIndex * numberOfCardsPerPack; currentCardInPackIndex < currentPackIndex * numberOfCardsPerPack + numberOfCardsPerPack; currentCardInPackIndex++)
                {
                    try
                    {
                        var cardItem = cardItems[currentCardInPackIndex]["<Def>k__BackingField"];
                        cards.Add(new CardInfo()
                        {
                            CardId = cardItem["<Name>k__BackingField"],
                            Premium = cardItem["<Premium>k__BackingField"],
                        });
                    }
                    catch
                    {
                        // Do nothing, probably because of the numberOfCardsPerPack that wasn't well defined
                    }
                }
                result.Add(new PackInfo()
                {
                    BoosterId = boosterId,
                    Cards = cards,
                });
            }

            return result;
        }

        public static bool ReadIsOpeningPack([NotNull] HearthstoneImage image)
        {
            int? cardsPendingReveal = image?["PackOpening"]?["s_instance"]?["m_director"]?["m_cardsPendingReveal"];
            return cardsPendingReveal != null && cardsPendingReveal > 0;
        }
    }
}
