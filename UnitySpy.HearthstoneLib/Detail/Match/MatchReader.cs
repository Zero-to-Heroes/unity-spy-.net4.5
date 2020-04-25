// ReSharper disable StringLiteralTypo
namespace HackF5.UnitySpy.HearthstoneLib.Detail.Match
{
    using System;

    internal static class MatchInfoReader
    {
        public static IMatchInfo ReadMatchInfo(HearthstoneImage image)
        {
            var matchInfo = new MatchInfo();
            var gameState = image["GameState"]?["s_instance"];
            var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];

            if (gameState != null)
            {
                var playerIds = gameState["m_playerMap"]?["keySlots"];
                var players = gameState["m_playerMap"]?["valueSlots"];
                for (var i = 0; i < playerIds.Length; i++)
                {
                    if (players[i]?.TypeDefinition?.Name != "Player")
                    {
                        continue;
                    }

                    var medalInfo = players[i]?["m_medalInfo"];
                    var standard = MatchInfoReader.BuildRank(image, medalInfo?["m_currMedalInfo"]);
                    var wild = MatchInfoReader.BuildRank(image, medalInfo?["m_currWildMedalInfo"]);
                    //var standardMedalInfo = medalInfo?["m_currMedalInfo"];
                    //var wildMedalInfo = medalInfo?["m_currWildMedalInfo"];
                    var playerName = players[i]?["m_name"];
                    //var standardRank = standardMedalInfo != null ? MatchInfoReader.GetRankValue(image, standardMedalInfo) : -1;
                    //var standardLegendRank = standardMedalInfo?["legendIndex"] ?? 0;
                    //var wildRank = wildMedalInfo != null ? MatchInfoReader.GetRankValue(image, wildMedalInfo) : -1;
                    //var wildLegendRank = wildMedalInfo?["legendIndex"] ?? 0;
                    var cardBack = players[i]?["m_cardBackId"] ?? -1;
                    var playerId = playerIds[i] ?? -1;
                    var side = (Side)(players[i]?["m_side"] ?? 0);
                    var accountId = players[i]?["m_gameAccountId"];
                    var account = new Account { Hi = accountId?["m_hi"] ?? 0, Lo = accountId?["m_lo"] ?? 0 };
                    var battleTag = MatchInfoReader.GetBattleTag(image, account);

                    switch (side)
                    {
                        case Side.FRIENDLY:
                            {
                                dynamic netCacheMedalInfo = null;
                                if (netCacheValues != null)
                                {
                                    foreach (var netCache in netCacheValues)
                                    {
                                        if (netCache?.TypeDefinition.Name != "NetCacheMedalInfo")
                                        {
                                            continue;
                                        }

                                        netCacheMedalInfo = netCache;
                                        break;
                                    }
                                }

                                var standardStars = netCacheMedalInfo?["<Standard>k__BackingField"]?["<Stars>k__BackingField"] ?? -1;
                                var wildStars = netCacheMedalInfo?["<Wild>k__BackingField"]?["<Stars>k__BackingField"] ?? -1;
                                matchInfo.LocalPlayer = new Player
                                {
                                    Id = playerId,
                                    Name = playerName,
                                    Standard = standard,
                                    Wild = wild,
                                    //StandardRank = standardRank,
                                    //StandardLegendRank = standardLegendRank,
                                    //StandardStars = standardStars,
                                    //WildRank = wildRank,
                                    //WildLegendRank = wildLegendRank,
                                    //WildStars = wildStars,
                                    CardBackId = cardBack,
                                    Account = account,
                                    BattleTag = battleTag,
                                };

                                break;
                            }

                        case Side.OPPOSING:
                            matchInfo.OpposingPlayer = new Player
                            {
                                Id = playerId,
                                Name = playerName,
                                Standard = standard,
                                Wild = wild,
                                CardBackId = cardBack,
                                Account = account,
                                BattleTag = battleTag,
                            };

                            break;

                        case Side.NEUTRAL:
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown side {side}.");
                    }
                }
            }

            if ((matchInfo.LocalPlayer == null) || (matchInfo.OpposingPlayer == null))
            {
                return null;
            }

            var gameMgr = image.GetService("GameMgr");
            if (gameMgr != null)
            {
                matchInfo.MissionId = gameMgr["m_missionId"] ?? -1;
                matchInfo.GameType = (GameType)(gameMgr["m_gameType"] ?? 0);
                matchInfo.FormatType = (GameFormat)(gameMgr["m_formatType"] ?? 0);
            }

