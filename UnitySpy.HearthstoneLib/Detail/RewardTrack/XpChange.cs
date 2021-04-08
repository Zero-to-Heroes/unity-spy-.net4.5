namespace HackF5.UnitySpy.HearthstoneLib.Detail.RewardTrack
{
    internal class XpChange : IXpChange
    {
        public int CurrentLevel { get; set; }

        public int CurrentXp { get; set; }

        public int PreviousLevel { get; set; }

        public int PreviousXp { get; set; }

        public int RewardSourceId { get; set; }

        public int RewardSourceType { get; set; }

        override
        public bool Equals(object obj)
        {
            if (!(obj is XpChange))
            {
                return false;
            }

            var other = obj as XpChange;
            if (other == null)
            {
                return false;
            }

            return this.CurrentLevel == other.CurrentLevel
                && this.CurrentXp == other.CurrentXp
                && this.PreviousLevel == other.PreviousLevel
                && this.PreviousXp == other.PreviousXp
                && this.RewardSourceId == other.RewardSourceId
                && this.RewardSourceType == other.RewardSourceType;
        }
    }
}