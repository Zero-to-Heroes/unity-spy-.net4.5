// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class BattlegroundsDuoInfoReader
    {
        public static BgsPlayerInfo ReadPlayerTeammateBoard(HearthstoneImage image)
        {
            var service = image["TeammateBoardViewer"]?["s_instance"];
            if (service == null)
            {
                return null;
            }

            List<BgsEntity> entities = ReadEntities(service);
            var hero = ReadHero(service);
            if (hero == null)
            {
                return null;
            }

            var heroController = hero.GetController();
            var heroPower = ReadHeroPower(service);
            var board = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetController() == heroController)
                .Where(e => e.IsOnBoard())
                .Select(e => AddEnchantments(e, entities))
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            var hand = entities
                .Where(e => e.GetZone() == Zone.HAND)
                .Where(e => e.GetController() == heroController)
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            var secrets = entities
                .Where(e => e.GetZone() == Zone.SECRET)
                .Where(e => e.GetController() == heroController)
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            return new BgsPlayerInfo()
            {
                Hero = hero,
                HeroPower = heroPower,
                Board = board,
                Hand = hand,
                Secrets = secrets,
            };
        }

        private static BgsEntity ReadHeroPower(dynamic service)
        {
            var entityActors = service["m_teammateHeroViewer"]?["m_entityActors"];
            if (entityActors == null)
            {
                return null;
            }

            var count = entityActors["_count"];
            var entries = entityActors["_entries"];
            for (var i = 0; i < count; i++)
            {
                var entity = entries[i]?["value"]["m_entity"];
                if (entity == null)
                {
                    continue;
                }
                var cardId = entity["m_cardIdInternal"];
                var memTags = entity["m_tags"]["m_values"];
                var tags = ReadTags(memTags);
                var result = new BgsEntity()
                {
                    CardId = cardId,
                    Tags = tags,
                };
                if (result.GetCardType() == CardType.HERO_POWER)
                {
                    return result;
                }
            }
            return null;
        }

        private static BgsEntity ReadHero(dynamic service)
        {
            var memHero = service["m_teammateHeroViewer"]?["m_teammateHero"];
            if (memHero == null)
            {
                return null;
            }

            var cardId = memHero["m_cardIdInternal"];
            var memTags = memHero["m_tags"]["m_values"];
            var tags = ReadTags(memTags);
            return new BgsEntity()
            {
                CardId = cardId,
                Tags = tags,
            };
        }

        private static List<BgsEntity> ReadEntities(dynamic service)
        {
            var actorsDict = service?["m_teammateMinionViewer"]?["m_entityActors"];
            var count = actorsDict?["_count"] ?? 0;
            var result = new List<BgsEntity>();
            for (var i = 0; i < count; i++)
            {
                var entity = actorsDict["_entries"][i]?["value"]?["m_entity"];
                if (entity == null)
                {
                    continue;
                }
                var cardId = entity["m_staticEntityDef"]["m_cardIdInternal"];
                var memTags = entity["m_tags"]["m_values"];
                var tags = ReadTags(memTags);
                result.Add(new BgsEntity()
                {
                    CardId = cardId,
                    Tags = tags,
                });
            }
            return result;
        }

        public static List<EntityTag> ReadTags(dynamic memTags)
        {
            var count = memTags["_count"];
            var entries = memTags["_entries"];
            var result = new List<EntityTag>();
            for (var i = 0; i < count; i++)
            {
                var memTag = entries[i];
                result.Add(new EntityTag()
                {
                    Name = memTag["key"],
                    Value = memTag["value"],
                });
            }
            return result;
        }

        public static BgsEntity AddEnchantments(BgsEntity entity, List<BgsEntity> entities)
        {
            var enchantments = entities
                .Where(e => e.GetZone() == entity.GetZone())
                .Where(e => e.GetTag(GameTag.ATTACHED) == entity.GetTag(GameTag.ENTITY_ID))
                .ToList();
            entity.Enchantments = enchantments;
            return entity;
        }
    }
}