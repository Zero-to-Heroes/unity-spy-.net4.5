namespace HackF5.UnitySpy.HearthstoneLib.Detail.Deck
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.ArenaInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Duels;
    using HackF5.UnitySpy.HearthstoneLib.Detail.DungeonInfo;
    using HackF5.UnitySpy.HearthstoneLib.Detail.Match;
    using HackF5.UnitySpy.HearthstoneLib.Detail.SceneMode;
    using JetBrains.Annotations;

    internal static class ActiveDeckReader
    {
        private static IList<SceneModeEnum?> SCENES_WITH_DECK_PICKER = new List<SceneModeEnum?> {
            SceneModeEnum.FRIENDLY,
            SceneModeEnum.TOURNAMENT
        };

        public static IDeck ReadActiveDeck([NotNull] HearthstoneImage image, long? inputSelectedDeckId)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image["DeckPickerTrayDisplay"] == null && inputSelectedDeckId == null)
            {
                return null;
            }

            if (image["CollectionManager"] == null)
            {
                return null;
            }

            long? selectedDeckId = 0;
            try
            {
                selectedDeckId = GetSelectedDeckId(image);
            }
            catch (Exception e) { }
            
            selectedDeckId = selectedDeckId ?? inputSelectedDeckId ?? 0;
            var deckFromMemory = ReadSelectedDeck(image, selectedDeckId.Value);
            if (deckFromMemory != null)
            {
                return deckFromMemory;
            }

            var matchInfo = MatchInfoReader.ReadMatchInfo(image);
            if (matchInfo == null)
            {
                return null;
            }

            switch (matchInfo.GameType)
            {
                case GameType.GT_ARENA:
                    return GetArenaDeck(image);
                case GameType.GT_CASUAL:
                    return GetCasualDeck(image);
                case GameType.GT_RANKED:
                    return GetRankedDeck(image);
                case GameType.GT_VS_AI:
                    return GetSoloDeck(image, matchInfo.MissionId);
                case GameType.GT_VS_FRIEND:
                    return GetFriendlyDeck(image);
                case GameType.GT_PVPDR:
                case GameType.GT_PVPDR_PAID:
                    return GetDuelsDeck(image);

                default: return null;
            }
        }

        private static IDeck ReadSelectedDeck(HearthstoneImage image, long selectedDeckId)
        {
            if (selectedDeckId != 0)
            {
                var deckMemory = GetDeckMemory(image);
                // We still want to fallback to the other deck detection modes if this one fails, so 
                // we don't return too early
                if (deckMemory == null)
                {
                    return null;
                }
                var deckIds = GetDeckIds(image);
                if (deckIds == null)
                {
                    return null;
                }
                var deckIndex = -1;
                // Sometimes there is a disconnect between the "count" value and the actual length
                // This seems to be because the count only counts non-0 ids
                //var count = deckMemory["count"];
                for (int i = 0; i < deckIds.Length; i++)
                {
                    if (deckIds[i] == selectedDeckId)
                    {
                        deckIndex = i;
                        break;
                    }
                }
                if (deckIndex >= 0)
                {
                    var deck = deckMemory["valueSlots"][deckIndex];
                    var fullDeck = GetDynamicDeck(deck);
                    if (fullDeck != null)
                    {
                        return fullDeck;
                    }
                }
            }
            return null;
        }

        public static IDeck ReadWhizbangDeck([NotNull] HearthstoneImage image, long whizbangDeckId)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            if (image["GameDbf"] == null
                || image["GameDbf"]["DeckTemplate"] == null
                || image["GameDbf"]["DeckTemplate"]["m_records"] == null)
            {
                return null;
            }

            var templates = image["GameDbf"]["DeckTemplate"]["m_records"];
            var size = templates["_size"];
            var items = templates["_items"];

            for (var i = 0; i < size; i++)
            {
                var template = items[i];
                if (template["m_deckId"] == whizbangDeckId)
                {
                    var deckId = template["m_deckId"];
                    var decklist = GetTemplateDeck(image, deckId);
                    return new Deck()
                    {
                        DeckList = decklist,
                    };
                }
            }

            return null;
        }


        public static IReadOnlyList<IDeck> ReadTemplateDecks(HearthstoneImage image)
        {
            var templates = image["GameDbf"]["DeckTemplate"]["m_records"]["_items"];
            var result = new List<IDeck>();
            for (var i = 0; i < templates.Length; i++)
            {
                if (templates[i] != null)
                {
                    var template = templates[i];
                    var deckId = template["m_deckId"];
                    DbfDeck dbfDeck = ActiveDeckReader.GetDbfDeck(image, deckId);
                    if (dbfDeck == null)
                    {
                        continue;
                    }

                    IList<int> decklist = ActiveDeckReader.BuildDecklistFromTopCard(image, dbfDeck.TopCardId);
                    result.Add(new Deck()
                    {
                        DeckId = deckId,
                        Id = template["m_ID"],
                        DeckList = decklist.Select(dbfId => "" + dbfId).ToList(),
                        Name = dbfDeck.Name,
                        HeroClass = template["m_classId"],
                    });
                }
            }
            return result;
        }

        private static DbfDeck GetDbfDeck(HearthstoneImage image, dynamic deckId)
        {
            var decks = image["GameDbf"]["Deck"]["m_records"];
            if (decks == null)
            {
                return null;
            }

            var items = decks["_items"];
            for (var i = 0; i < items.Length; i++)
            {
                var id = items[i]["m_ID"];
                if (id == deckId)
                {
                    if (items[i] == null)
                    {
                        continue;
                    }

                    var hasName = (items[i]?["m_name"]?["m_locValues"]?["_size"] ?? 0) > 0;
                    string name = null;
                    if (hasName)
                    {
                        name = items[i]["m_name"]["m_locValues"]["_items"][0];
                    }

                    return new DbfDeck()
                    {
                        TopCardId = items[i]["m_topCardId"],
                        Name = name,
                    };
                }
            }
            return null;
        }

        public static IList<int> GetTemplateDeck(HearthstoneImage image, int selectedDeck)
        {
            var dbf = image["GameDbf"];
            var starterDecks = dbf["Deck"]["m_records"]["_items"];
            var decklist = new List<int>();
            for (var i = 0; i < starterDecks.Length; i++)
            {
                if (starterDecks[i] != null)
                {
                    var deckId = starterDecks[i]["m_ID"];
                    if (deckId == selectedDeck)
                    {
                        var topCardId = starterDecks[i]["m_topCardId"];
                        decklist.AddRange(ActiveDeckReader.BuildDecklistFromTopCard(image, topCardId));
                    }
                }
            }
            return decklist;
        }

        public static IList<int> BuildDecklistFromTopCard(HearthstoneImage image, dynamic topCardId)
        {
            var deckList = new List<int>();
            var cardDbf = ActiveDeckReader.GetDeckCardDbf(image, topCardId);
            while (cardDbf != null)
            {
                deckList.Add(cardDbf["m_cardId"]);
                var next = cardDbf["m_nextCardId"];
                cardDbf = next == 0 ? null : ActiveDeckReader.GetDeckCardDbf(image, next);
            }
            return deckList;
        }

        public static dynamic GetDeckCardDbf(HearthstoneImage image, int cardId)
        {
            var cards = image["GameDbf"]["DeckCard"]["m_records"]["_items"];
            for (var i = 0; i < cards.Length; i++)
            {
                if (cards[i]["m_ID"] == cardId)
                {
                    return cards[i];
                }
            }

            return null;
        }

        public static long? GetSelectedDeckId(HearthstoneImage image)
        {
            var picker = image["DeckPickerTrayDisplay"];
            if (picker == null)
            {
                return null;
            }

            SceneModeEnum? currentScene = SceneModeReader.ReadSceneMode(image);
            if (currentScene == null || !SCENES_WITH_DECK_PICKER.Contains(currentScene))
            {
                return null;
            }

            var deckPicker = picker["s_instance"];
            if (deckPicker == null)
            {
                return null;
            }

            bool isSharing = deckPicker["m_usingSharedDecks"] ?? false;
            if (isSharing)
            {
                return null;
            }

            try
            {
                return deckPicker["m_selectedCustomDeckBox"]["m_deckID"];
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static dynamic GetDeckIds(HearthstoneImage image)
        {
            return image["CollectionManager"]["s_instance"]["m_decks"]["keySlots"];
        }

        private static dynamic GetDeckMemory(HearthstoneImage image)
        {
            return image["CollectionManager"]["s_instance"]["m_decks"];
        }

        private static IDeck GetDynamicDeck(dynamic deck)
        {
            if (deck == null)
            {
                return null;
            }
            var cardList = deck["m_slots"];
            var count = cardList["_size"];
            var cards = cardList["_items"];
            var deckList = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var card = cards[i];
                var copies = 0;
                var counts = card["m_count"];
                for (var j = 0; j < counts["_size"]; j++)
                {
                    copies += (int)counts["_items"][j];
                }
                for (int j = 0; j < copies; j++)
                {
                    deckList.Add(card["m_cardId"]);
                }
            }
            return new Deck
            {
                Name = deck["m_name"],
                DeckList = deckList,
                HeroCardId = deck["HeroCardID"],
                FormatType = deck["<FormatType>k__BackingField"],
            };
        }

        private static IDeck GetArenaDeck(HearthstoneImage image)
        {
            return ArenaInfoReader.ReadArenaDeck(image);
        }

        private static IDeck GetCasualDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetRankedDeck(HearthstoneImage image)
        {
            return null;
        }

        private static IDeck GetDuelsDeck(HearthstoneImage image)
        {
            return new Deck
            {
                DeckList = DuelsInfoReader.ReadDuelsInfo(image)?.DeckList?.Select(dbfId => dbfId.ToString())?.ToList(),
            };
        }

        private static IDeck GetSoloDeck(HearthstoneImage image, int missionId)
        {
            var deckList = GetSoloDeckList(image, missionId);
            if (deckList == null)
            {
                return null;
            }
            return new Deck
            {
                DeckList = deckList.Select(dbfId => dbfId.ToString()).ToList(),
            };
        }

        private static IReadOnlyList<int> GetSoloDeckList(HearthstoneImage image, int missionId)
        {
            var dungeonInfo = DungeonInfoReader.ReadCollection(image);
            switch (missionId)
            {
                case 2663:
                    return dungeonInfo?[DungeonKey.DungeonRun]?.DeckList;
                case 2706:
                case 2821:
                    return dungeonInfo?[DungeonKey.MonsterHunt]?.DeckList;
                case 2890:
                    return dungeonInfo?[DungeonKey.RumbleRun]?.DeckList;
                case 3005:
                case 3188:
                case 3189:
                case 3190:
                case 3191:
                case 3236:
                    return dungeonInfo?[DungeonKey.DalaranHeist]?.DeckList;
                case 3328:
                case 3329:
                case 3330:
                case 3331:
                case 3332:
                case 3359:
                    return dungeonInfo?[DungeonKey.DalaranHeistHeroic]?.DeckList;
                case 3428:
                case 3429:
                case 3430:
                case 3431:
                case 3432:
                case 3438:
                    return dungeonInfo?[DungeonKey.TombsOfTerror]?.DeckList;
                case 3433:
                case 3434:
                case 3435:
                case 3436:
                case 3437:
                case 3439:
                    return dungeonInfo?[DungeonKey.TombsOfTerrorHeroic]?.DeckList;
                // Jaina
                case 3724:
                case 3725:
                case 3726:
                case 3727:
                case 3728:
                case 3729:
                case 3730:
                case 3731:
                // Rexxar
                case 3766:
                case 3767:
                case 3768:
                case 3769:
                case 3770:
                case 3771:
                case 3772:
                case 3773:
                // Garrosh
                case 3793:
                case 3794:
                case 3795:
                case 3796:
                case 3797:
                case 3798:
                case 3799:
                case 3800:
                // Uther
                case 3810:
                case 3811:
                case 3812:
                case 3813:
                case 3814:
                case 3815:
                case 3816:
                case 3817:
                // Anduin
                case 3825:
                case 3826:
                case 3827:
                case 3828:
                case 3829:
                case 3830:
                case 3831:
                case 3832:
                case 3833:
                // Valeera
                case 3851:
                case 3852:
                case 3853:
                case 3854:
                case 3855:
                case 3856:
                case 3857:
                case 3858:
                // Thrall
                case 3891:
                case 3892:
                case 3893:
                case 3894:
                case 3895:
                case 3896:
                case 3897:
                case 3898:
                // Malfurion
                case 3923:
                case 3925:
                case 3926:
                case 3927:
                case 3928:
                case 3929:
                case 3932:
                case 3933:
                // Gul'dan
                case 4085:
                case 4086:
                case 4087:
                case 4088:
                case 4089:
                case 4090:
                case 4091:
                case 4092:
                // Illidan
                case 4137:
                case 4138:
                case 4139:
                case 4140:
                case 4141:
                case 4142:
                case 4143:
                case 4144:
                    var dungeonDetails = dungeonInfo?[DungeonKey.BookOfHeroes];
                    // When switching adventures, the memory info is not refreshed
                    if (dungeonDetails == null || dungeonDetails.ScenarioId != missionId)
                    {
                        return null;
                    }
                    return dungeonDetails.DeckList;
                // Rokara
                case 3839:
                case 3840:
                case 3841:
                case 3842:
                case 3843:
                case 3844:
                case 3845:
                case 3846:
                // Xyrella
                case 3991:
                case 3992:
                case 3993:
                case 3994:
                case 3995:
                case 3996:
                case 3997:
                case 3998:
                // Guff
                case 4074:
                case 4075:
                case 4076:
                case 4077:
                case 4078:
                case 4079:
                case 4080:
                case 4081:
                // Kurtrus
                case 4105:
                case 4113:
                case 4114:
                case 4115:
                case 4116:
                case 4117:
                case 4118:
                case 4119:
                // Tamsin
                case 4160:
                case 4161:
                case 4162:
                case 4163:
                case 4164:
                case 4165:
                case 4166:
                case 4167:
                // Cariel
                case 4307:
                case 4308:
                case 4309:
                case 4310:
                case 4311:
                case 4312:
                case 4313:
                case 4314:
                // Scabbs
                case 4478:
                case 4479:
                case 4481:
                case 4482:
                case 4483:
                case 4484:
                case 4485:
                case 4486:
                // Tavish
                case 4731:
                case 4732:
                case 4733:
                case 4734:
                case 4735:
                case 4736:
                case 4737:
                case 4738:
                // Bru'kan
                case 4844:
                case 4845:
                case 4846:
                case 4847:
                case 4848:
                case 4849:
                case 4850:
                case 4851:
                    var mercenaryDetails = dungeonInfo?[DungeonKey.BookOfMercenaries];
                    // When switching adventures, the memory info is not refreshed
                    if (mercenaryDetails == null || mercenaryDetails.ScenarioId != missionId)
                    {
                        return null;
                    }
                    return mercenaryDetails.DeckList;
            }

            Logger.Log($"Unsupported scenario id: {missionId}.");
            return null;
        }

        private static IDeck GetFriendlyDeck(HearthstoneImage image)
        {
            return null;
        }
    }
}
