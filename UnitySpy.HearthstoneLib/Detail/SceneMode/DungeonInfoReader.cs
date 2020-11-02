namespace HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class SceneModeReader
    {
        public static SceneModeEnum ReadSceneMode([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image["SceneMgr"] == null || image["SceneMgr"]["s_instance"] == null)
            {
                return SceneModeEnum.INVALID;
            }

            var sceneMgr = image["SceneMgr"]["s_instance"];
            var mode = sceneMgr["m_mode"];
            return (SceneModeEnum)mode;
        }
    }
}
