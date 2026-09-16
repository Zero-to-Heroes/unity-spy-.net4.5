namespace HackF5.UnitySpy.HearthstoneLib.Tests
{
    using HackF5.UnitySpy.HearthstoneLib.Detail.InputManager;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Process-free tests for hover identity. CardMouseOverNotifier skips a MemoryUpdate when
    /// <see cref="MousedOverCard.Equals"/> is true, so unidentified secrets (same CardId/Zone/Side)
    /// must differ by EntityId or the secrets helper stays on the first hovered slot.
    /// </summary>
    [TestClass]
    public class MousedOverCardEqualsTests
    {
        // Zone.SECRET = 7, Side.OPPOSING = 2 — same values Firestone uses for hung opponent secrets.
        private const int SecretZone = 7;
        private const int OpposingSide = 2;

        [TestMethod]
        public void Equals_SameCardIdZoneSide_DifferentEntityId_AreNotEqual()
        {
            var first = Secret("", 10);
            var second = Secret("", 11);

            Assert.IsFalse(first.Equals(second));
            Assert.IsTrue(ShouldEmitHoverChange(first, second));
        }

        [TestMethod]
        public void Equals_SameCardIdZoneSideEntityId_AreEqual()
        {
            var first = Secret("", 10);
            var sameSlot = Secret("", 10);

            Assert.IsTrue(first.Equals(sameSlot));
            Assert.IsFalse(ShouldEmitHoverChange(first, sameSlot));
        }

        [TestMethod]
        public void Notifier_MouseLeave_CountsAsChange()
        {
            var lastCard = Secret("", 10);

            Assert.IsTrue(ShouldEmitHoverChange(lastCard, null));
        }

        private static MousedOverCard Secret(string cardId, int entityId)
        {
            return new MousedOverCard
            {
                CardId = cardId,
                Zone = SecretZone,
                Side = OpposingSide,
                EntityId = entityId,
            };
        }

        /// <summary>
        /// Same condition as CardMouseOverNotifier.HandleMouseOver.
        /// </summary>
        private static bool ShouldEmitHoverChange(MousedOverCard lastCard, MousedOverCard mousedOverCard)
        {
            return (mousedOverCard == null && lastCard != null)
                || (mousedOverCard != null && !mousedOverCard.Equals(lastCard));
        }
    }
}
