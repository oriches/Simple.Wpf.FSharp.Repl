using System;
using System.Reactive;

namespace Simple.Wpf.FSharp.Repl.UI.ViewModels
{
    /// <summary>
    ///     ViewModel for the REPL engine
    /// </summary>
    public interface IReplEngineViewModel
    {
        /// <summary>
        ///     The REPL engine working directory.
        /// </summary>
        string WorkingDirectory { get; }

        /// <summary>
        ///     Reset requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        IObservable<Unit> Reset { get; }

        /// <summary>
        ///     Execution requests as a Reactive extensions stream, this is consumed by the controller.
        /// </summary>
        IObservable<string> Execute { get; }
    }
}