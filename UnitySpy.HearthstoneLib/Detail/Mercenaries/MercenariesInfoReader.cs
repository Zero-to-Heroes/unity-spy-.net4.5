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

                //var playerTeamList = playerDataItem?["_TeamList"]?["_Mercenaries"]; 
                var teamId = playerDataItem["_TeamId"];
                var mercCollection = ReadMercenariesCollectionInfo(image);
                var fullTeam = mercCollection.Teams.Where(t => t.Id == teamId).FirstOrDefault();

                //var teamList = new List<int>();
                //var teamSize = playerTeamList?["_size"] ?? 0;
                //for (var i = 0; i < teamSize; i++)
                //{
                //    teamList.Add(playerTeamList["_items"][i]);
                //}

                var deadMercsList = netCacheMercenariesMap?["_DeadMercenaries"];
                var deadMercsListSize = deadMercsList["_size"];
                var deadMercs = new List<int>();
                for (var i = 0; i < deadMercsListSize; i++)
                {
                    var deadMercsItem = deadMercsList["_items"][i]?["_MercenaryIds"];
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

                //var fullTeam = BuildFullTeam(image, teamList);
                map = new MercenariesMap()
                {
                    BountyId = netCacheMercenariesMap["_BountyId"],
                    MapId = netCacheMercenariesMap["_MapId"],
                    Seed = netCacheMercenariesMap["_Seed"],
                    PlayerTeamId = teamId,
                    PlayerTeamName = fullTeam?.Name,
                    PlayerTeamMercIds = fullTeam?.Mercenaries.Select(m => m.Id).ToList(),
                    DeadMercIds = deadMercs,
                    PlayerTeam = fullTeam?.Mercenaries,
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

        public static IMercenariesCollection ReadMercenariesCollectionInfo(HearthstoneImage image)
        {
            var mercList = BuildAllMercenaries(image);

            var teamsNode = image["CollectionManager"]["s_instance"]["m_teams"];
            var teamsCount = teamsNode?["count"] ?? 0;
            var teams = new List<IMercenariesTeam>();
            for (var i = 0; i < teamsCount; i++)
            {
                var memTeam = teamsNode["valueSlots"][i];
                if (memTeam != null)
                {
                    var loadouts = new Dictionary<int, int>();
                    var memLoadouts = memTeam["m_loadouts"];
                    if (memLoadouts != null)
                    {
                        var loadoutsCount = memLoadouts["count"];
                        var keys = memLoadouts["keySlots"];
                        var values = memLoadouts["valueSlots"];
                        for (var j = 0; j < loadoutsCount; j++)
                        {
                            var mercId = keys[j]["ID"];
                            var equipment = values[j]?["m_equipmentRecord"];
                            if (equipment != null)
                            {
                                var equipmentId = equipment["m_ID"];
                                loadouts.Add(mercId, equipmentId);
                            }
                        }
                    }
                    teams.Add(new MercenariesTeam()
                    {
                        Id = memTeam["ID"],
                        Name = memTeam["m_name"],
                        Mercenaries = BuildMercenariesList(image, memTeam["m_lettuceMercs"], loadouts)
                    });
                }
            }

            var visitors = ReadMercenariesVisitorsInfo(image, true);
            return new MercenariesCollection()
            {
                Mercenaries = mercList,
                Teams = teams,
                Visitors = visitors,
            };
        }


        public static IReadOnlyList<IMercenariesVisitor> ReadMercenariesVisitorsInfo(HearthstoneImage image, bool debug = false)
        {
            var visitors = new List<IMercenariesVisitor>();
            try
            {
                var visitorsInfo = image.GetNetCacheService("NetCacheMercenariesVillageVisitorInfo")?["<VisitorStates>k__BackingField"];
                var visitorsCount = visitorsInfo?["_size"] ?? 0;
                for (var i = 0; i < visitorsCount; i++)
                {
                    var visitorInfo = visitorsInfo["_items"][i];
                    visitors.Add(new MercenariesVisitor()
                    {
                        VisitorId = visitorInfo["_VisitorId"],
                        TaskId = visitorInfo["_ActiveTaskState"]?["_TaskId"] ?? -1,
                        TaskChainProgress = visitorInfo["_TaskChainProgress"],
                        TaskProgress = visitorInfo["_ActiveTaskState"]?["_Progress"] ?? 0,
                        Status = visitorInfo["_ActiveTaskState"]?["_Status_"] ?? 0,
                    });
                }
                return visitors;
            }
            catch (Exception e)
            {
                Logger.Log("Exception when retrieving visitors " + e.Message + " " + e.StackTrace);
                return visitors;
            }
        }

        public static IMercenariesPendingTreasureSelection ReadPendingTreasureSelection(HearthstoneImage image)
        {
            if (image == null)
            {
                return null;
            }

            var netCacheMercenariesMap = image.GetNetCacheService("NetCacheLettuceMap")?["<Map>k__BackingField"];
            if (netCacheMercenariesMap == null)
            {
                return null;
            }

            var pendingTreasureSelection = netCacheMercenariesMap["_PendingTreasureSelection"];
            if (pendingTreasureSelection == null)
            {
                return null;
            }

            var options = pendingTreasureSelection["_TreasureOptions"];
            if (options == null)
            {
                return null;
            }

            var numberOfOptions = options["_size"];
            if (numberOfOptions == 0)
            {
                return null;
            }

            var optionDbfIds = new List<int>();
            for (var i = 0; i < numberOfOptions; i++)
            {
                optionDbfIds.Add((int)options["_items"][i]);
            }

            return new MercenariesPendingTreasureSelection()
            {
                MercenaryId = pendingTreasureSelection["_MercenaryId"],
                Options = optionDbfIds,
            };
        }

        private static IReadOnlyList<IMercenary> BuildAllMercenaries(HearthstoneImage image)
        {
            var allMercenaries = image["CollectionManager"]["s_instance"]["m_collectibleMercenaries"];
            var mercenaries = BuildMercenariesList(image, allMercenaries);
            return mercenaries;
        }

        private static IReadOnlyList<IMercenary> BuildMercenariesList(
            HearthstoneImage image, 
            dynamic mercenariesRoot, 
            IReadOnlyDictionary<int, int> loadouts = null)
        {
            var mercsCount = mercenariesRoot["_size"];

            var mercenaries = new List<IMercenary>();
            for (var i = 0; i < mercsCount; i++)
            {
                var mercInfo = mercenariesRoot["_items"][i];
                int mercId = mercInfo["ID"];

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

                var equipmentList = mercInfo["m_equipmentList"];
                var equipmentCount = equipmentList["_size"];
                var mercEquipments = new List<IMercenaryEquipment>();
                for (var j = 0; j < equipmentCount; j++)
                {
                    var equipment = equipmentList["_items"][j];
                    var equipmentId = equipment["ID"];
                    var equipped = loadouts == null || !loadouts.ContainsKey(mercId) ? false : loadouts[mercId] == equipmentId;
                    mercEquipments.Add(new MercenaryEquipment()
                    {
                        Id = equipmentId,
                        CardType = equipment["m_cardType"],
                        Equipped = equipped,
                        Owned = equipment["m_owned"],
                        Tier = equipment["m_tier"],
                    });
                }

                var artVariations = mercInfo["m_artVariations"];
                var artVariationsCount = artVariations["_size"];
                var premium = 0;
                var skins = new List<IMercenarySkin>();
                for (var j = 0; j < artVariationsCount; j++)
                {
                    var variation = artVariations["_items"][j];
                    premium = Math.Max(premium, variation["m_premium"] ?? 0);
                    skins.Add(new MercenarySkin()
                    {
                        Id = variation["m_record"]["m_ID"],
                        CardDbfId = variation["m_record"]["m_cardId"],
                        Default = variation["m_default"],
                        //Equipped = variation["m_equipped"],
                        Premium = variation["m_premium"],
                    });
                }

                mercenaries.Add(new Mercenary()
                {
                    Id = mercId,
                    Level = mercLevel,
                    Abilities = mercAbilities,
                    Equipments = mercEquipments,
                    TreasureCardDbfIds = new List<int>(),
                    Attack = mercInfo["m_attack"],
                    Health = mercInfo["m_health"],
                    CurrencyAmount = mercInfo["m_currencyAmount"],
                    Experience = mercInfo["m_experience"],
                    IsFullyUpgraded = mercInfo["m_isFullyUpgraded"],
                    Owned = mercInfo["m_owned"],
                    Premium = premium,
                    Rarity = mercInfo["m_rarity"],
                    Role = mercInfo["m_role"],
                    Skins = skins,
                });
            }

            var treasureAssigments = image.GetNetCacheService("NetCacheLettuceMap")?["<Map>k__BackingField"]?["_TreasureAssignmentList"]?["_TreasureAssignments"];
            if (treasureAssigments != null)
            {
                var treasuresCount = treasureAssigments["_size"];
                for (var i = 0; i < treasuresCount; i++)
                {
                    var treasure = treasureAssigments["_items"][i];
                    var mercId = treasure["_AssignedMercenary"];
                    var cardDbfId = treasure["_TreasureCard"];
                    var teamMerc = mercenaries.Find(merc => merc.Id == mercId);
                    teamMerc?.TreasureCardDbfIds.Add((int)cardDbfId);
                }
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