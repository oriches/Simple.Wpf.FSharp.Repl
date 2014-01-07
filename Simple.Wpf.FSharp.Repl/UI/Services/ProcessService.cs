namespace Simple.Wpf.FSharp.Repl.UI.Services
{
    using System.Diagnostics;

    public sealed class ProcessService : IProcessService
    {
        public void Start(string directory)
        {
            Process.Start(directory);
        }
    }
}