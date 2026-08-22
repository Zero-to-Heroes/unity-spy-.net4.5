namespace HackF5.UnitySpy.HearthstoneLib.Detail.GameMenu
{
    using System;
    using JetBrains.Annotations;

    internal static class GameMenuReader
    {
        public static bool ReadGameMenuOpen([NotNull] HearthstoneImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            try
            {
                return IsMenuShown(image, "GameMenu") || IsMenuShown(image, "OptionsMenu");
            }
            catch (Exception)
            {
                // Don't log the error to avoid triggering memory reading resets
                return false;
            }
        }

        private static bool IsMenuShown(HearthstoneImage image, string className)
        {
            try
            {
                var instance = image[className]?["s_instance"];
                if (instance == null)
                {
                    return false;
                }

                // ButtonListMenu / OptionsMenu use m_isShown. UIBPopup still has m_shown, but GameMenu
                // does not inherit UIBPopup — reading m_shown threw and the overlay never hid.
                bool? shown = instance["m_isShown"];
                return shown == true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
