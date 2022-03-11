namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;

    public interface IDuelsPendingTreasureSelection
    {
        IReadOnlyList<int> Options { get; }
    }
}
