namespace Simple.Wpf.FSharp.Repl.UI.Controllers
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using Core;
    using ViewModels;

    /// <summary>
    /// Controller for the REPL engine UI, exposes the ViewModel.
    /// </summary>
    public sealed class ReplEngineController : IReplEngineController, IDisposable
    {
        private readonly string _startupScript;
        private readonly IReplEngine _replEngine;
        private readonly IScheduler _dispatcherScheduler;
        private readonly IScheduler _taskPoolScheduler;
        private readonly CompositeDisposable _disposable;

        private IReplEngineViewModel _viewModel;

        /// <summary>
        /// Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="baseDirectory">The base directory, default is null.</param>
        public ReplEngineController(string startupScript, string baseDirectory)
            : this(startupScript, baseDirectory, null, null, null)
        {
        }

        /// <summary>
        /// Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        public ReplEngineController(string startupScript)
            : this(startupScript, null, null, null, null)
        {
        }

        /// <summary>
        /// Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="baseDirectory">The base directory, default is null.</param>
        /// <param name="replEngine">The REPL engine.</param>
        /// <param name="dispatcherScheduler">The Reactive extensions shceduler for the UI thread (dispatcher).</param>
        /// <param name="taskScheduler">The Reactive extensiosn scheduler for the task pool scheduler.</param>
        public ReplEngineController(string startupScript = null,
            string baseDirectory = null,
            IReplEngine replEngine = null,
            IScheduler dispatcherScheduler = null,
            IScheduler taskScheduler = null)
        {
            _startupScript = startupScript;
            _disposable = new CompositeDisposable();

            _replEngine = replEngine ?? CreateEngine(baseDirectory);
            _dispatcherScheduler = dispatcherScheduler ?? DispatcherScheduler.Current;
            _taskPoolScheduler = taskScheduler ?? TaskPoolScheduler.Default;
        }

        private IReplEngine CreateEngine(string workingDirectory)
        {
            var replEngine = new ReplEngine(workingDirectory);
            _disposable.Add(replEngine);

            return replEngine;
        }

        /// <summary>
        /// The ViewModel for the REPL engine.
        /// </summary>
        public IReplEngineViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = CreateViewModelAndStartEngine()); }
        }

        /// <summary>
        /// The working directory for the REPL engine.
        /// </summary>
        public IObservable<string> WorkingDirectory
        {
            get { return _replEngine.WorkingDirectory.ObserveOn(_dispatcherScheduler); }
        }

        /// <summary>
        /// Execute the script
        /// </summary>
        /// <param name="script">The script to execute.</param>
        public void Execute(string script)
        {
            _replEngine.Execute(script);
        }

        /// <summary>
        /// Disposes the controller.
        /// </summary>
        public void Dispose()
        {
            _disposable.Dispose();
        }

        private IReplEngineViewModel CreateViewModelAndStartEngine()
        {
            var errorStream = _replEngine.Error
                        .Select(x => new ReplLineViewModel(x, true))
                        .ObserveOn(_dispatcherScheduler);

            var outputStream = _replEngine.Output
                        .Select(x => new ReplLineViewModel(x))
                        .ObserveOn(_dispatcherScheduler);

            var stateStream = _replEngine.State
                       .ObserveOn(_dispatcherScheduler);

            var workingDirectoryStream = _replEngine.WorkingDirectory
                .ObserveOn(_dispatcherScheduler);

            IReplEngineViewModel viewModel = new ReplEngineViewModel(stateStream, outputStream, errorStream, workingDirectoryStream);

            _disposable.Add(viewModel.Reset
               .ObserveOn(_taskPoolScheduler)
               .Subscribe(_ => _replEngine.Reset()));

            _disposable.Add(viewModel.Execute
               .ObserveOn(_taskPoolScheduler)
               .Subscribe(x => _replEngine.Execute(x)));

            _replEngine.Start(_startupScript);

            return viewModel;
        }
    }
}