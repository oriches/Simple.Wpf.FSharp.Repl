namespace Simple.Wpf.FSharp.Repl.Services
{
    /// <summary>
    /// Service wrapper around System.Diagnostics.Process instances.
    /// </summary>
    public interface IProcessService
    {
        /// <summary>
        /// Start an instance of windows explorer at the directory specified.
        /// </summary>
        /// <param name="directory">The directory to open.</param>
        /// <returns>The started process.</returns>
        IProcess StartWindowsExplorer(string directory);

        /// <summary>
        /// Start the REPL process.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the REPL process.</param>
        /// <param name="executableDirectory">The executable path for the REPL process.</param>
        /// <returns>The started process.</returns>
        IProcess StartReplExecutable(string workingDirectory, string executableDirectory);
    }
}