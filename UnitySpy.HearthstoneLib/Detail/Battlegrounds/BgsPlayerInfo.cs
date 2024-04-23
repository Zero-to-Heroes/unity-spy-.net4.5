using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    public class BgsTeamInfo
    {
        public BgsPlayerInfo Player;
        public BgsPlayerInfo Teammate;
    }

    public class BgsPlayerInfo
    {
        public BgsEntity Hero;
        public BgsEntity HeroPower;
        public List<BgsEntity> Board;
        public List<BgsEntity> Hand;
        public List<BgsEntity> Secrets;
    }

    public class BgsEntity
    {
        public string CardId;
        public List<EntityTag> Tags;
        public List<BgsEntity> Enchantments;

        private int? _entityId;

        public int EntityId()
        {
            if (this._entityId != null)
            {
                return this._entityId.Value;
            }
            this._entityId = GetTag(GameTag.ENTITY_ID);
            return this._entityId.Value;
        }

        public int GetTag(GameTag tag)
        {
            return Tags.Find(t => t.Name == (int)tag)?.Value ?? 0;
        }

        public Zone GetZone()
        {
            return (Zone)(Tags.Find(t => t.Name == (int)GameTag.ZONE)?.Value ?? (int)Zone.INVALID);
        }

        public int GetZonePosition()
        {
            return GetTag(GameTag.ZONE_POSITION);
        }

        public int GetController()
        {
            return Tags.Find(t => t.Name == (int)GameTag.CONTROLLER)?.Value ?? 0;
        }

        public CardType GetCardType()
        {
            return (CardType)(Tags.Find(t => t.Name == (int)GameTag.CARDTYPE)?.Value ?? (int)CardType.INVALID);
        }

        public bool IsOnBoard()
        {
            var cardType = GetCardType();
            return cardType == CardType.MINION || cardType == CardType.BATTLEGROUND_SPELL || cardType == CardType.LOCATION;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BgsEntity))
            {
                return false;
            }
            var other = obj as BgsEntity;
            return this.CardId == other.CardId
                && this.GetTag(GameTag.ATK) == other.GetTag(GameTag.ATK)
                && this.GetTag(GameTag.HEALTH) == other.GetTag(GameTag.HEALTH)
                && this.GetTag(GameTag.DIVINE_SHIELD) == other.GetTag(GameTag.DIVINE_SHIELD)
                && this.GetTag(GameTag.TAUNT) == other.GetTag(GameTag.TAUNT);
        }

        public override int GetHashCode()
        {
            return 1;
        }
    }

    public class EntityTag
    {
        public int Name;
        public int Value;
    }
}
