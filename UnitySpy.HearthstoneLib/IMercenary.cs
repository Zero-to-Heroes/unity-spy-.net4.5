namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;
    using System.Collections.Generic;

    [PublicAPI]
    public interface IMercenary
    {
        int Id { get; }

        int Level { get; }

        IReadOnlyList<IMercenaryAbility> Abilities { get; }
    }

    [PublicAPI]
    public interface IMercenaryAbility
    {
        string CardId { get; }

        int Tier { get; }
    }
}