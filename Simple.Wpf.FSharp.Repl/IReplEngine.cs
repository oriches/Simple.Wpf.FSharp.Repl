namespace Simple.Wpf.FSharp.Repl
{
    using System;

    public interface IReplEngine : IDisposable
    {
        IObservable<string> Output { get; }
        
        IReplEngine Start(string script = null);

        IReplEngine Stop();

        IReplEngine Reset();

        IReplEngine Execute(string script);
    }
}
