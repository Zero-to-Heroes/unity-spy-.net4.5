namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;

    public interface IDeck
    {
        string Name { get; }

        IReadOnlyList<string> DeckList { get; }
    }
}
