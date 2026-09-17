using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HackF5.UnitySpy;
using HackF5.UnitySpy.Detail;
using HackF5.UnitySpy.Gui.Services;

namespace HackF5.UnitySpy.Gui.ViewModels
{
    public sealed class MainViewModel : ObservableObject, IDisposable
    {
        private IAssemblyImage? image;
        private CancellationTokenSource? attachCts;
        private ProcessItem? selectedProcess;
        private TypeRowViewModel? selectedType;
        private InspectorNode? selectedNode;
        private string pathText = string.Empty;
        private string status = "Pick a Unity process and attach.";
        private bool isBusy;
        private bool isAttached;
        private bool suppressPathSync;
        private PathRootKind currentRootKind = PathRootKind.Type;
        private bool disposed;

        public MainViewModel()
        {
            this.TypeList = new TypeListViewModel();
            this.RefreshProcessesCommand = new AsyncRelayCommand(this.RefreshProcessesAsync, () => !this.IsBusy);
            this.AttachCommand = new AsyncRelayCommand(this.AttachAsync, () => this.SelectedProcess != null && !this.IsBusy);
            this.DisconnectCommand = new RelayCommand(this.Disconnect, () => this.IsAttached && !this.IsBusy);
            this.ApplyPathCommand = new RelayCommand(this.ApplyPath, () => this.InspectorRoots.Count > 0);
            this.PathBackCommand = new RelayCommand(this.PathBack, () => this.GetActiveSegments().Count > 1);
            this.RefreshNodeCommand = new RelayCommand(this.RefreshSelectedNode, () => this.SelectedNode != null || this.InspectorRoots.Count > 0);
            this.CopyPathCommand = new RelayCommand(this.CopyPath, () => this.GetActiveSegments().Count > 0);
            this.CopyValueCommand = new RelayCommand(this.CopyValue, () => this.SelectedNode != null);
            this.BreadcrumbCommand = new RelayCommand<BreadcrumbItem>(this.NavigateBreadcrumb);
        }

        public TypeListViewModel TypeList { get; }

        public ObservableCollection<ProcessItem> Processes { get; } = new ObservableCollection<ProcessItem>();

        public ObservableCollection<string> LogLines { get; } = new ObservableCollection<string>();

        public ObservableCollection<InspectorNode> InspectorRoots { get; } = new ObservableCollection<InspectorNode>();

        public ObservableCollection<BreadcrumbItem> Breadcrumbs { get; } = new ObservableCollection<BreadcrumbItem>();

        public IAsyncRelayCommand RefreshProcessesCommand { get; }

        public IAsyncRelayCommand AttachCommand { get; }

        public IRelayCommand DisconnectCommand { get; }

        public IRelayCommand ApplyPathCommand { get; }

        public IRelayCommand PathBackCommand { get; }

        public IRelayCommand RefreshNodeCommand { get; }

        public IRelayCommand CopyPathCommand { get; }

        public IRelayCommand CopyValueCommand { get; }

        public ICommand BreadcrumbCommand { get; }

