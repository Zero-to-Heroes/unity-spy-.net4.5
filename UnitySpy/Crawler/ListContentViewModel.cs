namespace HackF5.UnitySpy.Crawler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ListContentViewModel
    {
        private readonly IList list;

        public ListContentViewModel(IList list)
        {
            this.list = list;
            this.Items = this.list.Cast<object>().Select((o, i) => new ListItemViewModel(o, i)).ToArray();
        }

        //public delegate ListContentViewModel Factory(IList list);

        public IEnumerable<ListItemViewModel> Items { get; }

        public void DumpMemory(string previous, List<string> dump, List<uint> addresses)
        {
            //var currentNode = previous + Crawler.SEPARATOR + this.instance.TypeDefinition.Name;
            //Console.WriteLine(currentNode);
            foreach (var field in this.Items)
            {
                field.DumpMemory(previous + Crawler.SEPARATOR, dump, addresses);
            }
        }
    }
}