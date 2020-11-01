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

            if (gameState != null && gameState["m_playerMap"] != null)
            {
                var playerIds = gameState["m_playerMap"]["keySlots"];
                var players = gameState["m_playerMap"]["valueSlots"];
                for (var i = 0; i < playerIds.Length; i++)
                {
                    if (players[i] == null || players[i].TypeDefinition?.Name != "Player")
                    {
                        continue;
                    }

                    var side = (Side)(players[i]?["m_side"] ?? 0);
                    var playerId = playerIds[i] ?? -1;
                    var player = ReadPlayerInfo(image, players[i], playerId);

                    switch (side)
                    {
                        case Side.FRIENDLY:
                                matchInfo.LocalPlayer = player;
                                break;

                        case Side.OPPOSING:
                            matchInfo.OpposingPlayer = player;
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

            //if (netCacheValues != null)
            //{
            //    foreach (var netCache in netCacheValues)
            //    {
            //        if (netCache?.TypeDefinition?.Name != "NetCacheRewardProgress")
            //        {
            //            continue;
            //        }

            //        matchInfo.RankedSeasonId = netCache["<Season>k__BackingField"] ?? -1;
            //        break;
            //    }
            //}

            var boardDbId = MatchInfoReader.RetrieveBoardInfo(image);
            matchInfo.BoardDbId = boardDbId;

            return matchInfo;
        }

        private static Player ReadPlayerInfo(HearthstoneImage image, dynamic player, int playerId)
        {
            var medalInfo = player["m_medalInfo"];
            var standard = medalInfo != null ? MatchInfoReader.BuildRank(image, medalInfo["m_currMedalInfo"], true) : null;
            var wild = medalInfo != null ? MatchInfoReader.BuildRank(image, medalInfo["m_currWildMedalInfo"], false) : null;
            var playerName = player["m_name"];
            var cardBack = player["m_cardBackId"] ?? -1;
            var accountId = player["m_gameAccountId"];
            var account = new Account { Hi = accountId?["m_hi"] ?? 0, Lo = accountId?["m_lo"] ?? 0 };
            var battleTag = MatchInfoReader.GetBattleTag(image, account);
            return new Player
            {
                Id = playerId,
                Name = playerName,
                Standard = standard,
                Wild = wild,
                CardBackId = cardBack,
                Account = account,
                BattleTag = battleTag,
            };
        }

        private static int RetrieveBoardInfo(HearthstoneImage image)
        {
            var boardService = image["Board"];
            return boardService?["s_instance"]?["m_boardDbId"] ?? -1;
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

        private static Rank BuildRank(HearthstoneImage image, dynamic medalInfo, bool isStandard)
        {
            var internalLeagueId = medalInfo?["leagueId"] ?? -1;
            var starLevel = medalInfo?["starLevel"] ?? -1;
            var legendRank = medalInfo?["legendIndex"] ?? 0;
            var leagueRankInfo = MatchInfoReader.GetLeagueRank(image, internalLeagueId, starLevel);
            //var netCacheValues = image.GetService("NetCache")?["m_netCache"]?["valueSlots"];
            //dynamic netCacheMedalInfo = null;
            //if (netCacheValues != null)
            //{
            //    foreach (var netCache in netCacheValues)
            //    {
            //        if (netCache?.TypeDefinition.Name != "NetCacheMedalInfo")
            //        {
            //            continue;
            //        }

            //        netCacheMedalInfo = netCache;
            //        break;
            //    }
            //}

            //var netCacheInfo = isStandard ? netCacheMedalInfo?["<Standard>k__BackingField"] : netCacheMedalInfo?["<Wild>k__BackingField"];

            return new Rank
            {
                LeagueId = leagueRankInfo?.LeagueId ?? -1,
                RankValue = leagueRankInfo?.Rank ?? -1,
                LegendRank = legendRank,
            };
        }


        private static dynamic GetLeagueRank(HearthstoneImage image, int internalLeagueId, int starLevel)
        {
            var leagueRankRecord = MatchInfoReader.GetLeagueRankRecord(image, internalLeagueId, starLevel);
            if (leagueRankRecord == null)
            {
                return null;
            }

            string cheatName = leagueRankRecord["m_cheatName"];
            if (cheatName == null || cheatName.Length == 0)
            {
                return null;
            }

            string leagueName = MatchInfoReader.ExtractLeagueName(cheatName);
            if (leagueName == null || leagueName.Length == 0)
            {
                return null;
            }

            var splitRank = cheatName.Split(leagueName.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
            int.TryParse(splitRank, out int rank);
            return new
            {
                LeagueId = LeagueNameToId(leagueName),
                Rank = rank,
            };
        }

        private static string ExtractLeagueName(string cheatName)
        {
            if (cheatName == null)
            {
                return null;
            }

            if (cheatName.Contains("bronze"))
            {
                return "bronze";
            }
            else if (cheatName.Contains("silver"))
            {
                return "silver";
            }
            else if (cheatName.Contains("gold"))
            {
                return "gold";
            }
            else if (cheatName.Contains("plat"))
            {
                return "platinum";
            }
            else if (cheatName.Contains("diamond"))
            {
                return "diamond";
            }
            return null;
        }

        private static int LeagueNameToId(string leagueName)
        {
            switch (leagueName)
            {
                case "bronze": return 5;
                case "silver": return 4;
                case "gold": return 3;
                case "platinum": return 2;
                case "diamond": return 1;
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

    }
}