namespace Simple.Wpf.FSharp.Repl.Common.Services
{
    /// <summary>
    ///     Service starting .Net System.Diagnostics.Process instances for F# REPL engine &amp; Windows Explorer.
    /// </summary>
    public interface IProcessService
    {
        /// <summary>
        ///     Starts an instance of Windows Explorer at the directory specified.
        /// </summary>
        /// <param name="directory">The directory to open.</param>
        /// <returns>The started process.</returns>
        IProcess StartWindowsExplorer(string directory);

        /// <summary>
        ///     Start the F# REPL process.
        /// </summary>
        /// <param name="workingDirectory">The working directory for the F# REPL process.</param>
        /// <param name="executableName">The executable path for the F# REPL process.</param>
        /// <returns>The started F# REPL process.</returns>
        IProcess StartReplExecutable(string workingDirectory, string executableName);
    }
}