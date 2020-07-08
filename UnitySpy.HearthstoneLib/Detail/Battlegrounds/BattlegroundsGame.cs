﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    internal class BattlegroundsGame : IBattlegroundsGame
    {
        public IReadOnlyList<IBattlegroundsPlayer> Players { get; set;  }

        public IReadOnlyList<int> AvailableRaces { get; set; }
    }
}
