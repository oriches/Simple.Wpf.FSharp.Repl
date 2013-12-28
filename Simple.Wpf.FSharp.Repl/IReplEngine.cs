namespace Simple.Wpf.FSharp.Repl
{
    using System;


    public interface IReplEngine
    {
        IObservable<State> State { get; }

        IObservable<string> Error { get; }

        IObservable<string> Output { get; }
        
        IReplEngine Start(string script = null);

        IReplEngine Stop();

        IReplEngine Reset();

        IReplEngine Execute(string script);
    }
}
