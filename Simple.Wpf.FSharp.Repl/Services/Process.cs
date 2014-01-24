namespace Simple.Wpf.FSharp.Repl.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using UI.Extensions;

    /// <summary>
    /// Wraps the .Net System.Diagnostics.Process class.
    /// </summary>
    public sealed class Process : IProcess
    {
        private readonly System.Diagnostics.Process _process;

        /// <summary>
        /// Creates a wrapper around an instance of the .Net System.Diagnostics.Process class.
        /// </summary>
        /// <param name="process">The System.Diagnostics.Process instance.</param>
        public Process(System.Diagnostics.Process process)
        {
            _process = process;
        }

        /// <summary>
        /// Disposes the System.Diagnostics.Process instance.
        /// </summary>
        public void Dispose()
        {
            _process.Dispose();
        }

        /// <summary>
        /// Start the .Net process.
        /// </summary>
        public void Start()
        {
            _process.Start();
        }

        /// <summary>
        /// Waits for the .Net process to exit.
        /// </summary>
        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        /// <summary>
        /// Writes to the input stream of the .Net process.
        /// </summary>
        /// <param name="line">The line to written to the input stream.</param>
        public void WriteStandardInput(string line)
        {
            _process.StandardInput.WriteLine(line);
        }

        /// <summary>
        /// Reads the output stream of the .Net process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read output.</returns>
        public Task<int> StandardOutputReadAsync(CancellationToken cancellationToken)
        {
            return _process.StandardOutput.ReadAsync(cancellationToken);
        }

        /// <summary>
        /// Reads the error stream of the .Net process asynchronuously.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns the asynchronously read error.</returns>
        public Task<int> StandardErrorReadAsync(CancellationToken cancellationToken)
        {
            return _process.StandardError.ReadAsync(cancellationToken);
        }
    }
}