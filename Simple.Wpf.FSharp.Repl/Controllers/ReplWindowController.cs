namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using ViewModels;

    public sealed class ReplWindowController : IReplWindowController, IDisposable
    {
        private readonly IReplEngine _replEngine;
        private readonly IScheduler _dispatcherScheduler;
        private readonly IScheduler _taskPoolScheduler;

        private IReplWindowViewModel _viewModel;
        private IDisposable _resetDisposable;

        public ReplWindowController(IReplEngine replEngine = null, IScheduler dispatcherScheduler = null, IScheduler taskScheduler = null)
        {
            _replEngine = replEngine ?? new ReplEngine();
            _dispatcherScheduler = dispatcherScheduler ?? DispatcherScheduler.Current;
            _taskPoolScheduler = taskScheduler ?? TaskPoolScheduler.Default;
        }

        public IReplWindowViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = CreateViewModelAndStartEngine()); }
        }

        public void Dispose()
        {
            _resetDisposable.Dispose();
            _replEngine.Dispose();
        }

        private IReplWindowViewModel CreateViewModelAndStartEngine()
        {
            var outputStream = _replEngine.Output
                        .Select(x => new ReplOuputViewModel(x))
                        .ObserveOn(_dispatcherScheduler);

            var stateStream = _replEngine.State
                       .ObserveOn(_dispatcherScheduler);

            IReplWindowViewModel viewModel = new ReplWindowViewModel(stateStream, outputStream);

            _resetDisposable = viewModel.Reset
               .ObserveOn(_taskPoolScheduler)
               .Subscribe(_ => _replEngine.Reset());

            _replEngine.Start();
            return viewModel;
        }
    }
}