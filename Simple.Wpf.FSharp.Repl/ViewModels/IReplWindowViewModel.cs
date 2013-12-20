namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System;
    using System.Reactive;

    public interface IReplWindowViewModel 
    {
        IObservable<Unit> Reset { get; }     
    }
}