namespace Simple.Wpf.FSharp.Repl.UI.Controllers
{
    using System;
    using UI.ViewModels;

    /// <summary>
    /// Controller for the REPL engine, exposes the ViewModel.
    /// </summary>
    public interface IReplEngineController : IDisposable
    {
        /// <summary>
        /// The ViewModel for the REPL engine.
        /// </summary>
        IReplEngineViewModel ViewModel { get; }

        /// <summary>
        /// The current working directory for the REPL engine.
        /// </summary>
        IObservable<string> WorkingDirectory { get; } 

        /// <summary>
        /// Execute the script
        /// </summary>
        /// <param name="script">The script to execute.</param>
        void Execute(string script);
    }
}