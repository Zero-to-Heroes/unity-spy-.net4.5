namespace HackF5.UnitySpy.Crawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;

    public class TypeDefinitionContentViewModel
    {
        private readonly ITypeDefinition definition;

        public TypeDefinitionContentViewModel([NotNull] ITypeDefinition definition)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.StaticFields = this.definition.Fields.Where(f => f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                .Select(f => new StaticFieldViewModel(f))
                .ToArray();
        }

        //public delegate TypeDefinitionContentViewModel Factory(ITypeDefinition definition);

        public IEnumerable<StaticFieldViewModel> StaticFields { get; }

        public void DumpMemory(string previous, List<string> dump, List<uint> addresses)
        {
            var currentNode = previous + this.definition.FullName;
            dump.Add(currentNode);
            //Console.WriteLine(currentNode);
            foreach (var field in this.StaticFields) {
                field.DumpMemory(currentNode + Crawler.SEPARATOR, dump, addresses);
            }
        }
    }
}