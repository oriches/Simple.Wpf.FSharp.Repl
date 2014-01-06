namespace Simple.Wpf.FSharp.Repl.UI
{
    using System;

    /// <summary>
    /// REPL engine line event.
    /// </summary>
    public sealed class LineEventArgs : EventArgs
    {
        /// <summary>
        /// The line output by the REPL engine.
        /// </summary>
        public string Line { get; private set; }

        /// <summary>
        /// Constructor for REPL engine line event args.
        /// </summary>
        /// <param name="line">The REPL engine line.</param>
        public LineEventArgs(string line)
        {
            Line = line;
        }
    }
}