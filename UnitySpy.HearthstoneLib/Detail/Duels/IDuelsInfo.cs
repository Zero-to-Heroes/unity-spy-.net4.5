namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDuelsInfo
    {
        DuelsKey Key { get; }

        int StartingHeroPower { get; }

        string StartingHeroPowerCardId { get; }

        IReadOnlyList<int> DeckList { get; }

        IReadOnlyList<string> DeckListWithCardIds { get; }

        IReadOnlyList<IDungeonOptionBundle> LootOptionBundles { get; }

        int ChosenLoot { get; }

        IReadOnlyList<int> TreasureOption { get; }

        int ChosenTreasure { get; }

        int Wins { get; }

        int Losses { get; }

        int Rating { get; }

        int LastRatingChange { get; }

        int PaidRating { get; }

        int PlayerClass { get; }

        int RunActive { get; }
    }
}
