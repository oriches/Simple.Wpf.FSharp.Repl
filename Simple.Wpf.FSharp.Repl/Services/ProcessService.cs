namespace Simple.Wpf.FSharp.Repl.Services
{
    /// <summary>
    /// Service wrapper around System.Diagnostics.Process instances.
    /// </summary>
    public sealed class ProcessService : IProcessService
    {
        /// <summary>
        /// Start an instance of windows explorer at the directory specified.
        /// </summary>
        /// <param name="directory">The directory to open.</param>
        /// <returns>The started process.</returns>
        public IProcess StartWindowsExplorer(string directory)
        {
            return new Process(System.Diagnostics.Process.Start(directory));
        }

        /// <summary>
        /// Start the REPL process.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the REPL process.</param>
        /// <param name="executableDirectory">The executable path for the REPL process.</param>
        /// <returns>The started process.</returns>
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