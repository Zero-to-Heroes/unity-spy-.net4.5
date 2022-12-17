namespace HackF5.UnitySpy.HearthstoneLib.Detail.GameDbf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class GameDbfReader
    {
        public static IList<RaceTag> ReadRaceTags([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var records = image["GameDbf"]["CardRace"]["m_records"];
            var size = records["_size"];
            IList<RaceTag> result = new List<RaceTag>();
            for (var i = 0; i < size; i++)
            {
                var item = records["_items"][i];
                result.Add(new RaceTag()
                {
                    Id = item["m_ID"],
                    RaceTagId = item["m_isRaceTagId"],
                });
            }
            return result;
        }
    }
}
