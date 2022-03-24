using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    class DuelsInfo : IDuelsInfo
    {
        public DuelsKey Key { get; set; }

        public string HeroCardId { get; set; }

        public int StartingHeroPower { get; set; }

        public string StartingHeroPowerCardId { get; set; }

        public IReadOnlyList<int> DeckList { get; set; }

        public IReadOnlyList<string> DeckListWithCardIds { get; set; }

        public IReadOnlyList<IDungeonOptionBundle> LootOptionBundles { get; set; }

        public int ChosenLoot { get; set; }

        public IReadOnlyList<int> TreasureOption { get; set; }

        public int ChosenTreasure { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }

        public int Rating { get; set; }

        public int LastRatingChange { get; set; }

        public int PaidRating { get; set; }

        public int PlayerClass { get; set; }
        
        public int RunActive { get; set; }
    }
}
