﻿namespace Simple.Wpf.FSharp.Repl.Commands
{
    using System;
    using System.Windows.Input;

    public sealed class ReplRelayCommand : ReplRelayCommand<object>
    {
        public ReplRelayCommand(Action execute)
            : base(x => execute(), x => true)
        {
        }

        public ReplRelayCommand(Action execute, Func<bool> canExecute) : base(x => execute(), x => canExecute())
        {
        }
    }

    public class ReplRelayCommand<T> : IReplRelayCommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        
        public ReplRelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute((T)parameter);    
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove  { CommandManager.RequerySuggested -= value; }
        }
    }
}