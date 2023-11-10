using HackF5.UnitySpy.HearthstoneLib.Detail.Deck;
using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    public class DuelsInfo
    {
        //public string HeroCardId { get; set; }
        public string DeckId { get; set; }
        public bool IsPaidEntry { get; set; }
        public long? HeroCardDbfId { get; set; }
        public long? HeroPowerCardDbfId { get; set; }
        public long? SignatureTreasureCardDbfId { get; set; }
        public int? PlayerClass { get; set; }
        public bool? RunActive { get; set; }
        public bool? SessionActive { get; set; }
        public Deck.Deck DuelsDeck { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Rating { get; set; }
        public int? PaidRating { get; set; }
        public int? LastRatingChange { get; set; }
        public IReadOnlyList<IDungeonOptionBundle> LootOptionBundles { get; set; }
        public int? ChosenLoot { get; set; }
        public IReadOnlyList<int> TreasureOption { get; set; }
        public int? ChosenTreasure { get; set; }
    }

    //public class DuelsDeck
    //{
    //    public IReadOnlyList<int> DeckList { get; set; }
    //    public List<DeckSideboard> Sideboards { get; set; }
    //}
}
