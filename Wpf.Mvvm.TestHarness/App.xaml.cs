namespace Wpf.Mvvm.TestHarness
{
    using System;
    using System.Windows;

    public partial class App : Application
    {
        private MainViewModel _mainViewModel;

        public App()
        {
            _mainViewModel = new MainViewModel();
            var mainWindow = new MainWindow { DataContext = _mainViewModel };

            Current.MainWindow = mainWindow;
            mainWindow.Show();
            mainWindow.Closed += MainWindowOnClosed;
        }

        private void MainWindowOnClosed(object sender, EventArgs eventArgs)
        {
            _mainViewModel.Dispose();
        }
    }
}
