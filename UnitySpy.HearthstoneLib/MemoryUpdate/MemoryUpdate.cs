using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
using HackF5.UnitySpy.HearthstoneLib.Detail.InputManager;
using HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo;
using HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack;
using System;
using System.Collections.Generic;
using HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate
{
    public class MemoryUpdate : IMemoryUpdate
    {
        public bool HasUpdates { get; set; }
        public long TotalTimeElapsed { get; set; }

        public bool? CollectionInit { get; set; }

        public int? CollectionCardsCount { get; set; }
        public int? CollectionCardBacksCount { get; set; }
        public int? CollectionCoinsCount { get; set; }
        public int? CollectionBgHeroSkinsCount { get; set; }
        public int? BoostersCount { get; set; }

        public int? NumberOfAchievementsCompleted { get; set; }
        public bool? DisplayingAchievementToast { get; set; }

        public SceneModeEnum? CurrentScene { get; set; }

        public List<PackInfo> OpenedPacks { get; set; }
        public List<PackInfo> MassOpenedPacks { get; set; }

        public IReadOnlyList<ICardInfo> NewCards { get; set; }

        public IReadOnlyList<XpChange> XpChanges { get; set; }

        public IReadOnlyList<IRewardInfo> ArenaRewards { get; set; }
        public DraftSlotType? ArenaDraftStep { get; set; }
        public List<string> ArenaHeroOptions { get; set; }
        public List<string> ArenaCardOptions { get; set; }
        public int? ArenaCurrentCardsInDeck { get; set; }

        public long? SelectedDeckId { get; set; }

        public bool? IsOpeningPack { get; set; }

        public int? MercenariesTreasureSelectionIndex { get; set; }

        public IMercenariesPendingTreasureSelection MercenariesPendingTreasureSelection { get; set; }
        public bool? isFriendsListOpen { get; set; }

        public int? BattlegroundsNewRating { get; set; }
        public string BattlegroundsSelectedGameMode { get; set; }
        public MousedOverCard MousedOverCard { get; set; }

        public object Debug { get; set; }
    }
}
