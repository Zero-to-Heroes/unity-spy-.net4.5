﻿namespace HackF5.UnitySpy.HearthstoneLib
{
    using HackF5.UnitySpy.HearthstoneLib.Detail.InputManager;
    using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;
    using HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo;
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

        List<PackInfo> OpenedPacks { get; set; }
        List<PackInfo> MassOpenedPacks { get; set; }

        IReadOnlyList<ICardInfo> NewCards { get; set; }

        IReadOnlyList<XpChange> XpChanges { get; set; }

        IReadOnlyList<IRewardInfo> ArenaRewards { get; set; }
        DraftSlotType? ArenaDraftStep { get; set; }
        List<string> ArenaHeroOptions { get; set; }
        List<string> ArenaCardOptions { get; set; }
        int? ArenaCurrentCardsInDeck { get; set; }

        long? SelectedDeckId { get; set; }

        bool? IsOpeningPack { get; set; }

        int? MercenariesTreasureSelectionIndex { get; set; }

        IMercenariesPendingTreasureSelection MercenariesPendingTreasureSelection  { get; set; }

        bool? isFriendsListOpen { get; set; }

        int? BattlegroundsNewRating { get; set; }
        string BattlegroundsSelectedGameMode { get; set; }
        MousedOverCard MousedOverCard { get; set; }
    }
}