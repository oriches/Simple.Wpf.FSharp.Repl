﻿namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using ViewModels;

    /// <summary>
    /// Controller for the REPL engine UI, exposes the ViewModel.
    /// </summary>
    public sealed class ReplWindowController : IReplWindowController, IDisposable
    {
        private readonly string _startupScript;
        private readonly IReplEngine _replEngine;
        private readonly IScheduler _dispatcherScheduler;
        private readonly IScheduler _taskPoolScheduler;
        private readonly CompositeDisposable _disposable;

        private IReplWindowViewModel _viewModel;

        /// <summary>
        /// Creates an instance of the controller.
        /// </summary>
        /// <param name="startupScript">The script to run at startup, default is null.</param>
        /// <param name="replEngine">The REPL engine, default null.</param>
        /// <param name="dispatcherScheduler">The Reactive extensions shceduler for the UI thread (dispatcher).</param>
        /// <param name="taskScheduler">The Reactive extensiosn scheduler for the task pool scheduler.</param>
        public ReplWindowController(string startupScript = null,
            IReplEngine replEngine = null,
            IScheduler dispatcherScheduler = null,
            IScheduler taskScheduler = null)
        {
            _startupScript = startupScript;
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

        /// <summary>
        /// The ViewModel for the REPL engine.
        /// </summary>
        public IReplWindowViewModel ViewModel
        {
            get { return _viewModel ?? (_viewModel = CreateViewModelAndStartEngine()); }
        }

        /// <summary>
        /// Disposes the controller.
        /// </summary>
        public void Dispose()
        {
            _disposable.Dispose();
        }

        private IReplWindowViewModel CreateViewModelAndStartEngine()
        {
            var errorStream = _replEngine.Error
                        .Select(x => new ReplLineViewModel(x, true))
                        .ObserveOn(_dispatcherScheduler);

            var outputStream = _replEngine.Output
                        .Select(x => new ReplLineViewModel(x))
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

            _replEngine.Start(_startupScript);

            return viewModel;
        }
    }
}