using System;
using System.Windows;
using System.Windows.Input;
using Simple.Wpf.Themes.Common;

namespace Wpf.Mvvm.TestHarness
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ThemeControl.Scope = ReplEngine;
            ThemeControl.ItemsSource = new[]
            {
                new Theme("Default theme", null),
                new Theme("Default Red theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/DefaultRedTheme.xaml", UriKind.Relative)),
                new Theme("Default Green theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/DefaultGreenTheme.xaml", UriKind.Relative)),
                new Theme("Default Blue theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/DefaultBlueTheme.xaml", UriKind.Relative)),
                new Theme("Dark theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/DarkTheme.xaml", UriKind.Relative)),
                new Theme("Dark Blue theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/DarkBlueTheme.xaml", UriKind.Relative)),
                new Theme("Wingding theme",
                    new Uri("/Simple.Wpf.FSharp.Repl.Themes;component/WingdingTheme.xaml", UriKind.Relative))
            };

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Keyboard.Focus(ThemeControl);
        }
    }
}