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

        IReadOnlyList<IRewardInfo> ArenaRewards { get; set; }

        long? SelectedDeckId { get; set; }

        bool IsOpeningPack { get; set; }

        bool? IsMercenariesSelectingTreasure { get; set; }

        IMercenariesPendingTreasureSelection MercenariesPendingTreasureSelection  { get; set; }

        bool? IsMercenariesTasksUpdated { get; set; }

        int? BattlegroundsNewRating { get; set; }
    }
}