namespace Simple.Wpf.FSharp.Repl.UI.Services
{
    using System.Diagnostics;

    public class ProcessService : IProcessService
    {
        public void Start(string directory)
        {
            Process.Start(directory);
        }
    }
}