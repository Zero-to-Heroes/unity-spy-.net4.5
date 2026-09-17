using HackF5.UnitySpy;
using HackF5.UnitySpy.Gui.Services;

namespace HackF5.UnitySpy.Gui.ViewModels
{
    public sealed class TypeRowViewModel
    {
        public TypeRowViewModel(ITypeDefinition type)
        {
            this.Type = type;
            this.FullName = SafeName(() => type.FullName, type.Name);
            this.Name = type.Name ?? this.FullName;
            this.Kind = PathRootKind.Type;
        }

        public TypeRowViewModel(HearthstoneShortcut shortcut)
        {
            this.Type = shortcut.Type;
            this.FullName = shortcut.FullName;
            this.Name = shortcut.Name;
            this.HasStaticFields = shortcut.Type != null;
            this.Kind = shortcut.Kind;
            this.Shortcut = shortcut;
        }

        public ITypeDefinition? Type { get; }

        public string FullName { get; }

        public string Name { get; }

        public bool? HasStaticFields { get; set; }

        public PathRootKind Kind { get; }

        public HearthstoneShortcut? Shortcut { get; }

        public bool IsShortcut => this.Shortcut != null && this.Kind != PathRootKind.Type;

        public bool ComputeHasStaticFields()
        {
            if (this.HasStaticFields != null)
            {
                return this.HasStaticFields.Value;
            }

            var hasStatics = false;
            try
            {
                if (this.Type != null)
                {
                    hasStatics = this.Type.Fields.AnySafeStatic();
                }
            }
            catch
            {
                hasStatics = false;
            }

            this.HasStaticFields = hasStatics;
            return hasStatics;
        }

        private static string SafeName(System.Func<string> getter, string? fallback)
        {
            try
            {
                return getter() ?? fallback ?? string.Empty;
            }
            catch
            {
                return fallback ?? string.Empty;
            }
        }
    }

    internal static class TypeRowExtensions
    {
        public static bool AnySafeStatic(this System.Collections.Generic.IReadOnlyList<IFieldDefinition> fields)
        {
            foreach (var field in fields)
            {
                try
                {
                    if (field.TypeInfo != null && field.TypeInfo.IsStatic && !field.TypeInfo.IsConstant)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Skip unreadable fields.
                }
            }

            return false;
        }
    }
}
