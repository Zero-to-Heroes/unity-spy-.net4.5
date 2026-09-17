using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using HackF5.UnitySpy;
using HackF5.UnitySpy.Detail;
using TypeCode = HackF5.UnitySpy.Detail.TypeCode;

namespace HackF5.UnitySpy.Gui.Services
{
    public static class ValueFormatter
    {
        public static string FormatValue(object? value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string text)
            {
                return "\"" + text + "\"";
            }

            if (value is IManagedObjectInstance instance)
            {
                var typeName = instance.TypeDefinition?.Name ?? instance.TypeDefinition?.FullName ?? "object";
                return "{" + typeName + "}";
            }

            if (value is ITypeDefinition type)
            {
                return type.FullName;
            }

            if (value is IList list)
            {
                return "[" + list.Count + " items]";
            }

            if (value is Exception ex)
            {
                return "ERROR: " + ex.Message;
            }

            var raw = value.ToString();
            if (raw != null && raw.StartsWith("ERROR:", StringComparison.Ordinal))
            {
                return raw;
            }

            return raw ?? value.GetType().Name;
        }

        public static string FormatTypeName(IFieldDefinition? field, object? value)
        {
            if (field != null)
            {
                try
                {
                    if (field.TypeInfo != null && field.TypeInfo.TryGetTypeDefinition(out var definition) && definition != null)
                    {
                        return definition.FullName;
                    }

                    if (field.TypeInfo != null)
                    {
                        return DescribeTypeCode(field.TypeInfo.TypeCode);
                    }
                }
                catch
                {
                    // Fall through to the value.
                }
            }

            if (value is IManagedObjectInstance instance && instance.TypeDefinition != null)
            {
                return instance.TypeDefinition.FullName;
            }

            if (value is ITypeDefinition type)
            {
                return type.FullName;
            }

            if (value is IList list)
            {
                return list.GetType().Name;
            }

            if (value == null)
            {
                return "null";
            }

            return value.GetType().Name;
        }

        public static bool CanExpand(object? value, bool isArrayField = false)
        {
            if (isArrayField)
            {
                return true;
            }

            if (value is IManagedObjectInstance)
            {
                return true;
            }

            if (value is ITypeDefinition)
            {
                return true;
            }

            if (value is IList list)
            {
                return list.Count > 0;
            }

            return false;
        }

        public static bool IsArrayField(IFieldDefinition? field)
        {
            try
            {
                return field?.TypeInfo != null && field.TypeInfo.TypeCode == TypeCode.SZARRAY;
            }
            catch
            {
                return false;
            }
        }

        public static string DescribeTypeCode(TypeCode typeCode)
        {
            var name = typeCode.ToString();
            var member = typeof(TypeCode).GetMember(name).FirstOrDefault();
            return member?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
        }
    }
}
