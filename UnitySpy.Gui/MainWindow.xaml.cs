using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using HackF5.UnitySpy.Gui.ViewModels;

namespace HackF5.UnitySpy.Gui
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.viewModel = new MainViewModel();
            this.DataContext = this.viewModel;
            this.viewModel.LogLines.CollectionChanged += this.LogLinesOnCollectionChanged;
            this.Closed += (_, __) =>
            {
                this.viewModel.LogLines.CollectionChanged -= this.LogLinesOnCollectionChanged;
                this.viewModel.Dispose();
            };
            this.Loaded += async (_, __) => await this.viewModel.RefreshProcessesAsync();
        }

        private void InspectorTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.viewModel.SelectedNode = e.NewValue as InspectorNode;
        }

        private void LogLinesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.LogList.Items.Count == 0)
            {
                return;
            }

            this.LogList.ScrollIntoView(this.LogList.Items[this.LogList.Items.Count - 1]);
        }
    }
}
