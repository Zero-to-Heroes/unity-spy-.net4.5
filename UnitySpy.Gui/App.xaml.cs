using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HackF5.UnitySpy.Gui
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
            TaskScheduler.UnobservedTaskException += this.OnUnobservedTaskException;
            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Report("UI error: " + e.Exception);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Report("Unhandled error: " + e.ExceptionObject);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Report("Background error: " + e.Exception);
        }

        private static void Report(string message)
        {
            try
            {
                if (Current?.MainWindow?.DataContext is ViewModels.MainViewModel vm)
                {
                    vm.ReportError(message);
                    return;
                }
            }
            catch
            {
                // Fall through to a message box if the window is gone.
            }

            try
            {
                MessageBox.Show(message, "UnitySpy", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // ignored
            }
        }
    }
}
