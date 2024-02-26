namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using HackF5.UnitySpy.HearthstoneLib;

    internal class ArenaDraftNotifier
    {
        private List<string> lastHeroes = null;
        private List<string> lastCards = null;
        private DraftSlotType? lastStep = null;

        private bool sentExceptionMessage = false;
        internal void HandleStep(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
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

        internal void HandleHeroes(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
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

        internal void HandleCards(MindVision mindVision, IMemoryUpdate result, SceneModeEnum? currentScene)
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


        private bool AreEqual(IReadOnlyList<string> list1, IReadOnlyList<string> list2)
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
