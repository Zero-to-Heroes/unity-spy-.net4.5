using System;
using System.Collections.Generic;
using System.Linq;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class MercenariesTasksUpdatedNotifier
    {
        private IReadOnlyList<IMercenariesVisitor> lastVisitors;

        internal void HandleSelection(MindVision mindVision, IMemoryUpdate result)
        {
            var visitors = mindVision.GetMercenariesVisitors();
            if (lastVisitors != null && visitors != null && visitors.Count > 0 && lastVisitors.Count > 0 && !AreEqual(visitors, lastVisitors))
            {
                result.IsMercenariesTasksUpdated = true;
                result.HasUpdates = true;
            }
            lastVisitors = visitors;
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