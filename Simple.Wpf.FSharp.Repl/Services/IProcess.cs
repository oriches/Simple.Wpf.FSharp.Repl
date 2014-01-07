namespace Simple.Wpf.FSharp.Repl.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Wraps System.Diagnostics.Process
    /// </summary>
    public interface IProcess : IDisposable
    {
        /// <summary>
        /// Start the process.
        /// </summary>
        void Start();

        /// <summary>
        /// Wait for the process to exit.
        /// </summary>
        void WaitForExit();

        /// <summary>
        /// Writes to the input stream of the process.
        /// </summary>
        /// <param name="line">The line to written to the input stream.</param>
        void WriteStandardInput(string line);

        /// <summary>
        /// Read the output stream of the process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read ouput.</returns>
        Task<int> StandardOutputReadAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Read the error stream of the process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read error.</returns>
        Task<int> StandardErrorReadAsync(CancellationToken cancellationToken);
    }
}