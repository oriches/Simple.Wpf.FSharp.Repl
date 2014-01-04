namespace Wpf.Mvvm.TestHarness
{
    using System;
    using System.Windows;
    using Simple.Wpf.Themes;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ThemeControl.Scope = ReplWindow;
            ThemeControl.ItemsSource = new[]
            {
                new Theme("Default theme", null), 
                new Theme("Blue theme", new Uri("/Wpf.Mvvm.TestHarness;component/Themes/BlueTheme.xaml", UriKind.Relative)), 
                new Theme("Dark theme", new Uri("/Wpf.Mvvm.TestHarness;component/Themes/DarkTheme.xaml", UriKind.Relative)), 
                new Theme("Wingding theme", new Uri("/Wpf.Mvvm.TestHarness;component/Themes/WingdingTheme.xaml", UriKind.Relative)), 
            };
        }
    }
}
