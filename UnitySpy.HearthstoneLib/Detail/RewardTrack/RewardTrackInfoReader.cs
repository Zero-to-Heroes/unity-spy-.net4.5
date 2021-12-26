namespace HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class RewardTrackInfoReader
    {
        public static IRewardTrackInfo ReadRewardTrack([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var service = image.GetService("Hearthstone.Progression.RewardTrackManager");
            if (service == null)
            {
                return null;
            }

            var trackModel = service["<TrackDataModel>k__BackingField"];
            if (trackModel == null)
            {
                return null;
            }

            return new RewardTrackInfo
            {
                Level = trackModel["m_Level"],
                Xp = trackModel["m_Xp"],
                XpNeeded = trackModel["m_XpNeeded"],
                XpBonusPercent = trackModel["m_XpBonusPercent"],
            };
        }
        public static IReadOnlyList<IXpChange> ReadXpChanges([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var result = new List<IXpChange>();

            var service = image.GetService("Hearthstone.Progression.RewardXpNotificationManager");
            if (service == null)
            {
                return result;
            }

            dynamic xpChanges = service["m_xpChanges"];
            if (xpChanges == null)
            {
                return result;
            }

            var size = xpChanges["_size"];
            if (size == 0)
            {
                return result;
            }

            for (var i = 0; i < size; i++)
            {
                var xpChange = xpChanges["_items"][i];
                result.Add(new XpChange()
                {
                    CurrentLevel = xpChange["_CurrLevel"],
                    CurrentXp = xpChange["_CurrXp"],
                    PreviousLevel = xpChange["_PrevLevel"],
                    PreviousXp = xpChange["_PrevXp"],
                    RewardSourceId = xpChange["_RewardSourceId"],
                    RewardSourceType = xpChange["_RewardSourceType"],
                });
            }

            return result;
        }
    }
}
