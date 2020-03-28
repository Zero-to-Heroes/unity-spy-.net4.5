namespace HackF5.UnitySpy.Crawler
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;
    using TypeCode = HackF5.UnitySpy.Detail.TypeCode;

    [UsedImplicitly]
    public class StaticFieldViewModel
    {
        private readonly IFieldDefinition field;

        public StaticFieldViewModel([NotNull] IFieldDefinition field)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
        }

        //public delegate StaticFieldViewModel Factory(IFieldDefinition field);

        public string Name => this.field.Name;

        public string TypeName
        {
            get
            {
                if (this.field.TypeInfo.TryGetTypeDefinition(out var typeDefinition))
                {
                    return typeDefinition.FullName;
                }

                var typeName = this.field.TypeInfo.TypeCode.ToString();
                var member = typeof(TypeCode).GetMember(typeName).FirstOrDefault();
                return member?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? typeName;
            }
        }

        public void DumpMemory(string previous, List<string> dump, List<uint> addresses)
        {
            var currentNode = previous + this.field.Name;
            dump.Add(currentNode + "(" + this.TypeName + ")" + " = " + this.Value);
            //Console.WriteLine(currentNode + "(" + this.TypeName + ")" + " = " + this.Value);
            //dump.Add("Will handle " + currentNode);
            Crawler.DumpMemory(this.Value, currentNode, dump, addresses);
            //if (this.Value is IManagedObjectInstance instance)
            //{
            //    var model = new ManagedObjectInstanceContentViewModel(instance);
            //    model.DumpMemory(currentNode, dump);
            //    //this.Content = model;
            //    return;
            //}
        }

        public object Value
        {
            get
            {
                try
                {
                    return this.field.DeclaringType.GetStaticValue<object>(this.Name);
                }
                catch (Exception ex)
                {
                    return $"ERROR: {ex.Message}";
                }
            }
        }
    }
}