        public ProcessItem? SelectedProcess
        {
            get => this.selectedProcess;
            set
            {
                if (this.SetProperty(ref this.selectedProcess, value))
                {
                    this.AttachCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool UseHearthstoneShortcuts
        {
            get => this.TypeList.HearthstoneShortcuts;
            set
            {
                if (this.TypeList.HearthstoneShortcuts == value)
                {
                    return;
                }

                this.TypeList.HearthstoneShortcuts = value;
                this.OnPropertyChanged();
                if (value)
                {
                    _ = this.EnsureShortcutsAsync();
                }
            }
        }

        public TypeRowViewModel? SelectedType
        {
            get => this.selectedType;
            set
            {
                if (this.SetProperty(ref this.selectedType, value) && value != null)
                {
                    this.ShowTypeOrShortcut(value);
                }
            }
        }

        public void ReportError(string message)
        {
            this.AppendLog(message);
            this.Status = message;
        }

        public InspectorNode? SelectedNode
        {
            get => this.selectedNode;
            set
            {
                if (!this.SetProperty(ref this.selectedNode, value) || this.suppressPathSync)
                {
                    this.NotifyPathCommands();
                    return;
                }

                if (value?.IsLoadMore == true)
                {
                    value.Parent?.LoadNextPage();
                    return;
                }

                if (value != null)
                {
                    this.SyncPathFromNode(value);
                }

                this.NotifyPathCommands();
            }
        }

        public string PathText
        {
            get => this.pathText;
            set => this.SetProperty(ref this.pathText, value ?? string.Empty);
        }

        public string Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.OnPropertyChanged(nameof(this.IsIdle));
                    this.RefreshProcessesCommand.NotifyCanExecuteChanged();
                    this.AttachCommand.NotifyCanExecuteChanged();
                    this.DisconnectCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsAttached
        {
            get => this.isAttached;
            set
            {
                if (this.SetProperty(ref this.isAttached, value))
                {
                    this.DisconnectCommand.NotifyCanExecuteChanged();
                    this.OnPropertyChanged(nameof(this.WindowTitle));
                }
            }
        }

        public bool IsIdle => !this.IsBusy;

        public string WindowTitle
        {
            get
            {
                if (!this.IsAttached || this.SelectedProcess == null)
                {
                    return "UnitySpy Memory Explorer";
                }

                return "UnitySpy Memory Explorer — " + this.SelectedProcess.DisplayName;
            }
        }

        public async Task RefreshProcessesAsync()
        {
            this.IsBusy = true;
            this.Status = "Listing processes…";
            try
            {
                var items = await Task.Run(() => ProcessListService.ListProcesses()).ConfigureAwait(true);
                var previousId = this.SelectedProcess?.Id;
                this.Processes.Clear();
                foreach (var item in items)
                {
                    this.Processes.Add(item);
                }

                this.SelectedProcess = this.Processes.FirstOrDefault(p => p.Id == previousId)
                    ?? this.Processes.FirstOrDefault(p => p.IsHearthstone)
                    ?? this.Processes.FirstOrDefault(p => p.HasMono)
                    ?? this.Processes.FirstOrDefault();
                this.Status = this.IsAttached ? this.Status : "Pick a Unity process and attach.";
            }
            catch (Exception ex)
            {
                this.AppendLog("ERROR listing processes: " + ex.Message);
                this.Status = "Failed to list processes.";
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.CancelAttach();
            this.DisposeImage();
        }

        private async Task AttachAsync()
        {
            var process = this.SelectedProcess;
            if (process == null)
            {
                return;
            }

            if (!ProcessListService.BitnessMatches(process))
            {
                var explorer = Environment.Is64BitProcess ? "x64" : "x86";
                var target = process.Is64Bit == true ? "x64" : "x86";
                this.Status = "Bitness mismatch: this explorer is " + explorer + " but the process is " + target
                    + ". Rebuild UnitySpy.Gui as " + target + ".";
                this.AppendLog(this.Status);
                return;
            }

            this.CancelAttach();
            this.DisposeImage();
            this.ClearInspection();
            this.attachCts = new CancellationTokenSource();
            var token = this.attachCts.Token;

            this.IsBusy = true;
            this.Status = "Attaching to " + process.DisplayName + "…";
            this.AppendLog("Attaching to " + process.DisplayName);
            try
            {
                ProcessFacade.UseBlockReads = true;
                var created = await Task.Run(() => AssemblyImageFactory.Create(process.Id, this.AppendLog), token)
                    .ConfigureAwait(true);
                if (token.IsCancellationRequested)
                {
                    created.Dispose();
                    return;
                }

                this.image = created;
                this.IsAttached = true;
                this.Status = "Loading types…";
                var catalog = await Task.Run(() => this.BuildCatalog(created, token), token).ConfigureAwait(true);
                if (token.IsCancellationRequested)
                {
                    return;
                }

                this.TypeList.SetTypes(catalog.Types);
                this.SelectType(this.TypeList.FindByName("CollectionManager"), expand: false);
                this.Status = "Attached to " + process.DisplayName + " — " + catalog.Types.Count + " types. Scanning statics…";
                this.AppendLog("Attached. Listed " + catalog.Types.Count + " types.");
                _ = this.ScanStaticFieldsAsync(catalog.Types, token);
            }
            catch (OperationCanceledException)
            {
                this.Status = "Attach cancelled.";
            }
            catch (Exception ex)
            {
                this.AppendLog("ERROR: " + ex);
                this.Status = "Attach failed: " + ex.Message;
                this.DisposeImage();
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private CatalogResult BuildCatalog(IAssemblyImage assemblyImage, CancellationToken token)
        {
            var rows = new List<TypeRowViewModel>();
            foreach (var type in assemblyImage.TypeDefinitions)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    rows.Add(new TypeRowViewModel(type));
                }
                catch (Exception ex)
                {
                    this.AppendLog("Skipped a type: " + ex.Message);
                }
            }

            rows.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.OrdinalIgnoreCase));
            return new CatalogResult(rows);
        }

        private async Task ScanStaticFieldsAsync(List<TypeRowViewModel> rows, CancellationToken token)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (var row in rows)
                    {
                        token.ThrowIfCancellationRequested();
                        row.ComputeHasStaticFields();
                    }
                }, token).ConfigureAwait(true);

                if (token.IsCancellationRequested || !this.IsAttached)
                {
                    return;
                }

