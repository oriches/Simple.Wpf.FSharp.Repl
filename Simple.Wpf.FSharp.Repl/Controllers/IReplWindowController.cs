namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using ViewModels;

    /// <summary>
    /// Controller for the REPL engine UI, exposes the ViewModel.
    /// </summary>
    public interface IReplWindowController
    {
        /// <summary>
        /// The ViewModel for the REPL engine.
        /// </summary>
        IReplWindowViewModel ViewModel { get; }
    }
}