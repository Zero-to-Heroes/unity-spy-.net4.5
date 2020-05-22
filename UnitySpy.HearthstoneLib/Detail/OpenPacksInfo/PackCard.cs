namespace HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class PackCard : IPackCard
    {
        public string CardId { get; set; }

        public bool IsNew { get; set; }

        public bool Premium { get; set; }

        public bool Revealed { get; set; }
    }
}
