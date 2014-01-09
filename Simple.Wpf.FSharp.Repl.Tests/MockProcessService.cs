namespace Simple.Wpf.FSharp.Repl.Tests
{
    using Services;

    public class MockProcessService : IProcessService
    {
        public string StartWindowsExplorerDirectory { get; private set; }
        public int StartWindowsExplorerCalled { get; private set; }
        public int StartReplExecutableCalled { get; private set; }

        public IProcess StartWindowsExplorer(string directory)
        {
            StartWindowsExplorerDirectory = directory;
            StartWindowsExplorerCalled++;

            return new MockProcess();
        }

        public IProcess StartReplExecutable(string workingDirectory, string executableDirectory)
        {
            StartReplExecutableCalled++;

            return new MockProcess();
        }
    }
}