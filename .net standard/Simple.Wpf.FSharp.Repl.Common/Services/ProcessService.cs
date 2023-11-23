using System.Diagnostics;

namespace Simple.Wpf.FSharp.Repl.Common.Services
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
        /// <param name="executableName">The executable path for the F# REPL process.</param>
        /// <returns>The started F# REPL process.</returns>
        public IProcess StartReplExecutable(string workingDirectory, string executableName)
        {
            var startInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                WorkingDirectory = "C:\\Temp\\.net core 3.1.0\\fsharp",
                FileName = "dotnet \"C:\\Temp\\.net core 3.1.0\\fsharp\\fsi.exe\""
            };

            var process = new Process(new System.Diagnostics.Process
            {
                StartInfo = startInfo
            });

            return process;
        }
    }
}