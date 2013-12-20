namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using ViewModels;

    public interface IReplWindowController
    {
        IReplWindowViewModel ViewModel { get; }
    }
}