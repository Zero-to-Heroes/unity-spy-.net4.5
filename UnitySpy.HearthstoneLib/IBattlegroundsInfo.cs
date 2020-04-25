namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IBattlegroundsInfo
    {
        int Rating { get; }

        IBattlegroundsGame Game { get; }
    }
}