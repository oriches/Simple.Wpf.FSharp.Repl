using System;
using System.Reactive.Linq;
using System.Windows.Input;
using Simple.Wpf.FSharp.Repl.Common.Core;
using Simple.Wpf.FSharp.Repl.Common.Services;
using Simple.Wpf.FSharp.Repl.Common.UI.ViewModels;
using Simple.Wpf.FSharp.Repl.UI.Commands;

namespace Simple.Wpf.FSharp.Repl.UI.ViewModels
{
    /// <summary>
    ///     ViewModel for the REPL engine.
    /// </summary>
    public sealed class ReplEngineViewModel : BaseReplEngineViewModel
    {
        /// <summary>
        ///     Creates an instance of the REPL engine ViewModel.
        /// </summary>
        /// <param name="replState">Reactive extensions stream of the REPL engine state.</param>
        /// <param name="replOutput">Reactive extensions stream of the REPL engine output.</param>
        /// <param name="replError">Reactive extensions stream of the REPL engine errors.</param>
        /// <param name="workingDirectory">Reactive extensions stream of the REPL engine working directory.</param>
        /// <param name="processService">Handles starting windows processes.</param>
        public ReplEngineViewModel(IObservable<State> replState,
            IObservable<ReplLineViewModel> replOutput,
            IObservable<ReplLineViewModel> replError,
            string workingDirectory,
            IProcessService processService) : base(replState, replOutput, replError, workingDirectory, processService)
        {
            ClearCommand = new ReplRelayCommand(Clear, CanClear);
            ResetCommand = new ReplRelayCommand(ResetImpl, CanReset);
            ExecuteCommand = new ReplRelayCommand<string>(ExecuteImpl, CanExecute);
            OpenWorkingFolderCommand = new ReplRelayCommand(OpenWorkingFolder);

            Disposable.Add(replOutput.Where(x => x.Value != Prompt)
                .Subscribe(x => CommandManager.InvalidateRequerySuggested()));

            Disposable.Add(replError.Where(x => x.Value != Prompt)
                .Subscribe(x => CommandManager.InvalidateRequerySuggested()));
        }
    }
}