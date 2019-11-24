namespace Simple.Wpf.FSharp.Repl.Services
{
    /// <summary>
    ///     Service starting .Net System.Diagnostics.Process instances for F# REPL engine &amp; Windows Explorer.
    /// </summary>
    public sealed class ProcessService : IProcessService
    {
        /// <summary>
        ///     Start an instance of Windows Explorer at the directory specified.
        /// </summary>
        /// <param name="directory">The directory to open.</param>
        /// <returns>The started process.</returns>
        public IProcess StartWindowsExplorer(string directory)
        {
            return new Process(System.Diagnostics.Process.Start(directory));
        }

        /// <summary>
        ///     Start the F# REPL process.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the F# REPL process.</param>
        /// <param name="executableDirectory">The executable path for the F# REPL process.</param>
        /// <returns>The started F# REPL process.</returns>
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