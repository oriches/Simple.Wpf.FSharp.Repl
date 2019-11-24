using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Simple.Wpf.FSharp.Repl.Core;
using Simple.Wpf.FSharp.Repl.Services;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.UI.Controllers
{
    /// <summary>
    ///     Controller for the REPL engine UI, exposes the ViewModel.
    /// </summary>
    public sealed class ReplEngineController : IReplEngineController, IDisposable
    {
        private readonly IScheduler _dispatcherScheduler;
        private readonly CompositeDisposable _disposable;
        private readonly IProcessService _processService;
        private readonly IReplEngine _replEngine;
        private readonly string _startupScript;
        private readonly IScheduler _taskPoolScheduler;

        private IReplEngineViewModel _viewModel;

        /// <summary>
        ///     Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="workingDirectory">The working directory, default is null.</param>
        public ReplEngineController(string startupScript, string workingDirectory)
            : this(startupScript, workingDirectory, null)
        {
        }

        /// <summary>
        ///     Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        public ReplEngineController(string startupScript)
            : this(startupScript, null, null)
        {
        }

        /// <summary>
        ///     Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="workingDirectory">The working directory, default is null.</param>
        /// <param name="replEngine">The REPL engine.</param>
        /// <param name="processService">Service for starting windows processes.</param>
        /// <param name="dispatcherScheduler">The Reactive extensions shceduler for the UI thread (dispatcher).</param>
        /// <param name="taskScheduler">The Reactive extensiosn scheduler for the task pool scheduler.</param>
        public ReplEngineController(string startupScript = null,
            string workingDirectory = null,
            IReplEngine replEngine = null,
            IProcessService processService = null,
            IScheduler dispatcherScheduler = null,
            IScheduler taskScheduler = null)
        {
            _startupScript = startupScript;
            _processService = processService ?? new ProcessService();
            _disposable = new CompositeDisposable();

            _replEngine = replEngine ?? CreateEngine(workingDirectory);
            _dispatcherScheduler = dispatcherScheduler ?? DispatcherScheduler.Current;
            _taskPoolScheduler = taskScheduler ?? TaskPoolScheduler.Default;
        }

        /// <summary>
        ///     The ViewModel for the REPL engine.
        /// </summary>
        public IReplEngineViewModel ViewModel => _viewModel ?? (_viewModel = CreateViewModelAndStartEngine());

        /// <summary>
        ///     Execute the script
        /// </summary>
        /// <param name="script">The script to execute.</param>
        public void Execute(string script)
        {
            _replEngine.Execute(script);
        }

        /// <summary>
        ///     Disposes the controller.
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

            IReplEngineViewModel viewModel = new ReplEngineViewModel(stateStream, outputStream, errorStream,
                _replEngine.WorkingDirectory, _processService);

            _disposable.Add(viewModel.Reset
                .ObserveOn(_taskPoolScheduler)
                .Subscribe(_ => _replEngine.Reset()));

            _disposable.Add(viewModel.Execute
                .ObserveOn(_taskPoolScheduler)
                .Subscribe(x => _replEngine.Execute(x)));

            _replEngine.Start(_startupScript);

            return viewModel;
        }

        private IReplEngine CreateEngine(string workingDirectory)
        {
            var replEngine = new Core.ReplEngine(workingDirectory);
            _disposable.Add(replEngine);

            return replEngine;
        }
    }
}