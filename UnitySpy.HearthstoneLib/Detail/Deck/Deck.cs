namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class Deck : IDeck
    {
        public long DeckId { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public IReadOnlyList<string> DeckList { get; set; }

        public int HeroClass { get; set; }

        public string HeroCardId { get; set; }

        public int FormatType { get; set; }

        public List<DeckSideboard> Sideboards { get; set; }
    }

    internal class DeckSideboard
    {
        public string KeyCardId { get; set; }
        public List<string> Cards { get; set; }
    }
}
