namespace Simple.Wpf.FSharp.Repl.ViewModels
{
    using System;
    using System.Reactive;

    /// <summary>
    /// ViewModel for the REPL engine UI
    /// </summary>
    public interface IReplWindowViewModel 
    {
        /// <summary>
        /// Reset requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        IObservable<Unit> Reset { get; }

        /// <summary>
        /// Execution requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        IObservable<string> Execute { get; }     
    }
}