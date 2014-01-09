namespace Simple.Wpf.FSharp.Repl.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Services;

    public class MockProcess : IProcess
    {
        public int DisposeCalled { get; private set; }
        public int StartCalled { get; private set; }
        public int WaitForExitCalled { get; private set; }
        public int WriteStandardInputCalled { get; private set; }
        public int StandardOutputReadAsyncCalled { get; private set; }
        public int StandardErrorReadAsyncCalled { get; private set; }

        public void Dispose()
        {
            DisposeCalled++;
        }

        public void Start()
        {
            StartCalled++;
        }

        public void WaitForExit()
        {
            WaitForExitCalled++;
        }

        public void WriteStandardInput(string line)
        {
            WriteStandardInputCalled++;
        }

        public Task<int> StandardOutputReadAsync(CancellationToken cancellationToken)
        {
            StandardOutputReadAsyncCalled++;

            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(StandardErrorReadAsyncCalled);

            return tcs.Task;

        }

        public Task<int> StandardErrorReadAsync(CancellationToken cancellationToken)
        {
            StandardErrorReadAsyncCalled++;

            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(StandardErrorReadAsyncCalled);
            
            return tcs.Task;
        }
    }
}