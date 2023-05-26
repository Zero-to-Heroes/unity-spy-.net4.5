namespace HackF5.UnitySpy.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a class instance in managed memory.
    /// Mono and .NET don't necessarily use the same layout scheme, but assuming it is similar this article provides
    /// some useful information:
    /// https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx.
    /// </summary>
    [PublicAPI]
    public class ManagedClassInstance : ManagedObjectInstance
    {
        private IntPtr definitionAddress;
        private IntPtr vtable;
        private AssemblyImage image;

        public ManagedClassInstance([NotNull] AssemblyImage image, List<TypeInfo> genericTypeArguments, IntPtr address)
            : base(image, genericTypeArguments, address)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this.image = image;
            Init();
        }

        // Sometimes reading the value right away causes some issues
        // This might make the graph construction slower, but ultimately will
        // avoid having to do module-wide resets later on
        private void Init() {
            int tryCount = 0;

            IntPtr prevVtable = Constants.NullPtr;
            IntPtr prevDefinitionAddress = Constants.NullPtr;
            while (tryCount < 50)
            {
                try
                {
                    // the address of the class instance points directly back the the classes VTable
                    this.vtable = this.ReadPtr(0x0);

                    // The VTable points to the class definition itself.
                    this.definitionAddress = this.image.Process.ReadPtr(this.vtable);

                    if (this.definitionAddress == prevDefinitionAddress)
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    prevDefinitionAddress = this.definitionAddress;
                    prevVtable = this.vtable;

                    // We try to access the fields. This is probably costly (vs getting fields more lazily), but:
                    // - if we have an error, we can simply re-read the information from the memory. Indeed, it looks
                    // like the vtable offset is sometimes off by 1 right after instantiation, and re-reading the info
                    // after a short pause can yield the right value
                    // - when creating a managed class, we usually access the fields info soon after in any case
                    var _ = TypeDefinition?.Fields;

                    // If we managed to build the fields, we can exit
                    break;
                }
                catch (Exception ex)
                {
                    tryCount++;
                    Thread.Sleep(2);
                    continue;
                }
            }

            try
            {
                var _ = TypeDefinition?.Fields;
            }
            catch (Exception e)
            {
                throw new Exception($"Could not properly initialize fields. vtable={this.vtable}, definitionAddress={this.definitionAddress}, " +
                    $"address={this.Address}, type={this.GetType()}. Initialize exception message: {e.Message}");
            }
        }

        public override TypeDefinition TypeDefinition => this.Image.GetTypeDefinition(this.definitionAddress);
    }
}