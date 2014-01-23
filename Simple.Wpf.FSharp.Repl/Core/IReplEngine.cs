namespace Simple.Wpf.FSharp.Repl.Core
{
    using System;

    /// <summary>
    /// Interface for the wrapper around the F# Interactive process.
    /// </summary>
    public interface IReplEngine
    {
        /// <summary>
        /// REPL engine state changes as a Reactive extensions stream.
        /// </summary>
        IObservable<State> State { get; }

        /// <summary>
        /// REPL engine errors as a Reactive extensions stream.
        /// </summary>
        IObservable<string> Error { get; }

        /// <summary>
        /// REPL engine output as a Reactive extensions stream.
        /// </summary>
        IObservable<string> Output { get; }

        /// <summary>
        /// REPL engine working directory.
        /// </summary>
        string WorkingDirectory { get; }
        
        /// <summary>
        /// Starts the REPL engine.
        /// </summary>
        /// <param name="script">The script to run at startup.</param>
        /// <returns>Returns the REPL engine.</returns>
        IReplEngine Start(string script = null);

        /// <summary>
        /// Stops the REPL engine.
        /// </summary>
        /// <returns>Returns the REPL engine.</returns>
        IReplEngine Stop();

        /// <summary>
        /// Reset the REPL engine if it has already been started.
        /// </summary>
        /// <returns>Returns the REPL engine.</returns>
        IReplEngine Reset();

        /// <summary>
        /// Executes the scripts, if the REPL engine has been started.
        /// </summary>
        /// <param name="script">The script to be executed.</param>
        /// <returns>Returns the REPL engine.</returns>
        IReplEngine Execute(string script);
    }
}
