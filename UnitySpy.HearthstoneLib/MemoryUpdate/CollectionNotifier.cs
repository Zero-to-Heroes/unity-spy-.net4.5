using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class CollectionNotifier
    {
        private IReadOnlyList<ICollectionCard> lastCollection;
        private bool isInit;

        internal void HandleNewCards(MindVision mindVision, IMemoryUpdate result)
        {
            if (!isInit)
            {
                lastCollection = mindVision.GetCollectionCards();
                if (lastCollection == null || lastCollection.Count == 0)
                {
                    // We could not read the collection, so we throw and wait for a reset
                    throw new Exception("could not read collection " + (lastCollection != null));
                }
            }

            if (isInit)
            {
                var currentCards = mindVision.GetCollectionCards();
                if (currentCards == null || currentCards.Count == 0)
                {
                    return;
                }
                var newCards = GetNewCards(currentCards, lastCollection);
                if (newCards != null && newCards.Count > 0)
                {
                    result.HasUpdates = true;
                    result.NewCards = newCards;
                }
                lastCollection = currentCards;
            }
            
            isInit = true;
        }

        internal IReadOnlyList<ICardInfo> GetNewCards(IReadOnlyList<ICollectionCard> newCollection, IReadOnlyList<ICollectionCard> previousCollection)
        {
            var totalNewCards = newCollection.Select(card => card.Count + card.PremiumCount).Sum();
            var totalPreviousCards = previousCollection.Select(card => card.Count + card.PremiumCount).Sum();
            if (totalNewCards == totalPreviousCards)
            {
                return null;
            }

            var result = new List<ICardInfo>();
            foreach (var newCard in newCollection)
            {
                var existingCard = previousCollection.Where(card => card.CardId == newCard.CardId).FirstOrDefault();
                if (existingCard == null)
                {
                    continue;
                }

                // In case cards are disenchanted, we don't want to raise anything
                var newCount = Math.Max(0, newCard.Count - existingCard.Count);
                for (int i = 0; i < newCount; i++)
                {
                    result.Add(new CardInfo()
                    {
                        CardId = newCard.CardId,
                        Premium = false,
                        TotalCount = newCard.Count,
                    });
                }

                var newPremiumCount = Math.Max(0, newCard.PremiumCount - existingCard.PremiumCount);
                for (int i = 0; i < newPremiumCount; i++)
                {
                    result.Add(new CardInfo()
                    {
                        CardId = newCard.CardId,
                        Premium = true,
                        TotalCount = newCard.PremiumCount,
                    });
                }
            }
            return result;
        }

        public static long UnixTimestamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}