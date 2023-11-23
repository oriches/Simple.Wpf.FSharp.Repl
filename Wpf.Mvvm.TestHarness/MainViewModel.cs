using System;
using Simple.Wpf.FSharp.Repl.UI.Controllers;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Wpf.Mvvm.TestHarness
{
    public sealed class MainViewModel : IDisposable
    {
        private readonly IReplEngineController _controller;

        public MainViewModel() => _controller = new ReplEngineController("let ollie = 1337;;", @"C:\temp\fsharp");

        public IReplEngineViewModel Content => _controller.ViewModel;

        public void Dispose() => _controller.Dispose();
    }
}