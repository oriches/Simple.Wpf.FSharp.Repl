using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;
using Simple.Wpf.FSharp.Repl.Common.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.Common.UI.Controllers
{
    /// <summary>
    ///     Controller for the REPL engine UI, exposes the ViewModel.
    /// </summary>
    public abstract class BaseReplEngineController : IReplEngineController
    {
        private readonly IReplEngine _replEngine;
        private readonly string _startupScript;
        protected readonly IScheduler DispatcherScheduler;
        protected readonly CompositeDisposable Disposable;
        protected readonly IProcessService ProcessService;
        protected readonly IScheduler TaskPoolScheduler;
        private IReplEngineViewModel _viewModel;

        /// <summary>
        ///     Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="replEngine">The REPL engine.</param>
        /// <param name="dispatcherScheduler">The Reactive extensions scheduler for the UI thread (dispatcher).</param>
        /// <param name="taskScheduler">The Reactive extension scheduler for the task pool scheduler.</param>
        /// <param name="processService">Service for starting windows processes.</param>
        protected BaseReplEngineController(string startupScript,
            IReplEngine replEngine,
            IProcessService processService,
            IScheduler dispatcherScheduler,
            IScheduler taskScheduler)
        {
            _startupScript = startupScript;

            DispatcherScheduler = dispatcherScheduler;
            TaskPoolScheduler = taskScheduler;
            ProcessService = processService;
            Disposable = new CompositeDisposable();

            _replEngine = replEngine;
            Disposable.Add(_replEngine);
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
            Disposable.Dispose();
        }

        private IReplEngineViewModel CreateViewModelAndStartEngine()
        {
            var viewModel = CreateViewModel(_replEngine);

            Disposable.Add(viewModel.Reset
                .ObserveOn(TaskPoolScheduler)
                .Subscribe(_ => _replEngine.Reset()));

            Disposable.Add(viewModel.Execute
                .ObserveOn(TaskPoolScheduler)
                .Subscribe(x => _replEngine.Execute(x)));

            _replEngine.Start(_startupScript);

            return viewModel;
        }

        protected abstract IReplEngineViewModel CreateViewModel(IReplEngine replEngine);
    }
}