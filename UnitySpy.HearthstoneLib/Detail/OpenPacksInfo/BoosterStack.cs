namespace HackF5.UnitySpy.HearthstoneLib.Detail.OpenPacksInfo
{
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class BoosterStack : IBoosterStack
    {
        public int BoosterId { get; set; }

        public int Count { get; set; }

        public int EverGrantedCount { get; set; }
    }
}
