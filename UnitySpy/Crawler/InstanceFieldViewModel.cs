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

    public class InstanceFieldViewModel 
    {
        private readonly IFieldDefinition field;

        private readonly IManagedObjectInstance instance;

        public InstanceFieldViewModel([NotNull] IFieldDefinition field, [NotNull] IManagedObjectInstance instance)
        {
            this.field = field ?? throw new ArgumentNullException(nameof(field));
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        //public delegate InstanceFieldViewModel Factory(IFieldDefinition field, IManagedObjectInstance instance);

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
            Crawler.DumpMemory(this.Value, currentNode, dump, addresses);
        }

        public object Value
        {
            get
            {
                try
                {
                    return this.instance.GetValue<object>(this.Name);
                }
                catch (Exception ex)
                {
                    return $"ERROR: {ex.Message}";
                }
            }
        }
    }
}