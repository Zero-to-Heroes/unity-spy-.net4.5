namespace HackF5.UnitySpy.HearthstoneLib.Detail.InputManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
    using JetBrains.Annotations;

    internal static class InputManagerReader
    {
        public static MousedOverCard ReadCurrentMousedOverCard([NotNull] HearthstoneImage image)
        {
            var inputManager = image["InputManager"]?["s_instance"];
            if (inputManager == null)
            {
                return null;
            }

            var mousedOverCard = inputManager["m_mousedOverCard"];
            if (mousedOverCard == null)
            {
                return null;
            }

            var zone = mousedOverCard["m_zone"];
            var zoneSide = zone?["m_Side"];
            var zoneTag = zone?["m_ServerTag"];
            var entity = mousedOverCard["m_entity"];
            var cardId = entity["m_cardIdInternal"];
            var tagsMap = entity["m_tags"]["m_values"];
            var count = tagsMap["_count"];
            var entries = tagsMap["_entries"];
            int? entityId = null;
            for (var i = 0; i < count; i++)
            {
                var entry = entries[i];
                var tag = entry["key"];
                if (tag == (int)GameTag.ENTITY_ID)
                {
                    entityId = entry["value"];
                }
            }

            return new MousedOverCard
            {
                CardId = cardId,
                Zone = zoneTag,
                Side = zoneSide,
                EntityId = entityId,
            };
        }
    }
}
