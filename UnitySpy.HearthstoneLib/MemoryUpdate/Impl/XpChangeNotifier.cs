using System;
using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class XpChangeNotifier
    {
        private IReadOnlyList<IXpChange> lastXpChanges;

        private bool sentExceptionMessage = false;

        internal void HandleXpChange(MindVision mindVision, IMemoryUpdate result)
        {
            try
            {
                var xpChanges = mindVision.GetXpChanges();
                if (xpChanges != null && xpChanges.Count > 0 && !AreEqual(lastXpChanges, xpChanges))
                {
                    result.HasUpdates = true;
                    result.XpChanges = xpChanges;
                }
                lastXpChanges = xpChanges;
                sentExceptionMessage = false;
            }
            catch (Exception e)
            {
                if (!sentExceptionMessage)
                {
                    Logger.Log("Exception in XpChangeNotifier memory read " + e.Message + " " + e.StackTrace);
                    sentExceptionMessage = true;
                }
            }
        }

        private bool AreEqual(IReadOnlyList<IXpChange> lastXpChanges, IReadOnlyList<IXpChange> xpChanges)
        {
            if (lastXpChanges == null)
            {
                return false;
            }

            if (lastXpChanges.Count != xpChanges.Count)
            {
                return false;
            }

            for (var i = 0; i < xpChanges.Count; i++)
            {
                if (!lastXpChanges[i].Equals(xpChanges[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}