                this.TypeList.ApplyFilter();
                this.Status = "Attached to " + (this.SelectedProcess?.DisplayName ?? "process")
                    + " — " + this.TypeList.VisibleTypes.Count + " types visible.";
                this.AppendLog(this.Status);
            }
            catch (OperationCanceledException)
            {
                // Attach was cancelled or replaced.
            }
            catch (Exception ex)
            {
                this.AppendLog("Static-field scan failed: " + ex.Message);
            }
        }

        private async Task EnsureShortcutsAsync()
        {
            if (this.image == null || this.TypeList.HasShortcuts)
            {
                this.TypeList.ApplyFilter();
                return;
            }

            var assemblyImage = this.image;
            this.Status = "Loading Hearthstone shortcuts…";
            try
            {
                var shortcuts = await Task.Run(() =>
                {
                    var types = assemblyImage.TypeDefinitions.ToList();
                    return HearthstoneShortcutService.Build(assemblyImage, types)
                        .Select(s => new TypeRowViewModel(s))
                        .ToList();
                }).ConfigureAwait(true);

                if (!this.IsAttached)
                {
                    return;
                }

                this.TypeList.SetShortcuts(shortcuts);
                this.AppendLog("Loaded " + shortcuts.Count + " Hearthstone shortcuts.");
                this.Status = shortcuts.Count + " Hearthstone shortcuts available.";
            }
            catch (Exception ex)
            {
                this.AppendLog("Hearthstone shortcuts unavailable: " + ex.Message);
                this.Status = "Hearthstone shortcuts failed.";
                this.TypeList.HearthstoneShortcuts = false;
                this.OnPropertyChanged(nameof(this.UseHearthstoneShortcuts));
            }
        }

        private void Disconnect()
        {
            this.CancelAttach();
            this.DisposeImage();
            this.ClearInspection();
            this.Status = "Disconnected.";
            this.AppendLog("Disconnected.");
        }

        private void SelectType(TypeRowViewModel? row, bool expand)
        {
            this.selectedType = row;
            this.OnPropertyChanged(nameof(this.SelectedType));
            if (row != null)
            {
                this.ShowTypeOrShortcut(row, expand);
            }
        }

        private void ShowTypeOrShortcut(TypeRowViewModel row, bool expand = true)
        {
            try
            {
                this.InspectorRoots.Clear();
                this.selectedNode = null;
                this.OnPropertyChanged(nameof(this.SelectedNode));
                this.currentRootKind = row.Kind;

                if (row.Shortcut != null && row.Kind != PathRootKind.Type && this.image != null)
                {
                    var resolved = HearthstoneShortcutService.TryResolve(this.image, row.Shortcut);
                    if (resolved is ITypeDefinition type)
                    {
                        this.ShowRoot(InspectorNode.FromType(type), PathRootKind.Type, expand);
                        return;
                    }

                    this.ShowRoot(InspectorNode.FromResolvedValue(row.Name, resolved, row.Kind), row.Kind, expand);
                    return;
                }

                if (row.Type != null)
                {
                    this.ShowRoot(InspectorNode.FromType(row.Type), PathRootKind.Type, expand);
                }
            }
            catch (Exception ex)
            {
                this.AppendLog("Failed to open type: " + ex);
                this.Status = "Failed to open " + row.FullName + ".";
            }
        }

        private void ShowRoot(InspectorNode root, PathRootKind kind, bool expand)
        {
            this.currentRootKind = kind;
            this.InspectorRoots.Add(root);
            if (expand)
            {
                root.IsExpanded = true;
            }

            this.suppressPathSync = true;
            root.IsSelected = true;
            this.selectedNode = root;
            this.suppressPathSync = false;
            this.SyncPathFromNode(root);
            this.ApplyPathCommand.NotifyCanExecuteChanged();
            this.NotifyPathCommands();
        }

        private void ApplyPath()
        {
            if (this.InspectorRoots.Count == 0)
            {
                return;
            }

            var parts = PathFormatter.SplitDisplay(this.PathText);
            var root = this.InspectorRoots[0];
            var node = root;
            var start = 0;
            if (parts.Count > 0 && this.MatchesRoot(parts[0], root))
            {
                start = 1;
            }

            node.IsExpanded = true;
            for (var i = start; i < parts.Count; i++)
            {
                var part = parts[i];
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                {
                    node.EnsureLoadedThrough(index);
                }
                else
                {
                    node.IsExpanded = true;
                }

                var child = node.FindChild(part);
                if (child == null)
                {
                    this.Status = "Path stopped at '" + node.Name + "'.";
                    break;
                }

                node = child;
                node.IsExpanded = true;
            }

            this.SelectNode(node);
        }

        private void PathBack()
        {
            var segments = this.GetActiveSegments();
            if (segments.Count <= 1)
            {
                return;
            }

            this.PathText = PathFormatter.ToDisplay(segments.Take(segments.Count - 1).ToList());
            this.ApplyPath();
        }

        private void NavigateBreadcrumb(BreadcrumbItem? item)
        {
            if (item == null)
            {
                return;
            }

            this.PathText = item.Path;
            this.ApplyPath();
        }

        private void RefreshSelectedNode()
        {
            var node = this.SelectedNode ?? this.InspectorRoots.FirstOrDefault();
            node?.Refresh();
        }

        private void CopyPath()
        {
            var text = PathFormatter.ToCSharp(this.currentRootKind, this.GetActiveSegments());
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Clipboard.SetText(text);
            this.Status = "Copied " + text;
        }

        private void CopyValue()
        {
            if (this.SelectedNode == null)
            {
                return;
            }

            Clipboard.SetText(this.SelectedNode.ValueText ?? string.Empty);
            this.Status = "Copied value.";
        }

        private void SyncPathFromNode(InspectorNode node)
        {
            var segments = node.GetPathSegments();
            this.PathText = PathFormatter.ToDisplay(segments);
            this.RebuildBreadcrumbs(segments);
            this.NotifyPathCommands();
        }

        private void RebuildBreadcrumbs(IReadOnlyList<PathSegment>? segments = null)
        {
            segments = segments ?? this.GetActiveSegments();
            this.Breadcrumbs.Clear();
            var soFar = new List<PathSegment>();
            for (var i = 0; i < segments.Count; i++)
            {
                soFar.Add(segments[i]);
                this.Breadcrumbs.Add(new BreadcrumbItem(
                    segments[i].Value,
                    PathFormatter.ToDisplay(soFar),
                    i == segments.Count - 1));
            }
        }

        private IReadOnlyList<PathSegment> GetActiveSegments()
        {
            if (this.SelectedNode != null)
            {
                return this.SelectedNode.GetPathSegments();
            }

            return PathFormatter.SplitDisplay(this.PathText).Select(PathFormatter.FromToken).ToList();
        }

        private void SelectNode(InspectorNode node)
        {
            this.suppressPathSync = true;
            this.ClearSelection(this.InspectorRoots.FirstOrDefault());
            node.IsSelected = true;
            this.selectedNode = node;
            this.OnPropertyChanged(nameof(this.SelectedNode));
            this.suppressPathSync = false;
            this.SyncPathFromNode(node);
        }

        private void ClearSelection(InspectorNode? node)
        {
            if (node == null)
            {
                return;
            }

            node.IsSelected = false;
            foreach (var child in node.Children)
            {
                this.ClearSelection(child);
            }
        }

        private bool MatchesRoot(string part, InspectorNode root)
        {
            return string.Equals(part, root.PathSegment, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(part, this.selectedType?.FullName, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(part, this.selectedType?.Name, StringComparison.OrdinalIgnoreCase);
        }

        private void ClearInspection()
        {
            this.TypeList.Reset();
            this.InspectorRoots.Clear();
            this.Breadcrumbs.Clear();
            this.selectedType = null;
            this.OnPropertyChanged(nameof(this.SelectedType));
            this.selectedNode = null;
            this.OnPropertyChanged(nameof(this.SelectedNode));
            this.PathText = string.Empty;
            this.NotifyPathCommands();
        }

        private void DisposeImage()
        {
            try
            {
                this.image?.Dispose();
            }
            catch
            {
                // Ignore dispose failures from a dying process.
            }

            this.image = null;
            this.IsAttached = false;
        }

        private void CancelAttach()
        {
            try
            {
                this.attachCts?.Cancel();
            }
            catch
            {
                // ignored
            }

            this.attachCts?.Dispose();
            this.attachCts = null;
        }

        private void AppendLog(string? line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            var text = line!;
            void Add()
            {
                this.LogLines.Add(text);
                while (this.LogLines.Count > 400)
                {
                    this.LogLines.RemoveAt(0);
                }
            }

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                Add();
                return;
            }

            dispatcher.BeginInvoke(Add);
        }

        private sealed class CatalogResult
        {
            public CatalogResult(List<TypeRowViewModel> types)
            {
                this.Types = types;
            }

            public List<TypeRowViewModel> Types { get; }
        }

        private void NotifyPathCommands()
        {
            this.ApplyPathCommand.NotifyCanExecuteChanged();
            this.PathBackCommand.NotifyCanExecuteChanged();
            this.RefreshNodeCommand.NotifyCanExecuteChanged();
            this.CopyPathCommand.NotifyCanExecuteChanged();
            this.CopyValueCommand.NotifyCanExecuteChanged();
        }
    }
}
