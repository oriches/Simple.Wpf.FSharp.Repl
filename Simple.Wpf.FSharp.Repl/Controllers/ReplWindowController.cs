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
        private readonly CompositeDisposable _disposable;

        private IReplWindowViewModel _viewModel;

        public ReplWindowController(IReplEngine replEngine = null, IScheduler dispatcherScheduler = null, IScheduler taskScheduler = null)
        {
            _disposable = new CompositeDisposable();

            _replEngine = replEngine ?? CreateEngine();
            _dispatcherScheduler = dispatcherScheduler ?? DispatcherScheduler.Current;
            _taskPoolScheduler = taskScheduler ?? TaskPoolScheduler.Default;
        }

        private IReplEngine CreateEngine()
        {
            var replEngine = new ReplEngine();
            _disposable.Add(replEngine);

            return replEngine;
        }

        public IReplWindowViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = CreateViewModelAndStartEngine()); }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private IReplWindowViewModel CreateViewModelAndStartEngine()
        {
            var errorStream = _replEngine.Error
                        .Select(x => new ReplOuputViewModel(x, true))
                        .ObserveOn(_dispatcherScheduler);

            var outputStream = _replEngine.Output
                        .Select(x => new ReplOuputViewModel(x))
                        .ObserveOn(_dispatcherScheduler);

            var stateStream = _replEngine.State
                       .ObserveOn(_dispatcherScheduler);

            IReplWindowViewModel viewModel = new ReplWindowViewModel(stateStream, outputStream, errorStream);

            _disposable.Add(viewModel.Reset
               .ObserveOn(_taskPoolScheduler)
               .Subscribe(_ => _replEngine.Reset()));

            _disposable.Add(viewModel.Execute
               .ObserveOn(_taskPoolScheduler)
               .Subscribe(x => _replEngine.Execute(x)));

            _replEngine.Start("let answer = 42.00;;");

            return viewModel;
        }
    }
}