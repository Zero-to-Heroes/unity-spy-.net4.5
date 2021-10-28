using System.Collections.Generic;

namespace HackF5.UnitySpy.HearthstoneLib.Detail.Mercenaries
{
    internal class MercenariesCollection : IMercenariesCollection
    {
        public IReadOnlyList<IMercenary> Mercenaries { get; set; }

        public IReadOnlyList<IMercenariesTeam> Teams { get; set; }

        public IReadOnlyList<IMercenariesVisitor> Visitors { get; set; }
    }

    internal class MercenariesTeam : IMercenariesTeam
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public IReadOnlyList<IMercenary> Mercenaries { get; set; }
    }

    internal class MercenariesVisitor : IMercenariesVisitor
    {
        public int VisitorId { get; set; }

        public int TaskId { get; set; }

        public int TaskChainProgress { get; set; }

        public int TaskProgress { get; set; }

        public int Status { get; set; }
    }
}