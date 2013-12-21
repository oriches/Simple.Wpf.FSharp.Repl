namespace Simple.Wpf.FSharp.Repl.Views
{
    using System;

    public sealed class LineEventArgs : EventArgs
    {
        public string Script { get; private set; }

        public LineEventArgs(string script)
        {
            Script = script;
        }
    }
}