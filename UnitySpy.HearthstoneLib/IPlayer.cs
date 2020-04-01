namespace HackF5.UnitySpy.HearthstoneLib
{
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IPlayer
    {
        string Name { get; }

        int Id { get; }

        IRank Standard { get; }
        
        IRank Wild{ get; }

        int CardBackId { get; }

        IAccount Account { get; }

        IBattleTag BattleTag { get; }
    }
}