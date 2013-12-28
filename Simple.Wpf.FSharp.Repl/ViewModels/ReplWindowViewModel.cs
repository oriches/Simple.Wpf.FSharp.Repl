namespace Simple.Wpf.FSharp.Repl.ViewModels
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

    public sealed class ReplWindowViewModel : BaseViewModel, IReplWindowViewModel, IDisposable
    {
        private readonly CompositeDisposable _disposable;
        private readonly ObservableCollection<ReplLineViewModel> _output;
        private readonly Subject<Unit> _reset;
        private readonly Subject<string> _execute;
        
        private State _state;

        public ReplWindowViewModel(IObservable<State> replState, IObservable<ReplLineViewModel> replOutput, IObservable<ReplLineViewModel> replError)
        {
            _state = Repl.State.Unknown;
            _output = new ObservableCollection<ReplLineViewModel>();

            _reset = new Subject<Unit>();
            _execute = new Subject<string>();

            ClearCommand = new ReplRelayCommand(Clear, CanClear);
            ResetCommand = new ReplRelayCommand(ResetImpl, CanReset);
            ExecuteCommand = new ReplRelayCommand<string>(ExecuteImpl, CanExecute);

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

        public string Prompt { get { return "> "; } }

        public string State { get { return  _state == Repl.State.Executing ? "Executing" : string.Empty; } }

        public IObservable<Unit> Reset { get { return _reset; } }

        public IObservable<string> Execute { get { return _execute; } }

        public IEnumerable<ReplLineViewModel> Output { get { return _output; } }

        public ICommand ClearCommand { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public ICommand ExecuteCommand { get; private set; }

        public bool IsReadOnly { get { return _state == Repl.State.Executing; } }

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
            return _state == Repl.State.Running || _state == Repl.State.Executing;
        }

        private void ResetImpl()
        {
            _output.Clear();
            _reset.OnNext(Unit.Default);
        }

        private bool CanExecute(string arg)
        {
            return _state == Repl.State.Running || _state == Repl.State.Executing;
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
            Debug.WriteLine("state = " + state);

            _state = state;
            OnPropertyChanged("IsReadOnly");
            OnPropertyChanged("State");

            // Enable\Disable commands etc...
        }
    }
}