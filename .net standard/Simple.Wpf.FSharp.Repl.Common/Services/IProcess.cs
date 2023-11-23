using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Wpf.FSharp.Repl.Common.Services
{
    /// <summary>
    ///     Wraps the .Net System.Diagnostics.Process class.
    /// </summary>
    public interface IProcess : IDisposable
    {
        /// <summary>
        ///     Start the .Net process.
        /// </summary>
        void Start();

        /// <summary>
        ///     Waits for the .Net process to exit.
        /// </summary>
        void WaitForExit();

        /// <summary>
        ///     Writes to the input stream of the .Net process.
        /// </summary>
        /// <param name="line">The line to written to the input stream.</param>
        void WriteStandardInput(string line);

        /// <summary>
        ///     Reads the output stream of the .Net process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read ouput.</returns>
        Task<int> StandardOutputReadAsync(CancellationToken cancellationToken);

        /// <summary>
        ///     Reads the error stream of the .Net process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read error.</returns>
        Task<int> StandardErrorReadAsync(CancellationToken cancellationToken);
    }
}