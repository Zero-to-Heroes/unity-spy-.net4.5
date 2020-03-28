namespace HackF5.UnitySpy.Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public class ManagedObjectInstanceContentViewModel
    {
        private readonly IManagedObjectInstance instance;

        public ManagedObjectInstanceContentViewModel(
            [NotNull] IManagedObjectInstance instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.InstanceFields = this.instance.TypeDefinition.Fields
                .Where(f => !f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                .Select(f => new InstanceFieldViewModel(f, instance))
                .ToArray();
        }

        //public delegate ManagedObjectInstanceContentViewModel Factory(IManagedObjectInstance instance);

        public IEnumerable<InstanceFieldViewModel> InstanceFields { get; }

        public void DumpMemory(string previous, List<string> dump, List<uint> addresses)
        {
            if (addresses.Contains(this.instance.GetAddress()))
            {
                //Console.WriteLine("Already handled " + previous + " " + this.instance.GetAddress() + " " + addresses.Count);
                return;
            }
            //Console.WriteLine("Handling " + this.instance.GetAddress() + " with fields " + this.InstanceFields.Count());
            addresses.Add(this.instance.GetAddress());
            var currentNode = previous + this.instance.TypeDefinition.Name;
            //Console.WriteLine(currentNode);
            foreach (var field in this.InstanceFields)
            {
                //Console.WriteLine("Considering field " + field.Name);
                field.DumpMemory(currentNode + Crawler.SEPARATOR, dump, addresses);
            }
        }
    }
}