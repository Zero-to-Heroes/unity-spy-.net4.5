using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Mercenaries
{
    internal class Mercenary : IMercenary
    {
        public int Id { get; set; }

        public int Level { get; set; }

        public IReadOnlyList<IMercenaryAbility> Abilities { get; set; }

        public IReadOnlyList<IMercenaryEquipment> Equipments { get; set; }

        public IReadOnlyList<IMercenarySkin> Skins { get; set; }

        public IList<int> TreasureCardDbfIds { get; set; }

        public int Attack { get; set; }

        public int Health { get; set; }

        public long CurrencyAmount { get; set; }

        public long Experience { get; set; }

        public bool IsFullyUpgraded { get; set; }

        public bool Owned { get; set; }

        public int Premium { get; set; }

        public int Rarity { get; set; }

        public int Role { get; set; }
    }

    internal class MercenaryAbility : IMercenaryAbility
    {
        public string CardId { get; set; }

        public int Tier { get; set; }
    }

    internal class MercenaryEquipment : IMercenaryEquipment
    {
        public int Id { get; set; }

        public int CardType { get; set; }

        public bool Owned { get; set; }

        public bool Equipped { get; set; }

        public int Tier { get; set; }
    }


    internal class MercenarySkin : IMercenarySkin
    {

        public int Id { get; set; }

        public int CardDbfId { get; set; }

        public bool Default { get; set; }

        //public bool Equipped { get; set; }

        public int Premium { get; set; }
    }
}