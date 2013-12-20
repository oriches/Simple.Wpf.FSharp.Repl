namespace Simple.Wpf.FSharp.Repl.Controllers
{
    using ViewModels;

    public interface IReplWindowController
    {
        ReplWindowViewModel ViewModel { get; }
    }
}