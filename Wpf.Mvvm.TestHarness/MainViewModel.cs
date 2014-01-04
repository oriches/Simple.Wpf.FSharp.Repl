namespace Wpf.Mvvm.TestHarness
{
    using Simple.Wpf.FSharp.Repl.Controllers;
    using Simple.Wpf.FSharp.Repl.ViewModels;

    public sealed class MainViewModel
    {
        private readonly IReplWindowController _controller;

        public MainViewModel()
        {
            _controller = new ReplWindowController();
        }

        public IReplWindowViewModel Content { get { return _controller.ViewModel; } }
    }
}