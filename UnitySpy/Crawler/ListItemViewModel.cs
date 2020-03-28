using System;
using System.Collections.Generic;

namespace HackF5.UnitySpy.Crawler
{
    public class ListItemViewModel
    {
        public ListItemViewModel(object item, int index)
        {
            this.Item = item;
            this.Index = index;
        }

        //public delegate ListItemViewModel Factory(object item, int index);

        public object Item { get; }

        public int Index { get; }

        public void DumpMemory(string previous, List<string> dump, List<uint> addresses)
        {
            var currentNode = previous + "items[" + this.Index + "] = ";
            //Console.WriteLine(currentNode);
            Crawler.DumpMemory(this.Item, currentNode, dump, addresses);
            //foreach (var field in this.InstanceFields)
            //{
            //    field.DumpMemory(currentNode);
            //}
        }
    }
}