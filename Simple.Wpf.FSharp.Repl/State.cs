namespace Simple.Wpf.FSharp.Repl
{
    public enum State
    {
        Faulted = 0,
        Starting = 1,
        Running = 2,
        Stopping = 4,
        Stopped = 8,
        Executing = 16,
    }
}