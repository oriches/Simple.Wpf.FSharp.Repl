using System.IO;
using System.Reactive.Concurrency;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;
using Simple.Wpf.FSharp.Repl.Properties;

namespace Simple.Wpf.FSharp.Repl.Core
{
    public sealed class ReplEngine : BaseReplEngine
    {
        private readonly bool _anyCpu;

        public ReplEngine(string workingDirectory = null, IProcessService processService = null,
            IScheduler scheduler = null, bool anyCpu = true) : base(workingDirectory, processService, scheduler)
        {
            _anyCpu = anyCpu;
        }

        protected override byte[] GetFSharpResource()
        {
            return Resources.FSharp;
        }

        protected override string GetExecutablePath()
        {
            ExtractFSharpBinaries(out var binaryPath);

            var execute = _anyCpu ? ExecutableAnyCpu : Executable;
            return Path.Combine(binaryPath, execute);
        }
    }
}