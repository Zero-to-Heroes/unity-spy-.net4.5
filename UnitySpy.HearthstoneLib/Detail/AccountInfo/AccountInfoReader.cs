namespace HackF5.UnitySpy.HearthstoneLib.Detail.AccountInfo
{
    using System;
    using HackF5.UnitySpy.HearthstoneLib;
    using JetBrains.Annotations;

    internal static class AccountInfoReader
    {
        public static AccountInfo ReadAccountInfo([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var service = image["BnetPresenceMgr"]?["s_instance"];
            var account = service?["m_myGameAccountId"]?["<EntityId>k__BackingField"];
            if (account == null)
            {
                return null;
            }

            return new AccountInfo
            {
                Hi = account["high_"],
                Lo = account["low_"],
                BattleTag = ReadLocalBattleTag(service?["m_myPlayer"]),
            };
        }

        /// <summary>
        /// The local player's displayed tag is on <c>m_hsGameAccount</c>
        /// (<c>BnetPlayer.GetBestName</c> when <c>IsMyPlayer()</c>).
        /// <c>m_account.m_battleTag</c> can be missing. Concatenating null name and number
        /// with <c>dynamic</c> produces <c>"#"</c>, including after Battle.net disconnect
        /// clears <c>m_myPlayer</c>.
        /// </summary>
        private static string ReadLocalBattleTag(dynamic myPlayer)
        {
            if (myPlayer == null)
            {
                return null;
            }

            return TryFormatBattleTag(() => myPlayer["m_hsGameAccount"]?["m_battleTag"])
                ?? TryFormatBattleTag(() => myPlayer["m_account"]?["m_battleTag"]);
        }

        private static string TryFormatBattleTag(Func<dynamic> read)
        {
            try
            {
                return FormatBattleTag(read());
            }
            catch (Exception e)
            {
                Logger.Log("Could not read battle tag: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// <c>m_number</c> is a string on current clients and an int on older ones.
        /// </summary>
        private static string FormatBattleTag(dynamic battleTagObj)
        {
            if (battleTagObj == null)
            {
                return null;
            }

            var name = AsNonEmptyString(battleTagObj["m_name"]);
            var number = AsNonEmptyString(battleTagObj["m_number"]);
            if (name == null || number == null)
            {
                return null;
            }

            return name + "#" + number;
        }

        private static string AsNonEmptyString(object value)
        {
            if (value == null)
            {
                return null;
            }

            var text = value as string ?? value.ToString();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        public static BnetRegion? ReadCurrentRegion([NotNull] HearthstoneImage image)
        {
            var service = image.GetService("Network");
            var networkState = service?["m_state"];
            if (networkState == null)
            {
                return null;
            }

            var region = networkState["<CachedRegion>k__BackingField"];
            if (region == null || region <= 0)
            {
                return null;
            }
            return (BnetRegion)region;
        }
    }
}
