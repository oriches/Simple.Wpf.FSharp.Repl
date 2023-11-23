using System;
using System.Windows;

namespace Wpf.Mvvm.TestHarness
{
    public partial class App : Application
    {
        private readonly MainViewModel _mainViewModel;

        public App()
        {
            _mainViewModel = new MainViewModel();
            var mainWindow = new MainWindow { DataContext = _mainViewModel };

            Current.MainWindow = mainWindow;
            mainWindow.Show();
            mainWindow.Closed += MainWindowOnClosed;
        }

        private void MainWindowOnClosed(object sender, EventArgs eventArgs) => _mainViewModel.Dispose();
    }
}