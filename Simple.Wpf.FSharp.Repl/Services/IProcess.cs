namespace Simple.Wpf.FSharp.Repl.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProcess : IDisposable
    {
        void Start();

        void WaitForExit();
        void WriteStandardInput(string line);
        Task<int> StandardOutputReadAsync(CancellationToken cancellationToken);
        Task<int> StandardErrorReadAsync(CancellationToken cancellationToken);
    }
}