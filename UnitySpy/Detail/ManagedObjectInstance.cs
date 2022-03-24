namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;

    public abstract class ManagedObjectInstance : MemoryObject, IManagedObjectInstance
    {
        private readonly List<TypeInfo> genericTypeArguments;

        protected ManagedObjectInstance(AssemblyImage image, List<TypeInfo> genericTypeArguments, IntPtr address)
            : base(image, address)
        {
            this.genericTypeArguments = genericTypeArguments;
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => this.TypeDefinition;

        public abstract TypeDefinition TypeDefinition { get; }

        public dynamic this[string fieldName] => this.GetValue<dynamic>(fieldName);

        public dynamic this[string fieldName, bool exceptionOnMissingField] => this.GetValue<dynamic>(fieldName, exceptionOnMissingField);

        public dynamic this[string fieldName, string typeFullName, bool exceptionOnMissingField = false] => this.GetValue<dynamic>(fieldName, typeFullName, exceptionOnMissingField);

        public TValue GetValue<TValue>(string fieldName, bool exceptionOnMissingField = true) => this.GetValue<TValue>(fieldName, default, exceptionOnMissingField);

        public TValue GetValue<TValue>(string fieldName, string typeFullName, bool exceptionOnMissingField)
        {
            var field = this.TypeDefinition.GetField(fieldName, typeFullName);
            if (field == null && exceptionOnMissingField)
            {
                throw new ArgumentException(
                    $"No field exists with name {fieldName} in type {typeFullName ?? "<any>"}.");
            }

            if (field == null)
            {
                return default;
            }

            return field.GetValue<TValue>(this.genericTypeArguments, this.Address);
        }
    }
}