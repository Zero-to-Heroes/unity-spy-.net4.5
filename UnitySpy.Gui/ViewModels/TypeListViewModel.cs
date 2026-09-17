using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HackF5.UnitySpy.Gui.ViewModels
{
    public sealed class TypeListViewModel : ObservableObject
    {
        private readonly List<TypeRowViewModel> allTypes = new List<TypeRowViewModel>();
        private readonly List<TypeRowViewModel> shortcuts = new List<TypeRowViewModel>();
        private string filter = string.Empty;
        private bool staticsOnly = true;
        private bool hearthstoneShortcuts;

        public ObservableCollection<TypeRowViewModel> VisibleTypes { get; } = new ObservableCollection<TypeRowViewModel>();

        public string Filter
        {
            get => this.filter;
            set
            {
                if (this.SetProperty(ref this.filter, value ?? string.Empty))
                {
                    this.ApplyFilter();
                }
            }
        }

        public bool StaticsOnly
        {
            get => this.staticsOnly;
            set
            {
                if (this.SetProperty(ref this.staticsOnly, value))
                {
                    this.ApplyFilter();
                }
            }
        }

        public bool HearthstoneShortcuts
        {
            get => this.hearthstoneShortcuts;
            set
            {
                if (this.SetProperty(ref this.hearthstoneShortcuts, value))
                {
                    this.ApplyFilter();
                }
            }
        }

        public bool HasShortcuts => this.shortcuts.Count > 0;

        public void Reset()
        {
            this.allTypes.Clear();
            this.shortcuts.Clear();
            this.VisibleTypes.Clear();
        }

        public void SetTypes(IEnumerable<TypeRowViewModel> types)
        {
            this.allTypes.Clear();
            this.allTypes.AddRange(types);
            this.ApplyFilter();
        }

        public void SetShortcuts(IEnumerable<TypeRowViewModel> shortcutRows)
        {
            this.shortcuts.Clear();
            this.shortcuts.AddRange(shortcutRows);
            this.OnPropertyChanged(nameof(this.HasShortcuts));
            if (this.hearthstoneShortcuts)
            {
                this.ApplyFilter();
            }
        }

        public TypeRowViewModel? FindByName(string name)
        {
            return this.allTypes.FirstOrDefault(t =>
                       string.Equals(t.FullName, name, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase))
                   ?? this.shortcuts.FirstOrDefault(t =>
                       string.Equals(t.FullName, name, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void ApplyFilter()
        {
            IEnumerable<TypeRowViewModel> source = this.hearthstoneShortcuts && this.shortcuts.Count > 0
                ? this.shortcuts
                : this.allTypes;
            if (!this.hearthstoneShortcuts && this.staticsOnly)
            {
                source = source.Where(t => t.HasStaticFields != false);
            }

            if (!string.IsNullOrWhiteSpace(this.filter))
            {
                source = source.Where(t =>
                    t.FullName.IndexOf(this.filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || t.Name.IndexOf(this.filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var snapshot = source.ToList();
            this.VisibleTypes.Clear();
            foreach (var row in snapshot)
            {
                this.VisibleTypes.Add(row);
            }
        }
    }
}
