// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Mercenaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class MercenariesInfoReader
    {
        public static IMercenariesInfo ReadMercenariesInfo(HearthstoneImage image)
        {
            var netCacheMercenaries = image.GetNetCacheService("NetCacheMercenariesPlayerInfo");
            var pvpRating = netCacheMercenaries?["<PvpRating>k__BackingField"];

            var netCacheMercenariesMap = image.GetNetCacheService("NetCacheLettuceMap")?["<Map>k__BackingField"];
            IMercenariesMap map = null;
            if (netCacheMercenariesMap != null)
            {

                var playerData = netCacheMercenariesMap["_PlayerData"];
                var playerDataSize = playerData["_size"];
                var playerDataItem = playerDataSize > 0 ? playerData["_items"][0] : null;

                var playerTeamList = playerDataItem?["_TeamList"]?["_Mercenaries"];
                var teamList = new List<int>();
                var teamSize = playerTeamList?["_size"] ?? 0;
                for (var i = 0; i < teamSize; i++)
                {
                    teamList.Add(playerTeamList["_items"][i]);
                }

                var deadMercsList = netCacheMercenariesMap?["_DeadMercenaries"];
                var deadMercsListSize = deadMercsList["_size"];
                var deadMercs = new List<int>();
                for (var i = 0; i < deadMercsListSize; i++)
                {
                    var deadMercsItem = deadMercsList["_items"][i]?["_Mercenaries"];
                    var size = deadMercsItem["_size"];
                    for (var j = 0; j < size; j++)
                    {
                        deadMercs.Add(deadMercsItem["_items"][j]);
                    }

                }

                var nodes = netCacheMercenariesMap["_Nodes"];
                var nodesSize = nodes["_size"];
                uint? currentRow = 0;
                uint? maxRow = 0;
                for (var i = 0; i < nodesSize; i++)
                {
                    var node = nodes["_items"][i];
                    if (node["_NodeState_"] == (int)NodeState.COMPLETE)
                    {
                        // Row N is completed, which means we're at step N + 1
                        currentRow = Math.Max(currentRow ?? 0, node["_Row"] + 1);
                    }
                    maxRow = Math.Max(maxRow ?? 0, node["_Row"]);
                }

                var fullTeam = BuildFullTeam(image, teamList);
                map = new MercenariesMap()
                {
                    BountyId = netCacheMercenariesMap["_BountyId"],
                    MapId = netCacheMercenariesMap["_MapId"],
                    Seed = netCacheMercenariesMap["_Seed"],
                    PlayerTeamId = playerDataItem?["_TeamId"],
                    PlayerTeamName = playerDataItem?["_TeamName"],
                    PlayerTeamMercIds = teamList,
                    DeadMercIds = deadMercs,
                    PlayerTeam = fullTeam,
                    CurrentStep = currentRow ?? 0,
                    MaxStep = maxRow ?? 0,
                };
            }

            return new MercenariesInfo()
            {
                PvpRating = pvpRating ?? 0,
                Map = map,
            };
        }

        private static IReadOnlyList<IMercenary> BuildFullTeam(HearthstoneImage image, IReadOnlyList<int> mercIds)
        {
            var allMercenaries = image["CollectionManager"]["s_instance"]["m_collectibleMercenaries"];
            var mercsCount = allMercenaries["_size"];
            var mercenaries = new List<IMercenary>();
            for (var i = 0; i < mercsCount; i++)
            {
                var mercInfo = allMercenaries["_items"][i];
                int mercId = mercInfo["ID"];
                if (!mercIds.Contains(mercId))
                {
                    continue;
                }

                var mercLevel = mercInfo["m_level"];

                var abilityList = mercInfo["m_abilityList"];
                var abilitiesCount = abilityList["_size"];
                var mercAbilities = new List<IMercenaryAbility>();
                for (var j = 0; j < abilitiesCount; j++)
                {
                    var ability = abilityList["_items"][j];
                    var unlockLevel = ability["m_unlockLevel"];
                    if (mercLevel < unlockLevel)
                    {
                        continue;
                    }
                    var tierId = ability["m_tier"];
                    var tierList = ability["m_tierList"];
                    var tier = GetTier(tierList, tierId);
                    mercAbilities.Add(new MercenaryAbility()
                    {
                        CardId = tier.CardId,
                        Tier = tier.Tier,
                    });
                }
                mercenaries.Add(new Mercenary()
                {
                    Id = mercId,
                    Level = mercLevel,
                    Abilities = mercAbilities,
                });
            }
            return mercenaries;
        }

        private static dynamic GetTier(dynamic tierList, int tierId)
        {
            foreach (var tier in tierList)
            {
                if (tier["m_tier"] == tierId)
                {
                    return new
                    {
                        CardId = tier["m_cardId"],
                        Tier = tier["m_tier"],
                    };
                }
            }
            return null;
        }

        private static int ReadNewRating(dynamic gameState)
        {
            try
            {
                return gameState?["m_gameEntity"]?["<RatingChangeData>k__BackingField"]?["_NewRating"] ?? -1;
            }
            catch (Exception e)
            {
                // Do nothing, but don't pollute the logs
                return -1;
            }
        }
    }
}