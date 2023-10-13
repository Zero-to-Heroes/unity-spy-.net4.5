namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IMemoryUpdate
    {
        bool HasUpdates { get; set; }

        bool? CollectionInit { get; set; }

        int? CollectionCardsCount { get; set; }
        int? CollectionCardBacksCount { get; set; }
        int? CollectionCoinsCount { get; set; }
        int? CollectionBgHeroSkinsCount { get; set; }
        int? BoostersCount { get; set; }

        int? NumberOfAchievementsCompleted { get; set; }
        bool? DisplayingAchievementToast { get; set; }

        SceneModeEnum? CurrentScene { get; set; }

        IPackInfo OpenedPack { get; set; }

        IReadOnlyList<ICardInfo> NewCards { get; set; }

        IReadOnlyList<IXpChange> XpChanges { get; set; }

        IReadOnlyList<IRewardInfo> ArenaRewards { get; set; }

        long? SelectedDeckId { get; set; }

        bool? IsOpeningPack { get; set; }

        bool? IsDuelsMainRunScreen { get; set; }

        bool? IsDuelsDeckBuildingLobbyScreen { get; set; }

        bool? IsDuelsSelectingTreasure { get; set; }

        bool? IsDuelsChoosingHero { get; set; }

        int? DuelsCurrentOptionSelection { get; set; }

        int? DuelsCurrentCardsInDeck { get; set; }

        bool? IsDuelsRewardsPending { get; set; }

        IDuelsPendingTreasureSelection DuelsPendingTreasureSelection { get; set; }

        int? MercenariesTreasureSelectionIndex { get; set; }

        IMercenariesPendingTreasureSelection MercenariesPendingTreasureSelection  { get; set; }

        bool? isFriendsListOpen { get; set; }

        int? BattlegroundsNewRating { get; set; }
    }
}