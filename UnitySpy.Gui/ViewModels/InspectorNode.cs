using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using HackF5.UnitySpy;
using HackF5.UnitySpy.Gui.Services;

namespace HackF5.UnitySpy.Gui.ViewModels
{
    public sealed class InspectorNode : ObservableObject
    {
        public const int PageSize = 200;

        private readonly IFieldDefinition? field;
        private readonly ITypeDefinition? staticOwner;
        private readonly IManagedObjectInstance? instanceOwner;
        private readonly IList? list;
        private readonly IManagedObjectInstance? arrayInstance;
        private readonly string? arrayFieldName;
        private readonly Func<object?>? valueFactory;
        private readonly InspectorNode? placeholder;
        private bool isExpanded;
        private bool isSelected;
        private bool childrenLoaded;
        private int loadedCount;
        private int? totalCount;
        private string typeName;
        private string valueText;
        private object? value;

        private InspectorNode(
            string name,
            string pathSegment,
            InspectorNode? parent,
            string typeName,
            object? value,
            bool canExpand,
            IFieldDefinition? field = null,
            ITypeDefinition? staticOwner = null,
            IManagedObjectInstance? instanceOwner = null,
            IList? list = null,
            IManagedObjectInstance? arrayInstance = null,
            string? arrayFieldName = null,
            Func<object?>? valueFactory = null,
            bool isLoadMore = false,
            bool isRoot = false)
        {
            this.Name = name;
            this.PathSegment = pathSegment;
            this.Parent = parent;
            this.typeName = typeName;
            this.value = value;
            this.valueText = FormatValueSafe(value);
            this.CanExpand = canExpand;
            this.field = field;
            this.staticOwner = staticOwner;
            this.instanceOwner = instanceOwner;
            this.list = list;
            this.arrayInstance = arrayInstance;
            this.arrayFieldName = arrayFieldName;
            this.valueFactory = valueFactory;
            this.IsLoadMore = isLoadMore;
            this.IsRoot = isRoot;
            this.Children = new ObservableCollection<InspectorNode>();
            if (canExpand)
            {
                this.placeholder = new InspectorNode("…", string.Empty, this, string.Empty, null, false);
                this.Children.Add(this.placeholder);
            }
        }

        public string Name { get; }

        public string PathSegment { get; }

        public InspectorNode? Parent { get; }

        public bool CanExpand { get; }

        public bool IsLoadMore { get; }

        public bool IsRoot { get; }

        public ObservableCollection<InspectorNode> Children { get; }

        public string TypeName
        {
            get => this.typeName;
            private set => this.SetProperty(ref this.typeName, value);
        }

        public string ValueText
        {
            get => this.valueText;
            private set => this.SetProperty(ref this.valueText, value);
        }

        public object? Value
        {
            get => this.value;
            private set => this.SetProperty(ref this.value, value);
        }

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (!this.SetProperty(ref this.isExpanded, value) || !value)
                {
                    return;
                }

