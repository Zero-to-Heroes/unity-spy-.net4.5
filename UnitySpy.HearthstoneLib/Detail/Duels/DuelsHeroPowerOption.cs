using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Duels
{
    class DuelsHeroPowerOption : IDuelsHeroPowerOption
    {
        public long DatabaseId { get; set; }

        public bool Enabled { get; set; }

        public bool Visible { get; set; }

        public bool Completed { get; set; }

        public bool Locked { get; set; }

        public bool Selected { get; set; }
    }
}
