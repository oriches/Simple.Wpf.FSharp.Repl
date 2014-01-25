namespace Simple.Wpf.FSharp.Repl.UI.ViewModels
{
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
    using Commands;
    using Core;
    using Services;

    /// <summary>
    /// ViewModel for the REPL engine.
    /// </summary>
    public sealed class ReplEngineViewModel : BaseViewModel, IReplEngineViewModel, IDisposable
    {
        private const string PromptText = "> ";

        private readonly string _workingDirectory;
        private readonly IProcessService _processService;
        private readonly CompositeDisposable _disposable;
        private readonly ObservableCollection<ReplLineViewModel> _output;
        private readonly Subject<Unit> _reset;
        private readonly Subject<string> _execute;
        
        private State _state;

        /// <summary>
        /// Creates an instance of the REPL engine ViewModel.
        /// </summary>
        /// <param name="replState">Reactive extensions stream of the REPL engine state.</param>
        /// <param name="replOutput">Reactive extensions stream of the REPL engine output.</param>
        /// <param name="replError">Reactive extensions stream of the REPL engine errors.</param>
        /// <param name="workingDirectory">Reactive extensions stream of the REPL engine working directory.</param>
        /// <param name="processService">Handles starting windows processes.</param>
        public ReplEngineViewModel(IObservable<State> replState,
            IObservable<ReplLineViewModel> replOutput,
            IObservable<ReplLineViewModel> replError,
            string workingDirectory,
            IProcessService processService)
        {
            _workingDirectory = workingDirectory;
            _processService = processService;
            _state = Core.State.Unknown;
            _output = new ObservableCollection<ReplLineViewModel>();

            _reset = new Subject<Unit>();
            _execute = new Subject<string>();

            ClearCommand = new ReplRelayCommand(Clear, CanClear);
            ResetCommand = new ReplRelayCommand(ResetImpl, CanReset);
            ExecuteCommand = new ReplRelayCommand<string>(ExecuteImpl, CanExecute);
            OpenWorkingFolderCommand = new ReplRelayCommand(OpenWorkingFolder);

            _disposable = new CompositeDisposable
            {
                Disposable.Create(() =>
                                  {
                                        ClearCommand = null;
                                        ResetCommand = null;
                                        ExecuteCommand = null;
                                  }),
                _reset,
                _execute,
                replState.Subscribe(UpdateState),
                replOutput.Where(x => x.Value != Prompt)
                    .Subscribe(x =>
                    {
                        _output.Add(x);
                        CommandManager.InvalidateRequerySuggested();
                    }),
                replError.Where(x => x.Value != Prompt)
                    .Subscribe(x =>
                    {
                        _output.Add(x);
                        CommandManager.InvalidateRequerySuggested();
                    })
            };
        }

        /// <summary>
        /// The REPL engine prompt.
        /// </summary>
        public string Prompt
        {
            get
            {
                return _state == Core.State.Running ? PromptText : string.Empty;
            }
        }

        /// <summary>
        /// The REPL engine state.
        /// </summary>
        public string State
        {
            get
            {
                return _state == Core.State.Running
                    || _state == Core.State.Stopped
                    || _state == Core.State.Unknown
                    ? string.Empty
                    : string.Intern(_state.ToString());
            }
        }

        /// <summary>
        /// The REPL engine working directory.
        /// </summary>
        public string WorkingDirectory { get { return _workingDirectory; } }

        /// <summary>
        /// Reset requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        public IObservable<Unit> Reset { get { return _reset; } }

        /// <summary>
        /// Execution requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        public IObservable<string> Execute { get { return _execute; } }

        /// <summary>
        /// The aggregated output from the REPL engine.
        /// </summary>
        public IEnumerable<ReplLineViewModel> Output { get { return _output; } }

        /// <summary>
        /// Clear the output command.
        /// </summary>
        public ICommand ClearCommand { get; private set; }

        /// <summary>
        /// Reset the REPL engine commnad.
        /// </summary>
        public ICommand ResetCommand { get; private set; }

        /// <summary>
        /// Executes the REPL engine commnad.
        /// </summary>
        public ICommand ExecuteCommand { get; private set; }

        /// <summary>
        /// Opens the working folder.
        /// </summary>
        public ICommand OpenWorkingFolderCommand { get; private set; }

        /// <summary>
        /// Is the REPL engine UI in read only mode.
        /// </summary>
        public bool IsReadOnly { get { return _state == Core.State.Executing; } }

        /// <summary>
        /// Disposes the ViewModel.
        /// </summary>
        public void Dispose()
        {
            _disposable.Dispose();
        }

        private bool CanClear()
        {
            return _output.Any();
        }

        private void Clear()
        {
            _output.Clear();
        }

        private bool CanReset()
        {
            return _state == Core.State.Running;
        }

        private void ResetImpl()
        {
            _output.Clear();
            _reset.OnNext(Unit.Default);
        }

        private bool CanExecute(string arg)
        {
            return _state == Core.State.Running || _state == Core.State.Executing;
        }

        private void ExecuteImpl(string line)
        {
            var preparedLine = line;
            if (!line.EndsWith(Environment.NewLine))
            {
                preparedLine += Environment.NewLine;
            }

            _output.Add(new ReplLineViewModel(Prompt + preparedLine));

            _execute.OnNext(preparedLine);
        }

        private void UpdateState(State state)
        {
            if (_state != state)
            {
                Debug.WriteLine("state = " + state);

                _state = state;
                OnPropertyChanged("State");
                OnPropertyChanged("Prompt");
                OnPropertyChanged("IsReadOnly");
            }
        }

        private void OpenWorkingFolder()
        {
            _processService.StartWindowsExplorer(_workingDirectory);
        }
    }
}