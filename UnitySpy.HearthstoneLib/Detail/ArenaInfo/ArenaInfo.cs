namespace HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class ArenaInfo : IArenaInfo
    {
        public int Wins { get; set;  }

        public int Losses { get; set;  }

        public string HeroCardId { get; set;  }
    }
}
