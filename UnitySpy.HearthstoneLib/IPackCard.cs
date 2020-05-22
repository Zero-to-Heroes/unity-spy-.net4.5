namespace HackF5.UnitySpy.HearthstoneLib
{
    public interface IPackCard
    {
        string CardId { get; }

        bool IsNew { get; }

        bool Premium { get; }

        bool Revealed { get; }


    }
}
