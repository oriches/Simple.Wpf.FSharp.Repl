using System.IO;
using System.Reactive.Concurrency;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;

namespace Simple.Wpf.FSharp.Repl.Core
{
    public sealed class ReplEngine : BaseReplEngine
    {
        public ReplEngine(string workingDirectory = null, IProcessService processService = null,
            IScheduler scheduler = null) : base(workingDirectory, processService, scheduler)
        {
        }

        protected override byte[] GetFSharpResource()
        {
            return Resources.FSharp;
        }

        protected override string GetExecutablePath()
        {
            return null;
            ExtractFSharpBinaries(out var binaryPath);

            var fullPath = Path.Combine(binaryPath, Executable);
            //return $"dotnet \"{fullPath}\"";

            var tmp = "dotnet \"C:\\Temp\\.net core 3.1.0\\fsharp\\fsi.exe\"";
        }
}
}