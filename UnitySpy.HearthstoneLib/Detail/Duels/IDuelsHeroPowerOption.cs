namespace HackF5.UnitySpy.HearthstoneLib
{
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDuelsHeroPowerOption
    {
        long DatabaseId { get; }

        bool Enabled { get; }

        bool Visible { get; }

        bool Completed { get; }

        bool Locked { get; }

        bool Selected { get; }
    }
}
