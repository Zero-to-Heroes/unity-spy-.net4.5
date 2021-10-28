using HackF5.UnitySpy.HearthstoneLib.Detail.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate
{
    class MemoryUpdate : IMemoryUpdate
    {
        public bool HasUpdates { get; set; }

        public bool DisplayingAchievementToast { get; set; }

        public SceneModeEnum? CurrentScene { get; set; }

        public IPackInfo OpenedPack { get; set; }

        public IReadOnlyList<ICardInfo> NewCards { get; set; }

        public IReadOnlyList<IXpChange> XpChanges { get; set; }

        public IReadOnlyList<IRewardInfo> ArenaRewards { get; set; }

        public long? SelectedDeckId { get; set; }

        public bool IsOpeningPack { get; set; }

        public bool? IsMercenariesSelectingTreasure { get; set; }

        public IMercenariesPendingTreasureSelection MercenariesPendingTreasureSelection { get; set; }
    }
}
