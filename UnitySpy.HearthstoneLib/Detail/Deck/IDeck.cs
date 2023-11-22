namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;

    public interface IDeck
    {
        long DeckId { get; }

        string Id { get; }

        string Name { get; }

        IList<string> DeckList { get; }

        int HeroClass { get; }

        string HeroCardId { get; }

        int FormatType { get; }
    }
}
