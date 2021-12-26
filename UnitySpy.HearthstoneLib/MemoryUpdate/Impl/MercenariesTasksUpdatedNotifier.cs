using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class MercenariesTasksUpdatedNotifier
    {
        private IReadOnlyList<IMercenariesVisitor> lastVisitors;

        private bool sentExceptionMessage = false;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var visitors = mindVision.GetMercenariesVisitors();
                if (lastVisitors != null && visitors != null && visitors.Count > 0 && lastVisitors.Count > 0 && !AreEqual(visitors, lastVisitors))
                {
                    result.IsMercenariesTasksUpdated = true;
                    result.HasUpdates = true;
                }
                lastVisitors = visitors;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in MercenariesTasksUpdatedNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        private bool AreEqual(IReadOnlyList<IMercenariesVisitor> first, IReadOnlyList<IMercenariesVisitor> second)
        {
            if ((first == null && second != null) || (first != null && second == null))
            {
                return false;
            }

            if (first.Count != second.Count)
            {
                return false;
            }

            for (int i = 0; i < first.Count; i++)
            {
                var firstVisitor = first[i];
                var secondVisitor = second[i];
                if (!firstVisitor.Equals(secondVisitor))
                {
                    return false;
                }
            }

            return true;
        }
    }
}