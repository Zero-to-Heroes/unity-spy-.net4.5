// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Battlegrounds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class BattlegroundsPlayerInfoReader
    {
        // If the teammate fights first, it works, otherwise we have dupes
        // Maybe use cardId + atk + health + divine shield + taunt attributes to define equality, and remove dupes from player board
        public static BgsTeamInfo ReadPlayerBoard(HearthstoneImage image)
        {
            var teammateBoard = BattlegroundsDuoInfoReader.ReadPlayerTeammateBoard(image);
            var service = image["GameState"]?["s_instance"];
            List<BgsEntity> entities = ReadAllEntities(service);
            var hero = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetCardType() == CardType.HERO)
                .Where(e => e.EntityId() != teammateBoard?.Hero?.EntityId())
                .FirstOrDefault();
            var heroController = hero.GetController(); 
            var heroPower = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetCardType() == CardType.HERO_POWER)
                .Where(e => e.EntityId() != teammateBoard?.HeroPower?.EntityId())
                .FirstOrDefault();
            var board = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetController() == heroController)
                .Where(e => e.IsOnBoard())
                .Where(e => !(teammateBoard?.Board?.Select(m => m.EntityId()).Contains(e.EntityId()) ?? false))
                .Select(e => BattlegroundsDuoInfoReader.AddEnchantments(e, entities))
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            var boardDebug = entities
                .Where(e => e.GetZone() == Zone.PLAY)
                .Where(e => e.GetController() == heroController)
                .Where(e => e.IsOnBoard())
                // Not sure what this maps to, but it looks like the teammate entities don't have this set
                .Where(e => e.GetTag((GameTag)3669, -1) == 0)
                .Where(e => !(teammateBoard?.Board?.Select(m => m.EntityId()).Contains(e.EntityId()) ?? false))
                .Select(e => BattlegroundsDuoInfoReader.AddEnchantments(e, entities))
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            var hand = entities
                .Where(e => e.GetZone() == Zone.HAND)
                .Where(e => e.GetController() == heroController)
                .Where(e => !(teammateBoard?.Hand?.Contains(e) ?? false))
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            var secrets = entities
                .Where(e => e.GetZone() == Zone.SECRET)
                .Where(e => e.GetController() == heroController)
                .Where(e => !(teammateBoard?.Secrets?.Contains(e) ?? false))
                .OrderBy(e => e.GetZonePosition())
                .ToList();
            return new BgsTeamInfo()
            {
                Player = new BgsPlayerInfo()
                {
                    Hero = hero,
                    HeroPower = heroPower,
                    Board = board,
                    BoardDebug = boardDebug,
                    Hand = hand,
                    Secrets = secrets,
                },
                Teammate = teammateBoard,
            };
        }

        private static List<BgsEntity> ReadAllEntities(dynamic service)
        {
            var result = new List<BgsEntity>();
            var playerMap = service?["m_playerMap"];
            if (playerMap == null)
            {
                return result;
            }

            var count = playerMap["count"];
            var playerId = -1;
            var heroEntityId = -1;
            var controllerId = -1;
            for (var i = 0; i < count; i++)
            {
                var memPlayer = playerMap["valueSlots"][i];
                if (memPlayer == null || memPlayer["m_local"] == false)
                {
                    continue;
                }

                List<EntityTag> tags = BattlegroundsDuoInfoReader.ReadTags(memPlayer["m_tags"]["m_values"]);
                playerId = tags.Find(t => t.Name == (int)GameTag.PLAYER_ID)?.Value ?? 0;
                heroEntityId = tags.Find(t => t.Name == (int)GameTag.HERO_ENTITY)?.Value ?? 0;
                controllerId = tags.Find(t => t.Name == (int)GameTag.CONTROLLER)?.Value ?? 0;
            }

            var entityMap = service["m_entityMap"];
            var entitiesCount = entityMap["count"];
            var values = entityMap["valueSlots"];
            foreach (var entityNode in values)
            {
                if (entityNode == null)
                {
                    continue;
                }

                var cardId = entityNode["m_cardIdInternal"];
                if (string.IsNullOrEmpty(cardId))
                {
                    continue;
                }

                var memTags = entityNode["m_tags"]["m_values"];
                var tags = BattlegroundsDuoInfoReader.ReadTags(memTags);
                var entity = new BgsEntity()
                {
                    CardId = cardId,
                    Tags = tags,
                };
                if (entity.GetController() != controllerId) 
                {
                    continue;
                }

                result.Add(entity);
            }
            return result;

        }
    }
}