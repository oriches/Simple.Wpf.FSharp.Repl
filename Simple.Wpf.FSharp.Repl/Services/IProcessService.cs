namespace Simple.Wpf.FSharp.Repl.Services
{
    public interface IProcessService
    {
        IProcess StartWindowsExplorer(string directory);

        IProcess StartReplExecutable(string workingDirectory, string executableDirectory);
    }
}