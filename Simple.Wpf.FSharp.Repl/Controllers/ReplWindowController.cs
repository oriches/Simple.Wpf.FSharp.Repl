namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using ViewModels;

    public sealed class ReplWindowController
    {
        private readonly IReplEngine _replEngine;
        private readonly IScheduler _scheduler;

        private ReplWindowViewModel _viewModel;

        public ReplWindowController(IReplEngine replEngine, IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? DispatcherScheduler.Current;
            _replEngine = replEngine;
        }

        public ReplWindowViewModel ViewModel
        {
            get
            {
                if (_viewModel == null)
                {
                    var outputStream = _replEngine.Output
                        .Select(x => new ReplOuputViewModel(x))
                        .ObserveOn(_scheduler);

                    _replEngine.Start();

                    _viewModel = new ReplWindowViewModel(outputStream);
                }

                return _viewModel;
            }
        }
    }
}