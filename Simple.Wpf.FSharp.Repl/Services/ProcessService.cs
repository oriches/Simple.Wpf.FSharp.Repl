namespace Simple.Wpf.FSharp.Repl.Services
{
    public sealed class ProcessService : IProcessService
    {
        public IProcess StartWindowsExplorer(string directory)
        {
            return new Process(System.Diagnostics.Process.Start(directory));
        }

        public IProcess StartReplExecutable(string workingDirectory, string executableDirectory)
        {
            return new Process(new System.Diagnostics.Process
            {
                StartInfo =
                {
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = workingDirectory,
                    FileName = executableDirectory
                }
            });
        }
    }
}