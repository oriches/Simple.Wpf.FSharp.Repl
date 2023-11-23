using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;
using Simple.Wpf.FSharp.Repl.Common.UI.Controllers;
using Simple.Wpf.FSharp.Repl.Common.UI.ViewModels;
using Simple.Wpf.FSharp.Repl.UI.ViewModels;

namespace Simple.Wpf.FSharp.Repl.UI.Controllers
{
    public sealed class ReplEngineController : BaseReplEngineController
    {
        public ReplEngineController(string startupScript, string workingDirectory, IProcessService processService,
            IScheduler dispatcherScheduler, IScheduler taskScheduler)
            : base(startupScript, CreateEngine(workingDirectory), processService, dispatcherScheduler, taskScheduler)
        {
        }

        public ReplEngineController(string startupScript, IReplEngine replEngine, IProcessService processService,
            IScheduler dispatcherScheduler, IScheduler taskScheduler)
            : base(startupScript, replEngine, processService, dispatcherScheduler, taskScheduler)
        {
        }

        public ReplEngineController(string startupScript, string workingDirectory)
            : base(startupScript, CreateEngine(workingDirectory), new ProcessService(),
                System.Reactive.Concurrency.DispatcherScheduler.Current,
                System.Reactive.Concurrency.TaskPoolScheduler.Default)
        {
        }

        private static IReplEngine CreateEngine(string workingDirectory)
        {
            var replEngine = new Core.ReplEngine(workingDirectory);
            return replEngine;
        }

        protected override IReplEngineViewModel CreateViewModel(IReplEngine replEngine)
        {
            var errorStream = replEngine.Error
                .Select(x => new ReplLineViewModel(x, true))
                .ObserveOn(DispatcherScheduler);

            var outputStream = replEngine.Output
                .Select(x => new ReplLineViewModel(x))
                .ObserveOn(DispatcherScheduler);

            var stateStream = replEngine.State
                .ObserveOn(DispatcherScheduler);

            var viewModel = new ReplEngineViewModel(stateStream, outputStream, errorStream,
                replEngine.WorkingDirectory, ProcessService);

            return viewModel;
        }
    }
}