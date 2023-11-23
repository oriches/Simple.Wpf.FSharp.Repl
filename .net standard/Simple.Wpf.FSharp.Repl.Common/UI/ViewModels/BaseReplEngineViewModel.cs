using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;

namespace Simple.Wpf.FSharp.Repl.Common.UI.ViewModels
{
    /// <summary>
    ///     ViewModel for the REPL engine.
    /// </summary>
    public abstract class BaseReplEngineViewModel : BaseViewModel, IReplEngineViewModel, IDisposable
    {
        protected const string PromptText = "> ";
        private readonly Subject<string> _execute;
        private readonly Subject<Unit> _reset;
        protected readonly IProcessService ProcessService;
        protected CompositeDisposable Disposable;
        protected ObservableCollection<ReplLineViewModel> OutputInternal;

        protected State StateInternal;

        protected BaseReplEngineViewModel(IObservable<State> replState,
            IObservable<ReplLineViewModel> replOutput,
            IObservable<ReplLineViewModel> replError,
            string workingDirectory,
            IProcessService processService)
        {
            WorkingDirectory = workingDirectory;
            ProcessService = processService;
            StateInternal = Core.State.Unknown;
            OutputInternal = new ObservableCollection<ReplLineViewModel>();

            _reset = new Subject<Unit>();
            _execute = new Subject<string>();

            Disposable = new CompositeDisposable
            {
                System.Reactive.Disposables.Disposable.Create(() =>
                {
                    ClearCommand = null;
                    ResetCommand = null;
                    ExecuteCommand = null;
                }),
                _reset,
                _execute,
                replState.Subscribe(UpdateState),
                replOutput.Where(x => x.Value != Prompt)
                    .Subscribe(x => { OutputInternal.Add(x); }),
                replError.Where(x => x.Value != Prompt)
                    .Subscribe(x => { OutputInternal.Add(x); })
            };
        }

        /// <summary>
        ///     Is the REPL engine UI in read only mode.
        /// </summary>
        public bool IsReadOnly => StateInternal == Core.State.Executing;

        /// <summary>
        ///     The REPL engine prompt.
        /// </summary>
        public string Prompt => StateInternal == Core.State.Running ? PromptText : string.Empty;

        /// <summary>
        ///     The REPL engine state.
        /// </summary>
        public string State =>
            StateInternal == Core.State.Running
            || StateInternal == Core.State.Stopped
            || StateInternal == Core.State.Unknown
                ? string.Empty
                : string.Intern(StateInternal.ToString());

        /// <summary>
        ///     Clear the output command.
        /// </summary>
        public ICommand ClearCommand { get; protected set; }

        /// <summary>
        ///     Reset the REPL engine command.
        /// </summary>
        public ICommand ResetCommand { get; protected set; }

        /// <summary>
        ///     Executes the REPL engine command.
        /// </summary>
        public ICommand ExecuteCommand { get; protected set; }

        /// <summary>
        ///     Opens the working folder.
        /// </summary>
        public ICommand OpenWorkingFolderCommand { get; protected set; }

        /// <summary>
        ///     The aggregated output from the REPL engine.
        /// </summary>
        public IEnumerable<ReplLineViewModel> Output => OutputInternal;

        public void Dispose()
        {
            Disposable.Dispose();
        }

        /// <summary>
        ///     The REPL engine working directory.
        /// </summary>
        public string WorkingDirectory { get; protected set; }

        /// <summary>
        ///     Reset requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        public IObservable<Unit> Reset => _reset;

        /// <summary>
        ///     Execution requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        public IObservable<string> Execute => _execute;

        protected bool CanClear()
        {
            return OutputInternal.Any();
        }

        protected void Clear()
        {
            OutputInternal.Clear();
        }

        protected bool CanReset()
        {
            return StateInternal == Core.State.Running;
        }

        protected void ResetImpl()
        {
            OutputInternal.Clear();
            _reset.OnNext(Unit.Default);
        }

        protected bool CanExecute(string arg)
        {
            return StateInternal == Core.State.Running || StateInternal == Core.State.Executing;
        }

        protected void ExecuteImpl(string line)
        {
            OutputInternal.Add(new ReplLineViewModel(Prompt + line));

            _execute.OnNext(line);
        }

        private void UpdateState(State state)
        {
            if (StateInternal != state)
            {
                Debug.WriteLine("state = " + state);

                StateInternal = state;
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(Prompt));
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }

        protected void OpenWorkingFolder()
        {
            ProcessService.StartWindowsExplorer(WorkingDirectory);
        }
    }
}