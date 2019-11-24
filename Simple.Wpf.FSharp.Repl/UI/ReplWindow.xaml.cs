using System;
using System.Windows;
using System.Windows.Controls;
using Simple.Wpf.FSharp.Repl.UI.Controllers;

namespace Simple.Wpf.FSharp.Repl.UI
{
    /// <summary>
    ///     REPL engine window containing the REPL engine UI.
    /// </summary>
    public partial class ReplWindow : UserControl
    {
        /// <summary>
        ///     Optional startup script dependency property, used when the REPL engine starts.
        /// </summary>
        public static readonly DependencyProperty StartUpScriptProperty = DependencyProperty.Register("StartUpScript",
            typeof(string),
            typeof(ReplWindow),
            new PropertyMetadata(default(string)));

        /// <summary>
        ///     The working directory dependency property, used when the REPL engine starts.
        /// </summary>
        public static readonly DependencyProperty WorkingDirectoryProperty = DependencyProperty.Register(
            "WorkingDirectory",
            typeof(string),
            typeof(ReplWindow),
            new PropertyMetadata(default(string)));

        private ReplEngineController _controller;

        /// <summary>
        ///     Creates an instance of the Repl window user control.
        /// </summary>
        public ReplWindow()
        {
            InitializeComponent();

            ReplEngine.DataContext = null;

            Loaded += OnLoaded;
            Dispatcher.ShutdownStarted += DispatcherOnShutdownStarted;
        }

        /// <summary>
        ///     The startup script property.
        /// </summary>
        public string StartUpScript
        {
            get => (string) GetValue(StartUpScriptProperty);
            set => SetValue(StartUpScriptProperty, value);
        }

        /// <summary>
        ///     The current working directory property.
        /// </summary>
        public string WorkingDirectory
        {
            get => (string) GetValue(WorkingDirectoryProperty);
            set => SetValue(WorkingDirectoryProperty, value);
        }

        /// <summary>
        ///     Execute a script with the REPL engine.
        /// </summary>
        /// <param name="script">The script to execute.</param>
        public void ExecuteScript(string script)
        {
            if (_controller != null) _controller.Execute(script);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_controller == null) _controller = new ReplEngineController(StartUpScript, WorkingDirectory);

            ReplEngine.DataContext = _controller.ViewModel;
        }

        private void DispatcherOnShutdownStarted(object sender, EventArgs eventArgs)
        {
            if (_controller != null) _controller.Dispose();
        }
    }
}