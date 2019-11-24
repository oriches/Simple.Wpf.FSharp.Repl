using System.Threading;
using System.Threading.Tasks;
using Simple.Wpf.FSharp.Repl.UI.Extensions;

namespace Simple.Wpf.FSharp.Repl.Services
{
    public sealed class Process : IProcess
    {
        private readonly System.Diagnostics.Process _process;

        public Process(System.Diagnostics.Process process)
        {
            _process = process;
        }

        public void Dispose()
        {
            _process.Dispose();
        }

        public void Start()
        {
            _process.Start();
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        public void WriteStandardInput(string line)
        {
            _process.StandardInput.WriteLine(line);
        }

        public Task<int> StandardOutputReadAsync(CancellationToken cancellationToken)
        {
            return _process.StandardOutput.ReadAsync(cancellationToken);
        }

        public Task<int> StandardErrorReadAsync(CancellationToken cancellationToken)
        {
            return _process.StandardError.ReadAsync(cancellationToken);
        }
    }
}