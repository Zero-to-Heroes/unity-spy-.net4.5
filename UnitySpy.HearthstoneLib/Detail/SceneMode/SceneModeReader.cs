namespace HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Duels;
    using JetBrains.Annotations;

    internal static class SceneModeReader
    {
        public static SceneModeEnum? ReadSceneMode([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            try
            {
                if (image["SceneMgr"] == null || image["SceneMgr"]["s_instance"] == null)
                {
                    return null;
                }

                var sceneMgr = image["SceneMgr"]["s_instance"];
                var mode = sceneMgr["m_mode"];
                return (SceneModeEnum)mode;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool IsMaybeOnDuelsRewardsScreen([NotNull] HearthstoneImage image)
        {
            try
            {
                if (image == null)
                {
                    throw new ArgumentNullException(nameof(image));
                }

                var display = image["PvPDungeonRunScene"]["m_instance"]["m_display"];
                var backButtonEnabled = display["m_backButton"]["m_enabled"];
                var sessionActive = display["m_dataModel"]["m_IsSessionActive"];
                var hasSession = display["m_dataModel"]["m_HasSession"];
                var duelsInfo = DuelsInfoReader.ReadDuelsInfo(image);
                var isRunActive = duelsInfo?.RunActive ?? 0;
                return !backButtonEnabled && !sessionActive && hasSession && (isRunActive == 0);
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
