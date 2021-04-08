namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IMemoryUpdate
    {
        bool HasUpdates { get; set; }

        bool DisplayingAchievementToast { get; set; }

        SceneModeEnum? CurrentScene { get; set; }

        IPackInfo OpenedPack { get; set; }

        IReadOnlyList<ICardInfo> NewCards { get; set; }

        IReadOnlyList<IXpChange> XpChanges { get; set; }
    }
}