            if (netCacheValues != null)
            {
                foreach (var netCache in netCacheValues)
                {
                    if (netCache?.TypeDefinition?.Name != "NetCacheRewardProgress")
                    {
                        continue;
                    }

                    matchInfo.RankedSeasonId = netCache["<Season>k__BackingField"] ?? -1;
                    break;
                }
            }

            return matchInfo;
        }

        private static BattleTag GetBattleTag(HearthstoneImage image, IAccount account)
        {
            var gameAccounts = image["BnetPresenceMgr"]?["s_instance"]?["m_gameAccounts"];
            if (gameAccounts == null)
            {
                return null;
            }

            var keys = gameAccounts["keySlots"];
            for (var i = 0; i < keys.Length; i++)
            {
                if ((keys[i]?["m_hi"] != account.Hi) || (keys[i]?["m_lo"] != account.Lo))
                {
                    continue;
                }

                var bTag = gameAccounts["valueSlots"]?[i]?["m_battleTag"];
                return new BattleTag
                {
                    Name = bTag?["m_name"],
                    Number = bTag?["m_number"] ?? -1,
                };
            }

            return null;
        }

        private static Rank BuildRank(HearthstoneImage image, dynamic medalInfo)
        {
            var internalLeagueId = medalInfo?["leagueId"] ?? -1;
            var starLevel = medalInfo?["starLevel"] ?? -1;
            var legendRank = medalInfo?["legendIndex"] ?? 0;
            var leagueRankInfo = MatchInfoReader.GetLeagueRank(image, internalLeagueId, starLevel);
            return new Rank
            {
                LeagueId = leagueRankInfo?.LeagueId ?? -1,
                RankValue = leagueRankInfo?.Rank ?? -1,
                LegendRank = legendRank,
            };
        }
    

        private static dynamic GetLeagueRank(HearthstoneImage image, int internalLeagueId, int starLevel) { 
            var leagueRankRecord = MatchInfoReader.GetLeagueRankRecord(image, internalLeagueId, starLevel);
            if (leagueRankRecord == null)
            {
                return null;
            }

            var locValues = leagueRankRecord["m_rankName"]?["m_locValues"]?["_items"];
            foreach (string value in locValues)
            {
                if (value == null)
                {
                    continue;
                }

                if (value.Contains(" "))
                {
                    var leagueName = value.Split(' ')[0];
                    var leagueId = MatchInfoReader.LeagueNameToId(leagueName);
                    int.TryParse(value.Split(' ')[1], out int rank);
                    return new
                    {
                        Rank = rank,
                        LeagueId = leagueId,
                    };
                }
            }

            return null;
        }

        private static int LeagueNameToId(string leagueName)
        {
            switch (leagueName)
            {
                case "Bronze": return 5;
                case "Silver": return 4;
                case "Gold": return 3;
                case "Platinum": return 2;
                case "Diamond": return 1;
            }
            return -1;
        }

        private static dynamic GetLeagueRankRecord(HearthstoneImage image, int leagueId, int starLevel)
        {
            var rankManager = image["RankMgr"]?["s_instance"];
            if (rankManager == null)
            {
                return null;
            }

            var rankConfig = rankManager["m_rankConfigByLeagueAndStarLevel"];
            if (rankConfig == null)
            {
                return null;
            }

            var leagueKeys = rankConfig["keySlots"];
            var leagueValues = rankConfig["valueSlots"];
            for (var i = 0; i < leagueKeys.Length; i++)
            {
                if (leagueKeys[i] != leagueId)
                {
                    continue;
                }

                var starLevelMap = leagueValues[i];
                if (starLevelMap == null)
                {
                    return null;
                }

                var starLevelKeys = starLevelMap["keySlots"];
                var starLevelValues = starLevelMap["valueSlots"];
                for (var j = 0; j < starLevelKeys.Length; j++)
                {
                    if (starLevelKeys[j] != starLevel)
                    {
                        continue;
                    }

                    return starLevelValues[j];
                }
            }

            return null;
        }

        //private static int GetRankValue(HearthstoneImage image, dynamic medalInfo)
        //{
        //    var leagueId = medalInfo?["leagueId"];
        //    var starLevel = medalInfo?["starLevel"];
        //    var leagueRankRecord = MatchInfoReader.GetLeagueRankRecord(image, leagueId, starLevel);
        //    if (leagueRankRecord == null)
        //    {
        //        return 0;
        //    }

        //    var locValues = leagueRankRecord["m_medalText"]?["m_locValues"]?["_items"];
        //    foreach (var value in locValues)
        //    {
        //        if (value == null)
        //        {
        //            continue;
        //        }

        //        if (int.TryParse(value, out int rank))
        //        {
        //            return rank;
        //        }
        //    }

        //    return 0;
        //}
    }
}