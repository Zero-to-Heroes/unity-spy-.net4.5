﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib
{
    public interface IDungeonOptionBundle
    {
        int BundleId { get; }

        IReadOnlyList<int> Elements { get; }
    }
}
