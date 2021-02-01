namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IMemoryUpdate
    {
        bool DisplayingAchievementToast { get; }

        SceneModeEnum CurrentScene { get; }
    }
}