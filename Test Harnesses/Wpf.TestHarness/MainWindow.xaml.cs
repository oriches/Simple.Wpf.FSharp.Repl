namespace Wpf.TestHarness
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Simple.Wpf.FSharp.Repl.Controllers;
    using Simple.Wpf.FSharp.Repl.Views;
    using Simple.Wpf.Themes;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ReplWindow.DataContext = new ReplWindowController("let ollie = 1337;;").ViewModel;

            ThemeControl.Scope = ReplWindow;
            ThemeControl.ItemsSource = new[]
            {
                new Theme("Default theme", null), 
                new Theme("Blue theme", new Uri("/Wpf.TestHarness;component/Themes/BlueTheme.xaml", UriKind.Relative)), 
                new Theme("Dark theme", new Uri("/Wpf.TestHarness;component/Themes/DarkTheme.xaml", UriKind.Relative)), 
                new Theme("Wingding theme", new Uri("/Wpf.TestHarness;component/Themes/WingdingTheme.xaml", UriKind.Relative)), 
            };
        }
    }
}
