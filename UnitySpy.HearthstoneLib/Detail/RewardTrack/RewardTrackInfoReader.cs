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
    }
}