                this.ScheduleLoadChildren();
            }
        }

        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetProperty(ref this.isSelected, value);
        }

        public static InspectorNode FromType(ITypeDefinition type)
        {
            return new InspectorNode(
                type.FullName,
                type.FullName,
                null,
                type.FullName,
                type,
                true,
                isRoot: true);
        }

        public static InspectorNode FromResolvedValue(string name, object? value, PathRootKind _)
        {
            var canExpand = ValueFormatter.CanExpand(value);
            var typeName = ValueFormatter.FormatTypeName(null, value);
            return new InspectorNode(name, name, null, typeName, value, canExpand, isRoot: true);
        }

        public static InspectorNode FromField(IFieldDefinition field, ITypeDefinition staticOwner, InspectorNode parent)
        {
            return CreateFieldNode(field, parent, staticOwner, null);
        }

        public static InspectorNode FromField(IFieldDefinition field, IManagedObjectInstance instance, InspectorNode parent)
        {
            return CreateFieldNode(field, parent, null, instance);
        }

        public IReadOnlyList<PathSegment> GetPathSegments()
        {
            var stack = new Stack<PathSegment>();
            for (var node = this; node != null; node = node.Parent)
            {
                if (string.IsNullOrEmpty(node.PathSegment))
                {
                    continue;
                }

                stack.Push(PathFormatter.FromToken(node.PathSegment));
            }

            return stack.ToList();
        }

        public void ScheduleLoadChildren()
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || !dispatcher.CheckAccess())
            {
                this.LoadChildren();
                return;
            }

            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(this.LoadChildren));
        }

        public void LoadChildren()
        {
            if (this.childrenLoaded || !this.CanExpand || this.IsLoadMore)
            {
                return;
            }

            this.childrenLoaded = true;
            this.Children.Clear();

            try
            {
                if (this.Value is ITypeDefinition type)
                {
                    foreach (var child in CreateStaticFieldNodes(type, this))
                    {
                        this.Children.Add(child);
                    }

                    return;
                }

                if (this.IsArraySource(out var arrayOwner, out var arrayField) || this.Value is IList || this.list != null)
                {
                    this.EnsureListSource();
                    this.LoadNextPage();
                    return;
                }

                if (this.Value is IManagedObjectInstance instance)
                {
                    foreach (var child in CreateInstanceFieldNodes(instance, this))
                    {
                        this.Children.Add(child);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Children.Add(new InspectorNode("ERROR", string.Empty, this, "error", "ERROR: " + ex.Message, false));
            }
        }

        public void LoadNextPage()
        {
            try
            {
                this.EnsureListSource();
                this.RemoveLoadMore();

                var start = this.loadedCount;
                var end = this.totalCount != null
                    ? Math.Min(start + PageSize, this.totalCount.Value)
                    : start + PageSize;

                for (var i = start; i < end; i++)
                {
                    var item = this.ReadListItem(i);
                    if (this.totalCount == null && item == null && i > start && this.AreRemainingUnknown())
                    {
                        break;
                    }

                    this.Children.Add(CreateListItemNode(i, item, this));
                }

                this.loadedCount = end;
                if (this.totalCount == null || this.loadedCount < this.totalCount.Value)
                {
                    if (this.totalCount != null || end - start >= PageSize)
                    {
                        this.Children.Add(CreateLoadMore(this));
                    }
                }
            }
            catch (Exception ex)
            {
                this.Children.Add(new InspectorNode("ERROR", string.Empty, this, "error", "ERROR: " + ex.Message, false));
            }
        }

        public void EnsureLoadedThrough(int index)
        {
            if (!this.childrenLoaded)
            {
                this.LoadChildren();
            }

            while (this.totalCount == null || this.loadedCount <= index)
            {
                var hadLoadMore = this.Children.Any(c => c.IsLoadMore);
                if (!hadLoadMore)
                {
                    break;
                }

                this.LoadNextPage();
                if (this.loadedCount > index)
                {
                    break;
                }
            }
        }

        public InspectorNode? FindChild(string segment)
        {
            return this.Children.FirstOrDefault(c =>
                !c.IsLoadMore && string.Equals(c.PathSegment, segment, StringComparison.Ordinal));
        }

        public void Refresh()
        {
            if (this.valueFactory != null)
            {
                this.ApplyValue(ReadSafe(this.valueFactory));
            }
            else if (this.field != null)
            {
                this.ApplyValue(ReadFieldValue(this.field, this.staticOwner, this.instanceOwner));
            }

            if (!this.childrenLoaded)
            {
                return;
            }

            this.childrenLoaded = false;
            this.loadedCount = 0;
            this.totalCount = null;
            this.Children.Clear();
            if (this.CanExpand && this.placeholder != null)
            {
                this.Children.Add(this.placeholder);
            }

            if (this.isExpanded)
            {
                this.LoadChildren();
            }
        }

        public static IEnumerable<InspectorNode> CreateStaticFieldNodes(ITypeDefinition type, InspectorNode parent)
        {
            List<IFieldDefinition>? fields = null;
            string? error = null;
            try
            {
                fields = type.Fields
                    .Where(f => f.TypeInfo != null && f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                    .OrderBy(f => f.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                error = "ERROR: " + ex.Message;
            }

            if (error != null)
            {
                yield return new InspectorNode("ERROR", string.Empty, parent, "error", error, false);
                yield break;
            }

            foreach (var field in fields!)
            {
                yield return CreateFieldNode(field, parent, type, null);
            }
        }

        public static IEnumerable<InspectorNode> CreateInstanceFieldNodes(IManagedObjectInstance instance, InspectorNode parent)
        {
            List<IFieldDefinition>? fields = null;
            string? error = null;
            try
            {
                fields = instance.TypeDefinition.Fields
                    .Where(f => f.TypeInfo != null && !f.TypeInfo.IsStatic && !f.TypeInfo.IsConstant)
                    .OrderBy(f => f.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                error = "ERROR: " + ex.Message;
            }

            if (error != null)
            {
                yield return new InspectorNode("ERROR", string.Empty, parent, "error", error, false);
                yield break;
            }

            foreach (var field in fields!)
            {
                yield return CreateFieldNode(field, parent, null, instance);
            }
        }

        private static InspectorNode CreateFieldNode(
            IFieldDefinition field,
            InspectorNode parent,
            ITypeDefinition? staticOwner,
            IManagedObjectInstance? instanceOwner)
        {
            var isArray = ValueFormatter.IsArrayField(field);
            object? value = null;
            if (!isArray)
            {
                value = ReadFieldValue(field, staticOwner, instanceOwner);
            }

            var typeName = FormatTypeSafe(field, value);
            var canExpand = isArray;
            try
            {
                canExpand = isArray || ValueFormatter.CanExpand(value);
            }
            catch
            {
                canExpand = isArray;
            }
            var displayValue = isArray && value == null ? "(array)" : value;
            return new InspectorNode(
                field.Name,
                field.Name,
                parent,
                typeName,
                displayValue,
                canExpand,
                field,
                staticOwner,
                instanceOwner,
                arrayInstance: isArray ? instanceOwner : null,
                arrayFieldName: isArray ? field.Name : null,
                valueFactory: () => ReadFieldValue(field, staticOwner, instanceOwner));
        }

        private static InspectorNode CreateListItemNode(int index, object? value, InspectorNode parent)
        {
            var name = "[" + index + "]";
            return new InspectorNode(
                name,
                index.ToString(CultureInfo.InvariantCulture),
                parent,
                FormatTypeSafe(null, value),
                value,
                ValueFormatter.CanExpand(value));
        }

        private static InspectorNode CreateLoadMore(InspectorNode parent)
        {
            return new InspectorNode("Load more…", string.Empty, parent, string.Empty, null, false, isLoadMore: true);
        }

        private static object? ReadFieldValue(
            IFieldDefinition field,
            ITypeDefinition? staticOwner,
            IManagedObjectInstance? instanceOwner)
        {
            try
            {
                if (staticOwner != null)
                {
                    return staticOwner.GetStaticValue<object>(field.Name);
                }

                if (instanceOwner != null)
                {
                    return instanceOwner.GetValue<object>(field.Name);
                }
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }

            return null;
        }

        private static object? ReadSafe(Func<object?> factory)
        {
            try
            {
                return factory();
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        private void ApplyValue(object? value)
        {
            this.Value = value;
            this.ValueText = FormatValueSafe(value);
            this.TypeName = FormatTypeSafe(this.field, value);
        }

        private static string FormatValueSafe(object? value)
        {
            try
            {
                return ValueFormatter.FormatValue(value);
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        private static string FormatTypeSafe(IFieldDefinition? field, object? value)
        {
            try
            {
                return ValueFormatter.FormatTypeName(field, value);
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        private bool IsArraySource(out IManagedObjectInstance? owner, out string? fieldName)
        {
            owner = this.arrayInstance;
            fieldName = this.arrayFieldName;
            return owner != null && !string.IsNullOrEmpty(fieldName);
        }

        private void EnsureListSource()
        {
            if (this.totalCount != null)
            {
                return;
            }

            if (this.list != null)
            {
                this.totalCount = this.list.Count;
                return;
            }

            if (this.Value is IList existing)
            {
                this.totalCount = existing.Count;
                return;
            }

            if (this.arrayInstance != null && !string.IsNullOrEmpty(this.arrayFieldName))
            {
                var materialized = ReadSafe(() => this.arrayInstance.GetValue<object>(this.arrayFieldName));
                if (materialized is IList materializedList)
                {
                    this.Value = materializedList;
                    this.ValueText = FormatValueSafe(materializedList);
                    this.totalCount = materializedList.Count;
                    return;
                }

                if (materialized != null)
                {
                    this.Value = materialized;
                    this.ValueText = FormatValueSafe(materialized);
                }
            }
        }

        private object? ReadListItem(int index)
        {
            if (this.arrayInstance != null && !string.IsNullOrEmpty(this.arrayFieldName))
            {
                try
                {
                    return this.arrayInstance.GetArrayValue<object>(this.arrayFieldName, index);
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.Message;
                }
            }

            var source = this.list ?? this.Value as IList;
            if (source == null)
            {
                return null;
            }

            try
            {
                return source[index];
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.Message;
            }
        }

        private bool AreRemainingUnknown() => this.totalCount == null;

        private void RemoveLoadMore()
        {
            for (var i = this.Children.Count - 1; i >= 0; i--)
            {
                if (this.Children[i].IsLoadMore)
                {
                    this.Children.RemoveAt(i);
                }
            }
        }
    }
}
