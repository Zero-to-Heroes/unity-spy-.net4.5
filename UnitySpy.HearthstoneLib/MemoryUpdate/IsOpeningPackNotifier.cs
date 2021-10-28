namespace HackF5.UnitySpy.HearthstoneLib.MemoryUpdate
{
    public class IsOpeningPackNotifier
    {
        private bool lastIsOpeningPack;

        internal void HandleIsOpeningPack(MindVision mindVision, IMemoryUpdate result)
        {
            var isOpeningPack = mindVision.IsOpeningPack();
            // There are fewer unopened packs, which means we opened one, so we can trigger the update
            // to have the app request the full pack info
            if (isOpeningPack && isOpeningPack != lastIsOpeningPack)
            {
                result.HasUpdates = true;
                result.IsOpeningPack = isOpeningPack;
            }
            lastIsOpeningPack = isOpeningPack;
        }
    }
}