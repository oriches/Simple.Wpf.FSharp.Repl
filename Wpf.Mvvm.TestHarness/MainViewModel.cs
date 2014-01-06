namespace Wpf.Mvvm.TestHarness
{
    using Simple.Wpf.FSharp.Repl.UI.Controllers;
    using Simple.Wpf.FSharp.Repl.UI.ViewModels;

    public sealed class MainViewModel
    {
        private readonly IReplEngineController _controller;

        public MainViewModel()
        {
            _controller = new ReplEngineController("let ollie = 1337;;");
        }

        public IReplEngineViewModel Content { get { return _controller.ViewModel; } }
    }
}