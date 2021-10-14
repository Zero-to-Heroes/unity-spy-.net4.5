using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Mercenaries
{
    internal class Mercenary : IMercenary
    {
        public int Id { get; set; }

        public int Level { get; set; }

        public IReadOnlyList<IMercenaryAbility> Abilities { get; set; }
    }

    internal class MercenaryAbility : IMercenaryAbility
    {
        public string CardId { get; set; }

        public int Tier { get; set; }
    }
}