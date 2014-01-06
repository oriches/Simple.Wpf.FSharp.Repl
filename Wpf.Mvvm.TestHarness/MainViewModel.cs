namespace Wpf.Mvvm.TestHarness
{
    using System;
    using Simple.Wpf.FSharp.Repl.UI.Controllers;
    using Simple.Wpf.FSharp.Repl.UI.ViewModels;

    public sealed class MainViewModel : IDisposable
    {
        private readonly IReplEngineController _controller;

        public MainViewModel()
        {
            _controller = new ReplEngineController("let ollie = 1337;;", @"C:\temp\fsharp");
        }

        public IReplEngineViewModel Content { get { return _controller.ViewModel; } }
       
        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}