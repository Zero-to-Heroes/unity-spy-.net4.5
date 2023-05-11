namespace HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo
{
    using System;
    using System.Collections.Generic;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class AccountInfoReader
    {
        public static IAccountInfo ReadAccountInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var account = image["BnetPresenceMgr"]?["s_instance"]?["m_myGameAccountId"]?["<EntityId>k__BackingField"];
            return account == null ? null : new AccountInfo { Hi = account["high_"], Lo = account["low_"] };
        }
    }
}
