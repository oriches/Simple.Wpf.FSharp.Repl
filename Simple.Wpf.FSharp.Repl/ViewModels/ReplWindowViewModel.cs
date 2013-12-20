namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Subjects;
    using System.Windows.Input;
    using Commands;

    public sealed class ReplWindowViewModel : IReplWindowViewModel, IDisposable
    {
        private readonly IDisposable _disposable;
        private readonly ObservableCollection<ReplOuputViewModel> _output;
        private readonly Subject<Unit> _reset;

        public ReplWindowViewModel(IObservable<ReplOuputViewModel> replOutputObservable)
        {
            _output = new ObservableCollection<ReplOuputViewModel>();

            _reset = new Subject<Unit>();

            ClearCommand = new ReplRelayCommand(Clear, CanClear);
            ResetCommand = new ReplRelayCommand(ResetImpl);

            _disposable = replOutputObservable
                .Subscribe(x => _output.Add(x));
        }

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
            _reset.OnNext(Unit.Default);
        }
    }
}