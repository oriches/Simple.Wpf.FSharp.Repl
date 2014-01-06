namespace Simple.Wpf.FSharp.Repl.Core
{
    /// <summary>
    /// REPL engine state.
    /// </summary>
    public enum State
    {
        /// <summary>
        /// REPL engine is in an unknown state
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// REPL engine has errored and entered a faulted state.
        /// </summary>
        Faulted = 0,

        /// <summary>
        /// REPL engine is starting up.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// REPL engine is running and waiting to execute a script.
        /// </summary>
        Running = 2,

        /// <summary>
        /// REPL engine is stopping.
        /// </summary>
        Stopping = 4,

        /// <summary>
        /// REPL engine has stopped.
        /// </summary>
        Stopped = 8,

        /// <summary>
        /// REPL engine is executing a script.
        /// </summary>
        Executing = 16,
    }
}