namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using HackF5.UnitySpy.HearthstoneLib;
    using HackF5.UnitySpy.HearthstoneLib.Detail.GameDbf;
    using HackF5.UnitySpy.HearthstoneLib.Detail.MemoryUpdate;

    internal class ArenaDraftNotifier
    {
        private List<string> lastHeroes = null;
        private List<ArenaCardOption> lastCards = null;
        private List<string> lastPackageCards = null;
        private DraftSlotType? lastStep = null;
        private DraftMode? lastMode = null;
        private ArenaClientStateType? lastClientState = null;
        private ArenaSessionState? lastSessionState = null;
        private GameType? lastGameType = null;
        private int? lastDraftSlot = null;
        private int? lastUndergroundDraftSlot = null;

        private bool sentExceptionMessage = false;
        internal void HandleStep(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var step = mindVision.GetArenaDraftStep();
                if (step != null && lastStep != step)
                {
                    result.HasUpdates = true;
                    result.ArenaDraftStep = step.Value;
                }
                lastStep = step;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleStep memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleMode(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var step = mindVision.GetArenaDraftMode();
                if (step != null && lastMode != step)
                {
                    result.HasUpdates = true;
                    result.ArenaDraftMode = step.Value;
                }
                lastMode = step;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleMode memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleClientState(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var step = mindVision.GetArenaClientState();
                if (step != null && lastClientState != step)
                {
                    result.HasUpdates = true;
                    result.ArenaClientState = step.Value;
                }
                lastClientState = step;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleMode memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleSessionState(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var step = mindVision.GetArenaSessionState();
                if (step != null && lastSessionState != step)
                {
                    result.HasUpdates = true;
                    result.ArenaSessionState = step.Value;
                }
                lastSessionState = step;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleMode memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleHeroes(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var heroes = mindVision.GetArenaHeroOptions();
                if (heroes != null && heroes.Count > 0 && !AreEqual(lastHeroes, heroes))
                {
                    result.HasUpdates = true;
                    result.ArenaHeroOptions = heroes;
                }
                lastHeroes = heroes;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleHeroes memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleCards(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var cards = mindVision.GetArenaCardOptions();
                if (cards != null && cards.Count > 0 && !AreEqual(lastCards, cards))
                {
                    result.HasUpdates = true;
                    result.ArenaCardOptions = cards;
                }
                lastCards = cards;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleHeroes memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleCardPick(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var newDraftSlot = mindVision.GetArenaCurrentDraftSlot();
                if (newDraftSlot != null && lastDraftSlot != null && newDraftSlot != lastDraftSlot)
                {
                    var pick = mindVision.GetArenaLatestCardPick();
                    result.HasUpdates = true;
                    result.ArenaLatestCardPick = pick;
                }
                lastDraftSlot = newDraftSlot;

                var newDraftSlotUnderground = mindVision.GetArenaUndergroundCurrentDraftSlot();
                if (newDraftSlotUnderground != null && lastUndergroundDraftSlot != null && newDraftSlotUnderground != lastUndergroundDraftSlot)
                {
                    var pick = mindVision.GetArenaUndergroundLatestCardPick();
                    result.HasUpdates = true;
                    result.ArenaUndergroundLatestCardPick = pick;
                }
                lastUndergroundDraftSlot = newDraftSlotUnderground;

                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleCardPick memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandlePackageCards(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var cards = mindVision.GetArenaPackageCardOptions();
                if (cards != null && !AreEqual(lastPackageCards, cards))
                {
                    result.HasUpdates = true;
                    result.ArenaPackageCardOptions = cards;
                }
                lastPackageCards = cards;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandlePackageCards memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        internal void HandleGameMode(MindVision mindVision, MemoryUpdateResult result, SceneModeEnum? currentScene)
        {
            if (currentScene != SceneModeEnum.DRAFT)
            {
                return;
            }

            try
            {
                var gameMode = mindVision.GetArenaGameMode();
                if (gameMode != null && gameMode != lastGameType)
                {
                    result.HasUpdates = true;
                    result.ArenaCurrentMode = gameMode;
                }
                lastGameType = gameMode;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception when ArenaDraft.HandleGameMode memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }


        private bool AreEqual<T>(IReadOnlyList<T> list1, IReadOnlyList<T> list2)
        {
            if ((list1 == null && list2 != null ) || (list1 !=null && list2 == null))
            {
                return false;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (var i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
