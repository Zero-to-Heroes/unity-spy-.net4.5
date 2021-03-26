﻿namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class Deck : IDeck
    {
        public string Name { get; set; }

        public IReadOnlyList<string> DeckList { get; set; }

        public string HeroCardId { get; set; }

        public int FormatType { get; set; }
    }
}
