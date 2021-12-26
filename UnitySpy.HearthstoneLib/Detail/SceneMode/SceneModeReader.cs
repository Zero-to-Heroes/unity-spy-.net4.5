namespace HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode
{
    using System;
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

            if (image["SceneMgr"] == null || image["SceneMgr"]["s_instance"] == null)
            {
                return null;
            }

            var sceneMgr = image["SceneMgr"]["s_instance"];
            // This regularly throws a "Only part of a ReadProcessMemory or WriteProcessMemory request was completed"
            // exception right after a scene change
            // Because this happens pretty frequently, and pollutes the logs quite a bit, for this call only we work with a 
            // try/catch
            var mode = Utils.TryGetField(sceneMgr, "m_mode");
            if (mode == null)
            {
                return null;
            }

            return (SceneModeEnum)mode;
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

        public static bool ReadMercenariesIsSelectingTreasures(HearthstoneImage image)
        {
            if (ReadSceneMode(image) != SceneModeEnum.LETTUCE_MAP)
            {
                return false;
            }

            return image["SceneMgr"]?["s_instance"]?["m_scene"]?["m_sceneDisplay"]?["m_waitingForTreasureSelection"] ?? false;
        }
    }
}
