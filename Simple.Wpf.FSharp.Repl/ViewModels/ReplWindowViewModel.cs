namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Disposables;
    using System.Reactive.Subjects;
    using System.Windows.Input;
    using Commands;

    public sealed class ReplWindowViewModel : BaseViewModel, IReplWindowViewModel, IDisposable
    {
        private readonly CompositeDisposable _disposable;
        private readonly ObservableCollection<ReplOuputViewModel> _output;
        private readonly Subject<Unit> _reset;
        private State _state;

        public ReplWindowViewModel(IObservable<State> replState, IObservable<ReplOuputViewModel> replOutput)
        {
            _state = Repl.State.Unknown;
            _output = new ObservableCollection<ReplOuputViewModel>();

            _reset = new Subject<Unit>();

            ClearCommand = new ReplRelayCommand(Clear, CanClear);
            ResetCommand = new ReplRelayCommand(ResetImpl);

            _disposable = new CompositeDisposable
            {
                replState.Subscribe(UpdateState),
                replOutput.Subscribe(x =>
                {
                    _output.Add(x);
                    CommandManager.InvalidateRequerySuggested();
                })
            };
        }

        public string State { get { return _state.ToString(); } }

        public IObservable<Unit> Reset { get { return _reset; } }

        public IEnumerable<ReplOuputViewModel> Output { get { return _output; } }

        public ICommand ClearCommand { get; private set; }

        public ICommand ResetCommand { get; private set; }

        public void Dispose()
        {
            ClearCommand = null;
            ResetCommand = null;

            _reset.Dispose();
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

        private void ResetImpl()
        {
            _output.Clear();
            _reset.OnNext(Unit.Default);
        }

        private void UpdateState(State state)
        {
            Debug.WriteLine("state = " + state);

            _state = state;
            OnPropertyChanged("State");

            // Enable\Disable commands etc...
        }
    }
}