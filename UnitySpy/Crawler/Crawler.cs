namespace HackF5.UnitySpy.Crawler
{
    using JetBrains.Annotations;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class Crawler
    {
        public static readonly string SEPARATOR = "-->";
        private readonly IAssemblyImage image;

        //private readonly TypeDefinitionViewModel.Factory typeDefinitionFactory;

        public delegate Crawler Factory(IAssemblyImage image);

        public List<TypeDefinitionViewModel> Types = new List<TypeDefinitionViewModel>();

        public Crawler(
            [NotNull] IAssemblyImage image)
        {
            this.image = image ?? throw new ArgumentNullException(nameof(image));

            //this.typeDefinitionFactory =
            //    typeDefinitionFactory ?? throw new ArgumentNullException(nameof(typeDefinitionFactory));
        }

        public void DumpMemory()
        {
            this.Types.Clear();

            foreach (var type in this.image.TypeDefinitions.OrderBy(td => td.FullName).Select(td => new TypeDefinitionViewModel(td)))
            {
                this.Types.Add(type);
            }

            Console.WriteLine("Starting Dumpp");
            var dump = new List<string>();
            var addresses = new List<uint>();
            foreach (var type in this.Types)
            {
                type.DumpMemory(dump, addresses);
            }

            foreach (var memoryInfo in dump)
            {
                Console.WriteLine(memoryInfo);
            }
        }

        public static void DumpMemory(object value, string currentNode, List<string> dump, List<uint> addresses)
        {
            //if (Regex.Matches(currentNode, Crawler.SEPARATOR).Count > 4)
            //{
            //    dump.Add("Too deep, returning " + currentNode);
            //    return;
            //}
            //dump.Add("is value? " + (value is IManagedObjectInstance));
            //Console.WriteLine("Dumping memory value for " + value + " in " + currentNode);
            if (value is ITypeDefinition type)
            {
                var model = new TypeDefinitionContentViewModel(type);
                model.DumpMemory(currentNode, dump, addresses, value as dynamic);
                //this.Content = model;
            }
            else if (value is TypeDefinitionContentViewModel contentModel)
            {
                contentModel.DumpMemory(currentNode, dump, addresses, value as dynamic);
            }
            else if (value is IManagedObjectInstance instance)
            {
                //if (addresses.Contains(instance.GetAddress()))
                //{
                //    return;
                //}
                //addresses.Add(instance.GetAddress());
                //dump.Add("Ready to handle managed object " + currentNode);
                var model = new ManagedObjectInstanceContentViewModel(instance);
                model.DumpMemory(currentNode, dump, addresses);
                //this.Content = model;
            }
            else if (value is IList list)
            {
                var model = new ListContentViewModel(list);
                model.DumpMemory(currentNode, dump, addresses);
                //model.AppendToTrail += this.ModelOnAppendToTrail;
                //this.Content = model;
            }
            else
            {
                try
                {
                    var model = new TypeDefinitionContentViewModel((value as dynamic).TypeDefinition);
                    model.DumpMemory(currentNode, dump, addresses, value as dynamic);
                }
                catch (Exception e)
                {

                }
            }
            //else if (value != null)
            //{
            //    Console.WriteLine("Unknown content type " + value.GetType());
            //}
            //else if (value == null)
            //{
            //    Console.WriteLine("Null value for " + currentNode);
            //}
            //else
            //{
            //    Console.WriteLine("Wtf value for " + currentNode);
            //}
        }
    }
}
