namespace HackF5.UnitySpy.HearthstoneLib
{
    public interface IXpChange
    {
        int CurrentLevel { get; }

        int CurrentXp { get; }

        int PreviousLevel { get; }

        int PreviousXp { get; }

        int RewardSourceId { get; }

        int RewardSourceType { get; }
    }
}
